using Foobar2000.RESTClient.Api;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using RP_Notify.API;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.PlayerWatcher.Foobar2000;
using RP_Notify.PlayerWatcher.MusicBee;
using RP_Notify.PlayerWatcher.MusicBee.API;
using RP_Notify.SongInfoListener;
using RP_Notify.Toast;
using RP_Notify.TrayIcon;
using System;
using System.Windows.Forms;

namespace RP_Notify
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfig, IniConfig>()
                .AddSingleton<ILog, Log>()
                .AddSingleton<RestClient>()
                .AddSingleton<IRpApiHandler, RpApiHandler>()
                .AddScoped<IToastHandler, PackageToastHandler>()
                .AddSingleton<PlayerApi>()
                .AddSingleton<Foobar2000Watcher>()
                .AddSingleton<MusicBeeIPC>()
                .AddSingleton<MusicBeeWatcher>()
                .AddSingleton<ISongInfoListener, SongInfoListener.SongInfoListener>()
                .AddSingleton<RpTrayIcon>()
                .AddSingleton<RpApplicationCore>()
                .BuildServiceProvider();

            Application.Run(serviceProvider.GetService<RpApplicationCore>());

        }
    }
}
