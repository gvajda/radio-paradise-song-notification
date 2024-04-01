namespace RP_Notify.Config
{
    public interface IConfigRoot
    {
        IExternalConfig ExternalConfig { get; set; }

        State State { get; set; }

        StaticConfig StaticConfig { get; set; }

        bool IsUserAuthenticated();

        string GetLoggedInUsername();

        bool IsRpPlayerTrackingChannel();

        bool IsRpPlayerTrackingChannel(out int channel);
    }
}
