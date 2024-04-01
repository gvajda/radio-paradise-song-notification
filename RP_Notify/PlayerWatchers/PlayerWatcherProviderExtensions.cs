using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace RP_Notify.PlayerWatchers
{
    internal static class PlayerWatcherProviderExtensions
    {
        public static IServiceCollection AddPlayerWatchers(this IServiceCollection services)
        {
            services.AddSingleton<IPlayerWatcher, Foobar2000.Foobar2000Watcher>();
            services.AddSingleton<IPlayerWatcher, MusicBee.MusicBeeWatcher>();


            services.AddTransient<Func<IEnumerable<IPlayerWatcher>>>(serviceProvider => () => serviceProvider.GetServices<IPlayerWatcher>());


            services.AddSingleton<IPlayerWatcherProvider, PlayerWatcherProvider>();

            return services;
        }
    }
}
