using System;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class VersionCheckService
    {
        public string GetCurrentVersion()
        {
            try
            {
                var version = AppInfo.Version.ToString();
                return version;
            }
            catch(Exception Ex)
            {
                CrashDetected.LogCrash(Ex, "GetCurrentVersion");
                return "0.0.0";
            }

        }

        public async Task<string> GetLatestVersion()
        {
            //For Now Return Current Version until Version Table added then return that
            var version = AppInfo.Version.ToString();
            return version;
        }

        public async Task CheckForUpdate()
        {
            var currentVersion = GetCurrentVersion();
            var latestVersion = await GetLatestVersion();

            if (currentVersion != latestVersion)
            {
                // Prompt user to update
                await Application.Current.MainPage.DisplayAlert(
                    "Update Available",
                    "A new version of the app is available. Please update to continue.",
                    "Update");

                // Redirect to App Store
                var appStoreUrl = DeviceInfo.Platform == DevicePlatform.iOS
                    ? "https://apps.apple.com/app/6462983015" // Replace YOUR_APP_ID with your app's ID
                    : "https://play.google.com/store/apps/details?id=com.peoplewith.peoplewithresearch"; // Replace YOUR_PACKAGE_NAME with your app's package name

                await Launcher.Default.OpenAsync(new Uri(appStoreUrl));
            }
        }
    }
}