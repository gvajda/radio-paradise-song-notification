using RP_Notify.RP_Tracking;
using System;

namespace RP_Notify.Config
{
    public interface IConfig
    {
        event EventHandler<ConfigChangeEventArgs> ConfigChangedEventHandler;

        // Internal values 
        string LogFilePath { get; }
        string RpApiBaseUrl { get; }
        string RpImageBaseUrl { get; }
        string ToastAppID { get; }
        string ToastActivatorCLSID { get; }
        string CookieCachePath { get; }
        string AlbumArtImagePath { get; }
        string IconPath { get; }
        string ConfigBaseFolder { get; }
        bool LoggedIn { get; }
        RpTrackingConfig RpTrackingConfig { get; set; }

        // INI Values
        bool ShowOnNewSong { get; set; }
        bool EnablePlayerWatcher { get; set; }
        bool EnableLoggingToFile { get; set; }
        bool LargeAlbumArt { get; set; }
        bool ShowSongRating { get; set; }
        bool PromptForRating { get; set; }
        bool LeaveShorcutInStartMenu { get; set; }
        int Channel { get; set; }
        bool DeleteAllDataOnStartup { get; set; }

        void DeleteConfigRootFolder();
    }

    public interface IInternalConfig
    {
        // Internal values 
        string LogFilePath { get; }
        string RpApiBaseUrl { get; }
        string RpImageBaseUrl { get; }
        string ToastAppID { get; }
        string ToastActivatorCLSID { get; }
        string CookieCachePath { get; }
        string AlbumArtImagePath { get; }
        string IconPath { get; }
        string ConfigBaseFolder { get; }
        bool LoggedIn { get; }
    }

    public interface IExternalConfig
    {
        bool ShowOnNewSong { get; set; }
        bool EnablePlayerWatcher { get; set; }
        bool EnableLoggingToFile { get; set; }
        bool LargeAlbumArt { get; set; }
        bool ShowSongRating { get; set; }
        bool PromptForRating { get; set; }
        bool LeaveShorcutInStartMenu { get; set; }
        int Channel { get; set; }
        bool DeleteAllDataOnStartup { get; set; }
    }


}
