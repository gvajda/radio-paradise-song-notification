using RP_Notify.Config;
using System;

namespace RP_Notify.SongInfoUpdater
{
    public interface ISongInfoListener
    {
        event EventHandler<TooltipChangeEventArgs> TooltipUpdateChangedEventHandler;
        event EventHandler<ConfigChangeEventArgs> ConfigChangedEventHandler;

        void Run();

        void CheckSong();
    }
}
