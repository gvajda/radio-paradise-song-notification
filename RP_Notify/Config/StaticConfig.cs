using RP_Notify.Helpers;
using System.IO;

namespace RP_Notify.Config
{
    public class StaticConfig : IStaticConfig
    {
        public string ConfigBaseFolder { get; }
        public string CookieCachePath { get; }
        public string LogFilePath { get; }
        public string RpApiBaseUrl { get; }
        public string RpImageBaseUrl { get; }

        private readonly IniFileHelper _IniFileHelper;

        public StaticConfig()
        {
            _IniFileHelper = new IniFileHelper();

            ConfigBaseFolder = _IniFileHelper._iniFolder;
            CookieCachePath = Path.Combine(_IniFileHelper._iniFolder, "_cookieCache");
            LogFilePath = Path.Combine(_IniFileHelper._iniFolder, "ApplicationLogs", "rpnotify.log");
            RpApiBaseUrl = "https://api.radioparadise.com";
            RpImageBaseUrl = "https://img.radioparadise.com";
        }
    }
}
