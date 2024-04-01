using Microsoft.Extensions.DependencyInjection;
using RP_Notify.PlayerWatchers.Foobar2000.BeefWebApiClient;
using RP_Notify.PlayerWatchers.MusicBee.API;
using System;
using System.Collections.Generic;

namespace RP_Notify.PlayerWatchers
{
    internal static class PlayerWatcherProviderExtensions
    {
        public static IServiceCollection AddPlayerWatchers(this IServiceCollection services)
        {
            return services.AddBeefWebApiClient()
                .AddMusicBeeIPCClient()
                .AddSingleton<IPlayerWatcher, Foobar2000.Foobar2000Watcher>()
                .AddSingleton<IPlayerWatcher, MusicBee.MusicBeeWatcher>()
                .AddTransient<Func<IEnumerable<IPlayerWatcher>>>(serviceProvider => () =>
                    serviceProvider.GetServices<IPlayerWatcher>())
                .AddSingleton<IPlayerWatcherProvider, PlayerWatcherProvider>();
        }
    }
}
