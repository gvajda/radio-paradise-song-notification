using Microsoft.Extensions.DependencyInjection;
using System;

namespace RP_Notify.ToastHandler
{
    internal static class ToastHandlerFactoryExtensions
    {
        public static IServiceCollection AddToastHandler(this IServiceCollection services)
        {
            return services.AddTransient<IToastHandler, ToastHandler>()
                .AddTransient<Func<IToastHandler>>(serviceProvider => () =>
                    serviceProvider.GetService<IToastHandler>())
                .AddSingleton<IToastHandlerFactory, ToastHandlerFactory>();
        }
    }
}
