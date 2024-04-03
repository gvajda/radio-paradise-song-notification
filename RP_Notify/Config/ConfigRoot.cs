using System;
using System.Linq;

namespace RP_Notify.Config
{
    public class ConfigRoot : IConfigRoot
    {
        public StaticConfig StaticConfig { get; set; }
        public IExternalConfig ExternalConfig { get; set; }
        public State State { get; set; }

        public ConfigRoot()
        {
            StaticConfig = new StaticConfig();
            ExternalConfig = new ExternalConfigIni(StaticConfig.ConfigFilePath);
            State = new State(StaticConfig.CookieCachePath, new Uri(StaticConfig.RpApiBaseUrl));
        }

        public bool IsUserAuthenticated()
        {
            return State.RpCookieContainer != null
                && State.RpCookieContainer.GetCookies(new Uri(StaticConfig.RpApiBaseUrl)).Count >= 2
                && !State.RpCookieContainer.GetCookies(new Uri(StaticConfig.RpApiBaseUrl))[0].Expired;
        }

        public string GetLoggedInUsername()
        {
            if (!IsUserAuthenticated())
            {
                return null;
            }

            var cookieCollection = State.RpCookieContainer.GetCookies(new Uri(StaticConfig.RpApiBaseUrl));

            var usernameCookieValue = cookieCollection["C_username"].Value;

            return usernameCookieValue;
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
