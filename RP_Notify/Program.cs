using Microsoft.Extensions.DependencyInjection;
using RP_Notify.Config;
using RP_Notify.Logger;
using RP_Notify.PlayerWatchers;
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
            var rpConfig = new ConfigRoot();

            var serviceCollection = new ServiceCollection()
                .AddSingleton<IConfigRoot>(rpConfig)
                .AddLogger(rpConfig)
                .AddRpApiClient(rpConfig.State.RpCookieContainer)
                .AddToastHandler()
                .AddSingleton<LoginForm.LoginForm>()
                .AddPlayerWatchers()
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
