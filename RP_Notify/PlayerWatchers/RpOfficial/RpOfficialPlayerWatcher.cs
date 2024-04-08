using RP_Notify.Config;
using RP_Notify.Helpers;
using RP_Notify.Logger;
using RP_Notify.RpApi;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP_Notify.PlayerWatchers.RpOfficial
{
    internal class RpOfficialPlayerWatcher : IPlayerWatcher
    {
        private readonly IConfigRoot _config;
        private readonly ILoggerWrapper _log;
        private readonly IRpApiClientFactory _rpApiClientFactory;

        private int CheckDelayMillisecs { get; set; }
        private Task RpOfficialWatcherTask { get; set; }
        public RegisteredPlayer PlayerWatcherType { get => RegisteredPlayer.RpOfficial; }

        private CancellationTokenSource RpOfficialWatcherTaskCancellationTokenSource { get; set; }

        public RpOfficialPlayerWatcher(IConfigRoot config, ILoggerWrapper log, IRpApiClientFactory rpApiClientFactory)
        {
            _config = config;
            _log = log;
            _rpApiClientFactory = rpApiClientFactory;

            Init();
        }

        private void Init()
        {
            CheckDelayMillisecs = 10000;
            RpOfficialWatcherTaskCancellationTokenSource = new CancellationTokenSource();
            Application.ApplicationExit += (sender, e) => RpOfficialWatcherTaskCancellationTokenSource.Cancel();
        }

        public void Stop()
        {
            if (IsRpOfficialWatcherTaskRunning())
            {
                _log.Information(LogHelper.GetMethodName(this), $"Shutdown initiated");
                RpOfficialWatcherTaskCancellationTokenSource.Cancel();
            }
            else
            {
                _log.Information(LogHelper.GetMethodName(this), $"Not running");
            }
        }

        public void Start()
        {
            if (!IsRpOfficialWatcherTaskRunning())
            {
                _log.Information(LogHelper.GetMethodName(this), $"Invoked");
                Run();
            }
            else
            {
                _log.Information(LogHelper.GetMethodName(this), $"Alreay running");
            }
        }

        private void Run()
        {
            _log.Information(LogHelper.GetMethodName(this), $"Starting");

            RpOfficialWatcherTaskCancellationTokenSource = new CancellationTokenSource();
            RpOfficialWatcherTask = Task.Run(async () =>
            {

                while (!RpOfficialWatcherTaskCancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        CheckPlayerState(out bool _);

                        await Task.Delay(CheckDelayMillisecs);
                    }
                    catch (TaskCanceledException)
                    {
                        // Don't log error for stopping
                    }
                    catch (Exception ex)
                    {
                        _log.Error(LogHelper.GetMethodName(this), ex);
                        Task.Delay(10000).Wait();
                        Application.Exit();
                    }
                }

                _log.Information(LogHelper.GetMethodName(this), $"Stopped");

            }, RpOfficialWatcherTaskCancellationTokenSource.Token);

            _log.Information(LogHelper.GetMethodName(this), $"Running in background");
        }

        public bool CheckPlayerState(out bool channelChanged)
        {
            channelChanged = false;

            _log.Information(LogHelper.GetMethodName(this), "Refresh available players");

            _config.State.RpTrackingConfig.Players = _rpApiClientFactory.Create().GetSync_v2().Players;

            if (_config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int trackedChannel))
            {
                if (_config.PersistedConfig.Channel != trackedChannel)
                {
                    channelChanged = true;
                    _log.Information(LogHelper.GetMethodName(this), $"Tracking is active - Tracked channel: {trackedChannel}");

                    _config.PersistedConfig.Channel = trackedChannel;
                }

                return true;
            }

            return false;
        }

        private bool IsRpOfficialWatcherTaskRunning()
        {
            return RpOfficialWatcherTask != null && !RpOfficialWatcherTask.IsCompleted;
        }
    }
}