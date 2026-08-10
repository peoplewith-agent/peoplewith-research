using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System.Threading.Tasks;
using PeopleWithResearch;
using PeopleWithResearch.Droid;
using Application = Android.App.Application;

[assembly: Dependency(typeof(NotificationPermissionServiceAndroid))]

namespace PeopleWithResearch.Droid
{
    public class NotificationPermissionServiceAndroid : INotificationPermissionService
    {
        public Task<bool> RequestNotificationPermissions()
        {
            // Open notification settings for the app
            try
            {

                Intent intent = new Intent(Android.Provider.Settings.ActionAppNotificationSettings);
                intent.PutExtra(Android.Provider.Settings.ExtraAppPackage, Application.Context.PackageName);
                intent.AddFlags(ActivityFlags.NewTask); // Add the FLAG_ACTIVITY_NEW_TASK flag
                Application.Context.StartActivity(intent);

                return Task.FromResult(true); // Permissions are already granted
            }
            catch(Exception ex)
            {
                return Task.FromResult(false);
            }
        }
    }
}