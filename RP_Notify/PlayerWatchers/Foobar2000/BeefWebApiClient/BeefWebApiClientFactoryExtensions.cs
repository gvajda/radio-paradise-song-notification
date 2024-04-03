using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;

namespace RP_Notify.PlayerWatchers.Foobar2000.BeefWebApiClient
{
    internal static class BeefWebApiClientFactoryExtensions
    {
        public static IServiceCollection AddBeefWebApiClient(this IServiceCollection services)
        {
            return services.AddHttpClient<IBeefWebApiClient, BeefWebApiClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
                    Constants.HttpRetryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .Services
                .AddTransient<IBeefWebApiClient, BeefWebApiClient>()
                .AddTransient<Func<IBeefWebApiClient>>(serviceProvider => () => serviceProvider.GetService<IBeefWebApiClient>())
                .AddSingleton<IBeefWebApiClientFactory, BeefWebApiClientFactory>();
        }
    }
}
