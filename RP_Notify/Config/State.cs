using RP_Notify.API.ResponseModel;
using System;
using System.Collections.Generic;

namespace RP_Notify.Config
{
    public class State
    {
        public event EventHandler<RpEvent> StateChangeHandler = delegate { };

        private bool isUserAuthenticated;
        private List<Channel> channelList;
        private Playback playback;
        private string tooltipText;
        private bool foobar2000IsPlayingRP;

        public bool IsUserAuthenticated
        {
            get => isUserAuthenticated;
            set
            {
                if (isUserAuthenticated != value)
                {
                    isUserAuthenticated = value;
                    RaiseFieldChangeEvent(nameof(IsUserAuthenticated), value);
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
                    RaiseFieldChangeEvent(nameof(ChannelList));
                }
            }
        }

        public Playback Playback
        {
            get => playback;
            set
            {
                if (playback == null
                    || string.IsNullOrEmpty(playback.SongInfo.SongId)
                    || playback.SongInfo.SongId != value.SongInfo.SongId
                    || playback.SongInfo.Event != value.SongInfo.Event)
                {
                    playback = value;
                    RaiseFieldChangeEvent(nameof(Playback));
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
                    RaiseFieldChangeEvent(nameof(Playback));
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
                    RaiseFieldChangeEvent(nameof(TooltipText));
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
        public RpTrackingConfig RpTrackingConfig { get; set; }

        public State()
        {
            IsUserAuthenticated = false;
            ChannelList = null;
            TooltipText = null;
            Foobar2000IsPlayingRP = false;
            RpTrackingConfig = new RpTrackingConfig();
        }

        private void RaiseFieldChangeEvent(string fieldName, bool? value = null)
        {
            StateChangeHandler.Invoke(this, new RpEvent(RpEvent.EventType.StateChange, fieldName, value));
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

            SongInfo = NowplayingList.Song.TryGetValue("0", out var nowPlayingSong)
                ? nowPlayingSong
                : null;

            SongInfoExpiration = DateTime.Now.Add(TimeSpan.FromSeconds(NowplayingList.Refresh));
            SameSongOnlyInternalUpdate = false;
            ShowedOnNewSong = false;
        }
    }
}
