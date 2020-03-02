using RP_Notify.Config;
using System;
using System.Threading;

namespace RP_Notify.SongInfoUpdater
{
    public interface ISongInfoListener
    {
        CancellationTokenSource nextSongWaiterCancellationTokenSource { get; }

        event EventHandler<TooltipChangeEventArgs> TooltipUpdateChangedEventHandler;
        event EventHandler<ConfigChangeEventArgs> ConfigChangedEventHandler;

        void Run();
    }
}
