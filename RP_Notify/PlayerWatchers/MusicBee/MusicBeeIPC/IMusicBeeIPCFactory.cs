
using RP_Notify.PlayerWatcher.MusicBee.API;

namespace RP_Notify.PlayerWatchers.MusicBee.API
{
    internal interface IMusicBeeIPCFactory
    {
        IMusicBeeIPC GetClient();
    }
}