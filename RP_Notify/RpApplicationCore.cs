using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using RP_Notify.Config;
using RP_Notify.Helpers;
using RP_Notify.Logger;
using RP_Notify.LoginForm;
using RP_Notify.PlayerWatchers;
using RP_Notify.RpApi;
using RP_Notify.RpApi.ResponseModel;
using RP_Notify.SongInfoListener;
using RP_Notify.ToastHandler;
using RP_Notify.TrayIconMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Windows.Foundation.Collections;

namespace RP_Notify
{
    class RpApplicationCore : ApplicationContext
    {
        private readonly IRpApiClientFactory _rpApiClientFactory;
        private readonly IConfigRoot _config;
        private readonly IToastHandlerFactory _toastHandlerFactory;
        private readonly IPlayerWatcherProvider _playerWatcherProvider;
        private readonly ISongInfoListener _songInfoListener;
        private readonly ILoggerWrapper _log;
        private readonly RpTrayIconMenu _rpTrayIcon;
        private readonly LoginForm.LoginForm _loginForm;

        private int EventCounter { get; set; }

        public RpApplicationCore(IConfigRoot config, IRpApiClientFactory rpApiClientFactory, IToastHandlerFactory toastHandlerFactory, IPlayerWatcherProvider playerWatcherProvider, ISongInfoListener songInfoListener, ILoggerWrapper log, RpTrayIconMenu rpTrayIcon, LoginForm.LoginForm loginForm)
        {
            _log = log;
            _config = config;
            _rpApiClientFactory = rpApiClientFactory;
            _toastHandlerFactory = toastHandlerFactory;
            _songInfoListener = songInfoListener;
            _playerWatcherProvider = playerWatcherProvider;
            _rpTrayIcon = rpTrayIcon;
            _loginForm = loginForm;

            EventCounter = 0;

            Init();
        }

        private void Init()
        {
            // Check queued application data delete request
            CheckQueuedDataDeleteRequest();

            _log.Information(this.GetMethodName(), "Started ********************************************************************");

            // At the very first run, ask for config folder location
            if (!_config.StaticConfig.ConfigBaseFolderExisted)
            {
                _toastHandlerFactory.Create().ShowConfigFolderChoicePromptToast();
            }

            if (_config.PersistedConfig.Channel > 9)      // Reset channel if a special channel was tracked at exit
            {
                _config.PersistedConfig.Channel = 0;
            }

            // Refresh channel list
            _config.State.ChannelList = _rpApiClientFactory.Create().GetChannelList();

            // Refresh cookies
            if (_config.IsUserAuthenticated(out string _))
            {
                _rpApiClientFactory.Create().GetAuth();
            }

            // Set up event handlers
            _log.Information(this.GetMethodName(), "Create event listeners");
            _config.PersistedConfig.ExternalConfigChangeHandler += OnConfigChangeEvent;
            _config.State.StateChangeHandler += OnConfigChangeEvent;
            _config.State.RpTrackingConfig.RpTrackingConfigChangeHandler += OnConfigChangeEvent;
            Application.ApplicationExit += this.OnApplicationExit;
            SystemEvents.PowerModeChanged += OnComputerWakeUp;
            ToastNotificationManagerCompat.OnActivated += OnToastActivatedEvent;
            _loginForm.LoginInputEventHandler += OnLoginInputSubmitted;


            // Start watching for RP stream if enabled
            if (_config.PersistedConfig.EnableRpOfficialTracking)
            {
                _config.PersistedConfig.EnableFoobar2000Watcher = false;
                _config.PersistedConfig.EnableMusicBeeWatcher = false;
                _playerWatcherProvider.GetWatcher(RegisteredPlayer.RpOfficial).Start();
            }
            else if (_config.PersistedConfig.EnableFoobar2000Watcher)
            {
                _config.PersistedConfig.EnableRpOfficialTracking = false;
                _config.PersistedConfig.EnableMusicBeeWatcher = false;
                _playerWatcherProvider.GetWatcher(RegisteredPlayer.Foobar2000).Start();
            }
            else if (_config.PersistedConfig.EnableMusicBeeWatcher)
            {
                _config.PersistedConfig.EnableRpOfficialTracking = false;
                _config.PersistedConfig.EnableFoobar2000Watcher = false;
                _playerWatcherProvider.GetWatcher(RegisteredPlayer.MusicBee).Start();
            }


            // Start listen for song changes
            _songInfoListener.Start();

            // Add context menu
            _log.Information(this.GetMethodName(), "Create tray icon");
            _rpTrayIcon.Init();
            _rpTrayIcon.NotifyIcon.MouseDoubleClick += TrayIconDoubleClickHandler;
        }


