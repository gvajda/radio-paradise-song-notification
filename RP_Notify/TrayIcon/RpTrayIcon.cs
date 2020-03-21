﻿using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly ContextMenu contextMenu;

        public NotifyIcon NotifyIcon { get; }

        private CancellationTokenSource contextMenuBuilderCancellationTokenSource;

        private Task ContextMenuBuilderTask { get; set; }

        public RpTrayIcon(IConfig config, ILog log)
        {
            _config = config;
            _log = log;

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

            var menuEntryShowOnNewSong = CreateMenuEntryShowOnNewSong();
            var menuEntryLargeAlbumArt = CreateMenuEntryLargeAlbumArt();
            var menuEntryShowSongRating = CreateMenuEntryShowSongRating();
            var menuEntryPromptForRating = CreateMenuEntryPromptForRating();
            var menuEntryLeaveShorcutInStartMenu = CreateMenuEntryLeaveShorcutInStartMenu();
            var menuEntryReset = CreateMenuEntryReset();
            var menuEntryEnablePlayerWatcher = CreateMenuEntryEnablePlayerWatcher();
            var menuEntryRpTracking = CreateMenuEntryRpTracking();
            var menuEntryListTrackablePlayers = CreateMenuEntryListTrackablePlayers();
            var menuEntryListChannels = CreateMenuEntryListChannels();
            var menuEntryAbout = CreateMenuEntryAbout();
            var menuEntryExit = CreateMenuEntryExit();

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
            contextMenu.MenuItems.Add(menuEntryEnablePlayerWatcher);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(menuEntryRpTracking);
            contextMenu.MenuItems.AddRange(menuEntryListTrackablePlayers);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.AddRange(menuEntryListChannels);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(menuEntryAbout);
            contextMenu.MenuItems.Add(menuEntryExit);

            _log.Information(LogHelper.GetMethodName(this), "Finished");
        }

        private MenuItem CreateMenuEntryShowOnNewSong()
        {
            bool trackingActive = _config.State.Foobar2000IsPlayingRP
                || _config.IsRpPlayerTrackingChannel();

            MenuItem showOnNewSong = new MenuItem($"Show on new song{(!trackingActive ? " (Live stream)" : null)}")
            {
                Checked = _config.ExternalConfig.ShowOnNewSong || trackingActive,
                Enabled = !trackingActive,
                DefaultItem = _config.ExternalConfig.ShowOnNewSong && !trackingActive

            };

            showOnNewSong.Click += (sender, e) =>
            {
                _config.ExternalConfig.ShowOnNewSong = !_config.ExternalConfig.ShowOnNewSong;
            };

            return showOnNewSong;
        }

        private MenuItem CreateMenuEntryLargeAlbumArt()
        {
            MenuItem largeAlbumArt = new MenuItem("Large album art")
            {
                Checked = _config.ExternalConfig.LargeAlbumArt
            };

            largeAlbumArt.Click += (sender, e) =>
            {
                _config.ExternalConfig.LargeAlbumArt = !_config.ExternalConfig.LargeAlbumArt;
            };

            return largeAlbumArt;
        }

        private MenuItem CreateMenuEntryShowSongRating()
        {
            MenuItem showSongRating = new MenuItem("Show song rating")
            {
                Checked = _config.ExternalConfig.ShowSongRating
            };

            showSongRating.Click += (sender, e) =>
            {
                _config.ExternalConfig.ShowSongRating = !_config.ExternalConfig.ShowSongRating;
            };

            return showSongRating;
        }

        private MenuItem CreateMenuEntryPromptForRating()
        {
            MenuItem promptForRating = new MenuItem("Prompt for Rating")
            {
                Checked = _config.ExternalConfig.PromptForRating,
                Enabled = _config.State.IsUserAuthenticated
            };

            promptForRating.Click += (sender, e) =>
            {
                _config.ExternalConfig.PromptForRating = !_config.ExternalConfig.PromptForRating;
            };

            return promptForRating;
        }

        private MenuItem CreateMenuEntryLeaveShorcutInStartMenu()
        {
            MenuItem leaveShorcutInStartMenu = new MenuItem("Leave shortcut in Start menu")
            {
                Checked = _config.ExternalConfig.LeaveShorcutInStartMenu
            };
            leaveShorcutInStartMenu.Click += (sender, e) =>
            {
                _config.ExternalConfig.LeaveShorcutInStartMenu = !_config.ExternalConfig.LeaveShorcutInStartMenu;
            };

            return leaveShorcutInStartMenu;
        }

        private MenuItem CreateMenuEntryReset()
        {
            MenuItem reset = new MenuItem("Delete app data");
            reset.Click += (sender, e) =>
            {
                _config.ExternalConfig.DeleteAllData = !_config.ExternalConfig.DeleteAllData;
            };

            return reset;
        }

        private MenuItem CreateMenuEntryEnablePlayerWatcher()
        {
            MenuItem enablePlayerWatcher = new MenuItem("Track Foobar2000")
            {
                Checked = _config.ExternalConfig.EnableFoobar2000Watcher,
                DefaultItem = _config.State.Foobar2000IsPlayingRP
                    && !_config.IsRpPlayerTrackingChannel()
            };

            enablePlayerWatcher.Click += (sender, e) =>
            {
                _config.ExternalConfig.EnableFoobar2000Watcher = !_config.ExternalConfig.EnableFoobar2000Watcher;
            };

            return enablePlayerWatcher;
        }

        private MenuItem CreateMenuEntryRpTracking()
        {
            MenuItem rpTracking = new MenuItem("Track official RP players")
            {
                Checked = _config.ExternalConfig.EnableRpOfficialTracking,
                DefaultItem = _config.IsRpPlayerTrackingChannel()
            };

            rpTracking.Click += (sender, e) =>
            {
                _config.ExternalConfig.EnableRpOfficialTracking = !_config.ExternalConfig.EnableRpOfficialTracking;
            };

            return rpTracking;
        }

        private MenuItem[] CreateMenuEntryListTrackablePlayers()
        {
            List<MenuItem> TrackablePlayers = new List<MenuItem>();

            if (_config.ExternalConfig.EnableRpOfficialTracking)
            {
                foreach (var player in _config.State.RpTrackingConfig.Players)
                {
                    var playedChannel = _config.State.ChannelList.Where(c => c.Chan == player.Chan).FirstOrDefault();
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    var channelName = textInfo.ToTitleCase(playedChannel.StreamName);

                    MenuItem trackedPlayer = new MenuItem($"{player.Source} ({channelName})")
                    {
                        RadioCheck = true,
                        Checked = _config.State.RpTrackingConfig.ActivePlayerId == player.PlayerId,
                    };

                    trackedPlayer.Click += (sender, e) =>
                    {
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

        private MenuItem[] CreateMenuEntryListChannels()
        {
            List<MenuItem> Channels = new List<MenuItem>();

            foreach (Channel loopChannel in _config.State.ChannelList)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                MenuItem channelMenuItem = new MenuItem(textInfo.ToTitleCase(loopChannel.StreamName))
                {
                    RadioCheck = true,
                    Checked = _config.ExternalConfig.Channel.Equals(Int32.Parse(loopChannel.Chan)),
                    Tag = loopChannel,
                    Enabled = loopChannel.Chan != "99"
                        && (!((_config.ExternalConfig.EnableFoobar2000Watcher && _config.State.Foobar2000IsPlayingRP)
                        || _config.IsRpPlayerTrackingChannel()))
                };

                channelMenuItem.Click += (sender, e) =>
                {
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

        private MenuItem CreateMenuEntryAbout()
        {
            MenuItem about = new MenuItem("About");
            about.Click += (sender, e) =>
            {
                System.Diagnostics.Process.Start("https://github.com/gvajda/radio-paradise-song-notification");
            };

            return about;
        }

        private MenuItem CreateMenuEntryExit()
        {
            MenuItem exit = new MenuItem("E&xit");
            exit.Click += (sender, e) => { Application.Exit(); };

            return exit;
        }
    }
}
