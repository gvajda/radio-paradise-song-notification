using RP_Notify.Config;
using Serilog;
using System;
using System.Runtime.CompilerServices;

namespace RP_Notify.ErrorHandler
{
    public class Log : ILog
    {
        public ILogger Logger { get; set; }
        public Log(IConfig config)
        {
            Logger = GetLogger(config);
        }


        private ILogger GetLogger(IConfig config)
        {
            if (config.ExternalConfig.EnableLoggingToFile)
            {
                return new LoggerConfiguration()
                .WriteTo.File(
                    config.StaticConfig.LogFilePath,
                    fileSizeLimitBytes: 1048576,
                    rollOnFileSizeLimit: true,
                    shared: true
                    )
                .CreateLogger();
            }
            else
            {

                return new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            }
        }
    }

    public static class LogHelper
    {
        public static string GetMethodName(this object type, [CallerMemberName] string caller = null, bool fullName = false)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var name = fullName ? type.GetType().FullName : type.GetType().Name;
            return $"{name.PadLeft(17, '-')}.{caller}()";
            // return $"{name.PadRight(17, '-')} | {caller}()";
        }
    }
}
