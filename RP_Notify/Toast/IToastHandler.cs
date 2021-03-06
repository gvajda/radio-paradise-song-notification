﻿using System;

namespace RP_Notify.Toast
{
    public interface IToastHandler
    {
        void ShowSongStartToast(bool force = false);
        void ShowSongRatingToast();
        void ShowSongDetailToast();
        void ShowLoginToast();
        void ErrorToast(Exception exception);
        void DataEraseToast();
    }
}
