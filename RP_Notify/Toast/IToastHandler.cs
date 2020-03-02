namespace RP_Notify.Toast
{
    public interface IToastHandler
    {
        void ShowSongStartToast();
        void ShowSongRatingToast();
        void ShowSongDetailToast();
        void SongInfoListenerError();
    }
}
