// MauiShims.cs â€” Stub implementations of MAUI runtime types for unit-test compilation.
// These types replace Microsoft.Maui.* and related APIs that cannot run in a plain net9.0
// test process.  None of these execute real platform code.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// â”€â”€ Tell linked production code we are InternalsVisibleTo â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PeopleWithResearch.UnitTests")]

// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// Namespace aliases used by both production and test code
// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
namespace Microsoft.Datasync.Client
{
    /// <summary>Stub for DatasyncClientData â€” all model base classes inherit this.</summary>
    public abstract class DatasyncClientData
    {
        public virtual string? Id { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? Version { get; set; }
        public bool Deleted { get; set; }
    }
}

namespace Microsoft.Maui.ApplicationModel.Communication { }

// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// Microsoft.Maui â€” MainThread stub
// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
namespace Microsoft.Maui
{
    /// <summary>Stub â€” runs the action synchronously in unit tests.</summary>
    public static class MainThread
    {
        public static void BeginInvokeOnMainThread(Action action) => action?.Invoke();
        public static Task InvokeOnMainThreadAsync(Action action)
        {
            action?.Invoke();
            return Task.CompletedTask;
        }
        public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func) =>
            Task.FromResult(func!());
    }
}

// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// Microsoft.Maui.Devices â€” DeviceInfo stub
// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
namespace Microsoft.Maui.Devices
{
    public static class DeviceInfo
    {
        public static string Manufacturer => "TestManufacturer";
        public static string Model => "TestModel";
        public static string VersionString => "0.0";
        public static DevicePlatform Platform => DevicePlatform.Unknown;
    }

    public readonly struct DevicePlatform
    {
        private readonly string _platform;
        private DevicePlatform(string p) { _platform = p; }
        public static DevicePlatform Android { get; } = new DevicePlatform("Android");
        public static DevicePlatform iOS { get; } = new DevicePlatform("iOS");
        public static DevicePlatform Unknown { get; } = new DevicePlatform("Unknown");
        public override string ToString() => _platform;
    }
}

// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// Microsoft.Maui.Storage â€” Preferences stub
// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
namespace Microsoft.Maui.Storage
{
    /// <summary>In-memory preferences stub supporting both patterns:
    /// - Preferences.Get/Set (static pattern, used by most of Settings.cs)
    /// - Preferences.Default.Get/Set (instance pattern, used by newer Settings properties)
    /// Uses a DefaultAccessor inner class to avoid CS0111 (same-name static+instance members).
    /// </summary>
    public static class Preferences
    {
        internal static readonly Dictionary<string, object> _store = new();

        /// <summary>Accessor for Preferences.Default.Get/Set calls.</summary>
        public static DefaultAccessor Default { get; } = new DefaultAccessor();

        public sealed class DefaultAccessor
        {
            public T Get<T>(string key, T defaultValue)
            {
                if (_store.TryGetValue(key, out var v) && v is T t) return t;
                return defaultValue;
            }
            public void Set<T>(string key, T value) => _store[key] = value!;
            public bool ContainsKey(string key) => _store.ContainsKey(key);
            public void Remove(string key) => _store.Remove(key);
            public void Clear() => _store.Clear();
        }

        // ── Static methods (Preferences.Get / Preferences.Set) ──────────────
        public static T Get<T>(string key, T defaultValue) => Default.Get(key, defaultValue);
        public static void Set<T>(string key, T value) => Default.Set(key, value);
        public static bool ContainsKey(string key, string? sharedName = null) => Default.ContainsKey(key);
        public static void Remove(string key, string? sharedName = null) => Default.Remove(key);
        public static void Clear(string? sharedName = null) => Default.Clear();
    }
    // Alias removed — PreferencesStaticShim no longer needed
}

// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// Sentry stubs â€” CrashDetected uses SentrySdk
// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
namespace Sentry
{
    public static class SentrySdk
    {
        public static void AddBreadcrumb(string message, string? category = null,
            string? type = null, IDictionary<string, string>? data = null,
            BreadcrumbLevel level = BreadcrumbLevel.Info) { }

