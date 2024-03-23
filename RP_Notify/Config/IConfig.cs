using System;

namespace RP_Notify.Config
{
    public interface IConfig
    {
        IStaticConfig StaticConfig { get; set; }

        IExternalConfig ExternalConfig { get; set; }

        State State { get; set; }

        bool IsRpPlayerTrackingChannel();

        bool IsRpPlayerTrackingChannel(out int channel);
    }
}
