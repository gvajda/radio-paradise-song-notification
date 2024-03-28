using System;

namespace RP_Notify.Config
{
    public interface IExternalConfig
    {
        int Channel { get; set; }
        bool DeleteAllData { get; set; }
        bool EnableFoobar2000Watcher { get; set; }
        bool EnableLoggingToFile { get; set; }
        bool EnableMusicBeeWatcher { get; set; }
        bool EnableRpOfficialTracking { get; set; }
        bool LargeAlbumArt { get; set; }
        bool ChannelBannerOnDetail { get; set; }
        bool PromptForRating { get; set; }
        bool ShowOnNewSong { get; set; }
        bool ShowSongRating { get; set; }

        event EventHandler<RpEvent> ExternalConfigChangeHandler;
    }
}