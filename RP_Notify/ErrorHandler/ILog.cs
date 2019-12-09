using Serilog;

namespace RP_Notify.ErrorHandler
{
    public interface ILog
    {
        ILogger Logger { get; }
    }
}
