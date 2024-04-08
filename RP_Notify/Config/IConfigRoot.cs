namespace RP_Notify.Config
{
    public interface IConfigRoot
    {
        IPersistedConfig PersistedConfig { get; set; }

        State State { get; set; }

        StaticContext StaticConfig { get; set; }

        bool IsUserAuthenticated(out string loggedInUsername);
    }
}
