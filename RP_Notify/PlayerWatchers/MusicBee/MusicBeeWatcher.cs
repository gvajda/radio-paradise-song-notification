using RP_Notify.Config;
using RP_Notify.Helpers;
using RP_Notify.Logger;
using RP_Notify.PlayerWatchers.MusicBee.API;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP_Notify.PlayerWatchers.MusicBee
{
    class MusicBeeWatcher : IPlayerWatcher
    {
        private readonly IConfigRoot _config;
        private readonly ILoggerWrapper _log;
        private readonly IMusicBeeIPCFactory _musicBeeIPCFactory;

        private int CheckDelayMillisecs { get; set; }
        private Task MusicBeeWatcherTask { get; set; }
        private CancellationTokenSource MusicBeeWatcherTaskCancellationTokenSource { get; set; }
        public RegisteredPlayer PlayerWatcherType { get => RegisteredPlayer.MusicBee; }

        public MusicBeeWatcher(IConfigRoot config, ILoggerWrapper log, IMusicBeeIPCFactory musicBeeIPCFactory)
        {
            _config = config;
            _log = log;
            _musicBeeIPCFactory = musicBeeIPCFactory;

            Init();
        }

        private void Init()
        {
            CheckDelayMillisecs = 5000;
            MusicBeeWatcherTaskCancellationTokenSource = new CancellationTokenSource();
            Application.ApplicationExit += (sender, e) => MusicBeeWatcherTaskCancellationTokenSource.Cancel();
        }

        public void Stop()
        {
            if (IsMusicBeeWatcherTaskRunning())
            {
                _log.Information(LogHelper.GetMethodName(this), $"Shutdown initiated");
                MusicBeeWatcherTaskCancellationTokenSource.Cancel();
            }
            else
            {
                _log.Information(LogHelper.GetMethodName(this), $"Not running");
            }
        }

        public void Start()
        {
            if (!IsMusicBeeWatcherTaskRunning())
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

            MusicBeeWatcherTaskCancellationTokenSource = new CancellationTokenSource();

            MusicBeeWatcherTask = Task.Run(async () =>
            {
                while (!MusicBeeWatcherTaskCancellationTokenSource.IsCancellationRequested)
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

                _config.State.MusicBeeIsPlayingRP = false;

                _log.Information(LogHelper.GetMethodName(this), $"Stopped");

            }, MusicBeeWatcherTaskCancellationTokenSource.Token);

            _log.Information(LogHelper.GetMethodName(this), $"Running in background");
        }

        public bool CheckPlayerState(out bool channelChanged)
        {
            channelChanged = false;

            if (_config.PersistedConfig.EnableMusicBeeWatcher
                && RpChannelIsPlayingInMusicBee(out int matchingChannel))
            {
                _config.State.MusicBeeIsPlayingRP = true;

                CheckDelayMillisecs = 1000;

                if (matchingChannel != _config.PersistedConfig.Channel
                    && !_config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _))
                {
                    channelChanged = true;
                    _log.Information(LogHelper.GetMethodName(this), $"Channel change detected");
                    _config.PersistedConfig.Channel = matchingChannel;
                }
                return true;
            }
            else
            {
                CheckDelayMillisecs = 5000;
                _config.State.MusicBeeIsPlayingRP = false;
                return false;
            }
        }

        private bool IsMusicBeeWatcherTaskRunning()
        {
            return !(MusicBeeWatcherTask == null || MusicBeeWatcherTask.IsCompleted);
        }

        private bool RpChannelIsPlayingInMusicBee(out int matchingChannel)
        {
            if (TryGetPlayedFilePath(out string playedFilePath)
                && playedFilePath.Contains("radioparadise")
                && _musicBeeIPCFactory.Create().GetPlayState() == MusicBeeIPC.PlayState.Playing)
            {
                matchingChannel = Int32.Parse(
                    _config.State.ChannelList
                        .Where(channel => playedFilePath.Contains(channel.StreamName))
                        .DefaultIfEmpty(_config.State.ChannelList.First())
                        .FirstOrDefault()
                        .Chan
                );

                return true;
            }
            else
            {
                matchingChannel = -1;
                return false;
            }
        }

        private bool TryGetPlayedFilePath(out string playedFilePath)
        {
            playedFilePath = null;


            try
            {
                if (!_musicBeeIPCFactory.Create().Probe())
                {
                    return false;
                }

                playedFilePath = _musicBeeIPCFactory.Create().GetFileUrl();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
