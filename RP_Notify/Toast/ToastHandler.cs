using Microsoft.QueryStringDotNET;
using Newtonsoft.Json;
using RP_Notify.API;
using RP_Notify.API.ResponseModel;
using RP_Notify.Config;
using RP_Notify.ErrorHandler;
using RP_Notify.Toast.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.UI.Notifications;

namespace RP_Notify.Toast
{
    public class ToastHandler : IToastHandler
    {
        private readonly IConfig _config;
        private readonly IRpApiHandler _apiHandler;
        private readonly ILogger _log;

        private const string appId = "GergelyVajda.RP_Notify";
        internal const string guid = "8a8d7d8c-b191-4b17-b527-82c795243a12";

        public ToastHandler(IConfig config, IRpApiHandler apiHandler, ILog log)
        {
            _config = config;
            _apiHandler = apiHandler;
            _log = log.Logger;
            // Register AUMID and COM server (for Desktop Bridge apps, this no-ops)
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<ToastActivator>(appId);
            // Register COM server and activator type
            DesktopNotificationManagerCompat.RegisterActivator<ToastActivator>();
            WriteIconToDisk();
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
                    || _config.IsRpPlayerTrackingChannel()
                ))
            {
                return;
            }

            Task.Run(() =>
            {
                string toastVisual =
                $@"<visual>
              <binding template='ToastGeneric'>
                {ToastHelper.CreateTitleText(_config, true)}
                {ToastHelper.CreateContentText(_config)}
                {ToastHelper.CreateRatingText(_config)}
                {ToastHelper.CreateToastFooter(_config)}
                {ToastHelper.CreateImage(_config, false)}
              </binding>
            </visual>";

                // Create toast xml text
                string toastXmlString =
                $@"<toast launch='RpNotifySongDetails'>
                {toastVisual}
                {ToastHelper.toastAudio}
            </toast>";

                DisplayToast(toastXmlString);
            });

