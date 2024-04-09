using System;

namespace RP_Notify.Logger
{
    internal class LoggerFactory : ILoggerFactory
    {
        private readonly Func<Serilog.Core.Logger> _loggerCreator;

        public LoggerFactory(Func<Serilog.Core.Logger> loggerCreator)
        {
            _loggerCreator = loggerCreator;
        }

        public Serilog.Core.Logger Create()
        {
            return _loggerCreator();
        }
    }
}