        public static void CaptureException(Exception ex,
            Action<Scope>? scopeCallback = null) { }

        public static Task FlushAsync(TimeSpan timeout) => Task.CompletedTask;
        public static void Flush(TimeSpan timeout) { }
    }

    public class Scope
    {
        public SentryUser? User { get; set; }
        public void SetTag(string key, string value) { }
    }

    public class SentryUser
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
    }

    public enum BreadcrumbLevel { Debug, Info, Warning, Error, Critical }
}

// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// CommunityToolkit.Maui stub (ImperialViewModel.cs has `using CommunityToolkit.Maui;`)
// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
namespace CommunityToolkit.Maui { }

// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// PeopleWithResearch stubs â€” APICalls, App, ImperialDashboard, Constants,
// QuestionManager, UserNotifications, Helpers.Settings
// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
namespace PeopleWithResearch
{
    // â”€â”€ APICalls stub â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public class APICalls
    {
        public const string ApplicationURL = "https://test.local/api/";
        private static readonly APICalls _instance = new();
        public static APICalls Instance => _instance;

        private readonly HttpClient _client = new();
        public HttpClient GetClient() => _client;

        public Task<ObservableCollection<householdgroup>?> GetUserHouseholdInfo(string? groupId)
            => Task.FromResult<ObservableCollection<householdgroup>?>(null);
    }

    // â”€â”€ Constants stub â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public static class Constants
    {
        public const string ClientPrincipal = "test-principal";
        public const string ApiRole = "test-role";
        public const string ApplicationURL = "https://test.local/api/";
    }

    // â”€â”€ App stub â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public class App
    {
        public static Task SetMainPage(object page) => Task.CompletedTask;
    }

    // â”€â”€ ImperialDashboard stub â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public class ImperialDashboard { }

    // NOTE: Helpers.Settings stub removed — real Settings.cs is now linked directly
    // into the test project for PD2-29. The namespace PeopleWithResearch.Helpers
    // and its Settings class are provided by the linked production file.

    public class QuestionManager { }

    public class UserNotifications { }
}

// ── Microsoft.Maui.Controls stubs ──────────────────────────────────────────
namespace Microsoft.Maui.Controls
{
    // INavigation stub – used by CrashDetected.LogCrash and page code-behind
    public interface INavigation
    {
        System.Threading.Tasks.Task PushAsync(Page page);
        System.Threading.Tasks.Task PushAsync(Page page, bool animated);
        System.Threading.Tasks.Task<Page?> PopAsync();
        System.Threading.Tasks.Task<Page?> PopAsync(bool animated);
        System.Threading.Tasks.Task PopToRootAsync();
        System.Collections.Generic.IReadOnlyList<Page> NavigationStack { get; }
        System.Collections.Generic.IReadOnlyList<Page> ModalStack { get; }
    }

    // ImageSource stub – used by user.cs model
    public abstract class ImageSource { }

    // Page stub – required for INavigation
    public class Page { }

    // Brush stub – used by householdgroupjsondetails.cs model
    public abstract class Brush { }
    public class SolidColorBrush : Brush
    {
        public SolidColorBrush() { }
        public SolidColorBrush(Color color) { }
        public Color Color { get; set; }
    }

    // Color stub
    public struct Color
    {
        public static Color FromArgb(string hex) => default;
        public static Color FromRgba(int r, int g, int b, int a) => default;
        public static readonly Color Default = default;
    }

    // Command stub for ViewModel compilation
    public class Command : System.Windows.Input.ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;
        public Command(Action execute, Func<bool>? canExecute = null) { _execute = execute; _canExecute = canExecute; }
        public Command(Func<Task> execute, Func<bool>? canExecute = null) { _execute = () => execute(); _canExecute = canExecute; }
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? p) => _canExecute?.Invoke() ?? true;
        public void Execute(object? p) => _execute();
        public void ChangeCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}