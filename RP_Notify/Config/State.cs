using RP_Notify.Helpers;
using RP_Notify.RpApi.ResponseModel;
using System;
using System.Collections.Generic;
using System.Net;

namespace RP_Notify.Config
{
    public class State
    {
        private readonly string _cookieFilePath;

        public event EventHandler<RpConfigurationChangeEvent> StateChangeHandler = delegate { };

        private CookieContainer rpCookieContainer;
        private List<Channel> channelList;
        private Playback playback;
        private string tooltipText;
        private bool foobar2000IsPlayingRP;
        private bool musicBeeIsPlayingRP;



        public CookieContainer RpCookieContainer
        {
            get => rpCookieContainer;
            set
            {
                if (rpCookieContainer != value)
                {
                    rpCookieContainer = value;
                    CookieHelper.TryWriteCookieToDisk(_cookieFilePath, value);
                    RaiseFieldChangeEvent(nameof(RpCookieContainer), value);
                }
            }
        }

        public List<Channel> ChannelList
        {
            get => channelList;
            set
            {
                if (channelList != value)
                {
                    channelList = value;
                    RaiseFieldChangeEvent(nameof(ChannelList), value);
                }
            }
        }

        public Playback Playback
        {
            get => playback;
            set
            {
                if (playback == null
                    || playback.SongInfo == null
                    || string.IsNullOrEmpty(playback.SongInfo.SongId)
                    || playback.SongInfo.SongId != value.SongInfo.SongId
                    || playback.SongInfo.Event != value.SongInfo.Event)
                {
                    playback = value;
                    RaiseFieldChangeEvent(nameof(Playback), value);
                }
                else if (playback.SongInfo.UserRating != value.SongInfo.UserRating
                    ||
                        (   // if RP player was paused and restarted
                            playback.SongInfoExpiration.AddSeconds(10) < value.SongInfoExpiration
                            && DateTime.Now.AddSeconds(10) < value.SongInfoExpiration
                        ))
                {
                    value.SameSongOnlyInternalUpdate = true;
                    playback = value;
                    RaiseFieldChangeEvent(nameof(Playback), value);
                }
            }
        }

        public string TooltipText
        {
            get => tooltipText;
            set
            {
                if (tooltipText != value)
                {
                    tooltipText = value;
                    RaiseFieldChangeEvent(nameof(TooltipText), value);
                }
            }
        }

        public bool Foobar2000IsPlayingRP
        {
            get => foobar2000IsPlayingRP;
            set
            {
                if (foobar2000IsPlayingRP != value)
                {
                    foobar2000IsPlayingRP = value;
                    RaiseFieldChangeEvent(nameof(Foobar2000IsPlayingRP), value);
                }
            }
        }

        public bool MusicBeeIsPlayingRP
        {
            get => musicBeeIsPlayingRP;
            set
            {
                if (musicBeeIsPlayingRP != value)
                {
                    musicBeeIsPlayingRP = value;
                    RaiseFieldChangeEvent(nameof(MusicBeeIsPlayingRP), value);
                }
            }
        }

        public RpTrackingConfig RpTrackingConfig { get; set; }

        public State(string cookieFilePath)
        {
            _cookieFilePath = cookieFilePath;
            CookieHelper.TryGetCookieFromCache(cookieFilePath, out rpCookieContainer);
            channelList = null;
            tooltipText = null;
            foobar2000IsPlayingRP = false;
            RpTrackingConfig = new RpTrackingConfig();
        }

        private void RaiseFieldChangeEvent(string fieldName, object value)
        {
            StateChangeHandler.Invoke(this, new RpConfigurationChangeEvent(RpConfigurationChangeEvent.EventType.StateChange, fieldName, value));
        }
    }

    public class Playback
    {
        public NowplayingList NowplayingList { get; }
        public PlayListSong SongInfo { get; }
        private DateTime songInfoExpiration;
        public DateTime SongInfoExpiration
        {
            get => DateTime.Compare(DateTime.Now, songInfoExpiration) <= 0      // If expiration timestamp is in the future
                    ? songInfoExpiration
                    : DateTime.Now;
            set => songInfoExpiration = value;
        }
        public bool SameSongOnlyInternalUpdate { get; internal set; }
        public bool ShowedOnNewSong { get; set; }

        public Playback(NowplayingList NowplayingList)
        {
            this.NowplayingList = NowplayingList;

            if (NowplayingList.Song != null && NowplayingList.Song.TryGetValue("0", out var nowPlayingSong))
            {
                SongInfo = nowPlayingSong;
                SongInfoExpiration = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(nowPlayingSong.SchedTime + "000") + long.Parse(nowPlayingSong.Duration)).LocalDateTime;
            }
            else
            {
                SongInfo = null;
                SongInfoExpiration = DateTime.Now;
            }

            SameSongOnlyInternalUpdate = false;
            ShowedOnNewSong = false;
        }
    }
}
