using UserNotifications;
using UIKit;
using PeopleWithResearch;

namespace PeopleWithResearch;

public class NotificationSettingsService : INotificationSettingsService
{
    public async Task<bool> IsNotificationsEnabledAsync()
    {
        var settings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync();
        return settings.AuthorizationStatus == UNAuthorizationStatus.Authorized;
    }

    public void OpenSettings()
    {
        var url = new Foundation.NSUrl(UIKit.UIApplication.OpenSettingsUrlString);

        if (UIApplication.SharedApplication.CanOpenUrl(url))
        {
            UIApplication.SharedApplication.OpenUrl(url);
        }
    }
}