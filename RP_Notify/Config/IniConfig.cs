using MadMilkman.Ini;
using RP_Notify.API.ResponseModel;
using RP_Notify.ErrorHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RP_Notify.Config
{
    public interface IConfig
    {
        IStaticConfig StaticConfig { get; set; }

        IExternalConfig ExternalConfig { get; set; }

        State State { get; set; }

        bool IsRpPlayerTrackingChannel();

        bool IsRpPlayerTrackingChannel(out int channel);
    }

    public interface IStaticConfig
    {
        // Internal values 
        string ConfigBaseFolder { get; }
        string CookieCachePath { get; }
        string IconPath { get; }
        string LogFilePath { get; }
        string RpApiBaseUrl { get; }
        string RpImageBaseUrl { get; }
        string ToastActivatorCLSID { get; }
        string ToastAppID { get; }
    }

    public interface IExternalConfig
    {
        event EventHandler<RpEvent> ExternalConfigChangeHandler;

        int Channel { get; set; }
        bool DeleteAllData { get; set; }
        bool EnableLoggingToFile { get; set; }
        bool EnableFoobar2000Watcher { get; set; }
        bool EnableMusicBeeWatcher { get; set; }
        bool EnableRpOfficialTracking { get; set; }
        bool LargeAlbumArt { get; set; }
        bool LeaveShorcutInStartMenu { get; set; }
        bool PromptForRating { get; set; }
        bool ShowOnNewSong { get; set; }
        bool ShowSongRating { get; set; }

        void DeleteConfigRootFolder();
    }

    public class IniConfig : IConfig
    {
        public IStaticConfig StaticConfig { get; set; }
        public IExternalConfig ExternalConfig { get; set; }
        public State State { get; set; }

        public IniConfig()
        {
            StaticConfig = new StaticConfig();
            ExternalConfig = new ExternalConfig();
            State = new State
            {
                IsUserAuthenticated = File.Exists(StaticConfig.CookieCachePath)
            };
        }

        public bool IsRpPlayerTrackingChannel()
        {
            bool isActivePlayerIdValid = ExternalConfig.EnableRpOfficialTracking
                && !string.IsNullOrEmpty(State.RpTrackingConfig.ActivePlayerId)
                && State.RpTrackingConfig.Players.Any(p => p.PlayerId == State.RpTrackingConfig.ActivePlayerId);

            if (isActivePlayerIdValid)
            {
                return true;
            }
            else
            {
                State.RpTrackingConfig.ActivePlayerId = null;
                return false;
            }
        }

        public bool IsRpPlayerTrackingChannel(out int channel)
        {
            if (IsRpPlayerTrackingChannel())
            {
                channel = Int32.Parse(
                    State.RpTrackingConfig.Players
                    .Where(p => p.PlayerId == State.RpTrackingConfig.ActivePlayerId)
                    .First()
                    .Chan
                    );

                return true;
            }
            else
            {
                channel = -1;
                return false;
            }
        }
    }

    public class StaticConfig : IStaticConfig
    {
        public string ConfigBaseFolder { get; }
        public string CookieCachePath { get; }
        public string IconPath { get; }
        public string LogFilePath { get; }
        public string RpApiBaseUrl { get; }
        public string RpImageBaseUrl { get; }
        public string ToastActivatorCLSID { get; }
        public string ToastAppID { get; }
        public int SongHistoryLength { get; }

        private readonly IniHelper _iniHelper;

        public StaticConfig()
        {
            _iniHelper = new IniHelper();

            ConfigBaseFolder = _iniHelper._iniFolder;
            CookieCachePath = Path.Combine(_iniHelper._iniFolder, "_cookieCache");
            IconPath = Path.Combine(_iniHelper._iniFolder, "rp.ico");
            LogFilePath = Path.Combine(_iniHelper._iniFolder, ".log");
            RpApiBaseUrl = "https://api.radioparadise.com";
            RpImageBaseUrl = "https://img.radioparadise.com";
            ToastActivatorCLSID = "8a8d7d8c-b191-4b17-b527-82c795243a12";
            ToastAppID = "GergelyVajda.RP_Notify";
            SongHistoryLength = 20;
        }
    }

    public class ExternalConfig : IExternalConfig
    {
        public event EventHandler<RpEvent> ExternalConfigChangeHandler = delegate { };

        private readonly IniHelper _iniHelper;
        private readonly FileSystemWatcher _iniFileChangeWatcher;

        private int channel;
        private bool deleteAllData;
        private bool enableLoggingToFile;
        private bool enableFoobar2000Watcher;
        private bool enableMusicBeeWatcher;
        private bool enableRpOfficialTracking;
        private bool largeAlbumArt;
        private bool leaveShorcutInStartMenu;
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
        public bool LeaveShorcutInStartMenu
        {
            get => leaveShorcutInStartMenu;
            set
            {
                if (leaveShorcutInStartMenu != value)
                {
                    leaveShorcutInStartMenu = value;
                    RaiseFieldChangeEvent(nameof(LeaveShorcutInStartMenu), value);
                    SetIniValue(AppSettingsIniSectionName, nameof(LeaveShorcutInStartMenu), value);
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

        public ExternalConfig()
        {
            _iniHelper = new IniHelper();
            _iniFileChangeWatcher = new FileSystemWatcher(_iniHelper._iniFolder, "config.ini");

            SyncMemoryConfig();
            PromptForRating = File.Exists(Path.Combine(_iniHelper._iniFolder, "_cookieCache"))
                ? promptForRating
                : false;

            // FileWatcher setup
            StartConfigWatcher();
            Application.ApplicationExit += (s, e) => _iniFileChangeWatcher.Dispose();
        }

        public void DeleteConfigRootFolder()
        {
            Retry.Do(() => Directory.Delete(_iniHelper._iniFolder, true));
        }

        private void RaiseFieldChangeEvent(string fieldName, object value)
        {
            ExternalConfigChangeHandler.Invoke(this, new RpEvent(RpEvent.EventType.ConfigChange, fieldName, value));
        }

        private void SetIniValue<T>(string section, string key, T value)
        {
            _iniFileChangeWatcher.EnableRaisingEvents = false;
            var iniFile = _iniHelper.ReadIniFile();
            iniFile.Sections[section].Keys[key].TryParseValue(out T sValue);
            if (!EqualityComparer<T>.Default.Equals(value, sValue))
            {
                iniFile.Sections[section].Keys[key].Value = value.ToString();
                iniFile.Save(_iniHelper._iniPath);
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
            _iniHelper.CheckIniIntegrity();
            var iniFile = _iniHelper.ReadIniFile();


            var _LeaveShorcutInStartMenu = LeaveShorcutInStartMenu;
            LeaveShorcutInStartMenu = iniFile
                .Sections[AppSettingsIniSectionName]
                .Keys[nameof(LeaveShorcutInStartMenu)]
                .TryParseValue(out _LeaveShorcutInStartMenu)
                ? _LeaveShorcutInStartMenu
                : LeaveShorcutInStartMenu;

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
    public class State
    {
        public event EventHandler<RpEvent> StateChangeHandler = delegate { };

        private bool isUserAuthenticated;
        private List<Channel> channelList;
        private List<PlayListSong> songHistory;
        private Playback playback;
        private string tooltipText;
        private bool foobar2000IsPlayingRP;
        private bool musicBeeIsPlayingRP;

        public bool IsUserAuthenticated
        {
            get => isUserAuthenticated;
            set
            {
                if (isUserAuthenticated != value)
                {
                    isUserAuthenticated = value;
                    RaiseFieldChangeEvent(nameof(IsUserAuthenticated), value);
                }
            }
        }

        public List<Channel> ChannelList
        {
            get => channelList;
            set
            {
                if (channelList != value)
                {
                    channelList = value;
                    RaiseFieldChangeEvent(nameof(ChannelList), value);
                }
            }
        }

        public List<PlayListSong> SongHistory
        {
            get => songHistory;
            set
            {
                if (songHistory != value)
                {
                    songHistory = value;
                    RaiseFieldChangeEvent(nameof(SongHistory), value);
                }
            }
        }

        public Playback Playback
        {
            get => playback;
            set
            {
                if (playback == null
                    || string.IsNullOrEmpty(playback.SongInfo.SongId)
                    || playback.SongInfo.SongId != value.SongInfo.SongId
                    || playback.SongInfo.Event != value.SongInfo.Event)
                {
                    playback = value;

                    RaiseFieldChangeEvent(nameof(Playback), value);
                }
                else if (playback.SongInfo.UserRating != value.SongInfo.UserRating
                    ||
                        (   // if RP player was paused and restarted
                            playback.SongInfoExpiration.AddSeconds(10) < value.SongInfoExpiration
                            && DateTime.Now.AddSeconds(10) < value.SongInfoExpiration
                        ))
                {
                    value.SameSongOnlyInternalUpdate = true;
                    playback = value;
                    RaiseFieldChangeEvent(nameof(Playback), value);
                }
            }
        }

        public string TooltipText
        {
            get => tooltipText;
            set
            {
                if (tooltipText != value)
                {
                    tooltipText = value;
                    RaiseFieldChangeEvent(nameof(TooltipText), value);
                }
            }
        }

        public bool Foobar2000IsPlayingRP
        {
            get => foobar2000IsPlayingRP;
            set
            {
                if (foobar2000IsPlayingRP != value)
                {
                    foobar2000IsPlayingRP = value;
                    RaiseFieldChangeEvent(nameof(Foobar2000IsPlayingRP), value);
                }
            }
        }

        public bool MusicBeeIsPlayingRP
        {
            get => musicBeeIsPlayingRP;
            set
            {
                if (musicBeeIsPlayingRP != value)
                {
                    musicBeeIsPlayingRP = value;
                    RaiseFieldChangeEvent(nameof(MusicBeeIsPlayingRP), value);
                }
            }
        }

        public RpTrackingConfig RpTrackingConfig { get; set; }

        public State()
        {
            IsUserAuthenticated = false;
            ChannelList = null;
            TooltipText = null;
            Foobar2000IsPlayingRP = false;
            RpTrackingConfig = new RpTrackingConfig();
        }

        private void RaiseFieldChangeEvent(string fieldName, object value)
        {
            StateChangeHandler.Invoke(this, new RpEvent(RpEvent.EventType.StateChange, fieldName, value));
        }
    }

    public class Playback
    {
        private DateTime songInfoExpiration;

        public NowplayingList NowplayingList { get; }
        public PlayListSong SongInfo { get; }
        public DateTime SongInfoExpiration
        {
            get => DateTime.Compare(DateTime.Now, songInfoExpiration) <= 0      // If expiration timestamp is in the future
                    ? songInfoExpiration
                    : DateTime.Now;
            set => songInfoExpiration = value;
        }
        public bool SameSongOnlyInternalUpdate { get; internal set; }
        public bool ShowedOnNewSong { get; set; }

        public Playback(NowplayingList NowplayingList)
        {
            this.NowplayingList = NowplayingList;

            SongInfo = NowplayingList.Song.TryGetValue("0", out var nowPlayingSong)
                ? nowPlayingSong
                : null;

            SongInfoExpiration = DateTime.Now.Add(TimeSpan.FromSeconds(NowplayingList.Refresh));
            SameSongOnlyInternalUpdate = false;
            ShowedOnNewSong = false;
        }
    }


    internal class IniHelper
    {

        internal readonly string _iniPath;
        internal readonly string _iniFolder;

        internal IniHelper()
        {
            _iniPath = CheckForCustomIniPath() ?? Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData), @"RP_Notify\config.ini");
            _iniFolder = Path.GetDirectoryName(_iniPath);

            CreateIniWithDefaultValuesIfNotExists(_iniPath);
            CheckIniIntegrity();
        }

        internal IniFile ReadIniFile()
        {
            var iniFile = new IniFile();
            var iniContent = TryReadIniStringContent();
            iniFile.Load(new StringReader(iniContent));
            return iniFile;
        }

        internal void CheckIniIntegrity()
        {
            var defaultIniFile = new IniFile();
            defaultIniFile.Load(new StringReader(Properties.Resources.config));

            var iniFile = ReadIniFile();

            foreach (var defaultSection in defaultIniFile.Sections)
            {
                if (!iniFile.Sections.Where(s => s.Name == defaultSection.Name).Any())
                {
                    iniFile.Sections.Add(defaultSection.Name);
                }

                foreach (var defaultKey in defaultSection.Keys)
                {
                    if (!iniFile
                        .Sections[defaultSection.Name]
                        .Keys.Where(k => k.Name == defaultKey.Name)
                        .Any()
                        )
                    {
                        iniFile.Sections[defaultSection.Name].Keys.Add(defaultKey.Name, defaultKey.Value);
                    }
                }
            }

            iniFile.Save(_iniPath);
        }

        private void CreateIniWithDefaultValuesIfNotExists(string pIniPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pIniPath));
            if (!File.Exists(pIniPath))
            {
                File.WriteAllText(pIniPath, Properties.Resources.config);
            }
        }

        private string CheckForCustomIniPath()
        {
            foreach (string arg in Environment.GetCommandLineArgs().Where(arg => arg.EndsWith(".ini")))
            {
                if (File.Exists(arg) && !string.IsNullOrEmpty(File.ReadAllText(arg)))
                {
                    return arg;
                }

                try
                {   // Should run once
                    CreateIniWithDefaultValuesIfNotExists(arg);
                    return arg;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return null;
        }

        private string TryReadIniStringContent()
        {
            return Retry.Do(() =>
            {
                string iniContent = null;
                iniContent = File.ReadAllText(_iniPath, Encoding.Default);

                return iniContent;
            });
        }
    }
}
