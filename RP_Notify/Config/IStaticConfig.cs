namespace RP_Notify.Config
{
    public interface IStaticConfig
    {
        string ConfigBaseFolder { get; }
        string CookieCachePath { get; }
        string LogFilePath { get; }
        string RpApiBaseUrl { get; }
        string RpImageBaseUrl { get; }
    }
}