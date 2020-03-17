using Microsoft.Win32;
using RP_Notify.API;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Foobar2000;
using RP_Notify.SongInfoUpdater;
using RP_Notify.StartMenuShortcut;
using RP_Notify.Toast;
using RP_Notify.TrayIcon;
using Serilog;
using System;
using System.IO;
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
        private readonly ILogger _log;
        private readonly RpTrayIcon _rpTrayIcon;

        public RpApplicationCore(IConfig config, IRpApiHandler apiHandler, IToastHandler toastHandler, Foobar2000Watcher foobar2000Watcher, ISongInfoListener songInfoListener, ILog log, RpTrayIcon rpTrayIcon, ShortcutHelper shortcutHelper)
        {
            _log = log.Logger;
            _log.Information("RP_Notify started ********************************************************************");

            // From service collection
            _apihandler = apiHandler;
            _config = config;
            _toastHandler = toastHandler;
            _songInfoListener = songInfoListener;
            _foobar2000Watcher = foobar2000Watcher;
            _rpTrayIcon = rpTrayIcon;
            _shortcutHelper = shortcutHelper;

            if (_config.ExternalConfig.Channel == 99)      // Reset channel if Favourites were tracked at exit
            {
                _config.ExternalConfig.Channel = 0;
            }

            // Init fields
            _shortcutHelper.TryCreateShortcut();    // Add shortcut to Start menu (required for Toast Notifications)

            // Check if Foobar2000 tracking is enabled
            //if (config.ExternalConfig.EnablePlayerWatcher)
            //{
            //    _foobar2000Watcher.Start();
            //    Task.Run(() => Task.Delay(1500)).Wait();
            //}

            // Set up event handlers
            _log.Information("Create event listeners");
            _config.ExternalConfig.ExternalConfigChangeHandler += OnExternalConfigChange;
            _config.State.StateChangeHandler += OnStateChange;
            _config.State.RpTrackingConfig.RpTrackingConfigChangeHandler += OnRpTrackingConfigChange;
            Application.ApplicationExit += this.ApplicationExitHandler;
            SystemEvents.PowerModeChanged += WakeUpHandler;

            // Check queued application data delet request
            CheckQueuedDataDeleteRequest();

            // Start background tasks
            _log.Information("Start background tasks");
            _songInfoListener.Run();

            // Add context menu
            _log.Information("Create tray icon");
            _rpTrayIcon.Init();
            _rpTrayIcon.NotifyIcon.MouseDoubleClick += TrayIconDoubleClickHandler;
        }


        private void TrayIconDoubleClickHandler(object sender, MouseEventArgs e)
        {
            _log.Information("TrayIconDoubleClickHandler - Invoked - Sender: {Sender}", sender.GetType());
            Task.Run(() => _toastHandler.ShowSongDetailToast());

        }

        private void OnExternalConfigChange(object sender, RpEvent e)
        {
            var valueMessageComponent = e.BoolValue.HasValue
                ? $" - Boolean value: {e.BoolValue.Value}"
                : $" - Value: [Object]";
            _log.Information("OnChange - Invoked - Type: {ChangedType} - Changed field: {ChannelChanged}{ValueMessageComponent}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);

            _rpTrayIcon.BuildContextMenu();

            Task.Delay(200).Wait();

            switch (e.ChangedFieldName)
            {
                case nameof(_config.ExternalConfig.ShowOnNewSong):
                    OnShowOnNewSongChange();
                    break;
                case nameof(_config.ExternalConfig.EnablePlayerWatcher):
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
                case nameof(_config.ExternalConfig.DeleteAllDataOnStartup):
                    OnDeleteAllDataOnStartupChange();
                    break;
                default:
                    _log.Information("OnChange - Finished without action - Type: {ChangedType} - Changed field: {ChannelChanged}{ValueMessageComponent}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);
                    break;
            }
        }

        private void OnStateChange(object sender, RpEvent e)
        {
            var valueMessageComponent = e.BoolValue.HasValue
                ? $" - Boolean value: {e.BoolValue.Value}"
                : $" - Value: [Object]";
            if (!e.ChangedFieldName.Equals(nameof(_config.State.TooltipText)))
            {
                _log.Information("OnChange - Invoked - Type: {ChangedType} - Changed field: {ChannelChanged}{ValueMessageComponent}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);
            }

            Task.Delay(200).Wait();

            switch (e.ChangedFieldName)
            {
                case nameof(_config.State.Foobar2000IsPlayingRP):
                    OnFoobar2000IsPlayingRPChange();
                    break;
                case nameof(_config.State.TooltipText):
                    OnTooltipTextRPChange();
                    break;
                case nameof(_config.State.IsUserAuthenticated):
                    OnIsUserAuthenticatedChange();
                    break;
                case nameof(_config.State.Playback):
                    OnPlaybackChange();
                    break;
                default:
                    _log.Information("OnChange - Finished without action - Type: {ChangedType} - Changed field: {ChannelChanged}{ValueMessageComponent}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);
                    break;
            }
        }

        private void OnRpTrackingConfigChange(object sender, RpEvent e)
        {
            var valueMessageComponent = e.BoolValue.HasValue
                ? $" - Boolean value: {e.BoolValue.Value}"
                : $" - Value: [Object]";
            _log.Information("OnChange - Invoked - Type: {ChangedType} - Changed field: {ChannelChanged}{ValueMessageComponent}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);

            Task.Delay(200).Wait();

            switch (e.ChangedFieldName)
            {
                case nameof(_config.State.RpTrackingConfig.ActivePlayerId):
                    OnActivePlayerIdChange();
                    break;
                case nameof(_config.State.RpTrackingConfig.Enabled):
                    OnEnabledChange();
                    break;
                case nameof(_config.State.RpTrackingConfig.Players):
                    OnPlayersChange();
                    break;
                default:
                    _log.Information("OnChange - Finished without action - Type: {ChangedType} - Changed field: {ChannelChanged}{ValueMessageComponent}", e.SentEventType, e.ChangedFieldName, valueMessageComponent);
                    break;
            }
        }

        private void OnShowOnNewSongChange()
        {
            _toastHandler.ShowSongStartToast();
        }

        private void OnEnablePlayerWatcherChange()
        {
            if (_config.ExternalConfig.EnablePlayerWatcher)
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
            _toastHandler.ShowSongDetailToast();
        }

        private void OnShowSongRatingChange()
        {
            _toastHandler.ShowSongStartToast(true);
        }

        private void OnPromptForRatingChange()
        {
            if (_config.ExternalConfig.PromptForRating)
            {
                _toastHandler.ShowSongRatingToast();
            }
        }

        private void OnChannelChange()
        {
            if (!_config.State.RpTrackingConfig.ValidateActivePlayerId())
            {
                _songInfoListener.ResetListenerLoop();
            }
        }

        private void OnDeleteAllDataOnStartupChange()
        {
            if (_config.ExternalConfig.DeleteAllDataOnStartup)
            {
                _log.Information("ResetHandler - App data delete requested");

                if (_config.ExternalConfig.EnableLoggingToFile)
                {
                    _config.ExternalConfig.EnableLoggingToFile = false;
                    _config.ExternalConfig.DeleteAllDataOnStartup = true;
                    Application.Restart();
                }
                else
                {
                    _config.ExternalConfig.DeleteAllDataOnStartup = true;
                    Application.Exit();
                }
            }
        }

        private void OnFoobar2000IsPlayingRPChange()
        {
            _rpTrayIcon.BuildContextMenu();

            if (_config.State.Foobar2000IsPlayingRP && _config.State.Playback != null)
            {
                _toastHandler.ShowSongStartToast();
            }
        }

        private void OnTooltipTextRPChange()
        {
            _rpTrayIcon.NotifyIcon.Text = _config.State.TooltipText;
        }

        private void OnIsUserAuthenticatedChange()
        {
            _songInfoListener.ResetListenerLoop();
        }

        private void OnPlaybackChange()
        {
            if (!_config.State.Playback.RatingUpdated)
            {
                Retry.Do(() => { File.Delete(_config.StaticConfig.AlbumArtImagePath); });
            }

            _toastHandler.ShowSongStartToast();
        }

        private void OnActivePlayerIdChange()
        {
            _songInfoListener.ResetListenerLoop();
        }

        private void OnEnabledChange()
        {
            if (_config.State.RpTrackingConfig.Enabled)
            {
                _config.State.RpTrackingConfig.Players = _apihandler.GetSync_v2().Players;
            }
            else
            {
                _config.State.RpTrackingConfig = new RpTrackingConfig();
                if (_config.ExternalConfig.Channel == 99)
                {
                    if (_config.ExternalConfig.EnablePlayerWatcher
                        && _foobar2000Watcher.RpChannelIsPlaying(out int fb2kChannel))
                    {
                        _config.ExternalConfig.Channel = fb2kChannel;
                    }
                    else
                    {
                        _config.ExternalConfig.Channel = 0;
                    }
                }
                _config.ExternalConfig.Channel = _config.ExternalConfig.Channel != 99
                    ? _config.ExternalConfig.Channel
                    : 0;
            }
        }

        private void OnPlayersChange()
        {
            _rpTrayIcon.BuildContextMenu();
        }

        private void WakeUpHandler(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                _log.Information("WakeUpHandler - PC woke up - RESTART");
                Application.Restart();
            }
        }

        private void ApplicationExitHandler(object sender, EventArgs e)
        {
            _log.Information("ApplicationExitHandler - Exit process started");

            if (File.Exists(_config.StaticConfig.AlbumArtImagePath))
            {
                _log.Debug("ApplicationExitHandler - Delete album art");
                File.Delete(_config.StaticConfig.AlbumArtImagePath);
            }

            if (!_config.ExternalConfig.LeaveShorcutInStartMenu)
            {
                _shortcutHelper.DeleteShortcut();
                _log.Debug("ApplicationExitHandler - Shortcut removed from start menu");
            }

            _rpTrayIcon.Dispose();

            if (_config.ExternalConfig.DeleteAllDataOnStartup)
            {
                _shortcutHelper.DeleteShortcut();
                _config.ExternalConfig.DeleteConfigRootFolder();
            }
            else
            {
                _log.Information(@"ApplicationExitHandler - Finished
********************************************************************
********************************************************************
********************************************************************
********************************************************************
********************************************************************");
            }
        }

        private void CheckQueuedDataDeleteRequest()
        {
            if (_config.ExternalConfig.DeleteAllDataOnStartup)
            {
                _config.ExternalConfig.DeleteAllDataOnStartup = true;
                _config.ExternalConfig.ShowOnNewSong = false;
                _config.ExternalConfig.EnablePlayerWatcher = false;

                Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    Application.Exit();
                });
            }
        }
    }
}
