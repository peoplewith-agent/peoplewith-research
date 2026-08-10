using System;
namespace PeopleWithResearch
{
    public interface INotificationSettingsService
    {
        Task<bool> IsNotificationsEnabledAsync();
        void OpenSettings();
    }
}

