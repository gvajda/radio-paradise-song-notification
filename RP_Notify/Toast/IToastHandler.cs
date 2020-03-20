namespace RP_Notify.Toast
{
    public interface IToastHandler
    {
        void ShowSongStartToast(bool force = false);
        void ShowSongRatingToast();
        void ShowSongDetailToast();
        void SongInfoListenerErrorToast();
    }
}
