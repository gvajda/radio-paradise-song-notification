namespace RP_Notify.PlayerWatchers
{
    internal interface IPlayerWatcherProvider
    {
        IPlayerWatcher GetWatcher(RegisteredPlayer registeredPlayer);
    }
}