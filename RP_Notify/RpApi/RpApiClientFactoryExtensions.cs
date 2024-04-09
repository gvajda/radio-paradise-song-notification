using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Net;
using System.Net.Http;

namespace RP_Notify.RpApi
{
    internal static class RpApiClientFactoryExtensions
    {
        public static IServiceCollection AddRpApiClient(this IServiceCollection services, CookieContainer cookieContainer)
        {
            return services.AddHttpClient(nameof(RpApiClient))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    CookieContainer = cookieContainer,
                    UseCookies = true
                })
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                    Constants.RpApiClientHttpRetryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Max(Math.Pow(2, retryAttempt), 10))))
                .Services
                .AddTransient<IRpApiClient, RpApiClient>()
                .AddTransient<Func<IRpApiClient>>(serviceProvider => () => serviceProvider.GetService<IRpApiClient>())
                .AddSingleton<IRpApiClientFactory, RpApiClientFactory>();
        }
    }
}
