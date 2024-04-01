namespace RP_Notify.Logger
{
    internal interface ILoggerFactory
    {
        Serilog.Core.Logger Create();
    }
}