using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using RP_Notify.API;
using RP_Notify.API.ResponseModel;
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

namespace RP_Notify.Toast
{
    internal class PackageToastHandler : IToastHandler
    {
        private readonly IConfig _config;
        private readonly IRpApiHandler _apiHandler;
        private readonly ILog _log;

        public PackageToastHandler(IConfig config, IRpApiHandler apiHandler, ILog log)
        {
            _config = config;
            _apiHandler = apiHandler;
            _log = log;

            ToastNotificationManagerCompat.OnActivated += toastArgs => HandleToastActivatedEvent(toastArgs);
        }

        public void ShowSongStartToast(bool force = false)
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

            Task.Run(() =>
            {
                try
                {
                    PackagedToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowSongStartToast))
                    .AddSongInfoText(_config, true)
                    .AddSongAlbumText(_config)
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

        public void ShowSongRatingToast()
        {
            Task.Run(() =>
            {
                try
                {
                    PackagedToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowSongRatingToast))
                    .AddSongInfoText(_config, true)
                    .AddSongAlbumText(_config)
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
                PackagedToastHelper.CreateBaseToastContentBuilder(nameof(this.ShowSongDetailToast))
                .AddSongInfoText(_config, true)
                .AddSongAlbumText(_config)
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

                    if(exception.InnerException != null)
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
               

                _log.Information(LogHelper.GetMethodName(this), "{ eventArguments}", args.ToString());
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
                    userInput.TryGetValue("UserRate", out object rawUserRate);
                    if (Int32.TryParse((string)rawUserRate, out int userRate)
                        && 1 <= userRate && userRate <= 10
                        && Int32.TryParse(args["SongId"], out int songId))
                    {
                        var ratingResponse = _apiHandler.GetRating(songId.ToString(), userRate);
                        if (ratingResponse.Status == "success")
                        {
                            _config.State.Playback = new Playback(_apiHandler.GetNowplayingList());
                        }
                    }
                }
                else if (args["action"] == "RateTileRequested")
                {
                    KeyboardHelper.SendWinKeyN();
                    Task.Delay(200).Wait();
                    ShowSongRatingToast();
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
        internal static ToastContentBuilder CreateBaseToastContentBuilder(string toastId)
        {
            var toastContentBuilder = new ToastContentBuilder()
                .AddArgument(toastId)
                .AddAudio(new ToastAudio()
                {
                    Silent = true,
                });

            toastContentBuilder.Content.Actions = new ToastActionsCustom()
            {
                ContextMenuItems = {
                    new ToastContextMenuItem("Display song rating tile", "action=RateTileRequested")
                }
            };

            return toastContentBuilder;
        }

        internal static ToastContentBuilder AddSongInfoText(this ToastContentBuilder toastContentBuilder, IConfig _config, bool withDuration)
        {
            string duration = $" ({TimeSpanToMinutes(Int32.Parse(_config.State.Playback.SongInfo.Duration))})";
            string title = SecurityElement.Escape($"{_config.State.Playback.SongInfo.Artist}\n{_config.State.Playback.SongInfo.Title}{(withDuration ? duration : null)}");

            toastContentBuilder.AddText(title);
            return toastContentBuilder;
        }

        internal static ToastContentBuilder AddSongAlbumText(this ToastContentBuilder toastContentBuilder, IConfig _config)
        {
            string content = SecurityElement.Escape($"{_config.State.Playback.SongInfo.Album} ({_config.State.Playback.SongInfo.Year})");

            toastContentBuilder.AddText(content);
            return toastContentBuilder;
        }

        internal static ToastContentBuilder AddSongRatingText(this ToastContentBuilder toastContentBuilder, IConfig _config)
        {
            bool userRated = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating);

            var userRatingElement = userRated
                    ? $" - User rating: {_config.State.Playback.SongInfo.UserRating}"
                    : " - Not rated";

            string fullRatingText = $@"★ {_config.State.Playback.SongInfo.Rating}{(_config.State.IsUserAuthenticated ? userRatingElement : null)}";

            return _config.ExternalConfig.ShowSongRating || userRated
                ? toastContentBuilder.AddText(fullRatingText)
                : toastContentBuilder;
        }

        internal static ToastContentBuilder AddSongProgressBar(this ToastContentBuilder toastContentBuilder, IConfig _config)
        {
            // Construct the visuals of the toast
            int songDuration = Int32.Parse(_config.State.Playback.SongInfo.Duration);      // value is in milliseconds
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
            Retry.Do(() =>
            {
                if (!File.Exists(_config.StaticConfig.AlbumArtImagePath))
                {
                    throw new IOException("Cover is not yet downloaded");
                }
            }, 1000, 15);

            DownloadImageIfDoesntExist(_config);

            return optionalLargeAlbumart && _config.ExternalConfig.LargeAlbumArt
                ? toastContentBuilder.AddInlineImage(new Uri(_config.StaticConfig.AlbumArtImagePath))
                : toastContentBuilder.AddAppLogoOverride(new Uri(_config.StaticConfig.AlbumArtImagePath));
        }

        private static void DownloadImageIfDoesntExist(IConfig _config)
        {
            //TODO
            return; 
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
            var ratingHintText = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating)
                    ? $"Current rating: {_config.State.Playback.SongInfo.UserRating}"
                    : "Type your rating (1-10)";

            var buttonText = (string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating)
                                ? "Send"
                                : "Update")
                        + " rating";

            return _config.State.IsUserAuthenticated
                ? toastContentBuilder
                    .AddInputTextBox("UserRate", ratingHintText)
                    .AddArgument("action", "RateSubmitted")
                    .AddArgument("SongId", _config.State.Playback.SongInfo.SongId)
                    .AddButton(new ToastButton()
                        .SetContent(buttonText)
                        .SetTextBoxId("UserRate"))
                : toastContentBuilder
                    .AddButton(new ToastButton()
                        .SetContent("Log in to rate song")
                        .AddArgument("action", "LoginRequested"));
        }

        internal static string OpenInBrowserAction(IConfig _config)
        {
            string songWebUrl = $"https://radioparadise.com/player/info/{_config.State.Playback.SongInfo.SongId}";

            var actionElementText = $@"
                <action
                    content='Open in browser'
                    arguments='{songWebUrl}'
                    activationType ='protocol'/>";

            return actionElementText;
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
