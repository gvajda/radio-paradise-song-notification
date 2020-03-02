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

        public CancellationTokenSource nextSongWaiterCancellationTokenSource { get; set; }

        public SongInfoListener(IRpApiHandler apiHandler, IConfig config, IToastHandler toastHandler, ILog log)
        {
            _apiHandler = apiHandler;
            _config = config;
            _toastHandler = toastHandler;
            _log = log.Logger;

            nextSongWaiterCancellationTokenSource = new CancellationTokenSource();
            listenerCancellationTokenSource = new CancellationTokenSource();


            Application.ApplicationExit += (sender, e) => listenerCancellationTokenSource.Cancel();
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
                string lastLoopSongId = "1";
                bool configChanged = false;
                while (!listenerCancellationTokenSource.Token.IsCancellationRequested)     // keep getting new song info
                {
                    try
                    {

                        nextSongWaiterCancellationTokenSource = new CancellationTokenSource();
                        var cancellationToken = nextSongWaiterCancellationTokenSource.Token;

                        CheckTrackedPlayerStatus();
                        _apiHandler.UpdateSongInfo();

                        if ((_config.ShowOnNewSong && lastLoopSongId != _apiHandler.SongInfo.SongId)
                            || configChanged)
                        {
                            _toastHandler.ShowSongStartToast();
                            configChanged = false;
                        }

                        lastLoopSongId = _apiHandler.SongInfo.SongId;

                        var timeLeftFromSongMilliseconds = (int)(_apiHandler.SongInfoExpiration - DateTime.Now).TotalMilliseconds;
                        try
                        {
                            var loopWaitMilliseconds = _config.RpTrackingConfig.IsValidPlayerId()
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
                            configChanged = true;
                            PromptRatingAtEndOfSongOrIfCanceled(true);
                            _log.Information("SongInfoListener - External configuration change");
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
                try
                {
                    if (!_config.PromptForRating
                        || (Int32.TryParse(_apiHandler.SongInfo.UserRating, out int userRating) && userRating > 0)
                        || secondsLeft < (triggerSecondsBeforeSongEnds - 1)
                        || (!isCancellationRequested && secondsLeft > triggerSecondsBeforeSongEnds))
                    {
                        return;
                    }

                    _toastHandler.ShowSongRatingToast();
                }
                catch (Exception e)
                {
                    _log.Error(e.Message);
                }
            }
        }

        private void SendUpdateTooltipEvent()
        {
            //Update tooltip
            var timeLeft = (DateTime.Now - _apiHandler.SongInfoExpiration).ToString(@"m\:ss");
            string chanTitle = _apiHandler.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.Channel).First().Title;
            var trayIconText = $"{_apiHandler.SongInfo.Artist}\n{_apiHandler.SongInfo.Title}\n-{timeLeft}\n{chanTitle}";
            trayIconText = trayIconText.Length < 64 ? trayIconText : trayIconText.Substring(0, 63);
            TooltipUpdateChangedEventHandler?
                .Invoke(this, new TooltipChangeEventArgs() { ToolTipText = trayIconText });
        }

        private void CheckTrackedPlayerStatus()
        {
            if (_config.RpTrackingConfig.Enabled)
            {
                _log.Information("-- SongInfoListener - CheckTrackedPlayerStatus: Enabled");
                _config.RpTrackingConfig.Players = _apiHandler.GetSync_v2().Players;

                var channelChanged = false;
                if (_config.RpTrackingConfig.IsValidPlayerId())
                {
                    _log.Information("-- SongInfoListener - CheckTrackedPlayerStatus: Player is active");
                    var currentChannel = Int32.Parse(
                        _config.RpTrackingConfig.Players
                        .Where(p => p.PlayerId == _config.RpTrackingConfig.ActivePlayerId)
                        .First()
                        .Chan
                        );
                    if (_config.Channel != currentChannel)
                    {
                        _config.Channel = currentChannel;
                        channelChanged = true;
                    }
                }

                if (channelChanged)
                {
                    ConfigChangedEventHandler?
                        .Invoke(this,
                            new ConfigChangeEventArgs()
                            {
                                ChannelChanged = channelChanged
                            });
                }
            }
        }
    }

    public class TooltipChangeEventArgs : EventArgs
    {
        public TooltipChangeEventArgs()
        {
        }
        public string ToolTipText { get; set; }
    }
}
