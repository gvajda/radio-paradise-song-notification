using RP_Notify.PlayerWatcher.MusicBee.API;
using System;

namespace RP_Notify.PlayerWatchers.MusicBee.API
{
    internal class MusicBeeIPCFactory : IMusicBeeIPCFactory
    {
        private readonly Func<IMusicBeeIPC> _musicBeeIPCClientCreator;

        public MusicBeeIPCFactory(Func<IMusicBeeIPC> musicBeeIPCClientCreator)
        {
            _musicBeeIPCClientCreator = musicBeeIPCClientCreator;
        }

        public IMusicBeeIPC GetClient()
        {
            return _musicBeeIPCClientCreator();
        }
    }
}
