// TDD: Written before production code. Will compile after sub-write-code completes.

using FluentAssertions;
using PeopleWithResearch.Helpers;
using Xunit;

namespace PeopleWithResearch.UnitTests.Helpers;

/// <summary>
/// Tests for Settings.SelectedLanguage persistence via the in-memory Preferences shim.
///
/// AC4: Language persists after app close/reopen — modelled here as set/get round-trips
///      through the static Settings.SelectedLanguage property, which is backed by
///      Microsoft.Maui.Storage.Preferences (shimmed to an in-memory Dictionary in tests).
///
/// Each test class instance gets a clean Preferences store (cleared in constructor).
///
/// NOTE ON PLATFORM CONTEXT:
///   In production, Preferences.Get/Set route to the native app bundle (Android SharedPrefs
///   / iOS NSUserDefaults). In unit tests the MauiShims.cs stub provides a static
///   Dictionary-backed replacement so no platform context is required. All tests in this
///   file run without skipping.
///
///   [Collection("PreferencesTests")] ensures sequential execution with
///   LocalizationManagerTests to prevent static _store race conditions.
/// </summary>
[Collection("PreferencesTests")]
public class LanguagePersistenceTests : IDisposable
{
    // ── Setup / Teardown ──────────────────────────────────────────────────────

    public LanguagePersistenceTests()
    {
        // Start each test with an empty Preferences store.
        Microsoft.Maui.Storage.Preferences.Clear();
    }

    public void Dispose()
    {
        // Clean up after each test to avoid store pollution across parallel runs.
        Microsoft.Maui.Storage.Preferences.Clear();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Default value — freshly cleared store
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SelectedLanguage_DefaultValue_IsEmptyOrNull()
    {
        // Act — read from a clean store with no value persisted
        string value = Settings.SelectedLanguage;

        // Assert — Settings uses string.Empty as default in Preferences.Get
        value.Should().BeNullOrEmpty(
            because: "SelectedLanguage has no persisted value in a fresh state; default is string.Empty");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Basic round-trip — set then get returns same value
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SelectedLanguage_Set_CanBeRetrieved()
    {
        // Act
        Settings.SelectedLanguage = "pl";
        string retrieved = Settings.SelectedLanguage;

        // Assert
        retrieved.Should().Be("pl",
            because: "SelectedLanguage setter must persist the value so the getter returns it");
    }

    [Fact]
    public void SelectedLanguage_SetEnglish_CanBeRetrieved()
    {
        Settings.SelectedLanguage = "en";

        Settings.SelectedLanguage.Should().Be("en");
    }

    [Fact]
    public void SelectedLanguage_SetRomanian_CanBeRetrieved()
    {
        Settings.SelectedLanguage = "ro";

        Settings.SelectedLanguage.Should().Be("ro");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Overwrite — last written value wins
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SelectedLanguage_SetMultipleTimes_ReturnsLastValue()
    {
        // Act
        Settings.SelectedLanguage = "en";
        Settings.SelectedLanguage = "pl";
        Settings.SelectedLanguage = "ro";

        // Assert — only the last assigned value must survive
        Settings.SelectedLanguage.Should().Be("ro",
            because: "each assignment overwrites the previous value; the most recent set wins");
    }

    [Fact]
    public void SelectedLanguage_OverwriteWithEnglish_ReturnsEn()
    {
        // Arrange
        Settings.SelectedLanguage = "ro";

        // Act — overwrite with English
        Settings.SelectedLanguage = "en";

        // Assert
        Settings.SelectedLanguage.Should().Be("en");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Gujarati round-trip — verifies the gu code stores and retrieves correctly
    // AC6 (partial): Gujarati renders correctly — code is persisted faithfully.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SelectedLanguage_SetGujarati_RoundTrips()
    {
        // Act
        Settings.SelectedLanguage = "gu";
        string retrieved = Settings.SelectedLanguage;

        // Assert
        retrieved.Should().Be("gu",
            because: "the Gujarati language code \"gu\" must survive a set/get round-trip through Preferences");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // All four PD2-29 codes round-trip (table-driven via theory)
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("en")]
    [InlineData("pl")]
    [InlineData("ro")]
    [InlineData("gu")]
    public void SelectedLanguage_AllSupportedCodes_RoundTrip(string code)
    {
        // Act
        Settings.SelectedLanguage = code;

        // Assert
        Settings.SelectedLanguage.Should().Be(code,
            because: $"the language code \"{code}\" must round-trip through Settings.SelectedLanguage");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Isolation — one test's write does not bleed into the next
    // (Verified structurally via IDisposable.Clear, exercised explicitly here)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SelectedLanguage_AfterClear_ReturnsDefault()
    {
        // Arrange
        Settings.SelectedLanguage = "pl";

        // Act — simulate what IDisposable does between tests
        Microsoft.Maui.Storage.Preferences.Clear();

        // Assert — value no longer persisted
        Settings.SelectedLanguage.Should().BeNullOrEmpty(
            because: "clearing Preferences removes the persisted value; subsequent reads return the default");
    }
}
