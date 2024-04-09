using System;

namespace RP_Notify.Logger
{
    internal class LoggerWrapper : ILoggerWrapper
    {
        private readonly ILoggerFactory _loggerFactory;

        public LoggerWrapper(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void Information(string sender, string message, params object[] propertyValues)
        {
            var logger = _loggerFactory.Create();
            logger.Information($"{sender}- {message}", propertyValues);
            logger.Dispose();
        }

        public void Error(string sender, string message, params object[] propertyValues)
        {
            var logger = _loggerFactory.Create();
            logger.Error($"{sender}- ERROR - {message}", propertyValues);
            logger.Dispose();
        }

        public void Error(string sender, Exception ex)
        {
            var logger = _loggerFactory.Create();
            logger.Error($"{sender}- ERROR - {ex.Message}\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                logger.Error($"{sender}- INNEREXCEPTION - {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
            }
            logger.Dispose();
        }
    }
}
