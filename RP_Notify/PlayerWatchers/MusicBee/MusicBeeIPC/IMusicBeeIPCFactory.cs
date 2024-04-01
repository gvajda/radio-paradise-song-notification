
using RP_Notify.PlayerWatchers.MusicBee.API;

namespace RP_Notify.PlayerWatchers.MusicBee.API
{
    internal interface IMusicBeeIPCFactory
    {
        IMusicBeeIPC Create();
    }
}