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

        //public void DebugToast()
        //{
        //    var xmltext = File.ReadAllText(@"c:\Users\Gergely Vajda\Sandbox\SampleToastTestXML.xml");
        //    var doc = new XmlDocument();
        //    doc.LoadXml(xmltext);
        //    // And create the toast notification
        //    var toast = new ToastNotification(doc);
        //    // And then show it
        //    DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
        //}

        public void ShowSongStartToast()
        {
            string title = SecurityElement.Escape($"{_config.State.Playback.SongInfo.Artist}\n{_config.State.Playback.SongInfo.Title} ({TimeSpanToMinutes(Int32.Parse(_config.State.Playback.SongInfo.Duration))})");
            string content = SecurityElement.Escape($"{_config.State.Playback.SongInfo.Album} ({_config.State.Playback.SongInfo.Year})");
            string image = _config.InternalConfig.AlbumArtImagePath;

            string chanTitle = _config.State.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.ExternalConfig.Channel).First().Title;
            string trimmedTitle = chanTitle.Contains("RP ")
                ? chanTitle.Split(new[] { "RP " }, StringSplitOptions.None)[1]
                : chanTitle;

            bool userRated = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating);
            var userRatingText = userRated
                    ? $" - User rating: {_config.State.Playback.SongInfo.UserRating}"
                    : " - Not rated";
            string ratingText = $@"★ {_config.State.Playback.SongInfo.Rating}{(_config.State.IsUserAuthenticated ? userRatingText : null)}";
            string toastVisual =
            $@"<visual>
              <binding template='ToastGeneric'>
                <text>{title}</text>
                <text>{content}</text>
                {(_config.ExternalConfig.ShowSongRating || userRated ? $"<text>{ratingText}</text>" : null)}
                <text placement='attribution'>{trimmedTitle}{TrackedPlayerAsSuffix()}</text>
                <image src='{image}' placement='appLogoOverride'/>
              </binding>
            </visual>";

            // Construct the audio of the toast
            string toastAudio = "<audio silent='true' />";

            // Create toast xml text
            string toastXmlString =
            $@"<toast launch='RpNotifySongDetails'>
                {toastVisual}
                {toastAudio}
            </toast>";
            Retry.Do(() =>
                {
                    if (!File.Exists(image))
                    {
                        throw new IOException("Cover is not downloaded");
                    }
                });
            DisplayToast(toastXmlString);
        }

        public void ShowSongRatingToast()
        {
            string title = SecurityElement.Escape($"{_config.State.Playback.SongInfo.Artist}\n{_config.State.Playback.SongInfo.Title} ({TimeSpanToMinutes(Int32.Parse(_config.State.Playback.SongInfo.Duration))})");
            string content = SecurityElement.Escape($"{_config.State.Playback.SongInfo.Album} ({_config.State.Playback.SongInfo.Year})");
            string image = _config.InternalConfig.AlbumArtImagePath;

            string chanTitle = _config.State.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.ExternalConfig.Channel).First().Title;

            string trimmedTitle = chanTitle.Contains("RP ")
                ? chanTitle.Split(new[] { "RP " }, StringSplitOptions.None)[1]
                : chanTitle;

            bool userRated = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating);
            var userRatingText = userRated
                    ? $" - User rating: {_config.State.Playback.SongInfo.UserRating}"
                    : " - Not rated";
            string ratingText = $@"★ {_config.State.Playback.SongInfo.Rating}{(_config.State.IsUserAuthenticated ? userRatingText : null)}";
            string toastVisual =
            $@"<visual>
              <binding template='ToastGeneric'>
                <text>{title}</text>
                <text>{content}</text>
                {(_config.ExternalConfig.ShowSongRating || userRated ? $"<text>{ratingText}</text>" : null)}
                <text placement='attribution'>{trimmedTitle}{TrackedPlayerAsSuffix()}</text>
                <image src='{image}' placement='appLogoOverride'/>
              </binding>
            </visual>";

            // Construct the audio of the toast
            string toastAudio = "<audio silent='true' />";

            // Construct the actions of the toast
            string defaultItem = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating) ? _config.State.Playback.SongInfo.UserRating : "NotRated";

            string songWebUrl = $@"https://radioparadise.com/player/info/{_config.State.Playback.SongInfo.SongId}";

            var ratingHintText = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating)
                    ? $"Current rating: {_config.State.Playback.SongInfo.UserRating}"
                    : "Type your rating (1-10)";
            var loggedInAction = $@"
             <input id='UserRate' type='text' placeHolderContent='{ratingHintText}'/>

             <action
                  content='{(string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating) ? "Send" : "Update")} rating'
                  arguments ='action=RateSubmitted&amp;SongId={_config.State.Playback.SongInfo.SongId}&amp;toastType=ShowSongRatingToast'/>";
            var loggedOutAction = $@"
             <action
                  content='Log in to rate'
                  arguments ='action=LoginRequested'/>";

            string toastActions =
            $@"<actions>
              {(_config.State.IsUserAuthenticated ? loggedInAction : loggedOutAction)}
              <action
                  content='Open in browser'
                  arguments='{songWebUrl}'
                  activationType ='protocol'/>
 
            </actions>";

            // Create toast xml text
            string toastXmlString =
            $@"<toast launch='RpNotifySongDetails'>
                {toastVisual}
                {toastAudio}
                {toastActions}
            </toast>";

            Retry.Do(() =>
            {
                if (!File.Exists(image))
                {
                    throw new IOException("Cover is not downloaded");
                }
            });
            DisplayToast(toastXmlString, HandleToastActivatedEvent);
        }

        public void ShowSongDetailToast()
        {

            string title = SecurityElement.Escape($"{_config.State.Playback.SongInfo.Artist}\n{_config.State.Playback.SongInfo.Title}");
            string content = SecurityElement.Escape($"{_config.State.Playback.SongInfo.Album} ({_config.State.Playback.SongInfo.Year})");
            string image = _config.InternalConfig.AlbumArtImagePath;
            string logo = _config.InternalConfig.IconPath;

            // Construct the visuals of the toast
            int songDuration = Int32.Parse(_config.State.Playback.SongInfo.Duration);      // value is in milliseconds
            var mSecsLeftFromSong = (int)(_config.State.Playback.SongInfoExpiration - DateTime.Now).TotalMilliseconds;
            var elapsedMillisecs = Math.Min((songDuration - mSecsLeftFromSong + 500), songDuration);    // Corrigate rounding issues 
            double progressValue = (double)(songDuration - mSecsLeftFromSong) / songDuration;

            string chanTitle = _config.State.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.ExternalConfig.Channel).First().Title;

            string trimmedTitle = chanTitle.Contains("RP ")
                ? chanTitle.Split(new[] { "RP " }, StringSplitOptions.None)[1]
                : chanTitle;

            bool userRated = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating);
            var userRatingText = userRated
                    ? $" - User rating: {_config.State.Playback.SongInfo.UserRating}"
                    : " - Not rated";
            string ratingText = $@"★ {_config.State.Playback.SongInfo.Rating}{(_config.State.IsUserAuthenticated ? userRatingText : null)}";

            string largeAlbumart = $@"
                <image src='{image}'/>
                <image src='{logo}' placement='appLogoOverride' hint-crop='circle'/>
            ";
            string smallAlbumArt = $"<image src='{image}' placement='appLogoOverride'/>";

            string toastVisual =
            $@"<visual>
              <binding template='ToastGeneric'>
                <text>{title}</text>
                <text>{content}</text>
                {(_config.ExternalConfig.ShowSongRating || userRated ? $"<text>{ratingText}</text>" : null)}
                <text placement='attribution'>{trimmedTitle}{TrackedPlayerAsSuffix()}</text>
                <progress value='{progressValue}' title='{TimeSpanToMinutes(songDuration)}' status='{TimeSpanToMinutes(elapsedMillisecs)}' valueStringOverride='-{TimeSpanToMinutes(mSecsLeftFromSong)}' />
                {(_config.ExternalConfig.LargeAlbumArt ? largeAlbumart : smallAlbumArt)}
              </binding>
            </visual>";

            // Construct the audio of the toast
            string toastAudio = "<audio silent='true' />";

            // Construct the actions of the toast
            string defaultItem = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating) ? _config.State.Playback.SongInfo.UserRating : "NotRated";

            string songWebUrl = $@"https://radioparadise.com/player/info/{_config.State.Playback.SongInfo.SongId}";

            var ratingHintText = !string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating)
                    ? $"Current rating: {_config.State.Playback.SongInfo.UserRating}"
                    : "Type your rating (1-10)";
            var loggedInAction = $@"
             <input id='UserRate' type='text' placeHolderContent='{ratingHintText}'/>

             <action
                  content='{(string.IsNullOrEmpty(_config.State.Playback.SongInfo.UserRating) ? "Send" : "Update")} rating'
                  arguments ='action=RateSubmitted&amp;SongId={_config.State.Playback.SongInfo.SongId}&amp;toastType=ShowSongDetailToast'/>";
            var loggedOutAction = $@"
             <action
                  content='Log in to rate'
                  arguments ='action=LoginRequested'/>";

            string toastActions =
            $@"<actions>
              {(_config.State.IsUserAuthenticated ? loggedInAction : loggedOutAction)}
              <action
                  content='Open in browser'
                  arguments='{songWebUrl}'
                  activationType ='protocol'/>
 
            </actions>";

            // Create toast xml text
            string toastXmlString =
            $@"<toast launch='RpNotifySongDetails'>
                {toastVisual}
                {toastAudio}
                {toastActions}
            </toast>";

            DisplayToast(toastXmlString, HandleToastActivatedEvent);
        }

        public void LoginToast()
        {
            string logo = _config.InternalConfig.IconPath;

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
                <audio silent='true' />
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
                _log.Information("Toast activated - {eventArguments}", rpEvent.Arguments);

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
                            // _apiHandler.UpdateSongInfo();
                            if (args["toastType"] == "ShowSongDetailToast")
                            {
                                ShowSongStartToast();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"-- HandleToastActivatedEvent - {ex.Message}");
            }
        }

        public void LoginResponseToast(Auth authResp)
        {
            string logo = _config.InternalConfig.IconPath;
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
                <audio silent='true' />
            </toast>";


            DisplayToast(toastXmlString);
        }

        public void SongInfoListenerError()
        {
            string logo = _config.InternalConfig.IconPath;

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
                <audio silent='true' />
            </toast>";

            DisplayToast(toastXmlString);
        }
        private void DisplayToast(string toastXmlString, TypedEventHandler<ToastNotification, Object> activationHandler = null)
        {
            var callerClassName = new StackFrame(2, true).GetMethod().DeclaringType.FullName;
            callerClassName = callerClassName.Split(new[] { "+" }, StringSplitOptions.None)[0];
            var toastType = new StackFrame(1, true).GetMethod().Name;
            _log.Information("-- Display Toast Notification - Toast type: {ToastType} - Caller class: {CallerClass}", toastType, callerClassName);

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
            _log.Information("-- Display Toast Notification - Displayed successfully");
        }

        private string TimeSpanToMinutes(int timeLength)
        {
            TimeSpan time = (timeLength < 5000)
                ? TimeSpan.FromSeconds(timeLength)
                : TimeSpan.FromMilliseconds(timeLength);
            return time.ToString(@"m\:ss");
        }

        private string TrackedPlayerAsSuffix()
        {
            if (_config.State.Foobar2000IsPlayingRP)
            {
                return " • Foobar2000";
            }

            if (_config.State.RpTrackingConfig.ValidateActivePlayerId())
            {
                var activePlayerId = _config.State.RpTrackingConfig.ActivePlayerId;
                var activePlayer = _config.State.RpTrackingConfig.Players
                    .Where(p => p.PlayerId == activePlayerId).First().Source;

                return $" • {activePlayer}";
            }

            return null;
        }

        private void WriteIconToDisk()
        {
            if (!File.Exists(_config.InternalConfig.IconPath))
            {
                byte[] iconBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    Properties.Resources.RPIcon.Save(ms);
                    iconBytes = ms.ToArray();
                }
                File.WriteAllBytes(_config.InternalConfig.IconPath, iconBytes);
            }


            Application.ApplicationExit += (sender, e) =>
            {
                if (File.Exists(_config.InternalConfig.IconPath))
                {
                    File.Delete(_config.InternalConfig.IconPath);
                }
            };
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
