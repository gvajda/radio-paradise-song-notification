using RP_Notify.Config;
using Serilog;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace RP_Notify.Helpers
{
    internal static class LogHelper
    {
        internal static string GetMethodName(this object type, [CallerMemberName] string caller = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var className = type.GetType().Name;     // name of class
            className = className.Length < 23             // add leading space if short
                ? $" {className}"
                : className;

            var functionName = $"{caller}() ";

            return $"{className.PadLeft(23, '-')}.{functionName.PadRight(22, '-')}";
        }

        public static Serilog.Core.Logger GetLogger(IConfigRoot config)
        {
            if (config.PersistedConfig.EnableLoggingToFile
                && File.Exists(config.StaticConfig.ConfigFilePath))
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
}
