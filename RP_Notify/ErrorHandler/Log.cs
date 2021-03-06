﻿using RP_Notify.Config;
using Serilog;
using Serilog.Core;
using System;
using System.Runtime.CompilerServices;

namespace RP_Notify.ErrorHandler
{
    public class Log : ILog
    {
        private Logger Logger { get; set; }

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

        public void Dispose()
        {
            Logger.Dispose();
        }

        private Logger GetLogger(IConfig config)
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
