using Microsoft.Extensions.DependencyInjection;
using RP_Notify.PlayerWatcher.MusicBee.API;
using System;

namespace RP_Notify.PlayerWatchers.MusicBee.API
{
    internal static class MusicBeeIPCClientFactoryExtensions
    {
        public static IServiceCollection AddMusicBeeIPCClient(this IServiceCollection services)
        {
            services.AddTransient<IMusicBeeIPC, MusicBeeIPC>();
            services.AddTransient<Func<IMusicBeeIPC>>(serviceProvider => () => serviceProvider.GetService<IMusicBeeIPC>());
            services.AddSingleton<IMusicBeeIPCFactory, MusicBeeIPCFactory>();

            return services;
        }
    }
}
