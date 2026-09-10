using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Mopups.Services;
using PeopleWithResearch;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;
using Plugin.LocalNotification.EventArgs;
//using Sentry;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public partial class App : Application
    {
        private readonly ConnectivityService _connectivityService;

        public static string LoadingText { get; set; } = "Loading ...";
        public static bool IsRunning { get; private set; } = false;
        public static Func<Task>? PendingNavigation { get; set; } = null;
        public UserNotifications UpdateNotification = new();
        private const string DailyNotificationText = "HOPPER Study - Action Required";

        public App()
        {
            // Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NBaF5cXmZCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXlfdHRdRWFYVkVwXEU="); //NEW
            InitializeComponent();
            _connectivityService = new ConnectivityService();
            SubscribeConnectivity();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH5cdnRRRWRfVENzWEFWYEg=");
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                LocalNotificationCenter.Current.NotificationReceived += OnNotificationReceived;
            }
            //MainPage = new AppShell();
        }

        private async void OnNotificationReceived(NotificationEventArgs e)
        {
            if(DeviceInfo.Platform == DevicePlatform.Android)
            {
                var repeatType = e.Request.Schedule?.RepeatType ?? NotificationRepeat.No;
                var isDailyNotification = repeatType == NotificationRepeat.Daily
                    || e.Request.Description == DailyNotificationText;

                if (isDailyNotification)
                {
                    //Skip true 
                    await UpdateNotification.ScheduleDailyNotification();
                }
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return new Window(new NavigationPage(new NewMainPage()));
            }
            else
            {
                return new Window(new AppShell());
            }
        }

        private void SubscribeConnectivity()
        {
            _connectivityService.ConnectivityChanged += ConnectivityHandler;
        }

        private void UnsubscribeConnectivity()
        {
            _connectivityService.ConnectivityChanged -= ConnectivityHandler;
        }

        protected async override void OnStart()
        {
            try
            {
                base.OnStart();
                IsRunning = true;

                await CheckIfUserIsLoggedIn();
                await Checkifappisupdated();
               
            
            }
            catch (Exception Ex)
            {

            }
        }


        private async Task Checkifappisupdated()
        {
            try
            {
                var versionCheckService = new VersionCheckService();
                bool Check = await versionCheckService.CheckForUpdate();
                if (Check)
                {
                    await MainPage.Navigation.PushAsync(new UpdatePage(), false);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async Task CheckIfUserIsLoggedIn()
        {
            try
            {
                var userid = Preferences.Default.Get("userid", string.Empty);
                if (!string.IsNullOrEmpty(userid))
                {

                    if(!string.IsNullOrEmpty(Helpers.Settings.PrimaryUserID))
                    {
                        if(Helpers.Settings.UsersID != Helpers.Settings.PrimaryUserID)
                        {
                            Preferences.Default.Set("userid", Helpers.Settings.PrimaryUserID);
                        }
                    }


                    await SetMainPage(new ImperialDashboard());
                }
            }
            catch (Exception ex)
            {
            }
        }
        protected async override void OnSleep()
        {
            try
            {
                UnsubscribeConnectivity();
                IsRunning = false;
            }
            catch (Exception Ex)
            {
            }
        }
        protected async override void OnResume()
        {
            try
            {
                base.OnResume();
                IsRunning = true;
                SubscribeConnectivity();
                WeakReferenceMessenger.Default.Send(new RefreshNotificationDash("Refresh"));
            }
            catch (Exception Ex)
            {

            }
        }
        public static async Task SetMainPage(Page newRootPage)
        {
            var app = Application.Current;
            if (app == null) return;

            Window window = app.Windows.FirstOrDefault();
            int attempts = 0;
            while (window == null && attempts < 5)
            {
                await Task.Delay(100);
                window = app.Windows.FirstOrDefault();
                attempts++;
            }

            if (window != null)
            {
                if (newRootPage is not NavigationPage && newRootPage is not Shell)
                {
                    newRootPage = new NavigationPage(newRootPage);
                }

                window.Page = newRootPage;
            }
        }

        public static async Task SetMainPageWithStack(params Page[] pages)
        {
            try
            {
                if (pages == null || pages.Length == 0) return;

                await MopupService.Instance.PushAsync(new PopupPageHelper(App.LoadingText));

                var window = Application.Current?.Windows.FirstOrDefault();
                if (window != null)
                {
                    var navPage = new NavigationPage(pages[0]);
                    for (int i = 1; i < pages.Length; i++)
                    {
                        await navPage.Navigation.PushAsync(pages[i], false);
                    }

                    window.Page = navPage;
                }

                await Task.Yield();
                await MopupService.Instance.PopAllAsync(false);

            }
            catch (Exception Ex)
            {

            }
        }

        private async void ConnectivityHandler(object sender, bool isConnected)
        {
            try
            {
                if (MopupService.Instance.PopupStack.Count > 0)
                {
                    await MopupService.Instance.PopAsync();
                }
                if (!isConnected)
                {
                    if (!(Application.Current.MainPage is NoInternetPage))
                    {
                        await Application.Current.MainPage.Navigation.PushAsync(new NoInternetPage());
                    }
                }
                else
                {
                    var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (mainPage != null)
                    {
                        var pageToRemove = mainPage.Navigation.NavigationStack.FirstOrDefault(p => p is NoInternetPage);
                        if (pageToRemove != null)
                        {
                            mainPage.Navigation.RemovePage(pageToRemove);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
            }
        }

        public static void HandleLocalNotificationTap(NotificationActionEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LocalNotificationTapped(e);
            });
        }

        private static async Task LocalNotificationTapped(NotificationActionEventArgs e)
        {
            try
            {
                string userid = Preferences.Default.Get("userid", string.Empty);
                if (String.IsNullOrEmpty("userid")) return;

                var currentRoot = (Application.Current?.Windows.FirstOrDefault()?.Page as NavigationPage)?.RootPage;

                string AddData = e.Request.ReturningData;
                if (!string.IsNullOrEmpty(AddData))
                {
                    var notificationData = JsonSerializer.Deserialize<ObservableCollection<pushdata>>(AddData);

                    if (notificationData != null)
                    {
                        await PushNotificationTapped(notificationData);
                    }
                }
                else
                {

                //    if (e.Request.Title == "Medication Reminder" || e.Request.Title == "Supplement Reminder")
                //    {
                //        App.LoadingText = "Loading Schedule ...";

                //        var userfeedbacklist = await App.GetUserFeedback();
                //        if (userfeedbacklist != null)
                //        {
                //            if (currentRoot is MainDashboard)
                //            {
                //                await SetMainPageWithStack(new MainDashboard(), new MainSchedule(userfeedbacklist));
                //            }
                //            else
                //            {
                //                //Cold Start
                //                App.PendingNavigation = async () =>
                //                {
                //                    await SetMainPageWithStack(new MainDashboard(), new MainSchedule(userfeedbacklist));
                //                };
                //            }
                //        }
                //        return;
                //    }


                //    string QuestionnaireID = string.Empty;
                //    if (e.Request.Title == "Complete your EQ-5D Questionnaire")
                //    {
                //        QuestionnaireID = "A37CF880-080D-40D4-8A8D-1C0CEEC2FEBF";
                //    }
                //    else if (e.Request.Title == "Complete your SF-36 General Health Questionnaire")
                //    {
                //        QuestionnaireID = "DC6A9FD7-242B-4299-9672-D745669FEAF0";
                //    }
                //    else if (e.Request.Title == "Complete Your HOCM Baseline Questionnaire")
                //    {
                //        QuestionnaireID = "BE72B2A1-0707-4E8D-8E82-022BA4F959F4";
                //    }
                //    if (!String.IsNullOrEmpty(QuestionnaireID))
                //    {
                //        var userfeedbacklist = await App.GetUserFeedback();
                //        if (userfeedbacklist != null)
                //        {
                //            if (DeviceInfo.Platform == DevicePlatform.iOS)
                //            {
                //                App.LoadingText = "Loading Questionnaire ...";
                //                await SetMainPageWithStack(new MainDashboard(), new AndroidQuestionnaires(QuestionnaireID, userfeedbacklist));
                //            }
                //            else
                //            {
                //                App.LoadingText = "Loading Questionnaire ...";
                //                await SetMainPageWithStack(new MainDashboard(), new AndroidONLYQuestionnaires(QuestionnaireID, userfeedbacklist));
                //            }
                //        }
                //    }
                }
            }
            catch (Exception Ex)
            {

            }
        }

        public static async Task PushNotificationTapped(ObservableCollection<pushdata> NewNotification)
        {
            try
            {
                if (NewNotification == null) return;

                //Local Notification Saving (Make sure not Local Notification)
                if (NewNotification.Any(x => x.key == "Plugin.LocalNotification.RETURN_REQUEST"))
                {
                    await App.SetMainPage(new NavigationPage(new MainDashboard()));
                }

                var action = NewNotification.FirstOrDefault(x => x.key?.ToLower() == "action")?.Data?.ToLower() ?? string.Empty;
                var userid = Preferences.Default.Get("userid", string.Empty);
                var pincode = Preferences.Default.Get("pincode", string.Empty);

                if (string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(pincode) || string.IsNullOrEmpty(action)) return;

                // 3. Prepare Common Data
                var currentRoot = (Application.Current?.Windows.FirstOrDefault()?.Page as NavigationPage)?.RootPage;
                Page[] pages = null;
                string type = string.Empty;


                if (action.Contains("info"))
                {
                    if (currentRoot is not MainDashboard)
                    {
                        await App.SetMainPage(new NavigationPage(new MainDashboard()));
                        await Task.Delay(100);
                    }
                    await MopupService.Instance.PushAsync(new Infopopup(NewNotification));
                    return;
                }


                //if (action.Contains("question"))
                //{
                //    type = "Questionnaire";
                //    var QuestionId = NewNotification.FirstOrDefault(x => x.key?.ToLower() == "questionnaireid")?.Data;
                //    if (!string.IsNullOrEmpty(QuestionId))
                //    {
                //        Page questionPage = DeviceInfo.Platform == DevicePlatform.iOS
                //            ? new AndroidQuestionnaires(QuestionId, userFeedback)
                //            : new AndroidONLYQuestionnaires(QuestionId, userFeedback);

                //        pages = new Page[] { new MainDashboard(), questionPage };
                //    }
                //}
                //else if (action.Contains("med"))
                //{
                //    type = "Medications";
                //    pages = new Page[] { new MainDashboard(), new AllMedications(userFeedback) };
                //}
                //else if (action.Contains("diag"))
                //{
                //    type = "Medications";
                //    pages = new Page[] { new MainDashboard(), new AllDiagnosis(userFeedback) };
                //}
                //else if (action.Contains("meas"))
                //{
                //    type = "Measurements";
                //    pages = new Page[] { new MainDashboard(), new MeasurementsPage(userFeedback) };
                //}
                //else if (action.Contains("symp"))
                //{
                //    type = "Measurements";
                //    pages = new Page[] { new MainDashboard(), new AllSymptoms(userFeedback) };
                //}

                //Carry out Navigation 
                if (pages != null)
                {
                    App.LoadingText = $"Loading {type} ...";
                    if (currentRoot is MainDashboard)
                    {
                        await SetMainPageWithStack(pages);
                    }
                    else
                    {
                        // Cold Start
                        App.PendingNavigation = async () => await SetMainPageWithStack(pages);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }





        //public static async Task<Dictionary<string, string>> LoadAPIKeyAsync()
        //{
        //    Dictionary<string, string> APIKey = new Dictionary<string, string>();
        //    ////IOS////
        //    //    await SecureStorage.SetAsync("zumo-api-key", "iuwdbiuwdhhe2uh2eiuh2hd29h2e98h2u9h98hd98hhh29eh298h829h89h");
        //    //    var key = await SecureStorage.GetAsync("zumo-api-key");
        //    //    APIKey.Add("zumo-api-key", key);
        //    //    return APIKey;
        //    ////TEMP WORKAROUND DUE TO PACKAGE COMAPTBILITY ISSUE ON IOS////
        //    APIKey.Add("zumo-api-key", "iuwdbiuwdhhe2uh2eiuh2hd29h2e98h2u9h98hd98hhh29eh298h829h89h");
        //    return APIKey;
        //    ////
        //}
    }
}
