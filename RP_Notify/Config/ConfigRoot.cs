using System;

namespace RP_Notify.Config
{
    public class ConfigRoot : IConfigRoot
    {
        public StaticContext StaticConfig { get; set; }
        public IPersistedConfig PersistedConfig { get; set; }
        public State State { get; set; }

        public ConfigRoot()
        {
            StaticConfig = new StaticContext();
            PersistedConfig = new PersistedConfigIni(StaticConfig.ConfigFilePath);
            State = new State(StaticConfig.CookieCachePath, new Uri(StaticConfig.RpApiBaseUrl));
        }

        public bool IsUserAuthenticated(out string loggedInUsername)
        {
            loggedInUsername = null;

            if (State.RpCookieContainer != null)
            {
                var cookiCollection = State.RpCookieContainer.GetCookies(new Uri(StaticConfig.RpApiBaseUrl));

                if (cookiCollection.Count >= 2 && !cookiCollection[0].Expired)
                {
                    loggedInUsername = cookiCollection["C_username"].Value;
                    return true;
                }
            }

            return false;
        }
    }
}
