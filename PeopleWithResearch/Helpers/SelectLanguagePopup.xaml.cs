using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Pages;
using Mopups.Services;

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

            // Each language option shows the title/description in its OWN language
            Languages.Add(new LanguageOption { Name = "English",  FlagImage = "egflag.png", LanguageTitle = LocalizationManager.GetForLanguage("SelectLang_Title", "en"),       LanguageDescription = LocalizationManager.GetForLanguage("SelectLang_Description", "en"),       LanguageCode = "en", IsSelected = _currentLanguageCode == "en" });
            Languages.Add(new LanguageOption { Name = "Polski",   FlagImage = "plflag.png", LanguageTitle = LocalizationManager.GetForLanguage("SelectLang_Title", "pl"),       LanguageDescription = LocalizationManager.GetForLanguage("SelectLang_Description", "pl"),       LanguageCode = "pl", IsSelected = _currentLanguageCode == "pl" });
            Languages.Add(new LanguageOption { Name = "Română",   FlagImage = "roflag.png", LanguageTitle = LocalizationManager.GetForLanguage("SelectLang_Title", "ro"),       LanguageDescription = LocalizationManager.GetForLanguage("SelectLang_Description", "ro"),       LanguageCode = "ro", IsSelected = _currentLanguageCode == "ro" });
            Languages.Add(new LanguageOption { Name = "ગુજરાતી",  FlagImage = "guflag.png", LanguageTitle = LocalizationManager.GetForLanguage("SelectLang_Title", "gu"),       LanguageDescription = LocalizationManager.GetForLanguage("SelectLang_Description", "gu"),       LanguageCode = "gu", IsSelected = _currentLanguageCode == "gu" });
            Languages.Add(new LanguageOption { Name = "Español",  FlagImage = "esflag.png", LanguageTitle = LocalizationManager.GetForLanguage("SelectLang_Title", "es"),       LanguageDescription = LocalizationManager.GetForLanguage("SelectLang_Description", "es"),       LanguageCode = "es", IsSelected = _currentLanguageCode == "es" });

            var selected = Languages.FirstOrDefault(l => l.IsSelected) ?? Languages[0];
            LanguageTitleLabel.Text       = selected.LanguageTitle;
            LanguageDescriptionLabel.Text = selected.LanguageDescription;
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }

    private async void OnLanguageTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (e.Parameter is not LanguageOption selected) return;
            foreach (var lang in Languages) lang.IsSelected = lang == selected;

            LocalizationManager.SetLanguage(selected.LanguageCode);
            WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(selected.LanguageCode));

            if (reloadProfile)
                WeakReferenceMessenger.Default.Send(new UpdateProfile("Reload"));

            // Pop the popup FIRST so the close animation finishes before the TCS
            // result triggers App.SetMainPage — otherwise the page replacement tears
            // down the Mopups stack mid-animation and the popup never closes cleanly.
            await MopupService.Instance.PopAsync();

            returnlanguage?.TrySetResult(selected.LanguageCode);
            LanguageSelected?.Invoke(selected.LanguageCode);
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }

    private async void OnCancelTapped(object sender, EventArgs e)
    {
        try
        {
            await MopupService.Instance.PopAsync();
            returnlanguage?.TrySetResult(Helpers.Settings.SelectedLanguage ?? "en");
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }

    // Called by Mopups when the user taps the dim background (CloseWhenBackgroundIsClicked="True")
    protected override bool OnBackgroundClicked()
    {
        returnlanguage?.TrySetResult(Helpers.Settings.SelectedLanguage ?? "en");
        return base.OnBackgroundClicked();
    }
}
