using RP_Notify.Config;
using RP_Notify.Helpers;
using RP_Notify.Logger;
using RP_Notify.Properties;
using RP_Notify.RpApi.ResponseModel;
using RP_Notify.ToastHandler;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RP_Notify.TrayIconMenu
{
    class RpTrayIconMenu
    {
        private readonly IConfigRoot _config;
        private readonly ILoggerWrapper _log;
        private readonly LoginForm.LoginForm _loginForm;
        private readonly IToastHandlerFactory _toastHandlerFactory;


        private readonly ContextMenu contextMenu;

        public NotifyIcon NotifyIcon { get; }

        public RpTrayIconMenu(IConfigRoot config, ILoggerWrapper log, LoginForm.LoginForm loginForm, IToastHandlerFactory toastHandlerFactory)
        {
            _config = config;
            _log = log;
            _toastHandlerFactory = toastHandlerFactory;

            contextMenu = new ContextMenu();
            NotifyIcon = new NotifyIcon();
            _loginForm = loginForm;
        }

        public void Init()
        {

            BuildContextMenu();
            NotifyIcon.ContextMenu = contextMenu;
            NotifyIcon.Icon = Resources.RPIcon;
            NotifyIcon.Text = Application.ProductName;
            NotifyIcon.Visible = true;
        }

        public void Dispose()
        {
            _log.Information(this.GetMethodName(), "Started");

            if (contextMenu != null)
            {
                contextMenu.Dispose();
            }

            if (NotifyIcon != null)
            {
                NotifyIcon.Visible = false;
                NotifyIcon.Dispose();
            }

            _log.Information(this.GetMethodName(), "Finished");
        }
        public void BuildContextMenu()
        {
            _log.Information(this.GetMethodName(), "Started");

            var menuEntryShowOnNewSong = MenuEntryShowOnNewSong();
            var menuEntryLargeAlbumArt = MenuEntryLargeAlbumArt();
            var menuEntryRpBannerOnDetail = MenuEntryRpBannerOnDetail();
            var menuEntryShowSongRating = MenuEntryShowSongRating();
            var menuEntryPromptForRating = MenuEntryPromptForRating();
            var menuEntryMigrateCache = MenuEntryMigrateCache();
            var menuEntryReset = MenuEntryReset();
            var menuEntryEnableFoobar2000Watcher = MenuEntryEnableFoobar2000Watcher();
            var menuEntryEnableMusicbeeWatcher = MenuEntryEnableMusicbeeWatcher();
            var menuEntryRpTracking = MenuEntryRpTracking();
            var menuEntryListTrackablePlayers = MenuEntryListTrackablePlayers();
            var menuEntryListChannels = MenuEntryListChannels();
            var menuEntryLogin = MenuEntryLogIn();
            var menuEntryAbout = MenuEntryAbout();
            var menuEntryExit = MenuEntryExit();

            contextMenu.MenuItems.Clear();

            contextMenu.MenuItems.Add(menuEntryShowOnNewSong);
            contextMenu.MenuItems.Add("-");
            MenuItem toastFormat = new MenuItem("Toast format");
            toastFormat.MenuItems.Add(menuEntryLargeAlbumArt);
            toastFormat.MenuItems.Add(menuEntryRpBannerOnDetail);
            toastFormat.MenuItems.Add(menuEntryShowSongRating);
            contextMenu.MenuItems.Add(toastFormat);
            MenuItem appSettings = new MenuItem("App settings");
            appSettings.MenuItems.Add(menuEntryPromptForRating);
            appSettings.MenuItems.Add("-");
            appSettings.MenuItems.Add(menuEntryMigrateCache);
            appSettings.MenuItems.Add(menuEntryReset);
            contextMenu.MenuItems.Add(appSettings);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(menuEntryEnableFoobar2000Watcher);
            contextMenu.MenuItems.Add(menuEntryEnableMusicbeeWatcher);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(menuEntryRpTracking);
            contextMenu.MenuItems.AddRange(menuEntryListTrackablePlayers);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.AddRange(menuEntryListChannels);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(menuEntryLogin);
            contextMenu.MenuItems.Add(menuEntryAbout);
            contextMenu.MenuItems.Add(menuEntryExit);

            _log.Information(this.GetMethodName(), "Finished");
        }

        private MenuItem MenuEntryShowOnNewSong()
        {
            bool trackingActive = _config.State.Foobar2000IsPlayingRP
                || _config.State.MusicBeeIsPlayingRP
                || _config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _);
            var menuName = $"Show on new song{(!trackingActive ? " (Live stream)" : null)}";

            MenuItem showOnNewSong = new MenuItem(menuName)
            {
                Checked = _config.PersistedConfig.ShowOnNewSong || trackingActive,
                Enabled = !trackingActive,
                DefaultItem = _config.PersistedConfig.ShowOnNewSong && !trackingActive

            };
            ;

            showOnNewSong.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                _config.PersistedConfig.ShowOnNewSong = !_config.PersistedConfig.ShowOnNewSong;
            };

            return showOnNewSong;
        }

        private MenuItem MenuEntryLargeAlbumArt()
        {
            var menuName = "Large album art";

            MenuItem largeAlbumArt = new MenuItem(menuName)
            {
                Checked = _config.PersistedConfig.LargeAlbumArt
            };

            largeAlbumArt.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                _config.PersistedConfig.LargeAlbumArt = !_config.PersistedConfig.LargeAlbumArt;
            };

            return largeAlbumArt;
        }

        private MenuItem MenuEntryRpBannerOnDetail()
        {
            var menuName = "Display RP banner on detail toast";

            MenuItem rpBannerOnDetail = new MenuItem(menuName)
            {
                Checked = _config.PersistedConfig.RpBannerOnDetail,
                Enabled = _config.PersistedConfig.LargeAlbumArt
            };

            rpBannerOnDetail.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                _config.PersistedConfig.RpBannerOnDetail = !_config.PersistedConfig.RpBannerOnDetail;
            };

            return rpBannerOnDetail;
        }

        private MenuItem MenuEntryShowSongRating()
        {
            var menuName = "Show song rating";

            MenuItem showSongRating = new MenuItem(menuName)
            {
                Checked = _config.PersistedConfig.ShowSongRating
            };

            showSongRating.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                _config.PersistedConfig.ShowSongRating = !_config.PersistedConfig.ShowSongRating;
            };

            return showSongRating;
        }

        private MenuItem MenuEntryPromptForRating()
        {
            var menuName = "Prompt for Rating";

            MenuItem promptForRating = new MenuItem(menuName)
            {
                Checked = _config.PersistedConfig.PromptForRating,
                Enabled = _config.IsUserAuthenticated(out string _)
            };

            promptForRating.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                _config.PersistedConfig.PromptForRating = !_config.PersistedConfig.PromptForRating;
            };

            return promptForRating;
        }

        private MenuItem MenuEntryReset()
        {
            var menuName = "Delete app data (including login info)";

            MenuItem reset = new MenuItem(menuName);
            reset.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                _config.PersistedConfig.DeleteAllData = !_config.PersistedConfig.DeleteAllData;
            };

            return reset;
        }

        private MenuItem MenuEntryMigrateCache()
        {


            string menuName = "Migrate Config folder to ";
            ConfigLocationOptions startingLocation;
            ConfigLocationOptions targetLocation;
            switch (_config.StaticConfig.ConfigBaseFolderOption)
            {
                case ConfigLocationOptions.AppData:
                    menuName += nameof(ConfigLocationOptions.ExeContainingDirectory);
                    startingLocation = ConfigLocationOptions.AppData;
                    targetLocation = ConfigLocationOptions.ExeContainingDirectory;
                    break;
                case ConfigLocationOptions.ExeContainingDirectory:
                    menuName += nameof(ConfigLocationOptions.AppData);
                    startingLocation = ConfigLocationOptions.ExeContainingDirectory;
                    targetLocation = ConfigLocationOptions.AppData;
                    break;
                default:
                    throw new NotImplementedException();
            }

            MenuItem migrateCache = new MenuItem(menuName);
            migrateCache.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                _log.Information(this.GetMethodName(), $"The application will attempt to move the Config folder from [{ConfigDirectoryHelper.GetLocalPath(startingLocation)}] to [{ConfigDirectoryHelper.GetLocalPath(targetLocation)}]");
                _log.Information(this.GetMethodName(), $@"
//********************************************************************
//********************************************************************
//********************************************************************
//********************************************************************
//********************************************************************");
                ConfigDirectoryHelper.MoveConfigToNewLocation(startingLocation, targetLocation);
                Application.Restart();
            };

            return migrateCache;
        }

        private MenuItem MenuEntryEnableFoobar2000Watcher()
        {
            var menuName = "Track Foobar2000";

            MenuItem enablePlayerWatcher = new MenuItem(menuName)
            {
                Checked = _config.PersistedConfig.EnableFoobar2000Watcher,
                DefaultItem = _config.State.Foobar2000IsPlayingRP
                    && !_config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _)
            };

            enablePlayerWatcher.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                _config.PersistedConfig.EnableFoobar2000Watcher = !_config.PersistedConfig.EnableFoobar2000Watcher;
            };

            return enablePlayerWatcher;
        }

        private MenuItem MenuEntryEnableMusicbeeWatcher()
        {
            var menuName = "Track MusicBee";

            MenuItem enablePlayerWatcher = new MenuItem(menuName)
            {
                Checked = _config.PersistedConfig.EnableMusicBeeWatcher,
                DefaultItem = _config.State.MusicBeeIsPlayingRP
                    && !_config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _)
            };

            enablePlayerWatcher.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                _config.PersistedConfig.EnableMusicBeeWatcher = !_config.PersistedConfig.EnableMusicBeeWatcher;
            };

            return enablePlayerWatcher;
        }

        private MenuItem MenuEntryRpTracking()
        {
            var menuName = "Track official RP players";

            MenuItem rpTracking = new MenuItem(menuName)
            {
                Checked = _config.PersistedConfig.EnableRpOfficialTracking,
            };

            rpTracking.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                _config.PersistedConfig.EnableRpOfficialTracking = !_config.PersistedConfig.EnableRpOfficialTracking;
            };

            return rpTracking;
        }

        private MenuItem[] MenuEntryListTrackablePlayers()
        {
            List<MenuItem> TrackablePlayers = new List<MenuItem>();

            if (_config.PersistedConfig.EnableRpOfficialTracking)
            {
                foreach (var player in _config.State.RpTrackingConfig.Players)
                {
                    var playedChannel = _config.State.ChannelList.Where(c => c.Chan == player.Chan).FirstOrDefault();
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    var channelName = textInfo.ToTitleCase(playedChannel.StreamName);
                    var menuName = $"{player.Source} ({channelName})";

                    MenuItem trackedPlayer = new MenuItem(menuName)
                    {
                        RadioCheck = true,
                        Checked = _config.State.RpTrackingConfig.ActivePlayerId == player.PlayerId,
                        DefaultItem = _config.State.RpTrackingConfig.ActivePlayerId == player.PlayerId
                    };

                    trackedPlayer.Click += (sender, e) =>
                    {
                        _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");

                        if (_config.State.RpTrackingConfig.ActivePlayerId == player.PlayerId)
                        {
                            _config.State.RpTrackingConfig.ActivePlayerId = null;
                        }
                        else
                        {
                            _config.State.RpTrackingConfig.ActivePlayerId = player.PlayerId;
                        }
                    };

                    TrackablePlayers.Add(trackedPlayer);
                }
            }

            return TrackablePlayers.ToArray();
        }

        private MenuItem[] MenuEntryListChannels()
        {
            List<MenuItem> Channels = new List<MenuItem>();

            foreach (Channel loopChannel in _config.State.ChannelList)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                var menuName = textInfo.ToTitleCase(loopChannel.StreamName);

                MenuItem channelMenuItem = new MenuItem(menuName)
                {
                    RadioCheck = true,
                    Checked = _config.PersistedConfig.Channel.Equals(Int32.Parse(loopChannel.Chan)),
                    Tag = loopChannel,
                    Enabled = loopChannel.Chan != "99"
                        && (!((_config.PersistedConfig.EnableFoobar2000Watcher && _config.State.Foobar2000IsPlayingRP)
                        || (_config.PersistedConfig.EnableMusicBeeWatcher && _config.State.MusicBeeIsPlayingRP)
                        || _config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _)))
                };

                channelMenuItem.Click += (sender, e) =>
                {
                    _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");

                    foreach (MenuItem menu in contextMenu.MenuItems)
                    {
                        if (menu.Tag == null || menu.Tag.GetType() != loopChannel.GetType())
                        {
                            continue;
                        }

                        menu.Checked = ((Channel)menu.Tag).Chan == loopChannel.Chan;
                    }

                    _config.PersistedConfig.Channel = Int32.Parse(loopChannel.Chan);
                };

                Channels.Add(channelMenuItem);
            }

            return Channels.ToArray();
        }

        private MenuItem MenuEntryLogIn()
        {
            var menuName = _config.IsUserAuthenticated(out string loggedInUsername)
                ? $"Log Out ({loggedInUsername})"
                : "Log In";

            MenuItem login = new MenuItem(menuName);
            login.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");

                if (_config.IsUserAuthenticated(out string _))
                {
                    _config.State.RpCookieContainer = new System.Net.CookieContainer();
                    Retry.Do(() => { File.Delete(_config.StaticConfig.CookieCachePath); });
                    _toastHandlerFactory.Create().ShowLogoutRequestToast(loggedInUsername);
                    _log.Information(this.GetMethodName(), "Cookie cache deleted");
                    Application.Restart();
                }
                else
                {
                    _loginForm.ShowDialog();
                }
            };

            return login;
        }

        private MenuItem MenuEntryAbout()
        {
            var menuName = "About";

            MenuItem about = new MenuItem(menuName);
            about.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                System.Diagnostics.Process.Start("https://github.com/gvajda/radio-paradise-song-notification");
            };

            return about;
        }

        private MenuItem MenuEntryExit()
        {
            var menuName = "E&xit";

            MenuItem exit = new MenuItem(menuName);
            exit.Click += (sender, e) =>
            {
                _log.Information(this.GetMethodName(), $"User clicked menu: '{menuName}'");
                Application.Exit();
            };

            return exit;
        }
    }
}