        //
        //
        // Event handlers
        //
        //


        private void TrayIconDoubleClickHandler(object sender, MouseEventArgs e)
        {
            _log.Information(this.GetMethodName(), "Invoked - Sender: {Sender}", sender.GetType());
            _toastHandlerFactory.Create().ShowSongDetailToast();

        }

        /** The following methods are event handlers for the configuration change events.
         ** They are triggered by the event handlers in the Config class.
         ** The methods are responsible for updating the application state based on the configuration changes.
         ** The methods are also responsible for updating the UI based on the configuration changes.
         **/

        #region Config change event handlers

        private void OnConfigChangeEvent(object sender, RpConfigurationChangeEvent e)
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

                _log.Information(this.GetMethodName(), $"Event [{EventCounter}] - Received - Type: {{ChangedType}} - Changed field: {{ChannelChanged}} - Value: {{ValueMessageComponent}}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);

                try
                {
                    _rpTrayIcon.BuildContextMenu();

                    switch (e.ChangedFieldName)
                    {
                        case nameof(_config.PersistedConfig.ShowOnNewSong):
                            OnShowOnNewSongChange();
                            break;
                        case nameof(_config.PersistedConfig.EnableFoobar2000Watcher):
                            OnEnableFooBar2000WatcherChange();
                            break;
                        case nameof(_config.PersistedConfig.EnableMusicBeeWatcher):
                            OnEnableMusicBeeWatcherChange();
                            break;
                        case nameof(_config.PersistedConfig.EnableRpOfficialTracking):
                            OnEnableRpOfficialTrackingChange();
                            break;
                        case nameof(_config.PersistedConfig.LargeAlbumArt):
                            OnLargeAlbumArtChange();
                            break;
                        case nameof(_config.PersistedConfig.RpBannerOnDetail):
                            OnRpBannerOnDetailChange();
                            break;
                        case nameof(_config.PersistedConfig.ShowSongRating):
                            OnShowSongRatingChange();
                            break;
                        case nameof(_config.PersistedConfig.PromptForRating):
                            OnPromptForRatingChange();
                            break;
                        case nameof(_config.PersistedConfig.Channel):
                            OnChannelChange();
                            break;
                        case nameof(_config.PersistedConfig.DeleteAllData):
                            OnDeleteAllDataChange();
                            break;
                        case nameof(_config.State.Foobar2000IsPlayingRP):
                            OnFoobar2000IsPlayingRPChange();
                            break;
                        case nameof(_config.State.MusicBeeIsPlayingRP):
                            OnMusicBeeIsPlayingRPChange();
                            break;
                        case nameof(_config.State.RpCookieContainer):
                            OnIsCookieChange();
                            break;
                        case nameof(_config.State.Playback):
                            OnPlaybackChange();
                            break;
                        case nameof(_config.State.RpTrackingConfig.ActivePlayerId):
                            OnActivePlayerIdChange();
                            break;
                        case nameof(_config.State.RpTrackingConfig.Players):
                            OnPlayersChange();
                            break;
                        default:
                            _log.Information(this.GetMethodName(), $"Event [{EventCounter}] - Exit without action - Type: {{ChangedType}} - Changed field: {{ChannelChanged}} - Value: {{ValueMessageComponent}}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(this.GetMethodName(), ex);
                    _toastHandlerFactory.Create().ShowErrorToast(ex);
                    Task.Delay(10000).Wait();
                    Application.Exit();
                }

                _log.Information(this.GetMethodName(), $"Event [{EventCounter}] - Finished - Type: {{ChangedType}} - Changed field: {{ChannelChanged}} - Value: {{ValueMessageComponent}}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);
            }).Wait();
        }

        private void OnShowOnNewSongChange()
        {
            // USER - menu button demonstration

            if (_config.PersistedConfig.ShowOnNewSong)
            {
                _toastHandlerFactory.Create().ShowSongStartToast(true);
            }
        }

        private void OnEnableFooBar2000WatcherChange()
        {
            // USER - turn on and off the Foobar2000 watching infinite loop

            if (_config.PersistedConfig.EnableFoobar2000Watcher)
            {
                _config.PersistedConfig.EnableRpOfficialTracking = false;
                _config.PersistedConfig.EnableMusicBeeWatcher = false;
                _playerWatcherProvider.GetWatcher(RegisteredPlayer.Foobar2000).Start();
            }
            else
            {
                _playerWatcherProvider.GetWatcher(RegisteredPlayer.Foobar2000).Stop();
            }
        }

        private void OnEnableMusicBeeWatcherChange()
        {
            // USER - turn on and off the Foobar2000 watching infinite loop

            if (_config.PersistedConfig.EnableMusicBeeWatcher)
            {
                _config.PersistedConfig.EnableRpOfficialTracking = false;
                _config.PersistedConfig.EnableFoobar2000Watcher = false;
                _playerWatcherProvider.GetWatcher(RegisteredPlayer.MusicBee).Start();
            }
            else
            {
                _playerWatcherProvider.GetWatcher(RegisteredPlayer.MusicBee).Stop();
            }
        }

        private void OnEnableRpOfficialTrackingChange()
        {
            // USER and APP - when RP tracking is started at Startup or user enables/disables RP tracking

            if (_config.PersistedConfig.EnableRpOfficialTracking)
            {
                _config.PersistedConfig.EnableFoobar2000Watcher = false;
                _config.PersistedConfig.EnableMusicBeeWatcher = false;
                _playerWatcherProvider.GetWatcher(RegisteredPlayer.RpOfficial).Start();
            }
            else
            {
                _playerWatcherProvider.GetWatcher(RegisteredPlayer.RpOfficial).Stop();
                if (_config.PersistedConfig.Channel > 9)
                {
                    _config.PersistedConfig.Channel = 0;
                }

                _config.State.RpTrackingConfig.ActivePlayerId = null;
                _config.State.RpTrackingConfig.Players = new List<Player>();
            }
        }

        private void OnLargeAlbumArtChange()
        {
            // USER - menu button demonstration

            _toastHandlerFactory.Create().ShowSongDetailToast();
        }

        private void OnRpBannerOnDetailChange()
        {
            // USER - menu button demonstration

            _toastHandlerFactory.Create().ShowSongDetailToast();
        }

        private void OnShowSongRatingChange()
        {
            // USER - menu button demonstration

            _toastHandlerFactory.Create().ShowSongStartToast(true);
        }

        private void OnPromptForRatingChange()
        {
            // USER - menu button demonstration

            if (_config.PersistedConfig.PromptForRating)
            {
                _toastHandlerFactory.Create().ShowSongRatingToast();
            }
        }

        private void OnChannelChange()
        {
            // USER and APP - change played channel

            if (!_config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _))
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
                && !_config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _)
                && _playerWatcherProvider.GetWatcher(RegisteredPlayer.Foobar2000).CheckPlayerState(out bool channelChange)
                && !channelChange)
            {
                _toastHandlerFactory.Create().ShowSongStartToast();
            }
        }

        private void OnMusicBeeIsPlayingRPChange()
        {
            // APP - flag if MusicBee is watched and actively playing an RP stream

            if (_config.State.Playback != null
                && _config.State.MusicBeeIsPlayingRP
                && !_config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _)
                && _playerWatcherProvider.GetWatcher(RegisteredPlayer.MusicBee).CheckPlayerState(out bool channelChange)
                && !channelChange)
            {
                _toastHandlerFactory.Create().ShowSongStartToast();
            }
        }

        private void OnTooltipTextRPChange()
        {
            // APP - update tray icon text (when mouse hovered over icon)

            _rpTrayIcon.NotifyIcon.Text = _config.State.TooltipText;
        }

        private void OnIsCookieChange()
        {
            // APP - when user logs in

            _songInfoListener.ResetListenerLoop();
        }

        private void OnPlaybackChange()
        {
            // APP - song is updated in State
            _toastHandlerFactory.Create().ShowSongStartToast();
        }

        private void OnActivePlayerIdChange()
        {
            // USER and APP - when RP tracking is started at Startup or user changes/enables/disables RP player

            // if player became un-tracked
            if (!_config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _))
            {
                // if channel was favorites and Foobar2000 or MusicBee doesn't change it, then reset to the main stream
                if (_config.PersistedConfig.Channel == 99
                    && !(_playerWatcherProvider.GetWatcher(RegisteredPlayer.Foobar2000).CheckPlayerState(out bool channelChangedF) && channelChangedF)
                    && !(_playerWatcherProvider.GetWatcher(RegisteredPlayer.MusicBee).CheckPlayerState(out bool channelChangedM) && channelChangedM))
                {
                    _config.PersistedConfig.Channel = 0;
                }
            }
            else
            {
                _songInfoListener.ResetListenerLoop();
            }
        }

        private void OnPlayersChange()
        {
            // APP - available RP player list is refreshed

            // if Foobar2000 or MusicBee is not tracking an RP channel then start tracking without a second click
            if (_config.State.RpTrackingConfig.Players.Any()
                && !_config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _))
            {
                _config.State.RpTrackingConfig.ActivePlayerId = _config.State.RpTrackingConfig.Players.FirstOrDefault().PlayerId;
            }
        }

        #endregion

        /** The following methods are event handlers for the toast action events.
         ** They are triggered by the event handlers in the ToastHandler class.
         ** The methods are responsible for handling the user input from the toast notifications.
         **/

        #region Toas action event handlers

        private void OnToastActivatedEvent(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            // Obtain the arguments from the notification
            ToastArguments toastArguments = ToastArguments.Parse(toastArgs.Argument);

            // Obtain any user input (text boxes, menu selections) from the notification
            ValueSet userInput = toastArgs.UserInput;


            if (!Enum.TryParse<RpToastUserAction>(toastArguments[nameof(RpToastUserAction)], out RpToastUserAction rpToastUserAction))
            {
                return;     // Only process known actions
            }

            EventCounter++;

            Task.Run(() =>
            {
                var formattedArgumentsForLogging = string.Join("&", toastArguments.Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));

                _log.Information(this.GetMethodName(), $"Event [{EventCounter}] - Toast event - eventArguments: {formattedArgumentsForLogging}");

                try
                {
                    switch (rpToastUserAction)
                    {
                        case RpToastUserAction.ConfigFolderChosen:
                            OnConfigFolderChosenToastAction(userInput);
                            break;
                        case RpToastUserAction.FolderChoiceRefusedExitApp:
                            OnFolderChoiceRefusedExitAppAction();
                            break;
                        case RpToastUserAction.RatingToastRequested:
                            OnRatingToastRequestedAction(toastArguments);
                            break;
                        case RpToastUserAction.LoginRequested:
                            _loginForm.ShowDialog();
                            break;
                        case RpToastUserAction.RateSubmitted:
                            OnRateSubmittedToastAction(userInput, toastArguments);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(this.GetMethodName(), ex);
                    _toastHandlerFactory.Create().ShowErrorToast(ex);
                    Task.Delay(10000).Wait();
                    Application.Exit();
                }

                _log.Information(this.GetMethodName(), $"Event [{EventCounter}] - Toast event Finished");
            }).Wait();

        }

        private void OnConfigFolderChosenToastAction(ValueSet userInput)
        {
            if (!userInput.TryGetValue(nameof(ConfigFolderChoiceOption), out var rawFolder)) return;
            
            var folder = (string)rawFolder;

            ConfigLocationOptions startingLocation;
            ConfigLocationOptions targetLocation;
                
            if (folder == ConfigFolderChoiceOption.localCache.ToDescriptionString()
                && _config.StaticConfig.ConfigBaseFolderOption == ConfigLocationOptions.AppData)
            {
                startingLocation = ConfigLocationOptions.AppData;
                targetLocation = ConfigLocationOptions.ExeContainingDirectory;
                MoveConfigFolder(startingLocation, targetLocation);
            }
            else if (folder == ConfigFolderChoiceOption.appdata.ToDescriptionString()
                     && _config.StaticConfig.ConfigBaseFolderOption == ConfigLocationOptions.ExeContainingDirectory)
            {
                startingLocation = ConfigLocationOptions.ExeContainingDirectory;
                targetLocation = ConfigLocationOptions.AppData;
                MoveConfigFolder(startingLocation, targetLocation);
            }
            else if (folder == ConfigFolderChoiceOption.cleanonexit.ToDescriptionString())
            {
                _config.StaticConfig.CleanUpOnExit = true;
            }
        }

        private void OnFolderChoiceRefusedExitAppAction()
        {
            _config.PersistedConfig.DeleteAllData = true;
        }

        private void MoveConfigFolder(ConfigLocationOptions startingLocation, ConfigLocationOptions targetLocation)
        {
            _log.Information(this.GetMethodName(), $"The application will attempt to move the Config folder from [{ConfigDirectoryHelper.GetLocalPath(startingLocation)}] to [{ConfigDirectoryHelper.GetLocalPath(targetLocation)}]");
            _log.Information(this.GetMethodName(), $@"
//********************************************************************
//********************************************************************
//********************************************************************
//********************************************************************
//********************************************************************");
            ConfigDirectoryHelper.MoveConfigToNewLocation(startingLocation, targetLocation);
            Application.Restart();
        }

        private void OnRateSubmittedToastAction(ValueSet userInput, ToastArguments toastArguments)
        {
            var songInfo = ObjectSerializer.DeserializeFromBase64<PlayListSong>(toastArguments[nameof(PlayListSong)]);

            if (!userInput.TryGetValue(Constants.UserRatingFieldKey, out object rawUserRate)
                || !Int32.TryParse(rawUserRate.ToString().Trim(), out int userRate)
                || userRate < 1 || 10 < userRate)
            {
                _toastHandlerFactory.Create().ShowInvalidRatingArgumentToast(rawUserRate.ToString().Trim());
                return;
            }

            var ratingResponse = _rpApiClientFactory.Create().GetRating(songInfo.SongId, userRate);
            if (ratingResponse.Status == "success")
            {
                if (songInfo.SongId == _config.State.Playback.SongInfo.SongId)
                {
                    _config.State.Playback = new Playback(_rpApiClientFactory.Create().GetNowplayingList());
                }
                else
                {
                    var newRating = _rpApiClientFactory.Create().GetInfo(songInfo.SongId).UserRating.ToString();
                    songInfo.UserRating = newRating;
                    _toastHandlerFactory.Create().ShowSongStartToast(true, songInfo);
                }
            }
        }

        private void OnRatingToastRequestedAction(ToastArguments toastArguments)
        {
            var songInfo = ObjectSerializer.DeserializeFromBase64<PlayListSong>(toastArguments[nameof(PlayListSong)]);
            KeyboardSendKeyHelper.SendWinKeyN();
            Task.Delay(200).Wait();
            _toastHandlerFactory.Create().ShowSongRatingToast(songInfo);
        }

        #endregion

        private void OnLoginInputSubmitted(object sender, LoginInputEvent loginInputEvent)
        {
            _log.Information(this.GetMethodName(), $"Login input submitted for user [{loginInputEvent.UserName}]");

            var loginResponse = _rpApiClientFactory.Create().GetAuth(loginInputEvent.UserName, loginInputEvent.Password);
            _toastHandlerFactory.Create().ShowLoginResponseToast(loginResponse);
        }

        private void OnComputerWakeUp(object sender, PowerModeChangedEventArgs e)
        {
            // clean up potentially stuck threads / re-initiate app

            if (e.Mode == PowerModes.Resume)
            {
                _log.Information(this.GetMethodName(), "PC woke up - RESTART");
                Application.Restart();
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            _rpTrayIcon.Dispose();
            AlbumartFileHelper.DeleteOldAlbumartImageFiles(_config);

            _log.Information(this.GetMethodName(), $@"Exit Application
********************************************************************
********************************************************************
********************************************************************
********************************************************************
********************************************************************");

            CheckQueuedDataDeleteRequest();
        }

        private void CheckQueuedDataDeleteRequest()
        {
            if (_config.PersistedConfig.DeleteAllData || _config.StaticConfig.CleanUpOnExit)
            {
                _log.Information(this.GetMethodName(), @"App data delete requested
* *******************************************************************
********************************************************************
********************************************************************
********************************************************************
********************************************************************");
                _toastHandlerFactory.Create().ShowDataEraseToast();
                Task.Delay(5000).Wait();
                ToastNotificationManagerCompat.History.Clear();
                ToastNotificationManagerCompat.Uninstall();
                ConfigDirectoryHelper.DeleteConfigDirectory(_config.StaticConfig.ConfigBaseFolderOption);
                Application.Exit();
            }
        }
    }
}
