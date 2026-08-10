using Android.App;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using PeopleWithResearch;
using System;


namespace PeopleWithResearch
{
    public class NotificationSettingsService : INotificationSettingsService
    {
        public Task<bool> IsNotificationsEnabledAsync()
        {
            var manager = NotificationManagerCompat.From(Platform.AppContext);

            bool enabled = manager.AreNotificationsEnabled();

            // Optional: handle Android 13+
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
            {
                var permission = Android.Manifest.Permission.PostNotifications;
                enabled &= AndroidX.Core.Content.ContextCompat.CheckSelfPermission(
                    Platform.AppContext, permission) == Permission.Granted;
            }

            return Task.FromResult(enabled);
        }

        public void OpenSettings()
        {
            var intent = new Intent(Android.Provider.Settings.ActionAppNotificationSettings);
            intent.PutExtra(Android.Provider.Settings.ExtraAppPackage, Platform.AppContext.PackageName);
            intent.AddFlags(ActivityFlags.NewTask);
            Platform.AppContext.StartActivity(intent);
        }
    }
}
