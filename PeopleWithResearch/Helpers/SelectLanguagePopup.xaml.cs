using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Pages;
using Mopups.Services;
using PeoplewithResearch;

namespace PeopleWithResearch;

public partial class SelectLanguagePopup : PopupPage
{
    public event Action<string>? LanguageSelected;
    public string _currentLanguageCode;
    private TaskCompletionSource<string>? returnlanguage;
    private bool returnValue = false; 
    private bool reloadProfile = false;
    public ObservableCollection<LanguageOption> Languages { get; } = new();
    public SelectLanguagePopup(bool ReloadProfile = false)
    {
        InitializeComponent();
        BindingContext = this;
        this.reloadProfile = ReloadProfile;
        Task.Run(async () => await LoadData());
    }

     public SelectLanguagePopup(TaskCompletionSource<string> taskCompletionSource)
    {
        InitializeComponent();
        BindingContext = this;
        returnValue = true; 
        this.returnlanguage = taskCompletionSource;   
        Task.Run(async () => await LoadData());
    }

    async Task LoadData()
    {
        _currentLanguageCode = string.IsNullOrEmpty(Helpers.Settings.SelectedLanguage)
       ? "en" : Helpers.Settings.SelectedLanguage;

        Languages.Add(new LanguageOption { Name = "English",  FlagImage = "egflag.png", LanguageTitle = "Change language", LanguageDescription="Choose your preferred language - we'll update the whole app to match.", LanguageCode = "en", IsSelected = _currentLanguageCode == "en" });
        Languages.Add(new LanguageOption { Name = "Français", FlagImage = "frflag.png", LanguageTitle = "Changer de langue", LanguageDescription="Choisissez votre langue préférée - nous mettrons à jour toute l'application en conséquence.", LanguageCode = "fr", IsSelected = _currentLanguageCode == "fr" });
        Languages.Add(new LanguageOption { Name = "Español",  FlagImage = "esflag.png", LanguageTitle = "Cambiar idioma", LanguageDescription="Elige tu idioma preferido: actualizaremos toda la aplicación para que coincida.", LanguageCode = "es", IsSelected = _currentLanguageCode == "es" });
        Languages.Add(new LanguageOption { Name = "Deutsch",  FlagImage = "deflag.png", LanguageTitle = "Sprache ändern", LanguageDescription="Wählen Sie Ihre bevorzugte Sprache - wir aktualisieren die gesamte App entsprechend.", LanguageCode = "de", IsSelected = _currentLanguageCode == "de" });
        LanguageTitleLabel.Text = Languages.FirstOrDefault(l => l.IsSelected)?.LanguageTitle ?? "Change language";
        LanguageDescriptionLabel.Text = Languages.FirstOrDefault(l => l.IsSelected)?.LanguageDescription ?? "Choose your preferred language - we'll update the whole app to match.";
        if (!string.IsNullOrEmpty(_currentLanguageCode))
        {
            foreach (var lang in Languages)
                lang.IsSelected = lang.LanguageCode == _currentLanguageCode;
        }
    }

    private async void OnLanguageTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not LanguageOption selected) return;
        foreach (var lang in Languages) lang.IsSelected = lang == selected;
        Helpers.Settings.SelectedLanguage = selected.LanguageCode;
        LanguageTitleLabel.Text = Languages.FirstOrDefault(l => l.IsSelected)?.LanguageTitle ?? "Change language";
        LanguageDescriptionLabel.Text = Languages.FirstOrDefault(l => l.IsSelected)?.LanguageDescription ?? "Choose your preferred language - we'll update the whole app to match.";
        LanguageSelected?.Invoke(selected.LanguageCode);
        await Task.Delay(150);
        if (returnValue && returnlanguage != null)
        {
            returnlanguage.SetResult(selected.LanguageTitle);
        }
        if (reloadProfile)
        {
            WeakReferenceMessenger.Default.Send(new UpdateProfile("Reload"));
        }
        await MopupService.Instance.PopAsync();
    }
}
