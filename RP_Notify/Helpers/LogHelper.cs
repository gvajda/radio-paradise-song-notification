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

            var name = type.GetType().Name;     // name of class
            name = name.Length < 17             // add leading space if short
                ? $" {name}"
                : name;

            return $"{name.PadLeft(17, '-')}.{(caller)}()";
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
