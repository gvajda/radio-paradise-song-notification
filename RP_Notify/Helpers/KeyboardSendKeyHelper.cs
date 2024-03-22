using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RP_Notify.Helpers
{
    public static class KeyboardSendKeyHelper
    {
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        const int KEYEVENTF_KEYUP = 0x0002;
        const byte VK_LWIN = 0x5B;
        const byte VK_N = 0x4E;

        public static void SendWinKeyN()
        {
            keybd_event(VK_LWIN, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero); // Press Win key
            keybd_event(VK_N, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero); // Press D key
            keybd_event(VK_N, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, IntPtr.Zero); // Release D key                                                                                       
            keybd_event(VK_LWIN, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, IntPtr.Zero); // Release Win key
        }
    }
}
