//--------------------------------------------------------//
// MusicBeeIPCSDK C# v2.0.0                               //
// Copyright © Kerli Low 2014                             //
// This file is licensed under the                        //
// BSD 2-Clause License                                   //
// See LICENSE_MusicBeeIPCSDK for more information.       //
//--------------------------------------------------------//

using System;
using System.Runtime.InteropServices;

namespace RP_Notify.PlayerWatcher.MusicBee.API
{

    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public partial class MusicBeeIPC : IMusicBeeIPC
    {
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint wMsg, UIntPtr wParam, IntPtr lParam);
        [DllImport("user32")]
        public static extern IntPtr FindWindow(IntPtr lpClassName, string lpWindowName);

        public MusicBeeIPC()
        {
        }

        public bool Probe()
        {
            return (Error)SendMessage(FindHwnd(), WM_USER, (UIntPtr)Command.Probe, IntPtr.Zero) != Error.Error;
        }

        public PlayState GetPlayState()
        {
            return (PlayState)SendMessage(FindHwnd(), WM_USER, (UIntPtr)Command.GetPlayState, IntPtr.Zero);
        }

        public string GetFileUrl()
        {
            string r = "";

            IntPtr hwnd = FindHwnd();

            IntPtr lr = SendMessage(hwnd, WM_USER, (UIntPtr)Command.GetFileUrl, IntPtr.Zero);

            Unpack(lr, out r);

            SendMessage(hwnd, WM_USER, (UIntPtr)Command.FreeLRESULT, lr);

            return r;
        }

        private IntPtr FindHwnd()
        {
            return FindWindow(IntPtr.Zero, "MusicBee IPC Interface");
        }
    }
}