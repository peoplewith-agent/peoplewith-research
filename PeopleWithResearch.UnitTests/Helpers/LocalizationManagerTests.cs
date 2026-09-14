// TDD: Written before production code. Will compile after sub-write-code completes.

using System.Globalization;
using FluentAssertions;
using PeopleWithResearch.Helpers;
using Xunit;

namespace PeopleWithResearch.UnitTests.Helpers;

/// <summary>
/// Tests for LocalizationManager — the static helper that sets CultureInfo and
/// persists the selected language to Settings.SelectedLanguage.
///
/// AC3: Language selector on main page; UI updates immediately (culture applied on set).
/// AC4: Language persists after app close/reopen (saved to Settings/Preferences).
/// AC5: First launch defaults to device language or English.
/// AC7: 4 supported codes only; a 5th is out of scope for this class.
///
/// NOTE: CultureInfo is available in net9.0 unit tests with no platform context.
///       Settings.SelectedLanguage runs via the in-memory Preferences shim.
///       [Collection("PreferencesTests")] ensures sequential execution with
///       LanguagePersistenceTests to prevent static _store race conditions.
/// </summary>
[Collection("PreferencesTests")]
public class LocalizationManagerTests : IDisposable
{
    // ── Setup / Teardown ──────────────────────────────────────────────────────

    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUICulture;

    public LocalizationManagerTests()
    {
        // Snapshot ambient culture before each test so it can be restored.
        _originalCulture   = CultureInfo.CurrentCulture;
        _originalUICulture = CultureInfo.CurrentUICulture;

        // Clear the in-memory Preferences store so each test starts clean.
        Microsoft.Maui.Storage.Preferences.Clear();
    }

