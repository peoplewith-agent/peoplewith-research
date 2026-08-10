using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Mtp;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Navigation;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat;
using Microsoft.Maui.Storage;
using Plugin.Firebase.CloudMessaging;
using static Android.Provider.Settings;
//using Application = Android.App.Application;

namespace PeopleWithResearch
{
    [Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.SingleTop, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static readonly string CHANNEL_ID = "com.peoplewith.peoplewithresearch.general";
        private NotificationHubClient hub;
        private static string? GetDeviceId() => Secure.GetString(Android.App.Application.Context.ContentResolver, Secure.AndroidId);

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
                Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                Window.SetNavigationBarColor(Android.Graphics.Color.Transparent);

                // Set the screen orientation to portrait
                RequestedOrientation = ScreenOrientation.Portrait;

                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;

                try
                {
                    //Firebase.FirebaseApp.InitializeApp(this);


                    //var hubName = "PWDevHub";
                    //var connectionString = "Endpoint=sb://PWDevelopment.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=ZiwsFi5CJVNru6prZMix/55OIDEZJvXumOSBkRjU4gM="; // Can be found in Access policy. Use Listen connection
                    Firebase.FirebaseApp.InitializeApp(this);
                    hub = NotificationHubClient.CreateClientFromConnectionString(Constants.ListenConnectionString, Constants.NotificationHubName);

                    CreateNotificationChannel();
                    HandleNotificationIntent(Intent);
                    //CreateNotificationChannel(this);

                    //First Prompt to allow Notifications, From android 13 and above
                    //   if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) != Permission.Granted)
                    //  {
                    //      ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.PostNotifications }, 0);
                    //  }
                }
                catch (Exception ex)
                {
                }

                // Handle notification tap if the activity was launched from a notification
                if (Intent?.Extras != null)
                {
                    HandleNotificationTap(Intent);
                }
            }
            catch(Exception ex)
            {

            }

        }


        //protected override void OnNewIntent(Intent? intent)
        //{
        //    base.OnNewIntent(intent);
        //    HandleNotificationIntent(intent);
        //}

        private static void HandleNotificationIntent(Intent? intent)
        {
            if (intent?.Extras == null) return;

            //string? title = intent.GetStringExtra(
            //    NotificationManagerService.TitleKey);
            //string? message = intent.GetStringExtra(
            //    NotificationManagerService.MessageKey);

            //if (title is null || message is null) return;

            //var service = IPlatformApplication.Current?.Services
            //    .GetService<INotificationManagerService>();
            //service?.ReceiveNotification(title, message);
        }

        public async Task CreatePushNotificationChannel()
        {
            try
            {
                var deviceid = Preferences.Get("PW_DeviceToken", string.Empty);
                if (string.IsNullOrEmpty(deviceid))
                {
                    deviceid = Guid.NewGuid().ToString();
                    Preferences.Set("PW_DeviceToken", deviceid);
                }

                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                if (string.IsNullOrEmpty(token)) return; // Cannot register without a token
                Preferences.Set("fcm_token", token);

                // Build Tags
                var tags = new List<string>();
                if (!string.IsNullOrEmpty(Helpers.Settings.UsersID)) tags.Add(Helpers.Settings.UsersID);
                if (!string.IsNullOrEmpty(Helpers.Settings.SignUp)) tags.Add(Helpers.Settings.SignUp);

                //// Add Tags
                //if (!string.IsNullOrEmpty(Helpers.Settings.UserKey))
                //{
                //    var database = new APICalls();
                //    var newTags = await database.GetUserTags();
                //    if (newTags?.Count > 0) tags.AddRange(newTags);
                //}

                // Register with Azure
                var installation = new Microsoft.Azure.NotificationHubs.Installation
                {
                    InstallationId = deviceid,
                    PushChannel = token,
                    Platform = NotificationPlatform.FcmV1,
                    Tags = tags
                };

                await hub.CreateOrUpdateInstallationAsync(installation);
            }
            catch (Exception Ex)
            {

            }
        }

        void ConfigureSentryUserScope()
        {
            string userId = Preferences.Default.Get("userid", "Unknown");
            string userEmail = Preferences.Default.Get("email", "Unknown");

            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new SentryUser { Id = userId, Email = userEmail };
            });
        }

        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            ConfigureSentryUserScope();
            SentrySdk.CaptureException(e.Exception);
            SentrySdk.FlushAsync(TimeSpan.FromSeconds(2)).Wait();
        }

        private async void CreateNotificationChannel()
        {
            try
            {
                var deviceid = Preferences.Get("PW_DeviceToken", string.Empty);
                if (string.IsNullOrEmpty(deviceid))
                {
                    deviceid = Guid.NewGuid().ToString();
                    Preferences.Set("PW_DeviceToken", deviceid);
                }

                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                if (string.IsNullOrEmpty(token)) return; 
                Preferences.Set("fcm_token", token);

                // Build Tags
                var tags = new List<string>();
                if (!string.IsNullOrEmpty(Helpers.Settings.UsersID)) tags.Add(Helpers.Settings.UsersID);
                if (!string.IsNullOrEmpty(Helpers.Settings.SignUp)) tags.Add(Helpers.Settings.SignUp);

                // Add Tags
                //if (!string.IsNullOrEmpty(Helpers.Settings.UserKey))
                //{
                //    var database = new APICalls();
                //    var newTags = await database.GetUserTags();
                //    if (newTags?.Count > 0) tags.AddRange(newTags);
                //}

                // Register with Azure
                var installation = new Microsoft.Azure.NotificationHubs.Installation
                {
                    InstallationId = deviceid,
                    PushChannel = token,
                    Platform = NotificationPlatform.FcmV1,
                    Tags = tags
                };

                await hub.CreateOrUpdateInstallationAsync(installation);
            }
            catch (Exception Ex)
            {

            }
        }

        public static void CreateNotificationChannel(Activity activity)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

            var notificationManager = (NotificationManager)activity.GetSystemService(Context.NotificationService);

            // Build the Sound URI
            var soundUri = Android.Net.Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{activity.PackageName}/{Resource.Raw.pwjingo}");

            var audioAttributes = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Notification)
                .SetContentType(AudioContentType.Sonification)
                .Build();

            var channel = new NotificationChannel(CHANNEL_ID, "General", NotificationImportance.High)
            {
                Description = "PeopleWithResearch Push Notifications",
                LockscreenVisibility = NotificationVisibility.Public
            };

            channel.SetShowBadge(true);
            channel.SetSound(soundUri, audioAttributes);
            channel.EnableVibration(true);

            notificationManager.CreateNotificationChannel(channel);

            Plugin.Firebase.CloudMessaging.FirebaseCloudMessagingImplementation.ChannelId = CHANNEL_ID;
        }


        public override void OnBackPressed()
        {
            // Handle the back button press
            // You can show a dialog, navigate to a different page, or cancel the action

            // For example, to cancel the back button press:
            // base.OnBackPressed(); // Uncomment this line if you want to allow the default behavior

            // Alternatively, if you want to perform some action:
            // DisplayAlert("Back button pressed", "The back button was pressed and handled.", "OK");

            // To cancel the back button action, do not call the base method
        }

        // Add this method to handle the navigation

        //public override void HandleIntent(Intent? intent)
        //{
        //    base.HandleIntent(intent);
        //}
        protected override void OnNewIntent(Intent intent)
        {
            try
            {
                base.OnNewIntent(intent);

                // Handle notification tap if a new intent is received
                if (intent?.Extras != null)
                {
                    HandleNotificationTap(intent);
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void HandleNotificationTap(Intent intent)
        {
            try
            {
                string studyidfornotification = intent.GetStringExtra("studyid");
                if (studyidfornotification == "IID3")
                {
                    var action = intent.GetStringExtra("action");
                    var questionnaire = intent.GetStringExtra("questionnaire");
                    var textsummary = intent.GetStringExtra("textsummary");
                    var questionnaireid = intent.GetStringExtra("questionnaireid");
                    string[] originalArray = { action, studyidfornotification, questionnaire, questionnaireid, textsummary };


                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                         //   await NavigationPage.Navigation.PushAsync(new NotificationQuestion(originalArray));
                        });
                    
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                var errorMessage = ex.StackTrace.ToString();
                // Consider using a logging framework or analytics to track this error
            }
        }

    }
}