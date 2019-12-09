﻿using Microsoft.QueryStringDotNET;
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
            string title = SecurityElement.Escape($"{_apiHandler.SongInfo.Artist}\n{_apiHandler.SongInfo.Title} ({TimeSpanToMinutes(Int32.Parse(_apiHandler.SongInfo.Duration))})");
            string content = SecurityElement.Escape($"{_apiHandler.SongInfo.Album} ({_apiHandler.SongInfo.Year})");
            string image = _config.AlbumArtImagePath;

            string chanTitle = _apiHandler.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.Channel).First().Title;
            string trimmedTitle = chanTitle.Split(new[] { "RP " }, StringSplitOptions.None)[1];

            bool userRated = !string.IsNullOrEmpty(_apiHandler.SongInfo.UserRating);
            var userRatingText = userRated
                    ? $" - User rating: {_apiHandler.SongInfo.UserRating}"
                    : " - Not rated";
            string ratingText = $@"★ {_apiHandler.SongInfo.Rating}{(_apiHandler.IsUserAuthenticated ? userRatingText : null)}";
            string toastVisual =
            $@"<visual>
              <binding template='ToastGeneric'>
                <text>{title}</text>
                <text>{content}</text>
                {(_config.ShowSongRating || userRated ? $"<text>{ratingText}</text>" : null)}
                <text placement='attribution'>{trimmedTitle}</text>
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

        public void ShowSongDetailToast()
        {

            string title = SecurityElement.Escape($"{_apiHandler.SongInfo.Artist}\n{_apiHandler.SongInfo.Title}");
            string content = SecurityElement.Escape($"{_apiHandler.SongInfo.Album} ({_apiHandler.SongInfo.Year})");
            string image = _config.AlbumArtImagePath;
            string logo = _config.IconPath;

            // Construct the visuals of the toast
            int songDuration = Int32.Parse(_apiHandler.SongInfo.Duration);      // value is in milliseconds
            var mSecsLeftFromSong = (int)(_apiHandler.SongInfoExpiration - DateTime.Now).TotalMilliseconds;
            var elapsedMillisecs = Math.Min((songDuration - mSecsLeftFromSong + 500), songDuration);    // Corrigate rounding issues 
            double progressValue = (double)(songDuration - mSecsLeftFromSong) / songDuration;

            string chanTitle = _apiHandler.ChannelList.Where<Channel>(cl => Int32.Parse(cl.Chan) == _config.Channel).First().Title;
            string trimmedTitle = chanTitle.Split(new[] { "RP " }, StringSplitOptions.None)[1];

            bool userRated = !string.IsNullOrEmpty(_apiHandler.SongInfo.UserRating);
            var userRatingText = userRated
                    ? $" - User rating: {_apiHandler.SongInfo.UserRating}"
                    : " - Not rated";
            string ratingText = $@"★ {_apiHandler.SongInfo.Rating}{(_apiHandler.IsUserAuthenticated ? userRatingText : null)}";

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
                {(_config.ShowSongRating || userRated ? $"<text>{ratingText}</text>" : null)}
                <text placement='attribution'>{trimmedTitle}</text>
                <progress value='{progressValue}' title='{TimeSpanToMinutes(songDuration)}' status='{TimeSpanToMinutes(elapsedMillisecs)}' valueStringOverride='-{TimeSpanToMinutes(mSecsLeftFromSong)}' />
                {(_config.LargeAlbumArt ? largeAlbumart : smallAlbumArt)}
              </binding>
            </visual>";

            // Construct the audio of the toast
            string toastAudio = "<audio silent='true' />";

            // Construct the actions of the toast
            string defaultItem = !string.IsNullOrEmpty(_apiHandler.SongInfo.UserRating) ? _apiHandler.SongInfo.UserRating : "NotRated";

            string songWebUrl = $@"https://radioparadise.com/player/info/{_apiHandler.SongInfo.SongId}";

            var ratingHintText = !string.IsNullOrEmpty(_apiHandler.SongInfo.UserRating)
                    ? $"Current rating: {_apiHandler.SongInfo.UserRating}"
                    : "Type your rating (1-10)";
            var loggedInAction = $@"
             <input id='UserRate' type='text' placeHolderContent='{ratingHintText}'/>

             <action
                  content='{(string.IsNullOrEmpty(_apiHandler.SongInfo.UserRating) ? "Send" : "Update")} rating'
                  arguments ='action=RateSubmitted&amp;SongId={_apiHandler.SongInfo.SongId};'/>";
            var loggedOutAction = $@"
             <action
                  content='Log in to rate'
                  arguments ='action=LoginRequested'/>";

            string toastActions =
            $@"<actions>
              {(_apiHandler.IsUserAuthenticated ? loggedInAction : loggedOutAction)}
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
            string logo = _config.IconPath;

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
                        ShowSongDetailToast();
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
                            _apiHandler.UpdateSongInfo();
                            ShowSongDetailToast();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Not activated event
                var a = ex;
            }
        }

        public void LoginResponseToast(Auth authResp)
        {
            string logo = _config.IconPath;
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
            string logo = _config.IconPath;

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
        private void WriteIconToDisk()
        {
            if (!File.Exists(_config.IconPath))
            {
                byte[] iconBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    Properties.Resources.RPIcon.Save(ms);
                    iconBytes = ms.ToArray();
                }
                File.WriteAllBytes(_config.IconPath, iconBytes);
            }


            Application.ApplicationExit += (sender, e) =>
            {
                if (File.Exists(_config.IconPath))
                {
                    File.Delete(_config.IconPath);
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
