using RP_Notify.RpApi.ResponseModel;
using System;

namespace RP_Notify.ToastHandler
{
    public interface IToastHandler
    {
        void ShowSongStartToast(bool force = false, PlayListSong songInfo = null);
        void ShowSongRatingToast(PlayListSong songInfo = null);
        void ShowInvalidRatingArgumentToast(string invalidinput);
        void ShowSongDetailToast();
        void ShowErrorToast(Exception exception);
        void ShowDataEraseToast();
        void ShowLoginResponseToast(Auth authResp);
        void ShowLogoutRequestToast(string userName);
        void ShowConfigFolderChoicePromptToast();
    }
}
