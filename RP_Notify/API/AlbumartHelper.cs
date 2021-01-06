using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RP_Notify.API
{
    public interface IAlbumartHelper
    {
        Task<string> DownloadAlbumarAndGetFilePathtAsync(PlayListSong SongInfo = null);
        void DeleteAlbumArtFiles(int leaveLatestCount = 0);
    }

    public class AlbumartHelper : IAlbumartHelper
    {
        private readonly IConfig _config;
        private readonly ILog _log;

        public AlbumartHelper(IConfig config, ILog log)
        {
            _config = config;
            _log = log;
        }

        public void DeleteAlbumArtFiles(int leaveLatestCount = 0)
        {
            var enumerableAlbumartAll = new DirectoryInfo(_config.StaticConfig.ConfigBaseFolder).GetFiles("*.jpg");

            var enumerableAlbumartFiltered = leaveLatestCount > 0
                ? enumerableAlbumartAll
                    .OrderByDescending(x => x.LastWriteTime)
                    .Skip(leaveLatestCount)
                : enumerableAlbumartAll;

            foreach (var albumartFile in enumerableAlbumartFiltered.Select(f => f.FullName))
            {
                Retry.Do(() => { File.Delete(albumartFile); }, 500, 5);
            }
        }

        public async Task<string> DownloadAlbumarAndGetFilePathtAsync(PlayListSong SongInfo = null)
        {
            var songId = SongInfo != null
                ? SongInfo.SongId
                : _config.State.Playback.SongInfo.SongId;
            var albumartFullLocalPath = Path.Combine(_config.StaticConfig.ConfigBaseFolder, $"{songId}.jpg");

            if (File.Exists(albumartFullLocalPath))
            {
                return albumartFullLocalPath;
            }

            var tempFileName = $"{albumartFullLocalPath}.inprogress";

            if (File.Exists(tempFileName))
            {
                Retry.Do(() =>
                {
                    if (File.Exists(tempFileName))
                    {
                        throw new Exception($"Temporary file already exists: [{tempFileName}]");
                    }
                }, 500, 5);

                if (File.Exists(albumartFullLocalPath))
                {
                    return albumartFullLocalPath;
                }
            }


            // Download album art

            var coverUrl = SongInfo != null
                ? $"{_config.StaticConfig.RpImageBaseUrl}/{SongInfo.Cover}"
                : $"{_config.StaticConfig.RpImageBaseUrl}/{_config.State.Playback.SongInfo.Cover}";

            using (WebClient client = new WebClient())
            {
                Retry.Do(() => { client.DownloadFile(new Uri(coverUrl), tempFileName); }, 500, 5);
            }

            File.Move(tempFileName, albumartFullLocalPath);

            DeleteAlbumArtFiles(10);

            _log.Information(LogHelper.GetMethodName(this), "Albumart download successful - SongId: {songId}", songId);

            return albumartFullLocalPath;
        }
    }
}