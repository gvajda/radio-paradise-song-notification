using Microsoft.Extensions.DependencyInjection;
using RP_Notify.Config;
using System;

namespace RP_Notify.Logger
{
    internal static class LoggerFactoryExtensios
    {
        public static IServiceCollection AddLogger(this IServiceCollection services, ConfigRoot _congig)
        {
            services.AddTransient((Func<IServiceProvider, Func<Serilog.Core.Logger>>)(serviceProvider => () => Helpers.LogHelper.GetLogger(_congig)));

            services.AddSingleton<ILoggerFactory, LoggerFactory>();

            services.AddSingleton<ILoggerWrapper, LoggerWrapper>();

            return services;
        }
    }
}
