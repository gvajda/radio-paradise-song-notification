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
                CheckFoobar2000Status(out bool notUsedHere);
                Start();
            }
        }

        public void Stop()
        {
            if (IsFoobar2000WatcherTaskRunning())
            {
                _log.Information($"{LogHelper.GetMethodName(this)} - Shutdown initiated");
                Foobar2000WatcherTaskCancellationTokenSource.Cancel();
            }
        }

        public bool CheckFoobar2000Status(out bool channelChange)
        {
            channelChange = false;

            if (RpChannelIsPlaying(out int matchingChannel))
            {
                _config.State.Foobar2000IsPlayingRP = true;

                CheckDelayMillisecs = 1000;

                if (matchingChannel != _config.ExternalConfig.Channel
                    && !_config.State.RpTrackingConfig.ValidateActivePlayerId())
                {
                    channelChange = true;
                    _config.ExternalConfig.Channel = matchingChannel;
                }
                return true;
            }
            else
            {
                CheckDelayMillisecs = 5000;
                _config.State.Foobar2000IsPlayingRP = false;
                return false;
            }
        }

        public void Start()
        {
            if (IsFoobar2000WatcherTaskRunning())
            {
                return;
            }

            _log.Information($"{LogHelper.GetMethodName(this)} - Started");

            Foobar2000WatcherTaskCancellationTokenSource = new CancellationTokenSource();

            Foobar2000WatcherTask = Task.Run(async () =>
            {
                while (!Foobar2000WatcherTaskCancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        CheckFoobar2000Status(out bool notUsedHere);

                        await Task.Delay(CheckDelayMillisecs);
                    }
                    catch (TaskCanceledException)
                    {
                        // Don't log error for stopping
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"{LogHelper.GetMethodName(this)} - ERROR - {ex.Message}\n{ex.StackTrace}");
                    }
                }

                _config.State.Foobar2000IsPlayingRP = false;

                _log.Information($"{LogHelper.GetMethodName(this)} - Stopped");

            }, Foobar2000WatcherTaskCancellationTokenSource.Token);
            _log.Information($"{LogHelper.GetMethodName(this)} - Running in background");
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
