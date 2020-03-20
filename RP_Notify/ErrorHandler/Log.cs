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

        public void Information(string sender, string message, params object[] propertyValues)
        {
            Logger.Information($"{sender} - {message}", propertyValues);
        }

        public void Error(string sender, string message, params object[] propertyValues)
        {
            Logger.Error($"{sender} - ERROR - {message}", propertyValues);
        }

        public void Error(string sender, Exception ex)
        {
            Logger.Error($"{sender} - ERROR - {ex.Message}\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Logger.Error($"{sender} - INNEREXCEPTION - {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
            }
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
                    retainedFileCountLimit: 10,
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
            return $"{name.PadLeft(17, '-')}.{(caller != ".ctor" ? caller : "CONSTRUCTOR")}()";
            // return $"{name.PadRight(17, '-')} | {caller}()";
        }
    }
}
