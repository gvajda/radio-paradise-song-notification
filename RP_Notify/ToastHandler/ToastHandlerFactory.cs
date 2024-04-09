using System;

namespace RP_Notify.ToastHandler
{
    internal class ToastHandlerFactory : IToastHandlerFactory
    {
        private readonly Func<IToastHandler> _rpToastHandlerCreator;

        public ToastHandlerFactory(Func<IToastHandler> rpToastHandlerCreator)
        {
            _rpToastHandlerCreator = rpToastHandlerCreator;
        }

        public IToastHandler Create()
        {
            return _rpToastHandlerCreator();
        }
    }
}
