using RP_Notify.API;
using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Toast;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP_Notify.SongInfoUpdater
{
    class SongInfoListener : ISongInfoListener
    {

        private readonly IRpApiHandler _apiHandler;
        private readonly IConfig _config;
        private readonly ILogger _log;
        private readonly IToastHandler _toastHandler;

        private readonly CancellationTokenSource listenerCancellationTokenSource;

        private CancellationTokenSource NextSongWaiterCancellationTokenSource { get; set; }

        public SongInfoListener(IRpApiHandler apiHandler, IConfig config, ILog log, IToastHandler toastHandler)
        {
            _apiHandler = apiHandler;
            _config = config;
            _log = log.Logger;
            _toastHandler = toastHandler;

            NextSongWaiterCancellationTokenSource = new CancellationTokenSource();
            listenerCancellationTokenSource = new CancellationTokenSource();

            Application.ApplicationExit += (sender, e) => listenerCancellationTokenSource.Cancel();
        }

        public void ResetListenerLoop()
        {
            NextSongWaiterCancellationTokenSource.Cancel();
        }

        public void Run()
        {
            _log.Information("SongInfoListener - Invoked");

            Task.Run(async () =>
            {
                while (!listenerCancellationTokenSource.Token.IsCancellationRequested)     // keep getting new song info
                {
                    try
                    {
                        NextSongWaiterCancellationTokenSource = new CancellationTokenSource();
                        var cancellationToken = NextSongWaiterCancellationTokenSource.Token;

                        CheckTrackedPlayerStatus();
                        UpdateSongInfo();

                        var timeLeftFromSongMilliseconds = (int)(_config.State.Playback.SongInfoExpiration - DateTime.Now).TotalMilliseconds;
                        var loopWaitMilliseconds = _config.State.RpTrackingConfig.ValidateActivePlayerId()
                            ? Math.Min(7000, timeLeftFromSongMilliseconds)
                            : timeLeftFromSongMilliseconds;

                        SetupRatingPrompt(loopWaitMilliseconds, cancellationToken);

                        try
                        {
                            var waitForNextSong = Task.Delay(loopWaitMilliseconds, cancellationToken);

                            while (!waitForNextSong.IsCompleted && !cancellationToken.IsCancellationRequested)
                            {
                                SendUpdateTooltipEvent();
                                await Task.Delay(1000, cancellationToken);
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            _log.Information("SongInfoListener - Restart loop");
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"SongInfoListener - Loop breaking inner ERROR - {ex.Message}");
                        _toastHandler.SongInfoListenerError();
                        await Task.Delay(10000);
                        Application.Restart();
                    }
                }
            }, listenerCancellationTokenSource.Token);

            _log.Information("SongInfoListener - Running in background");
        }

        private void SetupRatingPrompt(int loopWaitMilliseconds, CancellationToken cancellationToken)
        {
            if (!_config.ExternalConfig.PromptForRating)
            {
                return;
            }

            var timeLeftToPrompt = (int)(_config.State.Playback.SongInfoExpiration - DateTime.Now).TotalMilliseconds - 20000;

            if (0 < timeLeftToPrompt && timeLeftToPrompt < loopWaitMilliseconds)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(timeLeftToPrompt, cancellationToken);
                        if (_config.ExternalConfig.ShowOnNewSong
                            || _config.State.Foobar2000IsPlayingRP
                            || _config.State.RpTrackingConfig.ValidateActivePlayerId())
                        {
                            _toastHandler.ShowSongRatingToast();
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        if (_config.ExternalConfig.ShowOnNewSong
                            || _config.State.Foobar2000IsPlayingRP
                            || _config.State.RpTrackingConfig.ValidateActivePlayerId())
                        {
                            _toastHandler.ShowSongRatingToast();
                        }
                    }
                }, cancellationToken);
            }
        }

        private void SendUpdateTooltipEvent()
        {
            var timeLeft = (DateTime.Now - _config.State.Playback.SongInfoExpiration).ToString(@"m\:ss");
            string chanTitle = _config.State.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.ExternalConfig.Channel).First().Title;
            var trayIconText = $"{_config.State.Playback.SongInfo.Artist}\n{_config.State.Playback.SongInfo.Title}\n-{timeLeft}\n{chanTitle}";
            trayIconText = trayIconText.Length <= 63
                ? trayIconText
                : trayIconText.Substring(0, 62);
            _config.State.TooltipText = trayIconText;
        }

        private void CheckTrackedPlayerStatus()
        {
            if (_config.State.RpTrackingConfig.Enabled)
            {
                _log.Information("-- SongInfoListener - CheckTrackedPlayerStatus: Enabled");
                _config.State.RpTrackingConfig.Players = _apiHandler.GetSync_v2().Players;

                if (_config.State.RpTrackingConfig.TryGetTrackedChannel(out int currentChannel) && _config.ExternalConfig.Channel != currentChannel)
                {
                    _log.Information($"-- SongInfoListener - CheckTrackedPlayerStatus: Tracking is active - Tracked channel: {currentChannel}");

                    _config.ExternalConfig.Channel = currentChannel;
                }
            }
        }

        private void UpdateSongInfo()
        {
            string player_id = _config.State.RpTrackingConfig.ValidateActivePlayerId()
                ? _config.State.RpTrackingConfig.ActivePlayerId
                : null;

            var logMessageDetail = !string.IsNullOrEmpty(player_id)
                ? $"Player_ID: {player_id}"
                : $"Channel: {_config.ExternalConfig.Channel.ToString()}";

            _log.Information($"UpdateSongInfo - Invoked - {logMessageDetail}");

            var playback = new Playback(_apiHandler.GetNowplayingList());

            _log.Information("UpdateSongInfo - RP API call returned successfully - SongId: {@songId}", playback.SongInfo.SongId);

            // Update class attributes
            if (_config.State.TryUpdatePlayback(playback))
            {
                if (!_config.State.Playback.RatingUpdated)
                {
                    _log.Information("UpdateSongInfo - New song - Start downloading album art - Song info: {@Songdata}", playback.SongInfo);

                    // Download album art
                    using (WebClient client = new WebClient())
                    {
                        var tempFileName = $"{_config.StaticConfig.AlbumArtImagePath}.inprogress";
                        client.DownloadFile(new Uri($"{_config.StaticConfig.RpImageBaseUrl}/{playback.SongInfo.Cover}"), tempFileName);
                        if (File.Exists(_config.StaticConfig.AlbumArtImagePath))
                        {
                            File.Delete(_config.StaticConfig.AlbumArtImagePath);
                        }
                        File.Move(tempFileName, _config.StaticConfig.AlbumArtImagePath);
                    }
                    _log.Information("UpdateSongInfo - Albumart downloaded - Song expires: {@RefreshTimestamp} ({ExpirySeconds} seconds)", playback.SongInfoExpiration.ToString(), playback.NowplayingList.Refresh);
                }
                else
                {
                    _log.Information("UpdateSongInfo - Only song rating changed - New rating: {@rating}", playback.SongInfo.Rating);
                }
            }
            else
            {
                _log.Information("UpdateSongInfo - Same song: albumart and expiration is not updated");
            }

            _log.Information("UpdateSongInfo - Finished");
        }
    }
}
