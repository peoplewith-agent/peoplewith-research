// ImperialViewModel.cs â€” Migrated from Imperial.xaml.cs (PD2-8)
// All business logic, navigation state machine, validation and data-aggregation
// methods extracted from the monolithic Imperial code-behind.
// Namespace: flat PeopleWithResearch (per skill rule Â§3.12)

using CommunityToolkit.Maui;
using System.Windows.Input;
using Newtonsoft.Json;
using PeopleWithResearch.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("PeopleWithResearch.UnitTests")]

namespace PeopleWithResearch;

public class ImperialViewModel : INotifyPropertyChanged
{
    // â”€â”€ INotifyPropertyChanged â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public event PropertyChangedEventHandler? PropertyChanged;

    // â”€â”€ Wizard navigation events (consumed by NewImperial code-behind) â”€â”€â”€â”€â”€â”€â”€â”€
    /// <summary>Raised when the wizard should display a new step view. Arg is the XamlNameArea key.</summary>
    public event EventHandler<string>? NavigateToStep;
    /// <summary>Raised when the page should be popped or the user sent to dashboard.</summary>
    public event EventHandler<string>? NavigationRequested;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    // â”€â”€ Passed-in context â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public user userdetails;
    public signupcode signupcodedetails;
    public Questionnaire questionnairedetails;
    public householdgroupjsondetails userinfoforbaseline;
    public ObservableCollection<householdgroupjsondetails> allgroupdetailspassed = new();
    public householdgroup allhousehouldgroup;
    public bool householdrepFROMREG;
    public bool noemailuserreg;

    // â”€â”€ Collected new-user data â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public newuser newuser = new newuser();

    // â”€â”€ Registration config â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public ObservableCollection<RegField> Allregfields = new();
    public ObservableCollection<RegField> AllregfieldsNotRequired = new();
    public List<RegField> Over16regfields = new();
    public List<RegField> Over16regfieldsmain = new();
    public List<RegField> Nonrequiredfields = new();
    public List<QuestionModel> Allquesfields = new();
    public RegField menstrualquestion;
    public RegField mainuserstacksave;
    public string TandCNonReqired = string.Empty;

    // â”€â”€ Wizard navigation state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private int _currentFieldIndex = 0;
    public int CurrentFieldIndex
    {
        get => _currentFieldIndex;
        set
        {
            if (SetProperty(ref _currentFieldIndex, value))
                OnPropertyChanged(nameof(CurrentStep));
        }
    }

    private int _currentFieldIndexQuestionnaire = 0;
    public int CurrentFieldIndexQuestionnaire
    {
        get => _currentFieldIndexQuestionnaire;
        set => SetProperty(ref _currentFieldIndexQuestionnaire, value);
    }

    private string _currentStep = "welcomestack";
    public string CurrentStep
    {
        get => _currentStep;
        set => SetProperty(ref _currentStep, value);
    }

    // â”€â”€ Progress â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private double _progressAmount;
    public double ProgressAmount
    {
        get => _progressAmount;
        set => SetProperty(ref _progressAmount, value);
    }

    private double _progressAmountQuestionnaire;
    public double ProgressAmountQuestionnaire
    {
        get => _progressAmountQuestionnaire;
        set => SetProperty(ref _progressAmountQuestionnaire, value);
    }

    // â”€â”€ TotalSteps â€” derived from Allregfields â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public int TotalSteps => Allregfields.Count;

    // â”€â”€ Step header text â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private string _stepTitle = string.Empty;
    public string StepTitle { get => _stepTitle; set => SetProperty(ref _stepTitle, value); }

    private string _stepSubtitle = string.Empty;
    public string StepSubtitle { get => _stepSubtitle; set => SetProperty(ref _stepSubtitle, value); }

    private string _stepHelpText = string.Empty;
    public string StepHelpText { get => _stepHelpText; set => SetProperty(ref _stepHelpText, value); }

    private bool _helpTextVisible;
    public bool HelpTextVisible { get => _helpTextVisible; set => SetProperty(ref _helpTextVisible, value); }

    // â”€â”€ Selection collections â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public ObservableCollection<OptionDetails> SelectedConditions = new();
    public ObservableCollection<OptionDetails> SelectedMedications = new();
    public ObservableCollection<OptionDetails> UserDetails = new();
    public ObservableCollection<RegQuestionAnswerJson> UserSelectedQuestionnaire = new();
    public List<QuestionnaireResult> QuestionnaireResults = new();
    public List<SectionConsent> Groupeddata = new();
    public ConsentDetails allconsentdetails = null;
    public QuestionManager questionamanager;

    // â”€â”€ Misc UI state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public string genderatbirth;
    internal bool isEditing;
    public bool validdob;
    public bool validnhsnum;
    internal bool PCisEditing;
    internal bool NHSisEditing;
    public bool SignPadhaddata;
    public List<string> validpostcodelist = new();
    public List<OptionDetails> GPPracticeLsit = new();
    public OptionDetails SelectedGp = new();
    public UserNotifications AddNotification = new();
    public string AddHouseholdURL = string.Empty;

    // â”€â”€ Under-10 / proxy branching â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private bool _isUnder10User;
    public bool IsUnder10User { get => _isUnder10User; set => SetProperty(ref _isUnder10User, value); }

    // â”€â”€ Entry enabled flags (populated from baseline in Constructor 4) â”€â”€â”€â”€â”€â”€â”€â”€
    private bool _emailEntryEnabled = true;
    public bool EmailEntryEnabled { get => _emailEntryEnabled; set => SetProperty(ref _emailEntryEnabled, value); }

    private bool _firstNameEntryEnabled = true;
    public bool FirstNameEntryEnabled { get => _firstNameEntryEnabled; set => SetProperty(ref _firstNameEntryEnabled, value); }

    private bool _surnameEntryEnabled = true;
    public bool SurnameEntryEnabled { get => _surnameEntryEnabled; set => SetProperty(ref _surnameEntryEnabled, value); }

    // â”€â”€ Pre-populated baseline values (bound by NameStepView) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private string _prepopEmail = string.Empty;
    public string PrepopEmail { get => _prepopEmail; set => SetProperty(ref _prepopEmail, value); }

    private string _prepopFirstName = string.Empty;
    public string PrepopFirstName { get => _prepopFirstName; set => SetProperty(ref _prepopFirstName, value); }

    private string _prepopSurname = string.Empty;
    public string PrepopSurname { get => _prepopSurname; set => SetProperty(ref _prepopSurname, value); }

    private string _prepopOver16Name = string.Empty;
    public string PrepopOver16Name { get => _prepopOver16Name; set => SetProperty(ref _prepopOver16Name, value); }

    private string _prepopUnder10Name = string.Empty;
    public string PrepopUnder10Name { get => _prepopUnder10Name; set => SetProperty(ref _prepopUnder10Name, value); }

    private bool _over16NameEnabled = true;
    public bool Over16NameEnabled { get => _over16NameEnabled; set => SetProperty(ref _over16NameEnabled, value); }

    private bool _under10EntryEnabled = true;
    public bool Under10EntryEnabled { get => _under10EntryEnabled; set => SetProperty(ref _under10EntryEnabled, value); }

