using RP_Notify.Config;
using Serilog;

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
}
