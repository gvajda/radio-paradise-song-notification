namespace RP_Notify.SongInfoUpdater
{
    public interface ISongInfoListener
    {
        void Start();

        void ResetListenerLoop();

        void CheckTrackedRpPlayerStatus();
    }
}
