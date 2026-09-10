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
                var version = (DeviceInfo.Current.Platform == DevicePlatform.iOS) ?  
                    AppInfo.BuildString.ToString() : AppInfo.VersionString.ToString();
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
            var AppStoreString = await APICalls.Instance.GetCurrentAppVersion();
            var version = AppStoreString;
            return version;
        }

        public async Task <bool> CheckForUpdate()
        {
            var currentVersion = GetCurrentVersion();
            var latestVersion = await GetLatestVersion();

            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(latestVersion)) return false;


            if (Version.TryParse(currentVersion, out Version appVersion) && Version.TryParse(latestVersion, out Version storeVersion))
            {
                if (appVersion < storeVersion)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}