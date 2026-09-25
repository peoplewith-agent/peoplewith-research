using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Maui.Devices;
using Newtonsoft.Json;
using PeopleWithResearch;
using Plugin.LocalNotification.AndroidOption;
using Syncfusion.Maui.Core.Carousel;

namespace PeopleWithResearch;

/// <summary>
/// ViewModel for NewImperial.xaml — the registration + baseline-questionnaire wizard.
///
/// COMPLETE port of Imperial.xaml.cs. Every section is here: Name/Account, Address, Gender,
/// Ethnicity, Body Metrics, mainuserstack (add up to 2 household members — see
/// HouseholdMemberEntryViewModel.cs), Education/Work, Household Structure, NHS Number,
/// Respiratory Illness, Health Conditions, Medications, Recent Vaccinations, Diet, Menstrual,
/// Health Status, Additional Questions, Antiviral, Tobacco, Alcohol, Drug, Sleep, Terms &amp;
/// Consent (incl. signature capture), the Finish screen, and SubmitAsync (was CreateAccount —
/// final assembly and submission). The navigation engine replaces the original's
/// reflection-based SetStackVisibility() and the 30-branch if/else in nextbtn_Clicked() with
/// a small handler registry keyed by RegField.XamlNameArea (RegisterSectionHandlers() below).
///
/// The questionnaire-type sub-flow (topprogress2, Type=="Questionnaire" sections driven by
/// ConvertQuestionGroupsToRegFields) is confirmed dead code in the original, not just
/// unencountered here: ConvertQuestionGroupsToRegFields is only ever called from a
/// commented-out line, so no field can ever actually carry Type=="Questionnaire" in practice,
/// and its target XamlNameArea ("questionnairestack") doesn't appear anywhere in the original
/// XAML. Not ported, on the same basis as this file's other confirmed-dead controls.
///
/// Requires the CommunityToolkit.Mvvm NuGet package (8.x). Field-based [ObservableProperty]
/// syntax is used throughout for compatibility with all 8.x versions.
///
/// A number of spots below diverge slightly from the original's literal behavior — each is
/// flagged with a NOTE comment explaining why, rather than silently copied or silently fixed.
/// </summary>
public partial class NewImperialViewModel : ObservableObject
{
    private readonly IAlertService _alertService;

    public NewImperialViewModel(IAlertService alertService)
    {
        _alertService = alertService;
        RegisterSectionHandlers();
        SelectedFluOptions.CollectionChanged += OnFluSelectionCollectionChanged;
    }

    #region Construction context — mirrors the four Imperial(...) constructor overloads

    public newuser NewUser { get; private set; } = new();
    public user? UserDetails { get; private set; }
    public signupcode? SignupCodeDetails { get; private set; }
    public Questionnaire? QuestionnaireDetails { get; private set; }

    private bool _householdRepFromReg;
    /// <summary>Was: householdrepFROMREG, exposed read-only for code-behind's
    /// SubmissionCompleted handler (was: new PopupPageHelper(true, householdrepFROMREG)).</summary>
    public bool IsHouseholdRepFromReg => _householdRepFromReg;
    private bool _noEmailUserReg;
    public bool HHRepFromDash { get; set; } = false;
    private RegField? _mainUserSectionSaved;
    private householdgroupjsondetails? _userInfoForBaseline;
    private ObservableCollection<householdgroupjsondetails> _allGroupDetailsPassed = new();
    private householdgroup? _allHouseholdGroup;

    /// <summary>Was: Imperial(user, advert, Questionnaire).</summary>
    public void ConfigureForHouseholdRegistration(user userPassed, advert signupDetailsPassed, Questionnaire questionnairePassed)
    {
        UserDetails = userPassed;
        _householdRepFromReg = true;
        // Original left signupcodedetails/questionnairedetails unassigned here too (commented
        // out) — preserved as-is rather than guessing what they should be.
    }

    /// <summary>Was: Imperial(user, signupcode).</summary>
    public async Task ConfigureForSignupCode(user userPassed, signupcode signupDetailsPassed)
    {
        UserDetails = userPassed;
        SignupCodeDetails = signupDetailsPassed;

        if (!string.IsNullOrEmpty(UserDetails.Email))
        {
            _householdRepFromReg = false;
            await gethouseholdata();

        }
        else
        {
            _householdRepFromReg = true;
        }
    }

    /// <summary>Was: Imperial(signupcode, householdgroupjsondetails, ObservableCollection&lt;householdgroupjsondetails&gt;, householdgroup).</summary>
    public void ConfigureForHouseholdMember(signupcode signupDetailsPassed, householdgroupjsondetails userInfoPassed,
        ObservableCollection<householdgroupjsondetails> allGroupDetails, householdgroup passedHousehold)
    {
        UserDetails = new user();
        SignupCodeDetails = signupDetailsPassed;
        _userInfoForBaseline = userInfoPassed;
        _allGroupDetailsPassed = allGroupDetails;
        _allHouseholdGroup = passedHousehold;
        _householdRepFromReg = false;

        BannerText = "You are completing this on behalf of " + userInfoPassed.household_individual_name;
        IsBannerVisible = true;

        Email = _userInfoForBaseline.household_individual_email;

        var splitName = userInfoPassed.household_individual_name.Split(' ');
        FirstName = splitName[0];
        Surname = splitName.Length > 1 ? splitName[1] : string.Empty;

        // TODO (next stage): the original also routes this name into under10entry or
        // over16nameentry here depending on household_individual_age. Those controls belong
        // to sections not yet ported (under10stack / over-16 handling) — re-add alongside them.

        if (_userInfoForBaseline.household_individual_email.Contains("N/A"))
        {
            _noEmailUserReg = true;
            IsEmailFieldVisible = false;
            IsPasswordFieldVisible = false;
            IsConfirmPasswordFieldVisible = false;
            IsTelephoneFieldVisible = false;
        }

        IsEmailEditable = false;
        IsFirstNameEditable = false;
        IsSurnameEditable = false;
    }

