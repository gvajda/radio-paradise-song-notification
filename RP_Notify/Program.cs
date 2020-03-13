using Foobar2000.RESTClient.Api;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using RP_Notify.API;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.SongInfoUpdater;
using RP_Notify.Toast;
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
                .AddSingleton<RestClient>()
                .AddSingleton<IConfig, IniConfig>()
                .AddSingleton<ILog, Log>()
                .AddSingleton<PlayerApi>()
                .AddSingleton<IRpApiHandler, RpApiHandler>()
                .AddScoped<IToastHandler, ToastHandler>()
                .AddSingleton<Foobar2000.Foobar2000Watcher>()
                .AddSingleton<ISongInfoListener, SongInfoListener>()
                .AddSingleton<TrayApplication>()
                .BuildServiceProvider();

            Application.Run(serviceProvider.GetService<TrayApplication>());

        }
    }
}
