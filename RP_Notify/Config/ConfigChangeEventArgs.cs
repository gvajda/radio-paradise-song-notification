using System;

namespace RP_Notify.Config
{
    public class ConfigChangeEventArgs : EventArgs
    {
        public ConfigChangeEventArgs()
        {
        }
        public bool ChannelChanged { get; set; }
        public bool ShowOnNewSongChanged { get; set; }
        public bool PlayerStateChanged { get; set; }
    }
}
