using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Firebase.Messaging;
using Microsoft.Maui.Controls;
using PeopleWithResearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using Resource = Microsoft.Maui.Resource;

namespace PeopleWithResearch
{
    [Service(Name = "com.peoplewith.peoplewithresearch.FirebaseService", Exported = true, Permission = "com.google.android.c2dm.permission.SEND")]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseService : FirebaseMessagingService
    {
        const string CHANNEL_ID = "com.peoplewith.peoplewithresearch.general";
        ObservableCollection<pushdata> NotificationData = new();

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            HandleMessageAsync(message).GetAwaiter().GetResult();
        }

        private async Task HandleMessageAsync(RemoteMessage message)
        {
            try
            {
                if (message == null) return;

                string title = "Notification";
                string body = "You have a new message.";

                if (message.GetNotification() != null)
                {
                    title = message.GetNotification().Title ?? title;
                    body = message.GetNotification().Body ?? body;
                }

                NotificationData.Clear();
                if (message.Data != null)
                {
                    if (message.Data.TryGetValue("title", out var dataTitle)) title = dataTitle;

                    if (message.Data.TryGetValue("body", out var dataBody)) body = dataBody;

                    foreach (var item in message.Data)
                    {
                        NotificationData.Add(new pushdata { key = item.Key, Data = item.Value });
                    }
                }

                await ShowNotification(title, body, NotificationData);
            }
            catch (Exception Ex)
            {

            }
        }

        private async Task ShowNotification(string title, string body, ObservableCollection<pushdata> ExtraData)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            foreach (var item in ExtraData)
            {
                intent.PutExtra(item.key, item.Data);
            }

            var pendingIntent = PendingIntent.GetActivity(
                this, 0, intent,
                PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
            );

            var soundUri = Android.Net.Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{PackageName}/{Resource.Raw.pwjingo}");

            var notificationBuilder = new NotificationCompat.Builder(this, CHANNEL_ID)
                //.SetColor(Android.Graphics.Color.ParseColor("#004B87"))
                //.SetSmallIcon(Resource.Drawable.pwicon)
                .SetSmallIcon(PeopleWithResearch.Resource.Drawable.pwrappicon)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetAutoCancel(true)
                .SetSound(soundUri)
                .SetPriority((int)NotificationPriority.High)
                .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(new Random().Next(), notificationBuilder.Build());

        }
    }
}