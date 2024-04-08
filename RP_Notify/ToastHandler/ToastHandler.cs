using Microsoft.Toolkit.Uwp.Notifications;
using RP_Notify.Config;
using RP_Notify.Helpers;
using RP_Notify.Logger;
using RP_Notify.RpApi.ResponseModel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RP_Notify.ToastHandler
{
    internal class ToastHandler : IToastHandler
    {
        private readonly IConfigRoot _config;
        private readonly ILoggerWrapper _log;

        public ToastHandler(IConfigRoot config, ILoggerWrapper log)
        {
            _config = config;
            _log = log;
        }

        public void ShowSongStartToast(bool force = false, PlayListSong songInfo = null)
        {
            // don't run without song info
            if (_config.State.Playback == null)
            {
                return;
            }

            // if force invocation or something is tracked
            if (!(
                force
                || (_config.PersistedConfig.ShowOnNewSong && !_config.State.Playback.ShowedOnNewSong)
                    || _config.State.Foobar2000IsPlayingRP
                    || _config.State.MusicBeeIsPlayingRP
                    || _config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _)
                ))
            {
                return;
            }

            var displaySongInfo = songInfo != null
                ? songInfo
                : _config.State.Playback.SongInfo;

            Task.Run(() =>
            {
                try
                {
                    ToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowSongStartToast), displaySongInfo)
                    .AddSongInfoText(true)
                    .AddSongAlbumText()
                    .AddSongRatingText(_config)
                    .AddSongFooterText(_config)
                    .AddSongAlbumArt(_config, false)
                    .Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });

            _config.State.Playback.ShowedOnNewSong = true;
        }

        public void ShowSongRatingToast(PlayListSong songInfo = null)
        {
            var displaySongInfo = songInfo != null
                ? songInfo
                : _config.State.Playback.SongInfo;
            Task.Run(() =>
            {
                try
                {
                    ToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowSongRatingToast), displaySongInfo)
                    .AddSongInfoText(true)
                    .AddSongAlbumText()
                    .AddSongRatingText(_config)
                    .AddSongFooterText(_config)
                    .AddSongAlbumArt(_config, false)
                    .AddSongInputActions(_config)
                    .Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });
        }

        public void ShowInvalidRatingArgumentToast(string invalidinput)
        {
            Task.Run(() =>
            {
                try
                {
                    ToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowConfigFolderChoicePromptToast))
                    .AddText($"Invalid user rating [{invalidinput}]")
                    .AddText("The rating value must be a number between 1-10")
                    .AddText("Please try again")
                    .Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });
        }

        public void ShowSongDetailToast()
        {
            try
            {
                ToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowSongDetailToast), _config.State.Playback.SongInfo)
                .AddSongInfoText(true)
                .AddSongAlbumText()
                .AddSongRatingText(_config)
                .AddSongFooterText(_config)
                .AddSongAlbumArt(_config, true)
                .AddSongProgressBar(_config)
                .AddSongInputActions(_config)
                .Show();
            }
            catch (Exception ex)
            {
                _log.Error(LogHelper.GetMethodName(this), ex);
            }
        }

        public void ShowConfigFolderChoicePromptToast()
        {
            Task.Run(() =>
            {
                try
                {
                    ToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowConfigFolderChoicePromptToast))
                    .AddText("You probably opened RP_Notify for the first time")
                    .AddText($"You have a few options where to create the folder (named '{Constants.ObsoleteConfigBaseFolder}') where the app may keep its configuration file, logs, etc. You only need to do this once - unless you choose 'clean up")
                    .AddText($"This action will create the '{Constants.ConfigBaseFolder}' folder")
                    .AddToastInput(new ToastSelectionBox(nameof(ConfigFolderChoiceOption))
                    {
                        DefaultSelectionBoxItemId = ConfigFolderChoiceOption.localCache.ToDescriptionString(),
                        Items =
                            {
                                new ToastSelectionBoxItem(ConfigFolderChoiceOption.localCache.ToDescriptionString(), "Next to the application (RP_Notify.exe)"),
                                new ToastSelectionBoxItem(ConfigFolderChoiceOption.appdata.ToDescriptionString(), $"C:\\users\\YOURNAME\\.AppData\\Roaming\\{Constants.ConfigBaseFolder}"),
                                new ToastSelectionBoxItem(ConfigFolderChoiceOption.cleanonexit.ToDescriptionString(), "Don't keep anything (clean up on exit)")
                            }
                    })
                    .AddButton(new ToastButton()
                        .SetContent("Choose")
                        .AddArgument(nameof(RpToastUserAction), RpToastUserAction.ConfigFolderChosen))
                    .AddButton(new ToastButton()
                        .SetContent("Exit App")
                        .AddArgument(nameof(RpToastUserAction), RpToastUserAction.FolderChoiceRefusedExitApp))
                    .SetToastDuration(ToastDuration.Long)
                    .Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });
        }

        public void ShowLoginResponseToast(Auth authResp)
        {
            string formattedStatusMessage = $"User authentication call [{authResp.Status}]";
            string authMessage = $@"{(authResp.Status == "success"
                ? $"Welcome {authResp.Username}!"
                : "Please try again")}";

            Task.Run(() =>
            {
                try
                {
                    ToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowLoginResponseToast))
                    .AddText(authMessage)
                    .AddText(formattedStatusMessage)
                    .Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });
        }

        public void ShowLogoutRequestToast(string userName)
        {
            Task.Run(() =>
            {
                try
                {
                    ToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowLogoutRequestToast))
                    .AddText($"Farewell {userName}")
                    .AddText("The web cookie storing your login information has been deleted from both the app data directory and the app's memory.")
                    .Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });
        }

        public void ShowDataEraseToast()
        {
            Task.Run(() =>
            {
                try
                {
                    ToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowDataEraseToast))
                    .AddText("Application Data Erase Requested")
                    .AddText($"Deleting RP_Notify folder from [{_config.StaticConfig.ConfigBaseFolderOption}]")
                    .AddText("Deleting notification handler")
                    .Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });
        }

        public void ShowErrorToast(Exception exception)
        {
            Task.Run(() =>
            {
                try
                {
                    var toastBuilder = new ToastContentBuilder()
                    .AddArgument(nameof(this.ShowDataEraseToast))
                    .AddText(exception.Message);

                    if (exception.InnerException != null)
                    {
                        toastBuilder.AddText(exception.InnerException.Message);
                    }

                    toastBuilder.Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });
        }
    }

    internal static class ToastHelper
    {
        internal static ToastContentBuilder CreateBaseToastContentBuilder(string toastId, PlayListSong songInfo = null)
        {
            var toastContentBuilder = new ToastContentBuilder()
                .AddArgument(toastId)
                .AddAudio(new ToastAudio()
                {
                    Silent = true,
                });

            if (songInfo != null)
            {
                var serializedSongInfo = ObjectSerializer.SerializeToBase64(songInfo);

                toastContentBuilder.Content.Actions = new ToastActionsCustom()
                {
                    ContextMenuItems = {
                    new ToastContextMenuItem("Display song rating tile (use from Action Center)", $"{nameof(RpToastUserAction)}={RpToastUserAction.RatingToastRequested};{nameof(PlayListSong)}={serializedSongInfo}")
                    }
                };
            }

            return toastContentBuilder;
        }

        internal static PlayListSong ExtractSonginfoObjectFromContextAction(this ToastContentBuilder toastContentBuilder)
        {

            ToastArguments args = ToastArguments
                .Parse(
                    toastContentBuilder
                        .Content
                        .Actions
                        .ContextMenuItems.FirstOrDefault()
                        .Arguments);
            var serializedSongInfo = args[nameof(PlayListSong)];

            var deserializedSongInfo = ObjectSerializer.DeserializeFromBase64<PlayListSong>(serializedSongInfo);

            return deserializedSongInfo;
        }

        internal static ToastContentBuilder AddSongInfoText(this ToastContentBuilder toastContentBuilder, bool withDuration)
        {
            var songInfo = toastContentBuilder.ExtractSonginfoObjectFromContextAction();

            string duration = $" ({TimeSpanToMinutes(Int32.Parse(songInfo.Duration))})";
            string title = $"{songInfo.Artist}\n{songInfo.Title}{(withDuration ? duration : null)}";

            toastContentBuilder.AddText(title);
            return toastContentBuilder;
        }

        internal static ToastContentBuilder AddSongAlbumText(this ToastContentBuilder toastContentBuilder)
        {
            var songInfo = toastContentBuilder.ExtractSonginfoObjectFromContextAction();

            string content = $"{songInfo.Album} ({songInfo.Year})";

            toastContentBuilder.AddText(content);
            return toastContentBuilder;
        }

        internal static ToastContentBuilder AddSongRatingText(this ToastContentBuilder toastContentBuilder, IConfigRoot _config)
        {
            var songInfo = toastContentBuilder.ExtractSonginfoObjectFromContextAction();

            bool userRated = !string.IsNullOrEmpty(songInfo.UserRating);

            var userRatingElement = userRated
                    ? $" - User rating: {songInfo.UserRating}"
                    : " - Not rated";

            string fullRatingText = $@"★ {songInfo.Rating}{(_config.IsUserAuthenticated(out string _) ? userRatingElement : null)}";

            return _config.PersistedConfig.ShowSongRating || userRated
                ? toastContentBuilder.AddText(fullRatingText)
                : toastContentBuilder;
        }

        internal static ToastContentBuilder AddSongProgressBar(this ToastContentBuilder toastContentBuilder, IConfigRoot _config)
        {
            var songInfo = toastContentBuilder.ExtractSonginfoObjectFromContextAction();

            // Construct the visuals of the toast
            int songDuration = Int32.Parse(songInfo.Duration);      // value is in milliseconds
            var mSecsLeftFromSong = (int)(_config.State.Playback.SongInfoExpiration - DateTime.Now).TotalMilliseconds;
            var elapsedMillisecs = Math.Min((songDuration - mSecsLeftFromSong + 500), songDuration);    // Corrigate rounding issues 

            string progressTitle = TimeSpanToMinutes(songDuration);
            double progressValue = (double)(songDuration - mSecsLeftFromSong) / songDuration;
            bool progressIsIndeterminate = false;
            string progressValueStringOverride = "-" + TimeSpanToMinutes(mSecsLeftFromSong);
            string progressStatus = TimeSpanToMinutes(elapsedMillisecs);

            var oldreturn = $"<progress value = '{progressValue}' title = '{TimeSpanToMinutes(songDuration)}' status = '{TimeSpanToMinutes(elapsedMillisecs)}' valueStringOverride = '-{TimeSpanToMinutes(mSecsLeftFromSong)}'/> ";

            if (IsPodcastDiscussion(_config))
            {
                return toastContentBuilder;
            }

            return toastContentBuilder.AddProgressBar(progressTitle,
                                                      progressValue,
                                                      progressIsIndeterminate,
                                                      progressValueStringOverride,
                                                      progressStatus);
        }

        internal static ToastContentBuilder AddSongAlbumArt(this ToastContentBuilder toastContentBuilder, IConfigRoot _config, bool isSongDetailToast)
        {
            var songInfo = toastContentBuilder.ExtractSonginfoObjectFromContextAction();
            var albumartFilePath = AlbumartFileHelper.DownloadAlbumartImageIfDoesntExist(_config, songInfo);

            Retry.Do(() =>
            {
                if (!File.Exists(albumartFilePath))
                {
                    throw new IOException("Cover is not yet downloaded");
                }
            }, 1000, 15);

            if (isSongDetailToast)
            {
                if (_config.PersistedConfig.LargeAlbumArt)
                {
                    toastContentBuilder.AddInlineImage(new Uri(albumartFilePath));
                }
                else
                {
                    return toastContentBuilder.AddAppLogoOverride(new Uri(albumartFilePath));
                }

                if (_config.PersistedConfig.RpBannerOnDetail)
                {
                    var rpLogoPath = Path.Combine(_config.StaticConfig.AlbumArtCacheFolder, "RP_logo.jpg");
                    if (!File.Exists(rpLogoPath))
                    {
                        Properties.Resources.RP_logo.Save(rpLogoPath);
                    }
                    toastContentBuilder.AddHeroImage(new Uri(rpLogoPath));
                }

                return toastContentBuilder;
            }
            else
            {
                return toastContentBuilder.AddAppLogoOverride(new Uri(albumartFilePath));
            }
        }

        internal static ToastContentBuilder AddSongFooterText(this ToastContentBuilder toastContentBuilder, IConfigRoot _config)
        {
            string chanTitle = _config.State.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.PersistedConfig.Channel).First().Title;

            string trimmedTitle = chanTitle.Contains("RP ")
                ? chanTitle.Split(new[] { "RP " }, StringSplitOptions.None)[1]
                : chanTitle;

            string footerText = trimmedTitle + TrackedPlayerAsSuffix(_config);

            return toastContentBuilder.AddAttributionText(footerText);
        }

        private static string TrackedPlayerAsSuffix(IConfigRoot _config)
        {
            if (_config.State.RpTrackingConfig.IsRpPlayerTrackingChannel(out int _))
            {
                var activePlayer = _config.State.RpTrackingConfig.Players
                    .First(p => p.PlayerId == _config.State.RpTrackingConfig.ActivePlayerId)
                    .Source;

                return $" • {activePlayer}";
            }

            if (_config.State.Foobar2000IsPlayingRP)
            {
                return " • Foobar2000";
            }

            if (_config.State.MusicBeeIsPlayingRP)
            {
                return " • MusicBee";
            }

            return null;
        }

        internal static ToastContentBuilder AddSongInputActions(this ToastContentBuilder toastContentBuilder, IConfigRoot _config)
        {
            var songInfo = toastContentBuilder.ExtractSonginfoObjectFromContextAction();

            var ratingHintText = !string.IsNullOrEmpty(songInfo.UserRating)
                    ? $"Current rating: {songInfo.UserRating}"
                    : "Type your rating (1-10)";

            var buttonText = (string.IsNullOrEmpty(songInfo.UserRating)
                                ? "Send"
                                : "Update")
                        + " rating";

            if (IsPodcastDiscussion(_config))
            {
                return toastContentBuilder;
            }

            return _config.IsUserAuthenticated(out string _)
                ? toastContentBuilder
                    .AddInputTextBox(Constants.UserRatingFieldKey, ratingHintText)
                    .AddButton(new ToastButton()
                        .SetContent(buttonText)
                        .SetTextBoxId(Constants.UserRatingFieldKey)
                        .AddArgument(nameof(RpToastUserAction), RpToastUserAction.RateSubmitted)
                        .AddArgument(nameof(PlayListSong), ObjectSerializer.SerializeToBase64(songInfo)))
                : toastContentBuilder
                    .AddButton(new ToastButton()
                        .SetContent("Log in to rate song")
                        .AddArgument(nameof(RpToastUserAction), RpToastUserAction.LoginRequested));
        }

        internal static string TimeSpanToMinutes(int timeLength)
        {
            TimeSpan time = (timeLength < 5000)
                ? TimeSpan.FromSeconds(timeLength)
                : TimeSpan.FromMilliseconds(timeLength);
            return time.ToString(@"m\:ss");
        }

        private static bool IsPodcastDiscussion(IConfigRoot _config)
        {
            return _config.PersistedConfig.Channel == 2050 && _config.State.Playback.SongInfoExpiration <= DateTime.Now;
        }
    }
}
