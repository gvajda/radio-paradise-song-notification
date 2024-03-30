using Microsoft.Extensions.DependencyInjection;
using System;

namespace RP_Notify.PlayerWatchers.Foobar2000.BeefWebApiClient
{
    internal static class BeefWebApiClientFactoryExtensions
    {
        public static IServiceCollection AddBeefWebApiClient(this IServiceCollection services)
        {
            services.AddHttpClient<IBeefWebApiClient, BeefWebApiClient>();
            services.AddTransient<IBeefWebApiClient, BeefWebApiClient>();
            services.AddTransient<Func<IBeefWebApiClient>>(serviceProvider => () => serviceProvider.GetService<IBeefWebApiClient>());
            services.AddSingleton<IBeefWebApiClientFactory, BeefWebApiClientFactory>();

            return services;
        }
    }
}
