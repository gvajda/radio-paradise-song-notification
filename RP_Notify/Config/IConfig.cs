using System;

namespace RP_Notify.Config
{
    public interface IConfig
    {
        event EventHandler<ConfigChangeEventArgs> ConfigChangedEventHandler;

        // Internal valuespublic 
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

        // INI Values
        bool ShowOnNewSong { get; set; }
        bool EnablePlayerWatcher { get; set; }
        bool EnableLoggingToFile { get; set; }
        bool LargeAlbumArt { get; set; }
        bool ShowSongRating { get; set; }
        bool LeaveShorcutInStartMenu { get; set; }
        int Channel { get; set; }

        void DeletePersistentData();
    }
}
