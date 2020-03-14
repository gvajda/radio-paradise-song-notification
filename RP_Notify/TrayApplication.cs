﻿using Microsoft.Win32;
using RP_Notify.API;
using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Foobar2000;
using RP_Notify.Properties;
using RP_Notify.SongInfoUpdater;
using RP_Notify.StartMenuShortcut;
using RP_Notify.Toast;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP_Notify
{
    class TrayApplication : ApplicationContext
    {
        private readonly IRpApiHandler _apihandler;
        private readonly IConfig _config;
        private readonly IToastHandler _toastHandler;
        private readonly ShortcutHelper _shortcutHelper;
        private readonly Foobar2000Watcher _foobar2000Watcher;
        private readonly ISongInfoListener _songInfoListener;
        private readonly ILogger _log;

        private bool resetFlag;

        protected ContextMenu ContextMenu { get; set; }
        protected NotifyIcon TrayIcon { get; set; }

        public TrayApplication(IConfig config, IRpApiHandler apiHandler, IToastHandler toastHandler, Foobar2000Watcher foobar2000Watcher, ISongInfoListener songInfoListener, ILog log)
        {
            _log = log.Logger;
            _log.Information("RP_Notify started ********************************************************************");

            // From service collection
            _apihandler = apiHandler;
            _config = config;
            _toastHandler = toastHandler;
            _songInfoListener = songInfoListener;
            _foobar2000Watcher = foobar2000Watcher;

            resetFlag = false;
            if (_config.ExternalConfig.Channel == 99)      // Reset channel if Favourites were tracked at exit
            {
                _config.ExternalConfig.Channel = 0;
            }

            // Init fields
            _shortcutHelper = new ShortcutHelper(config);
            _shortcutHelper.TryCreateShortcut();    // Add shortcut to Start menu (required for Toast Notifications)

            // Set up event handlers
            _log.Information("Create event listeners");
            _config.ExternalConfig.ConfigChangedEventHandler += ConfigChangeHandler;
            _songInfoListener.TooltipUpdateChangedEventHandler += (sender, e) => TrayIcon.Text = e.ToolTipText;
            Application.ApplicationExit += this.ApplicationExitHandler;
            SystemEvents.PowerModeChanged += WakeUpHandler;

            // Check queued application data delet request
            CheckQueuedDataDeleteRequest();

            // Start background tasks
            _log.Information("Start background tasks");
            if (config.ExternalConfig.EnablePlayerWatcher)
            {
                _foobar2000Watcher.Start();
                Task.Run(() => Task.Delay(1500)).Wait();
            }
            _songInfoListener.Run();

            // Run only after song updater
            _foobar2000Watcher.ConfigChangedEventHandler += ConfigChangeHandler;

            // Add context menu
            _log.Information("Create tray icon");
            ContextMenu = new ContextMenu();
            BuildContextMenu();
            TrayIcon = CreateTrayIcon();
            TrayIcon.MouseDoubleClick += this.TrayIconDoubleClickHandler;
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

            ContextMenu.MenuItems.Add(CreateMenuEntryShowOnNewSong());
            ContextMenu.MenuItems.Add("-");
            MenuItem toastFormat = new MenuItem("Toast format");
            toastFormat.MenuItems.Add(CreateMenuEntryLargeAlbumArt());
            toastFormat.MenuItems.Add(CreateMenuEntryShowSongRating());
            ContextMenu.MenuItems.Add(toastFormat);
            MenuItem appSettings = new MenuItem("App settings");
            appSettings.MenuItems.Add(CreateMenuEntryPromptForRating());
            appSettings.MenuItems.Add(CreateMenuEntryLeaveShorcutInStartMenu());
            appSettings.MenuItems.Add("-");
            appSettings.MenuItems.Add(CreateMenuEntryReset());
            ContextMenu.MenuItems.Add(appSettings);
            ContextMenu.MenuItems.Add("-");
            ContextMenu.MenuItems.Add(CreateMenuEntryEnablePlayerWatcher());
            ContextMenu.MenuItems.Add("-");
            ContextMenu.MenuItems.Add(CreateMenuEntryRpTracking());
            ContextMenu.MenuItems.AddRange(CreateMenuEntryListTrackablePlayers());
            ContextMenu.MenuItems.Add("-");
            ContextMenu.MenuItems.AddRange(CreateMenuEntryListChannels());
            ContextMenu.MenuItems.Add("-");
            ContextMenu.MenuItems.Add(CreateMenuEntryAbout());
            ContextMenu.MenuItems.Add(CreateMenuEntryExit());
        }

        private MenuItem CreateMenuEntryShowOnNewSong()
        {
            MenuItem showOnNewSong = new MenuItem("Show on new song");
            showOnNewSong.Checked = _config.ExternalConfig.ShowOnNewSong
                    || _config.State.Foobar2000IsPlayingRP
                    || _config.State.RpTrackingConfig.ValidateActivePlayerId();
            showOnNewSong.Enabled = !_config.State.Foobar2000IsPlayingRP
                    || !_config.State.RpTrackingConfig.ValidateActivePlayerId();

            showOnNewSong.Click += (sender, e) =>
            {
                _config.ExternalConfig.ShowOnNewSong = !_config.ExternalConfig.ShowOnNewSong;
                showOnNewSong.Checked = _config.ExternalConfig.ShowOnNewSong;

                if (_config.ExternalConfig.ShowOnNewSong)
                {
                    _toastHandler.ShowSongStartToast();
                }
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
                largeAlbumArt.Checked = !largeAlbumArt.Checked;
                _config.ExternalConfig.LargeAlbumArt = largeAlbumArt.Checked;
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
                showSongRating.Checked = !showSongRating.Checked;
                _config.ExternalConfig.ShowSongRating = showSongRating.Checked;
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
                promptForRating.Checked = !promptForRating.Checked;
                _config.ExternalConfig.PromptForRating = promptForRating.Checked;
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
                leaveShorcutInStartMenu.Checked = !leaveShorcutInStartMenu.Checked;
                _config.ExternalConfig.LeaveShorcutInStartMenu = leaveShorcutInStartMenu.Checked;
            };

            return leaveShorcutInStartMenu;
        }

        private MenuItem CreateMenuEntryReset()
        {
            MenuItem reset = new MenuItem("Delete app data");
            reset.Click += ResetHandler;
            ContextMenu.MenuItems.Add(reset);

            return reset;
        }

        private MenuItem CreateMenuEntryEnablePlayerWatcher()
        {
            MenuItem enablePlayerWatcher = new MenuItem("Track Foobar2000")
            {
                Checked = _config.ExternalConfig.EnablePlayerWatcher
            };

            enablePlayerWatcher.Click += (sender, e) =>
            {
                _config.ExternalConfig.EnablePlayerWatcher = !enablePlayerWatcher.Checked;
                if (_config.ExternalConfig.EnablePlayerWatcher)
                {
                    _config.State.RpTrackingConfig = new RpTrackingConfig();
                    _config.State.RpTrackingConfig.ActivePlayerId = "Foobar2000";
                    _foobar2000Watcher.Start();
                }
                else
                {
                    _config.State.RpTrackingConfig.ActivePlayerId = null;
                    _foobar2000Watcher.Stop();
                }
                BuildContextMenu();
            };

            return enablePlayerWatcher;
        }

        private MenuItem CreateMenuEntryRpTracking()
        {
            MenuItem rpTracking = new MenuItem("RP Tracking")
            {
                Checked = _config.State.RpTrackingConfig.Enabled
            };

            rpTracking.Click += (sender, e) =>
            {
                _config.State.RpTrackingConfig.Enabled = !_config.State.RpTrackingConfig.Enabled;
                if (_config.State.RpTrackingConfig.Enabled)
                {
                    _config.State.RpTrackingConfig.Players = _apihandler.GetSync_v2().Players;
                }
                else
                {
                    _config.State.RpTrackingConfig = new RpTrackingConfig();
                    _config.ExternalConfig.Channel = 0;
                }
                BuildContextMenu();
            };

            return rpTracking;
        }

        private MenuItem[] CreateMenuEntryListTrackablePlayers()
        {
            List<MenuItem> TrackablePlayers = new List<MenuItem>();

            if (_config.State.RpTrackingConfig.Players.Any())
            {
                TrackablePlayers.Add(new MenuItem("-"));

                foreach (var player in _config.State.RpTrackingConfig.Players)
                {
                    MenuItem trackedPlayer = new MenuItem(player.Source)
                    {
                        RadioCheck = true,
                        Checked = _config.State.RpTrackingConfig.ActivePlayerId == player.PlayerId,
                    };

                    trackedPlayer.Click += (sender, e) =>
                    {
                        _config.State.RpTrackingConfig.ActivePlayerId = player.PlayerId;
                        _config.ExternalConfig.Channel = Int32.Parse(player.Chan);
                        _songInfoListener.CheckSong();
                        _config.ExternalConfig.EnablePlayerWatcher = false;
                        BuildContextMenu();
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
                        && (!((_config.ExternalConfig.EnablePlayerWatcher && _config.State.Foobar2000IsPlayingRP)
                        || _config.State.RpTrackingConfig.ValidateActivePlayerId()))
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
                    _config.ExternalConfig.Channel = Int32.Parse(loopChannel.Chan);
                    _songInfoListener.CheckSong();
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
            exit.Click += (sender, e) => { ExitThread(); };

            return exit;
        }

        private void TrayIconDoubleClickHandler(object sender, MouseEventArgs e)
        {
            _log.Information("TrayIconDoubleClickHandler - Invoked - Sender: {Sender}", sender.GetType());
            Task.Run(() => _toastHandler.ShowSongDetailToast());

        }

        private void ConfigChangeHandler(object sender, ConfigChangeEventArgs e)
        {
            _log.Information("ConfigChangeHandler - Invoked - Sender: {Sender} - Channel changed: {ChannelChanged} - Show on new song flag changed: {ShowOnNewSongChanged} - Player state flag changed: {PlayerStateChanged}", sender.GetType(), e.ChannelChanged, e.ShowOnNewSongChanged, e.PlayerStateChanged);
            BuildContextMenu();
            if (e.ChannelChanged)
            {
                _songInfoListener.CheckSong();
            }
            else if ((e.ShowOnNewSongChanged && _config.ExternalConfig.ShowOnNewSong)   // ShowOnChanged turned on
                || (e.PlayerStateChanged && _config.State.Foobar2000IsPlayingRP))     // Playback just started
            {
                _toastHandler.ShowSongStartToast();
            }

            if (_config.ExternalConfig.EnablePlayerWatcher)
            {
                _foobar2000Watcher.Start();
            }
            else
            {
                _foobar2000Watcher.Stop();
            }
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

            if (File.Exists(_config.InternalConfig.AlbumArtImagePath))
            {
                _log.Debug("ApplicationExitHandler - Delete album art");
                File.Delete(_config.InternalConfig.AlbumArtImagePath);
            }

            if (!_config.ExternalConfig.LeaveShorcutInStartMenu)
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

        private void ResetHandler(object sender, EventArgs e)
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
                resetFlag = true;
                Application.Exit();
            }
        }

        private void CheckQueuedDataDeleteRequest()
        {
            if (_config.ExternalConfig.DeleteAllDataOnStartup)
            {
                resetFlag = true;
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
