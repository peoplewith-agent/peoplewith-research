using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Plugin.LocalNotification;
using Microsoft.Maui.Storage;
using System.Net;
using PeopleWithResearch;

namespace PeopleWithResearch
{
    public class Newlogout
    {
        string UserAction; 
      
        public Newlogout(string Action)
        {
            //Delete -or- Logout
            UserAction = Action;
            _ = ClearAllData();
        }


        public async Task ClearAllData()
        {
            try
            {
                await ClearNotifications();
                await ClearUserInfo();
                await Task.Delay(300);
                await App.SetMainPage(new NewMainPage());
            }
            catch (Exception ex) when (
        ex is HttpRequestException ||
        ex is WebException ||
        ex is TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        async public Task ClearNotifications()
        {
            try
            {
                // Clear local notifications
                LocalNotificationCenter.Current.ClearAll();
                LocalNotificationCenter.Current.CancelAll();

                // Clear Azure tags
                var notificationService = new NotificationService();
                await notificationService.ClearTagsAsync();
            }
            catch (Exception ex) when (
        ex is HttpRequestException ||
        ex is WebException ||
        ex is TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public async Task ClearUserInfo()
        {
            try
            {
                //Clear Everything use this
                Preferences.Clear(); 
            }
            catch (Exception ex) when (
         ex is HttpRequestException ||
         ex is WebException ||
         ex is TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}