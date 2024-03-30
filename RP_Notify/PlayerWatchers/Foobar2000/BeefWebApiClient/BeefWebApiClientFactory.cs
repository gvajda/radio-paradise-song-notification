using System;

namespace RP_Notify.PlayerWatchers.Foobar2000.BeefWebApiClient
{
    internal class BeefWebApiClientFactory : IBeefWebApiClientFactory
    {
        private readonly Func<IBeefWebApiClient> _beefWebApiClientCreator;

        public BeefWebApiClientFactory(Func<IBeefWebApiClient> beefWebApiClientCreator)
        {
            _beefWebApiClientCreator = beefWebApiClientCreator;
        }

        public IBeefWebApiClient GetClient()
        {
            return _beefWebApiClientCreator();
        }
    }
}
