using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.RpApi;
using RP_Notify.RpApi.ResponseModel;
using RP_Notify.ToastHandler;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP_Notify.SongInfoListener
{
    class SongInfoListener : ISongInfoListener
    {
        private readonly IRpApiClientFactory _rpApiClientFactory;
        private readonly IConfigRoot _config;
        private readonly ILog _log;
        private readonly IToastHandlerFactory _toastHandlerFactory;

        private const int secondsBeforeSongEndsToPromptRating = 20;

        private Task SongInfoListenerTask { get; set; }
        private CancellationTokenSource NextSongWaiterCancellationTokenSource { get; set; }
        private CancellationTokenSource ListenerCancellationTokenSource { get; }

        public SongInfoListener(IRpApiClientFactory rpApiClientFactory, IConfigRoot config, ILog log, IToastHandlerFactory toastHandlerFactory)
        {
            _rpApiClientFactory = rpApiClientFactory;
            _config = config;
            _log = log;
            _toastHandlerFactory = toastHandlerFactory;

            NextSongWaiterCancellationTokenSource = new CancellationTokenSource();
            ListenerCancellationTokenSource = new CancellationTokenSource();

            Application.ApplicationExit += (sender, e) => ListenerCancellationTokenSource.Cancel();
        }

        public void ResetListenerLoop()
        {
            Task.Run(() => { NextSongWaiterCancellationTokenSource.Cancel(); });
        }

        public void Start()
        {
            if (SongInfoListenerTask == null || SongInfoListenerTask.IsCompleted)
            {
                _log.Information(LogHelper.GetMethodName(this), "Invoked");
                Run();
            }
            else
            {
                _log.Information(LogHelper.GetMethodName(this), "Alreay running");
            }
        }

        private void Run()
        {
            _log.Information(LogHelper.GetMethodName(this), "Invoked");

            SongInfoListenerTask = Task.Run(async () =>
            {
                while (!ListenerCancellationTokenSource.Token.IsCancellationRequested)     // keep getting new song info
                {
                    _log.Information(LogHelper.GetMethodName(this), "Start loop *****************");

                    try
                    {
                        NextSongWaiterCancellationTokenSource = new CancellationTokenSource();
                        var loopCancellationToken = NextSongWaiterCancellationTokenSource.Token;

                        try
                        {
                            Task update = Task.Run(() =>
                            {
                                CheckTrackedRpPlayerStatus();
                                UpdateSongInfo();
                            }, loopCancellationToken);

                            // if an RP player is tracked then wait for the update to finish only
                            // at the end of the loop in order to don't potentially skip the rating prompt
                            // while checking song data
                            // in other cases it would cause unnecessary double API calls
                            if (_config.State.Playback == null
                                || !_config.IsRpPlayerTrackingChannel())
                            {
                                update.Wait();
                            }

                            var timeLeftFromSongMilliseconds = (int)(_config.State.Playback.SongInfoExpiration - DateTime.Now).TotalMilliseconds;

                            var loopDurationMilliseconds = _config.IsRpPlayerTrackingChannel()  // If tracking an RP official player
                                ? Math.Min(10000, timeLeftFromSongMilliseconds)                 // then check often
                                : timeLeftFromSongMilliseconds;                                 // otherwise check only at the end of the song

                            loopDurationMilliseconds = loopDurationMilliseconds > 0             // If the song is not over
                                ? loopDurationMilliseconds                                      // then wait until the end
                                : 10000;                                                         // otherwise csheck again in 10 seconds if William or ALanna is still talking

                            var waitForNextSongTask = Task.Delay(loopDurationMilliseconds, loopCancellationToken);

                            while (!waitForNextSongTask.IsCompleted && !loopCancellationToken.IsCancellationRequested)
                            {
                                var RefreshDelay = Task.Delay(1000, loopCancellationToken);
                                SendUpdateTooltipEvent();
                                PromptRatingAtTheEndOfSongOrIfCanceled();
                                await RefreshDelay;
                            }
                            await update;
                        }
                        catch (TaskCanceledException)
                        {
                            PromptRatingAtTheEndOfSongOrIfCanceled();
                            _log.Information(LogHelper.GetMethodName(this), "Restart loop");
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(LogHelper.GetMethodName(this), ex);
                        _toastHandlerFactory.Create().ErrorToast(ex);
                        Task.Delay(10000).Wait();
                        Application.Exit();
                    }
                }
            }, ListenerCancellationTokenSource.Token);

            _log.Information(LogHelper.GetMethodName(this), "Running in background");
        }

        public void CheckTrackedRpPlayerStatus()
        {
            _log.Information(LogHelper.GetMethodName(this), $"EnableRpOfficialTracking = [{_config.ExternalConfig.EnableRpOfficialTracking}]");
            if (_config.ExternalConfig.EnableRpOfficialTracking)
            {
                _log.Information(LogHelper.GetMethodName(this), "Refresh available players");

                _config.State.RpTrackingConfig.Players = _rpApiClientFactory.Create().GetSync_v2().Players;

                if (_config.IsRpPlayerTrackingChannel(out int trackedChannel) && _config.ExternalConfig.Channel != trackedChannel)
                {
                    _log.Information(LogHelper.GetMethodName(this), $"Tracking is active - Tracked channel: {trackedChannel}");

                    _config.ExternalConfig.Channel = trackedChannel;
                }
            }
        }

        private void PromptRatingAtTheEndOfSongOrIfCanceled()
        {
            bool somethingIsPlaying = _config.ExternalConfig.ShowOnNewSong
                || _config.State.Foobar2000IsPlayingRP
                || _config.State.MusicBeeIsPlayingRP
                || _config.IsRpPlayerTrackingChannel();

            var millisecsLeftToPrompt = (int)(_config.State.Playback.SongInfoExpiration - DateTime.Now).TotalMilliseconds - secondsBeforeSongEndsToPromptRating * 1000;

            if (_config.ExternalConfig.PromptForRating
                && string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating)
                && somethingIsPlaying)
            {
                if (0 < millisecsLeftToPrompt && millisecsLeftToPrompt < 1000)
                {
                    _log.Information(LogHelper.GetMethodName(this), "Prompt at the end of song");
                }
                else if (1000 < millisecsLeftToPrompt && NextSongWaiterCancellationTokenSource.IsCancellationRequested)
                {
                    _log.Information(LogHelper.GetMethodName(this), "Prompt due to song cancellation");
                }
                else
                {
                    return;
                }
                _toastHandlerFactory.Create().ShowSongRatingToast();
            }
        }

        private void SendUpdateTooltipEvent()
        {
            var timeLeft = DateTime.Now - _config.State.Playback.SongInfoExpiration;
            var timeLeftString = timeLeft.ToString(@"m\:ss");
            var timeleftPrefix = timeLeft.TotalSeconds < 0
                ? "-"
                : "";
            string chanTitle = _config.State.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.ExternalConfig.Channel).First().Title;

            var trackInfoString = $"{_config.State.Playback.SongInfo.Artist}\n{_config.State.Playback.SongInfo.Title}";
            var timeAndChannelString = $"\n{timeleftPrefix}{timeLeftString}\n{chanTitle}";

            trackInfoString = trackInfoString.Length <= (63 - timeAndChannelString.Length)
                ? trackInfoString
                : trackInfoString.Substring(0, 62 - timeAndChannelString.Length);

            var trayIconText = $"{trackInfoString}{timeAndChannelString}";

            _config.State.TooltipText = trayIconText;
        }

        private void UpdateSongInfo()
        {
            string player_id = _config.IsRpPlayerTrackingChannel()
                ? _config.State.RpTrackingConfig.ActivePlayerId
                : null;

            var logMessageDetail = !string.IsNullOrEmpty(player_id)
                ? $"RP Player ID: [{player_id}]"
                : $"Channel: [{_config.ExternalConfig.Channel}]";

            _log.Information(LogHelper.GetMethodName(this), $"Invoked - Get song info for {logMessageDetail}");

            var oldPlayback = _config.State.Playback;
            var nowPlayingList = _rpApiClientFactory.Create().GetNowplayingList();

            if (_config.ExternalConfig.Channel == 2050 &&
                (nowPlayingList.Song == null
                    || !nowPlayingList.Song.TryGetValue("0", out var nowPlayingSong)
                    || DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(nowPlayingSong.SchedTime + "000") + long.Parse(nowPlayingSong.Duration)).LocalDateTime < DateTime.Now)
                )
            {
                var channelImageUrl = _config.State.ChannelList.Where(c => c.Chan == "2050").First().Image;
                var formattedChannelImageUrl = channelImageUrl.Substring(_config.StaticConfig.RpImageBaseUrl.Length + 1);

                nowPlayingList.Song = new System.Collections.Generic.Dictionary<string, PlayListSong>
                {
                    { "0", new PlayListSong()
                        {
                            SongId = "2050",
                            SchedTime = "0000000000",
                            Duration = "000000",
                            UserRating = null,
                            Artist = "Radio Paradise",
                            Title = "2050",
                            Album = "Podcast discussion",
                            Year = "2050",
                            Cover = formattedChannelImageUrl,
                            Rating = "",
                            Elapsed = 0
                        }
                    }
                };
            }

            _config.State.Playback = new Playback(nowPlayingList);

            _log.Information(LogHelper.GetMethodName(this), "RP API call returned successfully - SongId: {@songId}", _config.State.Playback.SongInfo.SongId);

            // Update class attributes
            if (oldPlayback == null
                || _config.State.Playback.SongInfo.SongId != oldPlayback.SongInfo.SongId)
            {
                if (_config.State.Playback.SameSongOnlyInternalUpdate)
                {
                    var newRatinText = _config.State.Playback.SongInfo.Rating != oldPlayback.SongInfo.Rating
                        ? $" - New rating: {_config.State.Playback.SongInfo.Rating}"
                        : null;
                    var newExpirationText = _config.State.Playback.SongInfoExpiration != _config.State.Playback.SongInfoExpiration
                        ? $" - New expiration: {_config.State.Playback.SongInfoExpiration}"
                        : null;

                    _log.Information(LogHelper.GetMethodName(this), $"Same song - Only properties changed{newRatinText}{newExpirationText}");
                }
            }
            else
            {
                _log.Information(LogHelper.GetMethodName(this), $"Same song: albumart and expiration is not updated - Seconds left: {nowPlayingList.Refresh}");
            }

            _log.Information(LogHelper.GetMethodName(this), "Finished");
        }
    }
}
