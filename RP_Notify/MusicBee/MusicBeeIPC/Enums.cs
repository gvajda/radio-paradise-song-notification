﻿//--------------------------------------------------------//
// MusicBeeIPCSDK C# v2.0.0                               //
// Copyright © Kerli Low 2014                             //
// This file is licensed under the                        //
// BSD 2-Clause License                                   //
// See LICENSE_MusicBeeIPCSDK for more information.       //
//--------------------------------------------------------//

using System;



namespace RP_Notify.MusicBee.API
{

    public partial class MusicBeeIPC
    {
        public enum Bool
        {
            False = 0,
            True = 1
        }

        public enum Error
        {
            Error = 0,
            NoError = 1,
            CommandNotRecognized = 2
        }

        public enum PlayState
        {
            Undefined = 0,
            Loading = 1,
            Playing = 3,
            Paused = 6,
            Stopped = 7
        }

        public enum RepeatMode
        {
            None = 0,
            All = 1,
            One = 2
        }

        public enum ReplayGainMode
        {
            Off = 0,
            Track = 1,
            Album = 2,
            Smart = 3
        }

        public enum FileProperty
        {
            Url = 2,
            Kind = 4,
            Format = 5,
            Size = 7,
            Channels = 8,
            SampleRate = 9,
            Bitrate = 10,
            DateModified = 11,
            DateAdded = 12,
            LastPlayed = 13,
            PlayCount = 14,
            SkipCount = 15,
            Duration = 16,
            NowPlayingListIndex = 78,  // only has meaning when called from NowPlayingList_* commands
            ReplayGainTrack = 94,
            ReplayGainAlbum = 95
        }

        public enum MetaData
        {
            TrackTitle = 65,
            Album = 30,
            AlbumArtist = 31,       // displayed album artist
            AlbumArtistRaw = 34,       // stored album artist
            Artist = 32,       // displayed artist
            MultiArtist = 33,       // individual artists, separated by a null char
            PrimaryArtist = 19,       // first artist from multi-artist tagged file, otherwise displayed artist
            Artists = 144,
            ArtistsWithArtistRole = 145,
            ArtistsWithPerformerRole = 146,
            ArtistsWithGuestRole = 147,
            ArtistsWithRemixerRole = 148,
            Artwork = 40,
            BeatsPerMin = 41,
            Composer = 43,       // displayed composer
            MultiComposer = 89,       // individual composers, separated by a null char
            Comment = 44,
            Conductor = 45,
            Custom1 = 46,
            Custom2 = 47,
            Custom3 = 48,
            Custom4 = 49,
            Custom5 = 50,
            Custom6 = 96,
            Custom7 = 97,
            Custom8 = 98,
            Custom9 = 99,
            Custom10 = 128,
            Custom11 = 129,
            Custom12 = 130,
            Custom13 = 131,
            Custom14 = 132,
            Custom15 = 133,
            Custom16 = 134,
            DiscNo = 52,
            DiscCount = 54,
            Encoder = 55,
            Genre = 59,
            Genres = 143,
            GenreCategory = 60,
            Grouping = 61,
            Keywords = 84,
            HasLyrics = 63,
            Lyricist = 62,
            Lyrics = 114,
            Mood = 64,
            Occasion = 66,
            Origin = 67,
            Publisher = 73,
            Quality = 74,
            Rating = 75,
            RatingLove = 76,
            RatingAlbum = 104,
            Tempo = 85,
            TrackNo = 86,
            TrackCount = 87,
            Virtual1 = 109,
            Virtual2 = 110,
            Virtual3 = 111,
            Virtual4 = 112,
            Virtual5 = 113,
            Virtual6 = 122,
            Virtual7 = 123,
            Virtual8 = 124,
            Virtual9 = 125,
            Virtual10 = 135,
            Virtual11 = 136,
            Virtual12 = 137,
            Virtual13 = 138,
            Virtual14 = 139,
            Virtual15 = 140,
            Virtual16 = 141,
            Year = 88
        }

        [Flags()]
        public enum LibraryCategory
        {
            Music = 0,
            Audiobook = 1,
            Video = 2,
            Inbox = 4
        }

        public enum DataType
        {
            String = 0,
            Number = 1,
            DateTime = 2,
            Rating = 3
        }

        public enum LyricsType
        {
            NotSpecified = 0,
            Synchronised = 1,
            UnSynchronised = 2
        }

        public enum PlayButtonType
        {
            PreviousTrack = 0,
            PlayPause = 1,
            NextTrack = 2,
            Stop = 3
        }

