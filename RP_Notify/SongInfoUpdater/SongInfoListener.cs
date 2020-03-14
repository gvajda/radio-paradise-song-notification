using RP_Notify.API;
using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Toast;
using Serilog;
using System;
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
        private readonly IToastHandler _toastHandler;
        private readonly CancellationTokenSource listenerCancellationTokenSource;
        private readonly ILogger _log;

        public event EventHandler<TooltipChangeEventArgs> TooltipUpdateChangedEventHandler;
        public event EventHandler<ConfigChangeEventArgs> ConfigChangedEventHandler;

        private CancellationTokenSource NextSongWaiterCancellationTokenSource { get; set; }

        public SongInfoListener(IRpApiHandler apiHandler, IConfig config, IToastHandler toastHandler, ILog log)
        {
            _apiHandler = apiHandler;
            _config = config;
            _toastHandler = toastHandler;
            _log = log.Logger;

            NextSongWaiterCancellationTokenSource = new CancellationTokenSource();
            listenerCancellationTokenSource = new CancellationTokenSource();

            Application.ApplicationExit += (sender, e) => listenerCancellationTokenSource.Cancel();
        }

        public void CheckSong()
        {
            NextSongWaiterCancellationTokenSource.Cancel();
        }

        public void Run()
        {
            if (_config.ExternalConfig.EnablePlayerWatcher)
            {
                Thread.Sleep(200);      // Allow time the playerwatcher to find played channel
            }
            _log.Information("SongInfoListener - Invoked");

            Task.Run(async () =>
            {
                string lastLoopSongId = "-1";
                bool isPreviousLoopCanceled = false;
                while (!listenerCancellationTokenSource.Token.IsCancellationRequested)     // keep getting new song info
                {
                    try
                    {

                        NextSongWaiterCancellationTokenSource = new CancellationTokenSource();
                        var cancellationToken = NextSongWaiterCancellationTokenSource.Token;

                        CheckTrackedPlayerStatus();
                        UpdateSongInfo();

                        var extendedShowOnNewSong = _config.ExternalConfig.ShowOnNewSong
                            || _config.State.Foobar2000IsPlayingRP
                            || !_config.State.RpTrackingConfig.ValidateActivePlayerId();

                        if ((extendedShowOnNewSong && lastLoopSongId != _config.State.Playback.SongInfo.SongId)
                            || isPreviousLoopCanceled)
                        {
                            _toastHandler.ShowSongStartToast();
                            isPreviousLoopCanceled = false;
                        }

                        lastLoopSongId = _config.State.Playback.SongInfo.SongId;

                        var timeLeftFromSongMilliseconds = (int)(_config.State.Playback.SongInfoExpiration - DateTime.Now).TotalMilliseconds;
                        try
                        {
                            var loopWaitMilliseconds = _config.State.RpTrackingConfig.ValidateActivePlayerId()
                                ? Math.Min(7000, timeLeftFromSongMilliseconds)
                                : timeLeftFromSongMilliseconds;
                            var waitForNextSong = Task.Delay(loopWaitMilliseconds, cancellationToken);

                            while (!waitForNextSong.IsCompleted && !cancellationToken.IsCancellationRequested)
                            {
                                SendUpdateTooltipEvent();
                                PromptRatingAtEndOfSongOrIfCanceled();
                                await Task.Delay(1000, cancellationToken);
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            isPreviousLoopCanceled = true;
                            PromptRatingAtEndOfSongOrIfCanceled(true);
                            _log.Information("SongInfoListener - Canceled");
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error($"SongInfoListener - Loop breaking inner ERROR - {e.Message}");
                        _toastHandler.SongInfoListenerError();
                        await Task.Delay(10000);
                        Application.Restart();
                    }
                }
            }, listenerCancellationTokenSource.Token);

            _log.Information("SongInfoListener - Running in background");
        }

        private void PromptRatingAtEndOfSongOrIfCanceled(bool isCancellationRequested = false)
        {
            if (_config.ExternalConfig.ShowOnNewSong)
            {
                var triggerSecondsBeforeSongEnds = 20;
                var secondsLeft = Math.Abs((DateTime.Now - _config.State.Playback.SongInfoExpiration).TotalSeconds);

                bool alreadyRated = Int32.TryParse(_config.State.Playback.SongInfo.UserRating, out int userRating)
                    && userRating > 0;
                bool closeToEndOfSong = triggerSecondsBeforeSongEnds > secondsLeft
                    && secondsLeft > (triggerSecondsBeforeSongEnds - 1);
                bool canceledBeforePrompting = isCancellationRequested && secondsLeft > triggerSecondsBeforeSongEnds;

                if ((!_config.ExternalConfig.PromptForRating || alreadyRated)
                    && (closeToEndOfSong || canceledBeforePrompting))
                {
                    _toastHandler.ShowSongRatingToast();
                }
            }
        }

        private void SendUpdateTooltipEvent()
        {
            //Update tooltip
            var timeLeft = (DateTime.Now - _config.State.Playback.SongInfoExpiration).ToString(@"m\:ss");
            string chanTitle = _config.State.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.ExternalConfig.Channel).First().Title;
            var trayIconText = $"{_config.State.Playback.SongInfo.Artist}\n{_config.State.Playback.SongInfo.Title}\n-{timeLeft}\n{chanTitle}";
            trayIconText = trayIconText.Length <= 64
                ? trayIconText
                : trayIconText.Substring(0, 63);
            TooltipUpdateChangedEventHandler?
                .Invoke(this, new TooltipChangeEventArgs(trayIconText));
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
                    ConfigChangedEventHandler?
                        .Invoke(this,
                            new ConfigChangeEventArgs()
                            {
                                ChannelChanged = true
                            });
                }
            }
        }

        public void UpdateSongInfo()
        {
            string player_id = _config.State.RpTrackingConfig.ValidateActivePlayerId()
                ? _config.State.RpTrackingConfig.ActivePlayerId
                : null;

            var logMessageDetail = !string.IsNullOrEmpty(player_id)
                ? $"Channel: {_config.ExternalConfig.Channel.ToString()}"
                : $"Player_ID: {player_id}";

            _log.Information($"UpdateSongInfo - Invoked - {logMessageDetail}");

            var playback = new Playback(_apiHandler.GetNowplayingList(_config.ExternalConfig.Channel.ToString(), player_id));

            _log.Information("UpdateSongInfo - RP API call returned successfully - SongId: {@songId}", playback.SongInfo.SongId);

            // Update class attributes
            if (_config.State.TryUpdatePlayback(playback))
            {
                _log.Information("UpdateSongInfo - New song - Start downloading album art - Song info: {@Songdata}", playback.SongInfo);

                // Download album art
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri($"{_config.InternalConfig.RpImageBaseUrl}/{playback.SongInfo.Cover}"), _config.InternalConfig.AlbumArtImagePath);
                }
                _log.Information("UpdateSongInfo - Albumart downloaded - Song expires: {@RefreshTimestamp} ({ExpirySeconds} seconds)", playback.SongInfoExpiration.ToString(), playback.NowplayingList.Refresh);
            }
            else
            {
                _log.Information("UpdateSongInfo - Same song: albumart and expiration is not updated");
            }

            _log.Information("UpdateSongInfo - Finished");
        }
    }

    public class TooltipChangeEventArgs : EventArgs
    {
        public string ToolTipText { get; set; }
        public TooltipChangeEventArgs(string tooltipText)
        {
            ToolTipText = tooltipText;
        }
    }
}
