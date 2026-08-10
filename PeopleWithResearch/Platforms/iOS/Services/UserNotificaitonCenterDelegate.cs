using Azure;
using Foundation;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;

namespace PeopleWithResearch
{
    public class UserNotificaitonCenterDelegate : UNUserNotificationCenterDelegate
    {
        public UserNotifications UpdateNotification = new (); 
        private const string DailyNotificationText = "HOPPER Study - Action Required";
        public NSString jsonKey = new NSString("Plugin.LocalNotification.RETURN_REQUEST");

        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {

            var presentationOptions = UIDevice.CurrentDevice.CheckSystemVersion(14, 0)
       ? UNNotificationPresentationOptions.Banner | UNNotificationPresentationOptions.Sound | UNNotificationPresentationOptions.Badge
       : UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound | UNNotificationPresentationOptions.Badge;

            completionHandler(presentationOptions);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await HandleNotificationReceived(notification);
            });
        }

        private async Task HandleNotificationReceived(UNNotification notification)
        {
            var description = notification.Request.Content.Body;
            var isDailyRepeat = notification.Request.Trigger is UNCalendarNotificationTrigger { Repeats: true };
            var isDailyNotification = isDailyRepeat || description == DailyNotificationText;

            if (isDailyNotification)
            {
                await UpdateNotification.ScheduleDailyNotification();
            }
        }

        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await HandleNotificationData(response.Notification.Request.Content.UserInfo, false);
            });

            completionHandler();
        }


        //Local Notification Tapped Event 
        public async Task iOSlocalNotificationTapped(NSDictionary userInfo)
        {
            try
            {
                string userId = Preferences.Default.Get("userid", string.Empty);
                if (string.IsNullOrEmpty(userId)) return;

                if (!userInfo.ContainsKey(jsonKey)) return;
                string rawJson = userInfo.ValueForKey(jsonKey).ToString();

                using var doc = JsonDocument.Parse(rawJson);
                if (!doc.RootElement.TryGetProperty("Title", out JsonElement titleElement)) return;

                string title = titleElement.GetString();
                if (string.IsNullOrEmpty(title)) return;

                Page pagetoload = null;
                string Messagetoshow = "";

                //Questionnaires
                //var questionnaireArray = new Dictionary<string, string>
                //{
                //    { "Complete your EQ-5D Questionnaire", "A37CF880-080D-40D4-8A8D-1C0CEEC2FEBF" },
                //    { "Complete your SF-36 General Health Questionnaire", "DC6A9FD7-242B-4299-9672-D745669FEAF0" },
                //    { "Complete Your HOCM Baseline Questionnaire", "BE72B2A1-0707-4E8D-8E82-022BA4F959F4" }
                //};

                //if (questionnaireArray.TryGetValue(title, out string questionnaireId))
                //{
                //    Messagetoshow = "Loading Questionnaire ...";
                //    pagetoload = new AndroidQuestionnaires(questionnaireId, feedbackList);
                //}


                if (pagetoload != null)
                {
                    App.LoadingText = Messagetoshow;
                    var currentRoot = (Application.Current?.Windows.FirstOrDefault()?.Page as NavigationPage)?.RootPage;

                    if (currentRoot is MainDashboard)
                    {
                        await App.SetMainPageWithStack(new MainDashboard(), pagetoload);
                    }
                    else
                    {
                        App.PendingNavigation = async () => await App.SetMainPageWithStack(new MainDashboard(), pagetoload);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }


        public async Task HandleNotificationData(NSDictionary userInfo, bool FromStartup)
        {
            try
            {

                //local notification 
                if (userInfo.ContainsKey(jsonKey))
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await iOSlocalNotificationTapped(userInfo);

                    });
                    return;
                }

                //Push Notification
                var NewNotification = new ObservableCollection<pushdata>();

                NSDictionary dataToParse = userInfo.ContainsKey(new NSString("customData"))
            ? userInfo.ObjectForKey(new NSString("customData")) as NSDictionary
            : userInfo;

                if (dataToParse != null)
                {
                    foreach (var key in dataToParse.Keys)
                    {
                        var nsKey = key.ToString();
                        if (nsKey == "aps" || nsKey == "gcm.message_id" || nsKey == "google.c.a.e") continue;

                        NewNotification.Add(new pushdata
                        {
                            key = nsKey.ToLower(),
                            Data = dataToParse.ObjectForKey(key)?.ToString()
                        });
                    }
                }


                if (NewNotification.Count > 0)
                {
                    //Payload data with Action
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await App.PushNotificationTapped(NewNotification);

                    });
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}