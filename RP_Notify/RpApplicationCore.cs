using Microsoft.Win32;
using RP_Notify.API;
using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Foobar2000;
using RP_Notify.SongInfoUpdater;
using RP_Notify.StartMenuShortcut;
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
        private readonly IConfig _config;
        private readonly IToastHandler _toastHandler;
        private readonly ShortcutHelper _shortcutHelper;
        private readonly Foobar2000Watcher _foobar2000Watcher;
        private readonly ISongInfoListener _songInfoListener;
        private readonly ILog _log;
        private readonly RpTrayIcon _rpTrayIcon;

        private Task OnChangeTask { get; set; }
        private int EventCounter { get; set; }

        public RpApplicationCore(IConfig config, IRpApiHandler apiHandler, IToastHandler toastHandler, Foobar2000Watcher foobar2000Watcher, ISongInfoListener songInfoListener, ILog log, RpTrayIcon rpTrayIcon, ShortcutHelper shortcutHelper)
        {
            _log = log;
            _apihandler = apiHandler;
            _config = config;
            _toastHandler = toastHandler;
            _songInfoListener = songInfoListener;
            _foobar2000Watcher = foobar2000Watcher;
            _rpTrayIcon = rpTrayIcon;
            _shortcutHelper = shortcutHelper;

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

            _songInfoListener.CheckTrackedRpPlayerStatus();
            if (_config.State.RpTrackingConfig.Players.Any())
            {
                _config.State.RpTrackingConfig.ActivePlayerId = _config.State.RpTrackingConfig.Players.FirstOrDefault().PlayerId;
            }

            if (_config.ExternalConfig.EnableFoobar2000Watcher)
            {
                _foobar2000Watcher.CheckFoobar2000Status(out bool notUsedHere);
                _foobar2000Watcher.Start();
            }

            // Init fields
            _shortcutHelper.TryCreateShortcut();    // Add shortcut to Start menu (required for Toast Notifications)

            // Set up event handlers
            _log.Information(LogHelper.GetMethodName(this), "Create event listeners");
            _config.ExternalConfig.ExternalConfigChangeHandler += OnChange;
            _config.State.StateChangeHandler += OnChange;
            _config.State.RpTrackingConfig.RpTrackingConfigChangeHandler += OnChange;
            Application.ApplicationExit += this.ApplicationExitHandler;
            SystemEvents.PowerModeChanged += WakeUpHandler;

            // Check queued application data delet request
            CheckQueuedDataDeleteRequest();

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
                            OnEnablePlayerWatcherChange();
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

        private void OnEnablePlayerWatcherChange()
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
                && _foobar2000Watcher.CheckFoobar2000Status(out bool channelChange)
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

            if (!_config.State.Playback.SameSongOnlyInternalUpdate)
            {
                Retry.Do(() => { File.Delete(_config.StaticConfig.AlbumArtImagePath); });
            }

            _toastHandler.ShowSongStartToast();
        }

        private void OnActivePlayerIdChange()
        {
            // USER and APP - when RP tracking is started at Startup or user changes/enables/disables RP player

            // if player became un-tracked
            if (!_config.IsRpPlayerTrackingChannel())
            {
                // if channel was favourites and Foobar2000 doesn't change it, then reset to the main stream
                if (_config.ExternalConfig.Channel == 99
                    && !(_foobar2000Watcher.CheckFoobar2000Status(out bool channelChanged) && channelChanged))
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
                    && !(_foobar2000Watcher.CheckFoobar2000Status(out bool channelChanged)
                    && channelChanged))
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

            // if Foobar2000 is not tracking an RP channel then start trackin without a second click
            if (_config.State.RpTrackingConfig.Players.Any()
                && !_config.IsRpPlayerTrackingChannel()
                && !_config.State.Foobar2000IsPlayingRP)
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

            if (File.Exists(_config.StaticConfig.AlbumArtImagePath))
            {
                _log.Information(LogHelper.GetMethodName(this), $"Delete album art");
                File.Delete(_config.StaticConfig.AlbumArtImagePath);
            }

            if (!_config.ExternalConfig.LeaveShorcutInStartMenu)
            {
                _shortcutHelper.DeleteShortcut();
                _log.Information(LogHelper.GetMethodName(this), $"Shortcut removed from start menu");
            }

            _rpTrayIcon.Dispose();

            if (_config.ExternalConfig.DeleteAllData)
            {
                _shortcutHelper.DeleteShortcut();
                _config.ExternalConfig.DeleteConfigRootFolder();
            }
            else
            {
                _log.Information(LogHelper.GetMethodName(this), $@"Finished
********************************************************************
********************************************************************
********************************************************************
********************************************************************
********************************************************************");
            }
        }

        private void CheckQueuedDataDeleteRequest()
        {
            if (_config.ExternalConfig.DeleteAllData)
            {
                _log.Information(LogHelper.GetMethodName(this), "App data delete requested");
                _log.Dispose();
                _toastHandler.DataEraseToast();
                Task.Delay(1000).Wait();
                Application.Exit();
            }
        }
    }
}
