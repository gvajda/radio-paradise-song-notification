using System;
using System.Threading;

namespace RP_Notify.SongInfoUpdater
{
    public interface ISongInfoListener
    {
        CancellationTokenSource nextSongWaiterCancellationTokenSource { get; }

        event EventHandler<TooltipChangeEventArgs> TooltipUpdateChangedEventHandler;

        void Run();
    }
}
