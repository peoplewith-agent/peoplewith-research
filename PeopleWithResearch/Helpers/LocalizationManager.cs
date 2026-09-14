using System.Globalization;
using PeopleWithResearch.Resources.Strings;

namespace PeopleWithResearch;

public static class LocalizationManager
{
    private static readonly string[] _supportedLanguages = ["en", "pl", "ro", "gu"];

    public static string CurrentLanguage => Helpers.Settings.SelectedLanguage ?? "en";

    public static string[] GetSupportedLanguages() => _supportedLanguages;

    public static void SetLanguage(string languageCode)
    {
        // B4 fix: persist to Preferences FIRST — before culture is applied.
        // If the app is killed between these two operations, next launch will
        // correctly restore the persisted code via GetDefaultLanguage().
        Helpers.Settings.SelectedLanguage = languageCode;
        var culture = new CultureInfo(languageCode);
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
            return AppResources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
        }
        catch
        {
            return key;
        }
    }
}
