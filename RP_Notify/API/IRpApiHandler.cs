using RP_Notify.API.ResponseModel;
using System.Collections.Generic;

namespace RP_Notify.API
{
    public interface IRpApiHandler
    {
        NowplayingList GetNowplayingList(int list_num = 1);
        NowPlaying GetNowPlaying(string channel = "0");
        GetBlock GetGetBlock(string channel = "0", string info = "true", string bitrate = "4");
        Info GetInfo(string songId = null);
        Rating GetRating(string songId, int rating);
        List<Channel> GetChannelList();
        Auth GetAuth(string username = null, string passwd = null);
        Sync_v2 GetSync_v2();
    }
}
