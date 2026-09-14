using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Syncfusion.Maui.Core.Hosting;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;
using CommunityToolkit.Maui;
using Maui.FreakyControls.Extensions;
using System.Globalization;
using Microsoft.Maui.LifecycleEvents;
using Plugin.LocalNotification.AndroidOption;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
//using Sentry;
using Microsoft.Maui.Controls.Compatibility.Hosting;

#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

#if ANDROID
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Core.Platforms.Android;
using Android.OS;
#endif

#if IOS
using UIKit;
using CoreGraphics;
using Foundation;
using UserNotifications;
#endif


namespace PeopleWithResearch
{
    public static class MauiProgram
    {
        #if IOS
               private static UserNotificaitonCenterDelegate notificationDelegate;
        #endif
        public static MauiApp CreateMauiApp()
        {

            //Register Local Notification tap (Android) 
            LocalNotificationCenter.Current.NotificationActionTapped += (e) =>
            {
                App.HandleLocalNotificationTap(e);
            };

            var builder = MauiApp.CreateBuilder();
#if MAUI_DEVFLOW
            builder.AddMauiDevFlowAgent();
#endif
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore() 
                .UseMauiCommunityToolkit()
                .ConfigureMopups()
                .UseMauiCompatibility()
                .UseMauiCommunityToolkitCamera()
                .InitializeFreakyControls(useSkiaSharp: true)
                .UseLocalNotification(config =>
                 {
                     config.AddAndroid(android =>
                     {
                         android.AddChannel(new AndroidNotificationChannelRequest
                         {
                             Id = "pwr_notifications",
                             Name = "Research Notifications",
                             EnableSound = true,
                             Sound = "pwjingo",
                             Importance = AndroidImportance.Max
                         });
                     });
                 })

                                     .UseSentry(options =>
                                     {
                                         // The DSN is the only required setting.
                                         options.Dsn = "https://0448e8c255cd049faa169c8c7c9bc6ee@o4511862606331904.ingest.de.sentry.io/4511862626713680";

#if ANDROID
                                         options.Native.EnableActivityLifecycleBreadcrumbs = false;
                                         options.Native.EnableAppComponentBreadcrumbs = false;
                                         options.Native.EnableAppLifecycleBreadcrumbs = false;
                                         options.Native.EnableNetworkEventBreadcrumbs = false;
                                         options.Native.EnableSystemEventBreadcrumbs = false;
                                         options.Native.EnableUserInteractionBreadcrumbs = false;
                                         options.Android.SuppressSegfaults = false;
#endif


#if DEBUG
                                         options.Debug = true;
#else
                                         options.Debug = false;
#endif
                                    
                                         options.AttachStacktrace = true;
                                         // Set TracesSampleRate to 1.0 to capture 100% of transactions for tracing.
                                         // We recommend adjusting this value in production.
                                         options.TracesSampleRate = 1.0;
                                         options.SendDefaultPii = true;
                                         options.MaxBreadcrumbs = 1000;

                                         options.IncludeTextInBreadcrumbs = true;
                                         options.IncludeTitleInBreadcrumbs = true;
                                         options.IncludeBackgroundingStateInBreadcrumbs = true;
                                         //options.CaptureFailedRequests = true;

                                         //options.AttachScreenshot = true;
                                         //options.EnableAndroidNativeNdk = true;
                                         //options.EnableXamarinSupport = true;

                                         options.SetBeforeSend((sentryEvent, hint) =>
                                         {
                                             if (sentryEvent.Exception is TaskCanceledException or System.OperationCanceledException)
                                                 return null; // drop expected debounce cancellations
                                             return sentryEvent;
                                         });

                                     })
                  .ConfigureLifecycleEvents(events =>
                  {
#if ANDROID

                      events.AddAndroid(android => android
          .OnCreate((activity, bundle) =>
          {
              MainActivity.CreateNotificationChannel(activity);
              //FirebaseCloudMessagingImplementation.SmallIconRef = PeopleWithResearch.Resource.Drawable.pwrappicon;
            var smallIconRef = activity.Resources?.GetIdentifier("pwicon", "drawable", activity.PackageName) ?? 0;
            if (smallIconRef != 0)
            {
                FirebaseCloudMessagingImplementation.SmallIconRef = smallIconRef;
            }
              // Match this to the ID in your CreateNotificationChannel method
              FirebaseCloudMessagingImplementation.ChannelId = "com.peoplewith.peoplewithresearch.general";

              CrossFirebase.Initialize(activity, () => activity);
          }));
#endif

#if IOS
                    events.AddiOS(ios => ios
                    .FinishedLaunching((app, options) => 
                    {
                        // 1. Instantiate and assign the delegate
                        notificationDelegate = new UserNotificaitonCenterDelegate();
                        UNUserNotificationCenter.Current.Delegate = notificationDelegate;

                        // 2. Handle cold start notification taps
                        if (options != null && options.ContainsKey(UIKit.UIApplication.LaunchOptionsRemoteNotificationKey))
                        {
                            var userInfo = options[UIKit.UIApplication.LaunchOptionsRemoteNotificationKey] as Foundation.NSDictionary;
                            if (userInfo != null)
                            {
                                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
                                {
                                    await notificationDelegate.HandleNotificationData(userInfo, true);
                                });
                            }
                        }
                        
                        // 3. Request authorization (can also be done here or kept in AppDelegate)
                        UNUserNotificationCenter.Current.RequestAuthorization(
                            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge, 
                            (approved, err) => 
                            {
                                if (approved)
                                {
                                    Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => 
                                    {
                                        UIKit.UIApplication.SharedApplication.RegisterForRemoteNotifications();
                                    });
                                }
                            });

                        return true; 
                    }));
#endif
                  })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("HankenGrotesk-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("HankenGrotesk-Bold.ttf", "OpenSansSemibold");
                    // NotoSansGujarati-Regular.ttf must be placed in PeopleWithResearch/Resources/Fonts/
                    try { fonts.AddFont("NotoSansGujarati-Regular.ttf", "NotoSansGujarati"); } catch { /* font file not present at build time — Gujarati will fall back to system font */ }
                });


            //Entry Remove Border 
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("TrulyBorderless", (handler, view) =>
            {
#if ANDROID
                // Remove the tinted underline
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS 
                // Remove the border style, the layer border, and ensure clear background
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None; 
            handler.PlatformView.Layer.BorderWidth = 0;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
            });

            // Editor Remove Border
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("TrulyBorderless", (handler, view) =>
            {
#if ANDROID
                // Remove the tinted underline
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
    // For Editor (UITextView), we set BorderWidth to 0 and Background to Clear
    handler.PlatformView.Layer.BorderWidth = 0;
    handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#elif WINDOWS
    handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
#endif
            });


#if ANDROID
            builder.Services.AddSingleton<INotificationSettingsService, NotificationSettingsService>();
#elif IOS
builder.Services.AddSingleton<INotificationSettingsService, NotificationSettingsService>();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new SentryUser();
            });


            void ConfigureSentryUserScope()
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    string UserID = Preferences.Default.Get("userid", "Unknown");
                    string UserEmail = Preferences.Default.Get("email", "Unknown");
                    scope.User = new SentryUser
                    {
                        Id = UserID,
                        Email = UserEmail
                    };
                });
            }

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                //ConfigureSentryUserScope();
                SentrySdk.CaptureException(e.ExceptionObject as Exception);
                SentrySdk.FlushAsync(TimeSpan.FromSeconds(2)).Wait();
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                //ConfigureSentryUserScope();
                SentrySdk.CaptureException(e.Exception);
                SentrySdk.FlushAsync(TimeSpan.FromSeconds(2)).Wait();
            };

            return builder.Build();
        }
    }
}
