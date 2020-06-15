using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.PlayerWatcher.MusicBee.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP_Notify.PlayerWatcher.MusicBee
{
    class MusicBeeWatcher : IPlayerWatcher
    {
        private readonly IConfig _config;
        private readonly ILog _log;
        private readonly MusicBeeIPC _playerApi;

        private int CheckDelayMillisecs { get; set; }
        private Task MusicBeeWatcherTask { get; set; }
        private CancellationTokenSource MusicBeeWatcherTaskCancellationTokenSource { get; set; }

        public MusicBeeWatcher(IConfig config, ILog log, MusicBeeIPC playerApi)
        {
            _config = config;
            _log = log;
            _playerApi = playerApi;

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
                        CheckPlayerState(out bool notUsedHere);

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

            if (_config.ExternalConfig.EnableMusicBeeWatcher
                && RpChannelIsPlayingInMusicBee(out int matchingChannel))
            {
                _config.State.MusicBeeIsPlayingRP = true;

                CheckDelayMillisecs = 1000;

                if (matchingChannel != _config.ExternalConfig.Channel
                    && !_config.IsRpPlayerTrackingChannel())
                {
                    channelChanged = true;
                    _log.Information(LogHelper.GetMethodName(this), $"Channel change detected");
                    _config.ExternalConfig.Channel = matchingChannel;
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
                && _playerApi.GetPlayState() == MusicBeeIPC.PlayState.Playing)
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
                if (!_playerApi.Probe())
                {
                    return false;
                }

                playedFilePath = _playerApi.GetFileUrl();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
