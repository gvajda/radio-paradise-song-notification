//--------------------------------------------------------//
// MusicBeeIPCSDK C# v2.0.0                               //
// Copyright © Kerli Low 2014                             //
// This file is licensed under the                        //
// BSD 2-Clause License                                   //
// See LICENSE_MusicBeeIPCSDK for more information.       //
//--------------------------------------------------------//

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;



namespace RP_Notify.PlayerWatchers.MusicBee.API
{

    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public partial class MusicBeeIPC
    {
        // --------------------------------------------------------------------------------
        // All strings are encoded in UTF-16 little endian
        // --------------------------------------------------------------------------------

        // --------------------------------------------------------------------------------
        // -Int32:  Byte count of the string
        // -byte[]: String data
        // Free lr after use
        // --------------------------------------------------------------------------------
        private bool Unpack(IntPtr lr, out string string_1)
        {
            string_1 = "";

            BinaryReader reader = GetMmfReader(lr);
            if (reader == null)
                return false;

            try
            {
                int byteCount = reader.ReadInt32();

                if (byteCount > 0)
                {
                    byte[] bytes = reader.ReadBytes(byteCount);

                    string_1 = System.Text.Encoding.Unicode.GetString(bytes);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private BinaryReader GetMmfReader(IntPtr lr)
        {
            if (lr == IntPtr.Zero)
                return null;

            try
            {
                LRUShort ls = new LRUShort(lr);

                MemoryMappedFile mmf =
                    MemoryMappedFile.OpenExisting("mbipc_mmf_" + ls.low.ToString(), MemoryMappedFileRights.Read);

                long size = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read).ReadInt64(ls.high);

                MemoryMappedViewStream stream =
                    mmf.CreateViewStream(ls.high + Marshal.SizeOf(typeof(long)), size, MemoryMappedFileAccess.Read);

                return new BinaryReader(stream);
            }
            catch
            {
                return null;
            }
        }
    }
}