    async Task gethouseholdata()
    {
        try
        {
            var householdGroupList = await APICalls.Instance.GetUserHouseholdInfo(UserDetails.Householdgroupid);
            var HouseHoldGroup = householdGroupList?.FirstOrDefault();
            if (HouseHoldGroup == null) return;

            var Allhouseholdgroupinfodetails = HouseHoldGroup.userdetailslist ?? new ObservableCollection<householdgroupjsondetails>();

            var matchingUser = Allhouseholdgroupinfodetails
                .FirstOrDefault(x => x.household_individual_userid == UserDetails.Userid);

            if (matchingUser == null) return;

            _userInfoForBaseline = matchingUser;
            //Set Name for User 
            var nameParts = matchingUser?.household_individual_name?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            FirstName = nameParts?.FirstOrDefault() ?? string.Empty;
            Surname = nameParts?.Length > 1  ? string.Join(" ", nameParts.Skip(1)) : string.Empty;
            IsFirstNameEditable = string.IsNullOrEmpty(FirstName) ? true : false;
            IsSurnameEditable = string.IsNullOrEmpty(Surname) ? true : false;
            var SetEmail = matchingUser?.household_individual_email;
            Email = string.IsNullOrEmpty(SetEmail) ? string.Empty : SetEmail;
            IsEmailEditable = string.IsNullOrEmpty(Email) ? true : false; 
            Over16Name = FirstName + " " + Surname;
            IsOver16NameEditable = string.IsNullOrEmpty(Over16Name) ? true : false;

        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "gethouseholdata");
        }
    }

    #endregion

    #region Navigation engine

    // Replaces GetType().GetField(stackName, ...) reflection plus the 30-branch if/else in
    // nextbtn_Clicked(), with a small registry keyed by RegField.XamlNameArea — the same keys
    // the server config already uses, so nothing about the data-driven step model changes.
    private sealed record SectionHandler(Func<Task<bool>> ValidateAsync, Func<Task> OnAdvanceAsync);
    private readonly Dictionary<string, SectionHandler> _sectionHandlers = new();

    public ObservableCollection<RegField> RegistrationSections { get; private set; } = new();
    public ObservableCollection<RegField> RegistrationSectionsNotRequired { get; private set; } = new();
    private List<RegField> _over16MainSections = new();
    public List<QuestionnaireResult> QuestionnaireResults { get; } = new();

    // mainuserstack's two "add a household member" entries — see HouseholdMemberEntryViewModel.
    public HouseholdMemberEntryViewModel Member1 { get; } = new();
    public HouseholdMemberEntryViewModel Member2 { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressValue))]
    private int _currentFieldIndex;

    // One computed bool per section, all recomputed together whenever CurrentSectionKey
    // changes. Compile-time safe (typo a key here and you get a warning, not a silent
    // no-op like the reflection version had).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNameStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsAddressStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsGenderStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsEthnicityStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsBodyMetricsStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsMainUserStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsEducationWorkStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsHouseholdStructureStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsNhsNumStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsRiStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsHcStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsAddHcStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsMedynStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsMedicationsStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsRvStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsDietStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsMenstrualStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsHtStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsAdditionalQStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsAntiviralStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsTobaccoStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsAlcoholStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsDrugStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsSleepStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsTermsStackVisible))]
    [NotifyPropertyChangedFor(nameof(IsFinishStackVisible))]
    private string _currentSectionKey = string.Empty;

    public bool IsNameStackVisible => CurrentSectionKey == "namestack";
    public bool IsAddressStackVisible => CurrentSectionKey == "addressstack";
    public bool IsGenderStackVisible => CurrentSectionKey == "genderstack";
    public bool IsEthnicityStackVisible => CurrentSectionKey == "ethnicitystack";
    public bool IsBodyMetricsStackVisible => CurrentSectionKey == "bodymetricsstack";
    public bool IsMainUserStackVisible => CurrentSectionKey == "mainuserstack";
    public bool IsEducationWorkStackVisible => CurrentSectionKey == "educationworkstack";
    public bool IsHouseholdStructureStackVisible => CurrentSectionKey == "householdstructurestack";
    public bool IsNhsNumStackVisible => CurrentSectionKey == "nhsnumstack";
    public bool IsRiStackVisible => CurrentSectionKey == "ristack";
    public bool IsHcStackVisible => CurrentSectionKey == "hcstack";
    public bool IsAddHcStackVisible => CurrentSectionKey == "addhcstack";
    public bool IsMedynStackVisible => CurrentSectionKey == "medynstack";
    public bool IsMedicationsStackVisible => CurrentSectionKey == "medicationsstack";
    public bool IsRvStackVisible => CurrentSectionKey == "rvstack";
    public bool IsDietStackVisible => CurrentSectionKey == "dietstack";
    public bool IsMenstrualStackVisible => CurrentSectionKey == "menstrualstack";
    public bool IsHtStackVisible => CurrentSectionKey == "htstack";
    public bool IsAdditionalQStackVisible => CurrentSectionKey == "additionalqstack";
    public bool IsAntiviralStackVisible => CurrentSectionKey == "antiviralstack";
    public bool IsTobaccoStackVisible => CurrentSectionKey == "tobaccostack";
    public bool IsAlcoholStackVisible => CurrentSectionKey == "alcoholstack";
    public bool IsDrugStackVisible => CurrentSectionKey == "drugstack";
    public bool IsSleepStackVisible => CurrentSectionKey == "sleepstack";
    public bool IsTermsStackVisible => CurrentSectionKey == "termsstack";
    public bool IsFinishStackVisible => CurrentSectionKey == "finishstack";

    private bool CanExecuteNext() => !IsBusy && IsNextEnabled;
    private bool CanExecuteBack() => !IsBusy;

    // Add one more computed bool + [NotifyPropertyChangedFor] entry above per section as it's
    // ported (ethnicitystack, bodymetricsstack, ...) — mirrors adding a branch to the old
    // SetStackVisibility()/ShowCurrentStack(), just compile-checked instead of string-matched.

    [ObservableProperty] private bool _isWelcomeVisible = true;
    [ObservableProperty] private bool _isRegisterVisible;
    [ObservableProperty] private bool _isBackButtonVisible = true;
    [ObservableProperty] private bool _isProgressBarVisible;
    [ObservableProperty] private string _welcomeTitle = string.Empty;
    [ObservableProperty] private string _studyTitle = string.Empty;
    [ObservableProperty] private FormattedString _welcomeSubtitle = new FormattedString();
    [ObservableProperty] private string _bannerText = string.Empty;
    [ObservableProperty] private bool _isBannerVisible;
    [ObservableProperty] private string _nextButtonText = "Get Started";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNextEnabled))]
    private bool _isBusy;

    public bool IsNextEnabled => !IsBusy;
    [ObservableProperty] private string _sectionTitle = string.Empty;      // was: toplbl.Text
    [ObservableProperty] private string _sectionSubtitle = string.Empty;   // was: sublbl.Text
    [ObservableProperty] private string _sectionHelpText = string.Empty;   // was: infomainlbl.Text
    [ObservableProperty] private bool _isSectionHelpTextVisible;

    public double ProgressMaximum => Math.Max(RegistrationSections.Count - 1, 0);
    public double ProgressSegmentCount => Math.Max(RegistrationSections.Count - 1, 0);
    public double ProgressValue => CurrentFieldIndex;

    /// <summary>Code-behind hooks this up to mainscrollview.ScrollToAsync(0, 0, true).</summary>
    public event Func<Task>? ScrollResetRequested;

    /// <summary>Code-behind hooks this up to Navigation.RemovePage(this).</summary>
    public event Action? RequestClosePage;

    /// <summary>Code-behind hooks this up to MopupService.Instance.PushAsync(new Infopopup(...)).
    /// Was: TapGestureRecognizer_Tapped, shared by every "tap for more info" help label across
    /// every section (originally read the tapped Label's own Text directly in code-behind;
    /// here the label's bound text is passed as the command parameter instead).</summary>
    public event Action<string>? InfoRequested;

    [RelayCommand]
    private void ShowInfo(string text)
    {
        if (!string.IsNullOrEmpty(text)) InfoRequested?.Invoke(text);
    }

    private void RegisterSectionHandlers()
    {
        _sectionHandlers["namestack"] = new SectionHandler(ValidateNameSectionAsync, AddNameSectionInfoAsync);
        _sectionHandlers["addressstack"] = new SectionHandler(ValidateAddressSectionAsync, AddAddressSectionInfoAsync);
        _sectionHandlers["genderstack"] = new SectionHandler(() => Task.FromResult(ValidateGenderSection()), AddGenderSectionInfoAsync);
        _sectionHandlers["ethnicitystack"] = new SectionHandler(() => Task.FromResult(ValidateEthnicitySection()), AddEthnicitySectionInfoAsync);
        _sectionHandlers["bodymetricsstack"] = new SectionHandler(() => Task.FromResult(ValidateBodyMetricsSection()), AddBodyMetricsSectionInfoAsync);
        _sectionHandlers["mainuserstack"] = new SectionHandler(() => Task.FromResult(ValidateMainUserSection()), AddMainUserSectionInfoAsync);
        _sectionHandlers["educationworkstack"] = new SectionHandler(() => Task.FromResult(ValidateEducationWorkSection()), AddEducationWorkSectionInfoAsync);
        _sectionHandlers["householdstructurestack"] = new SectionHandler(() => Task.FromResult(ValidateHouseholdStructureSection()), AddHouseholdStructureSectionInfoAsync);
        _sectionHandlers["nhsnumstack"] = new SectionHandler(ValidateNhsNumSectionAsync, AddNhsNumSectionInfoAsync);
        _sectionHandlers["ristack"] = new SectionHandler(() => Task.FromResult(ValidateRiSection()), AddRiSectionInfoAsync);
        _sectionHandlers["hcstack"] = new SectionHandler(() => Task.FromResult(ValidateHcSection()), AddHcSectionInfoAsync);
        _sectionHandlers["addhcstack"] = new SectionHandler(() => Task.FromResult(ValidateAddHcSection()), AddAddHcSectionInfoAsync);
        _sectionHandlers["medynstack"] = new SectionHandler(() => Task.FromResult(ValidateMedynSection()), AddMedynSectionInfoAsync);
        _sectionHandlers["medicationsstack"] = new SectionHandler(() => Task.FromResult(ValidateMedicationsSection()), AddMedicationsSectionInfoAsync);
        _sectionHandlers["rvstack"] = new SectionHandler(() => Task.FromResult(ValidateRvSection()), AddRvSectionInfoAsync);
        _sectionHandlers["dietstack"] = new SectionHandler(() => Task.FromResult(ValidateDietSection()), AddDietSectionInfoAsync);
        _sectionHandlers["menstrualstack"] = new SectionHandler(() => Task.FromResult(ValidateMenstrualSection()), AddMenstrualSectionInfoAsync);
        _sectionHandlers["htstack"] = new SectionHandler(() => Task.FromResult(ValidateHtSection()), AddHtSectionInfoAsync);
        _sectionHandlers["additionalqstack"] = new SectionHandler(() => Task.FromResult(ValidateAdditionalQSection()), AddAdditionalQSectionInfoAsync);
        _sectionHandlers["antiviralstack"] = new SectionHandler(() => Task.FromResult(ValidateAntiviralSection()), AddAntiviralSectionInfoAsync);
        _sectionHandlers["tobaccostack"] = new SectionHandler(() => Task.FromResult(ValidateTobaccoSection()), AddTobaccoSectionInfoAsync);
        _sectionHandlers["alcoholstack"] = new SectionHandler(() => Task.FromResult(ValidateAlcoholSection()), AddAlcoholSectionInfoAsync);
        _sectionHandlers["drugstack"] = new SectionHandler(() => Task.FromResult(ValidateDrugSection()), AddDrugSectionInfoAsync);
        _sectionHandlers["sleepstack"] = new SectionHandler(() => Task.FromResult(ValidateSleepSection()), AddSleepSectionInfoAsync);
        _sectionHandlers["termsstack"] = new SectionHandler(() => Task.FromResult(ValidateTermsSection()), AddTermsSectionInfoAsync);
        // finishstack gets no handler — matches the original, which never runs it through
        // ValidateXStack/AddXInfo at all. It's the terminal "you're done" screen; reaching it
        // just flips NextButtonText to "Finish" (see NextAsync), same as the original's
        // ShowCurrentStack setting nextbtn.Text = "Finish" for this XamlNameArea.
    }

    public async Task LoadRegistrationConfigAsync()
    {
        try
        {
            if (SignupCodeDetails is null || string.IsNullOrEmpty(SignupCodeDetails.appdetails))
                return;

            var config = JsonConvert.DeserializeObject<AppConfig>(SignupCodeDetails.appdetails);
            if (config is null) return;

            var sortedList = config.RegFields
                .Where(x => x.Active)
                .OrderBy(x => int.TryParse(x.Order, out var o) ? o : int.MaxValue)
                .ToList();

            var allFields = new ObservableCollection<RegField>(sortedList);

            foreach (var item in allFields)
            {
                foreach (var sub in item.subFields ?? new List<RegField>())
                {
                    if (sub.Required) sub.Label += " *";
                }
            }

            if (!_householdRepFromReg)
            {
                _mainUserSectionSaved = allFields.FirstOrDefault(x => x.XamlNameArea == "mainuserstack");
                allFields.RemoveAll(x => x.XamlNameArea == "mainuserstack");
                allFields.RemoveAll(x => x.XamlNameArea == "addressstack");
                allFields.RemoveAll(x => x.XamlNameArea == "householdstructurestack");
            }

            RegistrationSectionsNotRequired = new ObservableCollection<RegField>(allFields.Where(x => !x.Required));
            RegistrationSections = new ObservableCollection<RegField>(allFields.Where(x => x.Required));


            //Add Additional to List 
            _over16MainSections = RegistrationSections
            .Where(x => string.Equals(x.Type, "over16main", StringComparison.OrdinalIgnoreCase))
            .ToList();

            //Remove from RegistrationSections
            RegistrationSections.RemoveAll(x => string.Equals(x.Type, "over16main", StringComparison.OrdinalIgnoreCase));
            // NOTE: the original stores this same filtered list in Over16regfields, but the
            // only place that later re-adds items (AddGenderInfo) reads Over16regfieldsmain
            // (Type == "over16main") instead — so this "over16" removal looks effectively
            // permanent in the original too. Carried over exactly as it behaves today.

            if (UserDetails is not null && !UserDetails.Primaryuser)
            {
                var removeMainUser = RegistrationSections.FirstOrDefault(x => x.XamlNameArea == "mainuserstack");
                if (removeMainUser is not null) RegistrationSections.Remove(removeMainUser);

                var removeAddress = RegistrationSections.FirstOrDefault(x => x.XamlNameArea == "addressstack");
                if (removeAddress is not null) RegistrationSections.Remove(removeAddress);
            }

            WelcomeTitle = "Welcome to the " + config.OverallSettings.StudyName;
            StudyTitle = config.OverallSettings.StudyTitle;


            WelcomeSubtitle = ParseBold(
    config.OverallSettings.StudyDescription,
    "OpenSansRegular",
    "OpenSansSemibold",   // matches the alias in ConfigureFonts
    14,
     Color.FromArgb("#777777"),   // regular text
    Color.FromArgb("#333333")); // bold text — darker, stands out more

            IsWelcomeVisible = true;
            NextButtonText = "Get Started";

            // These two lists were set once, hard-coded, in the original's
            // LoadRegistrationConfigAsync — not server-config-driven like everything else,
            // so they're not read from `config` and don't need re-setting per section display.
            var phoneOptions = new ObservableCollection<string> { "Yes, they own their own phone", "I will complete the forms for them, or on their behalf" };
            Member1.PhoneOptions = phoneOptions;
            Member2.PhoneOptions = new ObservableCollection<string>(phoneOptions);

            var ageOptions = new ObservableCollection<string> { "0 - 5","5 - 10", "11 - 15", "16+" };
            Member1.AgeOptions = ageOptions;
            Member2.AgeOptions = new ObservableCollection<string>(ageOptions);

            // Was: stringlistyn — hcfirstlist and medsfirstlist share this same Yes/No list.
            HealthConditionsGateOptions = new ObservableCollection<string> { "Yes", "No" };
            MedicationsGateOptions = new ObservableCollection<string> { "Yes", "No" };

            RefreshProgress();
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "LoadRegistrationConfigAsync");
        }
    }

    private static FormattedString ParseBold(string input, string fontFamily, string boldFontFamily, double fontSize, Color textColor, Color boldTextColor)
    {
        var result = new FormattedString();
        if (string.IsNullOrEmpty(input)) return result;

        var parts = Regex.Split(input, @"(\*\*.*?\*\*)");
        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part)) continue;

            bool isBold = part.StartsWith("**") && part.EndsWith("**");
            result.Spans.Add(new Span
            {
                Text = isBold ? part.Trim('*') : part,
                FontAttributes = isBold ? FontAttributes.Bold : FontAttributes.None,
                FontFamily = isBold ? boldFontFamily : fontFamily,
                FontSize = fontSize,
                TextColor = isBold ? boldTextColor : textColor
            });
        }
        return result;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteNext))]
    private async Task NextAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            //IsBackButtonVisible = true;
            if (NextButtonText == "Finish")
            {
                IsBackButtonVisible = false;
                await SubmitAsync();         
                return;
            }
            if (NextButtonText == "Get Started")
            {
                await StartRegistrationAsync();
                //IsBackButtonVisible = true;
                return;
            }
            if (CurrentFieldIndex >= RegistrationSections.Count)
            {
                NextButtonText = "Finish";
                //IsBackButtonVisible = false;
                return;
            }
            var currentField = RegistrationSections[CurrentFieldIndex];
            bool canProceed = true;
            if (_sectionHandlers.TryGetValue(currentField.XamlNameArea, out var handler))
            {
                canProceed = await handler.ValidateAsync();
                if (canProceed) await handler.OnAdvanceAsync();
            }
            if (!canProceed)
            {
                Vibration.Vibrate();
                return;
            }
            CurrentFieldIndex++;
            if (CurrentFieldIndex < RegistrationSections.Count)
            {
                CurrentSectionKey = RegistrationSections[CurrentFieldIndex].XamlNameArea;
                ApplyCurrentSectionText();
                ClearAllSectionErrors();
                //IsBackButtonVisible = true;
            }
            else
            {
                NextButtonText = "Finish";
                //IsBackButtonVisible = false;
            }
            RefreshProgress();
            if (ScrollResetRequested is not null)
                await ScrollResetRequested.Invoke();
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "NextAsync");
        }
        finally
        {
            IsBusy = false;
        }
    }
    // Ensure BackAsync guards against concurrent execution
    [RelayCommand(CanExecute = nameof(CanExecuteBack))]
    private async Task BackAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            if (CurrentFieldIndex <= 0)
            {
                if (NextButtonText == "Get Started")
                {
                    RequestClosePage?.Invoke();
                    return;
                }
                IsWelcomeVisible = true;
                IsRegisterVisible = false;
                IsProgressBarVisible = false;
                //IsBackButtonVisible = false;
                NextButtonText = "Get Started";
                RefreshProgress();
                return;
            }
            CurrentFieldIndex--;
            CurrentSectionKey = RegistrationSections[CurrentFieldIndex].XamlNameArea;
            ApplyCurrentSectionText();
            NextButtonText = "Next";
            //IsBackButtonVisible = true;
            IsProgressBarVisible = true;
            RefreshProgress();
            if (ScrollResetRequested is not null)
                await ScrollResetRequested.Invoke();
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "BackAsync");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StartRegistrationAsync()
    {
        IsWelcomeVisible = false;
        IsRegisterVisible = true;
        IsProgressBarVisible = true;
        NextButtonText = "Next";
        CurrentFieldIndex = 0;

        if (RegistrationSections.Count > 0)
        {
            CurrentSectionKey = RegistrationSections[0].XamlNameArea;
            ApplyCurrentSectionText();
        }

        RefreshProgress();

        if (ScrollResetRequested is not null)
            await ScrollResetRequested.Invoke();

        await Task.Delay(1000); // matches the original's pause before re-enabling Next
    }

    /// <summary>Was: CreateAccount(). Code-behind hooks this up to pushing the completion
    /// popup and navigating to the dashboard, since both are page/navigation concerns.</summary>
    public event Action? SubmissionCompleted;

    private async Task SubmitAsync()
    {
        try
        {
            if (!_householdRepFromReg)
            {
                // Completing this on behalf of another household member.
                NewUser.userid = _userInfoForBaseline?.household_individual_userid;
                NewUser.primaryuser = false;
                NewUser.signupcodegrouping = SignupCodeDetails?.signupcodegrouping;
                NewUser.householdgroupid = _userInfoForBaseline?.household_group_id;
                NewUser.signupcodeid = SignupCodeDetails?.signupcodeid;
                // Was: Helpers.Settings.Postcode — a locally-stored postcode from this device's
                // own prior registration, not something this ViewModel has a source for on its
                // own; carried over as a direct call, same as every other Helpers.Settings/
                // Preferences.Default reference in this method.
                NewUser.postcode = Helpers.Settings.Postcode;
            }
            else
            {
                NewUser.userid = UserDetails?.Userid;
                NewUser.primaryuser = UserDetails?.Primaryuser ?? false;
                NewUser.signupcodegrouping = UserDetails?.Signupcodegrouping;
                NewUser.householdgroupid = UserDetails?.Householdgroupid;
                NewUser.signupcodeid = UserDetails?.Signupid;
            }

            NewUser.status = "active";

            await PatchUserAsync();

            if (_householdRepFromReg && NewUser.primaryuser)
            {
                await AddHouseholdMembersAsync();
            }
            else
            {
                await UpdateHouseholdMembersAsync();
            }

            await SubmitQuestionnaireAsync();
            string signatureFileName = await UploadSignatureAsync();
            await SubmitConsentAsync(signatureFileName);

            //Only save if not Householdrep
            if (!HHRepFromDash)
            {
                SaveLocalUserPreferences();
            }

            SubmissionCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "SubmitAsync");
            IsBusy = false;
        }
    }

    private async Task PatchUserAsync()
    {
        var updateData = new
        {
            firstname = NewUser.firstname,
            surname = NewUser.surname,
            gender = NewUser.gender,
            status = NewUser.status,
            ethnicity = NewUser.ethnicity,
            email = NewUser.email,
            password = NewUser.password,
            postcode = NewUser.postcode,
            signupcodeid = NewUser.signupcodeid,
            signupcodegrouping = NewUser.signupcodegrouping,
            primarycareid = NewUser.primarycareid,
            primaryuser = NewUser.primaryuser,
            householdgroupid = NewUser.householdgroupid,
            dateofbirth = NewUser.dateofbirth,
            details = NewUser.details,
            telephone = NewUser.telephone
        };

        string json = System.Text.Json.JsonSerializer.Serialize(updateData);
        string url = $"{APICalls.ApplicationURL}user/userid/{NewUser.userid}";
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await APICalls.Instance.GetClient().PatchAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            _ = await response.Content.ReadAsStringAsync();
        }
    }

    /// <summary>Was: Addhouseholdmembers() — the household-rep path, submitting the two
    /// members entered in mainuserstack (Member1 / Member2 are guaranteed filled in by that
    /// point, since HouseholdMemberEntryViewModel.Validate() requires every field on both
    /// before Next will move past that section).</summary>
    private async Task AddHouseholdMembersAsync()
    {
        var newMembers = new ObservableCollection<householdgroupjsondetails>
        {
            new householdgroupjsondetails
            {
                household_group_id = UserDetails?.Householdgroupid,
                household_individual_name = $"{Member1.FirstName.Trim()} {Member1.Surname.Trim()}",
                household_individual_email = Member1.IsEmailSectionVisible ? Member1.Email?.Trim() : null,
                household_individual_status = "Onboarding",
                household_individual_relationship = Member1.SelectedRelationshipOption?.Text.Trim(),
                household_individual_age = Member1.SelectedAgeOption?.Trim()
            },
            new householdgroupjsondetails
            {
                household_group_id = UserDetails?.Householdgroupid,
                household_individual_name = $"{Member2.FirstName.Trim()} {Member2.Surname.Trim()}",
                household_individual_email = Member2.IsEmailSectionVisible ? Member2.Email?.Trim() : null,
                household_individual_status = "Onboarding",
                household_individual_relationship = Member2.SelectedRelationshipOption?.Text.Trim(),
                household_individual_age = Member2.SelectedAgeOption?.Trim()
            }
        };


      

        string json = JsonConvert.SerializeObject(newMembers, Newtonsoft.Json.Formatting.None)
            .Replace("\r", "").Replace("\n", "").Trim();
        string base64Members = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        string url = $"https://hopper.peoplewith.com/household-individual-alignment.php?hij={base64Members}"
            .Replace("\r", "").Replace("\n", "").Trim();

        // NOTE: the original guards this call with `if (Allregfields[currentFieldIndex]
        // .XamlNameArea != "mainuserstack")` — but by the time CreateAccount runs,
        // currentFieldIndex still points at the last section shown (finishstack), so that
        // check is always true in practice and never actually skips the call. Sent
        // unconditionally here, matching the guard's real effect rather than its literal form.
        var response = await APICalls.Instance.GetClient().GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _ = response.StatusCode + response.ReasonPhrase;
        }
    }

    /// <summary>Was: Updatehouseholdmembers() — the "filling this in for myself as a household
    /// member" path.</summary>
    private async Task UpdateHouseholdMembersAsync()
    {
        if (_allGroupDetailsPassed is null || !_allGroupDetailsPassed.Any())
        {
            var groupedData = await APICalls.Instance.GetUserHouseholdInfo(NewUser.householdgroupid);
            if (groupedData is not null)
            {
                _allHouseholdGroup = groupedData.FirstOrDefault();
                _allGroupDetailsPassed = _allHouseholdGroup?.userdetailslist ?? new ObservableCollection<householdgroupjsondetails>();
            }
        }

        if (_userInfoForBaseline is null && _allGroupDetailsPassed is not null)
        {
            _userInfoForBaseline = _allGroupDetailsPassed.FirstOrDefault(x => x.household_individual_userid == NewUser.userid);
        }

        var updateUser = _allGroupDetailsPassed?
            .FirstOrDefault(x => x?.household_individual_userid == _userInfoForBaseline?.household_individual_userid);

        if (updateUser is not null)
        {
            updateUser.household_individual_status = "active";

            // Falls back to Name/Account's FirstName/Surname if Member1's fields are empty —
            // matches the original, which reaches for mainuserstack's fields first even though
            // that section isn't shown to a non-primary user (so they're normally empty) and
            // only actually gets a name from the Name/Account section's own fields.
            string initialName = $"{Member1.FirstName?.Trim() ?? string.Empty} {Member1.Surname?.Trim() ?? string.Empty}".Trim();
            updateUser.household_individual_name = !string.IsNullOrEmpty(initialName)
                ? initialName
                : $"{FirstName?.Trim() ?? string.Empty} {Surname?.Trim() ?? string.Empty}".Trim();
        }

        var houseRepToRemove = _allGroupDetailsPassed?.FirstOrDefault(x => x?.household_individual_relationship == "Household Rep");
        if (houseRepToRemove is not null)
        {
            _allGroupDetailsPassed!.Remove(houseRepToRemove);
        }

        if (_allHouseholdGroup is not null)
        {

            //Status Issue
            //var updateData = new { groupuserdetails = _allHouseholdGroup.groupuserdetails };
            _allHouseholdGroup.groupuserdetails = JsonConvert.SerializeObject(_allGroupDetailsPassed);
            var updateData = new { groupuserdetails = _allHouseholdGroup.groupuserdetails };
            string updateJson = System.Text.Json.JsonSerializer.Serialize(updateData);

            if (_userInfoForBaseline is not null)
            {
                string url = $"{APICalls.ApplicationURL}householdgroup/householdgroupid/{_userInfoForBaseline.household_group_id}";
                using var content = new StringContent(updateJson, Encoding.UTF8, "application/json");
                var response = await APICalls.Instance.GetClient().PatchAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    _ = await response.Content.ReadAsStringAsync();
                }
            }
        }
    }

    private async Task SubmitQuestionnaireAsync()
    {
        var newBaselineQuestionnaire = new newuserquestionnaire
        {
            userid = NewUser.userid,
            questionnaireid = "b1_individual_questionnaire"
        };

        if (!_householdRepFromReg)
        {
            QuestionnaireResults.Add(new QuestionnaireResult
            {
                QuestionId = "b_who_filled_form",
                AnswerId = "b_who_filled_form_a2",
                InternalName = "b_who_filled_form"
            });

            var relationshipResult = new QuestionnaireResult { QuestionId = "b_who_filled_form_2" };

            var fillingForField = _mainUserSectionSaved?.subFields?.FirstOrDefault(f => f.Id == "familyrelationship");
            if (fillingForField is not null)
            {
                var matchedOption = fillingForField.Options?.FirstOrDefault(o =>
                    o.Text.Equals(_userInfoForBaseline?.household_individual_relationship, StringComparison.OrdinalIgnoreCase));

                relationshipResult.AnswerId = matchedOption?.AnswerId ?? "b_who_filled_form_2_4";
            }

            QuestionnaireResults.Add(relationshipResult);
        }

        newBaselineQuestionnaire.feedback = System.Text.Json.JsonSerializer.Serialize(QuestionnaireResults);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        string json = System.Text.Json.JsonSerializer.Serialize(newBaselineQuestionnaire, options);

        string url = $"{APICalls.ApplicationURL}userquestionnaire";
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await APICalls.Instance.GetClient().PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            _ = await response.Content.ReadAsStringAsync();
        }
    }

    /// <summary>Was: the Azure Blob Storage half of CreateAccount(). Returns the generated
    /// blob filename, matching the original's naming scheme, for SubmitConsentAsync to
    /// reference. The connection string is a placeholder here exactly as it was in the
    /// original file as supplied — fill in your own before shipping.</summary>
    private async Task<string> UploadSignatureAsync()
    {
        const string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=peoplewithappiamges;AccountKey=9maBMGnjWp6KfOnOuXWHqveV4LPKyOnlCgtkiKQOeA+d+cr/trKApvPTdQ+piyQJlicOE6dpeAWA56uD39YJhg==;EndpointSuffix=core.windows.net";

        var random = new Random();
        string imageName = $"{NewUser.userid}-{DateTime.Now:HHmmssfff}-{random.Next(1000, 10000000)}.png";

        if (RequestSignatureImageStream is null) return imageName;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var signatureStream = await RequestSignatureImageStream(cts.Token);

        if (signatureStream is not null)
        {
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("consentsignatures");
            var blobClient = containerClient.GetBlobClient(imageName);
            await blobClient.UploadAsync(signatureStream);
            await signatureStream.DisposeAsync();
        }

        return imageName;
    }

    private async Task SubmitConsentAsync(string signatureFileName)
    {
        var userConsent = new userconsent
        {
            userid = NewUser.userid,
            consentid = NewUser.signupcodeid,
            signaturefilename = signatureFileName
        };

        if (!string.IsNullOrEmpty(_tandCNonRequired))
        {
            userConsent.consentselection = _tandCNonRequired;
        }

        if (IsUnder10StackVisible)
        {
            string role = SelectedUnder10RoleOption?.label?.Trim() ?? string.Empty;
            userConsent.additionaldetails = $"{Under10Name}|{role}";
            userConsent.consentinput = Helpers.Settings.UsersID;
        }

        await APICalls.Instance.PostUserConsentAsync(userConsent);
    }

    private void SaveLocalUserPreferences()
    {
        Preferences.Default.Set("userid", NewUser.userid);
        Preferences.Default.Set("firstname", NewUser.firstname);
        Preferences.Default.Set("surname", NewUser.surname);
        Preferences.Default.Set("signupcode", NewUser.signupcodeid);
        Preferences.Default.Set("email", NewUser.email);
        Preferences.Default.Set("gender", NewUser.gender);
        Preferences.Default.Set("ethnicity", NewUser.ethnicity);
        Preferences.Default.Set("age", NewUser.dateofbirth);
        Preferences.Default.Set("userpasswordhash", NewUser.password);
        Preferences.Default.Set("sideupcodegrouping", NewUser.signupcodegrouping);
        Preferences.Default.Set("householdgrouping", NewUser.householdgroupid);
        Preferences.Default.Set("primarycardid", NewUser.primarycareid);
        Preferences.Default.Set("details", NewUser.details);
        Preferences.Default.Set("postcode", NewUser.postcode);
        Preferences.Default.Set("isprimaryuser", NewUser.primaryuser);
        Preferences.Default.Set("phonenumber", NewUser.telephone);
        Preferences.Default.Set("addresslineone", AddressLine1?.Trim() ?? string.Empty);
        Preferences.Default.Set("town", Town?.Trim() ?? string.Empty);
        Preferences.Default.Set("County", County?.Trim() ?? string.Empty);
        Preferences.Default.Set("devicemanufacturer", DeviceInfo.Manufacturer);
        Preferences.Default.Set("devicemodel", DeviceInfo.Model);
        Preferences.Default.Set("deviceversion", DeviceInfo.VersionString);

        if (NewUser.primaryuser)
        {
            Preferences.Default.Set("primaryuserid", NewUser.userid);
        }
    }




    /// <summary>Populates the shared header plus any section-specific labels/options that come
    /// from the server-driven RegField.subFields config. Was: ShowCurrentStack().</summary>
    private void ApplyCurrentSectionText()
    {
        if (CurrentFieldIndex < 0 || CurrentFieldIndex >= RegistrationSections.Count) return;
        var field = RegistrationSections[CurrentFieldIndex];

        SectionTitle = field.Label;
        SectionSubtitle = field.Placeholder;
        SectionHelpText = field.HelpText;
        IsSectionHelpTextVisible = !string.IsNullOrEmpty(field.HelpText);
        NextButtonText = "Next";

        // namestack: original's ShowCurrentStack branch for this key was entirely commented
        // out, so there's nothing live to port — the header text above is all it ever set.

        if (field.XamlNameArea == "addressstack")
        {
            var depField = (field.subFields ?? new List<RegField>()).FirstOrDefault(f => f.Id == "addressLine1");
            if (depField?.Options is { Count: > 0 })
            {
                ValidPostcodeList = depField.Options[0].validpostcodesvalues ?? new List<string>();
            }
        }

        else if (field.XamlNameArea == "genderstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var dobField = subFields.FirstOrDefault(f => f.Id == "dob");
            if (dobField is not null)
            {
                DobLabel = dobField.Label;
                DobHelpText = dobField.HelpText;
            }

            var genderField = subFields.FirstOrDefault(f => f.Id == "sexAtBirth");
            if (genderField is not null)
            {
                SexLabel = genderField.Label;
                SexSubLabel = genderField.SubLabel;
                SexHelpText = genderField.HelpText;
                var SelectedGO = SelectedGenderOption;                    
                GenderOptions = new ObservableCollection<OptionDetails>(genderField.Options ?? new List<OptionDetails>());
                if(SelectedGO is not null) { SelectedGenderOption = SelectedGO; }
            }

            var genderMatchField = subFields.FirstOrDefault(f => f.Id == "genderMatch");
            if (genderMatchField is not null)
            {
                GenderMatchLabel = genderMatchField.Label;
                var selected = SelectedGenderMatchOption; 
                GenderMatchOptions = new ObservableCollection<OptionDetails>(genderMatchField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedGenderMatchOption = selected; }

                if (!QuestionnaireResults.Any(a => a.InternalName == "genderMatch"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult
                    {
                        InternalName = "genderMatch",
                        QuestionId = genderMatchField.questionid,
                        AnswerId = ""
                    });
                }
            }

            var genderIdField = subFields.FirstOrDefault(f => f.Id == "genderidentity");
            if (genderIdField is not null)
            {
                GenderIdentityLabel = genderIdField.Label;
                GenderIdentityHelpText = genderIdField.HelpText;
                var selected = SelectedGenderIdentityOption;
                GenderIdentityOptions = new ObservableCollection<OptionDetails>(genderIdField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedGenderIdentityOption = selected; }

                if (!QuestionnaireResults.Any(a => a.InternalName == "genderidentity"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult
                    {
                        InternalName = "genderidentity",
                        QuestionId = genderIdField.questionid,
                        AnswerId = ""
                    });
                }
            }
        }

        // Additional "else if (field.XamlNameArea == "...")" blocks land here as each
        // remaining section is ported — mirrors the corresponding branch in the original
        // ShowCurrentStack().
        else if (field.XamlNameArea == "ethnicitystack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var ethField = subFields.FirstOrDefault(f => f.Id == "ethnicityDescription");
            if (ethField is not null)
            {
                EthnicityLabel = ethField.Label;
                EthnicitySubLabel = ethField.SubLabel;
                var selected = SelectedEthnicityOption;
                EthnicityOptions = new ObservableCollection<OptionDetails>(ethField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedEthnicityOption = selected; }

                var splitHelp = (ethField.HelpText ?? string.Empty).Split('|');
                EthnicityHelpText = splitHelp.Length > 0 ? splitHelp[0] : string.Empty;
                EthnicitySelectHelpText = splitHelp.Length > 1 ? splitHelp[1] : string.Empty;
            }

            var ethOwnWordsField = subFields.FirstOrDefault(f => f.Id == "ethnicityInOwnWords");
            if (ethOwnWordsField is not null)
            {
                EthnicityOwnWordsLabel = ethOwnWordsField.Label;
                if (!QuestionnaireResults.Any(a => a.InternalName == "ethnicityInOwnWords"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult
                    {
                        InternalName = "ethnicityInOwnWords",
                        QuestionId = ethOwnWordsField.questionid,
                        AnswerId = ""
                    });
                }
            }

            var bornUkField = subFields.FirstOrDefault(f => f.Id == "bornInUK");
            if (bornUkField is not null)
            {
                UkLabel = bornUkField.Label;
                UkHelpText = bornUkField.HelpText;
                var selected = SelectedBornInUkOption; 
                BornInUkOptions = new ObservableCollection<OptionDetails>(bornUkField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedBornInUkOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "bornInUK"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult
                    {
                        InternalName = "bornInUK",
                        QuestionId = bornUkField.questionid,
                        AnswerId = ""
                    });
                }
            }

            var moveUkField = subFields.FirstOrDefault(f => f.Id == "moveToUKAge");
            if (moveUkField is not null)
            {
                MoveToUkLabel = moveUkField.Label;
                var selected = SelectedMoveToUkOption; 
                MoveToUkOptions = new ObservableCollection<OptionDetails>(moveUkField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedMoveToUkOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "moveToUKAge"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult
                    {
                        InternalName = "moveToUKAge",
                        QuestionId = moveUkField.questionid,
                        AnswerId = ""
                    });
                }
            }

            var countryField = subFields.FirstOrDefault(f => f.Id == "countryOfOrigin");
            if (countryField is not null)
            {
                CountryOfOriginLabel = countryField.Label;
                CountryOfOriginSubLabel = countryField.SubLabel;
                var selected = SelectedCountryOfOriginOption;
                CountryOfOriginOptions = new ObservableCollection<OptionDetails>(countryField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedCountryOfOriginOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "countryOfOrigin"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult
                    {
                        InternalName = "countryOfOrigin",
                        QuestionId = countryField.questionid,
                        AnswerId = ""
                    });
                }
            }
        }
        else if (field.XamlNameArea == "bodymetricsstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var heightField = subFields.FirstOrDefault(f => f.Id == "heightUnit");
            if (heightField is not null)
            {
                HeightInputLabel = heightField.Label;
                var selected = SelectedHeightUnitOption;
                HeightUnitOptions = new ObservableCollection<OptionDetails>(heightField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedHeightUnitOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "heightUnit"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult
                    {
                        InternalName = "heightUnit",
                        QuestionId = heightField.questionid,
                        AnswerId = ""
                    });
                }
            }

            var weightField = subFields.FirstOrDefault(f => f.Id == "weightUnit");
            if (weightField is not null)
            {
                WeightInputLabel = weightField.Label;
                var selected = SelectedWeightUnitOption;
                WeightUnitOptions = new ObservableCollection<OptionDetails>(weightField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedWeightUnitOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "weightUnit"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult
                    {
                        InternalName = "weightUnit",
                        QuestionId = weightField.questionid,
                        AnswerId = ""
                    });
                }
            }

            // NOTE: the original recomputes age here with the locale-sensitive DateTime.Parse
            // rather than the exact-format TryParseExact it uses everywhere else for this same
            // field (e.g. ValidateGenderSection above) — that's a latent inconsistency (a device
            // set to a non-UK date format could parse "05/03/2020" as 3 May instead of 5 March).
            // Using the same exact-format parse as the rest of this ViewModel instead, so DOB
            // parsing behaves identically everywhere rather than differently in this one spot.
            int? age = DateTime.TryParseExact(DateOfBirthText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob)
                ? CalculateAge(dob)
                : null;

            var stepsField = subFields.FirstOrDefault(f => f.Id == "averageSteps");
            if (stepsField is not null)
            {
                IsStepsVisible = age is >= 16;
                if (IsStepsVisible)
                {
                    StepsLabel = stepsField.Label;
                    var selected = SelectedStepsOption;
                    StepsOptions = new ObservableCollection<OptionDetails>(stepsField.Options ?? new List<OptionDetails>());
                    if (selected is not null) { SelectedStepsOption = selected; }
                }
                if (!QuestionnaireResults.Any(a => a.InternalName == "averageSteps"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult
                    {
                        InternalName = "averageSteps",
                        QuestionId = stepsField.questionid,
                        AnswerId = ""
                    });
                }
            }

            var gymField = subFields.FirstOrDefault(f => f.Id == "muscleMass");
            if (gymField is not null)
            {
                IsGymVisible = age is >= 18;
                if (IsGymVisible)
                {
                    GymLabel = gymField.Label;
                    GymHelpText = gymField.HelpText;
                    var selected = SelectedGymOption;
                    GymOptions = new ObservableCollection<OptionDetails>(gymField.Options ?? new List<OptionDetails>());
                    if (selected is not null) { SelectedGymOption = selected; }
                }
                if (!QuestionnaireResults.Any(a => a.InternalName == "muscleMass"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult
                    {
                        InternalName = "muscleMass",
                        QuestionId = gymField.questionid,
                        AnswerId = ""
                    });
                }
            }
        }
        else if (field.XamlNameArea == "mainuserstack")
        {
            // Original only populates this when the current user is the primary/household
            // rep; when not, ShowCurrentStack's mainuserstack branch had an explicit
            // "//skip this field" else-branch — mainuserstack is also removed from
            // RegistrationSections entirely for non-primary users back in
            // LoadRegistrationConfigAsync, so reaching here with Primaryuser false shouldn't
            // normally happen, but the guard is kept to match the original's own check.
            if (UserDetails is not null && UserDetails.Primaryuser)
            {
                var relationshipField = (field.subFields ?? new List<RegField>())
                    .FirstOrDefault(f => f.Id == "familyrelationship");
                if (relationshipField is not null)
                {
                    var options = new ObservableCollection<OptionDetails>(relationshipField.Options ?? new List<OptionDetails>());
                    var M1relation = Member1.SelectedRelationshipOption; 
                    Member1.RelationshipOptions = options;
                    if (M1relation is not null) { Member1.SelectedRelationshipOption = M1relation; }
                    var M2relation = Member2.SelectedRelationshipOption;
                    Member2.RelationshipOptions = new ObservableCollection<OptionDetails>(options);
                    if (M2relation is not null) { Member2.SelectedRelationshipOption = M2relation; }
                    
                }
            }
        }
        else if (field.XamlNameArea == "educationworkstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var highEdField = subFields.FirstOrDefault(f => f.Id == "highestEducation");
            if (highEdField is not null)
            {
                HighestEducationLabel = highEdField.Label;
                HighestEducationHelpText = highEdField.HelpText;
                var selected = SelectedHighestEducationOption;
                HighestEducationOptions = new ObservableCollection<OptionDetails>(highEdField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedHighestEducationOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "highestEducation"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "highestEducation", QuestionId = highEdField.questionid, AnswerId = "" });
                }
            }

            var currentSituationField = subFields.FirstOrDefault(f => f.Id == "currentSituation");
            if (currentSituationField is not null)
            {
                CurrentSituationLabel = currentSituationField.Label;
                CurrentSituationHelpText = currentSituationField.HelpText;
                var selected = SelectedCurrentSituationOption;
                CurrentSituationOptions = new ObservableCollection<OptionDetails>(currentSituationField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedCurrentSituationOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "currentSituation"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "currentSituation", QuestionId = currentSituationField.questionid, AnswerId = "" });
                }
            }

            var workTypeField = subFields.FirstOrDefault(f => f.Id == "workType");
            if (workTypeField is not null)
            {
                WorkTypeLabel = workTypeField.Label;
                var selected = SelectedWorkTypeOption;
                WorkTypeOptions = new ObservableCollection<OptionDetails>(workTypeField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedWorkTypeOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "workType"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "workType", QuestionId = workTypeField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "householdstructurestack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var peopleField = subFields.FirstOrDefault(f => f.Id == "peopleinhome");
            if (peopleField is not null)
            {
                PeopleLabel = peopleField.Label;
                PeopleSubLabel = peopleField.SubLabel;
                if (!QuestionnaireResults.Any(a => a.InternalName == "peopleinhome"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "peopleinhome", QuestionId = peopleField.questionid, AnswerId = "" });
                }
            }

            var roomsField = subFields.FirstOrDefault(f => f.Id == "rooms");
            if (roomsField is not null)
            {
                RoomsLabel = roomsField.Label;
                RoomsSubLabel = roomsField.SubLabel;
                if (!QuestionnaireResults.Any(a => a.InternalName == "rooms"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "rooms", QuestionId = roomsField.questionid, AnswerId = "" });
                }
            }

            var bathroomsField = subFields.FirstOrDefault(f => f.Id == "bathrooms");
            if (bathroomsField is not null)
            {
                SharedBathroomsLabel = bathroomsField.Label;
                SharedBathroomsSubLabel = bathroomsField.SubLabel;
                if (!QuestionnaireResults.Any(a => a.InternalName == "bathrooms"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "bathrooms", QuestionId = bathroomsField.questionid, AnswerId = "" });
                }
            }

            var ventField = subFields.FirstOrDefault(f => f.Id == "ventiliation");
            if (ventField is not null)
            {
                VentilationLabel = ventField.Label;
                var selected = SelectedVentilationOption;
                VentilationOptions = new ObservableCollection<OptionDetails>(ventField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedVentilationOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "ventiliation"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "ventiliation", QuestionId = ventField.questionid, AnswerId = "" });
                }
            }

            var mouldField = subFields.FirstOrDefault(f => f.Id == "mould");
            if (mouldField is not null)
            {
                MouldLabel = mouldField.Label;
                var selected = SelectedMouldOption;
                MouldOptions = new ObservableCollection<OptionDetails>(mouldField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedMouldOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "mould"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "mould", QuestionId = mouldField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "nhsnumstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var nhsField = subFields.FirstOrDefault(f => f.Id == "nhsNumber");
            if (nhsField is not null)
            {
                NhsNumberLabel = nhsField.Label;
                NhsNumberSubLabel = nhsField.SubLabel;
                var splitHelp = (nhsField.HelpText ?? string.Empty).Split('|');
                NhsNumberHelpText = splitHelp.Length > 0 ? splitHelp[0] : string.Empty;
                NhsNumberSelectHelpText = splitHelp.Length > 1 ? splitHelp[1] : string.Empty;
                if (!QuestionnaireResults.Any(a => a.InternalName == "nhsNumber"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "nhsNumber", QuestionId = nhsField.questionid, AnswerId = "" });
                }
            }

            var gpField = subFields.FirstOrDefault(f => f.Id == "gpRegistered");
            if (gpField is not null)
            {
                GpRegisteredLabel = gpField.Label;
                GpRegisteredSubLabel = gpField.SubLabel;
                var selected = SelectedGpRegisteredOption;
                GpRegisteredOptions = new ObservableCollection<OptionDetails>(gpField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedGpRegisteredOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "gpRegistered"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "gpRegistered", QuestionId = gpField.questionid, AnswerId = "" });
                }
            }

            // Always looked up (not gated on gpField's answer) — matches the original, which
            // populates gpautocomplete.ItemsSource unconditionally in ShowCurrentStack and
            // relies purely on IsGpInfoVisible (was: gpinfolbl.IsVisible) to hide the control.
            var gpPracticeField = subFields.FirstOrDefault(f => f.Id == "gpPracticeName");
            if (gpPracticeField is not null)
            {
                GpPracticeLabel = gpPracticeField.Label;
                GpPracticeSubLabel = gpPracticeField.SubLabel;
                GpPracticeOptions = new ObservableCollection<OptionDetails>(gpPracticeField.Options ?? new List<OptionDetails>());
                if (!QuestionnaireResults.Any(a => a.InternalName == "gpPracticeName"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "gpPracticeName", QuestionId = gpPracticeField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "ristack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var coughField = subFields.FirstOrDefault(f => f.Id == "coughfield");
            if (coughField is not null)
            {
                CoughLabel = coughField.Label;
                var selected = SelectedCoughOption;
                CoughOptions = new ObservableCollection<OptionDetails>(coughField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedCoughOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "coughfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "coughfield", QuestionId = coughField.questionid, AnswerId = "" });
                }
            }

            var hosField = subFields.FirstOrDefault(f => f.Id == "hosfield");
            if (hosField is not null)
            {
                HospitalLabel = hosField.Label;
                HospitalSubLabel = hosField.SubLabel;
                HospitalHelpText = hosField.HelpText;
                var selected = SelectedHospitalOption;
                HospitalOptions = new ObservableCollection<OptionDetails>(hosField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedHospitalOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "hosfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "hosfield", QuestionId = hosField.questionid, AnswerId = "" });
                }
            }

            var infectionField = subFields.FirstOrDefault(f => f.Id == "infectionfield");
            if (infectionField is not null)
            {
                InfectionLabel = infectionField.Label;
                InfectionSubLabel = infectionField.SubLabel;
                if (!QuestionnaireResults.Any(a => a.InternalName == "infectionfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "infectionfield", QuestionId = infectionField.questionid, AnswerId = "" });
                }
            }

            var venField = subFields.FirstOrDefault(f => f.Id == "venfield");
            if (venField is not null)
            {
                VentilatorLabel = venField.Label;
                var selected = SelectedVentilatorOption;
                VentilatorOptions = new ObservableCollection<OptionDetails>(venField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedVentilatorOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "venfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "venfield", QuestionId = venField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "hcstack")
        {
            var diaField = (field.subFields ?? new List<RegField>()).FirstOrDefault(f => f.Id == "diastack");
            if (diaField is not null)
            {
                DiaLabel = diaField.Label;
                DiaDirections = diaField.SubLabel;
                if (!QuestionnaireResults.Any(a => a.InternalName == "diastack"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "diastack", QuestionId = diaField.questionid, AnswerId = "" });
                }
            }
            // disautocomplete's ItemsSource (was: diaaddstack's Options) isn't populated here —
            // in the original it repopulates the SAME physical control from ShowCurrentStack's
            // hcstack branch as prep for the next screen, but that control isn't even part of
            // hcstack's own visual tree (only addhcstack's), so the population there is
            // redundant. It's set once, in addhcstack's own branch below, when it's actually
            // reachable.
        }
        else if (field.XamlNameArea == "addhcstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var diaAddField = subFields.FirstOrDefault(f => f.Id == "diaaddstack");
            if (diaAddField is not null)
            {
                DiaAddLabel = diaAddField.Label;
                DiaAddDirections = diaAddField.SubLabel;
                ConditionOptions = new ObservableCollection<OptionDetails>(diaAddField.Options ?? new List<OptionDetails>());
                if (!QuestionnaireResults.Any(a => a.InternalName == "diaaddstack"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "diaaddstack", QuestionId = diaAddField.questionid, AnswerId = "" });
                }
            }

            var otherHcField = subFields.FirstOrDefault(f => f.Id == "otherhc");
            if (otherHcField is not null)
            {
                OtherHcLabel = otherHcField.Label;
                var selected = SelectedOtherHcOption;
                OtherHcOptions = new ObservableCollection<OptionDetails>(otherHcField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedOtherHcOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "otherhc"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "otherhc", QuestionId = otherHcField.questionid, AnswerId = "" });
                }
            }

            var affectedSystemField = subFields.FirstOrDefault(f => f.Id == "affectedsystem");
            if (affectedSystemField is not null)
            {
                TypeOtherHcLabel = affectedSystemField.Label;
                var selected = SelectedTypeOtherHcOption;
                TypeOtherHcOptions = new ObservableCollection<OptionDetails>(affectedSystemField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedTypeOtherHcOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "affectedsystem"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "affectedsystem", QuestionId = affectedSystemField.questionid, AnswerId = "" });
                }
            }

            var otherHcEnterField = subFields.FirstOrDefault(f => f.Id == "otherhealthconditions");
            if (otherHcEnterField is not null)
            {
                OtherHcEnterLabel = otherHcEnterField.Label;
                OtherHcEnterSubLabel = otherHcEnterField.Placeholder; // matches original: uses Placeholder, not SubLabel, for this one
                if (!QuestionnaireResults.Any(a => a.InternalName == "otherhealthconditions"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "otherhealthconditions", QuestionId = otherHcEnterField.questionid, AnswerId = "" });
                }
            }

            var pastCancerField = subFields.FirstOrDefault(f => f.Id == "pastcancerdiagnosis");
            if (pastCancerField is not null)
            {
                CancerLabel = pastCancerField.Label;
                var selected = SelectedCancerOption;
                CancerOptions = new ObservableCollection<OptionDetails>(pastCancerField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedCancerOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "pastcancerdiagnosis"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "pastcancerdiagnosis", QuestionId = pastCancerField.questionid, AnswerId = "" });
                }
            }

            var cancerNowField = subFields.FirstOrDefault(f => f.Id == "cancerremissionstatus");
            if (cancerNowField is not null)
            {
                CancerNowLabel = cancerNowField.Label;
                var selected = SelectedCancerNowOption;
                CancerNowHelpText = cancerNowField.HelpText;
                CancerNowOptions = new ObservableCollection<OptionDetails>(cancerNowField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedCancerNowOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "cancerremissionstatus"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "cancerremissionstatus", QuestionId = cancerNowField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "medynstack")
        {
            var medynField = (field.subFields ?? new List<RegField>()).FirstOrDefault(f => f.Id == "firstmedstack");
            if (medynField is not null)
            {
                MedynLabel = medynField.Label;
                MedynDirections = medynField.SubLabel;
                MedHelpText = medynField.HelpText;
                if (!QuestionnaireResults.Any(a => a.InternalName == "firstmedstack"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "firstmedstack", QuestionId = medynField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "medicationsstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var medField = subFields.FirstOrDefault(f => f.Id == "medqstack");
            if (medField is not null)
            {
                MedLabel = medField.Label;
                MedDirections = medField.SubLabel;
                MedicationOptions = new ObservableCollection<OptionDetails>(medField.Options ?? new List<OptionDetails>());
                if (!QuestionnaireResults.Any(a => a.InternalName == "medqstack"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "medqstack", QuestionId = medField.questionid, AnswerId = "" });
                }
            }

            var otherMedField = subFields.FirstOrDefault(f => f.Id == "othermeds");
            if (otherMedField is not null)
            {
                OtherMedLabel = otherMedField.Label;
                var selected = SelectedOtherMedOption;
                OtherMedOptions = new ObservableCollection<OptionDetails>(otherMedField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedOtherMedOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "othermeds"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "othermeds", QuestionId = otherMedField.questionid, AnswerId = "" });
                }
            }

            var otherMedDetailField = subFields.FirstOrDefault(f => f.Id == "othermedications");
            if (otherMedDetailField is not null)
            {
                OtherMedDetailsLabel = otherMedDetailField.Label;
                OtherMedDetailsSubLabel = otherMedDetailField.Placeholder; // matches original: uses Placeholder here, same as addhcstack's equivalent field
                if (!QuestionnaireResults.Any(a => a.InternalName == "othermedications"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "othermedications", QuestionId = otherMedDetailField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "rvstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var ynFluField = subFields.FirstOrDefault(f => f.Id == "ynflu");
            if (ynFluField is not null)
            {
                YnFluLabel = ynFluField.Label;
                YnFluSubLabel = ynFluField.SubLabel;
                YnFluDirections = ynFluField.Directions;
                var prevYnFlu = SelectedYnFluOption;
                YnFluOptions = new ObservableCollection<OptionDetails>(ynFluField.Options ?? new List<OptionDetails>());
                if (prevYnFlu is not null) { SelectedYnFluOption = prevYnFlu; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "ynflu"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "ynflu", QuestionId = ynFluField.questionid, AnswerId = "" });
                }
            }

            var fluField = subFields.FirstOrDefault(f => f.Id == "flufield");
            if (fluField is not null)
            {
                FluLabel = fluField.Label;
                FluSubLabel = fluField.SubLabel;
                var Seleted = SelectedFluOptions; 
                FluOptions = new ObservableCollection<OptionDetails>(fluField.Options ?? new List<OptionDetails>());
                if (Seleted is not null && Seleted.Count > 0) { SelectionFluRestore?.Invoke(); }
                
                if (!QuestionnaireResults.Any(a => a.InternalName == "flufield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "flufield", QuestionId = fluField.questionid, AnswerId = "" });
                }
            }

            var fluDateField = subFields.FirstOrDefault(f => f.Id == "fludatefield");
            if (fluDateField is not null)
            {
                FluDateLabel = fluDateField.Label;
                FluDateSubLabel = fluDateField.SubLabel;
                if (!QuestionnaireResults.Any(a => a.InternalName == "fludatefield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "fludatefield", QuestionId = fluDateField.questionid, AnswerId = "" });
                }
            }

            var covidDateField = subFields.FirstOrDefault(f => f.Id == "coviddatefield");
            if (covidDateField is not null)
            {
                CovidDateLabel = covidDateField.Label;
                CovidDateSubLabel = covidDateField.SubLabel;
                if (!QuestionnaireResults.Any(a => a.InternalName == "coviddatefield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "coviddatefield", QuestionId = covidDateField.questionid, AnswerId = "" });
                }
            }

            var rsvDateField = subFields.FirstOrDefault(f => f.Id == "rsvdatefield");
            if (rsvDateField is not null)
            {
                RsvDateLabel = rsvDateField.Label;
                RsvDateSubLabel = rsvDateField.SubLabel;
                if (!QuestionnaireResults.Any(a => a.InternalName == "rsvdatefield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "rsvdatefield", QuestionId = rsvDateField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "dietstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var dietField = subFields.FirstOrDefault(f => f.Id == "dietfield");
            if (dietField is not null)
            {
                DietLabel = dietField.Label;
                DietSubLabel = dietField.SubLabel;
                DietHelpText = dietField.HelpText;
                IsDietSubLabelVisible = !string.IsNullOrEmpty(dietField.SubLabel);
                var selected = SelectedDietOption;
                DietOptions = new ObservableCollection<OptionDetails>(dietField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedDietOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "dietfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "dietfield", QuestionId = dietField.questionid, AnswerId = "" });
                }
            }

            var dietLengthField = subFields.FirstOrDefault(f => f.Id == "dietlengthfield");
            if (dietLengthField is not null)
            {
                DietLengthLabel = dietLengthField.Label;
                var selected = SelectedDietLengthOption;
                DietLengthOptions = new ObservableCollection<OptionDetails>(dietLengthField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedDietLengthOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "dietlengthfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "dietlengthfield", QuestionId = dietLengthField.questionid, AnswerId = "" });
                }
            }

            // prevdietfield: the "previous diet" question, only shown when diet length is < 6 months (b_diet_type_length_1)
            var prevDietField = subFields.FirstOrDefault(f => f.Id == "dietfield" && f.questionid == "b_diet_type_2");
            if (prevDietField is not null)
            {
                PrevDietLabel = prevDietField.Label;
                var selected = SelectedPrevDietOption;
                PrevDietHelpText = prevDietField.HelpText;
                PrevDietOptions = new ObservableCollection<OptionDetails>(prevDietField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedPrevDietOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "prevdietfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "prevdietfield", QuestionId = prevDietField.questionid, AnswerId = "" });
                }
            }

            var supplementsField = subFields.FirstOrDefault(f => f.Id == "supplementsfield");
            if (supplementsField is not null)
            {
                SupplementsLabel = supplementsField.Label;
                SupplementsSubLabel = supplementsField.SubLabel;
                IsSupplementsSubLabelVisible = !string.IsNullOrEmpty(supplementsField.SubLabel);
                var selected = SelectedSupplementsOption;
                SupplementsOptions = new ObservableCollection<OptionDetails>(supplementsField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedSupplementsOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "supplementsfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "supplementsfield", QuestionId = supplementsField.questionid, AnswerId = "" });
                }
            }

            var takeField = subFields.FirstOrDefault(f => f.Id == "takefield");
            if (takeField is not null)
            {
                TakeLabel = takeField.Label;
                TakeSubLabel = takeField.SubLabel;
                IsTakeSubLabelVisible = !string.IsNullOrEmpty(takeField.SubLabel);
                var selected = SelectedTakeOption;
                TakeOptions = new ObservableCollection<OptionDetails>(takeField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedTakeOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "takefield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "takefield", QuestionId = takeField.questionid, AnswerId = "" });
                }
            }

            var nutrientField = subFields.FirstOrDefault(f => f.Id == "nutrientlistfield");
            if (nutrientField is not null)
            {
                ExtraLabel = nutrientField.Label;
                ExtraSubLabel = nutrientField.SubLabel;
                IsExtraSubLabelVisible = !string.IsNullOrEmpty(nutrientField.SubLabel);
                var selected = SelectedExtraOptions; 
                ExtraOptions = new ObservableCollection<OptionDetails>(nutrientField.Options ?? new List<OptionDetails>());
                if (selected is not null && selected.Count > 0) { SelecteionExtraRestore?.Invoke(); }
              

                if (!QuestionnaireResults.Any(a => a.InternalName == "nutrientlistfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "nutrientlistfield", QuestionId = nutrientField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "menstrualstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var mcField = subFields.FirstOrDefault(f => f.Id == "menstrualcyclefield");
            if (mcField is not null)
            {
                MenstrualLabel = mcField.Label;
                MenstrualHelpText = mcField.HelpText;
                var selected = SelectedMenstrualOption;
                MenstrualOptions = new ObservableCollection<OptionDetails>(mcField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedMenstrualOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "menstrualcyclefield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "menstrualcyclefield", QuestionId = mcField.questionid, AnswerId = "" });
                }
            }

            var pregField = subFields.FirstOrDefault(f => f.Id == "pregnancyweeksfield");
            if (pregField is not null)
            {
                PregnancyWeeksLabel = pregField.Label;
                if (!QuestionnaireResults.Any(a => a.InternalName == "pregnancyweeksfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "pregnancyweeksfield", QuestionId = pregField.questionid, AnswerId = "" });
                }
            }

            var ddField = subFields.FirstOrDefault(f => f.Id == "deliverydatefield");
            if (ddField is not null)
            {
                DeliveryDateLabel = ddField.Label;
                if (!QuestionnaireResults.Any(a => a.InternalName == "deliverydatefield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "deliverydatefield", QuestionId = ddField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "htstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var mobField = subFields.FirstOrDefault(f => f.Id == "mobilityfield");
            if (mobField is not null)
            {
                MobilityLabel = mobField.Label;
                MobilitySubLabel = mobField.SubLabel;
                IsMobilitySubLabelVisible = !string.IsNullOrEmpty(mobField.SubLabel);
                var selected = SelectedMobilityOption;
                MobilityOptions = new ObservableCollection<OptionDetails>(mobField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedMobilityOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "mobilityfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "mobilityfield", QuestionId = mobField.questionid, AnswerId = "" });
                }
            }

            var scField = subFields.FirstOrDefault(f => f.Id == "selfcarefield");
            if (scField is not null)
            {
                SelfCareLabel = scField.Label;
                SelfCareSubLabel = scField.SubLabel;
                IsSelfCareSubLabelVisible = !string.IsNullOrEmpty(scField.SubLabel);
                var selected = SelectedSelfCareOption;
                SelfCareOptions = new ObservableCollection<OptionDetails>(scField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedSelfCareOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "selfcarefield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "selfcarefield", QuestionId = scField.questionid, AnswerId = "" });
                }
            }

            var uaField = subFields.FirstOrDefault(f => f.Id == "usualactivitiesfield");
            if (uaField is not null)
            {
                UsualActivitiesLabel = uaField.Label;
                UsualActivitiesSubLabel = uaField.SubLabel;
                IsUsualActivitiesSubLabelVisible = !string.IsNullOrEmpty(uaField.SubLabel);
                var selected = SelectedUsualActivitiesOption;
                UsualActivitiesOptions = new ObservableCollection<OptionDetails>(uaField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedUsualActivitiesOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "usualactivitiesfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "usualactivitiesfield", QuestionId = uaField.questionid, AnswerId = "" });
                }
            }

            var painField = subFields.FirstOrDefault(f => f.Id == "painfield");
            if (painField is not null)
            {
                PainLabel = painField.Label;
                PainSubLabel = painField.SubLabel;
                IsPainSubLabelVisible = !string.IsNullOrEmpty(painField.SubLabel);
                var selected = SelectedPainOption;
                PainOptions = new ObservableCollection<OptionDetails>(painField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedPainOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "painfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "painfield", QuestionId = painField.questionid, AnswerId = "" });
                }
            }

            var depField = subFields.FirstOrDefault(f => f.Id == "anxietydepressionfield");
            if (depField is not null)
            {
                AnxietyDepressionLabel = depField.Label;
                AnxietyDepressionSubLabel = depField.SubLabel;
                IsAnxietyDepressionSubLabelVisible = !string.IsNullOrEmpty(depField.SubLabel);
                var selected = SelectedAnxietyDepressionOption;
                AnxietyDepressionOptions = new ObservableCollection<OptionDetails>(depField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAnxietyDepressionOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "anxietydepressionfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "anxietydepressionfield", QuestionId = depField.questionid, AnswerId = "" });
                }
            }

            var sliderField = subFields.FirstOrDefault(f => f.Id == "healthvasfield");
            if (sliderField is not null)
            {
                SliderLabel = sliderField.Label;
                SliderSubLabel = sliderField.SubLabel;
                IsSliderSubLabelVisible = !string.IsNullOrEmpty(sliderField.SubLabel);

                var options = sliderField.Options ?? new List<OptionDetails>();
                Slider0Label = options.FirstOrDefault(a => a.Value == "0")?.Text ?? string.Empty;
                Slider100Label = options.FirstOrDefault(a => a.Value == "100")?.Text ?? string.Empty;
                if(HealthSliderValue == 50 && HealthSliderDisplayText == "50")
                {
                    HealthSliderValue = 50;
                } 

                if (!QuestionnaireResults.Any(a => a.InternalName == "healthvasfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "healthvasfield", QuestionId = sliderField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "additionalqstack")
        {
            var aqField = (field.subFields ?? new List<RegField>()).FirstOrDefault(f => f.Id == "addqlifestyle");
            if (aqField is not null)
            {
                AdditionalQTitle = aqField.Label;
                AdditionalQSubtitle = aqField.SubLabel;
                var selected = SelectedAdditionalQOption;
                AdditionalQOptions = new ObservableCollection<OptionDetails>(aqField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAdditionalQOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "addqlifestyle"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "addqlifestyle", QuestionId = aqField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "antiviralstack")
        {
            AntiviralInfoText = field.HelpTextInfo;
            var subFields = field.subFields ?? new List<RegField>();

            var heardField = subFields.FirstOrDefault(f => f.Id == "antiviralheardfield");
            if (heardField is not null)
            {
                AntiviralHeardLabel = heardField.Label;
                AntiviralHeardSubLabel = heardField.SubLabel;
                var selected = SelectedAntiviralHeardOption;
                AntiviralHeardOptions = new ObservableCollection<OptionDetails>(heardField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAntiviralHeardOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralheardfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "antiviralheardfield", QuestionId = heardField.questionid, AnswerId = "" });
                }
            }

            var prescribedField = subFields.FirstOrDefault(f => f.Id == "antiviralprescribedfield");
            if (prescribedField is not null)
            {
                AntiviralPrescribedLabel = prescribedField.Label;
                AntiviralPrescribedSubLabel = prescribedField.SubLabel;
                var selected = SelectedAntiviralPrescribedOption;
                AntiviralPrescribedOptions = new ObservableCollection<OptionDetails>(prescribedField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAntiviralPrescribedOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralprescribedfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "antiviralprescribedfield", QuestionId = prescribedField.questionid, AnswerId = "" });
                }
            }

            var hospitalField = subFields.FirstOrDefault(f => f.Id == "antiviralhospitalfield");
            if (hospitalField is not null)
            {
                AntiviralHospitalLabel = hospitalField.Label;
                var selected = SelectedAntiviralHospitalOption;
                AntiviralHospitalOptions = new ObservableCollection<OptionDetails>(hospitalField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAntiviralHospitalOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralhospitalfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "antiviralhospitalfield", QuestionId = hospitalField.questionid, AnswerId = "" });
                }
            }

            var durationField = subFields.FirstOrDefault(f => f.Id == "antiviraldurationfield");
            if (durationField is not null)
            {
                AntiviralDurationLabel = durationField.Label;
                var selected = SelectedAntiviralDurationOption;
                AntiviralDurationOptions = new ObservableCollection<OptionDetails>(durationField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAntiviralDurationOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "antiviraldurationfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "antiviraldurationfield", QuestionId = durationField.questionid, AnswerId = "" });
                }
            }

            var preventionField = subFields.FirstOrDefault(f => f.Id == "antiviralpreventionfield");
            if (preventionField is not null)
            {
                AntiviralPreventionLabel = preventionField.Label;
                var selected = SelectedAntiviralPreventionOption;
                AntiviralPreventionOptions = new ObservableCollection<OptionDetails>(preventionField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAntiviralPreventionOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralpreventionfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "antiviralpreventionfield", QuestionId = preventionField.questionid, AnswerId = "" });
                }
            }

            var sideEffectsField = subFields.FirstOrDefault(f => f.Id == "antiviralsideeffectsfield");
            if (sideEffectsField is not null)
            {
                AntiviralSideEffectsLabel = sideEffectsField.Label;
                var selected = SelectedAntiviralSideEffectsOption;
                AntiviralSideEffectsOptions = new ObservableCollection<OptionDetails>(sideEffectsField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAntiviralSideEffectsOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralsideeffectsfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "antiviralsideeffectsfield", QuestionId = sideEffectsField.questionid, AnswerId = "" });
                }
            }

            var startedField = subFields.FirstOrDefault(f => f.Id == "antiviralstartedfield");
            if (startedField is not null)
            {
                AntiviralStartedLabel = startedField.Label;
                var selected = SelectedAntiviralStartedOption;
                AntiviralStartedOptions = new ObservableCollection<OptionDetails>(startedField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAntiviralStartedOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralstartedfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "antiviralstartedfield", QuestionId = startedField.questionid, AnswerId = "" });
                }
            }

            var finishField = subFields.FirstOrDefault(f => f.Id == "antiviralfinishfield");
            if (finishField is not null)
            {
                AntiviralFinishLabel = finishField.Label;
                var selected = SelectedAntiviralFinishOption;
                AntiviralFinishOptions = new ObservableCollection<OptionDetails>(finishField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAntiviralFinishOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralfinishfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "antiviralfinishfield", QuestionId = finishField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "tobaccostack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var everField = subFields.FirstOrDefault(f => f.Id == "tobaccoeverfield");
            if (everField is not null)
            {
                SmokeLabel = everField.Label;
                SmokeSubLabel = everField.SubLabel;
                var selected = SelectedSmokeOption;
                SmokeOptions = new ObservableCollection<OptionDetails>(everField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedSmokeOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccoeverfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "tobaccoeverfield", QuestionId = everField.questionid, AnswerId = "" });
                }
            }

            var typesField = subFields.FirstOrDefault(f => f.Id == "tobaccotypesfield");
            if (typesField is not null)
            {
                SmokeTypesLabel = typesField.Label;
                var selected = SelectedSmokeTypesOption;
                SmokeTypesOptions = new ObservableCollection<OptionDetails>(typesField.Options ?? new List<OptionDetails>());
                if (selected is not null && selected.Count > 0) { SelecteionTobaccoRestore?.Invoke(); }
                if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccotypesfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "tobaccotypesfield", QuestionId = typesField.questionid, AnswerId = "" });
                }
            }

            var startAgeField = subFields.FirstOrDefault(f => f.Id == "tobaccostartagefield");
            if (startAgeField is not null)
            {
                SmokeStartAgeLabel = startAgeField.Label;
                if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccostartagefield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "tobaccostartagefield", QuestionId = startAgeField.questionid, AnswerId = "" });
                }
            }

            var currentField = subFields.FirstOrDefault(f => f.Id == "tobaccocurrentfield");
            if (currentField is not null)
            {
                SmokeCurrentLabel = currentField.Label;
                var selected = SelectedSmokeCurrentOption;
                SmokeCurrentOptions = new ObservableCollection<OptionDetails>(currentField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedSmokeCurrentOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccocurrentfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "tobaccocurrentfield", QuestionId = currentField.questionid, AnswerId = "" });
                }
            }

            var stopAgeField = subFields.FirstOrDefault(f => f.Id == "tobaccostopagefield");
            if (stopAgeField is not null)
            {
                SmokeStopAgeLabel = stopAgeField.Label;
                if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccostopagefield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "tobaccostopagefield", QuestionId = stopAgeField.questionid, AnswerId = "" });
                }
            }

            var freqField = subFields.FirstOrDefault(f => f.Id == "tobaccofreqfield");
            if (freqField is not null)
            {
                SmokeFreqLabel = freqField.Label;
                var selected = SelectedSmokeFreqOption;
                SmokeFreqOptions = new ObservableCollection<OptionDetails>(freqField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedSmokeFreqOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccofreqfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "tobaccofreqfield", QuestionId = freqField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "alcoholstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var freqField = subFields.FirstOrDefault(f => f.Id == "alcoholfreqfield");
            if (freqField is not null)
            {
                AlcoholFreqLabel = freqField.Label;
                var selected = SelectedAlcoholFreqOption;
                AlcoholFreqOptions = new ObservableCollection<OptionDetails>(freqField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAlcoholFreqOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "alcoholfreqfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "alcoholfreqfield", QuestionId = freqField.questionid, AnswerId = "" });
                }
            }

            var unitsField = subFields.FirstOrDefault(f => f.Id == "alcoholunitsfield");
            if (unitsField is not null)
            {
                AlcoholUnitsLabel = unitsField.Label;
                var selected = SelectedAlcoholUnitsOption;
                AlcoholUnitsOptions = new ObservableCollection<OptionDetails>(unitsField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedAlcoholUnitsOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "alcoholunitsfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "alcoholunitsfield", QuestionId = unitsField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "drugstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var everField = subFields.FirstOrDefault(f => f.Id == "drugseverfield");
            if (everField is not null)
            {
                DrugsLabel = everField.Label;
                DrugsSubLabel = everField.SubLabel;
                var selected = SelectedDrugsOption;
                DrugsOptions = new ObservableCollection<OptionDetails>(everField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedDrugsOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "drugseverfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "drugseverfield", QuestionId = everField.questionid, AnswerId = "" });
                }
            }

            var listField = subFields.FirstOrDefault(f => f.Id == "drugslistfield");
            if (listField is not null)
            {
                WhatDrugsLabel = listField.Label;
                var selected = SelectedWhatDrugsOption;
                WhatDrugsOptions = new ObservableCollection<OptionDetails>(listField.Options ?? new List<OptionDetails>());
                if (selected is not null && selected.Count > 0) { SelecteionWhatDrugsRestore?.Invoke(); }
                if (!QuestionnaireResults.Any(a => a.InternalName == "drugslistfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "drugslistfield", QuestionId = listField.questionid, AnswerId = "" });
                }
            }

            var oftenField = subFields.FirstOrDefault(f => f.Id == "drugsfreqfield");
            if (oftenField is not null)
            {
                DrugsOftenLabel = oftenField.Label;
                var selected = SelectedDrugsOftenOption;
                DrugsOftenOptions = new ObservableCollection<OptionDetails>(oftenField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedDrugsOftenOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "drugsfreqfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "drugsfreqfield", QuestionId = oftenField.questionid, AnswerId = "" });
                }
            }

            // NOTE: registered under "drugssymptomsfield" — matching the field.subFields
            // lookup key the original uses here too. The original's AddDrugInfo() then reads
            // this back under a different, never-registered key ("drugsbreathingfield"),
            // which means that lookup would always return null and the answer would never
            // actually be committed. Using "drugssymptomsfield" consistently below instead —
            // see AddDrugSectionInfoAsync.
            var breathingField = subFields.FirstOrDefault(f => f.Id == "drugssymptomsfield");
            if (breathingField is not null)
            {
                BreathingLabel = breathingField.Label;
                var selected = SelectedBreathingOption;
                BreathingOptions = new ObservableCollection<OptionDetails>(breathingField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedBreathingOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "drugssymptomsfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "drugssymptomsfield", QuestionId = breathingField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "sleepstack")
        {
            var subFields = field.subFields ?? new List<RegField>();

            var onsetField = subFields.FirstOrDefault(f => f.Id == "sleeponsetfield");
            if (onsetField is not null)
            {
                SleepOnsetLabel = onsetField.Label;
                var selected = SelectedSleepOnsetOption;
                SleepOnsetOptions = new ObservableCollection<OptionDetails>(onsetField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedSleepOnsetOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "sleeponsetfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "sleeponsetfield", QuestionId = onsetField.questionid, AnswerId = "" });
                }
            }

            var wakeField = subFields.FirstOrDefault(f => f.Id == "wakedurationfield");
            if (wakeField is not null)
            {
                WakeLabel = wakeField.Label;
                var selected = SelectedWakeOption;
                WakeOptions = new ObservableCollection<OptionDetails>(wakeField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedWakeOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "wakedurationfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "wakedurationfield", QuestionId = wakeField.questionid, AnswerId = "" });
                }
            }

            var nightsField = subFields.FirstOrDefault(f => f.Id == "sleepproblemfreqfield");
            if (nightsField is not null)
            {
                NightsLabel = nightsField.Label;
                var selected = SelectedNightsOption;
                NightsOptions = new ObservableCollection<OptionDetails>(nightsField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedNightsOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "sleepproblemfreqfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "sleepproblemfreqfield", QuestionId = nightsField.questionid, AnswerId = "" });
                }
            }

            var qualityField = subFields.FirstOrDefault(f => f.Id == "sleepqualityfield");
            if (qualityField is not null)
            {
                QualityLabel = qualityField.Label;
                var selected = SelectedQualityOption;
                QualityOptions = new ObservableCollection<OptionDetails>(qualityField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedQualityOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "sleepqualityfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "sleepqualityfield", QuestionId = qualityField.questionid, AnswerId = "" });
                }
            }

            var moodField = subFields.FirstOrDefault(f => f.Id == "impactmoodfield");
            if (moodField is not null)
            {
                MoodLabel = moodField.Label;
                var selected = SelectedMoodOption;
                MoodOptions = new ObservableCollection<OptionDetails>(moodField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedMoodOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "impactmoodfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "impactmoodfield", QuestionId = moodField.questionid, AnswerId = "" });
                }
            }

            var prodField = subFields.FirstOrDefault(f => f.Id == "impactprodfield");
            if (prodField is not null)
            {
                ProdLabel = prodField.Label;
                var selected = SelectedProdOption;
                ProdOptions = new ObservableCollection<OptionDetails>(prodField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedProdOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "impactprodfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "impactprodfield", QuestionId = prodField.questionid, AnswerId = "" });
                }
            }

            var poorSleepField = subFields.FirstOrDefault(f => f.Id == "sleeptroubledfield");
            if (poorSleepField is not null)
            {
                PoorSleepLabel = poorSleepField.Label;
                var selected = SelectedPoorSleepOption;
                PoorSleepOptions = new ObservableCollection<OptionDetails>(poorSleepField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedPoorSleepOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "sleeptroubledfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "sleeptroubledfield", QuestionId = poorSleepField.questionid, AnswerId = "" });
                }
            }

            var sleepProbField = subFields.FirstOrDefault(f => f.Id == "sleepdurationfield");
            if (sleepProbField is not null)
            {
                SleepProbLabel = sleepProbField.Label;
                var selected = SelectedSleepProbOption;
                SleepProbOptions = new ObservableCollection<OptionDetails>(sleepProbField.Options ?? new List<OptionDetails>());
                if (selected is not null) { SelectedSleepProbOption = selected; }
                if (!QuestionnaireResults.Any(a => a.InternalName == "sleepdurationfield"))
                {
                    QuestionnaireResults.Add(new QuestionnaireResult { InternalName = "sleepdurationfield", QuestionId = sleepProbField.questionid, AnswerId = "" });
                }
            }
        }
        else if (field.XamlNameArea == "termsstack")
        {
            if (AllConsentDetails is null)
            {
                _ = LoadConsentAsync();
            }
        }
        else if (field.XamlNameArea == "finishstack")
        {
            // Matches the original exactly: reaching this screen immediately flips the button
            // to "Finish" and hides back/progress — it isn't waiting for a Next tap on this
            // screen itself the way every other section's "Next" label is.
            NextButtonText = "Finish";
            //IsBackButtonVisible = false;
            IsProgressBarVisible = false;

            var completionField = (field.subFields ?? new List<RegField>()).FirstOrDefault(f => f.Id == "completionText");
            if (completionField is not null)
            {
                FinishText = completionField.Label;
                FinishSubText = completionField.SubLabel;
            }
        }
    }

    /// <summary>Was: the "if (allconsentdetails == null)" block of ShowCurrentStack's
    /// termsstack branch, plus PopulateConsent(). Loads once, matching the original's own
    /// null-guard (re-showing this section doesn't reload it).</summary>
    private async Task LoadConsentAsync()
    {
        try
        {
            if (SignupCodeDetails is null || string.IsNullOrEmpty(SignupCodeDetails.consent)) return;

            var config = JsonConvert.DeserializeObject<ObservableCollection<ConsentDetails>>(SignupCodeDetails.consent);
            if (config is null) return;

            IsUnder10StackVisible = false;

            if (_householdRepFromReg)
            {
                AllConsentDetails = config.FirstOrDefault(x => x.age == "16+");
                if (AllConsentDetails is not null)
                {
                    Over16NameLabel = AllConsentDetails.signoffparameters[0].label;
                    Over16SignatureLabel = AllConsentDetails.signoffparameters[1].label;
                }
            }
            else if (_userInfoForBaseline?.household_individual_age == "16+")
            {
                AllConsentDetails = config.FirstOrDefault(x => x.age == "16+");
                if (AllConsentDetails is not null)
                {
                    Over16NameLabel = AllConsentDetails.signoffparameters[0].label;
                    Over16SignatureLabel = AllConsentDetails.signoffparameters[1].label;
                }
            }
            // else if (_userInfoForBaseline?.household_individual_age == "11 - 15")
            // {
            //     IsUnder10StackVisible = true;
            //     AllConsentDetails = config.FirstOrDefault(x => x.age == "11 - 15");
            //     if (AllConsentDetails is not null)
            //     {
            //         Over16NameLabel = AllConsentDetails.signoffparameters[0].label;
            //         Over16SignatureLabel = AllConsentDetails.signoffparameters[1].label;
            //     }       
            // }
            else
            {

                if (_userInfoForBaseline?.household_individual_age == "16+")
                {
                    AllConsentDetails = config.FirstOrDefault(x => x.age == "16+");
                    if (AllConsentDetails is not null)
                    {
                        Over16NameLabel = AllConsentDetails.signoffparameters[0].label;
                        Over16SignatureLabel = AllConsentDetails.signoffparameters[1].label;
                    }
                }
                // else if (_userInfoForBaseline?.household_individual_age == "11 - 15")
                // {
                //     IsUnder10StackVisible = true;
                //     AllConsentDetails = config.FirstOrDefault(x => x.age == "11 - 15");
                //     if (AllConsentDetails is not null)
                //     {
                //         Over16NameLabel = AllConsentDetails.signoffparameters[0].label;
                //         Over16SignatureLabel = AllConsentDetails.signoffparameters[1].label;
                //     }
                // }
                else if(_userInfoForBaseline?.household_individual_age == "11 - 15"|| _userInfoForBaseline?.household_individual_age == "5 - 10" || _userInfoForBaseline?.household_individual_age == "0 - 5")
                {
                    // 11 - 15, 5 - 10, 0 - 5
                    var ageBands = new[] { "11 - 15", "5 - 10", "0 - 5" };
                    AllConsentDetails = config.FirstOrDefault(x => ageBands.Contains(x.age));
                    if (AllConsentDetails is not null)
                    {
                        var listOfRoles = AllConsentDetails.signoffparameters.FirstOrDefault(x => x.type == "dropdown");

                        Under10NameLabel = AllConsentDetails.signoffparameters[0].label;
                        Over16NameLabel = AllConsentDetails.signoffparameters[1].label;
                        Over16SignatureLabel = AllConsentDetails.signoffparameters[2].label;
                        Under10RoleLabel = AllConsentDetails.signoffparameters[3].label;

                        if (listOfRoles is not null)
                        {
                            Under10RoleOptions = new ObservableCollection<SignoffOption>(listOfRoles.options ?? new List<SignoffOption>());
                        }

                        // Pre-populate child name from household member record and lock it
                        Under10Name = _userInfoForBaseline?.household_individual_name ?? string.Empty;
                        //Lock Editing name of child/young person          
                        IsUnder10NameEditable = false;

                        // Pre-populate main user (parent/guardian) name from device settings and lock it
                        var mainUserFirstName = Helpers.Settings.FirstName?.Trim() ?? string.Empty;
                        var mainUserSurname = Helpers.Settings.Surname?.Trim() ?? string.Empty;
                        Over16Name = string.IsNullOrWhiteSpace(mainUserSurname)
                            ? mainUserFirstName
                            : $"{mainUserFirstName} {mainUserSurname}".Trim();
                        IsOver16NameEditable = true;
                        IsUnder10StackVisible = true;
                    }
                }
            }

            if (AllConsentDetails is not null)
            {
                foreach (var section in AllConsentDetails.consentcontent)
                {
                    if (section.sectioncontent is null) continue;
                    foreach (var item in section.sectioncontent)
                    {
                        if (item.required) item.requiredlbl = "Required";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "LoadConsentAsync");
        }
    }

    private void ClearAllSectionErrors()
    {
        FirstNameError = string.Empty;
        SurnameError = string.Empty;
        EmailError = string.Empty;
        TelephoneError = string.Empty;
        PasswordError = string.Empty;
        ConfirmPasswordError = string.Empty;

        PostcodeError = string.Empty;

        DobHasError = false;
        GenderListError = string.Empty;
        OtherGenderError = string.Empty;
        GenderMatchListError = string.Empty;
        GenderIdentityError = string.Empty;

        BornInUkError = string.Empty;
        MoveToUkError = string.Empty;
        CountryOfOriginError = string.Empty;
        EthnicityListError = string.Empty;

        WeightListError = string.Empty;
        WeightHasError = false;
        HeightListError = string.Empty;
        HeightHasError = false;
        HeightCmHasError = false;
        StepsError = string.Empty;
        GymError = string.Empty;

        Member1.ClearErrors();
        Member2.ClearErrors();

        EducationError = string.Empty;
        SituationError = string.Empty;
        WorkTypeError = string.Empty;

        PeopleError = false;
        RoomsError = false;
        BathroomsError = false;
        VentilationError = string.Empty;
        MouldError = string.Empty;

        NhsNumberError = string.Empty;
        GpRegisteredError = string.Empty;
        GpAddressError = string.Empty;

        CoughError = string.Empty;
        HospitalError = string.Empty;
        InfectionYearError = false;
        VentilatorError = string.Empty;

        HcGateError = string.Empty;
        ConditionAddError = string.Empty;
        OtherHcError = string.Empty;
        BodyPartError = string.Empty;
        OtherHcFreeTextError = string.Empty;
        CancerError = string.Empty;
        CancerNowError = string.Empty;

        MedynGateError = string.Empty;
        MedicationAddError = string.Empty;
        OtherMedError = string.Empty;
        OtherMedDetailsError = string.Empty;

        FluGateError = string.Empty;
        FluDateError = string.Empty;
        CovidDateError = string.Empty;
        RsvDateError = string.Empty;

        DietError = string.Empty;
        DietLengthError = string.Empty;
        SupplementsError = string.Empty;
        TakeError = string.Empty;
        ExtraError = string.Empty;

        MenstrualError = string.Empty;
        PregnancyWeeksError = string.Empty;
        DeliveryDateError = false;

        MobilityError = string.Empty;
        SelfCareError = string.Empty;
        UsualActivitiesError = string.Empty;
        PainError = string.Empty;
        AnxietyDepressionError = string.Empty;
        SliderError = string.Empty;

        AdditionalQError = string.Empty;

        AntiviralHeardError = string.Empty;
        AntiviralPrescribedError = string.Empty;
        AntiviralHospitalError = string.Empty;
        AntiviralDurationError = string.Empty;
        AntiviralPreventionError = string.Empty;
        AntiviralSideEffectsError = string.Empty;

        SmokeError = string.Empty;
        SmokeTypesError = string.Empty;
        SmokeStartAgeError = false;
        SmokeCurrentError = string.Empty;
        SmokeStopAgeError = false;
        SmokeFreqError = string.Empty;

        AlcoholFreqError = string.Empty;
        AlcoholUnitsError = string.Empty;

        DrugsError = string.Empty;
        WhatDrugsError = string.Empty;
        DrugsOftenError = string.Empty;
        BreathingError = string.Empty;

        SleepOnsetError = string.Empty;
        WakeError = string.Empty;
        NightsError = string.Empty;
        QualityError = string.Empty;
        MoodError = string.Empty;
        ProdError = string.Empty;
        PoorSleepError = string.Empty;
        SleepProbError = string.Empty;
        // Extend as more sections are ported.
    }

    private void RefreshProgress()
    {
        OnPropertyChanged(nameof(ProgressMaximum));
        OnPropertyChanged(nameof(ProgressSegmentCount));
        OnPropertyChanged(nameof(ProgressValue));
    }

    private static int CalculateAge(DateTime dob)
    {
        var today = DateTime.Today;
        int age = today.Year - dob.Year;
        if (today < dob.AddYears(age)) age--;
        return age;
    }

    #endregion

    #region Name / Account section
    // Was: ValidateNameStack() / AddParticiantInfo() / firstnameentry_TextChanged /
    // surnameentry_TextChanged / emailentry_TextChanged / firstpasswordentry_TextChanged /
    // confirmpassentry_TextChanged / EmailIsValid() / HashPasswordAsync()

    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _firstNameError = string.Empty;
    [ObservableProperty] private bool _isFirstNameEditable = true;

    [ObservableProperty] private string _surname = string.Empty;
    [ObservableProperty] private string _surnameError = string.Empty;
    [ObservableProperty] private bool _isSurnameEditable = true;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _emailError = string.Empty;
    [ObservableProperty] private bool _isEmailFieldVisible = true;
    [ObservableProperty] private bool _isEmailEditable = true;

    [ObservableProperty] private string _telephone = string.Empty;
    [ObservableProperty] private string _telephoneError = string.Empty;
    [ObservableProperty] private bool _isTelephoneFieldVisible = true;

    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _passwordError = string.Empty;
    [ObservableProperty] private bool _isPasswordFieldVisible = true;

    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string _confirmPasswordError = string.Empty;
    [ObservableProperty] private bool _isConfirmPasswordFieldVisible = true;

    public bool PasswordHasMinLength => Password.Length >= 8;
    public bool PasswordHasSpecialChar => Regex.IsMatch(Password, @"[!@#$%^&*()_+=\[{\]};:<>|./?-]");
    public bool PasswordHasCapital => Regex.IsMatch(Password, "[A-Z]");
    public bool PasswordHasNumber => Regex.IsMatch(Password, "[0-9]");

    private static readonly Regex ValidEmailRegex = new(
        @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$",
        RegexOptions.IgnoreCase);

    // internal (not private): reused by HouseholdMemberEntryViewModel for the mainuserstack
    // section, which needs the exact same email format check ValidateNameSectionAsync uses.
    internal static bool IsEmailValid(string email) => ValidEmailRegex.IsMatch(email);

    partial void OnFirstNameChanged(string value) => FirstNameError = string.Empty;
    partial void OnSurnameChanged(string value) => SurnameError = string.Empty;
    partial void OnEmailChanged(string value) => EmailError = string.Empty;
    partial void OnTelephoneChanged(string value) => TelephoneError = string.Empty;
    partial void OnConfirmPasswordChanged(string value) => ConfirmPasswordError = string.Empty;

    partial void OnPasswordChanged(string value)
    {
        PasswordError = string.Empty;
        OnPropertyChanged(nameof(PasswordHasMinLength));
        OnPropertyChanged(nameof(PasswordHasSpecialChar));
        OnPropertyChanged(nameof(PasswordHasCapital));
        OnPropertyChanged(nameof(PasswordHasNumber));
    }

    private async Task<bool> ValidateNameSectionAsync()
    {
        bool isValid = true;
        FirstName = FirstName?.Trim() ?? string.Empty;
        Surname = Surname?.Trim() ?? string.Empty;
        Email = Email?.Trim() ?? string.Empty;
        Password = Password?.Trim() ?? string.Empty;
        ConfirmPassword = ConfirmPassword?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(FirstName))
            isValid = Fail(() => FirstNameError = "Please enter first name");

        if (string.IsNullOrWhiteSpace(Surname))
            isValid = Fail(() => SurnameError = "Please enter surname");

        if (_householdRepFromReg)
        {
            if (string.IsNullOrEmpty(Telephone))
            {
                isValid = Fail(() => TelephoneError = "Please enter an phone number");
            }
            else if (!Regex.IsMatch(Telephone.Trim(), @"^\+?[0-9\s\-\(\)]{7,15}$"))
            {
                isValid = Fail(() => TelephoneError = "Please enter a valid phone number");
            }

            if (string.IsNullOrEmpty(Email))
            {
                isValid = Fail(() => EmailError = "Please enter an email address");
            }
            else if (!IsEmailValid(Email))
            {
                isValid = Fail(() => EmailError = "Please enter a valid email address");
            }
            else
            {
                var checkUserEmail = await APICalls.Instance.CheckEmailExists(Email);
                if (checkUserEmail?.FirstOrDefault() is not null)
                {
                    await _alertService.DisplayAlertAsync(
                        "Email address already in use",
                        "This email has been registered to an account.",
                        "Ok");
                    isValid = Fail(() => EmailError = "Email already registered");
                }
            }
        }

        if (!_noEmailUserReg)
        {
            if (string.IsNullOrEmpty(Password))
                isValid = Fail(() => PasswordError = "Please enter a password");
            else if (Password.Length < 8)
                isValid = Fail(() => PasswordError = "Password must be greater than 8 characters");
            else if (!PasswordHasCapital)
                isValid = Fail(() => PasswordError = "Password must contain at least one uppercase letter.");
            else if (!PasswordHasNumber)
                isValid = Fail(() => PasswordError = "Password must contain at least one number.");
            else if (!PasswordHasSpecialChar)
                isValid = Fail(() => PasswordError = "Password must contain at least one symbol.");

            if (string.IsNullOrEmpty(ConfirmPassword))
                isValid = Fail(() => ConfirmPasswordError = "Please confirm your password");
            else if (Password != ConfirmPassword)
                isValid = Fail(() => ConfirmPasswordError = "Passwords do not match");
        }

        return isValid;

        bool Fail(Action setError)
        {
            setError();
            Vibration.Vibrate(); // matches ShowError()'s per-field vibrate in the original
            return false;
        }
    }

    private async Task AddNameSectionInfoAsync()
    {
        NewUser.firstname = FirstName.Trim();
        NewUser.surname = Surname.Trim();
        NewUser.email = Email.Trim();
        NewUser.telephone = Telephone.Trim();
        NewUser.password = await HashPasswordAsync(ConfirmPassword);
    }

    private static async Task<string?> HashPasswordAsync(string password)
    {
        // Carried over exactly as in the original (MD5). Flagging in chat, not changing here —
        // switching algorithms would break compatibility with whatever the backend expects for
        // already-stored hashes unless that's coordinated on the server side too.
        return await Task.Run(() =>
        {
            try
            {
                using var md5 = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                var sb = new System.Text.StringBuilder();
                foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        });
    }

    #endregion

    #region Address section
    // Was: ValidateaddressStack() / AddAddressInfo() / postcodeentry_TextChanged /
    // HandlePostcodeLookupAsync() / LookupPostcode() / postcodelist_ItemTapped /
    // ClearAddressbtn_Clicked() / addressoneentry_TextChanged / townentry_TextChanged /
    // countyentry_TextChanged

    private static readonly Regex UkPostcodeRegex = new(
        @"^(GIR\s?0AA|(?:(?:[A-PR-UWYZ][0-9][0-9]?|[A-PR-UWYZ][A-HK-Y][0-9][0-9]?|[A-PR-UWYZ][0-9][A-HJKPSTUW]|[A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRV-Y]))\s?[0-9][ABD-HJLNP-UW-Z]{2})$",
        RegexOptions.IgnoreCase);
    private static readonly HttpClient PostcodeClient = new();
    private CancellationTokenSource? _postcodeLookupCts;
    private bool _isEditingPostcode;
    private readonly ObservableCollection<OptionDetails> _addressDetailAnswers = new(); // was: UserDetails

    /// <summary>Populated server-side per study; empty means "no area restriction". Was: validpostcodelist.</summary>
    public List<string> ValidPostcodeList { get; set; } = new();

    [ObservableProperty] private string _postcode = string.Empty;
    [ObservableProperty] private int _postcodeCursorPosition;
    [ObservableProperty] private string _postcodeError = string.Empty;
    [ObservableProperty] private string _addresslineoneError = string.Empty;
    [ObservableProperty] private bool _isPostcodeNoResultsVisible;
    [ObservableProperty] private ObservableCollection<IdealAddress> _addressResults = new();
    [ObservableProperty] private bool _isAddressResultsVisible;
    [ObservableProperty] private IdealAddress? _selectedAddress;
    [ObservableProperty] private bool _isClearAddressButtonVisible;
    [ObservableProperty] private bool _isAddressFieldsVisible;
    [ObservableProperty] private string _addressLine1 = string.Empty;
    [ObservableProperty] private string _town = string.Empty;
    [ObservableProperty] private string _county = string.Empty;

    [RelayCommand]
    private async Task CheckPostcodeAsync()
    {
        _postcodeLookupCts?.Cancel();
        var cts = new CancellationTokenSource();
        _postcodeLookupCts = cts;
        try
        {
           // IsPostcodeCheckBusy = true;
            PostcodeError = string.Empty;
            IsPostcodeNoResultsVisible = false;
            IsClearAddressButtonVisible = false;
            AddressResults?.Clear();
            IsAddressResultsVisible = false;
            IsAddressFieldsVisible = false;

            string cleaned = Postcode?.Trim().ToUpper() ?? string.Empty;
            if (!UkPostcodeRegex.IsMatch(cleaned))
            {
                PostcodeError = "Enter a valid UK postcode";
                return;
            }

            var addresses = await LookupPostcodeAsync(cleaned, cts.Token);
            if (cts.IsCancellationRequested) return;

            if (addresses is null || addresses.Count == 0)
            {
                IsPostcodeNoResultsVisible = true;
            }
            else
            {
                AddressResults = new ObservableCollection<IdealAddress>(addresses);
                IsAddressResultsVisible = true;
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "CheckPostcodeAsync");
        }
        finally
        {
           // IsPostcodeCheckBusy = false;
            if (ReferenceEquals(_postcodeLookupCts, cts)) _postcodeLookupCts = null;
            cts.Dispose();
        }
    }

    //    partial void OnPostcodeChanged(string value)
    //    {
    //        if (IsClearAddressButtonVisible) IsClearAddressButtonVisible = false;
    //        if (_isEditingPostcode) return;
    //        _isEditingPostcode = true;
    //        try
    //        {
    //            IsPostcodeNoResultsVisible = false;

    //            // Clean and format input string
    //            string input = new string((value ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpper();
    //            if (input.Length > 7) input = input[..7];
    //            if (input.Length >= 5) input = input.Insert(input.Length - 3, " ");
    //            PostcodeError = string.Empty;

    //            // Defer the write-back so we don't mutate the bound Entry/UITextField
    //            // synchronously from inside its own text-changed callback.
    //            MainThread.BeginInvokeOnMainThread(() =>
    //            {
    //                try
    //                {
    //                    // 1. Update Cursor Position FIRST to avoid Android native out-of-bounds crash
    //                    PostcodeCursorPosition = input.Length;
    //                    // 2. Update the bound property
    //                    Postcode = input;
    //#if ANDROID
    //                    try
    //                    {
    //                        if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.CurrentFocus is AndroidX.AppCompat.Widget.AppCompatEditText editText)
    //                        {
    //                            editText.EmojiCompatEnabled = false;
    //                            editText.SetTextKeepState(input);
    //                        }
    //                    }
    //                    catch (Exception androidEx)
    //                    {
    //                        CrashDetected.LogCrash(androidEx, "OnPostcodeChanged.AndroidNativeFix");
    //                    }
    //#endif
    //                }
    //                catch (Exception mainThreadEx)
    //                {
    //                    CrashDetected.LogCrash(mainThreadEx, "OnPostcodeChanged.MainThreadUpdate");
    //                }
    //                finally
    //                {
    //                    // Reset the guard only after the deferred write actually completes,
    //                    // so a keystroke that lands while this callback is still queued
    //                    // doesn't re-enter and race the pending update.
    //                    _isEditingPostcode = false;
    //                }
    //            });

    //            _ = HandlePostcodeLookupAsync(input);
    //        }
    //        catch (Exception ex)
    //        {
    //            CrashDetected.LogCrash(ex, "OnPostcodeChanged");
    //            _isEditingPostcode = false;
    //        }
    //    }

    /// <summary>Was: postcodelist_ItemTapped. NOTE: the original nulls postcodelist.SelectedItem
    /// right after populating the fields, but ValidateaddressStack then checks that same
    /// SelectedItem to confirm an address was chosen — which would always fail if it's really
    /// cleared. That looked like it might not do what was intended, so here the selection is
    /// simply left set (matching what the Next-button validation appears to actually need);
    /// flagging this in chat too.</summary>
    partial void OnSelectedAddressChanged(IdealAddress? value)
    {
        if (value is null) return;

        AddressLine1 = value.line_1;
        Town = !string.IsNullOrEmpty(value.line_2) ? value.line_2 : value.post_town;
        County = value.County;
        IsAddressFieldsVisible = true;
        IsAddressResultsVisible = false;
        IsClearAddressButtonVisible = true;
        PostcodeError = string.Empty;
        AddresslineoneError = string.Empty; 
    }

    partial void OnAddressLine1Changed(string value) { AddresslineoneError = string.Empty; }
    partial void OnTownChanged(string value) { }
    partial void OnCountyChanged(string value) { }

    [RelayCommand]
    private void ClearAddress()
    {
        IsAddressFieldsVisible = false;
        AddressLine1 = string.Empty;
        Town = string.Empty;
        County = string.Empty;
        IsAddressResultsVisible = true;
        IsClearAddressButtonVisible = false;
        SelectedAddress = null;
        PostcodeError = string.Empty;
        AddresslineoneError = string.Empty; 
    }

    //private async Task HandlePostcodeLookupAsync(string formattedPostcode)
    //{
    //    _postcodeLookupCts?.Cancel();
    //    var cts = new CancellationTokenSource();
    //    _postcodeLookupCts = cts;
    //    try
    //    {
    //        await Task.Delay(250, cts.Token);

    //        if (UkPostcodeRegex.IsMatch(formattedPostcode))
    //        {
    //            var addresses = await LookupPostcodeAsync(formattedPostcode, cts.Token);
    //            if (cts.IsCancellationRequested || Postcode != formattedPostcode) return;

    //            MainThread.BeginInvokeOnMainThread(() =>
    //            {
    //                if (cts.IsCancellationRequested || Postcode != formattedPostcode) return;

    //                if (addresses is null || addresses.Count == 0)
    //                {
    //                    IsPostcodeNoResultsVisible = true;
    //                    IsAddressResultsVisible = false;
    //                    AddressResults?.Clear();
    //                    IsAddressFieldsVisible = false;
    //                }
    //                else
    //                {
    //                    IsPostcodeNoResultsVisible = false;
    //                    AddressResults = new ObservableCollection<IdealAddress>(addresses);
    //                    IsAddressResultsVisible = true;
    //                }
    //            });
    //        }
    //        else
    //        {
    //            MainThread.BeginInvokeOnMainThread(() =>
    //            {
    //                AddressResults?.Clear();
    //                IsPostcodeNoResultsVisible = formattedPostcode.Length >= 7;
    //                IsAddressResultsVisible = false;
    //                IsAddressFieldsVisible = false;
    //            });
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        CrashDetected.LogCrash(ex, "HandlePostcodeLookupAsync");
    //    }
    //    finally
    //    {
    //        if (ReferenceEquals(_postcodeLookupCts, cts)) _postcodeLookupCts = null;
    //        cts.Dispose();
    //    }
    //}

    public async Task<List<IdealAddress>?> LookupPostcodeAsync(string postcode, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            const string apiKey = "ak_mnh4f02ypPRnIXhiTBlDzYkMDFMU5";
            string encodedPostcode = Uri.EscapeDataString(postcode);
            string url = $"https://api.ideal-postcodes.co.uk/v1/postcodes/{encodedPostcode}?api_key={apiKey}";

            Task.Delay(50);
            var response = await PostcodeClient.GetAsync(url, linkedCts.Token);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<IdealResponse>(json);
                return data?.result ?? new List<IdealAddress>();
            }
            return new List<IdealAddress>();
        }
        catch (OperationCanceledException)
        {
            return null; 
        }
        catch (HttpRequestException) when (linkedCts.IsCancellationRequested)
        {
            return null;
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "LookupPostcodeAsync");
            return null;
        }
    }

    private async Task<bool> ValidateAddressSectionAsync()
    {
        bool isValid = true;
        PostcodeError = string.Empty;
        AddresslineoneError = string.Empty;
        string rawPostcode = Postcode?.Trim() ?? string.Empty;
        string rawAddressLone = AddressLine1?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(rawPostcode) || !UkPostcodeRegex.IsMatch(rawPostcode))
        {
            PostcodeError = "Enter a valid UK postcode";
            isValid = false;
        }

        if (isValid && ValidPostcodeList.Count > 0)
        {
            var cleanPostcode = rawPostcode.ToUpper().Replace(" ", "");
            var outwardCode = cleanPostcode.Length > 3 ? cleanPostcode[..^3] : cleanPostcode;
            if (!ValidPostcodeList.Any(p => p.Equals(outwardCode, StringComparison.OrdinalIgnoreCase)))
            {
                PostcodeError = "Sorry, this study is not available in your area";
                isValid = false;
            }
        }

        if (isValid)
        {
            if (SelectedAddress is not null)
            {
                var checkPostcode = await APICalls.Instance.Getuserspostcodes(rawPostcode);
                if (checkPostcode is not null && checkPostcode.Count > 0)
                {
                    bool matchCase = checkPostcode.Any(x => x.DetailsList != null &&
                        x.DetailsList.Any(d => string.Equals(
                            d.addresslineone?.Trim(), SelectedAddress.line_1?.Trim(),
                            StringComparison.OrdinalIgnoreCase)));
                    if (matchCase)
                    {
                        PostcodeError = "An account with this address already exists";
                        isValid = false;
                    }
                }
            }
            else
            {
                PostcodeError = "Please select an address shown below";
                return false;
            }
        }

        if (string.IsNullOrWhiteSpace(rawAddressLone))
        {
            AddresslineoneError = "Address line one cannot be empty";
            return false;
        }

        return isValid;
    }

    private Task AddAddressSectionInfoAsync()
    {
        NewUser.postcode = Postcode?.Trim();
        _addressDetailAnswers.Clear();
        AddDetail("addresslineone", AddressLine1?.Trim());
        AddDetail("town", Town?.Trim());
        AddDetail("County", County?.Trim());
        AddDetail("devicemanufacturer", DeviceInfo.Manufacturer);
        AddDetail("devicemodel", DeviceInfo.Model);
        AddDetail("deviceversion", DeviceInfo.VersionString);

        var newDetails = _addressDetailAnswers.ToDictionary(x => x.Text, x => x.Value);
        NewUser.details = JsonConvert.SerializeObject(new List<object> { newDetails });
        return Task.CompletedTask;
    }

    private void AddDetail(string text, string? value) =>
        _addressDetailAnswers.Add(new OptionDetails { Text = text, Value = value?.Trim() });

    #endregion

    #region Gender section
    // Was: ValidateGenderStack() / AddGenderInfo() / genderlist_ItemTapped /
    // gendermatchlist_ItemTapped / genidlist_ItemTapped / dateEntry_TextChanged
    // (ValidatedobStack() / the `validdob` field were dead code in the original — the only
    // place that ever set `validdob = true` was commented out, so that method could never
    // pass. Not carried over.)

    [ObservableProperty] private string _dobLabel = string.Empty;
    [ObservableProperty] private string _dobHelpText = string.Empty;
    [ObservableProperty] private string _dateOfBirthText = string.Empty;
    [ObservableProperty] private string _dobError = string.Empty;
    [ObservableProperty] private bool _dobHasError;

    [ObservableProperty] private string _sexLabel = string.Empty;
    [ObservableProperty] private string _sexSubLabel = string.Empty;
    [ObservableProperty] private string _sexHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _genderOptions = new();
    [ObservableProperty] private OptionDetails? _selectedGenderOption;
    [ObservableProperty] private string _genderListError = string.Empty;

    [ObservableProperty] private bool _isOtherGenderVisible;
    [ObservableProperty] private string _otherGenderText = string.Empty;
    [ObservableProperty] private string _otherGenderError = string.Empty;

    [ObservableProperty] private bool _isGenderMatchVisible;
    [ObservableProperty] private string _genderMatchLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _genderMatchOptions = new();
    [ObservableProperty] private OptionDetails? _selectedGenderMatchOption;
    [ObservableProperty] private string _genderMatchListError = string.Empty;

    [ObservableProperty] private bool _isGenderIdentityVisible;
    [ObservableProperty] private string _genderIdentityLabel = string.Empty;
    [ObservableProperty] private string _genderIdentityHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _genderIdentityOptions = new();
    [ObservableProperty] private OptionDetails? _selectedGenderIdentityOption;
    [ObservableProperty] private string _genderIdentityError = string.Empty;

    partial void OnDateOfBirthTextChanged(string value)
    {
        DobHasError = false;

        if (DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
        {
            IsGenderMatchVisible = CalculateAge(dob) > 13;
            // Original left the gender-identity visibility alone on a valid date (that branch
            // was commented out) — only the invalid-date path below ever hides it live.
        }
        else
        {
            SelectedGenderIdentityOption = null;
            SelectedGenderMatchOption = null;
            IsGenderMatchVisible = false;
            IsGenderIdentityVisible = false;
        }
    }

    partial void OnSelectedGenderOptionChanged(OptionDetails? value)
    {
        if (value is null) return;

        IsOtherGenderVisible = value.Text == "Other";
        if (!IsOtherGenderVisible && UserDetails is not null)
        {
            UserDetails.Gender = value.Text;
        }
        OtherGenderError = string.Empty;
        GenderListError = string.Empty;
        GenderIdentityError = string.Empty;
    }

    partial void OnOtherGenderTextChanged(string value) => OtherGenderError = string.Empty;

    partial void OnSelectedGenderMatchOptionChanged(OptionDetails? value)
    {
        if (value is null) return;

        IsGenderIdentityVisible = value.Text == "No";
        GenderIdentityError = string.Empty;
        GenderMatchListError = string.Empty;

        var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "genderMatch");
        if (record is not null) record.AnswerId = value.AnswerId;
    }

    partial void OnSelectedGenderIdentityOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        GenderIdentityError = string.Empty;

        var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "genderidentity");
        if (record is not null) record.AnswerId = value.AnswerId;
    }

    private bool ValidateGenderSection()
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(DateOfBirthText))
        {
            DobError = "Please enter a valid date of birth";
            DobHasError = true;
            isValid = false;
        }
        else if (!DateTime.TryParseExact(DateOfBirthText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
        {
            DobError = "Please enter a valid date of birth";
            DobHasError = true;
            isValid = false;
        }
        else if (dob > DateTime.Today)
        {
            DobError = "Date of birth cannot be in the future";
            DobHasError = true;
            isValid = false;
        }
        else
        {
            var age = DateTime.Today.Year - dob.Year;

            if (dob.Date > DateTime.Today.AddYears(-age))
            {
                age--;
            }

            //Ensure the primary user is older than 16
            if (_householdRepFromReg && age < 16)
            {
                DobError = "Household Representative must be older than 16";
                DobHasError = true;
                isValid = false;
            }
            else if (age > 120)
            {
                DobError = "Age cannot be greater than 120";
                DobHasError = true;
                isValid = false;
            }
            else if (UserDetails is not null)
            {
                UserDetails.Age = DateOfBirthText;
            }
        }

        if (SelectedGenderOption is null)
        {
            GenderListError = "Select an option";
            isValid = false;
        }

        if (IsGenderMatchVisible && SelectedGenderMatchOption is null)
        {
            GenderMatchListError = "Select an option";
            isValid = false;
        }

        if (IsOtherGenderVisible)
        {
            if (string.IsNullOrEmpty(OtherGenderText))
            {
                OtherGenderError = "Please enter gender";
                isValid = false;
            }
            else if (UserDetails is not null)
            {
                UserDetails.Gender = OtherGenderText;
            }
        }

        if (IsGenderIdentityVisible && SelectedGenderIdentityOption is null)
        {
            GenderIdentityError = "Select an option";
            isValid = false;
        }

        return isValid;
    }

    private Task AddGenderSectionInfoAsync()
    {
        NewUser.dateofbirth = DateOfBirthText?.Trim();

        if (SelectedGenderOption is not null)
        {
            // NOTE: ported exactly as AddGenderInfo() had it — this always uses the picked
            // option's Text, so if "Other" was chosen, newuser.gender ends up as literally
            // "Other" rather than OtherGenderText/UserDetails.Gender (which DO get the custom
            // value). Flagging this in chat in case it wasn't intentional in the original.
            NewUser.gender = SelectedGenderOption.Text;
        }

        if (DateTime.TryParseExact(DateOfBirthText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
        {
            int age = CalculateAge(dob);

            if (age >= 15 && age <= 55 && NewUser.gender == "Female")
            {
                var getMenstrual = RegistrationSectionsNotRequired.FirstOrDefault(x => x.XamlNameArea == "menstrualstack");
                if (getMenstrual is not null && !RegistrationSections.Any(x => x.XamlNameArea == "menstrualstack"))
                {
                    int targetIndex = int.TryParse(getMenstrual.Order, out var o) ? o : RegistrationSections.Count;
                    int safeIndex = Math.Min(targetIndex, RegistrationSections.Count);
                    RegistrationSections.Insert(safeIndex, getMenstrual);
                }
            }
            else
            {
                var getMenstrual = RegistrationSections.FirstOrDefault(x => x.XamlNameArea == "menstrualstack");
                if (getMenstrual is not null) RegistrationSections.Remove(getMenstrual);
            }

            if(age >= 16)
            {
                //Add Back in Additional Sections for over16main
                if(_over16MainSections.Count > 0)
                {
                    foreach (var item in _over16MainSections)
                    {
                        if (!RegistrationSections.Contains(item)) RegistrationSections.Add(item);
                    }
                }
            }


            // if (age < 16)
            // {
            //     var over16Main = RegistrationSections.Where(x => x.Type == "over16main").ToList();
            //     _over16MainSections = over16Main;
            //     foreach (var item in over16Main) RegistrationSections.Remove(item);
            // }
            // else if (_over16MainSections.Count > 0)
            // {
            //     foreach (var item in _over16MainSections)
            //     {
            //         if (!RegistrationSections.Contains(item)) RegistrationSections.Add(item);
            //     }
            // }

            RefreshProgress();
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Ethnicity section
    // Was: ValidateEthnicityStack() / AddEthnicityInfo() / uklist_ItemTapped /
    // movelist_ItemTapped / autocompletecounty_SelectionChanged / ethnicitylist_ItemTapped

    [ObservableProperty] private string _ukLabel = string.Empty;
    [ObservableProperty] private string _ukHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _bornInUkOptions = new();
    [ObservableProperty] private OptionDetails? _selectedBornInUkOption;
    [ObservableProperty] private string _bornInUkError = string.Empty;

    [ObservableProperty] private bool _isMoveToUkVisible;
    [ObservableProperty] private string _moveToUkLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _moveToUkOptions = new();
    [ObservableProperty] private OptionDetails? _selectedMoveToUkOption;
    // NOTE: the original's `moveerror` label defaulted to IsVisible="true" directly in XAML —
    // every other error label in the file defaults to False and only this one differed (even
    // in casing: "true" vs "False" elsewhere), which reads as a typo rather than an intentional
    // "show an error before the user has done anything" design. Defaulting hidden here, like
    // every sibling error message, rather than reproducing what looked like a mistake.
    [ObservableProperty] private string _moveToUkError = string.Empty;

    [ObservableProperty] private bool _isCountryOfOriginVisible;
    [ObservableProperty] private string _countryOfOriginLabel = string.Empty;
      [ObservableProperty] private string _countryOfOriginSubLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _countryOfOriginOptions = new();
    [ObservableProperty] private OptionDetails? _selectedCountryOfOriginOption;
    [ObservableProperty] private string _countryOfOriginError = string.Empty;

    [ObservableProperty] private string _ethnicityLabel = string.Empty;
    [ObservableProperty] private string _ethnicitySubLabel = string.Empty;
    [ObservableProperty] private string _ethnicityHelpText = string.Empty;
    [ObservableProperty] private string _ethnicitySelectHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _ethnicityOptions = new();
    [ObservableProperty] private OptionDetails? _selectedEthnicityOption;
    [ObservableProperty] private string _ethnicityListError = string.Empty;

    [ObservableProperty] private string _ethnicityOwnWordsLabel = string.Empty;
    [ObservableProperty] private string _ethnicityOwnWordsText = string.Empty;

    partial void OnSelectedBornInUkOptionChanged(OptionDetails? value)
    {
        if (value is null) return;

        BornInUkError = string.Empty;
        CountryOfOriginError = string.Empty;

        bool notBornInUk = value.Text == "No";
        IsMoveToUkVisible = notBornInUk;
        IsCountryOfOriginVisible = notBornInUk;
        if (!notBornInUk)
        {
            SelectedMoveToUkOption = null;
            MoveToUkError = string.Empty;
            SelectedCountryOfOriginOption = null;
        }

        var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "bornInUK");
        if (record is not null) record.AnswerId = value.AnswerId;
    }

    partial void OnSelectedMoveToUkOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        MoveToUkError = string.Empty;

        var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "moveToUKAge");
        if (record is not null) record.AnswerId = value.AnswerId;
    }

    partial void OnSelectedCountryOfOriginOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        CountryOfOriginError = string.Empty;

        var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "countryOfOrigin");
        if (record is not null) record.AnswerId = value.AnswerId;
    }

    partial void OnSelectedEthnicityOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        EthnicityListError = string.Empty;
        if (UserDetails is not null) UserDetails.Ethnicity = value.Text;
    }

    private bool ValidateEthnicitySection()
    {
        bool isValid = true;

        if (SelectedBornInUkOption is null)
        {
            BornInUkError = "Select an option";
            isValid = false;
        }

        if (IsMoveToUkVisible)
        {
            if (SelectedMoveToUkOption is null)
            {
                MoveToUkError = "Select an option";
                isValid = false;
            }

            //Not required option
            // if (SelectedCountryOfOriginOption is null)
            // {
            //     CountryOfOriginError = "Select an option";
            //     isValid = false;
            // }
        }

        if (SelectedEthnicityOption is null)
        {
            EthnicityListError = "Select an option";
            isValid = false;
        }

        return isValid;
    }

    private Task AddEthnicitySectionInfoAsync()
    {
        if (SelectedEthnicityOption is not null)
        {
            NewUser.ethnicity = SelectedEthnicityOption.Text;
        }

        if (!string.IsNullOrEmpty(EthnicityOwnWordsText))
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "ethnicityInOwnWords");
            if (record is not null) record.AnswerValue = EthnicityOwnWordsText;
        }

        // bornInUK / moveToUKAge / countryOfOrigin QuestionnaireResults entries are kept
        // current by the OnSelected*Changed handlers above as the user picks each one.
        // AddEthnicityInfo() in the original re-reads and re-writes the same three values
        // again here, which is redundant with what selection-time already wrote — not
        // repeated, since it can't produce a different result.

        return Task.CompletedTask;
    }

    #endregion

    #region Body Metrics section
    // Was: ValidatebodymetricsStack() / AddBodyMetricsInfo() / weightinputlist_ItemTapped /
    // heightinputlist_ItemTapped / weightEntry_TextChanged / inchesEntry_TextChanged (shared
    // by both feetEntry and inchesEntry in the original — same here) / heightcmentry_TextChanged /
    // stepslist_ItemTapped / gymlist_ItemTapped

    [ObservableProperty] private string _weightInputLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _weightUnitOptions = new();
    [ObservableProperty] private OptionDetails? _selectedWeightUnitOption;
    [ObservableProperty] private string _weightListError = string.Empty;

    [ObservableProperty] private bool _isWeightEntryVisible;
    [ObservableProperty] private string _weightErrorText = string.Empty;
    [ObservableProperty] private string _weightUnitSuffix = string.Empty; // "kg" or "st"
    [ObservableProperty] private string _weightText = string.Empty;
    [ObservableProperty] private bool _weightHasError;

    [ObservableProperty] private string _heightInputLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _heightUnitOptions = new();
    [ObservableProperty] private OptionDetails? _selectedHeightUnitOption;
    [ObservableProperty] private string _heightListError = string.Empty;

    [ObservableProperty] private bool _isFeetInchesVisible;
    [ObservableProperty] private string _feetText = string.Empty;
    [ObservableProperty] private string _inchesText = string.Empty;
    [ObservableProperty] private bool _heightHasError;
    [ObservableProperty] private string _heightErrorText = string.Empty;

    [ObservableProperty] private bool _isHeightCmVisible;
    [ObservableProperty] private string _heightCmText = string.Empty;
    [ObservableProperty] private bool _heightCmHasError;
    [ObservableProperty] private string _heightCmErrorText = string.Empty;

    [ObservableProperty] private bool _isStepsVisible;
    [ObservableProperty] private string _stepsLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _stepsOptions = new();
    [ObservableProperty] private OptionDetails? _selectedStepsOption;
    [ObservableProperty] private string _stepsError = string.Empty;

    [ObservableProperty] private bool _isGymVisible;
    [ObservableProperty] private string _gymLabel = string.Empty;
    [ObservableProperty] private string _gymHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _gymOptions = new();
    [ObservableProperty] private OptionDetails? _selectedGymOption;
    [ObservableProperty] private string _gymError = string.Empty;

    partial void OnSelectedWeightUnitOptionChanged(OptionDetails? value)
    {
        if (value is null) return;

        if (value.Text.Contains("kg"))
        {
            IsWeightEntryVisible = true;
            WeightUnitSuffix = "kg";
        }
        else if (value.Text.Contains("st"))
        {
            IsWeightEntryVisible = true;
            WeightUnitSuffix = "st";
        }
        else
        {
            IsWeightEntryVisible = false;
        }

        WeightHasError = false;
        WeightListError = string.Empty;
    }

    partial void OnSelectedHeightUnitOptionChanged(OptionDetails? value)
    {
        if (value is null) return;

        if (value.Text.Contains("cm"))
        {
            IsFeetInchesVisible = false;
            IsHeightCmVisible = true;
        }
        else if (value.Text.Contains("Feet"))
        {
            IsFeetInchesVisible = true;
            IsHeightCmVisible = false;
        }
        else
        {
            IsFeetInchesVisible = false;
            IsHeightCmVisible = false;
        }

        HeightHasError = false;
        HeightCmHasError = false;
        HeightListError = string.Empty;
    }

    partial void OnWeightTextChanged(string value) => WeightHasError = false;
    partial void OnFeetTextChanged(string value) => HeightHasError = false;
    partial void OnInchesTextChanged(string value) => HeightHasError = false;
    partial void OnHeightCmTextChanged(string value) => HeightCmHasError = false;
    partial void OnSelectedStepsOptionChanged(OptionDetails? value) { if (value is not null) StepsError = string.Empty; }
    partial void OnSelectedGymOptionChanged(OptionDetails? value) { if (value is not null) GymError = string.Empty; }

    private bool ValidateBodyMetricsSection()
    {
        bool isValid = true;

        if (SelectedWeightUnitOption is null)
        {
            WeightListError = "Select an option";
            isValid = false;
        }

        if (IsWeightEntryVisible && string.IsNullOrEmpty(WeightText))
        {
            WeightErrorText = LocalizationManager.Get("Register_EnterValue");
            WeightHasError = true;
            isValid = false;
        }
        else if (IsWeightEntryVisible)
        {
            if (decimal.TryParse(WeightText, out decimal weight))
            {
                if (weight <= 0)
                {
                    WeightErrorText = LocalizationManager.Get("Register_InvalidWeight");
                    WeightHasError = true;
                    isValid = false;
                }
                if(WeightUnitSuffix == "kg")
                {
                    if (weight > 500)
                    {
                        WeightErrorText = LocalizationManager.Get("Register_InvalidWeight");
                        WeightHasError = true;
                        isValid = false;
                    }
                }
                if(WeightUnitSuffix == "st")
                {
                    if (weight > 100)
                    {
                        WeightErrorText = LocalizationManager.Get("Register_InvalidWeight");
                        WeightHasError = true;
                        isValid = false;
                    }
                }
            }
        }

        if (SelectedHeightUnitOption is null)
        {
            HeightListError = "Select an option";
            isValid = false;
        }

        if (IsFeetInchesVisible)
{
    int feet = 0;
    int inches = 0;

    if (string.IsNullOrEmpty(FeetText))
    {
        HeightErrorText = LocalizationManager.Get("Register_EnterValue");
        HeightHasError = true;
        isValid = false;
    }
    else
    {
        if (int.TryParse(FeetText, out feet))
        {
            if (feet < 0 || feet > 8)
            {
                HeightErrorText = LocalizationManager.Get("Register_InvalidHeight");
                HeightHasError = true;
                isValid = false;
            }
        }
    }

    if (string.IsNullOrEmpty(InchesText))
    {
        HeightErrorText = LocalizationManager.Get("Register_EnterValue");
        HeightHasError = true;
        isValid = false;
    }
    else
    {
        if (int.TryParse(InchesText, out inches))
        {
            if (inches < 0 || inches >= 12)
            {
                HeightErrorText = LocalizationManager.Get("Register_InvalidHeight");
                HeightHasError = true;
                isValid = false;
            }
        }
    }

    // at least 1 foor or 1 inch
    if (isValid && feet == 0 && inches == 0)
    {
        HeightErrorText = LocalizationManager.Get("Register_InvalidHeight");
        HeightHasError = true;
        isValid = false;
    }
}

        if (IsHeightCmVisible && string.IsNullOrEmpty(HeightCmText))
        {
            HeightCmErrorText = LocalizationManager.Get("Register_EnterValue");
            HeightCmHasError = true;
            isValid = false;
        }
        else
        {
            if(int.TryParse(HeightCmText, out int heightCm))
                {
                    if (heightCm <= 0 || heightCm > 250)
                    {
                        HeightCmErrorText = LocalizationManager.Get("Register_InvalidHeight");
                        HeightCmHasError = true;
                        isValid = false;
                    }
                }
        }

        if (IsStepsVisible && SelectedStepsOption is null)
        {
            StepsError = "Select an option";
            isValid = false;
        }

        if (IsGymVisible && SelectedGymOption is null)
        {
            GymError = "Select an option";
            isValid = false;
        }

        return isValid;
    }

    private Task AddBodyMetricsSectionInfoAsync()
    {
        if (SelectedWeightUnitOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "weightUnit");
            if (record is not null)
            {
                record.AnswerId = SelectedWeightUnitOption.AnswerId;
                if (IsWeightEntryVisible) record.AnswerValue = WeightText?.Trim();
            }
        }

        if (SelectedHeightUnitOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "heightUnit");
            if (record is not null)
            {
                record.AnswerId = SelectedHeightUnitOption.AnswerId;
                if (IsFeetInchesVisible)
                {
                    record.AnswerValue = (FeetText?.Trim() ?? string.Empty) + (InchesText?.Trim() ?? string.Empty);
                }
                else if (IsHeightCmVisible)
                {
                    record.AnswerValue = HeightCmText?.Trim();
                }
            }
        }

        if (IsStepsVisible && SelectedStepsOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "averageSteps");
            if (record is not null) record.AnswerId = SelectedStepsOption.AnswerId;
        }

        if (IsGymVisible && SelectedGymOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "muscleMass");
            if (record is not null) record.AnswerId = SelectedGymOption.AnswerId;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region mainuserstack — add up to two household members
    // Was: ValidateFormStack() (per-member halves now live in HouseholdMemberEntryViewModel.
    // Validate(); the cross-member "emails must differ" check stays here since it needs both).
    //
    // Unlike every other section, the original does NOT commit this section's data in an
    // AddXInfo step — nextbtn_Clicked's mainuserstack branch has that call commented out
    // ("//Not Needed Here / //Addhouseholdmembers();"). The actual household-member records
    // get built later, in Addhouseholdmembers() / Updatehouseholdmembers(), which read the
    // raw entry values directly and are only called from CreateAccount() at final submission —
    // so they're ported alongside SubmitAsync once every section exists to submit.
    //
    // Also not carried over: the usinglist / studyreplist / relationlist / otherrelationentry
    // controls that follow both members in the original XAML. ShowCurrentStack's population
    // for them and ValidateFormStack's checks on them were both entirely commented out, so
    // they're declared but permanently inert in the live app today — same situation as
    // namestack's dead usinglist/relationlist/studyreplist noted earlier.

    private bool ValidateMainUserSection()
    {
        bool member1Valid = Member1.Validate();
        bool member2Valid = Member2.Validate();
        bool isValid = member1Valid && member2Valid;

        if (Member1.IsEmailSectionVisible && Member2.IsEmailSectionVisible
            && !string.IsNullOrEmpty(Member1.Email) && !string.IsNullOrEmpty(Member2.Email)
            && Member1.Email == Member2.Email)
        {
            Member2.EmailError = "Each family member requires a unique email";
            isValid = false;
        }

        return isValid;
    }

    private Task AddMainUserSectionInfoAsync() => Task.CompletedTask;

    #endregion

    #region Education / Work section
    // Was: ValidateeducationStack() / AddEducationWorkInfo() / higheducationlist_ItemTapped /
    // currentsituationlist_ItemTapped / typeworklist_ItemTapped

    [ObservableProperty] private string _highestEducationLabel = string.Empty;
    [ObservableProperty] private string _highestEducationHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _highestEducationOptions = new();
    [ObservableProperty] private OptionDetails? _selectedHighestEducationOption;
    [ObservableProperty] private string _educationError = string.Empty;

    [ObservableProperty] private string _currentSituationLabel = string.Empty;
    [ObservableProperty] private string _currentSituationHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _currentSituationOptions = new();
    [ObservableProperty] private OptionDetails? _selectedCurrentSituationOption;
    [ObservableProperty] private string _situationError = string.Empty;

    [ObservableProperty] private bool _isWorkTypeVisible;
    [ObservableProperty] private string _workTypeLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _workTypeOptions = new();
    [ObservableProperty] private OptionDetails? _selectedWorkTypeOption;
    [ObservableProperty] private string _workTypeError = string.Empty;

    partial void OnSelectedHighestEducationOptionChanged(OptionDetails? value)
    {
        if (value is not null) EducationError = string.Empty;
    }

    partial void OnSelectedCurrentSituationOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        SituationError = string.Empty;

        // Original: typeworklbl.IsVisible = true only for these four specific situations.
        IsWorkTypeVisible = value.Text.Contains("Full-time employed")
            || value.Text.Contains("Part-time employed")
            || value.Text.Contains("Doing unpaid or voluntary work")
            || value.Text.Contains("Homemaker");

        if (!IsWorkTypeVisible)
        {
            SelectedWorkTypeOption = null;
            WorkTypeError = string.Empty;
        }
    }

    partial void OnSelectedWorkTypeOptionChanged(OptionDetails? value)
    {
        if (value is not null) WorkTypeError = string.Empty;
    }

    private bool ValidateEducationWorkSection()
    {
        bool isValid = true;

        if (SelectedHighestEducationOption is null)
        {
            EducationError = "Select an option";
            isValid = false;
        }

        if (SelectedCurrentSituationOption is null)
        {
            SituationError = "Select an option";
            isValid = false;
        }

        if (IsWorkTypeVisible && SelectedWorkTypeOption is null)
        {
            WorkTypeError = "Select an option";
            isValid = false;
        }

        return isValid;
    }

    private Task AddEducationWorkSectionInfoAsync()
    {
        if (SelectedHighestEducationOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "highestEducation");
            if (record is not null) record.AnswerId = SelectedHighestEducationOption.AnswerId;
        }

        if (SelectedCurrentSituationOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "currentSituation");
            if (record is not null) record.AnswerId = SelectedCurrentSituationOption.AnswerId;
        }

        if (IsWorkTypeVisible && SelectedWorkTypeOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "workType");
            if (record is not null) record.AnswerId = SelectedWorkTypeOption.AnswerId;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Household Structure section
    // Was: ValidateHouseholdstructureStack() / AddHouseHoldStructureInfo() /
    // peopleentry_TextChanged / roomentry_TextChanged / sharedbathroomsentry_TextChanged /
    // ventlist_ItemTapped / damplist_ItemTapped

    [ObservableProperty] private string _peopleLabel = string.Empty;
    [ObservableProperty] private string _peopleSubLabel = string.Empty;
    [ObservableProperty] private string _peopleCount = string.Empty;
    [ObservableProperty] private bool _peopleError;
    [ObservableProperty] private string _peopleErrorText = string.Empty;

    [ObservableProperty] private string _roomsLabel = string.Empty;
    [ObservableProperty] private string _roomsSubLabel = string.Empty;
    [ObservableProperty] private string _roomsCount = string.Empty;
    [ObservableProperty] private bool _roomsError;
    [ObservableProperty] private string _roomsErrorText = string.Empty;

    [ObservableProperty] private string _sharedBathroomsLabel = string.Empty;
    [ObservableProperty] private string _sharedBathroomsSubLabel = string.Empty;
    [ObservableProperty] private string _sharedBathroomsCount = string.Empty;
    [ObservableProperty] private bool _bathroomsError;
    [ObservableProperty] private string _bathroomsErrorText = string.Empty;

    [ObservableProperty] private string _ventilationLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _ventilationOptions = new();
    [ObservableProperty] private OptionDetails? _selectedVentilationOption;
    [ObservableProperty] private string _ventilationError = string.Empty;

    [ObservableProperty] private string _mouldLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _mouldOptions = new();
    [ObservableProperty] private OptionDetails? _selectedMouldOption;
    [ObservableProperty] private string _mouldError = string.Empty;

    partial void OnPeopleCountChanged(string value) => PeopleError = false;
    partial void OnRoomsCountChanged(string value) => RoomsError = false;
    partial void OnSharedBathroomsCountChanged(string value) => BathroomsError = false;
    partial void OnSelectedVentilationOptionChanged(OptionDetails? value) { if (value is not null) VentilationError = string.Empty; }
    partial void OnSelectedMouldOptionChanged(OptionDetails? value) { if (value is not null) MouldError = string.Empty; }

    private bool ValidateHouseholdStructureSection()
    {
        bool isValid = true;

        if (string.IsNullOrEmpty(PeopleCount)) { PeopleErrorText = LocalizationManager.Get("Register_EnterValue"); PeopleError = true; isValid = false; }
        else
        {
            if(int.TryParse(PeopleCount, out int PC))
            {
                if(PC < 0 || PC> 20)
                {
                    PeopleErrorText = LocalizationManager.Get("Register_TooManyValue");
                    PeopleError = true;
                    isValid = false;
                }
            }
        }
        if (string.IsNullOrEmpty(RoomsCount)) { RoomsErrorText = LocalizationManager.Get("Register_EnterValue"); RoomsError = true; isValid = false; }
        else
        {
            if(int.TryParse(RoomsCount, out int RC))
            {
                if(RC < 0 || RC> 20)
                {
                    RoomsErrorText = LocalizationManager.Get("Register_TooManyValue");
                    RoomsError = true;
                    isValid = false;
                }
            }
        }
        if (string.IsNullOrEmpty(SharedBathroomsCount)) { BathroomsErrorText = LocalizationManager.Get("Register_EnterValue"); BathroomsError = true; isValid = false; }
        else
        {
            if(int.TryParse(SharedBathroomsCount, out int SBC))
            {
                if(SBC < 0 || SBC> 20)
                {
                    BathroomsErrorText = LocalizationManager.Get("Register_TooManyValue");
                    BathroomsError = true;
                    isValid = false;
                }
            }
        }

        if (SelectedVentilationOption is null)
        {
            VentilationError = "Select an option";
            isValid = false;
        }

        if (SelectedMouldOption is null)
        {
            MouldError = "Select an option";
            isValid = false;
        }

        return isValid;
    }

    private Task AddHouseholdStructureSectionInfoAsync()
    {
        var peopleRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "peopleinhome");
        if (peopleRecord is not null) peopleRecord.AnswerValue = PeopleCount?.Trim();

        var roomsRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "rooms");
        if (roomsRecord is not null) roomsRecord.AnswerValue = RoomsCount?.Trim();

        var bathroomsRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "bathrooms");
        if (bathroomsRecord is not null) bathroomsRecord.AnswerValue = SharedBathroomsCount?.Trim();

        if (SelectedVentilationOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "ventiliation");
            if (record is not null) record.AnswerId = SelectedVentilationOption.AnswerId;
        }

        if (SelectedMouldOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "mould");
            if (record is not null) record.AnswerId = SelectedMouldOption.AnswerId;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region NHS Number section
    // Was: ValidatenhsnumStack() / nhsentry_TextChanged() / IsValidNhsNumber() /
    // gplist_ItemTapped() / gpautocomplete_SelectionChanged() / ImageButton_Clicked()
    //
    // GP practice is a pre-filtered list from server config (gpPracticeName.Options), not a
    // live search API like the Address section's postcode lookup — same SfAutocomplete
    // pattern as Ethnicity's country-of-origin picker, just a different backing list.

    [ObservableProperty] private string _nhsNumberLabel = string.Empty;
    [ObservableProperty] private string _nhsNumberSubLabel = string.Empty;
    [ObservableProperty] private string _nhsNumberHelpText = string.Empty;
    [ObservableProperty] private string _nhsNumberSelectHelpText = string.Empty;
    [ObservableProperty] private string _nhsNumber = string.Empty;
    [ObservableProperty] private string _nhsNumberError = string.Empty;
    private bool _isNhsNumberValid;

    [ObservableProperty] private string _gpRegisteredLabel = string.Empty;
    [ObservableProperty] private string _gpRegisteredSubLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _gpRegisteredOptions = new();
    [ObservableProperty] private OptionDetails? _selectedGpRegisteredOption;
    [ObservableProperty] private string _gpRegisteredError = string.Empty;


    [ObservableProperty] private bool _isGpInfoVisible;
    [ObservableProperty] private string _gpPracticeLabel = string.Empty;
    [ObservableProperty] private string _gpPracticeSubLabel = string.Empty;
    [ObservableProperty] private string _gpPracticeSubLabelText = string.Empty;
    [ObservableProperty] private string _gpPracticeSubLabelUrl = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _gpPracticeOptions = new();
    [ObservableProperty] private OptionDetails? _selectedGpOption;
    [ObservableProperty] private bool _isGpSelected;
    [ObservableProperty] private string _gpAddressError = string.Empty;
    [ObservableProperty] private bool _isOutsideLondonWarningVisible;
    [ObservableProperty]
    private string _outsideLondonWarningText =
        "This GP practice appears to be outside London. Please check you've selected the right one.";
    private CancellationTokenSource? _londonCheckCts;

    public sealed class DismissGpKeyboardMessage { }

    partial void OnGpPracticeSubLabelChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        var urlMatch = Regex.Match(value, @"https?://\S+");
        if (urlMatch.Success)
        {
            GpPracticeSubLabelUrl = urlMatch.Value.TrimEnd('.');
            GpPracticeSubLabelText = value[..urlMatch.Index].TrimEnd();
        }
        else
        {
            GpPracticeSubLabelText = value;
            GpPracticeSubLabelUrl = string.Empty;
        }
    }

    [RelayCommand]
    private async Task OpenGpPracticeUrlAsync()
    {
        if (!string.IsNullOrEmpty(GpPracticeSubLabelUrl) &&
            Uri.TryCreate(GpPracticeSubLabelUrl, UriKind.Absolute, out var uri))
        {
            await Launcher.Default.OpenAsync(uri);
        }
    }

    partial void OnNhsNumberChanged(string value)
    {
        string digitsOnly = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 10)
        {
            _isNhsNumberValid = IsValidNhsNumber(digitsOnly);
            NhsNumberError = _isNhsNumberValid ? string.Empty : "Please enter a valid NHS number";
        }
        else
        {
            NhsNumberError = string.Empty;
            _isNhsNumberValid = false;
        }
    }

    partial void OnSelectedGpRegisteredOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        GpRegisteredError = string.Empty;

        IsGpInfoVisible = value.Text.Contains("Yes");
        if (!IsGpInfoVisible)
        {
            SelectedGpOption = null;
            IsGpSelected = false;
            GpAddressError = string.Empty;
        }
    }

    partial void OnSelectedGpOptionChanged(OptionDetails? value)
    {
        GpAddressError = string.Empty;
        IsGpSelected = value is not null;
        IsOutsideLondonWarningVisible = false;
        if (IsGpSelected)
        {
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true));
            _ = CheckGpPracticeLocationAsync(value!.Text);
        }

    }

    [RelayCommand]
    private void ClearGp()
    {
        SelectedGpOption = null;
        IsGpSelected = false;
    }

    private async Task CheckGpPracticeLocationAsync(string gpText)
    {
        _londonCheckCts?.Cancel();
        var cts = new CancellationTokenSource();
        _londonCheckCts = cts;
        try
        {
            var postcode = ExtractPostcode(gpText);
            if (postcode is null) return;

            var isLondon = await IsPostcodeInLondonAsync(postcode, cts.Token);
            if (cts.IsCancellationRequested) return;

            IsOutsideLondonWarningVisible = isLondon == false;
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "CheckGpPracticeLocationAsync");
        }
        finally
        {
            if (ReferenceEquals(_londonCheckCts, cts)) _londonCheckCts = null;
            cts.Dispose();
        }
    }

    private static readonly Regex PostcodeExtractRegex = new(
        @"\b([Gg][Ii][Rr] 0[Aa]{2}|[A-Za-z]{1,2}\d[A-Za-z\d]?\s*\d[A-Za-z]{2})\b",
        RegexOptions.Compiled);

    private static string? ExtractPostcode(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var match = PostcodeExtractRegex.Match(input);
        return match.Success ? match.Value.ToUpperInvariant().Trim() : null;
    }

    private static readonly HttpClient LondonCheckClient = new();

    private static async Task<bool?> IsPostcodeInLondonAsync(string postcode, CancellationToken ct)
    {
        var url = $"https://api.postcodes.io/postcodes/{Uri.EscapeDataString(postcode.Replace(" ", ""))}";
        try
        {
            using var response = await LondonCheckClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var result = doc.RootElement.GetProperty("result");
            var region = result.TryGetProperty("region", out var r) ? r.GetString() : null;
            return string.Equals(region, "London", StringComparison.OrdinalIgnoreCase);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "IsPostcodeInLondonAsync");
            return null;
        }
    }

    // Same Modulus 11 check digit algorithm as the original's IsValidNhsNumber — no I/O
    // happens in it despite the original declaring it `async Task<bool>`, so it's a plain
    // synchronous method here.
   private static bool IsValidNhsNumber(string nhsNumber)
{
    if (string.IsNullOrWhiteSpace(nhsNumber) || nhsNumber.Length != 10 || !nhsNumber.All(char.IsDigit))
        return false;

    // Reject obviously-fake numbers that pass the checksum arithmetically
    // but were never allocated (all zeros is the classic example).
    if (nhsNumber.All(c => c == '0'))
        return false;

    int[] weights = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
    int sum = 0;
    for (int i = 0; i < 9; i++)
    {
        sum += (nhsNumber[i] - '0') * weights[i];
    }

    int remainder = sum % 11;
    int checkDigit = 11 - remainder;
    if (checkDigit == 11) checkDigit = 0;
    if (checkDigit == 10) return false;

    return checkDigit == (nhsNumber[9] - '0');
}

    private Task<bool> ValidateNhsNumSectionAsync()
    {
        bool isValid = true;

        if (!_isNhsNumberValid || string.IsNullOrEmpty(NhsNumber))
        {
            NhsNumberError = "Please enter a valid NHS number";
            isValid = false;
        }

        if (SelectedGpRegisteredOption is null)
        {
            GpRegisteredError = "Select an option";
            isValid = false;
        }

        if (IsGpInfoVisible && SelectedGpOption is null)
        {
            GpAddressError = "Select an option";
            isValid = false;
        }

        return Task.FromResult(isValid);
    }

    private Task AddNhsNumSectionInfoAsync()
    {
        // Original left the actual long-term storage destination for the NHS number itself
        // as a TODO ("//add un where the user nhs number is stored") beyond this
        // QuestionnaireResults entry — that TODO is carried over as-is, not resolved here.
        var nhsRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "nhsNumber");
        if (nhsRecord is not null) nhsRecord.AnswerValue = NhsNumber?.Trim();

        if (SelectedGpRegisteredOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "gpRegistered");
            if (record is not null) record.AnswerId = SelectedGpRegisteredOption.AnswerId;
        }

        if (IsGpInfoVisible && SelectedGpOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "gpPracticeName");
            if (record is not null) record.AnswerId = SelectedGpOption.AnswerId;

            // Original also writes this straight onto newuser (not just QuestionnaireResults) —
            // it's what the API call in CreateAccount actually sends as the user's registered
            // practice, so it needs to survive independently of the questionnaire-results list.
            NewUser.primarycareid = SelectedGpOption.AnswerId;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Respiratory Illness section (ristack)
    // Was: ValidateRIStack() / AddRIInfo() / coughlist_ItemTapped / hoslist_ItemTapped /
    // infectionyearentry_TextChanged / venlist_ItemTapped

    [ObservableProperty] private string _coughLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _coughOptions = new();
    [ObservableProperty] private OptionDetails? _selectedCoughOption;
    [ObservableProperty] private string _coughError = string.Empty;

    [ObservableProperty] private string _hospitalLabel = string.Empty;
    [ObservableProperty] private string _hospitalSubLabel = string.Empty;
    [ObservableProperty] private string _hospitalHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _hospitalOptions = new();
    [ObservableProperty] private OptionDetails? _selectedHospitalOption;
    [ObservableProperty] private string _hospitalError = string.Empty;

    [ObservableProperty] private bool _isInfectionDetailVisible;
    [ObservableProperty] private string _infectionLabel = string.Empty;
    [ObservableProperty] private string _infectionSubLabel = string.Empty;
    [ObservableProperty] private string _infectionYear = string.Empty;
    [ObservableProperty] private bool _infectionYearError;

    [ObservableProperty] private string _ventilatorLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _ventilatorOptions = new();
    [ObservableProperty] private OptionDetails? _selectedVentilatorOption;
    [ObservableProperty] private string _ventilatorError = string.Empty;

    partial void OnSelectedCoughOptionChanged(OptionDetails? value)
    {
        if (value is not null) CoughError = string.Empty;
    }

    partial void OnSelectedHospitalOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        HospitalError = string.Empty;

        IsInfectionDetailVisible = value.Text.Contains("Yes");
        if (!IsInfectionDetailVisible)
        {
            SelectedVentilatorOption = null;
            VentilatorError = string.Empty;
            InfectionYearError = false;
        }
    }

    partial void OnInfectionYearChanged(string value) => InfectionYearError = false;

    partial void OnSelectedVentilatorOptionChanged(OptionDetails? value)
    {
        if (value is not null) VentilatorError = string.Empty;
    }

    private bool ValidateRiSection()
    {
        bool isValid = true;

        if (SelectedCoughOption is null)
        {
            CoughError = "Select an option";
            isValid = false;
        }

        if (SelectedHospitalOption is null)
        {
            HospitalError = "Select an option";
            isValid = false;
        }

        if (IsInfectionDetailVisible)
        {
            if (string.IsNullOrEmpty(InfectionYear) || !int.TryParse(InfectionYear, out int year) || year < 1900 || year > DateTime.Now.Year)
            {
                InfectionYearError = true;
                isValid = false;
            }

            if (SelectedVentilatorOption is null)
            {
                VentilatorError = "Select an option";
                isValid = false;
            }
        }

        return isValid;
    }

    private Task AddRiSectionInfoAsync()
    {
        if (SelectedCoughOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "coughfield");
            if (record is not null) record.AnswerId = SelectedCoughOption.AnswerId;
        }

        if (SelectedHospitalOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "hosfield");
            if (record is not null) record.AnswerId = SelectedHospitalOption.AnswerId;
        }

        if (IsInfectionDetailVisible)
        {
            var infectionRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "infectionfield");
            if (infectionRecord is not null) infectionRecord.AnswerValue = InfectionYear?.Trim();

            if (SelectedVentilatorOption is not null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "venfield");
                if (record is not null) record.AnswerId = SelectedVentilatorOption.AnswerId;
            }
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Health Conditions section (hcstack + addhcstack)
    // Was: ValidateHealthConditionsStack() / AddHealthConditionsInfo() / hcfirstlist_ItemTapped
    // and ValidateAddHealthConditionsStack() / AddHealthConditionsADD() / otherhclist_ItemTapped /
    // disautocomplete_SelectionChanged / OnRemoveChipTapped / typeotherhclist_ItemTapped /
    // otherhcentrytext_TextChanged / cancerlist_ItemTapped / cancernowlist_ItemTapped

    [ObservableProperty] private string _diaLabel = string.Empty;
    [ObservableProperty] private string _diaDirections = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _healthConditionsGateOptions = new();
    [ObservableProperty] private string? _selectedHcGateOption;
    [ObservableProperty] private string _hcGateError = string.Empty;

    [ObservableProperty] private string _diaAddLabel = string.Empty;
    [ObservableProperty] private string _diaAddDirections = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _conditionOptions = new();
    public ObservableCollection<OptionDetails> SelectedConditions { get; } = new();
    [ObservableProperty] private string _conditionAddError = string.Empty;

    [ObservableProperty] private string _otherHcLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _otherHcOptions = new();
    [ObservableProperty] private OptionDetails? _selectedOtherHcOption;
    [ObservableProperty] private string _otherHcError = string.Empty;

    [ObservableProperty] private bool _isOtherHcTypeVisible;
    [ObservableProperty] private string _typeOtherHcLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _typeOtherHcOptions = new();
    [ObservableProperty] private OptionDetails? _selectedTypeOtherHcOption;
    [ObservableProperty] private string _bodyPartError = string.Empty;
    [ObservableProperty] private string _otherHcEnterLabel = string.Empty;
    [ObservableProperty] private string _otherHcEnterSubLabel = string.Empty;
    [ObservableProperty] private string _otherHcFreeText = string.Empty;
    [ObservableProperty] private string _otherHcFreeTextError = string.Empty;

    [ObservableProperty] private bool _isCancerVisible;
    [ObservableProperty] private string _cancerLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _cancerOptions = new();
    [ObservableProperty] private OptionDetails? _selectedCancerOption;
    [ObservableProperty] private string _cancerError = string.Empty;
    [ObservableProperty] private string _cancerNowLabel = string.Empty;
    [ObservableProperty] private string _cancerNowHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _cancerNowOptions = new();
    [ObservableProperty] private OptionDetails? _selectedCancerNowOption;
    [ObservableProperty] private string _cancerNowError = string.Empty;

    partial void OnSelectedHcGateOptionChanged(string? value)
    {
        if (value is not null) HcGateError = string.Empty;
    }

    partial void OnSelectedOtherHcOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        OtherHcError = string.Empty;

        IsOtherHcTypeVisible = value.Text == "Yes";
        if (!IsOtherHcTypeVisible)
        {
            SelectedTypeOtherHcOption = null;
            BodyPartError = string.Empty;
            OtherHcFreeTextError = string.Empty;
            CancerError = string.Empty;
            CancerNowError = string.Empty;
        }
    }

    partial void OnSelectedTypeOtherHcOptionChanged(OptionDetails? value)
    {
        if (value is not null) BodyPartError = string.Empty;
    }

    partial void OnOtherHcFreeTextChanged(string value) => OtherHcFreeTextError = string.Empty;
    partial void OnSelectedCancerOptionChanged(OptionDetails? value) { if (value is not null) CancerError = string.Empty; }
    partial void OnSelectedCancerNowOptionChanged(OptionDetails? value) { if (value is not null) CancerNowError = string.Empty; }

    /// <summary>Was: disautocomplete_SelectionChanged's add-to-list half. Code-behind forwards
    /// the autocomplete's own SelectionChanged event here, since a multi-select "pick one,
    /// clear the field, add a chip" flow isn't something a plain property binding expresses —
    /// same reasoning as ScrollResetRequested/InfoRequested elsewhere in this file.</summary>
    [RelayCommand]
    private void AddCondition(OptionDetails item)
    {
        if (item is null) return;

        if (!SelectedConditions.Any(x => x.Text == item.Text))
        {
            SelectedConditions.Add(item);
            ConditionAddError = string.Empty;
        }

        IsCancerVisible = SelectedConditions.Any(x =>
            x.Value is not null && x.Value.Contains("Cancer", StringComparison.OrdinalIgnoreCase));
    }

    [RelayCommand]
    private void RemoveCondition(OptionDetails item)
    {
        if (item is not null) SelectedConditions.Remove(item);
        // NOTE: matches the original — removing a chip doesn't re-check IsCancerVisible, so
        // the cancer follow-up questions stay visible even if the condition that revealed them
        // gets removed afterward. Not carried over as a "this must be a bug" judgment call the
        // way the moveerror/duplicate-insert issues were — this one's more a rough edge than a
        // clear mistake, so it's left exactly as the original behaves.
    }

    private bool ValidateHcSection()
    {
        if (SelectedHcGateOption is null)
        {
            HcGateError = "Select an option";
            return false;
        }

        return true;
    }

    private Task AddHcSectionInfoAsync()
    {
        if (SelectedHcGateOption == "Yes")
        {
            var additionalConditions = RegistrationSectionsNotRequired.FirstOrDefault(x => x.Type == "otherhc");

            // NOTE: the original's duplicate-insert guard checks
            // `x.XamlNameArea == "otherhc"` — but "otherhc" is this field's Type marker, not
            // its XamlNameArea (used consistently as "addhcstack" in ValidateAddHealthConditionsStack,
            // AddHealthConditionsADD, and ShowCurrentStack's own addhcstack branch). A XamlNameArea
            // can't equal "otherhc", so that guard could never actually match, meaning repeated
            // "Yes" answers (e.g. after going back and re-answering) could insert the section
            // more than once. Checking Type == "otherhc" instead — matching how this exact
            // field is located everywhere else in the original — is what makes the guard work.
            if (additionalConditions is not null && !RegistrationSections.Any(x => x.Type == "otherhc"))
            {
                int targetIndex = CurrentFieldIndex + 1;
                int safeIndex = Math.Clamp(targetIndex, 0, RegistrationSections.Count);
                RegistrationSections.Insert(safeIndex, additionalConditions);
                RefreshProgress();
            }
        }
        else
        {
            var existing = RegistrationSections.FirstOrDefault(x => x.Type == "otherhc");
            if (existing is not null)
            {
                RegistrationSections.Remove(existing);
                RefreshProgress();
            }
        }

        var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "diastack");
        if (record is not null)
        {
            // Matches the original's hard-coded convention for this one Yes/No answer
            // (QuestionId + "_1"/"_2") rather than the usual OptionDetails.AnswerId lookup —
            // hcfirstlist is a plain string list, not OptionDetails, so there's no AnswerId to
            // read here in the first place.
            record.AnswerId = SelectedHcGateOption == "Yes" ? $"{record.QuestionId}_1" : $"{record.QuestionId}_2";
        }

        return Task.CompletedTask;
    }

    private bool ValidateAddHcSection()
    {
        bool isValid = true;
        // Original leaves the "must add at least one condition" check commented out
        // (conditionschips.ItemsSource == null) — no validation on SelectedConditions here,
        // matching that; adding a condition is effectively optional at this step.

        if (SelectedOtherHcOption is null)
        {
            OtherHcError = "Select an option";
            isValid = false;
        }

        if (IsOtherHcTypeVisible)
        {
            if (SelectedTypeOtherHcOption is null)
            {
                BodyPartError = "Select an option";
                isValid = false;
            }

            if (string.IsNullOrEmpty(OtherHcFreeText))
            {
                OtherHcFreeTextError = "Please complete";
                isValid = false;
            }
        }

        if (IsCancerVisible)
        {
            if (SelectedCancerOption is null)
            {
                CancerError = "Select an option";
                isValid = false;
            }

            if (SelectedCancerNowOption is null)
            {
                CancerNowError = "Select an option";
                isValid = false;
            }
        }

        return isValid;
    }

    private Task AddAddHcSectionInfoAsync()
    {
        var diaAddRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "diaaddstack");
        if (diaAddRecord is not null)
        {
            diaAddRecord.AnswerId = string.Join(", ", SelectedConditions.Select(x => x.AnswerId));
        }

        if (SelectedOtherHcOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "otherhc");
            if (record is not null) record.AnswerId = SelectedOtherHcOption.AnswerId;
        }

        if (IsOtherHcTypeVisible)
        {
            if (SelectedTypeOtherHcOption is not null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "affectedsystem");
                if (record is not null) record.AnswerId = SelectedTypeOtherHcOption.AnswerId;
            }

            var freeTextRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "otherhealthconditions");
            if (freeTextRecord is not null) freeTextRecord.AnswerValue = OtherHcFreeText?.Trim();
        }

        if (IsCancerVisible)
        {
            if (SelectedCancerOption is not null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "pastcancerdiagnosis");
                if (record is not null) record.AnswerId = SelectedCancerOption.AnswerId;
            }

            if (SelectedCancerNowOption is not null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "cancerremissionstatus");
                if (record is not null) record.AnswerId = SelectedCancerNowOption.AnswerId;
            }
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Medications section (medynstack + medicationsstack)
    // Was: ValidateMedicationsStack() / AddMedicationsInfo() / medsfirstlist_ItemTapped
    // and ValidateMedicationsADDStack() / AddMedicationsADD() / medautocomplete_SelectionChanged /
    // othermedlist_ItemTapped / othermedtextentry_TextChanged / TapGestureRecognizer_Tapped_2
    // (the medications chip-remove handler — same shape as addhcstack's OnRemoveChipTapped,
    // just a second copy under a different name in the original since it targets
    // SelectedMedications instead of SelectedConditons).
    //
    // Structurally identical to Health Conditions, minus the cancer sub-flow: a Yes/No gate
    // that dynamically inserts the add-a-medication screen, which itself is an autocomplete +
    // chips list plus a "not listed" Yes/No with a conditional free-text detail field.

    [ObservableProperty] private string _medynLabel = string.Empty;
    [ObservableProperty] private string _medynDirections = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _medicationsGateOptions = new();
    [ObservableProperty] private string? _selectedMedynGateOption;
    [ObservableProperty] private string _medynGateError = string.Empty;

    [ObservableProperty] private string _medLabel = string.Empty;
    [ObservableProperty] private string _medDirections = string.Empty;
    [ObservableProperty] private string _medHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _medicationOptions = new();
    public ObservableCollection<OptionDetails> SelectedMedications { get; } = new();
    [ObservableProperty] private string _medicationAddError = string.Empty;

    [ObservableProperty] private string _otherMedLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _otherMedOptions = new();
    [ObservableProperty] private OptionDetails? _selectedOtherMedOption;
    [ObservableProperty] private string _otherMedError = string.Empty;

    [ObservableProperty] private bool _isOtherMedDetailsVisible;
    [ObservableProperty] private string _otherMedDetailsLabel = string.Empty;
    [ObservableProperty] private string _otherMedDetailsSubLabel = string.Empty;
    [ObservableProperty] private string _otherMedFreeText = string.Empty;
    [ObservableProperty] private string _otherMedDetailsError = string.Empty;

    partial void OnSelectedMedynGateOptionChanged(string? value)
    {
        if (value is not null) MedynGateError = string.Empty;
    }

    [RelayCommand]
    private void AddMedication(OptionDetails item)
    {
        if (item is null) return;

        if (!SelectedMedications.Any(x => x.Text == item.Text))
        {
            SelectedMedications.Add(item);
            MedicationAddError = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveMedication(OptionDetails item)
    {
        if (item is not null) SelectedMedications.Remove(item);
    }

    partial void OnSelectedOtherMedOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        OtherMedError = string.Empty;

        IsOtherMedDetailsVisible = value.Text == "Yes";
        if (!IsOtherMedDetailsVisible)
        {
            OtherMedDetailsError = string.Empty;
        }
    }

    partial void OnOtherMedFreeTextChanged(string value) => OtherMedDetailsError = string.Empty;

    private bool ValidateMedynSection()
    {
        if (SelectedMedynGateOption is null)
        {
            MedynGateError = "Select an option";
            return false;
        }

        return true;
    }

    private Task AddMedynSectionInfoAsync()
    {
        if (SelectedMedynGateOption == "Yes")
        {
            var additionalMedications = RegistrationSectionsNotRequired.FirstOrDefault(x => x.XamlNameArea == "medicationsstack");
            if (additionalMedications is not null && !RegistrationSections.Any(x => x.XamlNameArea == "medicationsstack"))
            {
                int targetIndex = CurrentFieldIndex + 1;
                int safeIndex = Math.Clamp(targetIndex, 0, RegistrationSections.Count);
                RegistrationSections.Insert(safeIndex, additionalMedications);
                RefreshProgress();
            }
        }
        else
        {
            var existing = RegistrationSections.FirstOrDefault(x => x.XamlNameArea == "medicationsstack");
            if (existing is not null)
            {
                RegistrationSections.Remove(existing);
                RefreshProgress();
            }
        }

        var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "firstmedstack");
        if (record is not null)
        {
            record.AnswerId = SelectedMedynGateOption == "Yes" ? $"{record.QuestionId}_1" : $"{record.QuestionId}_2";
        }

        return Task.CompletedTask;
    }

    private bool ValidateMedicationsSection()
    {
        bool isValid = true;

        if (SelectedOtherMedOption is null)
        {
            OtherMedError = "Select an option";
            isValid = false;
        }

        if (IsOtherMedDetailsVisible && string.IsNullOrEmpty(OtherMedFreeText))
        {
            OtherMedDetailsError = "Please complete";
            isValid = false;
        }

        return isValid;
    }

    private Task AddMedicationsSectionInfoAsync()
    {
        var medRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "medqstack");
        if (medRecord is not null)
        {
            medRecord.AnswerId = string.Join(", ", SelectedMedications.Select(x => x.AnswerId));
        }

        if (SelectedOtherMedOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "othermeds");
            if (record is not null) record.AnswerId = SelectedOtherMedOption.AnswerId;
        }

        if (IsOtherMedDetailsVisible)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "othermedications");
            if (record is not null) record.AnswerValue = OtherMedFreeText?.Trim();
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Recent Vaccinations section (rvstack)
    // Was: ValidateRVStack() / AddRVInfo() / flulist_SelectionChanged() / SetError()
    //
    // ynflu (order 1) is a Yes/No gate: only it is shown initially. Selecting "Yes" reveals
    // the flufield multi-select (and its conditional date fields). Selecting "No" skips the
    // flu detail questions entirely. The gate must have a selection before the user can
    // proceed.
    //
    // flulist is a multi-select list ("which vaccines have you had: Flu / COVID / RSV") —
    // selecting any one of those three options reveals its own date field. The separate
    // covidlist/rsvlist controls and their _ItemTapped handlers in the original are leftover
    // from an earlier single-question-per-vaccine design: their ItemsSource is only ever set
    // inside commented-out code, so they render with no items and can never actually be
    // interacted with — not carried over, same as the other confirmed-dead controls noted
    // elsewhere in this file.

    // --- ynflu gate (new Yes/No question, shown first) ---
    [ObservableProperty] private string _ynFluLabel = string.Empty;
    [ObservableProperty] private string _ynFluSubLabel = string.Empty;
    [ObservableProperty] private string _ynFluDirections = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _ynFluOptions = new();
    [ObservableProperty] private OptionDetails? _selectedYnFluOption;
    [ObservableProperty] private string _ynFluGateError = string.Empty;

    /// <summary>True only when the user has selected "Yes" on the ynflu gate question,
    /// revealing the flufield multi-select and its downstream date fields.</summary>
    public bool IsFluSectionVisible => SelectedYnFluOption?.Text == "Yes";

    partial void OnSelectedYnFluOptionChanged(OptionDetails? value)
    {
        YnFluGateError = string.Empty;
        OnPropertyChanged(nameof(IsFluSectionVisible));
        // Clear flu selection + errors when user switches back to "No"
        if (value?.Text != "Yes")
        {
            SelectedFluOptions.Clear();
            FluGateError = string.Empty;
            FluDateError = string.Empty;
            CovidDateError = string.Empty;
            RsvDateError = string.Empty;
        }
    }

    [ObservableProperty] private string _fluLabel = string.Empty;
    [ObservableProperty] private string _fluSubLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _fluOptions = new();
    // FIX: SfListView.SelectedItems has no accessible setter, so it can't be TwoWay-bound —
    // this is a plain mirror that code-behind populates from the control's own SelectedItems
    // whenever its SelectionChanged fires (see NewImperial.xaml.cs). No clear-request event
    // needed here, unlike SelectedExtraOptions below — nothing in this ViewModel ever clears
    // this collection programmatically, only the control's own selection changes touch it.
    public ObservableCollection<OptionDetails> SelectedFluOptions { get; set; } = new();
    [ObservableProperty] private string _fluGateError = string.Empty;

    [ObservableProperty] private bool _isFluDateVisible;
    [ObservableProperty] private string _fluDateLabel = string.Empty;
    [ObservableProperty] private string _fluDateSubLabel = string.Empty;
    [ObservableProperty] private string _fluDateText = string.Empty;
    [ObservableProperty] private string _fluDateError = string.Empty;

    [ObservableProperty] private bool _isCovidDateVisible;
    [ObservableProperty] private string _covidDateLabel = string.Empty;
    [ObservableProperty] private string _covidDateSubLabel = string.Empty;
    [ObservableProperty] private string _covidDateText = string.Empty;
    [ObservableProperty] private string _covidDateError = string.Empty;

    [ObservableProperty] private bool _isRsvDateVisible;
    [ObservableProperty] private string _rsvDateLabel = string.Empty;
    [ObservableProperty] private string _rsvDateSubLabel = string.Empty;
    [ObservableProperty] private string _rsvDateText = string.Empty;
    [ObservableProperty] private string _rsvDateError = string.Empty;

    /// <summary>Was: flulist_SelectionChanged. Bound via SelectedItems (TwoWay) rather than a
    /// code-behind event, since — unlike the autocomplete "add and clear" flows above — this
    /// is a plain "keep three booleans in sync with the current multi-selection" job, which a
    /// CollectionChanged subscription on the ViewModel's own collection handles directly.</summary>
    private void OnFluSelectionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        bool hasFlu = SelectedFluOptions.Any(x => x.Text.Contains("Flu"));
        bool hasCovid = SelectedFluOptions.Any(x => x.Text.Contains("COVID"));
        bool hasRsv = SelectedFluOptions.Any(x => x.Text.Contains("RSV"));

        IsFluDateVisible = hasFlu;
        if (!hasFlu) FluDateError = string.Empty;

        IsCovidDateVisible = hasCovid;
        if (!hasCovid) CovidDateError = string.Empty;

        IsRsvDateVisible = hasRsv;
        if (!hasRsv) RsvDateError = string.Empty;

        FluGateError = string.Empty;
    }

    public event Action? SelectionFluRestore;
    public event Action? SelecteionExtraRestore;
    public event Action? SelecteionWhatDrugsRestore;
    public event Action? SelecteionTobaccoRestore;

    partial void OnFluDateTextChanged(string value) => FluDateError = string.Empty;
    partial void OnCovidDateTextChanged(string value) => CovidDateError = string.Empty;
    partial void OnRsvDateTextChanged(string value) => RsvDateError = string.Empty;

    // Same exact-format, invariant-culture parse used throughout this file for
    // DateOfBirthText (see ValidateGenderSection / the Body Metrics age calculation) — applied
    // here too, to both the stored birth date and each vaccine date, instead of the original's
    // locale-sensitive DateTime.TryParse for this one section.
    private static bool TryParseExactDate(string? text, out DateTime result) =>
        DateTime.TryParseExact(text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);

    private bool ValidateRvSection()
    {
        // ynflu gate must be answered before anything else
        if (SelectedYnFluOption is null)
        {
            YnFluGateError = "Select an option";
            return false;
        }

        // "No" → user has confirmed no recent vaccines; skip all flu detail validation
        if (SelectedYnFluOption.Text == "No")
            return true;

        // "Yes" → validate the flufield multi-select and any visible date fields
        bool isValid = true;

        if (SelectedFluOptions.Count == 0)
        {
            FluGateError = "Select an option";
            isValid = false;
        }

        DateTime? birthDate = TryParseExactDate(DateOfBirthText, out var dob) ? dob : null;
        var today = DateTime.Today;

        if (IsFluDateVisible) isValid &= ValidateVaccineDate(FluDateText, birthDate, today, v => FluDateError = v);
        if (IsCovidDateVisible) isValid &= ValidateVaccineDate(CovidDateText, birthDate, today, v => CovidDateError = v);
        if (IsRsvDateVisible) isValid &= ValidateVaccineDate(RsvDateText, birthDate, today, v => RsvDateError = v);

        return isValid;
    }

    private static readonly string[] AcceptedDateFormats =
    {
        "dd/MM/yyyy",
        "dd/MM/yy",
        "dd/MM"
    };

    private static bool TryParseSelectDate(string? text, out DateTime date)
    {
        return DateTime.TryParseExact(
            text,
            AcceptedDateFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);
    }

    private static bool ValidateVaccineDate(string? text, DateTime? birthDate, DateTime today, Action<string> setError)
    {
        if (string.IsNullOrWhiteSpace(text)) { setError("Enter Value"); return false; }
        if (!TryParseSelectDate(text, out var date)) { setError("Enter Valid Date"); return false; }
        if (date.Date > today) { setError("Date cannot be in the future"); return false; }
        if (birthDate.HasValue && date.Date < birthDate.Value.Date) { setError("Date cannot be before birth date"); return false; }
        setError(string.Empty);
        return true;
    }

    private Task AddRvSectionInfoAsync()
    {
        // Record the ynflu gate answer
        var ynFluRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "ynflu");
        if (ynFluRecord is not null && SelectedYnFluOption is not null)
        {
            ynFluRecord.AnswerId = SelectedYnFluOption.AnswerId;
        }

        // If user said "No" there are no vaccine details to record
        if (SelectedYnFluOption?.Text != "Yes")
            return Task.CompletedTask;

        // NOTE: ported as-is from AddRVInfo() — it reads a single "SelectedItem" from what's
        // now a multi-select list, an inconsistency already present in the original (the
        // selection handler correctly treats this as multi-select; the commit step doesn't).
        // Using the most recently added selection as the closest equivalent, since a
        // multi-select list's own "SelectedItem" typically reflects the most recent tap rather
        // than list order — flagging this rather than guessing further, since it only affects
        // which single AnswerId gets recorded when more than one vaccine is selected.
        var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "flufield");
        if (record is not null && SelectedFluOptions.Count > 0)
        {
            record.AnswerId = SelectedFluOptions[^1].AnswerId;
        }

        if (IsFluDateVisible)
        {
            var r = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "fludatefield");
            if (r is not null) r.AnswerValue = FluDateText?.Trim();
        }

        if (IsCovidDateVisible)
        {
            var r = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "coviddatefield");
            if (r is not null) r.AnswerValue = CovidDateText?.Trim();
        }

        if (IsRsvDateVisible)
        {
            var r = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "rsvdatefield");
            if (r is not null) r.AnswerValue = RsvDateText?.Trim();
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Diet section (dietstack)
    // Was: ValidateDietStack() / AddDietInfo() / dietlist_ItemTapped / anylist_ItemTapped /
    // dietlengthlist_ItemTapped / takelist_ItemTapped / extralist_ItemTapped
    //
    // dietlist's follow-up (diet length) is revealed by the tapped option's *position* in the
    // list (indices 1-3), not its text — ported exactly as that index check, even though it's
    // inherently order-dependent on however the server returns dietfield.Options.

    [ObservableProperty] private string _dietLabel = string.Empty;
    [ObservableProperty] private string _dietSubLabel = string.Empty;
    [ObservableProperty] private string _dietHelpText = string.Empty;
    [ObservableProperty] private bool _isDietSubLabelVisible;
    [ObservableProperty] private ObservableCollection<OptionDetails> _dietOptions = new();
    [ObservableProperty] private OptionDetails? _selectedDietOption;
    [ObservableProperty] private string _dietError = string.Empty;

    [ObservableProperty] private bool _isDietLengthVisible;
    [ObservableProperty] private string _dietLengthLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _dietLengthOptions = new();
    [ObservableProperty] private OptionDetails? _selectedDietLengthOption;
    [ObservableProperty] private string _dietLengthError = string.Empty;

    // prevdietfield: previous diet question, visible only when diet length answer is b_diet_type_length_1 ("Less than 6 months")
    [ObservableProperty] private bool _isPrevDietVisible;
    [ObservableProperty] private string _prevDietLabel = string.Empty;
    [ObservableProperty] private string _prevDietHelpText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _prevDietOptions = new();
    [ObservableProperty] private OptionDetails? _selectedPrevDietOption;
    [ObservableProperty] private string _prevDietError = string.Empty;

    [ObservableProperty] private string _supplementsLabel = string.Empty;
    [ObservableProperty] private string _supplementsSubLabel = string.Empty;
    [ObservableProperty] private bool _isSupplementsSubLabelVisible;
    [ObservableProperty] private ObservableCollection<OptionDetails> _supplementsOptions = new();
    [ObservableProperty] private OptionDetails? _selectedSupplementsOption;
    [ObservableProperty] private string _supplementsError = string.Empty;

    [ObservableProperty] private bool _isTakeExtraVisible;
    [ObservableProperty] private string _takeLabel = string.Empty;
    [ObservableProperty] private string _takeSubLabel = string.Empty;
    [ObservableProperty] private bool _isTakeSubLabelVisible;
    [ObservableProperty] private ObservableCollection<OptionDetails> _takeOptions = new();
    [ObservableProperty] private OptionDetails? _selectedTakeOption;
    [ObservableProperty] private string _takeError = string.Empty;

    [ObservableProperty] private string _extraLabel = string.Empty;
    [ObservableProperty] private string _extraSubLabel = string.Empty;
    [ObservableProperty] private bool _isExtraSubLabelVisible;
    [ObservableProperty] private ObservableCollection<OptionDetails> _extraOptions = new();
    // FIX: SfListView.SelectedItems has no accessible setter, so it can't be TwoWay-bound —
    // this is now a plain mirror that code-behind populates from the control's own
    // SelectedItems whenever its SelectionChanged fires (see NewImperial.xaml.cs).
    public ObservableCollection<OptionDetails> SelectedExtraOptions { get; set; } = new();
    [ObservableProperty] private string _extraError = string.Empty;

    /// <summary>Code-behind hooks this up to clearing extraListView.SelectedItems directly —
    /// see the note on SelectedExtraOptions above for why this can't just be a binding.</summary>
    public event Action? ExtraSelectionClearRequested;

    partial void OnSelectedDietOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        DietError = string.Empty;

        int index = DietOptions.IndexOf(value);
        IsDietLengthVisible = index is 1 or 2 or 3;
        if (!IsDietLengthVisible)
        {
            SelectedDietLengthOption = null;
            DietLengthError = string.Empty;
        }
    }

    partial void OnSelectedDietLengthOptionChanged(OptionDetails? value)
    {
        if (value is not null) DietLengthError = string.Empty;

        IsPrevDietVisible = value?.AnswerId == "b_diet_type_length_1";
        if (!IsPrevDietVisible)
        {
            //SelectedPrevDietOption = null;
            PrevDietError = string.Empty;
        }
    }

    partial void OnSelectedPrevDietOptionChanged(OptionDetails? value)
    {
        if (value is not null) PrevDietError = string.Empty;
    }

    partial void OnSelectedSupplementsOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        SupplementsError = string.Empty;

        IsTakeExtraVisible = value.Text == "Yes";
        if (!IsTakeExtraVisible)
        {
            SelectedTakeOption = null;
            SelectedExtraOptions.Clear();
            // SelectedExtraOptions here is a mirror this ViewModel keeps in sync with the
            // control's own SfListView.SelectedItems (see the fix note on that property
            // below) — clearing the mirror doesn't clear the control itself, so code-behind
            // needs telling separately.
            ExtraSelectionClearRequested?.Invoke();
            TakeError = string.Empty;
            ExtraError = string.Empty;
        }
    }

    partial void OnSelectedTakeOptionChanged(OptionDetails? value)
    {
        if (value is not null) TakeError = string.Empty;
    }

    private bool ValidateDietSection()
    {
        bool isValid = true;

        if (SelectedDietOption is null)
        {
            DietError = "Select an option";
            isValid = false;
        }

        if (IsDietLengthVisible && SelectedDietLengthOption is null)
        {
            DietLengthError = "Select an option";
            isValid = false;
        }

        if (IsPrevDietVisible && SelectedPrevDietOption is null)
        {
            PrevDietError = "Select an option";
            isValid = false;
        }

        if (SelectedSupplementsOption is null)
        {
            SupplementsError = "Select an option";
            isValid = false;
        }

        if (IsTakeExtraVisible)
        {
            if (SelectedTakeOption is null)
            {
                TakeError = "Select an option";
                isValid = false;
            }

            if (SelectedExtraOptions.Count == 0)
            {
                ExtraError = "Select an option";
                isValid = false;
            }
        }

        return isValid;
    }

    private Task AddDietSectionInfoAsync()
    {
        if (SelectedDietOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "dietfield");
            if (record is not null) record.AnswerId = SelectedDietOption.AnswerId;
        }

        if (IsDietLengthVisible && SelectedDietLengthOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "dietlengthfield");
            if (record is not null) record.AnswerId = SelectedDietLengthOption.AnswerId;
        }

        if (IsPrevDietVisible && SelectedPrevDietOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "prevdietfield");
            if (record is not null) record.AnswerId = SelectedPrevDietOption.AnswerId;
        }

        if (SelectedSupplementsOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "supplementsfield");
            if (record is not null) record.AnswerId = SelectedSupplementsOption.AnswerId;
        }

        if (IsTakeExtraVisible)
        {
            if (SelectedTakeOption is not null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "takefield");
                if (record is not null) record.AnswerId = SelectedTakeOption.AnswerId;
            }

            if (SelectedExtraOptions.Count > 0)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "nutrientlistfield");
                if (record is not null) record.AnswerId = string.Join(", ", SelectedExtraOptions.Select(x => x.AnswerId));
            }
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Menstrual section (menstrualstack)
    // Was: ValidateMenstrualInfo() / AddMenstrualInfo() / mensturallist_ItemTapped /
    // pregweeksentry_TextChanged / pregdateentry_TextChanged
    //
    // Same index-based follow-up pattern as dietstack: index 4 reveals "weeks pregnant",
    // index 5 reveals "delivery date" — mutually exclusive, ported exactly as the position
    // check rather than guessing at option text.

    [ObservableProperty] private string _menstrualLabel = string.Empty;
    [ObservableProperty] private string _menstrualHelpText = string.Empty;
    [ObservableProperty] private string _deliveryDateErrorText = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _menstrualOptions = new();
    [ObservableProperty] private OptionDetails? _selectedMenstrualOption;
    [ObservableProperty] private string _menstrualError = string.Empty;

    [ObservableProperty] private bool _isPregnancyWeeksVisible;
    [ObservableProperty] private string _pregnancyWeeksLabel = string.Empty;
    [ObservableProperty] private string _pregnancyWeeksText = string.Empty;
    [ObservableProperty] private string _pregnancyWeeksError = string.Empty;

    [ObservableProperty] private bool _isDeliveryDateVisible;
    [ObservableProperty] private string _deliveryDateLabel = string.Empty;
    [ObservableProperty] private string _deliveryDateText = string.Empty;
    [ObservableProperty] private bool _deliveryDateError;

    partial void OnSelectedMenstrualOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        MenstrualError = string.Empty;

        int index = MenstrualOptions.IndexOf(value);
        IsPregnancyWeeksVisible = index == 4;
        IsDeliveryDateVisible = index == 5;

        if (!IsPregnancyWeeksVisible) PregnancyWeeksError = string.Empty;
        if (!IsDeliveryDateVisible) DeliveryDateError = false;
    }

    partial void OnPregnancyWeeksTextChanged(string value) => PregnancyWeeksError = string.Empty;
    partial void OnDeliveryDateTextChanged(string value) => DeliveryDateError = false;

    private bool ValidateMenstrualSection()
    {
        bool isValid = true;

        if (SelectedMenstrualOption is null)
        {
            MenstrualError = "Select an option";
            isValid = false;
        }

        if (IsPregnancyWeeksVisible)
        {
            if (string.IsNullOrWhiteSpace(PregnancyWeeksText))
            {
                PregnancyWeeksError = "Enter Value";
                isValid = false;
            }
            else if (!int.TryParse(PregnancyWeeksText, out int weeks) || weeks < 0 || weeks > 55)
            {
                PregnancyWeeksError = "Enter Value between 0 and 55 weeks";
                isValid = false;
            }
        }

        // Matches the original exactly: a length check (>= 10 chars, e.g. "31/01/2026"),
        // not an actual date parse.
        if (IsDeliveryDateVisible)
        {
            if (string.IsNullOrWhiteSpace(DeliveryDateText))
            {
                DeliveryDateError = true;
                DeliveryDateErrorText = "Enter Value";
                isValid = false;
            }
            else
            {
                if(!DateTime.TryParseExact(DeliveryDateText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var deliveryDate))
                {
                    DeliveryDateError = true;
                    DeliveryDateErrorText = "Enter a valid date";
                    isValid = false;
                }
                else if (deliveryDate > DateTime.Today)
                {
                    DeliveryDateError = true;
                    DeliveryDateErrorText = "Delivery date cannot be in the future";
                    isValid = false;
                }
                else
                {
                    DeliveryDateError = false;
                    DeliveryDateErrorText = string.Empty;
                }

            }
        }

        return isValid;
    }

    private Task AddMenstrualSectionInfoAsync()
    {
        if (SelectedMenstrualOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "menstrualcyclefield");
            if (record is not null) record.AnswerId = SelectedMenstrualOption.AnswerId;
        }

        if (IsPregnancyWeeksVisible)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "pregnancyweeksfield");
            if (record is not null) record.AnswerValue = PregnancyWeeksText?.Trim() ?? string.Empty;
        }

        if (IsDeliveryDateVisible)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "deliverydatefield");
            if (record is not null) record.AnswerValue = DeliveryDateText?.Trim() ?? string.Empty;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Health Status section (htstack)
    // Was: ValidateHtInfo() / AddHtInfo() / moblist_ItemTapped / sclist_ItemTapped /
    // uclist_ItemTapped / painlist_ItemTapped / deplist_ItemTapped / healthSlider_ValueChanged
    //
    // Standard EQ-5D-5L shape: five single-select dimensions plus a 0-100 health "thermometer"
    // slider that starts at 50 every time this section is shown (matches the original —
    // ShowCurrentStack always resets it to 50 rather than restoring a previous answer, same as
    // every other section here not restoring prior selections on revisit).

    [ObservableProperty] private string _mobilityLabel = string.Empty;
    [ObservableProperty] private string _mobilitySubLabel = string.Empty;
    [ObservableProperty] private bool _isMobilitySubLabelVisible;
    [ObservableProperty] private ObservableCollection<OptionDetails> _mobilityOptions = new();
    [ObservableProperty] private OptionDetails? _selectedMobilityOption;
    [ObservableProperty] private string _mobilityError = string.Empty;

    [ObservableProperty] private string _selfCareLabel = string.Empty;
    [ObservableProperty] private string _selfCareSubLabel = string.Empty;
    [ObservableProperty] private bool _isSelfCareSubLabelVisible;
    [ObservableProperty] private ObservableCollection<OptionDetails> _selfCareOptions = new();
    [ObservableProperty] private OptionDetails? _selectedSelfCareOption;
    [ObservableProperty] private string _selfCareError = string.Empty;

    [ObservableProperty] private string _usualActivitiesLabel = string.Empty;
    [ObservableProperty] private string _usualActivitiesSubLabel = string.Empty;
    [ObservableProperty] private bool _isUsualActivitiesSubLabelVisible;
    [ObservableProperty] private ObservableCollection<OptionDetails> _usualActivitiesOptions = new();
    [ObservableProperty] private OptionDetails? _selectedUsualActivitiesOption;
    [ObservableProperty] private string _usualActivitiesError = string.Empty;

    [ObservableProperty] private string _painLabel = string.Empty;
    [ObservableProperty] private string _painSubLabel = string.Empty;
    [ObservableProperty] private bool _isPainSubLabelVisible;
    [ObservableProperty] private ObservableCollection<OptionDetails> _painOptions = new();
    [ObservableProperty] private OptionDetails? _selectedPainOption;
    [ObservableProperty] private string _painError = string.Empty;

    [ObservableProperty] private string _anxietyDepressionLabel = string.Empty;
    [ObservableProperty] private string _anxietyDepressionSubLabel = string.Empty;
    [ObservableProperty] private bool _isAnxietyDepressionSubLabelVisible;
    [ObservableProperty] private ObservableCollection<OptionDetails> _anxietyDepressionOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAnxietyDepressionOption;
    [ObservableProperty] private string _anxietyDepressionError = string.Empty;

    [ObservableProperty] private string _sliderLabel = string.Empty;
    [ObservableProperty] private string _sliderSubLabel = string.Empty;
    [ObservableProperty] private bool _isSliderSubLabelVisible;
    [ObservableProperty] private string _slider0Label = string.Empty;
    [ObservableProperty] private string _slider100Label = string.Empty;
    [ObservableProperty] private string _sliderError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HealthSliderDisplayText))]
    private double _healthSliderValue = 50;

    public string HealthSliderDisplayText => Math.Round(HealthSliderValue).ToString();

    partial void OnSelectedMobilityOptionChanged(OptionDetails? value) { if (value is not null) MobilityError = string.Empty; }
    partial void OnSelectedSelfCareOptionChanged(OptionDetails? value) { if (value is not null) SelfCareError = string.Empty; }
    partial void OnSelectedUsualActivitiesOptionChanged(OptionDetails? value) { if (value is not null) UsualActivitiesError = string.Empty; }
    partial void OnSelectedPainOptionChanged(OptionDetails? value) { if (value is not null) PainError = string.Empty; }
    partial void OnSelectedAnxietyDepressionOptionChanged(OptionDetails? value) { if (value is not null) AnxietyDepressionError = string.Empty; }
    partial void OnHealthSliderValueChanged(double value) => SliderError = string.Empty;

    private bool ValidateHtSection()
    {
        bool isValid = true;

        if (SelectedMobilityOption is null) { MobilityError = "Select an option"; isValid = false; }
        if (SelectedSelfCareOption is null) { SelfCareError = "Select an option"; isValid = false; }
        if (SelectedUsualActivitiesOption is null) { UsualActivitiesError = "Select an option"; isValid = false; }
        if (SelectedPainOption is null) { PainError = "Select an option"; isValid = false; }
        if (SelectedAnxietyDepressionOption is null) { AnxietyDepressionError = "Select an option"; isValid = false; }

        // Matches the original exactly: this check doesn't fail validation (no isValid =
        // false here) — it can only ever show the error label without blocking Next, and in
        // practice HealthSliderValue is never empty (it starts at 50 and the slider always
        // holds a numeric value), so this is effectively unreachable, same as in the original.
        if (string.IsNullOrEmpty(HealthSliderDisplayText))
        {
            SliderError = "Select an option";
        }

        return isValid;
    }

    private Task AddHtSectionInfoAsync()
    {
        if (SelectedMobilityOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "mobilityfield");
            if (record is not null) record.AnswerId = SelectedMobilityOption.AnswerId;
        }

        if (SelectedSelfCareOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "selfcarefield");
            if (record is not null) record.AnswerId = SelectedSelfCareOption.AnswerId;
        }

        if (SelectedUsualActivitiesOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "usualactivitiesfield");
            if (record is not null) record.AnswerId = SelectedUsualActivitiesOption.AnswerId;
        }

        if (SelectedPainOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "painfield");
            if (record is not null) record.AnswerId = SelectedPainOption.AnswerId;
        }

        if (SelectedAnxietyDepressionOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "anxietydepressionfield");
            if (record is not null) record.AnswerId = SelectedAnxietyDepressionOption.AnswerId;
        }

        var sliderRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "healthvasfield");
        if (sliderRecord is not null) sliderRecord.AnswerValue = Math.Round(HealthSliderValue).ToString();

        return Task.CompletedTask;
    }

    #endregion

    #region Additional Questions section (additionalqstack)
    // Was: validateAQ() / AddAddqInfo() / addqlist_ItemTapped
    //
    // Answering "Yes" here dynamically inserts the whole set of Type=="over16" sections
    // (antiviral/tobacco/alcohol/drug/sleep — the sections after this one) right after
    // wherever the Type=="over16main" section currently sits; "No" removes them again. This
    // is the same dynamic-insert pattern as Gender's menstrual question and Health
    // Conditions'/Medications' follow-up screens.

    [ObservableProperty] private string _additionalQTitle = string.Empty;
    [ObservableProperty] private string _additionalQSubtitle = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _additionalQOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAdditionalQOption;
    [ObservableProperty] private string _additionalQError = string.Empty;

    partial void OnSelectedAdditionalQOptionChanged(OptionDetails? value)
    {
        if (value is not null) AdditionalQError = string.Empty;
    }

    private bool ValidateAdditionalQSection()
    {
        if (SelectedAdditionalQOption is null)
        {
            AdditionalQError = "Select an option";
            return false;
        }

        return true;
    }

    private Task AddAdditionalQSectionInfoAsync()
    {
        if (SelectedAdditionalQOption?.Text.Contains("Yes") == true)
        {
            var over16Sections = RegistrationSectionsNotRequired
                .Where(x => x.Type == "over16")
                .OrderBy(x => int.TryParse(x.Order, out var o) ? o : int.MaxValue)
                .ToList();

            var over16Main = RegistrationSections.FirstOrDefault(x => x.Type == "over16main");
            int index = RegistrationSections.IndexOf(over16Main!);
            // NOTE: if over16main isn't currently in RegistrationSections (e.g. removed for an
            // under-16 user back in AddGenderSectionInfoAsync), IndexOf returns -1 here, same
            // as the original — the items would then insert starting at position 0 rather than
            // after over16main. Carried over as-is rather than guessing a fix, since whether
            // this is reachable at all depends on server-side Required flags per age band that
            // aren't visible from this file alone.

            foreach (var item in over16Sections)
            {
                if (!RegistrationSections.Contains(item))
                {
                    index++;
                    int safeIndex = Math.Clamp(index, 0, RegistrationSections.Count);
                    RegistrationSections.Insert(safeIndex, item);
                }
            }

            RefreshProgress();
        }
        else
        {
            RegistrationSections.RemoveAll(x => x.Type == "over16");
            RefreshProgress();
        }

        if (SelectedAdditionalQOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "addqlifestyle");
            if (record is not null) record.AnswerId = SelectedAdditionalQOption.AnswerId;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Antiviral section (antiviralstack)
    // Was: ValidateAntiViralInfo() / AddAntiViralInfo() / heardavlist_ItemTapped /
    // usedavlist_ItemTapped / futureavlist(2/3/4)_ItemTapped
    //
    // Only AntiviralPrescribed (was: usedavlist) is conditionally revealed (by the "heard of
    // antivirals" gate); the four questions after it (hospital/duration/prevention/side
    // effects) default to visible in the original XAML and nothing ever hides them — they're
    // just the rest of a fixed six-question set, not a cascade. Confirmed by checking their
    // _ItemTapped handlers, which only clear their own error label and never touch another
    // control's visibility.

    [ObservableProperty] private string _antiviralInfoText = string.Empty;

    [ObservableProperty] private string _antiviralHeardLabel = string.Empty;

    [ObservableProperty] private string _antiviralHeardSubLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _antiviralHeardOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAntiviralHeardOption;
    [ObservableProperty] private string _antiviralHeardError = string.Empty;

    [ObservableProperty] private bool _isAntiviralPrescribedVisible;
    [ObservableProperty] private string _antiviralPrescribedLabel = string.Empty;

    [ObservableProperty] private string _antiviralPrescribedSubLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _antiviralPrescribedOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAntiviralPrescribedOption;
    [ObservableProperty] private string _antiviralPrescribedError = string.Empty;

    [ObservableProperty] private string _antiviralHospitalLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _antiviralHospitalOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAntiviralHospitalOption;
    [ObservableProperty] private string _antiviralHospitalError = string.Empty;

    [ObservableProperty] private string _antiviralDurationLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _antiviralDurationOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAntiviralDurationOption;
    [ObservableProperty] private string _antiviralDurationError = string.Empty;

    [ObservableProperty] private string _antiviralPreventionLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _antiviralPreventionOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAntiviralPreventionOption;
    [ObservableProperty] private string _antiviralPreventionError = string.Empty;

    [ObservableProperty] private string _antiviralSideEffectsLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _antiviralSideEffectsOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAntiviralSideEffectsOption;
    [ObservableProperty] private string _antiviralSideEffectsError = string.Empty;

    // antiviralstartedfield / antiviralfinishfield: shown only when prescribed answer is Yes (b_antiviral_2_1)
    [ObservableProperty] private bool _isAntiviralStartedVisible;
    [ObservableProperty] private string _antiviralStartedLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _antiviralStartedOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAntiviralStartedOption;

    [ObservableProperty] private string _antiviralFinishLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _antiviralFinishOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAntiviralFinishOption;

    partial void OnSelectedAntiviralHeardOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        AntiviralHeardError = string.Empty;
        AntiviralPrescribedError = string.Empty;

        IsAntiviralPrescribedVisible = value.Text == "Yes";
        if (!IsAntiviralPrescribedVisible)
        {
            SelectedAntiviralPrescribedOption = null;
        }
    }

    partial void OnSelectedAntiviralPrescribedOptionChanged(OptionDetails? value)
    {
        if (value is not null) AntiviralPrescribedError = string.Empty;

        IsAntiviralStartedVisible = value?.AnswerId == "b_antiviral_2_1";
        if (!IsAntiviralStartedVisible)
        {
            //SelectedAntiviralStartedOption = null;
            //SelectedAntiviralFinishOption = null;
        }
    }
    partial void OnSelectedAntiviralHospitalOptionChanged(OptionDetails? value) { if (value is not null) AntiviralHospitalError = string.Empty; }
    partial void OnSelectedAntiviralDurationOptionChanged(OptionDetails? value) { if (value is not null) AntiviralDurationError = string.Empty; }
    partial void OnSelectedAntiviralPreventionOptionChanged(OptionDetails? value) { if (value is not null) AntiviralPreventionError = string.Empty; }
    partial void OnSelectedAntiviralSideEffectsOptionChanged(OptionDetails? value) { if (value is not null) AntiviralSideEffectsError = string.Empty; }

    private bool ValidateAntiviralSection()
    {
        bool isValid = true;

        if (SelectedAntiviralHeardOption is null) { AntiviralHeardError = "Select an option"; isValid = false; }
        if (IsAntiviralPrescribedVisible && SelectedAntiviralPrescribedOption is null) { AntiviralPrescribedError = "Select an option"; isValid = false; }
        if (SelectedAntiviralHospitalOption is null) { AntiviralHospitalError = "Select an option"; isValid = false; }
        if (SelectedAntiviralDurationOption is null) { AntiviralDurationError = "Select an option"; isValid = false; }
        if (SelectedAntiviralPreventionOption is null) { AntiviralPreventionError = "Select an option"; isValid = false; }
        if (SelectedAntiviralSideEffectsOption is null) { AntiviralSideEffectsError = "Select an option"; isValid = false; }

        return isValid;
    }

    private Task AddAntiviralSectionInfoAsync()
    {
        void SetAnswer(string internalName, OptionDetails? option)
        {
            if (option is null) return;
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == internalName);
            if (record is not null) record.AnswerId = option.AnswerId;
        }

        SetAnswer("antiviralheardfield", SelectedAntiviralHeardOption);
        if (IsAntiviralPrescribedVisible) SetAnswer("antiviralprescribedfield", SelectedAntiviralPrescribedOption);
        SetAnswer("antiviralhospitalfield", SelectedAntiviralHospitalOption);
        SetAnswer("antiviraldurationfield", SelectedAntiviralDurationOption);
        SetAnswer("antiviralpreventionfield", SelectedAntiviralPreventionOption);
        SetAnswer("antiviralsideeffectsfield", SelectedAntiviralSideEffectsOption);
        // not required — only record if answered
        if (IsAntiviralStartedVisible)
        {
            SetAnswer("antiviralstartedfield", SelectedAntiviralStartedOption);
            SetAnswer("antiviralfinishfield", SelectedAntiviralFinishOption);
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Tobacco section (tobaccostack)
    // Was: ValidateTobaccoInfo() / AddTobaccoInfo() / smokelist_ItemTapped /
    // usesmokelistr_ItemTapped / the other four handlers (error-clear only)
    //
    // Two-level cascade, unlike antiviral's flat one-level reveal: "ever smoked" = Yes reveals
    // four fields together (types, start age, currently-smoking, frequency); within that,
    // "currently smoking" = No additionally reveals a fifth (stop age). "Ever smoked" = No
    // hides everything including stop age.

    [ObservableProperty] private string _smokeLabel = string.Empty;
    [ObservableProperty] private string _smokeSubLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _smokeOptions = new();
    [ObservableProperty] private OptionDetails? _selectedSmokeOption;
    [ObservableProperty] private string _smokeError = string.Empty;

    [ObservableProperty] private bool _isSmokeDetailVisible;

    [ObservableProperty] private string _smokeTypesLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _smokeTypesOptions = new();
    public ObservableCollection<OptionDetails>? SelectedSmokeTypesOption { get; set; } = new();
    //[ObservableProperty] private OptionDetails? SelectedSmokeTypesOption ;
    [ObservableProperty] private string _smokeTypesError = string.Empty;

    [ObservableProperty] private string _smokeStartAgeLabel = string.Empty;
    [ObservableProperty] private string _smokeStartAgeText = string.Empty;
    [ObservableProperty] private string _smokeStartAgeErrorText = string.Empty;
    [ObservableProperty] private bool _smokeStartAgeError;

    [ObservableProperty] private string _smokeCurrentLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _smokeCurrentOptions = new();
    [ObservableProperty] private OptionDetails? _selectedSmokeCurrentOption;
    [ObservableProperty] private string _smokeCurrentError = string.Empty;

    [ObservableProperty] private bool _isSmokeStopAgeVisible;
    [ObservableProperty] private string _smokeStopAgeLabel = string.Empty;

    [ObservableProperty] private string _smokeStopAgeErrorText = string.Empty;
    [ObservableProperty] private string _smokeStopAgeText = string.Empty;
    [ObservableProperty] private bool _smokeStopAgeError;

    [ObservableProperty] private string _smokeFreqLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _smokeFreqOptions = new();
    [ObservableProperty] private OptionDetails? _selectedSmokeFreqOption;
    [ObservableProperty] private string _smokeFreqError = string.Empty;

    partial void OnSelectedSmokeOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        SmokeError = string.Empty;

        IsSmokeDetailVisible = value.Text == "Yes";
        if (!IsSmokeDetailVisible)
        {
            IsSmokeStopAgeVisible = false;
            //SelectedSmokeTypesOption = null;
            SmokeStartAgeText = string.Empty;
            SelectedSmokeCurrentOption = null;
            SmokeStopAgeText = string.Empty;
            SelectedSmokeFreqOption = null;
            SmokeTypesError = string.Empty;
            SmokeCurrentError = string.Empty;
            SmokeFreqError = string.Empty;
        }
    }

    //partial void OnSelectedSmokeTypesOptionChanged(OptionDetails? value) { if (value is not null) SmokeTypesError = string.Empty; }
    partial void OnSmokeStartAgeTextChanged(string value) => SmokeStartAgeError = false;

    partial void OnSelectedSmokeCurrentOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        SmokeCurrentError = string.Empty;

        IsSmokeStopAgeVisible = value.Text == "No";
        if (!IsSmokeStopAgeVisible) SmokeStopAgeText = string.Empty;
    }

    partial void OnSmokeStopAgeTextChanged(string value) => SmokeStopAgeError = false;
    partial void OnSelectedSmokeFreqOptionChanged(OptionDetails? value) { if (value is not null) SmokeFreqError = string.Empty; }

    private bool ValidateTobaccoSection()
    {
        bool isValid = true;

        if (SelectedSmokeOption is null) { SmokeError = "Select an option"; isValid = false; }

        if (IsSmokeDetailVisible)
        {
            if (SelectedSmokeTypesOption.Count == 0) { SmokeTypesError = "Select an option"; isValid = false; }
            if (string.IsNullOrWhiteSpace(SmokeStartAgeText)) {SmokeStartAgeErrorText = "Enter a value"; SmokeStartAgeError = true; isValid = false; }
            else
            {
                if(int.TryParse(SmokeStartAgeText, out var startAge))
                {
                    if(startAge <= 0 || startAge > 120) { SmokeStartAgeErrorText = "Enter a valid age"; SmokeStartAgeError = true; isValid = false; }
                }
                else { SmokeStartAgeErrorText = "Enter a valid age"; SmokeStartAgeError = true; isValid = false; }
            }
            if (SelectedSmokeCurrentOption is null) { SmokeCurrentError = "Select an option"; isValid = false; }
            if (IsSmokeStopAgeVisible && string.IsNullOrWhiteSpace(SmokeStopAgeText)) {SmokeStopAgeErrorText = "Enter a value"; SmokeStopAgeError = true; isValid = false; }
             else
            {
                if(int.TryParse(SmokeStopAgeText, out var stopAge))
                {
                    if(stopAge <= 0 || stopAge > 120) { SmokeStopAgeErrorText = "Enter a valid age"; SmokeStopAgeError = true; isValid = false; }
                }
                else { SmokeStopAgeErrorText = "Enter a valid age"; SmokeStopAgeError = true; isValid = false; }
            }
            if (SelectedSmokeFreqOption is null) { SmokeFreqError = "Select an option"; isValid = false; }
        }

        return isValid;
    }

    private Task AddTobaccoSectionInfoAsync()
    {
        void SetAnswer(string internalName, OptionDetails? option)
        {
            if (option is null) return;
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == internalName);
            if (record is not null) record.AnswerId = option.AnswerId;
        }
        void SetAnswers(string internalName, IEnumerable<OptionDetails>? options)
        {
            var selected = options?.ToList();
            if (selected is null || selected.Count == 0) return;
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == internalName);
            if (record is not null) record.AnswerId = string.Join(",", selected.Select(o => o.AnswerId));
        }

        SetAnswer("tobaccoeverfield", SelectedSmokeOption);

        if (IsSmokeDetailVisible)
        {
            SetAnswers("tobaccotypesfield", SelectedSmokeTypesOption);

            var startAgeRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "tobaccostartagefield");
            if (startAgeRecord is not null) startAgeRecord.AnswerValue = SmokeStartAgeText?.Trim() ?? string.Empty;

            SetAnswer("tobaccocurrentfield", SelectedSmokeCurrentOption);

            if (IsSmokeStopAgeVisible)
            {
                var stopAgeRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "tobaccostopagefield");
                if (stopAgeRecord is not null) stopAgeRecord.AnswerValue = SmokeStopAgeText?.Trim() ?? string.Empty;
            }

            SetAnswer("tobaccofreqfield", SelectedSmokeFreqOption);
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Alcohol section (alcoholstack)
    // Was: ValidateAlcoholInfo() / AddAlcoholInfo() / alochollist_ItemTapped /
    // usealochollist_ItemTapped
    //
    // One-level reveal, keyed on specific option text ("Daily"/"Weekly"/"Occasionally")
    // rather than a plain "Yes" — ported as the same text check.

    [ObservableProperty] private string _alcoholFreqLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _alcoholFreqOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAlcoholFreqOption;
    [ObservableProperty] private string _alcoholFreqError = string.Empty;

    [ObservableProperty] private bool _isAlcoholUnitsVisible;
    [ObservableProperty] private string _alcoholUnitsLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _alcoholUnitsOptions = new();
    [ObservableProperty] private OptionDetails? _selectedAlcoholUnitsOption;
    [ObservableProperty] private string _alcoholUnitsError = string.Empty;

    partial void OnSelectedAlcoholFreqOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        AlcoholFreqError = string.Empty;

        IsAlcoholUnitsVisible = value.Text is "Daily" or "Weekly" or "Occasionally";
        if (!IsAlcoholUnitsVisible)
        {
            SelectedAlcoholUnitsOption = null;
            AlcoholUnitsError = string.Empty;
        }
    }

    partial void OnSelectedAlcoholUnitsOptionChanged(OptionDetails? value) { if (value is not null) AlcoholUnitsError = string.Empty; }

    private bool ValidateAlcoholSection()
    {
        bool isValid = true;

        if (SelectedAlcoholFreqOption is null) { AlcoholFreqError = "Select an option"; isValid = false; }
        if (IsAlcoholUnitsVisible && SelectedAlcoholUnitsOption is null) { AlcoholUnitsError = "Select an option"; isValid = false; }

        return isValid;
    }

    private Task AddAlcoholSectionInfoAsync()
    {
        if (SelectedAlcoholFreqOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "alcoholfreqfield");
            if (record is not null) record.AnswerId = SelectedAlcoholFreqOption.AnswerId;
        }

        if (IsAlcoholUnitsVisible && SelectedAlcoholUnitsOption is not null)
        {
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "alcoholunitsfield");
            if (record is not null) record.AnswerId = SelectedAlcoholUnitsOption.AnswerId;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Drug section (drugstack)
    // Was: ValidateDrugInfo() / AddDrugInfo() / druglist_ItemTapped / the other three
    // handlers (error-clear only)
    //
    // One-level reveal like antiviral/health-conditions: "Yes" reveals three fields together.

    [ObservableProperty] private string _drugsLabel = string.Empty;
    [ObservableProperty] private string _drugsSubLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _drugsOptions = new();
    [ObservableProperty] private OptionDetails? _selectedDrugsOption;
    [ObservableProperty] private string _drugsError = string.Empty;

    [ObservableProperty] private bool _isDrugsDetailVisible;

    [ObservableProperty] private string _whatDrugsLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _whatDrugsOptions = new();
    public ObservableCollection<OptionDetails>? SelectedWhatDrugsOption { get; set; } = new();
    //[ObservableProperty] private OptionDetails? _selectedWhatDrugsOption;
    [ObservableProperty] private string _whatDrugsError = string.Empty;

    [ObservableProperty] private string _drugsOftenLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _drugsOftenOptions = new();
    [ObservableProperty] private OptionDetails? _selectedDrugsOftenOption;
    [ObservableProperty] private string _drugsOftenError = string.Empty;

    [ObservableProperty] private string _breathingLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _breathingOptions = new();
    [ObservableProperty] private OptionDetails? _selectedBreathingOption;
    [ObservableProperty] private string _breathingError = string.Empty;

    partial void OnSelectedDrugsOptionChanged(OptionDetails? value)
    {
        if (value is null) return;
        DrugsError = string.Empty;

        IsDrugsDetailVisible = value.Text == "Yes";
        if (!IsDrugsDetailVisible)
        {
            SelectedWhatDrugsOption.Clear();
            SelectedDrugsOftenOption = null;
            SelectedBreathingOption = null;
            WhatDrugsError = string.Empty;
            DrugsOftenError = string.Empty;
            BreathingError = string.Empty;
        }
    }

    //partial void OnSelectedWhatDrugsOptionChanged(OptionDetails? value) { if (value is not null) WhatDrugsError = string.Empty; }
    partial void OnSelectedDrugsOftenOptionChanged(OptionDetails? value) { if (value is not null) DrugsOftenError = string.Empty; }
    partial void OnSelectedBreathingOptionChanged(OptionDetails? value) { if (value is not null) BreathingError = string.Empty; }

    private bool ValidateDrugSection()
    {
        bool isValid = true;

        if (SelectedDrugsOption is null) { DrugsError = "Select an option"; isValid = false; }

        if (IsDrugsDetailVisible)
        {
            if (SelectedWhatDrugsOption is null || SelectedWhatDrugsOption.Count == 0) { WhatDrugsError = "Select an option"; isValid = false; }
            if (SelectedDrugsOftenOption is null) { DrugsOftenError = "Select an option"; isValid = false; }
            if (SelectedBreathingOption is null) { BreathingError = "Select an option"; isValid = false; }
        }

        return isValid;
    }

    private Task AddDrugSectionInfoAsync()
    {
        void SetAnswer(string internalName, OptionDetails? option)
        {
            if (option is null) return;
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == internalName);
            if (record is not null) record.AnswerId = option.AnswerId;
        }

        void SetAnswers(string internalName, IEnumerable<OptionDetails>? options)
        {
            var selected = options?.ToList();
            if (selected is null || selected.Count == 0) return;
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == internalName);
            if (record is not null) record.AnswerId = string.Join(",", selected.Select(o => o.AnswerId));
        }

        SetAnswer("drugseverfield", SelectedDrugsOption);

        if (IsDrugsDetailVisible)
        {
            SetAnswers("drugslistfield", SelectedWhatDrugsOption);
            SetAnswer("drugsfreqfield", SelectedDrugsOftenOption);
            // "drugssymptomsfield" — see the NOTE in ApplyCurrentSectionText's drugstack
            // branch on why this isn't "drugsbreathingfield" (what the original's AddDrugInfo
            // actually reads, which never matches anything registered and silently drops
            // this answer there).
            SetAnswer("drugssymptomsfield", SelectedBreathingOption);
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Sleep section (sleepstack)
    // Was: ValidateSleepInfo() / AddSleepInfo() — eight always-visible questions, no
    // conditional reveals at all (confirmed: none of the eight Validate checks are gated on
    // an .IsVisible condition, unlike every other lifestyle section above).

    [ObservableProperty] private string _sleepOnsetLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _sleepOnsetOptions = new();
    [ObservableProperty] private OptionDetails? _selectedSleepOnsetOption;
    [ObservableProperty] private string _sleepOnsetError = string.Empty;

    [ObservableProperty] private string _wakeLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _wakeOptions = new();
    [ObservableProperty] private OptionDetails? _selectedWakeOption;
    [ObservableProperty] private string _wakeError = string.Empty;

    [ObservableProperty] private string _nightsLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _nightsOptions = new();
    [ObservableProperty] private OptionDetails? _selectedNightsOption;
    [ObservableProperty] private string _nightsError = string.Empty;

    [ObservableProperty] private string _qualityLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _qualityOptions = new();
    [ObservableProperty] private OptionDetails? _selectedQualityOption;
    [ObservableProperty] private string _qualityError = string.Empty;

    [ObservableProperty] private string _moodLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _moodOptions = new();
    [ObservableProperty] private OptionDetails? _selectedMoodOption;
    [ObservableProperty] private string _moodError = string.Empty;

    [ObservableProperty] private string _prodLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _prodOptions = new();
    [ObservableProperty] private OptionDetails? _selectedProdOption;
    [ObservableProperty] private string _prodError = string.Empty;

    [ObservableProperty] private string _poorSleepLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _poorSleepOptions = new();
    [ObservableProperty] private OptionDetails? _selectedPoorSleepOption;
    [ObservableProperty] private string _poorSleepError = string.Empty;

    [ObservableProperty] private string _sleepProbLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<OptionDetails> _sleepProbOptions = new();
    [ObservableProperty] private OptionDetails? _selectedSleepProbOption;
    [ObservableProperty] private string _sleepProbError = string.Empty;

    partial void OnSelectedSleepOnsetOptionChanged(OptionDetails? value) { if (value is not null) SleepOnsetError = string.Empty; }
    partial void OnSelectedWakeOptionChanged(OptionDetails? value) { if (value is not null) WakeError = string.Empty; }
    partial void OnSelectedNightsOptionChanged(OptionDetails? value) { if (value is not null) NightsError = string.Empty; }
    partial void OnSelectedQualityOptionChanged(OptionDetails? value) { if (value is not null) QualityError = string.Empty; }
    partial void OnSelectedMoodOptionChanged(OptionDetails? value) { if (value is not null) MoodError = string.Empty; }
    partial void OnSelectedProdOptionChanged(OptionDetails? value) { if (value is not null) ProdError = string.Empty; }
    partial void OnSelectedPoorSleepOptionChanged(OptionDetails? value) { if (value is not null) PoorSleepError = string.Empty; }
    partial void OnSelectedSleepProbOptionChanged(OptionDetails? value) { if (value is not null) SleepProbError = string.Empty; }

    private bool ValidateSleepSection()
    {
        bool isValid = true;

        if (SelectedSleepOnsetOption is null) { SleepOnsetError = "Select an option"; isValid = false; }
        if (SelectedWakeOption is null) { WakeError = "Select an option"; isValid = false; }
        if (SelectedNightsOption is null) { NightsError = "Select an option"; isValid = false; }
        if (SelectedQualityOption is null) { QualityError = "Select an option"; isValid = false; }
        if (SelectedMoodOption is null) { MoodError = "Select an option"; isValid = false; }
        if (SelectedProdOption is null) { ProdError = "Select an option"; isValid = false; }
        if (SelectedPoorSleepOption is null) { PoorSleepError = "Select an option"; isValid = false; }
        if (SelectedSleepProbOption is null) { SleepProbError = "Select an option"; isValid = false; }

        return isValid;
    }

    private Task AddSleepSectionInfoAsync()
    {
        void SetAnswer(string internalName, OptionDetails? option)
        {
            if (option is null) return;
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == internalName);
            if (record is not null) record.AnswerId = option.AnswerId;
        }

        SetAnswer("sleeponsetfield", SelectedSleepOnsetOption);
        SetAnswer("wakedurationfield", SelectedWakeOption);
        SetAnswer("sleepproblemfreqfield", SelectedNightsOption);
        SetAnswer("sleepqualityfield", SelectedQualityOption);
        SetAnswer("impactmoodfield", SelectedMoodOption);
        SetAnswer("impactprodfield", SelectedProdOption);
        SetAnswer("sleeptroubledfield", SelectedPoorSleepOption);
        SetAnswer("sleepdurationfield", SelectedSleepProbOption);

        return Task.CompletedTask;
    }

    #endregion

    #region Terms & Consent section (termsstack) + Finish screen (finishstack)
    // Was: CheckTermsandConditions() / AddTandCsInfo() / PopulateConsent() /
    // TapGestureRecognizer_Tapped_3/_4 / EmailBorder_Tapped / under10rolelist_ItemTapped /
    // under10otherroleentry_TextChanged / over16nameentry_TextChanged / under10entry_TextChanged /
    // drawingpad_DrawingLineCompleted / signpad_DrawCompleted / Button_Clicked
    //
    // Each consent item's checkbox binds straight to that item's own ChckedState property
    // (ConsentItem, an external model type) — same TwoWay binding the original XAML already
    // used, so no per-item command is needed for the checkbox itself. HasError on each item is
    // a computed property on that same model (recalculated from ShowValidation/required/
    // ChckedState), which is why ValidateTermsSection only needs to set ShowValidation=true,
    // matching the original's CheckTermsandConditions exactly — it never sets HasError
    // directly either.
    //
    // Signature capture is a genuine platform/canvas concern (DrawingView on iOS,
    // SfSignaturePad on Android) that can't move into a ViewModel. NewImperial.xaml.cs calls
    // SetSignatureCaptured(bool) from whichever pad's completed-drawing event fired; getting
    // the actual image bytes for upload happens through RequestSignatureImageStream, which
    // code-behind assigns to a platform-appropriate delegate — used by SubmitAsync (ported
    // alongside CreateAccount, once every section including this one is in place).

    // FIX: this was `public ConsentDetails? AllConsentDetails { get; private set; }` — a plain
    // auto-property, not an [ObservableProperty]. It never raised PropertyChanged, so the XAML
    // binding path "{Binding AllConsentDetails.consentcontent}" evaluated exactly once, while
    // this was still null (LoadConsentAsync runs async and hadn't completed), and then never
    // re-evaluated once LoadConsentAsync actually populated it. The data was always correct —
    // the CollectionView just never found out it had arrived. This is why it looked "missing"
    // rather than "empty": the underlying ItemsSource was frozen at null forever, not at [].
    [ObservableProperty] private ConsentDetails? _allConsentDetails;
    private string _tandCNonRequired = string.Empty;

    [ObservableProperty] private bool _isTcChecked;
    [ObservableProperty] private bool _isTcError;
    [ObservableProperty] private bool _isEmailOptInChecked;

    [ObservableProperty] private bool _isUnder10StackVisible;
    [ObservableProperty] private string _under10NameLabel = string.Empty;
    [ObservableProperty] private string _under10Name = string.Empty;
    [ObservableProperty] private bool _under10NameError;
    [ObservableProperty] private bool _isUnder10NameEditable = true;

    [ObservableProperty] private string _under10RoleLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<SignoffOption> _under10RoleOptions = new();
    [ObservableProperty] private SignoffOption? _selectedUnder10RoleOption;
    [ObservableProperty] private bool _under10RoleError;

    [ObservableProperty] private bool _isUnder10OtherRoleVisible;
    [ObservableProperty] private string _under10OtherRole = string.Empty;
    [ObservableProperty] private bool _under10OtherRoleError;

    [ObservableProperty] private string _over16NameLabel = string.Empty;
    [ObservableProperty] private string _over16Name = string.Empty;
    [ObservableProperty] private bool _over16NameError;
    [ObservableProperty] private bool _isOver16NameEditable = true;

    [ObservableProperty] private string _over16SignatureLabel = string.Empty;
    [ObservableProperty] private bool _isSignatureCaptured;
    [ObservableProperty] private bool _isSignatureError;

    [ObservableProperty] private string _finishText = string.Empty;
    [ObservableProperty] private string _finishSubText = string.Empty;

    /// <summary>Assigned by NewImperial.xaml.cs to read the current platform's signature pad
    /// and return the captured image as a PNG stream. Null until SubmitAsync needs it.</summary>
    public Func<CancellationToken, Task<Stream?>>? RequestSignatureImageStream { get; set; }

    /// <summary>Code-behind hooks this up to clearing whichever signature pad is active for
    /// the current platform (was: Button_Clicked's DeviceInfo.Current.Platform branch).</summary>
    public event Action? SignatureClearRequested;

    [RelayCommand]
    private void ToggleTcChecked()
    {
        IsTcChecked = !IsTcChecked;
        IsTcError = false;
    }

    [RelayCommand]
    private void ToggleEmailOptIn() => IsEmailOptInChecked = !IsEmailOptInChecked;

    /// <summary>Sets ConsentGiven = true ("I consent") on the tapped item.</summary>
    [RelayCommand]
    private void SetConsentGiven(ConsentItem item)
    {
        if (item is not null) item.ConsentGiven = true;
    }

    /// <summary>Sets ConsentGiven = false ("I do not consent") on the tapped item.</summary>
    [RelayCommand]
    private void SetConsentNotGiven(ConsentItem item)
    {
        if (item is not null) item.ConsentGiven = false;
    }

    [RelayCommand]
    private void ClearSignature()
    {
        SignatureClearRequested?.Invoke();
        IsSignatureCaptured = false;
    }

    /// <summary>Called by code-behind from whichever platform's draw-completed event fired.</summary>
    public void SetSignatureCaptured(bool hasData)
    {
        IsSignatureCaptured = hasData;
        if (hasData) IsSignatureError = false;
    }

    partial void OnUnder10NameChanged(string value) => Under10NameError = false;
    partial void OnUnder10OtherRoleChanged(string value) => Under10OtherRoleError = false;
    partial void OnOver16NameChanged(string value) => Over16NameError = false;

    partial void OnSelectedUnder10RoleOptionChanged(SignoffOption? value)
    {
        if (value is null) return;
        Under10RoleError = false;
        IsUnder10OtherRoleVisible = value.label?.Contains("Other") == true;
    }

    private bool ValidateTermsSection()
    {
        bool isValid = true;

        if (AllConsentDetails is not null)
        {
            foreach (var section in AllConsentDetails.consentcontent)
            {
                if (section.sectioncontent is null) continue;
                foreach (var item in section.sectioncontent)
                {
                    item.ShowValidation = true;
                    // Required items are invalid when unanswered (null) OR actively declined (false).
                    // Optional items never block progression regardless of selection.
                    if (item.required && item.ConsentGiven != true) isValid = false;
                }
            }
        }

        if (!IsTcChecked)
        {
            IsTcError = true;
            isValid = false;
        }

        if (IsUnder10StackVisible)
        {
            if (string.IsNullOrEmpty(Under10Name)) { Under10NameError = true; isValid = false; }
            if (SelectedUnder10RoleOption is null) { Under10RoleError = true; isValid = false; }
            if (IsUnder10OtherRoleVisible && string.IsNullOrEmpty(Under10OtherRole)) { Under10OtherRoleError = true; isValid = false; }
        }

        // Matches the original: checked unconditionally, not gated on any "visible" flag.
        if (string.IsNullOrEmpty(Over16Name))
        {
            Over16NameError = true;
            isValid = false;
        }

        if (!IsSignatureCaptured)
        {
            IsSignatureError = true;
            isValid = false;
        }

        return isValid;
    }

    private Task AddTermsSectionInfoAsync()
    {
        // Matches AddTandCsInfo exactly: only the *non-required* consent choices get gathered
        // here, as a pipe-delimited id list. Required consents, the under10/over16 name and
        // role fields, and the signature image itself are all read directly at final
        // submission (SubmitAsync) — same "commit happens later" pattern as mainuserstack.
        if (AllConsentDetails is not null)
        {
            var selectedIds = AllConsentDetails.consentcontent
                .SelectMany(section => section.sectioncontent ?? new ObservableCollection<ConsentItem>())
                .Where(item => item.ConsentGiven == true && !item.required)
                .Select(item => item.consentitemid)
                .ToList();

            _tandCNonRequired = string.Join("|", selectedIds);
        }

        return Task.CompletedTask;
    }

    #endregion
}
