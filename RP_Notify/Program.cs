using Microsoft.Extensions.DependencyInjection;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.PlayerWatcher.Foobar2000;
using RP_Notify.PlayerWatcher.MusicBee;
using RP_Notify.PlayerWatcher.MusicBee.API;
using RP_Notify.PlayerWatchers.Foobar2000.BeefWebApiClient;
using RP_Notify.RpApi;
using RP_Notify.SongInfoListener;
using RP_Notify.ToastHandler;
using RP_Notify.TrayIconMenu;
using System;
using System.Windows.Forms;

namespace RP_Notify
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var serviceCollection = new ServiceCollection()
                .AddHttpClient()
                .AddTransient<BeefWebApiClient>()
                .AddSingleton<LoginForm.LoginForm>()
                .AddSingleton<IConfigRoot, ConfigRoot>()
                .AddSingleton<ILog, Log>()
                .AddSingleton<IRpApiHandler, RpApiHandler>()
                .AddScoped<IToastHandler, ToastHandler.ToastHandler>()
                .AddSingleton<Foobar2000Watcher>()
                .AddSingleton<MusicBeeIPC>()
                .AddSingleton<MusicBeeWatcher>()
                .AddSingleton<ISongInfoListener, SongInfoListener.SongInfoListener>()
                .AddSingleton<RpTrayIconMenu>()
                .AddSingleton<RpApplicationCore>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(serviceProvider.GetService<RpApplicationCore>());
        }
    }
}
