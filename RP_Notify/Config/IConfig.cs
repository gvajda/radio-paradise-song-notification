﻿using System;

namespace RP_Notify.Config
{
    public interface IConfig
    {
        IStaticConfig StaticConfig { get; set; }

        IExternalConfig ExternalConfig { get; set; }

        State State { get; set; }
    }

    public interface IStaticConfig
    {
        // Internal values 
        string AlbumArtImagePath { get; }
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
        bool DeleteAllDataOnStartup { get; set; }
        bool EnableLoggingToFile { get; set; }
        bool EnablePlayerWatcher { get; set; }
        bool LargeAlbumArt { get; set; }
        bool LeaveShorcutInStartMenu { get; set; }
        bool PromptForRating { get; set; }
        bool ShowOnNewSong { get; set; }
        bool ShowSongRating { get; set; }

        void DeleteConfigRootFolder();
    }
}
