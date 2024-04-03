using RP_Notify.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace RP_Notify.Config
{
    public class ExternalConfigIni : IExternalConfig
    {
        public event EventHandler<RpConfigurationChangeEvent> ExternalConfigChangeHandler = delegate { };

        private readonly FileSystemWatcher _iniFileChangeWatcher = new FileSystemWatcher();
        private readonly string _iniFilePath;

        private int channel;
        private bool deleteAllData;
        private bool enableLoggingToFile;
        private bool enableFoobar2000Watcher;
        private bool enableMusicBeeWatcher;
        private bool enableRpOfficialTracking;
        private bool largeAlbumArt;
        private bool rpBannerOnDetail;
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
        public bool RpBannerOnDetail
        {
            get => rpBannerOnDetail;
            set
            {
                if (rpBannerOnDetail != value)
                {
                    rpBannerOnDetail = value;
                    RaiseFieldChangeEvent(nameof(RpBannerOnDetail), value);
                    SetIniValue(ToastIniSectionName, nameof(RpBannerOnDetail), value);
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

        public ExternalConfigIni(string iniFilePath)
        {
            _iniFilePath = iniFilePath;
            IniFileHelper.EnsureValidIniFileExists(iniFilePath);

            SetupAndStartConfigWatcher();
            SyncMemoryConfig();
        }

        private void RaiseFieldChangeEvent(string fieldName, object value)
        {
            ExternalConfigChangeHandler.Invoke(this, new RpConfigurationChangeEvent(RpConfigurationChangeEvent.EventType.ConfigChange, fieldName, value));
        }

        private void SetIniValue<T>(string section, string key, T value)
        {
            _iniFileChangeWatcher.EnableRaisingEvents = false;
            var iniFile = IniFileHelper.ReadIniFile(_iniFilePath);
            iniFile.Sections[section].Keys[key].TryParseValue(out T sValue);
            if (!EqualityComparer<T>.Default.Equals(value, sValue))
            {
                iniFile.Sections[section].Keys[key].Value = value.ToString();
                iniFile.Save(_iniFilePath);
            }
            _iniFileChangeWatcher.EnableRaisingEvents = true;
        }

        private void SetupAndStartConfigWatcher()
        {
            _iniFileChangeWatcher.Path = Path.GetDirectoryName(_iniFilePath);
            _iniFileChangeWatcher.Filter = Path.GetFileName(_iniFilePath);
            _iniFileChangeWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _iniFileChangeWatcher.Changed += HandleExternalConfigChange;
            _iniFileChangeWatcher.EnableRaisingEvents = true;
            Application.ApplicationExit += (s, e) => _iniFileChangeWatcher.Dispose();
        }

        private void HandleExternalConfigChange(object source, EventArgs e)
        {
            SyncMemoryConfig();
        }

        private void SyncMemoryConfig()
        {
            var iniFile = IniFileHelper.ReadIniFile(_iniFilePath);

            EnableFoobar2000Watcher = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(EnableFoobar2000Watcher)]
                .TryParseValue(out bool _EnableFoobar2000Watcher)
                ? _EnableFoobar2000Watcher
                : EnableFoobar2000Watcher;

            EnableMusicBeeWatcher = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(EnableMusicBeeWatcher)]
                .TryParseValue(out bool _EnableMusicBeeWatcher)
                ? _EnableMusicBeeWatcher
                : EnableMusicBeeWatcher;

            EnableRpOfficialTracking = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(EnableRpOfficialTracking)]
                .TryParseValue(out bool _EnableRpOfficialTracking)
                ? _EnableRpOfficialTracking
                : EnableRpOfficialTracking;

            EnableLoggingToFile = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(EnableLoggingToFile)]
                .TryParseValue(out bool _EnableLoggingToFile)
                ? _EnableLoggingToFile
                : EnableLoggingToFile;

            PromptForRating = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(PromptForRating)]
                .TryParseValue(out bool _PromptForRating)
                ? _PromptForRating
                : PromptForRating;

            ShowOnNewSong = iniFile
                .Sections[ToastIniSectionName]
                .Keys[nameof(ShowOnNewSong)]
                .TryParseValue(out bool _ShowOnNewSong)
                ? _ShowOnNewSong
                : ShowOnNewSong;

            LargeAlbumArt = iniFile
                .Sections[ToastIniSectionName]
                .Keys[nameof(LargeAlbumArt)]
                .TryParseValue(out bool _LargeAlbumArt)
                ? _LargeAlbumArt
                : LargeAlbumArt;

            RpBannerOnDetail = iniFile
                .Sections[ToastIniSectionName]
                .Keys[nameof(RpBannerOnDetail)]
                .TryParseValue(out bool _RpBannerOnDetail)
                ? _RpBannerOnDetail
                : RpBannerOnDetail;

            ShowSongRating = iniFile
                .Sections[ToastIniSectionName]
                .Keys[nameof(ShowSongRating)]
                .TryParseValue(out bool _ShowSongRating)
                ? _ShowSongRating
                : ShowSongRating;

            Channel = iniFile
                .Sections[ChannelIniSectionName]
                .Keys[nameof(Channel)]
                .TryParseValue(out int _Channel)
                ? _Channel
                : Channel;

            DeleteAllData = iniFile
                .Sections[SpecialSettingsIniSectionName]
                .Keys[nameof(DeleteAllData)]
                .TryParseValue(out bool _DeleteAllDataOnStartup)
                ? _DeleteAllDataOnStartup
                : DeleteAllData;
        }
    }
}
