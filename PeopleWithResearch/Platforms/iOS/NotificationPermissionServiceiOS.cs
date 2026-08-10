using System.Threading.Tasks;
using PeopleWithResearch;
using UIKit;
using Foundation;
using UserNotifications;
using PeopleWithResearch;
using PeopleWithResearch.iOS;
[assembly: Dependency(typeof(NotificationPermissionServiceiOS))]
namespace PeopleWithResearch.iOS
{
    public class NotificationPermissionServiceiOS : INotificationPermissionService
    {
        public async Task<bool> RequestNotificationPermissions()
        {
            // Open app settings
            // Open app settings
            UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenNotificationSettingsUrl));
            // Await the result of the authorization request
            return true;
        }
    }
}