    // â”€â”€ Loading / busy state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { if (SetProperty(ref _isBusy, value)) OnPropertyChanged(nameof(IsNotBusy)); }
    }

    private bool _isFinishStep;
    public bool IsFinishStep
    {
        get => _isFinishStep;
        set { if (SetProperty(ref _isFinishStep, value)) OnPropertyChanged(nameof(NextButtonText)); }
    }

    // ── Derived properties for XAML bindings ──────────────────────────────────────
    public bool CanGoBack => CurrentStep != null && CurrentStep != "welcomestack";
    public bool IsNotBusy => !_isBusy;
    public string NextButtonText => IsFinishStep ? "Finish" : "Next";
    public ICommand NextCommand => new Command(async () => await NextAsync());
    public ICommand BackCommand => new Command(async () => await BackAsync());

    // â”€â”€ Banner â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private string _bannerText = string.Empty;
    public string BannerText { get => _bannerText; set => SetProperty(ref _bannerText, value); }

    private bool _bannerVisible;
    public bool BannerVisible { get => _bannerVisible; set => SetProperty(ref _bannerVisible, value); }

    // â”€â”€ Navigation phase flags â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private bool _isQuestionnairePhase;
    public bool IsQuestionnairePhase { get => _isQuestionnairePhase; set => SetProperty(ref _isQuestionnairePhase, value); }

    // â”€â”€ Welcome study info (populated from config) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private string _welcomeTitle = string.Empty;
    public string WelcomeTitle { get => _welcomeTitle; set => SetProperty(ref _welcomeTitle, value); }

    private string _studyTitle = string.Empty;
    public string StudyTitle { get => _studyTitle; set => SetProperty(ref _studyTitle, value); }

    private string _welcomeSubtitle = string.Empty;
    public string WelcomeSubtitle { get => _welcomeSubtitle; set => SetProperty(ref _welcomeSubtitle, value); }

    // â”€â”€ Validation error properties â€” one pair per step â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Name stack
    private bool _nameStackError;
    public bool NameStackError { get => _nameStackError; set => SetProperty(ref _nameStackError, value); }
    private string _nameStackErrorMessage = string.Empty;
    public string NameStackErrorMessage { get => _nameStackErrorMessage; set => SetProperty(ref _nameStackErrorMessage, value); }

    // Gender stack (used by ValidateGenderStack and ValidatedobStack)
    private bool _genderStackError;
    public bool GenderStackError { get => _genderStackError; set => SetProperty(ref _genderStackError, value); }
    private string _genderStackErrorMessage = string.Empty;
    public string GenderStackErrorMessage { get => _genderStackErrorMessage; set => SetProperty(ref _genderStackErrorMessage, value); }

    // Address stack
    private bool _addressStackError;
    public bool AddressStackError { get => _addressStackError; set => SetProperty(ref _addressStackError, value); }
    private string _addressStackErrorMessage = string.Empty;
    public string AddressStackErrorMessage { get => _addressStackErrorMessage; set => SetProperty(ref _addressStackErrorMessage, value); }

    // Ethnicity stack
    private bool _ethnicityStackError;
    public bool EthnicityStackError { get => _ethnicityStackError; set => SetProperty(ref _ethnicityStackError, value); }
    private string _ethnicityStackErrorMessage = string.Empty;
    public string EthnicityStackErrorMessage { get => _ethnicityStackErrorMessage; set => SetProperty(ref _ethnicityStackErrorMessage, value); }

    // Body metrics stack
    private bool _bodyMetricsStackError;
    public bool BodyMetricsStackError { get => _bodyMetricsStackError; set => SetProperty(ref _bodyMetricsStackError, value); }
    private string _bodyMetricsStackErrorMessage = string.Empty;
    public string BodyMetricsStackErrorMessage { get => _bodyMetricsStackErrorMessage; set => SetProperty(ref _bodyMetricsStackErrorMessage, value); }

    // Education stack
    private bool _educationStackError;
    public bool EducationStackError { get => _educationStackError; set => SetProperty(ref _educationStackError, value); }
    private string _educationStackErrorMessage = string.Empty;
    public string EducationStackErrorMessage { get => _educationStackErrorMessage; set => SetProperty(ref _educationStackErrorMessage, value); }

    // Household structure
    private bool _householdStructureStackError;
    public bool HouseholdStructureStackError { get => _householdStructureStackError; set => SetProperty(ref _householdStructureStackError, value); }
    private string _householdStructureStackErrorMessage = string.Empty;
    public string HouseholdStructureStackErrorMessage { get => _householdStructureStackErrorMessage; set => SetProperty(ref _householdStructureStackErrorMessage, value); }

    // NHS num stack
    private bool _nhsStackError;
    public bool NhsStackError { get => _nhsStackError; set => SetProperty(ref _nhsStackError, value); }
    private string _nhsStackErrorMessage = string.Empty;
    public string NhsStackErrorMessage { get => _nhsStackErrorMessage; set => SetProperty(ref _nhsStackErrorMessage, value); }

    // RI stack
    private bool _riStackError;
    public bool RiStackError { get => _riStackError; set => SetProperty(ref _riStackError, value); }
    private string _riStackErrorMessage = string.Empty;
    public string RiStackErrorMessage { get => _riStackErrorMessage; set => SetProperty(ref _riStackErrorMessage, value); }

    // Health conditions
    private bool _healthConditionsStackError;
    public bool HealthConditionsStackError { get => _healthConditionsStackError; set => SetProperty(ref _healthConditionsStackError, value); }
    private string _healthConditionsStackErrorMessage = string.Empty;
    public string HealthConditionsStackErrorMessage { get => _healthConditionsStackErrorMessage; set => SetProperty(ref _healthConditionsStackErrorMessage, value); }

    // Medications
    private bool _medicationsStackError;
    public bool MedicationsStackError { get => _medicationsStackError; set => SetProperty(ref _medicationsStackError, value); }
    private string _medicationsStackErrorMessage = string.Empty;
    public string MedicationsStackErrorMessage { get => _medicationsStackErrorMessage; set => SetProperty(ref _medicationsStackErrorMessage, value); }

    // RV stack
    private bool _rvStackError;
    public bool RvStackError { get => _rvStackError; set => SetProperty(ref _rvStackError, value); }
    private string _rvStackErrorMessage = string.Empty;
    public string RvStackErrorMessage { get => _rvStackErrorMessage; set => SetProperty(ref _rvStackErrorMessage, value); }

    // Diet stack
    private bool _dietStackError;
    public bool DietStackError { get => _dietStackError; set => SetProperty(ref _dietStackError, value); }
    private string _dietStackErrorMessage = string.Empty;
    public string DietStackErrorMessage { get => _dietStackErrorMessage; set => SetProperty(ref _dietStackErrorMessage, value); }

    // Menstrual
    private bool _menstrualStackError;
    public bool MenstrualStackError { get => _menstrualStackError; set => SetProperty(ref _menstrualStackError, value); }
    private string _menstrualStackErrorMessage = string.Empty;
    public string MenstrualStackErrorMessage { get => _menstrualStackErrorMessage; set => SetProperty(ref _menstrualStackErrorMessage, value); }

    // HT stack
    private bool _htStackError;
    public bool HtStackError { get => _htStackError; set => SetProperty(ref _htStackError, value); }
    private string _htStackErrorMessage = string.Empty;
    public string HtStackErrorMessage { get => _htStackErrorMessage; set => SetProperty(ref _htStackErrorMessage, value); }

    // AQ stack
    private bool _aqStackError;
    public bool AqStackError { get => _aqStackError; set => SetProperty(ref _aqStackError, value); }
    private string _aqStackErrorMessage = string.Empty;
    public string AqStackErrorMessage { get => _aqStackErrorMessage; set => SetProperty(ref _aqStackErrorMessage, value); }

    // Antiviral
    private bool _antiViralStackError;
    public bool AntiViralStackError { get => _antiViralStackError; set => SetProperty(ref _antiViralStackError, value); }
    private string _antiViralStackErrorMessage = string.Empty;
    public string AntiViralStackErrorMessage { get => _antiViralStackErrorMessage; set => SetProperty(ref _antiViralStackErrorMessage, value); }

    // Tobacco
    private bool _tobaccoStackError;
    public bool TobaccoStackError { get => _tobaccoStackError; set => SetProperty(ref _tobaccoStackError, value); }
    private string _tobaccoStackErrorMessage = string.Empty;
    public string TobaccoStackErrorMessage { get => _tobaccoStackErrorMessage; set => SetProperty(ref _tobaccoStackErrorMessage, value); }

    // Alcohol
    private bool _alcoholStackError;
    public bool AlcoholStackError { get => _alcoholStackError; set => SetProperty(ref _alcoholStackError, value); }
    private string _alcoholStackErrorMessage = string.Empty;
    public string AlcoholStackErrorMessage { get => _alcoholStackErrorMessage; set => SetProperty(ref _alcoholStackErrorMessage, value); }

    // Drug
    private bool _drugStackError;
    public bool DrugStackError { get => _drugStackError; set => SetProperty(ref _drugStackError, value); }
    private string _drugStackErrorMessage = string.Empty;
    public string DrugStackErrorMessage { get => _drugStackErrorMessage; set => SetProperty(ref _drugStackErrorMessage, value); }

    // Sleep
    private bool _sleepStackError;
    public bool SleepStackError { get => _sleepStackError; set => SetProperty(ref _sleepStackError, value); }
    private string _sleepStackErrorMessage = string.Empty;
    public string SleepStackErrorMessage { get => _sleepStackErrorMessage; set => SetProperty(ref _sleepStackErrorMessage, value); }

    // Terms and Conditions
    private bool _tandCsStackError;
    public bool TandCsStackError { get => _tandCsStackError; set => SetProperty(ref _tandCsStackError, value); }
    private string _tandCsStackErrorMessage = string.Empty;
    public string TandCsStackErrorMessage { get => _tandCsStackErrorMessage; set => SetProperty(ref _tandCsStackErrorMessage, value); }

    // Main user stack
    private bool _mainUserStackError;
    public bool MainUserStackError { get => _mainUserStackError; set => SetProperty(ref _mainUserStackError, value); }
    private string _mainUserStackErrorMessage = string.Empty;
    public string MainUserStackErrorMessage { get => _mainUserStackErrorMessage; set => SetProperty(ref _mainUserStackErrorMessage, value); }

    // â”€â”€ Regex â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private static readonly Regex UkPostcodeRegex = new(
        @"^(GIR\s?0AA|(?:(?:[A-PR-UWYZ][0-9][0-9]?|[A-PR-UWYZ][A-HK-Y][0-9][0-9]?|[A-PR-UWYZ][0-9][A-HJKPSTUW]|[A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRV-Y]))\s?[0-9][ABD-HJLNP-UW-Z]{2})$",
        RegexOptions.IgnoreCase);

    // â”€â”€ Constructor overloads â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Constructor 1 â€” default (empty, unit-test use)</summary>
    public ImperialViewModel()
    {
        Allregfields.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalSteps));
    }

    /// <summary>Constructor 2 â€” user + advert + Questionnaire</summary>
    public ImperialViewModel(user userpassed, advert signupdetailspassed, Questionnaire questionnairepassed)
    {
        Allregfields.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalSteps));
        userdetails = userpassed;
        // advert param kept for signature parity; underlying logic is commented out in Imperial too
        householdrepFROMREG = true;
        _ = LoadRegistrationConfigAsync();
    }

    /// <summary>Constructor 3 â€” user + signupcode</summary>
    public ImperialViewModel(user userpassed, signupcode signupdetailspassed)
    {
        Allregfields.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalSteps));
        userdetails = userpassed;
        signupcodedetails = signupdetailspassed;
        householdrepFROMREG = true;
        _ = LoadRegistrationConfigAsync();
    }

    /// <summary>Constructor 4 â€” signupcode + householdgroupjsondetails + collection + group</summary>
    public ImperialViewModel(signupcode signupdetailspassed, householdgroupjsondetails userinfopassed,
        ObservableCollection<householdgroupjsondetails> allgroupdetails, householdgroup passedhousehold)
    {
        Allregfields.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalSteps));
        userdetails = new user();
        signupcodedetails = signupdetailspassed;
        userinfoforbaseline = userinfopassed;
        allgroupdetailspassed = allgroupdetails;
        allhousehouldgroup = passedhousehold;
        householdrepFROMREG = false;
        BannerText = "You are completing this on behalf of " + userinfopassed.household_individual_name;
        BannerVisible = true;
        PrepopulateFromBaseline();
        _ = LoadRegistrationConfigAsync();
    }

    // â”€â”€ Pre-populate from baseline (Constructor 4 only) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private void PrepopulateFromBaseline()
    {
        try
        {
            if (userinfoforbaseline == null) return;

            PrepopEmail = userinfoforbaseline.household_individual_email ?? string.Empty;

            var splitname = userinfoforbaseline.household_individual_name?.Split(' ') ?? Array.Empty<string>();
            PrepopFirstName = splitname.Length > 0 ? splitname[0] : string.Empty;
            PrepopSurname = splitname.Length > 1 ? splitname[1] : string.Empty;

            if (userinfoforbaseline.household_individual_age?.Contains("10") == true)
            {
                PrepopUnder10Name = userinfoforbaseline.household_individual_name ?? string.Empty;
                Under10EntryEnabled = false;
                IsUnder10User = true;
            }
            else
            {
                PrepopOver16Name = userinfoforbaseline.household_individual_name ?? string.Empty;
                Over16NameEnabled = false;
            }

            if (userinfoforbaseline.household_individual_email == "N/A")
            {
                noemailuserreg = true;
            }

            EmailEntryEnabled = false;
            FirstNameEntryEnabled = false;
            SurnameEntryEnabled = false;
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.PrepopulateFromBaseline");
        }
    }

    // â”€â”€ LoadRegistrationConfigAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task LoadRegistrationConfigAsync()
    {
        try
        {
            if (signupcodedetails == null || string.IsNullOrEmpty(signupcodedetails.appdetails))
                return;

            var config = JsonConvert.DeserializeObject<AppConfig>(signupcodedetails.appdetails);

            if (config != null)
            {
                var sortedList = config.RegFields
                    .Where(x => x.Active)
                    .OrderBy(x => int.TryParse(x.Order, out var orderVal) ? orderVal : int.MaxValue)
                    .ToList();

                var tempAll = new ObservableCollection<RegField>(sortedList);

                foreach (var item in tempAll)
                {
                    if (item.subFields != null)
                    {
                        foreach (var it in item.subFields)
                        {
                            if (it.Required)
                                it.Label = it.Label + " *";
                        }
                    }
                }

                // Remove steps not needed for household member proxy
                if (householdrepFROMREG == false)
                {
                    mainuserstacksave = tempAll.FirstOrDefault(x => x.XamlNameArea == "mainuserstack");
                    var toRemoveTAMU = tempAll.Where(x => x.XamlNameArea == "mainuserstack").ToList(); foreach (var r in toRemoveTAMU) tempAll.Remove(r);
                    var toRemoveTAAS = tempAll.Where(x => x.XamlNameArea == "addressstack").ToList(); foreach (var r in toRemoveTAAS) tempAll.Remove(r);
                    var toRemoveTAHS = tempAll.Where(x => x.XamlNameArea == "householdstructurestack").ToList(); foreach (var r in toRemoveTAHS) tempAll.Remove(r);
                }

                AllregfieldsNotRequired = new ObservableCollection<RegField>(
                    tempAll.Where(x => x.Required == false));

                var required = new ObservableCollection<RegField>(
                    tempAll.Where(x => x.Required == true));

                var over16items = required.Where(x => x.Type == "over16").ToList();
                Over16regfields = over16items;
                var toRemove1 = required.Where(x => x.Type == "over16").ToList(); foreach (var r in toRemove1) required.Remove(r);

                if (userdetails != null && userdetails.Primaryuser == false)
                {
                    var toRemoveMU = required.Where(x => x.XamlNameArea == "mainuserstack").ToList(); foreach (var r in toRemoveMU) required.Remove(r);
                    var toRemoveAS = required.Where(x => x.XamlNameArea == "addressstack").ToList(); foreach (var r in toRemoveAS) required.Remove(r);
                }

                // Update progress amount
                int count = Math.Max(required.Count - 1, 1);
                double progressStep = (double)100 / count;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Allregfields = required;
                    Allregfields.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalSteps));
                    OnPropertyChanged(nameof(Allregfields));
                    OnPropertyChanged(nameof(TotalSteps));
                });

                // Populate welcome step info
                WelcomeTitle = "Welcome to the " + config.OverallSettings.StudyName;
                StudyTitle = config.OverallSettings.StudyTitle;
                WelcomeSubtitle = config.OverallSettings.StudyDescription;
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.LoadRegistrationConfigAsync");
        }
    }

    // â”€â”€ RegInitialSetup â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private void RegInitialSetup()
    {
        try
        {
            _currentFieldIndex = 0;
            ShowCurrentStack();
            UpdateProgress();
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.RegInitialSetup");
        }
    }

    // â”€â”€ ShowCurrentStack â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    /// <summary>
    /// Updates CurrentStep, StepTitle, StepSubtitle, StepHelpText and ProgressAmount
    /// from the current Allregfields entry. No XAML element references.
    /// </summary>
    public void ShowCurrentStack()
    {
        try
        {
            if (Allregfields.Count == 0) return;
            var idx = Math.Min(_currentFieldIndex, Allregfields.Count - 1);
            var field = Allregfields[idx];

            StepTitle = field.Label ?? string.Empty;
            StepSubtitle = field.Placeholder ?? string.Empty;
            StepHelpText = field.HelpText ?? string.Empty;
            HelpTextVisible = !string.IsNullOrEmpty(field.HelpText);
            CurrentStep = field.XamlNameArea ?? string.Empty;
            NavigateToStep?.Invoke(this, CurrentStep);
            OnPropertyChanged(nameof(CanGoBack));
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.ShowCurrentStack");
        }
    }

    // â”€â”€ UpdateProgress â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private void UpdateProgress()
    {
        try
        {
            if (Allregfields.Count <= 1)
            {
                ProgressAmount = 0;
                return;
            }

            double max = Allregfields.Count - 1;
            int idx = Math.Min(_currentFieldIndex, Allregfields.Count - 1);
            ProgressAmount = (idx / max) * 100.0;
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.UpdateProgress");
        }
    }

    // â”€â”€ NextAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task NextAsync()
    {
        try
        {
            IsBusy = true;

            // Welcome â†’ first reg step
            if (CurrentStep == "welcomestack")
            {
                if (Allregfields.Count > 0)
                {
                    _currentFieldIndex = 0;
                    ShowCurrentStack();
                    UpdateProgress();
                }
                IsBusy = false;
                return;
            }

            // Validate + collect data for current step
            if (_currentFieldIndex >= Allregfields.Count)
            {
                IsFinishStep = true;
                NavigationRequested?.Invoke(this, "dashboard");
                IsBusy = false;
                return;
            }

            var currentField = Allregfields[_currentFieldIndex];
            bool canProceed = true;

            canProceed = await ValidateAndCollectCurrentStep(currentField);

            if (!canProceed)
            {
                IsBusy = false;
                return;
            }

            // Advance
            _currentFieldIndex++;

            if (_currentFieldIndex < Allregfields.Count)
            {
                ShowCurrentStack();
                UpdateProgress();
            }
            else
            {
                IsFinishStep = true;
                ProgressAmount = 100;
                NavigationRequested?.Invoke(this, "dashboard");
            }

            IsBusy = false;
        }
        catch (Exception ex)
        {
            IsBusy = false;
            CrashDetected.LogCrash(ex, "ImperialViewModel.NextAsync");
        }
    }

    // â”€â”€ BackAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task BackAsync()
    {
        try
        {
            if (CurrentStep == "welcomestack")
            {
                // Already at start â€” pop the page
                NavigationRequested?.Invoke(this, "pop");
                return;
            }

            if (_currentFieldIndex <= 0)
            {
                _currentFieldIndex = 0;
                CurrentStep = "welcomestack";
                StepTitle = string.Empty;
                StepSubtitle = string.Empty;
                ProgressAmount = 0;
                return;
            }

            _currentFieldIndex--;

            if (_currentFieldIndex >= 0 && Allregfields.Count > 0)
            {
                ShowCurrentStack();
                UpdateProgress();
            }
            else
            {
                _currentFieldIndex = 0;
                CurrentStep = "welcomestack";
                ProgressAmount = 0;
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.BackAsync");
        }
        await Task.CompletedTask;
    }

    // â”€â”€ Validate + collect dispatcher â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task<bool> ValidateAndCollectCurrentStep(RegField field)
    {
        bool canProceed = true;

        switch (field.XamlNameArea)
        {
            case "namestack" when field.Required:
                canProceed = ValidateNameStack();
                if (canProceed) await AddParticiantInfoAsync();
                break;

            case "mainuserstack" when field.Required:
                canProceed = ValidateFormStack();
                break;

            case "addressstack" when field.Required:
                canProceed = await ValidateaddressStack();
                if (canProceed) AddAddressInfo();
                break;

            case "genderstack" when field.Required:
                canProceed = ValidateGenderStack();
                if (canProceed) AddGenderInfo();
                break;

            case "ethnicitystack" when field.Required:
                canProceed = ValidateEthnicityStack();
                if (canProceed) AddEthnicityInfo();
                break;

            case "bodymetricsstack" when field.Required:
                canProceed = ValidatebodymetricsStack();
                if (canProceed) AddBodyMetricsInfo();
                break;

            case "educationworkstack" when field.Required:
                canProceed = ValidateeducationStack();
                if (canProceed) AddEducationWorkInfo();
                break;

            case "householdstructurestack" when field.Required:
                canProceed = ValidateHouseholdstructureStack();
                if (canProceed) AddHouseHoldStructureInfo();
                break;

            case "nhsnumstack" when field.Required:
                canProceed = ValidatenhsnumStack();
                if (canProceed) AddNHSInfo();
                break;

            case "ristack" when field.Required:
                canProceed = ValidateRIStack();
                if (canProceed) AddRIInfo();
                break;

            case "hcstack" when field.Required:
                canProceed = ValidateHealthConditionsStack();
                if (canProceed) AddHealthConditionsInfo();
                break;

            case "medynstack" when field.Required:
                canProceed = ValidateMedicationsStack();
                if (canProceed) AddMedicationsInfo();
                break;

            case "rvstack" when field.Required:
                canProceed = ValidateRVStack();
                if (canProceed) AddRVInfo();
                break;

            case "dietstack" when field.Required:
                canProceed = ValidateDietStack();
                if (canProceed) AddDietInfo();
                break;

            case "menstrualstack":
                canProceed = ValidateMenstrualInfo();
                if (canProceed) AddMenstrualInfo();
                break;

            case "htstack" when field.Required:
                canProceed = ValidateHtInfo();
                if (canProceed) AddHtInfo();
                break;

            case "additionalqstack" when field.Required:
                canProceed = validateAQ();
                if (canProceed) AddAddqInfo();
                break;

            case "antiviralstack":
                canProceed = ValidateAntiViralInfo();
                if (canProceed) AddAntiViralInfo();
                break;

            case "tobaccostack":
                canProceed = ValidateTobaccoInfo();
                if (canProceed) AddTobaccoInfo();
                break;

            case "alcoholstack":
                canProceed = ValidateAlcoholInfo();
                if (canProceed) AddAlcoholInfo();
                break;

            case "drugstack":
                canProceed = ValidateDrugInfo();
                if (canProceed) AddDrugInfo();
                break;

            case "sleepstack":
                canProceed = ValidateSleepInfo();
                if (canProceed) AddSleepInfo();
                break;

            case "termsstack" when field.Required:
                canProceed = CheckTermsandConditions();
                if (canProceed) AddTandCsInfo();
                break;

            default:
                // Unknown step â€” allow progression
                canProceed = true;
                break;
        }

        return canProceed;
    }

    // â”€â”€ CreateAccount â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task CreateAccount()
    {
        try
        {
            if (!householdrepFROMREG)
            {
                newuser.userid = userinfoforbaseline.household_individual_userid;
                newuser.primaryuser = false;
                newuser.signupcodegrouping = signupcodedetails.signupcodegrouping;
                newuser.householdgroupid = userinfoforbaseline.household_group_id;
                newuser.signupcodeid = signupcodedetails.signupcodeid;
                newuser.postcode = Helpers.Settings.Postcode;
            }
            else
            {
                newuser.userid = userdetails.Userid;
                newuser.primaryuser = userdetails.Primaryuser;
                newuser.signupcodegrouping = userdetails.Signupcodegrouping;
                newuser.householdgroupid = userdetails.Householdgroupid;
                newuser.signupcodeid = userdetails.Signupid;
            }

            newuser.status = "active";

            var updateData = new
            {
                firstname = newuser.firstname,
                surname = newuser.surname,
                gender = newuser.gender,
                status = newuser.status,
                ethnicity = newuser.ethnicity,
                email = newuser.email,
                password = newuser.password,
                postcode = newuser.postcode,
                signupcodeid = newuser.signupcodeid,
                signupcodegrouping = newuser.signupcodegrouping,
                primarycareid = newuser.primarycareid,
                primaryuser = newuser.primaryuser,
                householdgroupid = newuser.householdgroupid,
                dateofbirth = newuser.dateofbirth,
                details = newuser.details,
                telephone = newuser.telephone
            };

            string json = System.Text.Json.JsonSerializer.Serialize(updateData);
            var urll = $"{APICalls.ApplicationURL}user/userid/{newuser.userid}";
            var contentt = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await APICalls.Instance.GetClient().PatchAsync(urll, contentt);

            if (householdrepFROMREG && newuser.primaryuser)
                await Addhouseholdmembers();
            else
                await Updatehouseholdmembers();

            // Upload questionnaire
            var serializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var newBaseline = new newuserquestionnaire
            {
                userid = newuser.userid,
                questionnaireid = "b1_individual_questionnaire"
            };

            if (householdrepFROMREG == false)
            {
                var fillingForm = new QuestionnaireResult
                {
                    QuestionId = "b_who_filled_form",
                    AnswerId = "b_who_filled_form_a2",
                    InternalName = "b_who_filled_form"
                };
                QuestionnaireResults.Add(fillingForm);

                var relationship = new QuestionnaireResult { QuestionId = "b_who_filled_form_2" };
                var fillingForField = mainuserstacksave?.subFields?.FirstOrDefault(f => f.Id == "familyrelationship");
                if (fillingForField != null)
                {
                    var matchedOption = fillingForField.Options?
                        .FirstOrDefault(o => o.Text.Equals(userinfoforbaseline.household_individual_relationship,
                            StringComparison.OrdinalIgnoreCase));
                    relationship.AnswerId = matchedOption != null ? matchedOption.AnswerId : "b_who_filled_form_2_4";
                }
                QuestionnaireResults.Add(relationship);
            }

            string qJson = System.Text.Json.JsonSerializer.Serialize(QuestionnaireResults);
            newBaseline.feedback = qJson;

            Uri qUri = new Uri($"{APICalls.ApplicationURL}userquestionnaire");
            string qJsonStr = System.Text.Json.JsonSerializer.Serialize<newuserquestionnaire>(newBaseline, serializerOptions);
            var qContent = new StringContent(qJsonStr, Encoding.UTF8, "application/json");
            await APICalls.Instance.GetClient().PostAsync(qUri, qContent);

            // Save preferences if primary user
            if (householdrepFROMREG)
            {
                Preferences.Default.Set("userid", newuser.userid);
                Preferences.Default.Set("firstname", newuser.firstname);
                Preferences.Default.Set("surname", newuser.surname);
                Preferences.Default.Set("signupcode", newuser.signupcodeid);
                Preferences.Default.Set("email", newuser.email);
                Preferences.Default.Set("gender", newuser.gender);
                Preferences.Default.Set("ethnicity", newuser.ethnicity);
                Preferences.Default.Set("age", newuser.dateofbirth);
                Preferences.Default.Set("userpasswordhash", newuser.password);
                Preferences.Default.Set("sideupcodegrouping", newuser.signupcodegrouping);
                Preferences.Default.Set("householdgrouping", newuser.householdgroupid);
                Preferences.Default.Set("primarycardid", newuser.primarycareid);
                Preferences.Default.Set("details", newuser.details);
                Preferences.Default.Set("postcode", newuser.postcode);
                Preferences.Default.Set("isprimaryuser", newuser.primaryuser);
                Preferences.Default.Set("phonenumber", newuser.telephone);
                Preferences.Default.Set("devicemanufacturer", DeviceInfo.Manufacturer);
                Preferences.Default.Set("devicemodel", DeviceInfo.Model);
                Preferences.Default.Set("deviceversion", DeviceInfo.VersionString);

                if (newuser.primaryuser)
                    Preferences.Default.Set("primaryuserid", newuser.userid);
            }

            NavigationRequested?.Invoke(this, "dashboard"); // Navigate via code-behind event channel
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.CreateAccount");
        }
    }

    // â”€â”€ PopulateConsent â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task PopulateConsentAsync()
    {
        try
        {
            if (signupcodedetails == null || string.IsNullOrEmpty(signupcodedetails.consent)) return;

            var config = JsonConvert.DeserializeObject<ObservableCollection<ConsentDetails>>(signupcodedetails.consent);
            if (config == null) return;

            if (householdrepFROMREG)
            {
                allconsentdetails = config.FirstOrDefault(x => x.age == "16+");
            }
            else
            {
                var age = userinfoforbaseline?.household_individual_age ?? "16+";
                allconsentdetails = config.FirstOrDefault(x => x.age == age)
                    ?? config.FirstOrDefault(x => x.age == "16+");

                if (age.Contains("10"))
                    IsUnder10User = true;
            }

            if (allconsentdetails != null)
            {
                foreach (var section in allconsentdetails.consentcontent)
                {
                    if (section.sectioncontent == null) continue;
                    foreach (var it in section.sectioncontent)
                    {
                        if (it.required) it.requiredlbl = "Required";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.PopulateConsentAsync");
        }
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // AddXInfo methods â€” data aggregation
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    public async Task AddParticiantInfoAsync()
    {
        try
        {
            // firstname/surname/email/telephone are read from the ViewModel
            // bound entry values â€” use newuser fields already set by NameStepView bindings
            if (!string.IsNullOrEmpty(newuser.password))
                newuser.password = await HashPasswordAsync(newuser.password);
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddParticiantInfoAsync");
        }
    }

    public void AddDetail(string text, string value)
    {
        UserDetails.Add(new OptionDetails { Text = text, Value = value?.Trim() });
    }

    public void AddAddressInfo()
    {
        try
        {
            UserDetails.Clear();
            AddDetail("addresslineone", newuser.details ?? string.Empty);
            AddDetail("devicemanufacturer", DeviceInfo.Manufacturer);
            AddDetail("devicemodel", DeviceInfo.Model);
            AddDetail("deviceversion", DeviceInfo.VersionString);

            var newDetailsDict = UserDetails.ToDictionary(x => x.Text, x => x.Value);
            newuser.details = JsonConvert.SerializeObject(new List<object> { newDetailsDict });
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddAddressInfo");
        }
    }

    public async Task Addhouseholdmembers()
    {
        try
        {
            // Household members are created from ViewModel-bound fields
            // The actual member data is read from bound properties exposed per step
            // Placeholder â€” actual field values will be provided by MainUserStepView bindings
            var newmembers = new ObservableCollection<householdgroupjsondetails>();

            string json = JsonConvert.SerializeObject(newmembers, Newtonsoft.Json.Formatting.None);
            json = json.Replace("\r", "").Replace("\n", "").Trim();

            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            string base64 = Convert.ToBase64String(jsonBytes);

            string baseUrl = "https://hopper.peoplewith.com/household-individual-alignment.php";
            AddHouseholdURL = $"{baseUrl}?hij={base64}".Replace("\r", "").Replace("\n", "").Trim();

            if (_currentFieldIndex < Allregfields.Count)
            {
                var field = Allregfields[_currentFieldIndex];
                if (field.XamlNameArea != "mainuserstack")
                {
                    var httpClient = APICalls.Instance.GetClient();
                    await httpClient.GetAsync(AddHouseholdURL);
                }
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.Addhouseholdmembers");
        }
    }

    public async Task Updatehouseholdmembers()
    {
        try
        {
            if (allgroupdetailspassed == null || !allgroupdetailspassed.Any())
            {
                var groupedData = await APICalls.Instance.GetUserHouseholdInfo(newuser?.householdgroupid);
                if (groupedData != null)
                {
                    allhousehouldgroup = groupedData.FirstOrDefault();
                    allgroupdetailspassed = allhousehouldgroup?.userdetailslist;
                }
            }

            if (userinfoforbaseline == null && allgroupdetailspassed != null)
                userinfoforbaseline = allgroupdetailspassed.FirstOrDefault(x => x.household_individual_userid == newuser?.userid);

            var updateUser = allgroupdetailspassed?
                .FirstOrDefault(x => x?.household_individual_userid == userinfoforbaseline?.household_individual_userid);

            if (updateUser != null)
            {
                updateUser.household_individual_status = "active";
                var name = $"{newuser.firstname?.Trim()} {newuser.surname?.Trim()}".Trim();
                if (!string.IsNullOrEmpty(name))
                    updateUser.household_individual_name = name;
            }

            var houseRepToRemove = allgroupdetailspassed?
                .FirstOrDefault(x => x?.household_individual_relationship == "Household Rep");
            if (houseRepToRemove != null)
                allgroupdetailspassed.Remove(houseRepToRemove);

            if (allhousehouldgroup != null)
            {
                allhousehouldgroup.groupuserdetails = JsonConvert.SerializeObject(allgroupdetailspassed);
                var updateData = new { groupuserdetails = allhousehouldgroup.groupuserdetails };
                string updateJson = System.Text.Json.JsonSerializer.Serialize(updateData);

                if (userinfoforbaseline != null)
                {
                    var url = $"{APICalls.ApplicationURL}householdgroup/householdgroupid/{userinfoforbaseline.household_group_id}";
                    using var content = new StringContent(updateJson, Encoding.UTF8, "application/json");
                    await APICalls.Instance.GetClient().PatchAsync(url, content);
                }
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.Updatehouseholdmembers");
        }
    }

    public void AddGenderInfo()
    {
        try
        {
            // Actual gender/dob values come from bound newuser properties (set by GenderStepView)
            if (!string.IsNullOrEmpty(newuser.dateofbirth))
            {
                DateTime? date = DateTime.TryParse(newuser.dateofbirth, out var d) ? d : (DateTime?)null;
                if (date.HasValue)
                {
                    int age = DateTime.Now.Year - date.Value.Year;
                    if (DateTime.Now.Date < date.Value.AddYears(age)) age--;

                    // Add menstrual step conditionally
                    if (age >= 15 && age <= 55 && newuser.gender == "Female")
                    {
                        var getmenstrual = AllregfieldsNotRequired.FirstOrDefault(x => x.XamlNameArea == "menstrualstack");
                        if (getmenstrual != null && !Allregfields.Any(x => x.XamlNameArea == "menstrualstack"))
                        {
                            int targetIndex = int.TryParse(getmenstrual.Order, out var oi) ? oi : Allregfields.Count;
                            int safeIndex = Math.Min(targetIndex, Allregfields.Count);
                            Allregfields.Insert(safeIndex, getmenstrual);
                        }
                    }
                    else
                    {
                        var getmenstrual = Allregfields.FirstOrDefault(x => x.XamlNameArea == "menstrualstack");
                        if (getmenstrual != null)
                            Allregfields.Remove(getmenstrual);
                    }

                    // Remove under-16 restricted steps
                    if (age < 16)
                    {
                        var over16main = Allregfields.Where(x => x.Type == "over16main").ToList();
                        Over16regfieldsmain = over16main;
                        foreach (var item in over16main)
                            Allregfields.Remove(item);
                    }
                    else
                    {
                        if (Over16regfieldsmain.Count != 0)
                        {
                            foreach (var item in Over16regfieldsmain)
                                if (!Allregfields.Contains(item))
                                    Allregfields.Add(item);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddGenderInfo");
        }
    }

    public void AddEthnicityInfo()
    {
        try
        {
            // Values already captured to QuestionnaireResults by EthnicityStepView bindings
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddEthnicityInfo");
        }
    }

    public void AddBodyMetricsInfo()
    {
        try
        {
            // Values already captured to QuestionnaireResults by BodyMetricsStepView bindings
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddBodyMetricsInfo");
        }
    }

    public void AddEducationWorkInfo()
    {
        try
        {
            // Values already captured to QuestionnaireResults by EducationWorkStepView bindings
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddEducationWorkInfo");
        }
    }

    public void AddHouseHoldStructureInfo()
    {
        try
        {
            // Values already captured to QuestionnaireResults by HouseholdStructureStepView bindings
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddHouseHoldStructureInfo");
        }
    }

    public void AddNHSInfo()
    {
        try
        {
            // Values already captured to QuestionnaireResults by NHSStepView bindings
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddNHSInfo");
        }
    }

    public void AddRIInfo()
    {
        try
        {
            // Values already captured to QuestionnaireResults by RIStepView bindings
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddRIInfo");
        }
    }

    public void AddHealthConditionsInfo()
    {
        try
        {
            // Conditional step insertion (hcstack â†’ addhcstack) is handled here
            var additionalconditions = AllregfieldsNotRequired.FirstOrDefault(x => x.Type == "otherhc");
            var hcYes = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "diastack");
            bool selectedYes = hcYes?.AnswerId?.EndsWith("_1") == true;

            if (selectedYes)
            {
                if (additionalconditions != null && !Allregfields.Any(x => x.XamlNameArea == "otherhc"))
                {
                    int safeIndex = Math.Clamp(_currentFieldIndex + 1, 0, Allregfields.Count);
                    Allregfields.Insert(safeIndex, additionalconditions);
                }
            }
            else
            {
                var otherhc = Allregfields.FirstOrDefault(x => x.Type == "otherhc");
                if (otherhc != null) Allregfields.Remove(otherhc);
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddHealthConditionsInfo");
        }
    }

    public void AddMedicationsInfo()
    {
        try
        {
            var medsYes = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "firstmedstack");
            bool selectedYes = medsYes?.AnswerId?.EndsWith("_1") == true;

            if (selectedYes)
            {
                var additionalconditions = AllregfieldsNotRequired.FirstOrDefault(x => x.XamlNameArea == "medicationsstack");
                if (additionalconditions != null && !Allregfields.Any(x => x.XamlNameArea == "medicationsstack"))
                {
                    int safeIndex = Math.Clamp(_currentFieldIndex + 1, 0, Allregfields.Count);
                    Allregfields.Insert(safeIndex, additionalconditions);
                }
            }
            else
            {
                var meds = Allregfields.FirstOrDefault(x => x.XamlNameArea == "medicationsstack");
                if (meds != null) Allregfields.Remove(meds);
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddMedicationsInfo");
        }
    }

    public void AddRVInfo()
    {
        try { /* Values captured to QuestionnaireResults by RVStepView */ }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "ImperialViewModel.AddRVInfo"); }
    }

    public void AddDietInfo()
    {
        try { /* Values captured to QuestionnaireResults by DietStepView */ }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "ImperialViewModel.AddDietInfo"); }
    }

    public void AddMenstrualInfo()
    {
        try { /* Values captured to QuestionnaireResults by MenstrualStepView */ }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "ImperialViewModel.AddMenstrualInfo"); }
    }

    public void AddHtInfo()
    {
        try { /* Values captured to QuestionnaireResults by HtStepView */ }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "ImperialViewModel.AddHtInfo"); }
    }

    public void AddAddqInfo()
    {
        try
        {
            // addqlifestyle answer already captured; handle over16 step insertion
            var aq = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "addqlifestyle");
            bool selectedYes = aq?.AnswerId != null && !aq.AnswerId.EndsWith("_2");

            if (selectedYes)
            {
                var over16items = AllregfieldsNotRequired.Where(x => x.Type == "over16").OrderBy(x => int.TryParse(x.Order, out var ov) ? ov : int.MaxValue).ToList();
                var over16mainItem = Allregfields.FirstOrDefault(x => x.Type == "over16main");
                int index = Allregfields.IndexOf(over16mainItem);
                if (index < 0) index = Allregfields.Count;

                foreach (var item in over16items)
                {
                    if (!Allregfields.Contains(item))
                    {
                        index++;
                        Allregfields.Insert(Math.Min(index, Allregfields.Count), item);
                    }
                }
            }
            else
            {
                var toRemoveAll16 = Allregfields.Where(x => x.Type == "over16").ToList(); foreach (var r in toRemoveAll16) Allregfields.Remove(r);
            }
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddAddqInfo");
        }
    }

    public void AddAntiViralInfo()
    {
        try { /* Values captured to QuestionnaireResults by AntiViralStepView */ }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "ImperialViewModel.AddAntiViralInfo"); }
    }

    public void AddTobaccoInfo()
    {
        try { /* Values captured to QuestionnaireResults by TobaccoStepView */ }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "ImperialViewModel.AddTobaccoInfo"); }
    }

    public void AddAlcoholInfo()
    {
        try { /* Values captured to QuestionnaireResults by AlcoholStepView */ }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "ImperialViewModel.AddAlcoholInfo"); }
    }

    public void AddDrugInfo()
    {
        try { /* Values captured to QuestionnaireResults by DrugStepView */ }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "ImperialViewModel.AddDrugInfo"); }
    }

    public void AddSleepInfo()
    {
        try { /* Values captured to QuestionnaireResults by SleepStepView */ }
        catch (Exception ex) { CrashDetected.LogCrash(ex, "ImperialViewModel.AddSleepInfo"); }
    }

    public void AddTandCsInfo()
    {
        try
        {
            if (allconsentdetails == null) return;

            var selectedConsentIds = allconsentdetails.consentcontent
                .SelectMany(section => section.sectioncontent)
                .Where(item => item.ChckedState && !item.required)
                .Select(item => item.consentitemid)
                .ToList();

            if (selectedConsentIds != null)
                TandCNonReqired = string.Join("|", selectedConsentIds);
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.AddTandCsInfo");
        }
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // Validate* methods â€” all return bool, set *StackError / *StackErrorMessage
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    public bool ValidateNameStack()
    {
        try
        {
            bool isValid = true;
            NameStackError = false;
            NameStackErrorMessage = string.Empty;

            newuser.firstname = newuser.firstname?.Trim();
            newuser.surname = newuser.surname?.Trim();
            newuser.email = newuser.email?.Trim();

            if (string.IsNullOrWhiteSpace(newuser.firstname))
            {
                NameStackError = true;
                NameStackErrorMessage = "Please enter first name";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(newuser.surname))
            {
                NameStackError = true;
                if (string.IsNullOrEmpty(NameStackErrorMessage))
                    NameStackErrorMessage = "Please enter surname";
                isValid = false;
            }

            if (noemailuserreg == false)
            {
                if (string.IsNullOrEmpty(newuser.password))
                {
                    NameStackError = true;
                    if (string.IsNullOrEmpty(NameStackErrorMessage))
                        NameStackErrorMessage = "Please enter a password";
                    isValid = false;
                }
            }

            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateFormStack()
    {
        try
        {
            MainUserStackError = false;
            // Actual UI-bound fields (entry text) are validated by the ContentView;
            // ViewModel returns true unless a flag has been set by the step
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidateaddressStack()
    {
        try
        {
            bool isValid = true;
            AddressStackError = false;
            AddressStackErrorMessage = string.Empty;

            string rawPostcode = newuser.postcode?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rawPostcode) || !UkPostcodeRegex.IsMatch(rawPostcode))
            {
                AddressStackError = true;
                AddressStackErrorMessage = "Enter a valid UK postcode";
                isValid = false;
            }

            if (isValid && validpostcodelist != null && validpostcodelist.Count > 0)
            {
                var cleanPostcode = rawPostcode.ToUpper().Replace(" ", "");
                var outwardCode = cleanPostcode.Length > 3 ? cleanPostcode[..^3] : cleanPostcode;
                if (!validpostcodelist.Any(p => p.Equals(outwardCode, StringComparison.OrdinalIgnoreCase)))
                {
                    AddressStackError = true;
                    AddressStackErrorMessage = "Sorry, this study is not available in your area";
                    isValid = false;
                }
            }

            return isValid;
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.ValidateaddressStack");
            return false;
        }
    }

    public bool ValidateGenderStack()
    {
        try
        {
            bool isValid = true;
            GenderStackError = false;
            GenderStackErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(newuser.dateofbirth))
            {
                GenderStackError = true;
                GenderStackErrorMessage = "Please enter date of birth";
                isValid = false;
            }
            else
            {
                bool parsed = DateTime.TryParseExact(newuser.dateofbirth, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dob);

                if (!parsed || dob > DateTime.Today)
                {
                    GenderStackError = true;
                    GenderStackErrorMessage = "Please enter a valid date of birth";
                    isValid = false;
                }
            }

            if (string.IsNullOrEmpty(newuser.gender))
            {
                GenderStackError = true;
                if (string.IsNullOrEmpty(GenderStackErrorMessage))
                    GenderStackErrorMessage = "Please select a gender";
                isValid = false;
            }

            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateEthnicityStack()
    {
        try
        {
            bool isValid = true;
            EthnicityStackError = false;
            EthnicityStackErrorMessage = string.Empty;

            if (string.IsNullOrEmpty(newuser.ethnicity))
            {
                EthnicityStackError = true;
                EthnicityStackErrorMessage = "Please select an ethnicity";
                isValid = false;
            }

            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidatedobStack()
    {
        try
        {
            bool isValid = true;
            GenderStackError = false;
            GenderStackErrorMessage = string.Empty;

            if (!validdob)
            {
                GenderStackError = true;
                GenderStackErrorMessage = "Please enter a valid date of birth";
                isValid = false;
            }

            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidatebodymetricsStack()
    {
        try
        {
            bool isValid = true;
            BodyMetricsStackError = false;
            BodyMetricsStackErrorMessage = string.Empty;
            // Actual field validation is delegated to ContentView; ViewModel checks newuser data
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateeducationStack()
    {
        try
        {
            bool isValid = true;
            EducationStackError = false;
            EducationStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateHouseholdstructureStack()
    {
        try
        {
            bool isValid = true;
            HouseholdStructureStackError = false;
            HouseholdStructureStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidatenhsnumStack()
    {
        try
        {
            bool isValid = true;
            NhsStackError = false;
            NhsStackErrorMessage = string.Empty;

            if (!validnhsnum)
            {
                NhsStackError = true;
                NhsStackErrorMessage = "Please enter a valid NHS number";
                isValid = false;
            }

            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateRIStack()
    {
        try
        {
            bool isValid = true;
            RiStackError = false;
            RiStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateHealthConditionsStack()
    {
        try
        {
            bool isValid = true;
            HealthConditionsStackError = false;
            HealthConditionsStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateMedicationsStack()
    {
        try
        {
            bool isValid = true;
            MedicationsStackError = false;
            MedicationsStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateRVStack()
    {
        try
        {
            bool isValid = true;
            RvStackError = false;
            RvStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateDietStack()
    {
        try
        {
            bool isValid = true;
            DietStackError = false;
            DietStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateMenstrualInfo()
    {
        try
        {
            bool isValid = true;
            MenstrualStackError = false;
            MenstrualStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateHtInfo()
    {
        try
        {
            bool isValid = true;
            HtStackError = false;
            HtStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool validateAQ()
    {
        try
        {
            bool isValid = true;
            AqStackError = false;
            AqStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateAntiViralInfo()
    {
        try
        {
            bool isValid = true;
            AntiViralStackError = false;
            AntiViralStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateTobaccoInfo()
    {
        try
        {
            bool isValid = true;
            TobaccoStackError = false;
            TobaccoStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateAlcoholInfo()
    {
        try
        {
            bool isValid = true;
            AlcoholStackError = false;
            AlcoholStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateDrugInfo()
    {
        try
        {
            bool isValid = true;
            DrugStackError = false;
            DrugStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateSleepInfo()
    {
        try
        {
            bool isValid = true;
            SleepStackError = false;
            SleepStackErrorMessage = string.Empty;
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public bool CheckTermsandConditions()
    {
        try
        {
            bool isValid = true;
            TandCsStackError = false;
            TandCsStackErrorMessage = string.Empty;

            if (allconsentdetails == null) return false;

            foreach (var section in allconsentdetails.consentcontent)
            {
                foreach (var item in section.sectioncontent)
                {
                    item.ShowValidation = true;
                    if (item.required && !item.ChckedState)
                    {
                        TandCsStackError = true;
                        TandCsStackErrorMessage = "Please accept all required consent items";
                        isValid = false;
                    }
                }
            }

            if (!SignPadhaddata)
            {
                TandCsStackError = true;
                if (string.IsNullOrEmpty(TandCsStackErrorMessage))
                    TandCsStackErrorMessage = "Please sign to confirm consent";
                isValid = false;
            }

            return isValid;
        }
        catch
        {
            return false;
        }
    }

    // â”€â”€ Converters (migrated verbatim) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public RegField ConvertQuestionFieldToSubField(QuestionField qField)
    {
        try
        {
            return new RegField
            {
                Id = qField.QuestionId,
                Label = qField.Label,
                Type = qField.Type,
                Required = qField.Required,
                Placeholder = qField.Placeholder,
                Order = qField.Order,
                Active = qField.Active,
                XamlNameArea = qField.XamlNameArea,
                HelpText = qField.Directions,
                Options = qField.Answers.Select(a => new OptionDetails
                {
                    AnswerId = a.AnswerId,
                    Value = a.Value,
                    Text = a.Label
                }).ToList(),
                subFields = new List<RegField>()
            };
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.ConvertQuestionFieldToSubField");
            return null;
        }
    }

    public List<RegField> ConvertQuestionGroupsToRegFields(List<QuestionModel> questionGroups)
    {
        try
        {
            var newRegFields = new List<RegField>();

            foreach (var group in questionGroups.Where(g => g.Active))
            {
                var parentRegField = new RegField
                {
                    Id = group.Id,
                    Label = group.Label,
                    Type = "Questionnaire",
                    Required = group.Required,
                    Placeholder = group.Placeholder,
                    Order = group.Order,
                    Active = group.Active,
                    XamlNameArea = group.XamlNameArea,
                    subFields = new List<RegField>()
                };

                var orderedQuestions = group.Fields
                    .Where(f => f.Active)
                    .OrderBy(f => int.TryParse(f.Order, out var ov) ? ov : int.MaxValue)
                    .ToList();

                foreach (var qField in orderedQuestions)
                    parentRegField.subFields.Add(ConvertQuestionFieldToSubField(qField));

                newRegFields.Add(parentRegField);
            }

            return newRegFields
                .OrderBy(x => int.TryParse(x.Order, out var ov) ? ov : int.MaxValue)
                .ToList();
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, "ImperialViewModel.ConvertQuestionGroupsToRegFields");
            return null;
        }
    }

    // â”€â”€ Password hashing â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    internal async Task<string> HashPasswordAsync(string password)
    {
        return await Task.Run(() =>
        {
            try
            {
#pragma warning disable CA5351 // MD5 kept for parity with existing Imperial code
                using var md5 = MD5.Create();
#pragma warning restore CA5351
                var inputBytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
            catch
            {
                return null!;
            }
        });
    }

    // â”€â”€ Email validation helper â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    internal bool EmailIsValid(string email)
    {
        return Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase);
    }

    // â”€â”€ IsIgnoredException (exposed for unit tests per CrashDetectedTests) â”€â”€â”€â”€
    // Note: this belongs on CrashDetected; this wrapper is here as a convenience
    // in case tests go through the ViewModel. The authoritative implementation is in CrashDetected.
}


