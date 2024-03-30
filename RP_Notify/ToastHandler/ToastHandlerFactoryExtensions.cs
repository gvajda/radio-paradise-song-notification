using Microsoft.Extensions.DependencyInjection;
using System;

namespace RP_Notify.ToastHandler
{
    internal static class ToastHandlerFactoryExtensions
    {
        public static IServiceCollection AddToastHandler(this IServiceCollection services)
        {
            services.AddTransient<IToastHandler, ToastHandler>();
            services.AddTransient<Func<IToastHandler>>(serviceProvider => () => serviceProvider.GetService<IToastHandler>());
            services.AddSingleton<IToastHandlerFactory, ToastHandlerFactory>();

            return services;
        }
    }
}
