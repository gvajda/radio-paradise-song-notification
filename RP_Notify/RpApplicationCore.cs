using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using RP_Notify.RpApi;
using RP_Notify.RpApi.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Helpers;
using RP_Notify.PlayerWatcher;
using RP_Notify.PlayerWatcher.Foobar2000;
using RP_Notify.PlayerWatcher.MusicBee;
using RP_Notify.SongInfoListener;
using RP_Notify.Toast;
using RP_Notify.TrayIcon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP_Notify
{
    class RpApplicationCore : ApplicationContext
    {
        private readonly IRpApiHandler _apihandler;
        private readonly IConfigRoot _config;
        private readonly IToastHandler _toastHandler;
        private readonly IPlayerWatcher _foobar2000Watcher;
        private readonly IPlayerWatcher _musicBeeWatcher;
        private readonly ISongInfoListener _songInfoListener;
        private readonly ILog _log;
        private readonly RpTrayIcon _rpTrayIcon;

        private int EventCounter { get; set; }

        public RpApplicationCore(IConfigRoot config, IRpApiHandler apiHandler, IToastHandler toastHandler, Foobar2000Watcher foobar2000Watcher, MusicBeeWatcher musicBeeWatcher, ISongInfoListener songInfoListener, ILog log, RpTrayIcon rpTrayIcon)
        {
            _log = log;
            _apihandler = apiHandler;
            _config = config;
            _toastHandler = toastHandler;
            _songInfoListener = songInfoListener;
            _foobar2000Watcher = foobar2000Watcher;
            _musicBeeWatcher = musicBeeWatcher;
            _rpTrayIcon = rpTrayIcon;

            EventCounter = 0;

            Init();
        }

        private void Init()
        {
            _log.Information(LogHelper.GetMethodName(this), "Started ********************************************************************");


            if (_config.ExternalConfig.Channel == 99)      // Reset channel if Favourites were tracked at exit
            {
                _config.ExternalConfig.Channel = 0;
            }

            _songInfoListener.CheckTrackedRpPlayerStatus();     // Check if RP player is still tracked (updates State)

            if (_config.State.RpTrackingConfig.Players.Any())
            {
                // if multiple officcial RP player is active then start tracking the first available
                _config.State.RpTrackingConfig.ActivePlayerId = _config.State.RpTrackingConfig.Players.FirstOrDefault().PlayerId;
            }

            if (_config.ExternalConfig.EnableFoobar2000Watcher)
            {
                _foobar2000Watcher.CheckPlayerState(out bool notUsedHere);
                _foobar2000Watcher.Start();
            }
            else if (_config.ExternalConfig.EnableMusicBeeWatcher)
            {
                _musicBeeWatcher.CheckPlayerState(out bool notUsedHere);
                _musicBeeWatcher.Start();
            }

            // Set up event handlers
            _log.Information(LogHelper.GetMethodName(this), "Create event listeners");
            _config.ExternalConfig.ExternalConfigChangeHandler += OnChange;
            _config.State.StateChangeHandler += OnChange;
            _config.State.RpTrackingConfig.RpTrackingConfigChangeHandler += OnChange;
            Application.ApplicationExit += this.ApplicationExitHandler;
            SystemEvents.PowerModeChanged += WakeUpHandler;

            // Check queued application data delet request
            CheckQueuedDataDeleteRequest();

            // At the very first run, ask for config folder location
            if (!_config.StaticConfig.ConfigBaseFolderExisted)
            {
                _toastHandler.ConfigFolderToast();
            }

            // Start listen for song changes
            _songInfoListener.Start();

            // Add context menu
            _log.Information(LogHelper.GetMethodName(this), "Create tray icon");
            _rpTrayIcon.Init();
            _rpTrayIcon.NotifyIcon.MouseDoubleClick += TrayIconDoubleClickHandler;
        }


        private void TrayIconDoubleClickHandler(object sender, MouseEventArgs e)
        {
            _log.Information(LogHelper.GetMethodName(this), "Invoked - Sender: {Sender}", sender.GetType());
            _toastHandler.ShowSongDetailToast();

        }

        private void OnChange(object sender, RpEvent e)
        {
            if (e.ChangedFieldName.Equals(nameof(_config.State.TooltipText)))
            {
                // Tooltip text update triggered every second - not intrusive, no need to spam the event queue
                OnTooltipTextRPChange();
                return;
            }

            EventCounter++;

            Task.Run(() =>
            {
                string valueMessageComponent = e.Content.ToString() == e.Content.GetType().FullName
                    ? $"Object[{e.Content.ToString()}]"
                    : e.Content.ToString();

                _log.Information(LogHelper.GetMethodName(this), $"Event [{EventCounter}] - Received - Type: {{ChangedType}} - Changed field: {{ChannelChanged}} - Value: {{ValueMessageComponent}}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);

                try
                {
                    _rpTrayIcon.BuildContextMenu();

                    switch (e.ChangedFieldName)
                    {
                        case nameof(_config.ExternalConfig.ShowOnNewSong):
                            OnShowOnNewSongChange();
                            break;
                        case nameof(_config.ExternalConfig.EnableFoobar2000Watcher):
                            OnEnableFooBar2000WatcherChange();
                            break;
                        case nameof(_config.ExternalConfig.EnableMusicBeeWatcher):
                            OnEnableMusicBeeWatcherChange();
                            break;
                        case nameof(_config.ExternalConfig.LargeAlbumArt):
                            OnLargeAlbumArtChange();
                            break;
                        case nameof(_config.ExternalConfig.ShowSongRating):
                            OnShowSongRatingChange();
                            break;
                        case nameof(_config.ExternalConfig.PromptForRating):
                            OnPromptForRatingChange();
                            break;
                        case nameof(_config.ExternalConfig.Channel):
                            OnChannelChange();
                            break;
                        case nameof(_config.ExternalConfig.DeleteAllData):
                            OnDeleteAllDataChange();
                            break;
                        case nameof(_config.State.Foobar2000IsPlayingRP):
                            OnFoobar2000IsPlayingRPChange();
                            break;
                        case nameof(_config.State.MusicBeeIsPlayingRP):
                            OnMusicBeeIsPlayingRPChange();
                            break;
                        case nameof(_config.State.IsUserAuthenticated):
                            OnIsUserAuthenticatedChange();
                            break;
                        case nameof(_config.State.Playback):
                            OnPlaybackChange();
                            break;
                        case nameof(_config.State.RpTrackingConfig.ActivePlayerId):
                            OnActivePlayerIdChange();
                            break;
                        case nameof(_config.ExternalConfig.EnableRpOfficialTracking):
                            OnEnableRpOfficialTrackingChange();
                            break;
                        case nameof(_config.State.RpTrackingConfig.Players):
                            OnPlayersChange();
                            break;
                        default:
                            _log.Information(LogHelper.GetMethodName(this), $"Event [{EventCounter}] - Exit without action - Type: {{ChangedType}} - Changed field: {{ChannelChanged}} - Value: {{ValueMessageComponent}}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                    _toastHandler.ErrorToast(ex);
                    Task.Delay(10000).Wait();
                    Application.Exit();
                }

                _log.Information(LogHelper.GetMethodName(this), $"Event [{EventCounter}] - Finished - Type: {{ChangedType}} - Changed field: {{ChannelChanged}} - Value: {{ValueMessageComponent}}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);
            }).Wait();
        }

        private void OnShowOnNewSongChange()
        {
            // USER - menu button demonstration

            if (_config.ExternalConfig.ShowOnNewSong)
            {
                _toastHandler.ShowSongStartToast(true);
            }
        }

        private void OnEnableFooBar2000WatcherChange()
        {
            // USER - turn on and off the Foobar2000 watching infinite loop

            if (_config.ExternalConfig.EnableFoobar2000Watcher)
            {
                _foobar2000Watcher.Start();
            }
            else
            {
                _foobar2000Watcher.Stop();
            }
        }

        private void OnEnableMusicBeeWatcherChange()
        {
            // USER - turn on and off the Foobar2000 watching infinite loop

            if (_config.ExternalConfig.EnableMusicBeeWatcher)
            {
                _musicBeeWatcher.Start();
            }
            else
            {
                _musicBeeWatcher.Stop();
            }
        }

        private void OnLargeAlbumArtChange()
        {
            // USER - menu button demonstration

            _toastHandler.ShowSongDetailToast();
        }

        private void OnShowSongRatingChange()
        {
            // USER - menu button demonstration

            _toastHandler.ShowSongStartToast(true);
        }

        private void OnPromptForRatingChange()
        {
            // USER - menu button demonstration

            if (_config.ExternalConfig.PromptForRating)
            {
                _toastHandler.ShowSongRatingToast();
            }
        }

        private void OnChannelChange()
        {
            // USER and APP - change played channel

            if (!_config.IsRpPlayerTrackingChannel())
            {
                _songInfoListener.ResetListenerLoop();
            }
        }

        private void OnDeleteAllDataChange()
        {
            // USER - request to delete all config/log files in AppData

            CheckQueuedDataDeleteRequest();
        }

        private void OnFoobar2000IsPlayingRPChange()
        {
            // APP - flag if Foobar2000 is watched and actively playing an RP stream

            if (_config.State.Playback != null
                && _config.State.Foobar2000IsPlayingRP
                && !_config.IsRpPlayerTrackingChannel()
                && _foobar2000Watcher.CheckPlayerState(out bool channelChange)
                && !channelChange)
            {
                _toastHandler.ShowSongStartToast();
            }
        }

        private void OnMusicBeeIsPlayingRPChange()
        {
            // APP - flag if MusicBee is watched and actively playing an RP stream

            if (_config.State.Playback != null
                && _config.State.MusicBeeIsPlayingRP
                && !_config.IsRpPlayerTrackingChannel()
                && _musicBeeWatcher.CheckPlayerState(out bool channelChange)
                && !channelChange)
            {
                _toastHandler.ShowSongStartToast();
            }
        }

        private void OnTooltipTextRPChange()
        {
            // APP - update tray icon text (when mouse hovered over icon)

            _rpTrayIcon.NotifyIcon.Text = _config.State.TooltipText;
        }

        private void OnIsUserAuthenticatedChange()
        {
            // APP - when user logs in

            _songInfoListener.ResetListenerLoop();
        }

        private void OnPlaybackChange()
        {
            // APP - song is updated in State
            _toastHandler.ShowSongStartToast();
        }

        private void OnActivePlayerIdChange()
        {
            // USER and APP - when RP tracking is started at Startup or user changes/enables/disables RP player

            // if player became un-tracked
            if (!_config.IsRpPlayerTrackingChannel())
            {
                // if channel was favourites and Foobar2000 or MusicBee doesn't change it, then reset to the main stream
                if (_config.ExternalConfig.Channel == 99
                    && !(_foobar2000Watcher.CheckPlayerState(out bool channelChangedF) && channelChangedF)
                    && !(_musicBeeWatcher.CheckPlayerState(out bool channelChangedM) && channelChangedM))
                {
                    _config.ExternalConfig.Channel = 0;
                }
            }
            else
            {
                _songInfoListener.ResetListenerLoop();
            }
        }

        private void OnEnableRpOfficialTrackingChange()
        {
            // USER and APP - when RP tracking is started at Startup or user enables/disables RP tracking

            if (_config.ExternalConfig.EnableRpOfficialTracking)
            {
                _config.State.RpTrackingConfig.Players = _apihandler.GetSync_v2().Players;
            }
            else
            {
                if (_config.ExternalConfig.Channel == 99
                    && !(_foobar2000Watcher.CheckPlayerState(out bool channelChanged) && channelChanged)
                    && !(_musicBeeWatcher.CheckPlayerState(out bool channelChangedM) && channelChangedM))
                {
                    _config.ExternalConfig.Channel = 0;
                }

                _config.State.RpTrackingConfig.Players = _apihandler.GetSync_v2().Players;
                _config.State.RpTrackingConfig.ActivePlayerId = null;
                _config.State.RpTrackingConfig.Players = new List<Player>();
            }
        }

        private void OnPlayersChange()
        {
            // APP - availablable RP player list is refreshed

            // if Foobar2000 or MusicBee is not tracking an RP channel then start trackin without a second click
            if (_config.State.RpTrackingConfig.Players.Any()
                && !_config.IsRpPlayerTrackingChannel()
                && !_config.State.Foobar2000IsPlayingRP
                && !_config.State.MusicBeeIsPlayingRP)
            {
                _config.State.RpTrackingConfig.ActivePlayerId = _config.State.RpTrackingConfig.Players.FirstOrDefault().PlayerId;
            }
        }

        private void WakeUpHandler(object sender, PowerModeChangedEventArgs e)
        {
            // clean up potentially stuck threads / reinitiate app

            if (e.Mode == PowerModes.Resume)
            {
                _log.Information(LogHelper.GetMethodName(this), "PC woke up - RESTART");
                Application.Restart();
            }
        }

        private void ApplicationExitHandler(object sender, EventArgs e)
        {
            _log.Information(LogHelper.GetMethodName(this), "Exit process started");

            _rpTrayIcon.Dispose();

            CheckQueuedDataDeleteRequest();

            AlbumartFileHelper.DeleteOldAlbumartImageFiles(_config);
            _log.Information(LogHelper.GetMethodName(this), $@"Finished
********************************************************************
********************************************************************
********************************************************************
********************************************************************
********************************************************************");
        }

        private void CheckQueuedDataDeleteRequest()
        {
            if (_config.ExternalConfig.DeleteAllData || _config.StaticConfig.CleanUpOnExit)
            {
                _log.Information(LogHelper.GetMethodName(this), "App data delete requested");
                _log.Dispose();
                _toastHandler.DataEraseToast();
                Task.Delay(5000).Wait();
                ToastNotificationManagerCompat.History.Clear();
                ToastNotificationManagerCompat.Uninstall();
                ConfigDirectoryHelper.DeleteConfigDirectory(_config.StaticConfig.ConfigBaseFolderOption);
                Application.Exit();
            }
        }
    }
}
