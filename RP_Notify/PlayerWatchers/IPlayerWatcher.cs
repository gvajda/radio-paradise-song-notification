namespace RP_Notify.PlayerWatchers
{
    public interface IPlayerWatcher
    {
        RegisteredPlayer PlayerWatcherType { get; }
        void Start();
        void Stop();
        bool CheckPlayerState(out bool channelChanged);
    }
}
