using RP_Notify.API.ResponseModel;
using RP_Notify.RP_Tracking;
using System;
using System.Collections.Generic;

namespace RP_Notify.Config
{
    public class State
    {
        public RpTrackingConfig RpTrackingConfig { get; set; }



        private DateTime songInfoExpiration;
        public DateTime SongInfoExpiration
        {
            get => DateTime.Compare(DateTime.Now, songInfoExpiration) <= 0      // If expiration timestamp is in the future
                    ? songInfoExpiration
                    : DateTime.Now;
            set => songInfoExpiration = value;
        }
        public List<Channel> ChannelList { get; set; }
        public PlayListSong SongInfo { get; set; }
        public bool IsUserAuthenticated { get; set; }

        public bool ShowOnSonginfoOverride { get; set; }
    }
}
