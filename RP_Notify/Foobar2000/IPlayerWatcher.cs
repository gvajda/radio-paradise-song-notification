using RP_Notify.Config;
using System;
using System.Threading;

namespace RP_Notify.Foobar2000Watcher
{
    public interface IPlayerWatcher
    {
        event EventHandler<ConfigChangeEventArgs> ConfigChangedEventHandler;
        CancellationTokenSource PlayerWatcherCancellationTokenSource { get; }
        bool PlayerIsActive { get; }
        void Start();
        void Stop();
    }
}
