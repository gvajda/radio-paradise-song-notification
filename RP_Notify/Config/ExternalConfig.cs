using RP_Notify.ErrorHandler;
using RP_Notify.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace RP_Notify.Config
{
    public class ExternalConfig : IExternalConfig
    {
        public event EventHandler<RpEvent> ExternalConfigChangeHandler = delegate { };

        private readonly IniFileHelper _IniFileHelper;
        private readonly FileSystemWatcher _iniFileChangeWatcher;

        private int channel;
        private bool deleteAllData;
        private bool enableLoggingToFile;
        private bool enableFoobar2000Watcher;
        private bool enableMusicBeeWatcher;
        private bool enableRpOfficialTracking;
        private bool largeAlbumArt;
        private bool promptForRating;
        private bool showOnNewSong;
        private bool showSongRating;

        private const string ToastIniSectionName = "Toast";
        private const string AppSettingsIniSectionName = "AppSettings";
        private const string ChannelIniSectionName = "Channel";
        private const string SpecialSettingsIniSectionName = "SpecialSettings";

        public bool ShowOnNewSong
        {
            get => showOnNewSong;
            set
            {
                if (showOnNewSong != value)
                {
                    showOnNewSong = value;
                    RaiseFieldChangeEvent(nameof(ShowOnNewSong), value);
                    SetIniValue(ToastIniSectionName, nameof(ShowOnNewSong), value);
                }
            }
        }
        public bool LargeAlbumArt
        {
            get => largeAlbumArt;
            set
            {
                if (largeAlbumArt != value)
                {
                    largeAlbumArt = value;
                    RaiseFieldChangeEvent(nameof(LargeAlbumArt), value);
                    SetIniValue(ToastIniSectionName, nameof(LargeAlbumArt), value);
                }
            }
        }
        public bool ShowSongRating
        {
            get => showSongRating;
            set
            {
                if (showSongRating != value)
                {
                    showSongRating = value;
                    RaiseFieldChangeEvent(nameof(ShowSongRating), value);
                    SetIniValue(ToastIniSectionName, nameof(ShowSongRating), value);
                }
            }
        }
        public bool PromptForRating
        {
            get => promptForRating;
            set
            {
                if (promptForRating != value)
                {
                    promptForRating = value;
                    RaiseFieldChangeEvent(nameof(PromptForRating), value);
                    SetIniValue(AppSettingsIniSectionName, nameof(PromptForRating), value);
                }
            }
        }

        public bool EnableFoobar2000Watcher
        {
            get => enableFoobar2000Watcher;
            set
            {
                if (enableFoobar2000Watcher != value)
                {
                    enableFoobar2000Watcher = value;
                    RaiseFieldChangeEvent(nameof(EnableFoobar2000Watcher), value);
                    SetIniValue(AppSettingsIniSectionName, nameof(EnableFoobar2000Watcher), value);
                }
            }
        }

        public bool EnableMusicBeeWatcher
        {
            get => enableMusicBeeWatcher;
            set
            {
                if (enableMusicBeeWatcher != value)
                {
                    enableMusicBeeWatcher = value;
                    RaiseFieldChangeEvent(nameof(EnableMusicBeeWatcher), value);
                    SetIniValue(AppSettingsIniSectionName, nameof(EnableMusicBeeWatcher), value);
                }
            }
        }

        public bool EnableRpOfficialTracking
        {
            get => enableRpOfficialTracking;
            set
            {
                if (enableRpOfficialTracking != value)
                {
                    enableRpOfficialTracking = value;
                    RaiseFieldChangeEvent(nameof(EnableRpOfficialTracking), value);
                    SetIniValue(AppSettingsIniSectionName, nameof(EnableRpOfficialTracking), value);
                }
            }
        }
        public bool EnableLoggingToFile
        {
            get => enableLoggingToFile;
            set
            {
                if (enableLoggingToFile != value)
                {
                    enableLoggingToFile = value;
                    RaiseFieldChangeEvent(nameof(EnableLoggingToFile), value);
                    SetIniValue(AppSettingsIniSectionName, nameof(EnableLoggingToFile), value);
                }
            }
        }
        public int Channel
        {
            get => channel;
            set
            {
                if (channel != value)
                {
                    channel = value;
                    RaiseFieldChangeEvent(nameof(Channel), value);
                    SetIniValue(ChannelIniSectionName, nameof(Channel), value);
                }
            }
        }
        public bool DeleteAllData
        {
            get => deleteAllData;
            set
            {
                if (deleteAllData != value)
                {
                    deleteAllData = value;
                    RaiseFieldChangeEvent(nameof(DeleteAllData), value);
                    SetIniValue(SpecialSettingsIniSectionName, nameof(DeleteAllData), value);
                }
            }
        }

        public ExternalConfig(bool isUserAuthenticated)
        {
            _IniFileHelper = new IniFileHelper();
            _iniFileChangeWatcher = new FileSystemWatcher(_IniFileHelper._iniFolder, "config.ini");

            SyncMemoryConfig();
            PromptForRating = isUserAuthenticated
                ? promptForRating
                : false;

            // FileWatcher setup
            StartConfigWatcher();
            Application.ApplicationExit += (s, e) => _iniFileChangeWatcher.Dispose();
        }

        public void DeleteConfigRootFolder()
        {
            Retry.Do(() => Directory.Delete(_IniFileHelper._iniFolder, true));
        }

        private void RaiseFieldChangeEvent(string fieldName, object value)
        {
            ExternalConfigChangeHandler.Invoke(this, new RpEvent(RpEvent.EventType.ConfigChange, fieldName, value));
        }

        private void SetIniValue<T>(string section, string key, T value)
        {
            _iniFileChangeWatcher.EnableRaisingEvents = false;
            var iniFile = _IniFileHelper.ReadIniFile();
            iniFile.Sections[section].Keys[key].TryParseValue(out T sValue);
            if (!EqualityComparer<T>.Default.Equals(value, sValue))
            {
                iniFile.Sections[section].Keys[key].Value = value.ToString();
                iniFile.Save(_IniFileHelper._iniPath);
            }
            _iniFileChangeWatcher.EnableRaisingEvents = true;
        }

        private void StartConfigWatcher()
        {
            _iniFileChangeWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _iniFileChangeWatcher.Changed += HandleExternalConfigChange;
            _iniFileChangeWatcher.EnableRaisingEvents = true;
        }

        private void HandleExternalConfigChange(object source, EventArgs e)
        {
            SyncMemoryConfig();
        }

        private void SyncMemoryConfig()
        {
            _IniFileHelper.CheckIniIntegrity();
            var iniFile = _IniFileHelper.ReadIniFile();

            var _EnableFoobar2000Watcher = EnableFoobar2000Watcher;
            EnableFoobar2000Watcher = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(EnableFoobar2000Watcher)]
                .TryParseValue(out _EnableFoobar2000Watcher)
                ? _EnableFoobar2000Watcher
                : EnableFoobar2000Watcher;

            var _EnableMusicBeeWatcher = EnableMusicBeeWatcher;
            EnableMusicBeeWatcher = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(EnableMusicBeeWatcher)]
                .TryParseValue(out _EnableMusicBeeWatcher)
                ? _EnableMusicBeeWatcher
                : EnableMusicBeeWatcher;

            var _EnableRpOfficialTracking = EnableRpOfficialTracking;
            EnableRpOfficialTracking = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(EnableRpOfficialTracking)]
                .TryParseValue(out _EnableRpOfficialTracking)
                ? _EnableRpOfficialTracking
                : EnableRpOfficialTracking;

            var _EnableLoggingToFile = EnableLoggingToFile;
            EnableLoggingToFile = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(EnableLoggingToFile)]
                .TryParseValue(out _EnableLoggingToFile)
                ? _EnableLoggingToFile
                : EnableLoggingToFile;

            var _PromptForRating = PromptForRating;
            PromptForRating = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(PromptForRating)]
                .TryParseValue(out _PromptForRating)
                ? _PromptForRating
                : PromptForRating;

            var _ShowOnNewSong = ShowOnNewSong;
            ShowOnNewSong = iniFile
                .Sections[ToastIniSectionName]
                .Keys[nameof(ShowOnNewSong)]
                .TryParseValue(out _ShowOnNewSong)
                ? _ShowOnNewSong
                : ShowOnNewSong;

            var _LargeAlbumArt = LargeAlbumArt;
            LargeAlbumArt = iniFile
                .Sections[ToastIniSectionName]
                .Keys[nameof(LargeAlbumArt)]
                .TryParseValue(out _LargeAlbumArt)
                ? _LargeAlbumArt
                : LargeAlbumArt;

            var _ShowSongRating = ShowSongRating;
            ShowSongRating = iniFile
                .Sections[ToastIniSectionName]
                .Keys[nameof(ShowSongRating)]
                .TryParseValue(out _ShowSongRating)
                ? _ShowSongRating
                : ShowSongRating;

            var _Channel = Channel;
            Channel = iniFile
                .Sections[ChannelIniSectionName]
                .Keys[nameof(Channel)]
                .TryParseValue(out _Channel)
                ? _Channel
                : Channel;

            var _DeleteAllDataOnStartup = DeleteAllData;
            DeleteAllData = iniFile
                .Sections[SpecialSettingsIniSectionName]
                .Keys[nameof(DeleteAllData)]
                .TryParseValue(out _DeleteAllDataOnStartup)
                ? _DeleteAllDataOnStartup
                : DeleteAllData;
        }
    }
}
