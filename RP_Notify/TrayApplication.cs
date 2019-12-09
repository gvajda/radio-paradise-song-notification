using RP_Notify.API;
using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Foobar2000Watcher;
using RP_Notify.Properties;
using RP_Notify.SongInfoUpdater;
using RP_Notify.StartMenuShortcut;
using RP_Notify.Toast;
using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace RP_Notify
{
    class TrayApplication : ApplicationContext
    {
        private readonly IRpApiHandler _apihandler;
        private readonly IConfig _config;
        private readonly IToastHandler _toastHandler;
        private readonly ShortcutHelper _shortcutHelper;
        private readonly IPlayerWatcher _playerWatcher;
        private readonly ISongInfoListener _songInfoListener;
        private readonly ILogger _log;

        private bool resetFlag;

        protected ContextMenu ContextMenu { get; set; }
        protected NotifyIcon TrayIcon { get; set; }

        public TrayApplication(IConfig config, IRpApiHandler apiHandler, IToastHandler toastHandler, IPlayerWatcher playerWatcher, ISongInfoListener songInfoListener, ILog log)
        {
            _log = log.Logger;
            _log.Information("RP_Notify started ********************************************************************");

            // From service collection
            _apihandler = apiHandler;
            _config = config;
            _toastHandler = toastHandler;
            _songInfoListener = songInfoListener;
            _playerWatcher = playerWatcher;

            resetFlag = false;

            // Init fields
            _shortcutHelper = new ShortcutHelper(config);
            _shortcutHelper.TryCreateShortcut();    // Add shortcut to Start menu (required for Toast Notifications)

            // Add context menu
            _log.Information("Create tray icon");
            ContextMenu = new ContextMenu();
            BuildContextMenu();
            TrayIcon = CreateTrayIcon();

            // Set up event handlers
            _log.Information("Create event listeners");
            TrayIcon.MouseDoubleClick += this.TrayIconDoubleClickHandler;
            _config.ConfigChangedEventHandler += ConfigChangeHandler;
            _playerWatcher.ConfigChangedEventHandler += ConfigChangeHandler;
            _songInfoListener.TooltipUpdateChangedEventHandler += (sender, e) => TrayIcon.Text = e.ToolTipText;
            Application.ApplicationExit += this.ApplicationExitHandler;

            // Start background tasks
            _log.Information("Start background tasks");
            _playerWatcher.StartChannelWatch();
            _songInfoListener.Run();
        }

        private NotifyIcon CreateTrayIcon()
        {
            return new NotifyIcon
            {
                Icon = Resources.RPIcon,
                ContextMenu = ContextMenu,
                Text = Application.ProductName,
                Visible = true
            };
        }

        private void BuildContextMenu()
        {
            _log.Information("Build context menu");
            ContextMenu.MenuItems.Clear();

            MenuItem showOnNewSong = new MenuItem("Show on new song")
            {
                Checked = _config.ShowOnNewSong
                    || (_config.EnablePlayerWatcher && _playerWatcher.PlayerIsActive)
            };
            showOnNewSong.Click += (sender, e) =>
            {
                showOnNewSong.Checked = !showOnNewSong.Checked;
                _config.ShowOnNewSong = showOnNewSong.Checked;
                if (_config.ShowOnNewSong)
                {
                    _toastHandler.ShowSongStartToast();
                }
            };
            ContextMenu.MenuItems.Add(showOnNewSong);
            ContextMenu.MenuItems[ContextMenu.MenuItems.Count - 1].Enabled = !(_config.EnablePlayerWatcher && _playerWatcher.PlayerIsActive);

            ContextMenu.MenuItems.Add(new MenuItem("-"));

            MenuItem largeAlbumArt = new MenuItem("Large album art")
            {
                Checked = _config.LargeAlbumArt
            };
            largeAlbumArt.Click += (sender, e) =>
            {
                largeAlbumArt.Checked = !largeAlbumArt.Checked;
                _config.LargeAlbumArt = largeAlbumArt.Checked;
            };
            ContextMenu.MenuItems.Add(largeAlbumArt);

            MenuItem showSongRating = new MenuItem("Show song rating")
            {
                Checked = _config.ShowSongRating
            };
            showSongRating.Click += (sender, e) =>
            {
                showSongRating.Checked = !showSongRating.Checked;
                _config.ShowSongRating = showSongRating.Checked;
            };
            ContextMenu.MenuItems.Add(showSongRating);

            MenuItem toastFormat = new MenuItem("Toast format");
            toastFormat.MenuItems.AddRange(new MenuItem[] { largeAlbumArt, showSongRating });
            ContextMenu.MenuItems.Add(toastFormat);

            MenuItem leaveShorcutInStartMenu = new MenuItem("Leave shortcut in Start menu")
            {
                Checked = _config.LeaveShorcutInStartMenu
            };
            leaveShorcutInStartMenu.Click += (sender, e) =>
            {
                leaveShorcutInStartMenu.Checked = !leaveShorcutInStartMenu.Checked;
                _config.LeaveShorcutInStartMenu = leaveShorcutInStartMenu.Checked;
            };
            ContextMenu.MenuItems.Add(leaveShorcutInStartMenu);

            MenuItem enablePlayerWatcher = new MenuItem("Enable Foobar2000 watcher")
            {
                Checked = _config.EnablePlayerWatcher
            };
            enablePlayerWatcher.Click += (sender, e) =>
            {
                _config.EnablePlayerWatcher = !enablePlayerWatcher.Checked;
                if (_config.EnablePlayerWatcher)
                {
                    _toastHandler.ShowSongStartToast();
                }
                BuildContextMenu();
            };
            ContextMenu.MenuItems.Add(enablePlayerWatcher);

            MenuItem reset = new MenuItem("Delete app data");
            reset.Click += ResetHandler;
            ContextMenu.MenuItems.Add(reset);

            MenuItem appSettings = new MenuItem("App settings");
            appSettings.MenuItems.Add(enablePlayerWatcher);
            appSettings.MenuItems.Add(leaveShorcutInStartMenu);
            appSettings.MenuItems.Add(new MenuItem("-"));
            appSettings.MenuItems.Add(reset);
            ContextMenu.MenuItems.Add(appSettings);

            ContextMenu.MenuItems.Add(new MenuItem("-"));

            foreach (Channel loopChannel in _apihandler.ChannelList)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                MenuItem channelMenuItem = new MenuItem(textInfo.ToTitleCase(loopChannel.StreamName))
                {
                    RadioCheck = true,
                    Checked = _config.Channel.Equals(Int32.Parse(loopChannel.Chan)),
                    Tag = loopChannel
                };
                channelMenuItem.Click += (sender, e) =>
                {
                    foreach (MenuItem menu in ContextMenu.MenuItems)
                    {
                        if (menu.Tag == null || menu.Tag.GetType() != loopChannel.GetType())
                        {
                            continue;
                        }

                        menu.Checked = ((Channel)menu.Tag).Chan == loopChannel.Chan;
                    }
                    //does not send ConfigExternallyChangedEvent
                    _config.Channel = Int32.Parse(loopChannel.Chan);
                    _songInfoListener.nextSongWaiterCancellationTokenSource.Cancel();
                };
                ContextMenu.MenuItems.Add(channelMenuItem);
                ContextMenu.MenuItems[ContextMenu.MenuItems.Count - 1].Enabled = !(_config.EnablePlayerWatcher && _playerWatcher.PlayerIsActive);
            }

            ContextMenu.MenuItems.Add(new MenuItem("-"));

            MenuItem about = new MenuItem("About");
            about.Click += (sender, e) => { System.Diagnostics.Process.Start("https://github.com/gvajda/radio-paradise-song-notification"); };
            ContextMenu.MenuItems.Add(about);

            MenuItem exit = new MenuItem("E&xit");
            exit.Click += (sender, e) => { ExitThread(); };
            ContextMenu.MenuItems.Add(exit);
        }

        private void TrayIconDoubleClickHandler(object sender, MouseEventArgs e)
        {
            _log.Information("TrayIconDoubleClickHandler - Invoked - Sender: {Sender}", sender.GetType());
            _toastHandler.ShowSongDetailToast();

        }

        private void ConfigChangeHandler(object sender, ConfigChangeEventArgs e)
        {
            _log.Information("ConfigChangeHandler - Invoked - Sender: {Sender} - Channel changed: {ChannelChanged} - Show on new song flag changed: {ShowOnNewSongChanged} - Player state flag changed: {PlayerStateChanged}", sender.GetType(), e.ChannelChanged, e.ShowOnNewSongChanged, e.PlayerStateChanged);
            BuildContextMenu();
            if (e.ChannelChanged)
            {
                // nextSongWaiterCancellationTokenSource.Cancel();
                _songInfoListener.nextSongWaiterCancellationTokenSource.Cancel();
            }
            else if ((e.ShowOnNewSongChanged && _config.ShowOnNewSong)   // ShowOnChanged turned on
                || (e.PlayerStateChanged && _playerWatcher.PlayerIsActive))     // Playback started
            {
                _toastHandler.ShowSongStartToast();
            }
        }

        private void ApplicationExitHandler(object sender, EventArgs e)
        {
            _log.Information("ApplicationExitHandler - Exit process started");

            if (File.Exists(_config.AlbumArtImagePath))
            {
                _log.Debug("ApplicationExitHandler - Delete album art");
                File.Delete(_config.AlbumArtImagePath);
            }

            if (!_config.LeaveShorcutInStartMenu)
            {
                _shortcutHelper.DeleteShortcut();
                _log.Debug("ApplicationExitHandler - Shortcut removed from start menu");
            }

            if (ContextMenu != null)
            {
                ContextMenu.Dispose();
            }

            if (TrayIcon != null)
            {
                _log.Debug("ApplicationExitHandler - Remove tray icon");
                TrayIcon.Visible = false;
                TrayIcon.Dispose();
            }
            if (resetFlag)
            {
                _shortcutHelper.DeleteShortcut();
                _config.DeletePersistentData();
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

        private void ResetHandler(object sender, EventArgs e)
        {
            _log.Information("ResetHandler - App data delete requested");

            if (_config.EnableLoggingToFile)
            {
                _config.EnableLoggingToFile = false;
                _log.Information("ResetHandler - Can't delete log when file logging is enabled. Logging is disabled now. Please restart and try again");
                TrayIcon.BalloonTipTitle = @"Data delete ERROR";
                TrayIcon.BalloonTipText = @"Can't delete log when file logging is enabled. Logging is disabled now. Please restart and try again";
                TrayIcon.ShowBalloonTip(10);
            }
            else
            {
                resetFlag = true;
            }
            ExitThread();
        }

    }
}
