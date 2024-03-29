using System;

namespace RP_Notify.LoginForm
{
    internal class LoginInputEvent : EventArgs
    {
        internal string UserName { get; set; }
        internal string Password { get; set; }

        internal LoginInputEvent(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
