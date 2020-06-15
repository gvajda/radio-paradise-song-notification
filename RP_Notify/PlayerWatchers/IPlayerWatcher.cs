namespace RP_Notify.PlayerWatcher
{
    public interface IPlayerWatcher
    {
        void Start();
        void Stop();
        bool CheckPlayerState(out bool channelChanged);
    }
}
