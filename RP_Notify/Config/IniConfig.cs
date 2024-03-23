using System;
using System.IO;
using System.Linq;

namespace RP_Notify.Config
{
    public class IniConfig : IConfig
    {
        public IStaticConfig StaticConfig { get; set; }
        public IExternalConfig ExternalConfig { get; set; }
        public State State { get; set; }

        public IniConfig()
        {
            StaticConfig = new StaticConfig();
            var isUserAuthenticated = File.Exists(StaticConfig.CookieCachePath);
            ExternalConfig = new ExternalConfig(isUserAuthenticated);
            State = new State
            {
                IsUserAuthenticated = isUserAuthenticated
            };
        }

        public bool IsRpPlayerTrackingChannel()
        {
            bool isActivePlayerIdValid = ExternalConfig.EnableRpOfficialTracking
                && !string.IsNullOrEmpty(State.RpTrackingConfig.ActivePlayerId)
                && State.RpTrackingConfig.Players.Any(p => p.PlayerId == State.RpTrackingConfig.ActivePlayerId);

            if (isActivePlayerIdValid)
            {
                return true;
            }
            else
            {
                State.RpTrackingConfig.ActivePlayerId = null;
                return false;
            }
        }

        public bool IsRpPlayerTrackingChannel(out int channel)
        {
            if (IsRpPlayerTrackingChannel())
            {
                channel = Int32.Parse(
                    State.RpTrackingConfig.Players
                    .Where(p => p.PlayerId == State.RpTrackingConfig.ActivePlayerId)
                    .First()
                    .Chan
                    );

                return true;
            }
            else
            {
                channel = -1;
                return false;
            }
        }
    }
}
