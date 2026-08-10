using Microsoft.Azure.NotificationHubs;
using Microsoft.Maui.Storage;
#if ANDROID
using Plugin.Firebase.CloudMessaging;
using static Android.Provider.Settings;
#endif


namespace PeopleWithResearch
{
    public class NotificationService
    {
        //private NotificationHubClient hub;
        //private string deviceid;
        //private string token;

        private readonly NotificationHubClient _hub;
        private string _deviceId;
        private string _token;

        public NotificationService()
        {
            //hub = NotificationHubClient.CreateClientFromConnectionString(Constants.ListenConnectionString, Constants.NotificationHubName);

            _hub = NotificationHubClient.CreateClientFromConnectionString(
            Constants.ListenConnectionString,
            Constants.NotificationHubName);
        }

        private async Task GetDeviceToken()
        {
            _deviceId = Preferences.Get("PW_DeviceToken", string.Empty);
            if (string.IsNullOrEmpty(_deviceId))
            {
                _deviceId = Guid.NewGuid().ToString();
                Preferences.Set("PW_DeviceToken", _deviceId);
            }

#if ANDROID
            _token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            if (!string.IsNullOrEmpty(_token))
            {
                Preferences.Set("fcm_token", _token);
            }
#elif IOS
        _token = Preferences.Get("fcm_token", string.Empty);
#endif
        }

        private NotificationPlatform GetCurrentPlatform()
        {
            return DeviceInfo.Current.Platform == DevicePlatform.Android
                ? NotificationPlatform.FcmV1
                : NotificationPlatform.Apns;
        }

        public async Task AddTag()
        {
            try
            {
                await GetDeviceToken();
                if (string.IsNullOrEmpty(_token)) return;

                var tags = new List<string>();
                if (!string.IsNullOrEmpty(Helpers.Settings.SignUp)) tags.Add(Helpers.Settings.SignUp);
                if (!string.IsNullOrEmpty(Helpers.Settings.UsersID)) tags.Add(Helpers.Settings.UsersID);

                var installation = new Installation
                {
                    InstallationId = _deviceId,
                    PushChannel = _token,
                    Tags = tags,
                    Platform = GetCurrentPlatform()
                };

                await _hub.CreateOrUpdateInstallationAsync(installation);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task ClearTagsAsync()
        {
            try
            {
                await GetDeviceToken();
                if (string.IsNullOrEmpty(_token)) return;

                var installation = new Installation
                {
                    InstallationId = _deviceId,
                    PushChannel = _token,
                    Tags = new List<string>(),
                    Platform = GetCurrentPlatform()
                };

                await _hub.CreateOrUpdateInstallationAsync(installation);
            }
            catch (Exception ex)
            {

            }
        }
    

    //        private async Task GetDeviceToken()
    //        {
    //            deviceid = Preferences.Get("PW_DeviceToken", string.Empty);
    //            if (string.IsNullOrEmpty(deviceid))
    //            {
    //                deviceid = Guid.NewGuid().ToString();
    //                Preferences.Set("PW_DeviceToken", deviceid);
    //            }

    //#if ANDROID

    //            token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
    //#elif IOS
    //            token = Preferences.Get("fcm_token", string.Empty);
    //#endif
    //        }

    //        public async Task ClearTagsAsync()
    //        {
    //            try
    //            {
    //                await GetDeviceToken();

    //                if (string.IsNullOrEmpty(token)) return;

    //                var installation = new Installation
    //                {
    //                    InstallationId = deviceid,
    //                    PushChannel = token,
    //                    Tags = new List<string>(), 
    //                    Platform = DeviceInfo.Current.Platform == DevicePlatform.Android
    //                            ? NotificationPlatform.FcmV1
    //                            : NotificationPlatform.Apns
    //                };

    //                await hub.CreateOrUpdateInstallationAsync(installation);
    //            }
    //            catch (Exception ex)
    //            {
    //                //Add AppCenter
    //            }
    //        }

    //        public async Task AddTag()
    //        {
    //            try
    //            {
    //                await GetDeviceToken();

    //                if (string.IsNullOrEmpty(token)) return;

    //                var tags = new List<string>();
    //                if (!string.IsNullOrEmpty(Helpers.Settings.SignUp)) tags.Add(Helpers.Settings.SignUp);
    //                if (!string.IsNullOrEmpty(Helpers.Settings.UsersID)) tags.Add(Helpers.Settings.UsersID);

    //                var installation = new Installation
    //                {
    //                    InstallationId = deviceid,
    //                    PushChannel = token,
    //                    Tags = tags,
    //                    Platform = DeviceInfo.Current.Platform == DevicePlatform.Android
    //                               ? NotificationPlatform.FcmV1
    //                               : NotificationPlatform.Apns
    //                };

    //                await hub.CreateOrUpdateInstallationAsync(installation);

    //            }
    //            catch (Exception ex)
    //            {
    //                //Add AppCenter
    //            }
    //        }
}
}