    public void Dispose()
    {
        // Restore ambient culture so parallel tests are not affected.
        CultureInfo.CurrentCulture   = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUICulture;

        // Clean up persisted language.
        Microsoft.Maui.Storage.Preferences.Clear();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // SetLanguage — CurrentCulture mutations
    // AC3: UI culture is applied immediately on SetLanguage call.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SetLanguage_English_SetsCultureToEn()
    {
        // Act
        LocalizationManager.SetLanguage("en");

        // Assert — both CurrentCulture and CurrentUICulture must reflect "en"
        CultureInfo.CurrentCulture.Name.Should().StartWith("en",
            because: "SetLanguage(\"en\") must apply an English CultureInfo");
        CultureInfo.CurrentUICulture.Name.Should().StartWith("en",
            because: "SetLanguage(\"en\") must apply an English UI CultureInfo");
    }

    [Fact]
    public void SetLanguage_Polish_SetsCultureToPl()
    {
        // Act
        LocalizationManager.SetLanguage("pl");

        // Assert
        CultureInfo.CurrentCulture.Name.Should().StartWith("pl",
            because: "SetLanguage(\"pl\") must apply a Polish CultureInfo");
        CultureInfo.CurrentUICulture.Name.Should().StartWith("pl",
            because: "SetLanguage(\"pl\") must apply a Polish UI CultureInfo");
    }

    [Fact]
    public void SetLanguage_Romanian_SetsCultureToRo()
    {
        // Act
        LocalizationManager.SetLanguage("ro");

        // Assert
        CultureInfo.CurrentCulture.Name.Should().StartWith("ro",
            because: "SetLanguage(\"ro\") must apply a Romanian CultureInfo");
        CultureInfo.CurrentUICulture.Name.Should().StartWith("ro",
            because: "SetLanguage(\"ro\") must apply a Romanian UI CultureInfo");
    }

    [Fact]
    public void SetLanguage_Gujarati_SetsCultureToGu()
    {
        // Act
        LocalizationManager.SetLanguage("gu");

        // Assert
        CultureInfo.CurrentCulture.Name.Should().StartWith("gu",
            because: "SetLanguage(\"gu\") must apply a Gujarati CultureInfo");
        CultureInfo.CurrentUICulture.Name.Should().StartWith("gu",
            because: "SetLanguage(\"gu\") must apply a Gujarati UI CultureInfo");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // SetLanguage — persistence to Settings.SelectedLanguage
    // AC4: Language persists after app close/reopen.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SetLanguage_PersistsToPreferences()
    {
        // Act
        LocalizationManager.SetLanguage("pl");

        // Assert — Settings.SelectedLanguage is backed by the in-memory shim
        Settings.SelectedLanguage.Should().Be("pl",
            because: "SetLanguage must call Settings.SelectedLanguage = languageCode");
    }

    [Fact]
    public void SetLanguage_English_PersistsEnToPreferences()
    {
        LocalizationManager.SetLanguage("en");

        Settings.SelectedLanguage.Should().Be("en");
    }

    [Fact]
    public void SetLanguage_Romanian_PersistsRoToPreferences()
    {
        LocalizationManager.SetLanguage("ro");

        Settings.SelectedLanguage.Should().Be("ro");
    }

    [Fact]
    public void SetLanguage_Gujarati_PersistsGuToPreferences()
    {
        LocalizationManager.SetLanguage("gu");

        Settings.SelectedLanguage.Should().Be("gu");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // CurrentLanguage — reflects last SetLanguage call
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CurrentLanguage_AfterSetLanguage_ReturnsSetCode()
    {
        // Arrange & Act
        LocalizationManager.SetLanguage("ro");

        // Assert
        LocalizationManager.CurrentLanguage.Should().Be("ro",
            because: "CurrentLanguage must return the code passed to the last SetLanguage call");
    }

    [Fact]
    public void CurrentLanguage_AfterSetLanguagePolish_ReturnsPl()
    {
        LocalizationManager.SetLanguage("pl");

        LocalizationManager.CurrentLanguage.Should().Be("pl");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GetSupportedLanguages — must include exactly the four PD2-29 languages
    // AC2: Full UI in EN/PL/RO/GU — no missing and no extra codes.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetSupportedLanguages_ReturnsAllFour()
    {
        // Act
        string[] supported = LocalizationManager.GetSupportedLanguages();

        // Assert — all four required codes present
        supported.Should().Contain("en",
            because: "English is a required supported language for PD2-29");
        supported.Should().Contain("pl",
            because: "Polish is a required supported language for PD2-29");
        supported.Should().Contain("ro",
            because: "Romanian is a required supported language for PD2-29");
        supported.Should().Contain("gu",
            because: "Gujarati is a required supported language for PD2-29");
    }

    [Fact]
    public void GetSupportedLanguages_ReturnsExactlyFourCodes()
    {
        string[] supported = LocalizationManager.GetSupportedLanguages();

        supported.Should().HaveCount(4,
            because: "PD2-29 defines exactly four supported languages (en, pl, ro, gu); FR/ES/DE are out of scope");
    }

    [Fact]
    public void GetSupportedLanguages_DoesNotReturnFr()
    {
        // French was the previous language — must be absent in PD2-29.
        string[] supported = LocalizationManager.GetSupportedLanguages();

        supported.Should().NotContain("fr",
            because: "French (fr) was removed in PD2-29 and is out of scope");
    }

    [Fact]
    public void GetSupportedLanguages_DoesNotReturnEs()
    {
        string[] supported = LocalizationManager.GetSupportedLanguages();

        supported.Should().NotContain("es",
            because: "Spanish (es) was removed in PD2-29 and is out of scope");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GetDefaultLanguage — first-launch behaviour
    // AC5: First launch defaults to device language or English.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetDefaultLanguage_WhenNoPreference_ReturnsEnOrSupportedCode()
    {
        // Arrange — no preference set (cleared in constructor)
        string[] supported = LocalizationManager.GetSupportedLanguages();

        // Act
        string defaultLang = LocalizationManager.GetDefaultLanguage();

        // Assert — must be one of the four supported codes
        supported.Should().Contain(defaultLang,
            because: "GetDefaultLanguage must return a supported language code");
    }

    [Fact]
    public void GetDefaultLanguage_ReturnsNonNullNonEmpty()
    {
        string defaultLang = LocalizationManager.GetDefaultLanguage();

        defaultLang.Should().NotBeNullOrEmpty(
            because: "GetDefaultLanguage must always return a usable language code");
    }

    [Fact]
    public void GetDefaultLanguage_FallsBackToEnWhenDeviceLanguageUnsupported()
    {
        // Arrange — inject an unsupported ambient culture to simulate a device
        // whose installed language (e.g. Japanese) is not in the supported list.
        var savedInstalled = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("ja");

        try
        {
            string defaultLang = LocalizationManager.GetDefaultLanguage();

            // Assert — must fall back to "en" when device language is unsupported
            defaultLang.Should().Be("en",
                because: "GetDefaultLanguage must return \"en\" when the device culture is not in the supported list");
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedInstalled;
        }
    }
}
