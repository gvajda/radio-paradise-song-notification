namespace RP_Notify
{
    internal static class Constants
    {
        public const string ConfigBaseFolder = "RP_Notify_Data";
        public const string ObsoleteConfigBaseFolder = "RP_Notify_Cache";
        public const string ConfigFileName = "rp_config.ini";
        public const string CookieCacheFileName = "rp_cookieContainer";
        public const string AlbumArtCacheFolderName = "AlbumArtCache";
        public const string LogFolderName = "ApplicationLogs";
        public const string LogFileName = "rpnotify.log";
        public const string RpApiBaseUrl = "https://api.radioparadise.com";
        public const string RpImageBaseUrl = "https://img.radioparadise.com";

        public const string UserRatingFieldKey = "UserRatingValue";

        public const int RpApiClientHttpRetryAttempts = 4;
    }
}
