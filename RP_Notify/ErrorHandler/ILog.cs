using System;

namespace RP_Notify.ErrorHandler
{
    public interface ILog
    {
        void Information(string sender, string message, params object[] propertyValues);
        void Error(string sender, Exception ex);
        void Error(string sender, string message, params object[] propertyValues);
    }
}