            _config.State.Playback.ShowedOnNewSong = true;
        }

        public void ShowSongRatingToast()
        {
            Task.Run(() =>
            {
                string toastVisual =
                $@"<visual>
                  <binding template='ToastGeneric'>
                    {ToastHelper.CreateTitleText(_config, true)}
                    {ToastHelper.CreateContentText(_config)}
                    {ToastHelper.CreateRatingText(_config)}
                    {ToastHelper.CreateToastFooter(_config)}
                    {ToastHelper.CreateImage(_config, false)}
                  </binding>
                </visual>";

                string toastActions =
                    $@"<actions>
                      {ToastHelper.RatingInputAction(_config)}
                      {ToastHelper.OpenInBrowserAction(_config)}
                    </actions>";

                // Create toast xml text
                string toastXmlString =
                $@"<toast launch='RpNotifySongDetails'>
                    {toastVisual}
                    {ToastHelper.toastAudio}
                    {toastActions}
                </toast>";

                DisplayToast(toastXmlString, HandleToastActivatedEvent);
            });
        }

        public void ShowSongDetailToast()
        {
            Task.Run(() =>
            {
                string toastVisual =
                $@"<visual>
                  <binding template='ToastGeneric'>
                    {ToastHelper.CreateTitleText(_config, false)}
                    {ToastHelper.CreateContentText(_config)}
                    {ToastHelper.CreateRatingText(_config)}
                    {ToastHelper.CreateToastFooter(_config)}
                    {ToastHelper.CreateProgressBar(_config)}
                    {ToastHelper.CreateImage(_config, true)}
                  </binding>
                </visual>";

                string toastActions =
                $@"<actions>
                  {ToastHelper.RatingInputAction(_config)}
                  {ToastHelper.OpenInBrowserAction(_config)}
                </actions>";

                // Create toast xml text
                string toastXmlString =
                $@"<toast launch='RpNotifySongDetails'>
                    {toastVisual}
                    {ToastHelper.toastAudio}
                    {toastActions}
                </toast>";

                DisplayToast(toastXmlString, HandleToastActivatedEvent);
            });
        }

        public void LoginToast()
        {
            Task.Run(() =>
            {
                string logo = _config.StaticConfig.IconPath;

                // Create toast xml text
                string toastXmlString =
                $@"<toast launch='RpNotifySongDetails'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>User authentication</text>
                            <text>Note: the applet doesn't save your password, only the same cookie as your browser</text>
                            <image src='{logo}' placement='appLogoOverride' hint-crop='circle'/>
                        </binding>
                    </visual>
                    {ToastHelper.toastAudio}
                    <actions>
                        <input id='Username' type='text' placeHolderContent='Username'/>
                        <input id='Password' type='text' placeHolderContent='Password'/>
                        <action
                            content='Log in'
                            arguments ='action=LoginDataSent'/>
                        <action
                            content='Not now'
                            arguments='NotNow'/>
                    </actions>
                </toast>";

                DisplayToast(toastXmlString, HandleToastActivatedEvent);
            });
        }

        private void HandleToastActivatedEvent(object sender, object e)
        {
            RpToastNotificationActivatedEventArgs rpEvent;
            try
            {
                ToastActivatedEventArgs myEvent = (ToastActivatedEventArgs)e;
                rpEvent = JsonConvert
                    .DeserializeObject<RpToastNotificationActivatedEventArgs>(
                        JsonConvert.SerializeObject(myEvent)
                    );
                _log.Information($"{ LogHelper.GetMethodName(this)} - {{ eventArguments}}", rpEvent.Arguments);

                QueryString args = QueryString.Parse(rpEvent.Arguments);
                if (args["action"] == "LoginRequested")
                {
                    LoginToast();
                }
                else
                if (args["action"] == "LoginDataSent")
                {
                    var usr = rpEvent.UserInput["Username"];
                    var pwd = rpEvent.UserInput["Password"];

                    var response = _apiHandler.GetAuth(usr, pwd);

                    LoginResponseToast(response);

                    if (response.Status == "success")
                    {
                        Application.Restart();
                    }
                }
                else if (args["action"] == "RateSubmitted")
                {
                    if (Int32.TryParse(rpEvent.UserInput["UserRate"], out int userRate)
                        && 1 <= userRate && userRate <= 10
                        && Int32.TryParse(args["SongId"], out int songId))
                    {
                        var ratingResponse = _apiHandler.GetRating(songId.ToString(), userRate);
                        if (ratingResponse.Status == "success")
                        {
                            _config.State.TryUpdatePlayback(new Playback(_apiHandler.GetNowplayingList()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"{LogHelper.GetMethodName(this)} - ERROR - {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void LoginResponseToast(Auth authResp)
        {
            Task.Run(() =>
            {
                string logo = _config.StaticConfig.IconPath;
                string authMessage = $@"{(authResp.Status == "success"
                                ? $"Welcome {authResp.Username}!"
                                : "Please try again")}";
                // Create toast xml text
                string toastXmlString =
                $@"<toast launch='RpNotifySongDetails'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>User authentication {authResp.Status}</text>
                            <text>{authMessage}</text>
                            <image src='{logo}' placement='appLogoOverride' hint-crop='circle'/>
                        </binding>
                    </visual>
                    {ToastHelper.toastAudio}
                </toast>";


                DisplayToast(toastXmlString);
            });
        }

        public void SongInfoListenerError()
        {
            Task.Run(() =>
            {
                string logo = _config.StaticConfig.IconPath;

                // Create toast xml text
                string toastXmlString =
                $@"<toast launch='RpNotifySongDetails'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>Application ERROR</text>
                            <text>Can't load song info</text>
                            <text>Please check your network status and firewall settings</text>
                            <image src='{logo}' placement='appLogoOverride' hint-crop='circle'/>
                        </binding>
                    </visual>
                    {ToastHelper.toastAudio}
                </toast>";

                DisplayToast(toastXmlString);
            });
        }
        private void DisplayToast(string toastXmlString, TypedEventHandler<ToastNotification, Object> activationHandler = null)
        {
            var callerClassName = new StackFrame(2, true).GetMethod().DeclaringType.FullName;
            callerClassName = callerClassName.Split(new[] { "+" }, StringSplitOptions.None)[0];
            var toastType = new StackFrame(1, true).GetMethod().Name;
            _log.Information($"{LogHelper.GetMethodName(this)} - Toast type: {{ToastType}} - Caller class: {{CallerClass}}", toastType, callerClassName);

            // Parse to XML
            XmlDocument toastXml = new XmlDocument();
            toastXml.LoadXml(toastXmlString);

            // Setup Toast
            ToastNotification toast = new ToastNotification(toastXml);
            if (activationHandler != null)
            {
                toast.Activated += activationHandler;
            }

            // Display Toast
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
            _log.Information($"{LogHelper.GetMethodName(this)} - Displayed successfully");
        }

        private void WriteIconToDisk()
        {
            if (!File.Exists(_config.StaticConfig.IconPath))
            {
                byte[] iconBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    Properties.Resources.RPIcon.Save(ms);
                    iconBytes = ms.ToArray();
                }
                File.WriteAllBytes(_config.StaticConfig.IconPath, iconBytes);
            }


            Application.ApplicationExit += (sender, e) =>
            {
                if (File.Exists(_config.StaticConfig.IconPath))
                {
                    File.Delete(_config.StaticConfig.IconPath);
                }
            };
        }
    }

    internal static class ToastHelper
    {
        internal static string toastAudio = "<audio silent='true' />";

        internal static string CreateTitleText(IConfig _config, bool withDuration)
        {
            string duration = $" ({TimeSpanToMinutes(Int32.Parse(_config.State.Playback.SongInfo.Duration))})";
            string title = SecurityElement.Escape($"{_config.State.Playback.SongInfo.Artist}\n{_config.State.Playback.SongInfo.Title}{(withDuration ? duration : null)}");

            return $"<text>{title}</text>";
        }

        internal static string CreateContentText(IConfig _config)
        {
            string content = SecurityElement.Escape($"{_config.State.Playback.SongInfo.Album} ({_config.State.Playback.SongInfo.Year})");

            return $"<text>{content}</text>";
        }

        internal static string CreateRatingText(IConfig _config)
        {
            bool userRated = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating);
            var userRatingText = userRated
                    ? $" - User rating: {_config.State.Playback.SongInfo.UserRating}"
                    : " - Not rated";
            string ratingText = $@"★ {_config.State.Playback.SongInfo.Rating}{(_config.State.IsUserAuthenticated ? userRatingText : null)}";
            return _config.ExternalConfig.ShowSongRating || userRated
                ? $"<text>{ratingText}</text>"
                : null;
        }

        internal static string CreateProgressBar(IConfig _config)
        {
            // Construct the visuals of the toast
            int songDuration = Int32.Parse(_config.State.Playback.SongInfo.Duration);      // value is in milliseconds
            var mSecsLeftFromSong = (int)(_config.State.Playback.SongInfoExpiration - DateTime.Now).TotalMilliseconds;
            var elapsedMillisecs = Math.Min((songDuration - mSecsLeftFromSong + 500), songDuration);    // Corrigate rounding issues 
            double progressValue = (double)(songDuration - mSecsLeftFromSong) / songDuration;

            return $"<progress value = '{progressValue}' title = '{TimeSpanToMinutes(songDuration)}' status = '{TimeSpanToMinutes(elapsedMillisecs)}' valueStringOverride = '-{TimeSpanToMinutes(mSecsLeftFromSong)}'/> ";
        }

        internal static string CreateImage(IConfig _config, bool optionalLargeAlbumart)
        {
            string image = _config.StaticConfig.AlbumArtImagePath;
            string logo = _config.StaticConfig.IconPath;

            string largeAlbumart = $@"
                <image src='{image}'/>
                <image src='{logo}' placement='appLogoOverride' hint-crop='circle'/>
            ";
            string smallAlbumArt = $"<image src='{image}' placement='appLogoOverride'/>";

            Retry.Do(() =>
            {
                if (!File.Exists(image))
                {
                    throw new IOException("Cover is not yet downloaded");
                }
            }, 1000, 5);

            return optionalLargeAlbumart && _config.ExternalConfig.LargeAlbumArt
                ? largeAlbumart
                : smallAlbumArt;
        }

        internal static string CreateToastFooter(IConfig _config)
        {
            string chanTitle = _config.State.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.ExternalConfig.Channel).First().Title;

            string trimmedTitle = chanTitle.Contains("RP ")
                ? chanTitle.Split(new[] { "RP " }, StringSplitOptions.None)[1]
                : chanTitle;

            return $"<text placement='attribution'>{trimmedTitle}{TrackedPlayerAsSuffix(_config)}</text>";
        }

        private static string TrackedPlayerAsSuffix(IConfig _config)
        {
            if (_config.IsRpPlayerTrackingChannel())
            {
                var activePlayerId = _config.State.RpTrackingConfig.ActivePlayerId;
                var activePlayer = _config.State.RpTrackingConfig.Players
                    .Where(p => p.PlayerId == activePlayerId).First().Source;

                return $" • {activePlayer}";
            }

            if (_config.State.Foobar2000IsPlayingRP)
            {
                return " • Foobar2000";
            }

            return null;
        }

        internal static string RatingInputAction(IConfig _config)
        {
            var ratingHintText = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating)
                    ? $"Current rating: {_config.State.Playback.SongInfo.UserRating}"
                    : "Type your rating (1-10)";
            var loggedInAction = $@"
                 <input id='UserRate' type='text' placeHolderContent='{ratingHintText}'/>
                 <action
                      content='{(string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating) ? "Send" : "Update")} rating'
                      arguments ='action=RateSubmitted&amp;SongId={_config.State.Playback.SongInfo.SongId}'/>";
            var loggedOutAction = $@"
                 <action
                      content='Log in to rate'
                      arguments ='action=LoginRequested'/>";

            return _config.State.IsUserAuthenticated
                ? loggedInAction
                : loggedOutAction;
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

    internal partial class RpToastNotificationActivatedEventArgs
    {
        [JsonProperty("Arguments")]
        public string Arguments { get; set; }

        [JsonProperty("UserInput")]
        public Dictionary<string, string> UserInput { get; set; }
    }

    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [System.Runtime.InteropServices.Guid(ToastHandler.guid), ComVisible(true)]
    public class ToastActivator : NotificationActivator
    {
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            // Used to register toast
        }
    }
}
