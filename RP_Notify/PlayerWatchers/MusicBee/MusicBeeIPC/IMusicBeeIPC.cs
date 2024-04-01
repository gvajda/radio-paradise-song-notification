//--------------------------------------------------------//
// MusicBeeIPCSDK C# v2.0.0                               //
// Copyright © Kerli Low 2014                             //
// This file is licensed under the                        //
// BSD 2-Clause License                                   //
// See LICENSE_MusicBeeIPCSDK for more information.       //
//--------------------------------------------------------//

namespace RP_Notify.PlayerWatchers.MusicBee.API
{
    public interface IMusicBeeIPC
    {
        string GetFileUrl();
        MusicBeeIPC.PlayState GetPlayState();
        bool Probe();
    }
}