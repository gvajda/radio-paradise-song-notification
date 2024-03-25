using RP_Notify.Helpers;
using System.IO;

namespace RP_Notify.Config
{
    public class StaticConfig
    {
        private readonly string configBaseFoldepath;
        public ConfigLocationOptions ConfigBaseFolderOption { get; }
        public bool ConfigBaseFolderExisted { get; }
        public string ConfigFilePath { get; }
        public string CookieCachePath { get; }
        public string AlbumArtCacheFolder { get; }
        public string LogFilePath { get; }
        public string RpApiBaseUrl { get; }
        public string RpImageBaseUrl { get; }

        public StaticConfig()
        {

            ConfigBaseFolderExisted = ConfigDirectoryHelper.TryFindConfigDirectory(out ConfigLocationOptions configBaseFolder);
            ConfigBaseFolderOption = configBaseFolder;
            configBaseFoldepath = ConfigDirectoryHelper.GetLocalPath(configBaseFolder);

            ConfigFilePath = Path.Combine(configBaseFoldepath, "rp_config.ini");
            CookieCachePath = Path.Combine(configBaseFoldepath, "rp_cookieCache");
            AlbumArtCacheFolder = Path.Combine(configBaseFoldepath, "AlbumArtCache");
            LogFilePath = Path.Combine(configBaseFoldepath, "ApplicationLogs", "rpnotify.log");
            RpApiBaseUrl = "https://api.radioparadise.com";
            RpImageBaseUrl = "https://img.radioparadise.com";
        }
    }
}
