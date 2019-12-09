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

                        _apiHandler.UpdateSongInfo();

                        if ((_config.ShowOnNewSong && lastLoopSongId != _apiHandler.SongInfo.SongId)
                            || configChanged)
                        {
                            _toastHandler.ShowSongStartToast();
                            configChanged = false;
                        }

                        lastLoopSongId = _apiHandler.SongInfo.SongId;

                        try
                        {
                            var waitForNextSong = Task.Delay((int)(_apiHandler.SongInfoExpiration - DateTime.Now).TotalMilliseconds, cancellationToken);
                            var song = _apiHandler.SongInfo;
                            while (!waitForNextSong.IsCompleted && !cancellationToken.IsCancellationRequested)
                            {
                                SendUpdateTooltipEvent();
                                await Task.Delay(1000, cancellationToken);
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            configChanged = true;
                            _log.Information("SongInfoListener - External configuration change");
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error($"SongInfoListener - Loop breaking inner ERROR - {e.Message}");
                        _toastHandler.SongInfoListenerError();
                        await Task.Delay(10000, listenerCancellationTokenSource.Token);
                    }
                }
            }, listenerCancellationTokenSource.Token);

            _log.Information("SongInfoListener - Running in background");
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
    }

    public class TooltipChangeEventArgs : EventArgs
    {
        public TooltipChangeEventArgs()
        {
        }
        public string ToolTipText { get; set; }
    }
}
