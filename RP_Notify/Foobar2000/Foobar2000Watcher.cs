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

namespace RP_Notify.Foobar2000Watcher
{
    class Foobar2000Watcher : IPlayerWatcher
    {
        private readonly IConfig _config;
        private readonly IRpApiHandler _apiHandler;
        private readonly ILogger _log;
        private readonly PlayerApi _playerApi;
        private int checkDelayMillisecs;
        public event EventHandler<ConfigChangeEventArgs> ConfigChangedEventHandler;
        public CancellationTokenSource PlayerWatcherCancellationTokenSource { get; }
        public bool PlayerIsActive { get; set; }


        public Foobar2000Watcher(IConfig config, IRpApiHandler apiHandler, ILog log, PlayerApi playerApi)
        {
            PlayerIsActive = false;
            _config = config;
            _apiHandler = apiHandler;
            _log = log.Logger;
            _playerApi = playerApi;
            checkDelayMillisecs = 5000;
            PlayerWatcherCancellationTokenSource = new CancellationTokenSource();
            Application.ApplicationExit += (sender, e) => PlayerWatcherCancellationTokenSource.Cancel();
        }

        public void StartChannelWatch()
        {
            _log.Information("StartChannelWatch - Invoked");
            Task.Run(async () =>
            {
                string matchingChannel = null;
                bool showOnNewSongUserSetting = _config.ShowOnNewSong;
                while (!PlayerWatcherCancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (!PlayerIsActive)
                    {
                        showOnNewSongUserSetting = _config.ShowOnNewSong;
                    }

                    if (_config.EnablePlayerWatcher)
                    {
                        if (TryGetPlayedFilePath(out string playedFilePath)
                            && RpChannelIsPlaying(playedFilePath, out matchingChannel))
                        {
                            checkDelayMillisecs = 1000;
                            HandleChannelMatch(matchingChannel);
                        }
                        else
                        {
                            checkDelayMillisecs = 5000;
                            HandleInactivePlayer(showOnNewSongUserSetting);
                        }
                    }
                    else
                    {
                        checkDelayMillisecs = 5000;
                        if (!_config.RpTrackingConfig.Enabled)
                        {
                            HandleInactivePlayer(showOnNewSongUserSetting);
                        }
                    }
                    await Task.Delay(checkDelayMillisecs, PlayerWatcherCancellationTokenSource.Token);
                }
            }, PlayerWatcherCancellationTokenSource.Token);
            _log.Information("StartChannelWatch - Running in background");
        }

        private bool TryGetPlayedFilePath(out string playedFilePath)
        {
            playedFilePath = null;
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
                return false;
            }

        }

        private bool RpChannelIsPlaying(string playedFilePath, out string matchingChannel)
        {
            matchingChannel = "0";
            if (!playedFilePath.Contains("radioparadise"))
            {
                return false;
            }

            foreach (var channel in _apiHandler.ChannelList.Where(channel => playedFilePath.Contains(channel.StreamName)))
            {
                matchingChannel = channel.Chan;
            }

            return true;
        }


        private void HandleChannelMatch(string matchingChannel)
        {
            bool sendShowOnNewSongChangeEvent = false;
            bool sendChannelChangeEvent = false;
            bool playerStateChanged = false;
            _config.RpTrackingConfig.ActivePlayerId = "Foobar2000";
            if (!_config.ShowOnNewSong)
            {
                _config.ShowOnNewSong = !_config.ShowOnNewSong;
                sendShowOnNewSongChangeEvent = true;
            }
            if (_config.Channel != Int32.Parse(matchingChannel))
            {
                _config.Channel = Int32.Parse(matchingChannel);
                sendChannelChangeEvent = true;
            }
            if (!PlayerIsActive)
            {
                PlayerIsActive = true;
                playerStateChanged = true;

            }
            if (sendShowOnNewSongChangeEvent || sendChannelChangeEvent || playerStateChanged)
            {
                ConfigChangedEventHandler?
                    .Invoke(this,
                        new ConfigChangeEventArgs()
                        {
                            ChannelChanged = sendChannelChangeEvent,
                            ShowOnNewSongChanged = sendShowOnNewSongChangeEvent,
                            PlayerStateChanged = playerStateChanged
                        });
            }
        }

        private void HandleInactivePlayer(bool showOnNewSong)
        {

            _config.RpTrackingConfig.ActivePlayerId = null;
            bool sendShowOnNewSongChangeEvent = false;
            bool playerStateChanged = false;
            if (_config.ShowOnNewSong != showOnNewSong)
            {
                _config.ShowOnNewSong = showOnNewSong;
                sendShowOnNewSongChangeEvent = true;
            }
            if (PlayerIsActive)
            {
                PlayerIsActive = false;
                playerStateChanged = true;

            }
            if (sendShowOnNewSongChangeEvent || playerStateChanged)
            {
                ConfigChangedEventHandler?
                    .Invoke(this,
                        new ConfigChangeEventArgs()
                        {
                            ShowOnNewSongChanged = sendShowOnNewSongChangeEvent,
                            PlayerStateChanged = playerStateChanged
                        });
            }
        }
    }
}
