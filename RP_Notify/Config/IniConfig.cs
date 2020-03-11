using MadMilkman.Ini;
using RP_Notify.ErrorHandler;
using RP_Notify.RP_Tracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RP_Notify.Config
{
    public class IniConfig : IConfig
    {
        public event EventHandler<ConfigChangeEventArgs> ConfigChangedEventHandler;
        private readonly IniHelper _iniHelper;
        private readonly FileSystemWatcher _iniFileChangeWatcher;

        // INI config values
        private bool showOnNewSong;
        private bool largeAlbumArt;
        private bool showSongRating;
        private bool promptForRating;
        private bool leaveShorcutInStartMenu;
        private bool enablePlayerWatcher;
        private bool enableLoggingToFile;
        private int channel;

        public bool ShowOnNewSong
        {
            get => showOnNewSong;
            set
            {
                showOnNewSong = value;
                SetIniValue("Toast", "ShowOnNewSong", value);
            }
        }
        public bool LargeAlbumArt
        {
            get => largeAlbumArt;
            set
            {
                largeAlbumArt = value;
                SetIniValue("Toast", "LargeAlbumArt", value);
            }
        }
        public bool ShowSongRating
        {
            get => showSongRating;
            set
            {
                showSongRating = value;
                SetIniValue("Toast", "ShowSongRating", value);
            }
        }
        public bool PromptForRating
        {
            get => promptForRating;
            set
            {
                promptForRating = value;
                SetIniValue("AppSettings", "PromptForRating", value);
            }
        }
        public bool LeaveShorcutInStartMenu
        {
            get => leaveShorcutInStartMenu;
            set
            {
                leaveShorcutInStartMenu = value;
                SetIniValue("AppSettings", "LeaveShorcutInStartMenu", value);
            }
        }
        public bool EnablePlayerWatcher
        {
            get => enablePlayerWatcher;
            set
            {
                enablePlayerWatcher = value;
                SetIniValue("AppSettings", "EnablePlayerWatcher", value);
            }
        }
        public bool EnableLoggingToFile
        {
            get => enableLoggingToFile;
            set
            {
                enableLoggingToFile = value;
                SetIniValue("AppSettings", "EnableLoggingToFile", value);
            }
        }
        public int Channel
        {
            get => channel;
            set
            {
                channel = value;
                SetIniValue("Channel", "Channel", value);
            }
        }


        // Internal config values
        public string LogFilePath { get; }
        public string CookieCachePath { get; }
        public string AlbumArtImagePath { get; }
        public string IconPath { get; }
        public string ConfigBaseFolder { get; }
        public bool LoggedIn { get; }
        public string RpApiBaseUrl { get; }
        public string RpImageBaseUrl { get; }
        public string ToastAppID { get; }
        public string ToastActivatorCLSID { get; }
        public RpTrackingConfig RpTrackingConfig { get; set; }

        public IniConfig()
        {
            // Set objects
            _iniHelper = new IniHelper();
            _iniFileChangeWatcher = new FileSystemWatcher(_iniHelper._iniFolder, "config.ini");

            // Constants
            LogFilePath = Path.Combine(_iniHelper._iniFolder, "log.txt");
            CookieCachePath = Path.Combine(_iniHelper._iniFolder, "_cookieCache");
            AlbumArtImagePath = Path.Combine(_iniHelper._iniFolder, "albumart.jpg");
            IconPath = Path.Combine(_iniHelper._iniFolder, "rp.ico");
            ConfigBaseFolder = _iniHelper._iniFolder;
            LoggedIn = File.Exists(CookieCachePath);

            ToastAppID = "GergelyVajda.RP_Notify";
            ToastActivatorCLSID = "8a8d7d8c-b191-4b17-b527-82c795243a12";
            RpApiBaseUrl = "https://api.radioparadise.com";
            RpImageBaseUrl = "https://img.radioparadise.com";

            RpTrackingConfig = new RpTrackingConfig();

            // Parse config from Ini
            SyncMemoryConfig();
            PromptForRating = LoggedIn
                ? promptForRating
                : false;

            // FileWatcher setup
            StartConfigWatcher();
            Application.ApplicationExit += (s, e) => _iniFileChangeWatcher.Dispose();
        }

        public void DeletePersistentData()
        {
            Retry.Do(() => Directory.Delete(_iniHelper._iniFolder, true));
        }

        private void StartConfigWatcher()
        {
            _iniFileChangeWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _iniFileChangeWatcher.Changed += HandleExternalConfigChange;
            _iniFileChangeWatcher.EnableRaisingEvents = true;
        }

        private void HandleExternalConfigChange(object source, EventArgs e)
        {
            // var isChannelChanged = IsChannelChnaged();
            var isChannelChanged = IniPropertyChnaged(channel, "Channel", "Channel");
            var isShowOnNewSongChanged = IniPropertyChnaged(showOnNewSong, "Toast", "ShowOnNewSong");
            SyncMemoryConfig();
            ConfigChangedEventHandler?.
                Invoke(this,
                    new ConfigChangeEventArgs()
                    {
                        ChannelChanged = isChannelChanged,
                        ShowOnNewSongChanged = isShowOnNewSongChanged
                    });
        }

        private bool IniPropertyChnaged<T>(T property, string sectionName, string keyName)
        {
            var iniFile = _iniHelper.ReadIniFile();
            iniFile.Sections[sectionName].Keys[keyName].TryParseValue(out T iniValue);
            return !iniValue.Equals(property);
        }

        private void SyncMemoryConfig()
        {
            var iniFile = _iniHelper.ReadIniFile();

            // foreach(var sections in iniFile.Sections)

            iniFile.Sections["AppSettings"].Keys["LeaveShorcutInStartMenu"].TryParseValue(out leaveShorcutInStartMenu);
            iniFile.Sections["AppSettings"].Keys["EnablePlayerWatcher"].TryParseValue(out enablePlayerWatcher);
            iniFile.Sections["AppSettings"].Keys["EnableLoggingToFile"].TryParseValue(out enableLoggingToFile);
            iniFile.Sections["AppSettings"].Keys["PromptForRating"].TryParseValue(out promptForRating);
            iniFile.Sections["Toast"].Keys["ShowOnNewSong"].TryParseValue(out showOnNewSong);
            iniFile.Sections["Toast"].Keys["LargeAlbumArt"].TryParseValue(out largeAlbumArt);
            iniFile.Sections["Toast"].Keys["ShowSongRating"].TryParseValue(out showSongRating);
            iniFile.Sections["Channel"].Keys["Channel"].TryParseValue(out channel);
        }

        private void CheckIniIntegrity()
        {

        }

        void SetIniValue<T>(string section, string key, T value)
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

        private T GetIniValue<T>(string section, string key)
        {
            _iniFileChangeWatcher.EnableRaisingEvents = false;
            var iniFile = _iniHelper.ReadIniFile();
            iniFile.Sections[section].Keys[key].TryParseValue(out T gValue);
            _iniFileChangeWatcher.EnableRaisingEvents = true;
            return gValue;

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
            if (!File.Exists(_iniPath) || (File.Exists(_iniPath) && string.IsNullOrEmpty(File.ReadAllText(_iniPath))))
            {
                InitIni(_iniPath);
            }
        }

        internal IniFile ReadIniFile()
        {
            var iniFile = new IniFile();
            var iniContent = TryReadIniStringContent();
            iniFile.Load(new StringReader(iniContent));
            return iniFile;
        }

        private void InitIni(string pIniPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pIniPath));
            if (!File.Exists(pIniPath))
            {
                File.WriteAllText(pIniPath, Properties.Resources.config);
            }
        }


        private string CheckForCustomIniPath()
        {
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.EndsWith(".ini"))
                {
                    if (File.Exists(arg) && !string.IsNullOrEmpty(File.ReadAllText(_iniPath)))
                    {
                        return arg;
                    }
                    else
                    {
                        try
                        {       // Should run once
                            InitIni(arg);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
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
                if (string.IsNullOrEmpty(iniContent))
                {
                    throw new InvalidDataException("Ini file read error");
                }
                return iniContent;
            });
        }
    }
    public class ConfigChangeEventArgs : EventArgs
    {
        public ConfigChangeEventArgs()
        {
        }
        public bool ChannelChanged { get; set; }
        public bool ShowOnNewSongChanged { get; set; }
        public bool PlayerStateChanged { get; set; }
    }
}
