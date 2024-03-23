using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using RP_Notify.RpApi;
using RP_Notify.RpApi.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using RP_Notify.Helpers;
using System.Web.UI.WebControls;
using System.Globalization;
using RestSharp;
using System.Web;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RP_Notify.Toast
{
    internal class ToastHandler : IToastHandler
    {
        private readonly IConfig _config;
        private readonly IRpApiHandler _apiHandler;
        private readonly ILog _log;

        public ToastHandler(IConfig config, IRpApiHandler apiHandler, ILog log)
        {
            _config = config;
            _apiHandler = apiHandler;
            _log = log;

            ToastNotificationManagerCompat.OnActivated += toastArgs => HandleToastActivatedEvent(toastArgs);
        }

        public void ShowSongStartToast(bool force = false, PlayListSong songInfo = null)
        {
            // don't run without song info
            if (_config.State.Playback == null)
            {
                return;
            }

            // force invocation or something is tracked
            if (!(
                force
                || (_config.ExternalConfig.ShowOnNewSong && !_config.State.Playback.ShowedOnNewSong)
                    || _config.State.Foobar2000IsPlayingRP
                    || _config.State.MusicBeeIsPlayingRP
                    || _config.IsRpPlayerTrackingChannel()
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
                    PackagedToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowSongStartToast), displaySongInfo)
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
                    PackagedToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowSongRatingToast), displaySongInfo)
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

        public void ShowSongDetailToast()
        {
            try
            {
                PackagedToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowSongDetailToast), _config.State.Playback.SongInfo)
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

        public void ShowLoginToast()
        {
            Task.Run(() =>
            {
                try
                {
                    PackagedToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowLoginToast))
                    .AddText("User authentication")
                    .AddText("Note: the applet doesn't save your password, only the same cookie as your browser")
                    .AddInputTextBox("Username", "Username")
                    .AddInputTextBox("Password", "Password")
                    .AddButton(new ToastButton()
                        .SetContent("Log in")
                        .AddArgument("action", "LoginDataSent"))
                    .AddButton(new ToastButton()
                        .SetContent("Not now")
                        .AddArgument("action", "NotNow"))
                    .Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });
        }

        public void LoginResponseToast(Auth authResp)
        {
            string formattedStatusMessage = $"User authentication {authResp.Status}";
            string authMessage = $@"{(authResp.Status == "success"
                ? $"Welcome {authResp.Username}!"
                : "Please try again")}";

            Task.Run(() =>
            {
                try
                {
                    PackagedToastHelper.CreateBaseToastContentBuilder(nameof(this.LoginResponseToast))
                    .AddText(formattedStatusMessage)
                    .AddText(authMessage)
                    .Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });
        }

        public void DataEraseToast()
        {
            Task.Run(() =>
            {
                try
                {
                    PackagedToastHelper.CreateBaseToastContentBuilder(nameof(this.DataEraseToast))
                    .AddText("Application Data Erase Requested")
                    .AddText("Deleting RP_Notify folder from APPDATA")
                    .AddText("Deleting notification handler")
                    .Show();
                }
                catch (Exception ex)
                {
                    _log.Error(LogHelper.GetMethodName(this), ex);
                }
            });
        }

        public void ErrorToast(Exception exception)
        {
            Task.Run(() =>
            {
                try
                {
                    var toastBuilder = new ToastContentBuilder()
                    .AddArgument(nameof(this.DataEraseToast))
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

        private void HandleToastActivatedEvent(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            try
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Obtain any user input (text boxes, menu selections) from the notification
                ValueSet userInput = toastArgs.UserInput;

                var formattedArgumentsForLogging = string.Join("&", args.Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));
                _log.Information(LogHelper.GetMethodName(this), " eventArguments: " + formattedArgumentsForLogging);

                if (args["action"] == "LoginRequested")
                {
                    ShowLoginToast();
                }
                else if (args["action"] == "LoginDataSent")
                {
                    userInput.TryGetValue("Username", out object usr);
                    userInput.TryGetValue("Password", out object pwd);

                    var response = _apiHandler.GetAuth((string)usr, (string)pwd);

                    LoginResponseToast(response);

                    if (response.Status == "success")
                    {
                        Application.Restart();
                    }
                }
                else if (args["action"] == "RateSubmitted")
                {

                    var songInfo = ObjectSerializer.DeserializeFromBase64<PlayListSong>(args["serializedSongInfo"]);

                    if (!userInput.TryGetValue("UserRate", out object rawUserRate)) return;
                    if (!Int32.TryParse((string)rawUserRate, out int userRate)
                        && 1 <= userRate && userRate <= 10) return;

                    var ratingResponse = _apiHandler.GetRating(songInfo.SongId, userRate);
                    if (ratingResponse.Status == "success")
                    {
                        if (songInfo.SongId == _config.State.Playback.SongInfo.SongId)
                        {
                            _config.State.Playback = new Playback(_apiHandler.GetNowplayingList());
                        }
                        else
                        {
                            var newRating = _apiHandler.GetInfo(songInfo.SongId).UserRating.ToString();
                            songInfo.UserRating = newRating;
                            ShowSongStartToast(true, songInfo);
                        }
                    }

                }
                else if (args["action"] == "RateTileRequested")
                {
                    var songInfo = ObjectSerializer.DeserializeFromBase64<PlayListSong>(args["serializedSongInfo"]);
                    KeyboardSendKeyHelper.SendWinKeyN();
                    Task.Delay(200).Wait();
                    ShowSongRatingToast(songInfo);
                }
            }
            catch (Exception ex)
            {
                _log.Error(LogHelper.GetMethodName(this), ex);
            }
        }
    }

    internal static class PackagedToastHelper
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
                    new ToastContextMenuItem("Display song rating tile (use from Action Center)", $"action=RateTileRequested;serializedSongInfo={serializedSongInfo}")
                    }
                };
            }

            var content = toastContentBuilder.GetToastContent();
            var toast = new ToastNotification(content.GetXml());

            toast.Group = "RP_Notify";
            if (songInfo != null)
            {
                toast.Tag = songInfo.SongId;
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
            var serializedSongInfo = args["serializedSongInfo"];

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

        internal static ToastContentBuilder AddSongRatingText(this ToastContentBuilder toastContentBuilder, IConfig _config)
        {
            var songInfo = toastContentBuilder.ExtractSonginfoObjectFromContextAction();

            bool userRated = !string.IsNullOrEmpty(songInfo.UserRating);

            var userRatingElement = userRated
                    ? $" - User rating: {songInfo.UserRating}"
                    : " - Not rated";

            string fullRatingText = $@"★ {songInfo.Rating}{(_config.State.IsUserAuthenticated ? userRatingElement : null)}";

            return _config.ExternalConfig.ShowSongRating || userRated
                ? toastContentBuilder.AddText(fullRatingText)
                : toastContentBuilder;
        }

        internal static ToastContentBuilder AddSongProgressBar(this ToastContentBuilder toastContentBuilder, IConfig _config)
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

            return toastContentBuilder.AddProgressBar(progressTitle,
                                                      progressValue,
                                                      progressIsIndeterminate,
                                                      progressValueStringOverride,
                                                      progressStatus);
        }

        internal static ToastContentBuilder AddSongAlbumArt(this ToastContentBuilder toastContentBuilder, IConfig _config, bool optionalLargeAlbumart)
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

            return optionalLargeAlbumart && _config.ExternalConfig.LargeAlbumArt
                ? toastContentBuilder.AddInlineImage(new Uri(albumartFilePath))
                : toastContentBuilder.AddAppLogoOverride(new Uri(albumartFilePath));
        }

        internal static ToastContentBuilder AddSongFooterText(this ToastContentBuilder toastContentBuilder, IConfig _config)
        {
            string chanTitle = _config.State.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.ExternalConfig.Channel).First().Title;

            string trimmedTitle = chanTitle.Contains("RP ")
                ? chanTitle.Split(new[] { "RP " }, StringSplitOptions.None)[1]
                : chanTitle;

            string footerText = trimmedTitle + TrackedPlayerAsSuffix(_config);

            return toastContentBuilder.AddAttributionText(footerText);
        }

        private static string TrackedPlayerAsSuffix(IConfig _config)
        {
            if (_config.IsRpPlayerTrackingChannel())
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

        internal static ToastContentBuilder AddSongInputActions(this ToastContentBuilder toastContentBuilder, IConfig _config)
        {
            var songInfo = toastContentBuilder.ExtractSonginfoObjectFromContextAction();

            var ratingHintText = !string.IsNullOrEmpty(songInfo.UserRating)
                    ? $"Current rating: {songInfo.UserRating}"
                    : "Type your rating (1-10)";

            var buttonText = (string.IsNullOrEmpty(songInfo.UserRating)
                                ? "Send"
                                : "Update")
                        + " rating";

            return _config.State.IsUserAuthenticated
                ? toastContentBuilder
                    .AddInputTextBox("UserRate", ratingHintText)
                    .AddArgument("action", "RateSubmitted")
                    .AddArgument("serializedSongInfo", ObjectSerializer.SerializeToBase64(songInfo))
                    .AddButton(new ToastButton()
                        .SetContent(buttonText)
                        .SetTextBoxId("UserRate"))
                : toastContentBuilder
                    .AddButton(new ToastButton()
                        .SetContent("Log in to rate song")
                        .AddArgument("action", "LoginRequested"));
        }

        internal static string TimeSpanToMinutes(int timeLength)
        {
            TimeSpan time = (timeLength < 5000)
                ? TimeSpan.FromSeconds(timeLength)
                : TimeSpan.FromMilliseconds(timeLength);
            return time.ToString(@"m\:ss");
        }
    }
}
