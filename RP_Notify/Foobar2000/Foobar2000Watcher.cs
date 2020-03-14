using Foobar2000.RESTClient.Api;
using RP_Notify.API;
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
        private readonly IRpApiHandler _apiHandler;
        private readonly ILogger _log;
        private readonly PlayerApi _playerApi;

        private int CheckDelayMillisecs { get; set; }
        private Task Foobar2000WatcherTask { get; set; }

        public event EventHandler<ConfigChangeEventArgs> ConfigChangedEventHandler;
        public CancellationTokenSource PlayerWatcherCancellationTokenSource { get; set; }


        public Foobar2000Watcher(IConfig config, IRpApiHandler apiHandler, ILog log, PlayerApi playerApi)
        {
            _config = config;
            _apiHandler = apiHandler;
            _log = log.Logger;
            _playerApi = playerApi;

            CheckDelayMillisecs = 5000;
            PlayerWatcherCancellationTokenSource = new CancellationTokenSource();
            Application.ApplicationExit += (sender, e) => Stop();
        }

        public void Start()
        {
            if (Foobar2000WatcherTask == null
                || Foobar2000WatcherTask.Status == TaskStatus.Running
                || Foobar2000WatcherTask.Status == TaskStatus.WaitingToRun)
            {
                return;
            }

            _log.Information("Foobar2000Watcher - Started");

            PlayerWatcherCancellationTokenSource = new CancellationTokenSource();

            Foobar2000WatcherTask = Task.Run(async () =>
            {
                string matchingChannel = null;

                while (!PlayerWatcherCancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (RpChannelIsPlaying(out matchingChannel))
                        {
                            CheckDelayMillisecs = 1000;
                            HandleChannelMatch(matchingChannel);
                        }
                        else
                        {
                            CheckDelayMillisecs = 5000;
                            HandleInactivePlayer();
                        }

                        await Task.Delay(CheckDelayMillisecs, PlayerWatcherCancellationTokenSource.Token);
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

            }, PlayerWatcherCancellationTokenSource.Token);
            _log.Information("Foobar2000Watcher - Running in background");
        }

        public void Stop()
        {
            if (Foobar2000WatcherTask == null
                || Foobar2000WatcherTask.Status == TaskStatus.Running
                || Foobar2000WatcherTask.Status == TaskStatus.WaitingToRun)
            {
                _log.Information("Foobar2000Watcher - Initiate shutdown");
                PlayerWatcherCancellationTokenSource.Cancel();
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

        private bool RpChannelIsPlaying(out string matchingChannel)
        {
            if (TryGetPlayedFilePath(out string playedFilePath)
                && playedFilePath.Contains("radioparadise"))
            {
                matchingChannel = _config.State.ChannelList
                    .Where(channel => playedFilePath.Contains(channel.StreamName))
                    .DefaultIfEmpty(_config.State.ChannelList.First())
                    .FirstOrDefault()
                    .Chan;

                return true;
            }
            else
            {
                matchingChannel = "-1";
                return false;
            }
        }


        private void HandleChannelMatch(string matchingChannel)
        {
            bool sendChannelChangeEvent = false;
            bool playerStateChanged = false;

            if (_config.ExternalConfig.Channel != Int32.Parse(matchingChannel))
            {
                _config.ExternalConfig.Channel = Int32.Parse(matchingChannel);
                sendChannelChangeEvent = true;
            }

            if (!_config.State.Foobar2000IsPlayingRP)
            {
                _config.State.Foobar2000IsPlayingRP = true;
                playerStateChanged = true;

            }
            if (sendChannelChangeEvent || playerStateChanged)
            {
                ConfigChangedEventHandler?
                    .Invoke(this,
                        new ConfigChangeEventArgs()
                        {
                            ChannelChanged = sendChannelChangeEvent,
                            PlayerStateChanged = playerStateChanged
                        });
            }
        }

        private void HandleInactivePlayer()
        {

            _config.State.RpTrackingConfig.ActivePlayerId = null;
            bool playerStateChanged = false;

            if (_config.State.Foobar2000IsPlayingRP)
            {
                _config.State.Foobar2000IsPlayingRP = false;
                playerStateChanged = true;

            }

            if (playerStateChanged)
            {
                ConfigChangedEventHandler?
                    .Invoke(this,
                        new ConfigChangeEventArgs()
                        {
                            PlayerStateChanged = playerStateChanged
                        });
            }
        }
    }
}
