using RP_Notify.RpApi.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace RP_Notify.Helpers
{
    public static class AlbumartFileHelper
    {
        private const string AlbumArtFolderName = "AlbumArtCache";
        private const int MaxAgeDays = 3;
        private const int MaxFileCount = 100;

        public static string DownloadAlbumartImageIfDoesntExist(IConfig _config, PlayListSong overrideSongInfo = null)
        {
            var songInfo = overrideSongInfo != null
                ? overrideSongInfo
                : _config.State.Playback.SongInfo;

            var albumArtCacheDirectory = Path.Combine(_config.StaticConfig.ConfigBaseFolder, AlbumArtFolderName);
            var albumartFilePath = Path.Combine(albumArtCacheDirectory, songInfo.SongId + ".jpg");

            if (!Directory.Exists(albumArtCacheDirectory))
            {
                Directory.CreateDirectory(albumArtCacheDirectory);
            }

            if (File.Exists(albumartFilePath))
            {
                File.SetLastAccessTime(albumartFilePath, DateTime.Now);

            } else
            {
                var rpImageUrl = new Uri($"{_config.StaticConfig.RpImageBaseUrl}/{songInfo.Cover}");
                var tempFileName = $"{albumartFilePath}.inprogress";

                using (WebClient client = new WebClient())
                {
                    Retry.Do(() => { client.DownloadFile(rpImageUrl, tempFileName); }, 500, 5);
                }

                File.Move(tempFileName, albumartFilePath);
            }

            return albumartFilePath;
        }

        public static void DeleteOldAlbumartImageFiles(IConfig _config)
        {
            try
            {
                DateTime currentDateTime = DateTime.Now;
                DateTime minimumDate = currentDateTime.AddDays(-MaxAgeDays);

                var albumArtCacheDirectory = Path.Combine(_config.StaticConfig.ConfigBaseFolder, AlbumArtFolderName);
                var cachedImageFileInfoList = Directory
                    .GetFiles(albumArtCacheDirectory)
                    .Select(p => new FileInfo(p));

                cachedImageFileInfoList
                    .Where(f => f.LastWriteTime < minimumDate)
                    .ToList()
                    .ForEach(f => f.Delete());

                cachedImageFileInfoList
                    .OrderByDescending(f => f.LastWriteTime)
                    .Skip(MaxFileCount)
                    .ToList()
                    .ForEach(f => f.Delete());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deleting files: {e.Message}");
            }
        }
    }
}
