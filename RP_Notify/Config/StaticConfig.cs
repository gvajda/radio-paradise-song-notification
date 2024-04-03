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
        public bool CleanUpOnExit { get; set; }

        public StaticConfig()
        {
            CleanUpOnExit = false;
            ConfigBaseFolderExisted = ConfigDirectoryHelper.TryFindConfigDirectory(out ConfigLocationOptions configBaseFolder);
            ConfigBaseFolderOption = configBaseFolder;
            configBaseFoldepath = ConfigDirectoryHelper.GetLocalPath(configBaseFolder);

            ConfigFilePath = Path.Combine(configBaseFoldepath, Constants.ConfigFileName);
            CookieCachePath = Path.Combine(configBaseFoldepath, Constants.CookieCacheFileName);
            AlbumArtCacheFolder = Path.Combine(configBaseFoldepath, Constants.AlbumArtCacheFolderName);
            LogFilePath = Path.Combine(configBaseFoldepath, Constants.LogFolderName, Constants.LogFileName);
            RpApiBaseUrl = Constants.RpApiBaseUrl;
            RpImageBaseUrl = Constants.RpImageBaseUrl;
        }
    }
}
