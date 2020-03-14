using RP_Notify.API.ResponseModel;
using System;
using System.Collections.Generic;

namespace RP_Notify.Config
{
    public class State
    {
        public RpTrackingConfig RpTrackingConfig { get; set; }

        public Playback Playback { get; set; }

        public List<Channel> ChannelList { get; set; }

        public bool IsUserAuthenticated { get; set; }

        public bool Foobar2000IsPlayingRP { get; set; }

        public State()
        {
            RpTrackingConfig = new RpTrackingConfig();
            Playback = null;
            ChannelList = null;
            IsUserAuthenticated = false;
            Foobar2000IsPlayingRP = false;
        }

        public bool TryUpdatePlayback(Playback newPlayback)
        {
            if (Playback == null
                || string.IsNullOrEmpty(Playback.SongInfo.SongId)
                || Playback.SongInfo.SongId != newPlayback.SongInfo.SongId
                || Playback.SongInfo.Event != newPlayback.SongInfo.Event)
            {
                Playback = newPlayback;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class Playback
    {
        public NowplayingList NowplayingList { get; set; }

        public PlayListSong SongInfo { get; }

        private DateTime songInfoExpiration;

        public DateTime SongInfoExpiration
        {
            get => DateTime.Compare(DateTime.Now, songInfoExpiration) <= 0      // If expiration timestamp is in the future
                    ? songInfoExpiration
                    : DateTime.Now;
            set => songInfoExpiration = value;
        }

        public Playback(NowplayingList NowplayingList)
        {
            this.NowplayingList = NowplayingList;

            SongInfo = NowplayingList.Song.TryGetValue("0", out var nowPlayingSong)
                ? nowPlayingSong
                : null;

            SongInfoExpiration = DateTime.Now.Add(TimeSpan.FromSeconds(NowplayingList.Refresh));
        }
    }
}
