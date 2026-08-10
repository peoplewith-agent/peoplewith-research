using Foundation;
using Microsoft.Azure.NotificationHubs;
using PeopleWithResearch;
using Plugin.LocalNotification;
using UIKit;
using UserNotifications;

namespace PeopleWithResearch
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            try
            {
                return base.FinishedLaunching(app, options);
            }
            catch(Exception Ex)
            {
                return false;
            }
        }
        [Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
        public async void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            try
            {

            //Lock Font Size 
            UIKit.UIFont.GetPreferredFontForTextStyle(UIKit.UIFontTextStyle.Body).WithSize(16);

                //string token = null!;
                //if (deviceToken.Length > 0)
                //{
                //    if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                //    {
                //        var data = deviceToken.ToArray();
                //        token = BitConverter
                //            .ToString(data)
                //            .Replace("-", "")
                //            .Replace("\"", "");
                //    }
                //    else if (!string.IsNullOrEmpty(deviceToken.Description))
                //    {
                //        token = deviceToken.Description.Trim('<', '>');
                //    }
                //}
                //var hubName = "PWDevHub";
                //var connectionString = "Endpoint=sb://PWDevelopment.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=ZiwsFi5CJVNru6prZMix/55OIDEZJvXumOSBkRjU4gM="; // Can be found in Access policy. Use Listen connection
                //try
                //{
                //    var hub = NotificationHubClient.CreateClientFromConnectionString(connectionString, hubName);
                //    var installation = new Installation
                //    {
                //        InstallationId = token,
                //        PushChannel = token,
                //        Platform = NotificationPlatform.Apns,
                //        Tags = ["MarkTestTag"]
                //    };
                //    await hub.CreateOrUpdateInstallationAsync(installation);
                //}
                //catch (Exception ex) { }
                string token = null;
                if (deviceToken.Length > 0)
                {
                    var data = deviceToken.ToArray();
                    token = BitConverter.ToString(data).Replace("-", "").Replace("\"", "");
                    Preferences.Set("fcm_token", token);
                }
                var notifyService = new NotificationService();
                await notifyService.AddTag();

            }
            catch (Exception ex)
            {

            }
        }
    }
}