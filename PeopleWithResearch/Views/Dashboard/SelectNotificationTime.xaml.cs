using Microsoft.Maui;
//using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Devices;
using Mopups.Services;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace PeopleWithResearch
{
    public partial class SelectNotificationTime : Mopups.Pages.PopupPage
    {
        public ObservableCollection<string> HoursList { get; set; }
        //public DateTime updatedDateTime = new DateTime();
        //public ObservableCollection<newuser> userpassed = new();
        UserNotifications AddDailyNotification = new();
        public SelectNotificationTime()
        {
            InitializeComponent();
            detailslbl.Text = $"Hi {Helpers.Settings.FirstName}! {LocalizationManager.Get("Notification_Detail")}";

            HoursList = new ObservableCollection<string>();
            //Code to test 
            //var now = DateTime.Now;
            //for (int i = 0; i < 5; i++)
            //{
            //    var newTime = now.AddMinutes(i * 2);
            //    HoursList.Add(newTime.ToString("HH:mm"));
            //}
            foreach (int hour in Enumerable.Range(0, 24))
            {
                HoursList.Add($"{hour:D2}:00");
            }

            listView.ItemsSource = HoursList;
            var time = Preferences.Default.Get("notificationtime", string.Empty);
            if (!string.IsNullOrEmpty(time))
            {
                listView.SelectedItem = time;
            }
       
            //getuserdetails();
        }
        //async void getuserdetails()
        //{
        //    try 
        //    { 
        //        userpassed = await APICalls.Instance.Getuser(); 
        //    }
        //    catch (Exception ex) { }
        //}
        private void listView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try { Errorlbl.IsVisible = false; } catch (Exception Ex) { CrashDetected.LogCrash(Ex, Navigation, "listView_ItemTapped"); }
        }

        private async Task<bool> EnsureNotificationPermissionAsync()
        {
            try
            {
                var hasPermission = await LocalNotificationCenter.Current.AreNotificationsEnabled();
                if (!hasPermission)
                {
                    hasPermission = await LocalNotificationCenter.Current.RequestNotificationPermission();
                }
                return hasPermission;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "EnsureNotificationPermissionAsync");
                return false;
            }
        }

        private async void btnsubmit_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (listView.SelectedItem is not string selectedHour)
                {
                    Errorlbl.IsVisible = true;
                    Vibration.Vibrate();
                    return;
                }

                bool notificationsAllowed = await EnsureNotificationPermissionAsync();

                if (!notificationsAllowed) {   // Do Something
                }

                Timelbl.Text = selectedHour;
                await TimeStack.FadeTo(0, 250);
                TimeStack.IsVisible = false;
                CompletedStack.IsVisible = true;
                await CompletedStack.FadeTo(1, 400);
               
                Preferences.Default.Set("notificationtime", selectedHour);
                await AddDailyNotification.ScheduleDailyNotification();

                await Task.Delay(3000);
                await MopupService.Instance.PopAsync();
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "btnsubmit_Clicked");
            }
        }
    }
}