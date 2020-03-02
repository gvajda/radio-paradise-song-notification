using RP_Notify.API.ResponseModel;
using System;
using System.Collections.Generic;

namespace RP_Notify.API
{
    public interface IRpApiHandler
    {
        List<Channel> ChannelList { get; }
        PlayListSong SongInfo { get; }
        DateTime SongInfoExpiration { get; }
        bool IsUserAuthenticated { get; }
        void UpdateSongInfo();
        NowplayingList GetNowplayingList(string channel = "0", string player_id = null, int list_num = 1);
        NowPlaying GetNowPlaying(string channel = "0");
        GetBlock GetGetBlock(string channel = "0", string info = "true", string bitrate = "4");
        Info GetInfo(string songId = null);
        Rating GetRating(string songId, int rating);
        List<Channel> GetChannelList();
        Auth GetAuth(string username = null, string passwd = null);
        Sync_v2 GetSync_v2();

        //Task<NowplayingList> NowplayingListAsync(string channel = "0");
        //Task<NowPlaying> NowplayingAsync(string channel = "0");
        //Task<GetBlock> GetBlockAsync(string channel = "0", string info = "true", string bitrate = "4");
        //Task<Info> InfoAsync(string songId = null);
        //Task<Rating> RatingAsync(string songId, int rating);
        //Task<List<Channel>> ChannelListAsync();
        //// Auth Auth(string username, string passwd);
        //Task<Auth> AuthAsync(string username, string passwd);
    }
}