        public enum PlaylistFormat
        {
            Unknown = 0,
            M3u = 1,
            Xspf = 2,
            Asx = 3,
            Wpl = 4,
            Pls = 5,
            Auto = 7,
            M3uAscii = 8,
            AsxFile = 9,
            Radio = 10,
            M3uExtended = 11,
            Mbp = 12
        }

        public enum MusicBeeVersion
        {
            v2_0 = 0,
            v2_1 = 1,
            v2_2 = 2,
            v2_3 = 3
        }

        public enum Command
        {
            PlayPause = 100,      // WM_USER
            Play = 101,      // WM_USER
            Pause = 102,      // WM_USER
            Stop = 103,      // WM_USER
            StopAfterCurrent = 104,      // WM_USER
            PreviousTrack = 105,      // WM_USER
            NextTrack = 106,      // WM_USER
            StartAutoDj = 107,      // WM_USER
            EndAutoDj = 108,      // WM_USER
            GetPlayState = 109,      // WM_USER
            GetPosition = 110,      // WM_USER
            SetPosition = 111,      // WM_USER
            GetVolume = 112,      // WM_USER
            SetVolume = 113,      // WM_USER
            GetVolumep = 114,      // WM_USER
            SetVolumep = 115,      // WM_USER
            GetVolumef = 116,      // WM_USER
            SetVolumef = 117,      // WM_USER
            GetMute = 118,      // WM_USER
            SetMute = 119,      // WM_USER
            GetShuffle = 120,      // WM_USER
            SetShuffle = 121,      // WM_USER
            GetRepeat = 122,      // WM_USER
            SetRepeat = 123,      // WM_USER
            GetEqualiserEnabled = 124,      // WM_USER
            SetEqualiserEnabled = 125,      // WM_USER
            GetDspEnabled = 126,      // WM_USER
            SetDspEnabled = 127,      // WM_USER
            GetScrobbleEnabled = 128,      // WM_USER
            SetScrobbleEnabled = 129,      // WM_USER
            ShowEqualiser = 130,      // WM_USER
            GetAutoDjEnabled = 131,      // WM_USER
            GetStopAfterCurrentEnabled = 132,      // WM_USER
            SetStopAfterCurrentEnabled = 133,      // WM_USER
            GetCrossfade = 134,      // WM_USER
            SetCrossfade = 135,      // WM_USER
            GetReplayGainMode = 136,      // WM_USER
            SetReplayGainMode = 137,      // WM_USER
            QueueRandomTracks = 138,      // WM_USER
            GetDuration = 139,      // WM_USER
            GetFileUrl = 140,      // WM_USER
            GetFileProperty = 141,      // WM_USER
            GetFileTag = 142,      // WM_USER
            GetLyrics = 143,      // WM_USER
            GetDownloadedLyrics = 144,      // WM_USER
            GetArtwork = 145,      // WM_USER
            GetArtworkUrl = 146,      // WM_USER
            GetDownloadedArtwork = 147,      // WM_USER
            GetDownloadedArtworkUrl = 148,      // WM_USER
            GetArtistPicture = 149,      // WM_USER
            GetArtistPictureUrls = 150,      // WM_USER
            GetArtistPictureThumb = 151,      // WM_USER
            IsSoundtrack = 152,      // WM_USER
            GetSoundtrackPictureUrls = 153,      // WM_USER
            GetCurrentIndex = 154,      // WM_USER
            GetNextIndex = 155,      // WM_USER
            IsAnyPriorTracks = 156,      // WM_USER
            IsAnyFollowingTracks = 157,      // WM_USER
            PlayNow = 158,      // WM_COPYDATA
            QueueNext = 159,      // WM_COPYDATA
            QueueLast = 160,      // WM_COPYDATA
            RemoveAt = 161,      // WM_USER
            ClearNowPlayingList = 162,      // WM_USER
            MoveFiles = 163,      // WM_COPYDATA
            ShowNowPlayingAssistant = 164,      // WM_USER
            GetShowTimeRemaining = 165,      // WM_USER
            GetShowRatingTrack = 166,      // WM_USER
            GetShowRatingLove = 167,      // WM_USER
            GetButtonEnabled = 168,      // WM_USER
            Jump = 169,      // WM_USER
            Search = 170,      // WM_COPYDATA
            SearchFirst = 171,      // WM_COPYDATA
            SearchIndices = 172,      // WM_COPYDATA
            SearchFirstIndex = 173,      // WM_COPYDATA
            SearchAndPlayFirst = 174,      // WM_COPYDATA
            NowPlayingList_GetListFileUrl = 200,      // WM_COPYDATA
            NowPlayingList_GetFileProperty = 201,      // WM_COPYDATA
            NowPlayingList_GetFileTag = 202,      // WM_COPYDATA
            NowPlayingList_QueryFiles = 203,      // WM_COPYDATA
            NowPlayingList_QueryGetNextFile = 204,      // WM_USER
            NowPlayingList_QueryGetAllFiles = 205,      // WM_USER
            NowPlayingList_QueryFilesEx = 206,      // WM_COPYDATA
            NowPlayingList_PlayLibraryShuffled = 207,      // WM_USER
            NowPlayingList_GetItemCount = 208,      // WM_USER
            Playlist_GetName = 300,      // WM_COPYDATA
            Playlist_GetType = 301,      // WM_COPYDATA
            Playlist_IsInList = 302,      // WM_COPYDATA
            Playlist_QueryPlaylists = 303,      // WM_USER
            Playlist_QueryGetNextPlaylist = 304,      // WM_USER
            Playlist_QueryFiles = 305,      // WM_COPYDATA
            Playlist_QueryGetNextFile = 306,      // WM_USER
            Playlist_QueryGetAllFiles = 307,      // WM_USER
            Playlist_QueryFilesEx = 308,      // WM_COPYDATA
            Playlist_CreatePlaylist = 309,      // WM_COPYDATA
            Playlist_DeletePlaylist = 310,      // WM_COPYDATA
            Playlist_SetFiles = 311,      // WM_COPYDATA
            Playlist_AppendFiles = 312,      // WM_COPYDATA
            Playlist_RemoveAt = 313,      // WM_COPYDATA
            Playlist_MoveFiles = 314,      // WM_COPYDATA
            Playlist_PlayNow = 315,      // WM_COPYDATA
            Playlist_GetItemCount = 316,      // WM_COPYDATA
            Library_GetFileProperty = 400,      // WM_COPYDATA
            Library_GetFileTag = 401,      // WM_COPYDATA
            Library_SetFileTag = 402,      // WM_COPYDATA
            Library_CommitTagsToFile = 403,      // WM_COPYDATA
            Library_GetLyrics = 404,      // WM_COPYDATA
            Library_GetArtwork = 405,      // WM_COPYDATA
            Library_GetArtworkUrl = 406,      // WM_COPYDATA
            Library_GetArtistPicture = 407,      // WM_COPYDATA
            Library_GetArtistPictureUrls = 408,      // WM_COPYDATA
            Library_GetArtistPictureThumb = 409,      // WM_COPYDATA
            Library_AddFileToLibrary = 410,      // WM_COPYDATA
            Library_QueryFiles = 411,      // WM_COPYDATA
            Library_QueryGetNextFile = 412,      // WM_USER
            Library_QueryGetAllFiles = 413,      // WM_USER
            Library_QueryFilesEx = 414,      // WM_COPYDATA
            Library_QuerySimilarArtists = 415,      // WM_COPYDATA
            Library_QueryLookupTable = 416,      // WM_COPYDATA
            Library_QueryGetLookupTableValue = 417,      // WM_COPYDATA
            Library_GetItemCount = 418,      // WM_USER
            Library_Jump = 419,      // WM_USER
            Library_Search = 420,      // WM_COPYDATA
            Library_SearchFirst = 421,      // WM_COPYDATA
            Library_SearchIndices = 422,      // WM_COPYDATA
            Library_SearchFirstIndex = 423,      // WM_COPYDATA
            Library_SearchAndPlayFirst = 424,      // WM_COPYDATA
            Setting_GetFieldName = 700,      // WM_COPYDATA
            Setting_GetDataType = 701,      // WM_COPYDATA
            Window_GetHandle = 800,      // WM_USER
            Window_Close = 801,      // WM_USER
            Window_Restore = 802,      // WM_USER
            Window_Minimize = 803,      // WM_USER
            Window_Maximize = 804,      // WM_USER
            Window_Move = 805,      // WM_USER
            Window_Resize = 806,      // WM_USER
            Window_BringToFront = 807,      // WM_USER
            Window_GetPosition = 808,      // WM_USER
            Window_GetSize = 809,      // WM_USER
            FreeLRESULT = 900,      // WM_USER
            MusicBeeVersion = 995,      // WM_USER
            PluginVersion = 996,      // WM_USER
            Test = 997,      // WM_USER      For debugging purposes
            MessageBox = 998,      // WM_COPYDATA  For debugging purposes
            Probe = 999       // WM_USER      To test MusicBeeIPC hwnd is valid
        }

        private const int WM_USER = 0x0400,
                          WM_COPYDATA = 0x004A;
    }
}