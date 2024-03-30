using Microsoft.Extensions.DependencyInjection;
using System;

namespace RP_Notify.RpApi
{
    internal static class RpApiClientFactoryExtensions
    {
        public static IServiceCollection AddRpApiClient(this IServiceCollection services)
        {
            services.AddTransient<IRpApiClient, RpApiClient>();
            services.AddTransient<Func<IRpApiClient>>(serviceProvider => () => serviceProvider.GetService<IRpApiClient>());
            services.AddSingleton<IRpApiClientFactory, RpApiClientFactory>();

            return services;
        }
    }
}
