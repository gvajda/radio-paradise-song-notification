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
        public static string DownloadAlbumartImageIfDoesntExist(IConfigRoot _config, PlayListSong overrideSongInfo = null)
        {
            var songInfo = overrideSongInfo != null
                ? overrideSongInfo
                : _config.State.Playback.SongInfo;

            var albumartFilePath = Path.Combine(_config.StaticConfig.AlbumArtCacheFolder, songInfo.SongId + ".jpg");

            if (!Directory.Exists(_config.StaticConfig.AlbumArtCacheFolder))
            {
                Directory.CreateDirectory(_config.StaticConfig.AlbumArtCacheFolder);
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

        public static void DeleteOldAlbumartImageFiles(IConfigRoot _config, int MaxAgeDays = 3, int MaxFileCount = 100)
        {
            try
            {
                DateTime currentDateTime = DateTime.Now;
                DateTime minimumDate = currentDateTime.AddDays(-MaxAgeDays);

                var cachedImageFileInfoList = Directory
                    .GetFiles(_config.StaticConfig.AlbumArtCacheFolder)
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
