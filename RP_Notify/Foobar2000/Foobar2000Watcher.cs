using Foobar2000.RESTClient.Api;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP_Notify.Foobar2000
{
    class Foobar2000Watcher
    {
        private readonly IConfig _config;
        private readonly ILogger _log;
        private readonly PlayerApi _playerApi;

        private int CheckDelayMillisecs { get; set; }
        private Task Foobar2000WatcherTask { get; set; }
        private CancellationTokenSource Foobar2000WatcherTaskCancellationTokenSource { get; set; }

        public Foobar2000Watcher(IConfig config, ILog log, PlayerApi playerApi)
        {
            _config = config;
            _log = log.Logger;
            _playerApi = playerApi;

            Init();
        }

        private void Init()
        {
            CheckDelayMillisecs = 5000;
            Foobar2000WatcherTaskCancellationTokenSource = new CancellationTokenSource();
            Application.ApplicationExit += (sender, e) => Foobar2000WatcherTaskCancellationTokenSource.Cancel();

            if (_config.ExternalConfig.EnablePlayerWatcher)
            {
                Start();
            }
        }

        public void Stop()
        {
            if (IsFoobar2000WatcherTaskRunning())
            {
                _log.Information("Foobar2000Watcher - Shutdown initiated");
                Foobar2000WatcherTaskCancellationTokenSource.Cancel();
            }
        }

        public void Start()
        {
            if (IsFoobar2000WatcherTaskRunning())
            {
                return;
            }

            _log.Information("Foobar2000Watcher - Started");

            Foobar2000WatcherTaskCancellationTokenSource = new CancellationTokenSource();

            Foobar2000WatcherTask = Task.Run(async () =>
            {
                int matchingChannel = -1;

                while (!Foobar2000WatcherTaskCancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        if (RpChannelIsPlaying(out matchingChannel)
                            && !_config.State.RpTrackingConfig.ValidateActivePlayerId())
                        {
                            CheckDelayMillisecs = 1000;
                            _config.ExternalConfig.Channel = matchingChannel;
                            _config.State.Foobar2000IsPlayingRP = true;
                        }
                        else
                        {
                            CheckDelayMillisecs = 5000;
                            _config.State.Foobar2000IsPlayingRP = false;
                        }

                        await Task.Delay(CheckDelayMillisecs);
                    }
                    catch (TaskCanceledException)
                    {
                        continue;       // Don't log error for stopping
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Foobar2000Watcher - ERROR - {ex.Message}");
                    }
                }

                _config.State.Foobar2000IsPlayingRP = false;

                _log.Information("Foobar2000Watcher - Stopped");

            }, Foobar2000WatcherTaskCancellationTokenSource.Token);
            _log.Information("Foobar2000Watcher - Running in background");
        }

        private bool IsFoobar2000WatcherTaskRunning()
        {
            return Foobar2000WatcherTask != null
                && (
                    Foobar2000WatcherTask.Status == TaskStatus.Running
                    || Foobar2000WatcherTask.Status == TaskStatus.WaitingToRun
                    || Foobar2000WatcherTask.Status == TaskStatus.WaitingForActivation
                    );
        }
        public bool RpChannelIsPlaying(out int matchingChannel)
        {
            if (TryGetPlayedFilePath(out string playedFilePath)
                && playedFilePath.Contains("radioparadise"))
            {
                matchingChannel = Int32.Parse(
                    _config.State.ChannelList
                        .Where(channel => playedFilePath.Contains(channel.StreamName))
                        .DefaultIfEmpty(_config.State.ChannelList.First())
                        .FirstOrDefault()
                        .Chan
                );

                return true;
            }
            else
            {
                matchingChannel = -1;
                return false;
            }
        }

        private bool TryGetPlayedFilePath(out string playedFilePath)
        {
            var columns = new List<string>();
            columns.Add("%path%");

            try
            {
                var foobarApiResp = _playerApi.GetPlayerStateAsync(columns).Result;
                playedFilePath = foobarApiResp.Player.ActiveItem.Columns.First();
                return true;
            }
            catch
            {
                playedFilePath = null;
                return false;
            }

        }

    }
}
