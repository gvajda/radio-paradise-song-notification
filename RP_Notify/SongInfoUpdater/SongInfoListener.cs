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

        private const int secondsBeforeSongEndsToPromptRating = 20;

        private Task SongInfoListenerTask { get; set; }
        private CancellationTokenSource NextSongWaiterCancellationTokenSource { get; set; }
        private CancellationTokenSource listenerCancellationTokenSource { get; }

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
            Task.Run(() => { NextSongWaiterCancellationTokenSource.Cancel(); });
        }

        public void Start()
        {
            if (SongInfoListenerTask == null || SongInfoListenerTask.IsCompleted)
            {
                _log.Information($"{LogHelper.GetMethodName(this)} - Invoked");
                Run();
            }
            else
            {
                _log.Information($"{LogHelper.GetMethodName(this)} - Alreay running");
            }
        }

        private void Run()
        {
            _log.Information($"{LogHelper.GetMethodName(this)} - Invoked");

            SongInfoListenerTask = Task.Run(async () =>
            {
                while (!listenerCancellationTokenSource.Token.IsCancellationRequested)     // keep getting new song info
                {
                    try
                    {
                        NextSongWaiterCancellationTokenSource = new CancellationTokenSource();
                        var cancellationToken = NextSongWaiterCancellationTokenSource.Token;

                        CheckTrackedRpPlayerStatus();
                        UpdateSongInfo();

                        var timeLeftFromSongMilliseconds = (int)(_config.State.Playback.SongInfoExpiration - DateTime.Now).TotalMilliseconds;

                        // check more often when RP player is tracked
                        var loopWaitMilliseconds = _config.IsRpPlayerTrackingChannel()
                            ? Math.Min(7000, timeLeftFromSongMilliseconds)
                            : timeLeftFromSongMilliseconds;

                        // when Bill is talking
                        loopWaitMilliseconds = loopWaitMilliseconds > 0
                            ? loopWaitMilliseconds
                            : 5000;

                        try
                        {
                            var waitForNextSong = Task.Delay(loopWaitMilliseconds, cancellationToken);

                            while (!waitForNextSong.IsCompleted && !cancellationToken.IsCancellationRequested)
                            {
                                var RefreshDelay = Task.Delay(1000, cancellationToken);
                                SendUpdateTooltipEvent();
                                PromptRatingAtTheEndOfSongOrIfCanceled();
                                await RefreshDelay;
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            PromptRatingAtTheEndOfSongOrIfCanceled();
                            _log.Information($"{LogHelper.GetMethodName(this)} - Restart loop");
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"{LogHelper.GetMethodName(this)} - Loop breaking inner ERROR - {ex.Message}\n{ex.StackTrace}");
                        _toastHandler.SongInfoListenerError();
                        await Task.Delay(10000);
                        Application.Restart();
                    }
                }
            }, listenerCancellationTokenSource.Token);

            _log.Information($"{LogHelper.GetMethodName(this)} - Running in background");
        }

        public void CheckTrackedRpPlayerStatus()
        {
            _log.Information($"{LogHelper.GetMethodName(this)} - {_config.ExternalConfig.EnableRpOfficialTracking}");
            if (_config.ExternalConfig.EnableRpOfficialTracking)
            {
                _log.Information($"{LogHelper.GetMethodName(this)} - Refresh available players");

                _config.State.RpTrackingConfig.Players = _apiHandler.GetSync_v2().Players;

                if (_config.IsRpPlayerTrackingChannel(out int trackedChannel) && _config.ExternalConfig.Channel != trackedChannel)
                {
                    _log.Information($"{LogHelper.GetMethodName(this)} - Tracking is active - Tracked channel: {trackedChannel}");

                    _config.ExternalConfig.Channel = trackedChannel;
                }
            }
        }

        private void PromptRatingAtTheEndOfSongOrIfCanceled()
        {
            bool somethingIsPlaying = _config.ExternalConfig.ShowOnNewSong
                || _config.State.Foobar2000IsPlayingRP
                || _config.IsRpPlayerTrackingChannel();

            var millisecsLeftToPrompt = (int)(_config.State.Playback.SongInfoExpiration - DateTime.Now).TotalMilliseconds - secondsBeforeSongEndsToPromptRating * 1000;

            if (_config.ExternalConfig.PromptForRating
                && string.IsNullOrEmpty(_config.State.Playback.SongInfo.Rating)
                && somethingIsPlaying)
            {
                if (0 < millisecsLeftToPrompt && millisecsLeftToPrompt < 1000)
                {
                    _log.Information($"{LogHelper.GetMethodName(this)} - Prompt at the end of song");
                }
                else if (1000 < millisecsLeftToPrompt && NextSongWaiterCancellationTokenSource.IsCancellationRequested)
                {
                    _log.Information($"{LogHelper.GetMethodName(this)} - Prompt due to song cancellation");
                }
                else
                {
                    return;
                }
                _toastHandler.ShowSongRatingToast();
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

        private void UpdateSongInfo()
        {
            string player_id = _config.IsRpPlayerTrackingChannel()
                ? _config.State.RpTrackingConfig.ActivePlayerId
                : null;

            var logMessageDetail = !string.IsNullOrEmpty(player_id)
                ? $"Player_ID: {player_id}"
                : $"Channel: {_config.ExternalConfig.Channel.ToString()}";

            _log.Information($"{LogHelper.GetMethodName(this)} - Invoked - {logMessageDetail}");

            var getNowplayingList = _apiHandler.GetNowplayingList();
            var playback = new Playback(getNowplayingList);

            _log.Information($"{LogHelper.GetMethodName(this)} - RP API call returned successfully - SongId: {{@songId}}", playback.SongInfo.SongId);

            // Update class attributes
            if (_config.State.TryUpdatePlayback(playback))
            {
                if (!_config.State.Playback.RatingUpdated)
                {
                    _log.Information($"{LogHelper.GetMethodName(this)} - New song - Start downloading album art - Song info: {{@Songdata}}", playback.SongInfo);

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
                    _log.Information($"{LogHelper.GetMethodName(this)} - Albumart downloaded - Song expires: {{@RefreshTimestamp}} ({{ExpirySeconds}} seconds)", playback.SongInfoExpiration.ToString(), playback.NowplayingList.Refresh);
                }
                else
                {
                    _log.Information($"{LogHelper.GetMethodName(this)} - Only song rating changed - New rating: {{@rating}}", playback.SongInfo.Rating);
                }
            }
            else
            {
                _log.Information($"{LogHelper.GetMethodName(this)} - Same song: albumart and expiration is not updated - Seconds left: {getNowplayingList.Refresh}");
            }

            _log.Information($"{LogHelper.GetMethodName(this)} - Finished");
        }
    }
}
