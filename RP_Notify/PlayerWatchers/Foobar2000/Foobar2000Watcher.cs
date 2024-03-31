using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.PlayerWatchers.Foobar2000.BeefWebApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP_Notify.PlayerWatcher.Foobar2000
{
    class Foobar2000Watcher : IPlayerWatcher
    {
        private readonly IConfigRoot _config;
        private readonly ILog _log;
        private readonly IBeefWebApiClientFactory _beefWebApiClientFactory;

        private int CheckDelayMillisecs { get; set; }
        private Task Foobar2000WatcherTask { get; set; }
        private CancellationTokenSource Foobar2000WatcherTaskCancellationTokenSource { get; set; }

        public Foobar2000Watcher(IConfigRoot config, ILog log, IBeefWebApiClientFactory beefWebApiClientFactory)
        {
            _config = config;
            _log = log;
            _beefWebApiClientFactory = beefWebApiClientFactory;

            Init();
        }

        private void Init()
        {
            CheckDelayMillisecs = 5000;
            Foobar2000WatcherTaskCancellationTokenSource = new CancellationTokenSource();
            Application.ApplicationExit += (sender, e) => Foobar2000WatcherTaskCancellationTokenSource.Cancel();
        }

        public void Stop()
        {
            if (IsFoobar2000WatcherTaskRunning())
            {
                _log.Information(LogHelper.GetMethodName(this), $"Shutdown initiated");
                Foobar2000WatcherTaskCancellationTokenSource.Cancel();
            }
            else
            {
                _log.Information(LogHelper.GetMethodName(this), $"Not running");
            }
        }

        public void Start()
        {
            if (!IsFoobar2000WatcherTaskRunning())
            {
                _log.Information(LogHelper.GetMethodName(this), $"Invoked");
                Run();
            }
            else
            {
                _log.Information(LogHelper.GetMethodName(this), $"Alreay running");
            }
        }

        private void Run()
        {
            _log.Information(LogHelper.GetMethodName(this), $"Starting");

            Foobar2000WatcherTaskCancellationTokenSource = new CancellationTokenSource();

            Foobar2000WatcherTask = Task.Run(async () =>
            {
                while (!Foobar2000WatcherTaskCancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        CheckPlayerState(out bool _);

                        await Task.Delay(CheckDelayMillisecs);
                    }
                    catch (TaskCanceledException)
                    {
                        // Don't log error for stopping
                    }
                    catch (Exception ex)
                    {
                        _log.Error(LogHelper.GetMethodName(this), ex);
                        Task.Delay(10000).Wait();
                        Application.Exit();
                    }
                }

                _config.State.Foobar2000IsPlayingRP = false;

                _log.Information(LogHelper.GetMethodName(this), $"Stopped");

            }, Foobar2000WatcherTaskCancellationTokenSource.Token);

            _log.Information(LogHelper.GetMethodName(this), $"Running in background");
        }

        public bool CheckPlayerState(out bool channelChanged)
        {
            channelChanged = false;

            if (_config.ExternalConfig.EnableFoobar2000Watcher
                && RpChannelIsPlayingInFB2K(out int matchingChannel))
            {
                _config.State.Foobar2000IsPlayingRP = true;

                CheckDelayMillisecs = 1000;

                if (matchingChannel != _config.ExternalConfig.Channel
                    && !_config.IsRpPlayerTrackingChannel())
                {
                    channelChanged = true;
                    _log.Information(LogHelper.GetMethodName(this), $"Channel change detected");
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

        private bool IsFoobar2000WatcherTaskRunning()
        {
            return !(Foobar2000WatcherTask == null || Foobar2000WatcherTask.IsCompleted);
        }

        private bool RpChannelIsPlayingInFB2K(out int matchingChannel)
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
                var foobarApiResp = _beefWebApiClientFactory.Create().GetPlayerStateAsync(columns).Result;
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
