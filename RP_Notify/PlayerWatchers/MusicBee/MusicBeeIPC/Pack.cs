﻿//--------------------------------------------------------//
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
    public partial class MusicBeeIPC
    {
        private int sizeofInt32 = Marshal.SizeOf(typeof(Int32));
        private int sizeofDouble = Marshal.SizeOf(typeof(double));

        // --------------------------------------------------------------------------------
        // All strings are encoded in UTF-16 little endian
        // --------------------------------------------------------------------------------

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private void Free(ref COPYDATASTRUCT cds)
        {
            Marshal.FreeHGlobal(cds.lpData);
            cds.lpData = IntPtr.Zero;
        }

        // --------------------------------------------------------------------------------
        // -Int32: 32 bit integer
        // -Int32: 32 bit integer
        // -...
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(params int[] int32s)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int cb = sizeofInt32 * int32s.Length;

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            Marshal.Copy(int32s, 0, ptr, int32s.Length);

            return cds;
        }

        // --------------------------------------------------------------------------------
        // -Int32:  Byte count of string
        // -byte[]: String data
        // -...
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(string string_1)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int cb = sizeofInt32 + System.Text.Encoding.Unicode.GetByteCount(string_1);

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            int byteCount = System.Text.Encoding.Unicode.GetByteCount(string_1);

            Marshal.WriteInt32(ptr, byteCount);
            ptr += sizeofInt32;

            if (byteCount > 0)
                Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(string_1), 0, ptr, byteCount);

            return cds;
        }

        // --------------------------------------------------------------------------------
        // -Int32:  Byte count of 1st string
        // -byte[]: 1st string data
        // -Int32:  Byte count of 2nd string
        // -byte[]: 2nd string data
        // -...
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(params string[] strings)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int cb = 0;

            foreach (string s in strings)
                cb += sizeofInt32 + System.Text.Encoding.Unicode.GetByteCount(s);

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            int byteCount = 0;

            foreach (string s in strings)
            {
                byteCount = System.Text.Encoding.Unicode.GetByteCount(s);

                Marshal.WriteInt32(ptr, byteCount);
                ptr += sizeofInt32;

                if (byteCount > 0)
                    Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(s), 0, ptr, byteCount);

                ptr += byteCount;
            }

            return cds;
        }

        // --------------------------------------------------------------------------------
        // -Int32:  Byte count of string
        // -byte[]: String data
        // -Int32:  32 bit integer
        // -Int32:  32 bit integer
        // -...
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(string string_1, params int[] int32s)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int cb = sizeofInt32 + System.Text.Encoding.Unicode.GetByteCount(string_1) + sizeofInt32 * int32s.Length;

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            int byteCount = System.Text.Encoding.Unicode.GetByteCount(string_1);

            Marshal.WriteInt32(ptr, byteCount);
            ptr += sizeofInt32;

            if (byteCount > 0)
                Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(string_1), 0, ptr, byteCount);

            ptr += byteCount;

            Marshal.Copy(int32s, 0, ptr, int32s.Length);

            return cds;
        }

        // --------------------------------------------------------------------------------
        // -Int32:  Byte count of string
        // -byte[]: String data
        // -Int32:  bool
        // -Int32:  bool
        // -...
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(string string_1, params bool[] bools)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int cb = sizeofInt32 + System.Text.Encoding.Unicode.GetByteCount(string_1) + sizeofInt32 * bools.Length;

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            int byteCount = System.Text.Encoding.Unicode.GetByteCount(string_1);

            Marshal.WriteInt32(ptr, byteCount);
            ptr += sizeofInt32;

            if (byteCount > 0)
                Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(string_1), 0, ptr, byteCount);

            ptr += byteCount;

            foreach (bool b in bools)
            {
                Marshal.WriteInt32(ptr, b ? (int)Bool.True : (int)Bool.False);
                ptr += sizeofInt32;
            }

            return cds;
        }

        // --------------------------------------------------------------------------------
        // -Int32:  Byte count of string
        // -byte[]: String data
        // -double: 64-bit floating-point value
        // -double: 64-bit floating-point value
        // -...
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(string string_1, params double[] doubles)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int cb = sizeofInt32 + System.Text.Encoding.Unicode.GetByteCount(string_1) + sizeofDouble * doubles.Length;

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            int byteCount = System.Text.Encoding.Unicode.GetByteCount(string_1);

            Marshal.WriteInt32(ptr, byteCount);
            ptr += sizeofInt32;

            if (byteCount > 0)
                Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(string_1), 0, ptr, byteCount);

            ptr += byteCount;

            Marshal.Copy(doubles, 0, ptr, doubles.Length);

            return cds;
        }

        // --------------------------------------------------------------------------------
        // -Int32:  Byte count of string
        // -byte[]: String data
        // -Int32:  Number of strings in string array
        // -Int32:  Byte count of 1st string in string array
        // -byte[]: 1st string data in string array
        // -Int32:  Byte count of 2nd string in string array
        // -byte[]: 2nd string data in string array
        // -...
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(string string_1, string[] strings)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int cb = sizeofInt32 * 2 + System.Text.Encoding.Unicode.GetByteCount(string_1);

            foreach (string s in strings)
                cb += sizeofInt32 + System.Text.Encoding.Unicode.GetByteCount(s);

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            int byteCount = System.Text.Encoding.Unicode.GetByteCount(string_1);

            Marshal.WriteInt32(ptr, byteCount);
            ptr += sizeofInt32;

            if (byteCount > 0)
                Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(string_1), 0, ptr, byteCount);

            ptr += byteCount;

            Marshal.WriteInt32(ptr, strings.Length);
            ptr += sizeofInt32;

            foreach (string s in strings)
            {
                byteCount = System.Text.Encoding.Unicode.GetByteCount(s);

                Marshal.WriteInt32(ptr, byteCount);
                ptr += sizeofInt32;

                if (byteCount > 0)
                    Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(s), 0, ptr, byteCount);

                ptr += byteCount;
            }

            return cds;
        }

        // --------------------------------------------------------------------------------
        // -Int32:  Byte count of 1st string
        // -byte[]: 1st string data
        // -Int32:  Byte count of 2nd string
        // -byte[]: 2nd string data
        // -Int32:  Number of strings in string array
        // -Int32:  Byte count of 1st string in string array
        // -byte[]: 1st string data in string array
        // -Int32:  Byte count of 2nd string in string array
        // -byte[]: 2nd string data in string array
        // -...
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(string string_1, string string_2, string[] strings)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int cb = sizeofInt32 * 3 +
                     System.Text.Encoding.Unicode.GetByteCount(string_1) +
                     System.Text.Encoding.Unicode.GetByteCount(string_2);

            foreach (string s in strings)
                cb += sizeofInt32 + System.Text.Encoding.Unicode.GetByteCount(s);

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            int byteCount = System.Text.Encoding.Unicode.GetByteCount(string_1);

            Marshal.WriteInt32(ptr, byteCount);
            ptr += sizeofInt32;

            if (byteCount > 0)
                Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(string_1), 0, ptr, byteCount);

            ptr += byteCount;

            byteCount = System.Text.Encoding.Unicode.GetByteCount(string_2);

            Marshal.WriteInt32(ptr, byteCount);
            ptr += sizeofInt32;

            if (byteCount > 0)
                Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(string_2), 0, ptr, byteCount);

            ptr += byteCount;

            Marshal.WriteInt32(ptr, strings.Length);
            ptr += sizeofInt32;

            foreach (string s in strings)
            {
                byteCount = System.Text.Encoding.Unicode.GetByteCount(s);

                Marshal.WriteInt32(ptr, byteCount);
                ptr += sizeofInt32;

                if (byteCount > 0)
                    Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(s), 0, ptr, byteCount);

                ptr += byteCount;
            }

            return cds;
        }

        // --------------------------------------------------------------------------------
        // -Int32:  Byte count of 1st string
        // -byte[]: 1st string data
        // -Int32:  32 bit integer
        // -Int32:  Byte count of 2nd string
        // -byte[]: 2nd string data
        // -...
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(string string_1, int int32_1, string string_2)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int cb = sizeofInt32 * 3 +
                     System.Text.Encoding.Unicode.GetByteCount(string_1) +
                     System.Text.Encoding.Unicode.GetByteCount(string_2);

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            int byteCount = System.Text.Encoding.Unicode.GetByteCount(string_1);

            Marshal.WriteInt32(ptr, byteCount);
            ptr += sizeofInt32;

            if (byteCount > 0)
                Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(string_1), 0, ptr, byteCount);

            ptr += byteCount;

            Marshal.WriteInt32(ptr, int32_1);
            ptr += sizeofInt32;

            byteCount = System.Text.Encoding.Unicode.GetByteCount(string_2);

            Marshal.WriteInt32(ptr, byteCount);
            ptr += sizeofInt32;

            if (byteCount > 0)
                Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(string_2), 0, ptr, byteCount);

            return cds;
        }

        // --------------------------------------------------------------------------------
        // -Int32: Number of integers in integer array
        // -Int32: 32 bit integer
        // -Int32: 32 bit integer
        // -...
        // -Int32: 32 bit integer
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(int[] int32s, int int32_1)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int cb = sizeofInt32 * (int32s.Length + 2);

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            Marshal.WriteInt32(ptr, int32s.Length);
            ptr += sizeofInt32;

            Marshal.Copy(int32s, 0, ptr, int32s.Length);
            ptr += sizeofInt32 * int32s.Length;

            Marshal.WriteInt32(ptr, int32_1);

            return cds;
        }

        // --------------------------------------------------------------------------------
        // -Int32:  Byte count of string
        // -byte[]: String data
        // -Int32:  Number of integers in integer array
        // -Int32:  32 bit integer
        // -Int32:  32 bit integer
        // -...
        // -Int32:  32 bit integer
        // Free the returned COPYDATASTRUCT after use
        // --------------------------------------------------------------------------------
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private COPYDATASTRUCT Pack(string string_1, int[] int32s, int int32_1)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();

            int byteCount = System.Text.Encoding.Unicode.GetByteCount(string_1);

            int cb = sizeofInt32 * (int32s.Length + 3) + byteCount;

            cds.cbData = (uint)cb;

            cds.lpData = Marshal.AllocHGlobal(cb);

            IntPtr ptr = cds.lpData;

            Marshal.WriteInt32(ptr, byteCount);
            ptr += sizeofInt32;

            if (byteCount > 0)
                Marshal.Copy(System.Text.Encoding.Unicode.GetBytes(string_1), 0, ptr, byteCount);

            ptr += byteCount;

            Marshal.WriteInt32(ptr, int32s.Length);
            ptr += sizeofInt32;

            Marshal.Copy(int32s, 0, ptr, int32s.Length);
            ptr += sizeofInt32 * int32s.Length;

            Marshal.WriteInt32(ptr, int32_1);

            return cds;
        }
    }
}