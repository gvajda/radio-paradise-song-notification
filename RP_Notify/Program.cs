﻿using Microsoft.Extensions.DependencyInjection;
using RP_Notify.API;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Foobar2000Watcher;
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
                .AddSingleton<IConfig, IniConfig>()
                .AddSingleton<ILog, Log>()
                .AddSingleton<IRpApiHandler, RpApiHandler>()
                .AddScoped<IToastHandler, ToastHandler>()
                .AddSingleton<IPlayerWatcher, Foobar2000Watcher.Foobar2000Watcher>()
                .AddSingleton<ISongInfoListener, SongInfoListener>()
                .AddSingleton<TrayApplication>()
                .BuildServiceProvider();

            Application.Run(serviceProvider.GetService<TrayApplication>());

        }
    }
}
