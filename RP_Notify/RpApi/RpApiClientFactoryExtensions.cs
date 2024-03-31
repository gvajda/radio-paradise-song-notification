using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;

namespace RP_Notify.RpApi
{
    internal static class RpApiClientFactoryExtensions
    {
        public static IServiceCollection AddRpApiClient(this IServiceCollection services, CookieContainer cookieContainer)
        {
            services.AddHttpClient(nameof(RpApiClient))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    CookieContainer = cookieContainer,
                    UseCookies = true
                });

            services.AddTransient<IRpApiClient, RpApiClient>();
            services.AddTransient<Func<IRpApiClient>>(serviceProvider => () => serviceProvider.GetService<IRpApiClient>());

            services.AddSingleton<IRpApiClientFactory, RpApiClientFactory>();

            return services;
        }
    }
}
