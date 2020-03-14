using MadMilkman.Ini;
using RP_Notify.ErrorHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RP_Notify.Config
{
    public class IniConfig : IConfig
    {
        public IInternalConfig InternalConfig { get; set; }
        public IExternalConfig ExternalConfig { get; set; }
        public State State { get; set; }

        public IniConfig()
        {
            InternalConfig = new InternalConfig();
            ExternalConfig = new ExternalConfig();
            State = new State
            {
                IsUserAuthenticated = File.Exists(InternalConfig.CookieCachePath)
            };
        }
    }

    public class InternalConfig : IInternalConfig
    {
        public string AlbumArtImagePath { get; }
        public string ConfigBaseFolder { get; }
        public string CookieCachePath { get; }
        public string IconPath { get; }
        public string LogFilePath { get; }
        public string RpApiBaseUrl { get; }
        public string RpImageBaseUrl { get; }
        public string ToastActivatorCLSID { get; }
        public string ToastAppID { get; }

        private readonly IniHelper _iniHelper;

        public InternalConfig()
        {
            _iniHelper = new IniHelper();

            AlbumArtImagePath = Path.Combine(_iniHelper._iniFolder, "albumart.jpg");
            ConfigBaseFolder = _iniHelper._iniFolder;
            CookieCachePath = Path.Combine(_iniHelper._iniFolder, "_cookieCache");
            IconPath = Path.Combine(_iniHelper._iniFolder, "rp.ico");
            LogFilePath = Path.Combine(_iniHelper._iniFolder, "log.txt");
            RpApiBaseUrl = "https://api.radioparadise.com";
            RpImageBaseUrl = "https://img.radioparadise.com";
            ToastActivatorCLSID = "8a8d7d8c-b191-4b17-b527-82c795243a12";
            ToastAppID = "GergelyVajda.RP_Notify";
        }
    }

    public class ExternalConfig : IExternalConfig
    {
        private readonly IniHelper _iniHelper;
        private readonly FileSystemWatcher _iniFileChangeWatcher;

        public event EventHandler<ConfigChangeEventArgs> ConfigChangedEventHandler;
        private int channel;
        private bool deleteAllDataOnStartup;
        private bool enableLoggingToFile;
        private bool enablePlayerWatcher;
        private bool largeAlbumArt;
        private bool leaveShorcutInStartMenu;
        private bool promptForRating;
        private bool showOnNewSong;
        private bool showSongRating;

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
        public bool DeleteAllDataOnStartup
        {
            get => deleteAllDataOnStartup;
            set
            {
                deleteAllDataOnStartup = value;
                SetIniValue("OnStartupSettings", "DeleteAllDataOnStartup", value);
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

            iniFile.Sections["AppSettings"].Keys["LeaveShorcutInStartMenu"].TryParseValue(out leaveShorcutInStartMenu);
            iniFile.Sections["AppSettings"].Keys["EnablePlayerWatcher"].TryParseValue(out enablePlayerWatcher);
            iniFile.Sections["AppSettings"].Keys["EnableLoggingToFile"].TryParseValue(out enableLoggingToFile);
            iniFile.Sections["AppSettings"].Keys["PromptForRating"].TryParseValue(out promptForRating);
            iniFile.Sections["Toast"].Keys["ShowOnNewSong"].TryParseValue(out showOnNewSong);
            iniFile.Sections["Toast"].Keys["LargeAlbumArt"].TryParseValue(out largeAlbumArt);
            iniFile.Sections["Toast"].Keys["ShowSongRating"].TryParseValue(out showSongRating);
            iniFile.Sections["Channel"].Keys["Channel"].TryParseValue(out channel);
            iniFile.Sections["OnStartupSettings"].Keys["DeleteAllDataOnStartup"].TryParseValue(out deleteAllDataOnStartup);
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
            if (!File.Exists(_iniPath))
            {
                InitIni(_iniPath);
            }
            else
            {
                CheckIniIntegrity();
            }
        }

        internal IniFile ReadIniFile()
        {
            var iniFile = new IniFile();
            var iniContent = TryReadIniStringContent();
            iniFile.Load(new StringReader(iniContent));
            return iniFile;
        }

        private void CheckIniIntegrity()
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
            foreach (string arg in Environment.GetCommandLineArgs().Where(arg => arg.EndsWith(".ini")))
            {
                if (File.Exists(arg) && !string.IsNullOrEmpty(File.ReadAllText(arg)))
                {
                    return arg;
                }

                try
                {   // Should run once
                    InitIni(arg);
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
