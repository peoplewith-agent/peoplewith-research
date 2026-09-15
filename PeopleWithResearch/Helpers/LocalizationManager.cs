using System.Globalization;
using PeopleWithResearch.Resources.Strings;

namespace PeopleWithResearch;

public static class LocalizationManager
{
    private static readonly string[] _supportedLanguages = ["en", "pl", "ro", "gu", "es"];

    public static string CurrentLanguage => Helpers.Settings.SelectedLanguage ?? "en";

    public static string[] GetSupportedLanguages() => _supportedLanguages;

    public static void SetLanguage(string languageCode)
    {
        // B4 fix: persist to Preferences FIRST — before culture is applied.
        // If the app is killed between these two operations, next launch will
        // correctly restore the persisted code via GetDefaultLanguage().
        Helpers.Settings.SelectedLanguage = languageCode;
        var culture = new CultureInfo(languageCode);

        // Release the ResourceManager's per-culture cache so that the next
        // GetString call re-probes the satellite assembly for the new culture
        // rather than returning the previously cached neutral (English) ResourceSet.
        AppResources.ResourceManager.ReleaseAllResources();

        // DefaultThreadCurrentCulture/DefaultThreadCurrentUICulture apply to ALL threads
        // (including the UI/main thread). Without these, the culture set on
        // Thread.CurrentThread is invisible to MainThread.BeginInvokeOnMainThread
        // and to any page constructed on the UI thread.
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    public static string GetDefaultLanguage()
    {
        var saved = Helpers.Settings.SelectedLanguage;
        if (!string.IsNullOrEmpty(saved) && _supportedLanguages.Contains(saved))
            return saved;

        var deviceLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return _supportedLanguages.Contains(deviceLang) ? deviceLang : "en";
    }

    public static string Get(string key)
    {
        try
        {
            // Explicitly construct the culture from the persisted preference rather
            // than trusting CultureInfo.CurrentUICulture — this avoids any thread-
            // marshalling race where the UI thread hasn't yet picked up the new
            // DefaultThreadCurrentUICulture value.
            var langCode = Helpers.Settings.SelectedLanguage;
            var culture = string.IsNullOrEmpty(langCode)
                ? System.Globalization.CultureInfo.CurrentUICulture
                : new System.Globalization.CultureInfo(langCode);
            return AppResources.ResourceManager.GetString(key, culture) ?? key;
        }
        catch
        {
            return key;
        }
    }

    /// <summary>
    /// Look up a key in a specific language, regardless of the currently selected language.
    /// Used by the language picker so each option shows its title/description in its own language.
    /// </summary>
    public static string GetForLanguage(string key, string languageCode)
    {
        try
        {
            var culture = new CultureInfo(languageCode);
            return AppResources.ResourceManager.GetString(key, culture) ?? key;
        }
        catch
        {
            return key;
        }
    }
}
