using System;

namespace RP_Notify.Config
{
    public interface IPersistedConfig
    {
        int Channel { get; set; }
        bool DeleteAllData { get; set; }
        bool EnableFoobar2000Watcher { get; set; }
        bool EnableLoggingToFile { get; set; }
        bool EnableMusicBeeWatcher { get; set; }
        bool EnableRpOfficialTracking { get; set; }
        bool LargeAlbumArt { get; set; }
        bool RpBannerOnDetail { get; set; }
        bool PromptForRating { get; set; }
        bool ShowOnNewSong { get; set; }
        bool ShowSongRating { get; set; }

        event EventHandler<RpConfigurationChangeEvent> ExternalConfigChangeHandler;
    }
}