//--------------------------------------------------------//
// MusicBeeIPCSDK C# v2.0.0                               //
// Copyright © Kerli Low 2014                             //
// This file is licensed under the                        //
// BSD 2-Clause License                                   //
// See LICENSE_MusicBeeIPCSDK for more information.       //
//--------------------------------------------------------//

using System;
using System.Runtime.InteropServices;



namespace RP_Notify.PlayerWatchers.MusicBee.API
{

    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public partial class MusicBeeIPC
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct FloatInt
        {
            [FieldOffset(0)]
            public float f;
            [FieldOffset(0)]
            public int i;

            public FloatInt(float f) { this.i = 0; this.f = f; }
            public FloatInt(int i) { this.f = 0.0f; this.i = i; }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct LRUShort
        {
            [FieldOffset(0)]
            public IntPtr lr;
            [FieldOffset(0)]
            public ushort low;
            [FieldOffset(2)]
            public ushort high;

            public LRUShort(IntPtr ptr) { this.low = this.high = 0; this.lr = ptr; }
        }
    }
}