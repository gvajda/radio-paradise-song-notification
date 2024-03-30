using System;

namespace RP_Notify.RpApi
{
    internal class RpApiClientFactory : IRpApiClientFactory
    {
        private readonly Func<IRpApiClient> _rpApiClientCreator;

        public RpApiClientFactory(Func<IRpApiClient> rpApiClientCreator)
        {
            _rpApiClientCreator = rpApiClientCreator;
        }

        public IRpApiClient Create()
        {
            return _rpApiClientCreator();
        }
    }
}
