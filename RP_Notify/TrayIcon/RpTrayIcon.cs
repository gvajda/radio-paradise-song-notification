using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Properties;
using RP_Notify.Toast;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP_Notify.TrayIcon
{
    class RpTrayIcon
    {
        private readonly IConfig _config;
        private readonly ILog _log;
        private readonly IToastHandler _toastHandler;


        private readonly ContextMenu contextMenu;

        public NotifyIcon NotifyIcon { get; }

        private CancellationTokenSource contextMenuBuilderCancellationTokenSource;

        private Task ContextMenuBuilderTask { get; set; }

        public RpTrayIcon(IConfig config, ILog log, IToastHandler toastHandler)
        {
            _config = config;
            _log = log;
            _toastHandler = toastHandler;

            contextMenu = new ContextMenu();
            NotifyIcon = new NotifyIcon();
            contextMenuBuilderCancellationTokenSource = new CancellationTokenSource();
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
            _log.Information(LogHelper.GetMethodName(this), "Started");

            if (contextMenu != null)
            {
                contextMenu.Dispose();
            }

            if (NotifyIcon != null)
            {
                NotifyIcon.Visible = false;
                NotifyIcon.Dispose();
            }

            _log.Information(LogHelper.GetMethodName(this), "Finished");
        }
        public void BuildContextMenu()
        {
            _log.Information(LogHelper.GetMethodName(this), "Started");

            var menuEntryShowOnNewSong = MenuEntryShowOnNewSong();
            var menuEntryLargeAlbumArt = MenuEntryLargeAlbumArt();
            var menuEntryShowSongRating = MenuEntryShowSongRating();
            var menuEntryPromptForRating = MenuEntryPromptForRating();
            var menuEntryLeaveShorcutInStartMenu = MenuEntryLeaveShorcutInStartMenu();
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
            toastFormat.MenuItems.Add(menuEntryShowSongRating);
            contextMenu.MenuItems.Add(toastFormat);
            MenuItem appSettings = new MenuItem("App settings");
            appSettings.MenuItems.Add(menuEntryPromptForRating);
            appSettings.MenuItems.Add(menuEntryLeaveShorcutInStartMenu);
            appSettings.MenuItems.Add("-");
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

            _log.Information(LogHelper.GetMethodName(this), "Finished");
        }

        private MenuItem MenuEntryShowOnNewSong()
        {
            bool trackingActive = _config.State.Foobar2000IsPlayingRP
                || _config.State.MusicBeeIsPlayingRP
                || _config.IsRpPlayerTrackingChannel();
            var menuName = $"Show on new song{(!trackingActive ? " (Live stream)" : null)}";

            MenuItem showOnNewSong = new MenuItem(menuName)
            {
                Checked = _config.ExternalConfig.ShowOnNewSong || trackingActive,
                Enabled = !trackingActive,
                DefaultItem = _config.ExternalConfig.ShowOnNewSong && !trackingActive

            };

            showOnNewSong.Click += (sender, e) =>
            {
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
                _config.ExternalConfig.ShowOnNewSong = !_config.ExternalConfig.ShowOnNewSong;
            };

            return showOnNewSong;
        }

        private MenuItem MenuEntryLargeAlbumArt()
        {
            var menuName = "Large album art";

            MenuItem largeAlbumArt = new MenuItem(menuName)
            {
                Checked = _config.ExternalConfig.LargeAlbumArt
            };

            largeAlbumArt.Click += (sender, e) =>
            {
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
                _config.ExternalConfig.LargeAlbumArt = !_config.ExternalConfig.LargeAlbumArt;
            };

            return largeAlbumArt;
        }

        private MenuItem MenuEntryShowSongRating()
        {
            var menuName = "Show song rating";

            MenuItem showSongRating = new MenuItem(menuName)
            {
                Checked = _config.ExternalConfig.ShowSongRating
            };

            showSongRating.Click += (sender, e) =>
            {
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
                _config.ExternalConfig.ShowSongRating = !_config.ExternalConfig.ShowSongRating;
            };

            return showSongRating;
        }

        private MenuItem MenuEntryPromptForRating()
        {
            var menuName = "Prompt for Rating";

            MenuItem promptForRating = new MenuItem(menuName)
            {
                Checked = _config.ExternalConfig.PromptForRating,
                Enabled = _config.State.IsUserAuthenticated
            };

            promptForRating.Click += (sender, e) =>
            {
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
                _config.ExternalConfig.PromptForRating = !_config.ExternalConfig.PromptForRating;
            };

            return promptForRating;
        }

        private MenuItem MenuEntryLeaveShorcutInStartMenu()
        {
            var menuName = "Leave shortcut in Start menu";

            MenuItem leaveShorcutInStartMenu = new MenuItem(menuName)
            {
                Checked = _config.ExternalConfig.LeaveShorcutInStartMenu
            };
            leaveShorcutInStartMenu.Click += (sender, e) =>
            {
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
                _config.ExternalConfig.LeaveShorcutInStartMenu = !_config.ExternalConfig.LeaveShorcutInStartMenu;
            };

            return leaveShorcutInStartMenu;
        }

        private MenuItem MenuEntryReset()
        {
            var menuName = "Delete app data";

            MenuItem reset = new MenuItem(menuName);
            reset.Click += (sender, e) =>
            {
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
                _config.ExternalConfig.DeleteAllData = !_config.ExternalConfig.DeleteAllData;
            };

            return reset;
        }

        private MenuItem MenuEntryEnableFoobar2000Watcher()
        {
            var menuName = "Track Foobar2000";

            MenuItem enablePlayerWatcher = new MenuItem(menuName)
            {
                Checked = _config.ExternalConfig.EnableFoobar2000Watcher,
                DefaultItem = _config.State.Foobar2000IsPlayingRP
                    && !_config.IsRpPlayerTrackingChannel()
            };

            enablePlayerWatcher.Click += (sender, e) =>
            {
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
                _config.ExternalConfig.EnableFoobar2000Watcher = !_config.ExternalConfig.EnableFoobar2000Watcher;
            };

            return enablePlayerWatcher;
        }

        private MenuItem MenuEntryEnableMusicbeeWatcher()
        {
            var menuName = "Track MusicBee";

            MenuItem enablePlayerWatcher = new MenuItem(menuName)
            {
                Checked = _config.ExternalConfig.EnableMusicBeeWatcher,
                DefaultItem = _config.State.MusicBeeIsPlayingRP
                    && !_config.IsRpPlayerTrackingChannel()
            };

            enablePlayerWatcher.Click += (sender, e) =>
            {
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
                _config.ExternalConfig.EnableMusicBeeWatcher = !_config.ExternalConfig.EnableMusicBeeWatcher;
            };

            return enablePlayerWatcher;
        }

        private MenuItem MenuEntryRpTracking()
        {
            var menuName = "Track official RP players";

            MenuItem rpTracking = new MenuItem(menuName)
            {
                Checked = _config.ExternalConfig.EnableRpOfficialTracking,
            };

            rpTracking.Click += (sender, e) =>
            {
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
                _config.ExternalConfig.EnableRpOfficialTracking = !_config.ExternalConfig.EnableRpOfficialTracking;
            };

            return rpTracking;
        }

        private MenuItem[] MenuEntryListTrackablePlayers()
        {
            List<MenuItem> TrackablePlayers = new List<MenuItem>();

            if (_config.ExternalConfig.EnableRpOfficialTracking)
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
                        _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");

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
                    Checked = _config.ExternalConfig.Channel.Equals(Int32.Parse(loopChannel.Chan)),
                    Tag = loopChannel,
                    Enabled = loopChannel.Chan != "99"
                        && (!((_config.ExternalConfig.EnableFoobar2000Watcher && _config.State.Foobar2000IsPlayingRP)
                        || (_config.ExternalConfig.EnableMusicBeeWatcher && _config.State.MusicBeeIsPlayingRP)
                        || _config.IsRpPlayerTrackingChannel()))
                };

                channelMenuItem.Click += (sender, e) =>
                {
                    _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");

                    foreach (MenuItem menu in contextMenu.MenuItems)
                    {
                        if (menu.Tag == null || menu.Tag.GetType() != loopChannel.GetType())
                        {
                            continue;
                        }

                        menu.Checked = ((Channel)menu.Tag).Chan == loopChannel.Chan;
                    }

                    _config.ExternalConfig.Channel = Int32.Parse(loopChannel.Chan);
                };

                Channels.Add(channelMenuItem);
            }

            return Channels.ToArray();
        }

        private MenuItem MenuEntryLogIn()
        {
            var menuName = _config.State.IsUserAuthenticated
                ? "Log Out"
                : "Log In";

            MenuItem login = new MenuItem(menuName);
            login.Click += (sender, e) =>
            {
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");

                if (_config.State.IsUserAuthenticated)
                {
                    Retry.Do(() => { File.Delete(_config.StaticConfig.CookieCachePath); });
                    _log.Information(LogHelper.GetMethodName(this), "Cookie cache deleted - Restart");
                    Application.Restart();
                }
                else
                {
                    _toastHandler.ShowLoginToast();
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
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
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
                _log.Information(LogHelper.GetMethodName(this), $"User clicked menu: '{menuName}'");
                Application.Exit();
            };

            return exit;
        }
    }
}
