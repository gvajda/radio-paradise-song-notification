using RP_Notify.Config;
using Serilog;
using Serilog.Core;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace RP_Notify.ErrorHandler
{
    public class Log : ILog
    {
        private readonly IConfigRoot _config;

        public Log(IConfigRoot config)
        {
            _config = config;
        }

        public void Information(string sender, string message, params object[] propertyValues)
        {
            var logger = GetLogger(_config);
            logger.Information($"{sender} - {message}", propertyValues);
            logger.Dispose();

        }

        public void Error(string sender, string message, params object[] propertyValues)
        {
            var logger = GetLogger(_config);
            logger.Error($"{sender} - ERROR - {message}", propertyValues);
            logger.Dispose();
        }

        public void Error(string sender, Exception ex)
        {
            var logger = GetLogger(_config);
            logger.Error($"{sender} - ERROR - {ex.Message}\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                logger.Error($"{sender} - INNEREXCEPTION - {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
            }
            logger.Dispose();
        }

        private Logger GetLogger(IConfigRoot config)
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
                if (File.Exists(config.StaticConfig.LogFilePath))
                {
                    Directory.Delete(Path.GetDirectoryName(config.StaticConfig.LogFilePath));
                }

                return new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            }
        }


    }

    public static class LogHelper
    {
        public static string GetMethodName(this object type, [CallerMemberName] string caller = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var name = type.GetType().Name;     // name of class
            name = name.Length < 17             // add leading space if short
                ? $" {name}"
                : name;

            return $"{name.PadLeft(17, '-')}.{(caller)}()";
        }
    }
}
