using RP_Notify.API;
using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Toast;
using Serilog;
using System;
using System.Linq;
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
            if (_config.EnablePlayerWatcher)
            {
                Thread.Sleep(200);      // Allow time the playerwatcher to find played channel
            }
            _log.Information("SongInfoListener - Invoked");

            Task.Run(async () =>
            {
                string lastLoopSongId = "-1";
                bool canceled = false;
                while (!listenerCancellationTokenSource.Token.IsCancellationRequested)     // keep getting new song info
                {
                    try
                    {

                        NextSongWaiterCancellationTokenSource = new CancellationTokenSource();
                        var cancellationToken = NextSongWaiterCancellationTokenSource.Token;

                        CheckTrackedPlayerStatus();
                        _apiHandler.UpdateSongInfo();

                        if ((_config.ShowOnNewSong && lastLoopSongId != _apiHandler.SongInfo.SongId)
                            || canceled)
                        {
                            _toastHandler.ShowSongStartToast();
                            canceled = false;
                        }

                        lastLoopSongId = _apiHandler.SongInfo.SongId;

                        var timeLeftFromSongMilliseconds = (int)(_apiHandler.SongInfoExpiration - DateTime.Now).TotalMilliseconds;
                        try
                        {
                            var loopWaitMilliseconds = _config.RpTrackingConfig.ValidateActivePlayerId()
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
                            canceled = true;
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
            if (_config.ShowOnNewSong)
            {
                var triggerSecondsBeforeSongEnds = 20;
                var secondsLeft = Math.Abs((DateTime.Now - _apiHandler.SongInfoExpiration).TotalSeconds);

                bool alreadyRated = Int32.TryParse(_apiHandler.SongInfo.UserRating, out int userRating)
                    && userRating > 0;
                bool closeToEndOfSong = triggerSecondsBeforeSongEnds > secondsLeft
                    && secondsLeft > (triggerSecondsBeforeSongEnds - 1);
                bool canceledBeforePrompting = isCancellationRequested && secondsLeft > triggerSecondsBeforeSongEnds;

                if ((!_config.PromptForRating || alreadyRated)
                    && (closeToEndOfSong || canceledBeforePrompting))
                {
                    _toastHandler.ShowSongRatingToast();
                }
            }
        }

        private void SendUpdateTooltipEvent()
        {
            //Update tooltip
            var timeLeft = (DateTime.Now - _apiHandler.SongInfoExpiration).ToString(@"m\:ss");
            string chanTitle = _apiHandler.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.Channel).First().Title;
            var trayIconText = $"{_apiHandler.SongInfo.Artist}\n{_apiHandler.SongInfo.Title}\n-{timeLeft}\n{chanTitle}";
            trayIconText = trayIconText.Length <= 64
                ? trayIconText
                : trayIconText.Substring(0, 63);
            TooltipUpdateChangedEventHandler?
                .Invoke(this, new TooltipChangeEventArgs(trayIconText));
        }

        private void CheckTrackedPlayerStatus()
        {
            if (_config.RpTrackingConfig.Enabled)
            {
                _log.Information("-- SongInfoListener - CheckTrackedPlayerStatus: Enabled");
                _config.RpTrackingConfig.Players = _apiHandler.GetSync_v2().Players;

                if (_config.RpTrackingConfig.TryGetTrackedChannel(out int currentChannel) && _config.Channel != currentChannel)
                {
                    _log.Information($"-- SongInfoListener - CheckTrackedPlayerStatus: Tracking is active - Tracked channel: {currentChannel}");

                    _config.Channel = currentChannel;
                    ConfigChangedEventHandler?
                        .Invoke(this,
                            new ConfigChangeEventArgs()
                            {
                                ChannelChanged = true
                            });
                }
            }
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
