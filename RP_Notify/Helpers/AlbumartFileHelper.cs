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
        public static string DownloadAlbumartImageIfDoesntExist(IConfig _config, PlayListSong overrideSongInfo = null)
        {
            var songInfo = overrideSongInfo != null
                ? overrideSongInfo
                : _config.State.Playback.SongInfo;

            // Download album art
            var albumartFilePath = Path.Combine(_config.StaticConfig.ConfigBaseFolder, songInfo.SongId + ".jpg");

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

        public static void DeleteOldAlbumartImageFiles(IConfig _config, int maxAgeDays = 3)
        {
            try
            {
                DateTime currentDateTime = DateTime.Now;
                DateTime minimumDate = currentDateTime.AddDays(-maxAgeDays);

                var cachedImagePathList = Directory.GetFiles(_config.StaticConfig.ConfigBaseFolder);

                var albumartFilePattern = new Regex(@"\d+\.jpg");

                foreach (string filePath in cachedImagePathList)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    var regexMatch = albumartFilePattern.Match(fileInfo.Name);

                    if (regexMatch.Success && fileInfo.LastAccessTime < minimumDate)
                    {
                        fileInfo.Delete();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deleting files: {e.Message}");
            }
        }
    }
}
