using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Pages;
using Mopups.Services;
using PeopleWithResearch.Resources.Strings;

namespace PeopleWithResearch;

public partial class SelectLanguagePopup : PopupPage
{
    // B3 fix: required ConnectivityChanged event on every page/popup
    public event EventHandler<bool> ConnectivityChanged;

    // B1 fix: crash boilerplate required on every page/popup
    CrashDetected crashHandler;
    public void NotasyncMethod(Exception Ex) { CrashDetected.LogCrash(Ex, "SelectLanguagePopup"); }

    public event Action<string>? LanguageSelected;
    public string _currentLanguageCode;
    private TaskCompletionSource<string>? returnlanguage;
    private bool returnValue = false;
    private bool reloadProfile = false;
    public ObservableCollection<LanguageOption> Languages { get; } = new();

    public SelectLanguagePopup(bool ReloadProfile = false)
    {
        // B1 fix: constructor wrapped in try/catch
        try
        {
            InitializeComponent();
            BindingContext = this;
            this.reloadProfile = ReloadProfile;
            // B5 fix: LoadData is synchronous — call directly on UI thread, no Task.Run
            LoadData();
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }

    public SelectLanguagePopup(TaskCompletionSource<string> taskCompletionSource)
    {
        // B1 fix: constructor wrapped in try/catch
        try
        {
            InitializeComponent();
            BindingContext = this;
            returnValue = true;
            this.returnlanguage = taskCompletionSource;
            // B5 fix: LoadData is synchronous — call directly on UI thread, no Task.Run
            LoadData();
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }

    // B5 fix: changed from async Task to void — no awaited operations, must run on UI thread
    void LoadData()
    {
        try
        {
            _currentLanguageCode = string.IsNullOrEmpty(Helpers.Settings.SelectedLanguage)
                ? "en" : Helpers.Settings.SelectedLanguage;

            // B2 fix: removed spurious using PeoplewithResearch (wrong-case namespace) — not needed here
            Languages.Add(new LanguageOption { Name = "English", FlagImage = "egflag.png", LanguageTitle = AppResources.ResourceManager.GetString("SelectLang_Title", CultureInfo.CurrentUICulture) ?? "Select Language", LanguageDescription = AppResources.ResourceManager.GetString("SelectLang_Description", CultureInfo.CurrentUICulture) ?? "Choose your preferred language", LanguageCode = "en", IsSelected = _currentLanguageCode == "en" });
            Languages.Add(new LanguageOption { Name = "Polski",  FlagImage = "plflag.png", LanguageTitle = AppResources.ResourceManager.GetString("SelectLang_Title", CultureInfo.CurrentUICulture) ?? "Select Language", LanguageDescription = AppResources.ResourceManager.GetString("SelectLang_Description", CultureInfo.CurrentUICulture) ?? "Choose your preferred language", LanguageCode = "pl", IsSelected = _currentLanguageCode == "pl" });
            Languages.Add(new LanguageOption { Name = "Română",  FlagImage = "roflag.png", LanguageTitle = AppResources.ResourceManager.GetString("SelectLang_Title", CultureInfo.CurrentUICulture) ?? "Select Language", LanguageDescription = AppResources.ResourceManager.GetString("SelectLang_Description", CultureInfo.CurrentUICulture) ?? "Choose your preferred language", LanguageCode = "ro", IsSelected = _currentLanguageCode == "ro" });
            Languages.Add(new LanguageOption { Name = "ગુજરાતી", FlagImage = "guflag.png", LanguageTitle = AppResources.ResourceManager.GetString("SelectLang_Title", CultureInfo.CurrentUICulture) ?? "Select Language", LanguageDescription = AppResources.ResourceManager.GetString("SelectLang_Description", CultureInfo.CurrentUICulture) ?? "Choose your preferred language", LanguageCode = "gu", IsSelected = _currentLanguageCode == "gu" });

            LanguageTitleLabel.Text = Languages.FirstOrDefault(l => l.IsSelected)?.LanguageTitle ?? "Select Language";
            LanguageDescriptionLabel.Text = Languages.FirstOrDefault(l => l.IsSelected)?.LanguageDescription ?? "Choose your preferred language";
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }

    private async void OnLanguageTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (e.Parameter is not LanguageOption selected) return;
            foreach (var lang in Languages) lang.IsSelected = lang == selected;

            // B4 fix: persist to Preferences BEFORE applying culture (avoid race if app killed mid-method)
            LocalizationManager.SetLanguage(selected.LanguageCode);
            WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(selected.LanguageCode));

            LanguageTitleLabel.Text = Languages.FirstOrDefault(l => l.IsSelected)?.LanguageTitle ?? "Select Language";
            LanguageDescriptionLabel.Text = Languages.FirstOrDefault(l => l.IsSelected)?.LanguageDescription ?? "Choose your preferred language";
            LanguageSelected?.Invoke(selected.LanguageCode);

            await Task.Delay(150);

            if (returnValue && returnlanguage != null)
            {
                // Return the language CODE (not the display title) for correct TCS semantics
                returnlanguage.SetResult(selected.LanguageCode);
            }
            if (reloadProfile)
            {
                WeakReferenceMessenger.Default.Send(new UpdateProfile("Reload"));
            }
            await MopupService.Instance.PopAsync();
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }
}
