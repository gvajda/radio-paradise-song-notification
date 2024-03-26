using RP_Notify.RpApi.ResponseModel;
using System;

namespace RP_Notify.Toast
{
    public interface IToastHandler
    {
        void ShowSongStartToast(bool force = false, PlayListSong songInfo = null);
        void ShowSongRatingToast(PlayListSong songInfo = null);
        void ShowSongDetailToast();
        void ShowLoginToast();
        void ErrorToast(Exception exception);
        void DataEraseToast();
        void LoginResponseToast(Auth authResp);
        void ConfigFolderToast();
    }
}
