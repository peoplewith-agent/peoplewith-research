using System.Collections.ObjectModel;
using PeopleWithResearch.Views.New.RegisterLogin.SignUpCodes;

namespace PeopleWithResearch;

public partial class NewImperial : ContentPage
{
    public ImperialViewModel ViewModel { get; private set; }
    public event EventHandler<bool> ConnectivityChanged;

    // Constructor 1 — default
    public NewImperial()
    {
        try { InitializeComponent(); }
        catch (Exception ex) { NotasyncMethod(ex); return; }
        ViewModel = new ImperialViewModel();
        BindingContext = ViewModel;
        WireEvents();
    }

    // Constructor 2 — household rep from registration
    public NewImperial(user userpassed, advert signupdetailspassed, Questionnaire questionnairepassed)
    {
        try { InitializeComponent(); }
        catch (Exception ex) { NotasyncMethod(ex); return; }
        ViewModel = new ImperialViewModel(userpassed, signupdetailspassed, questionnairepassed);
        BindingContext = ViewModel;
        WireEvents();
    }

    // Constructor 3 — standard signup
    public NewImperial(user userpassed, signupcode signupdetailspassed)
    {
        try { InitializeComponent(); }
        catch (Exception ex) { NotasyncMethod(ex); return; }
        ViewModel = new ImperialViewModel(userpassed, signupdetailspassed);
        BindingContext = ViewModel;
        WireEvents();
    }

    // Constructor 4 — household member
    public NewImperial(signupcode signupdetailspassed, householdgroupjsondetails userinfopassed,
        ObservableCollection<householdgroupjsondetails> allgroupdetails, householdgroup passedhousehold)
    {
        try { InitializeComponent(); }
        catch (Exception ex) { NotasyncMethod(ex); return; }
        ViewModel = new ImperialViewModel(signupdetailspassed, userinfopassed, allgroupdetails, passedhousehold);
        BindingContext = ViewModel;
        WireEvents();
    }

    private void WireEvents()
    {
        ViewModel.NavigateToStep += OnNavigateToStep;
        ViewModel.NavigationRequested += OnNavigationRequested;
        // Show first step immediately
        OnNavigateToStep(this, ViewModel.CurrentStep ?? "welcomestack");
    }

    private void OnNavigateToStep(object sender, string stepKey)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StepHost.Content = CreateStepView(stepKey);
        });
    }

    private View CreateStepView(string stepKey) => stepKey switch
    {
        "welcomestack"         => new WelcomeStepView(),
        "namestack"            => new NameStepView(),
        "mainuserstack"        => new MainUserStepView(),
        "addressstack"         => new AddressStepView(),
        "genderstack"          => new GenderStepView(),
        "ethnicitystack"       => new EthnicityStepView(),
        "bodymetricsstack"     => new BodyMetricsStepView(),
        "educationworkstack"    => new EducationWorkStepView(),
        "householdstructurestack" => new HouseholdStructureStepView(),
        "nhsnumstack"          => new NHSStepView(),
        "ristack"              => new RIStepView(),
        "hcstack"              => new HealthConditionsStepView(),
        "medynstack"           => new MedicationsStepView(),
        "rvstack"              => new RVStepView(),
        "dietstack"            => new DietStepView(),
        "menstrualstack"       => new MenstrualStepView(),
        "htstack"              => new HtStepView(),
        "additionalqstack"     => new AdditionalQuestionsStepView(),
        "antiviralstack"       => new AntiViralStepView(),
        "tobaccostack"         => new TobaccoStepView(),
        "alcoholstack"         => new AlcoholStepView(),
        "drugstack"            => new DrugStepView(),
        "sleepstack"           => new SleepStepView(),
        "termsstack"           => new TandCsStepView(),
        "addhcstack"           => new HouseholdMembersStepView(),
        _                      => new WelcomeStepView()
    };

    private async void OnNavigationRequested(object sender, string destination)
    {
        try
        {
            if (destination == "pop")
                await Navigation.PopAsync();
            else if (destination == "dashboard")
                Application.Current.MainPage = new ImperialDashboard();
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, Navigation, "NewImperial.OnNavigationRequested");
        }
    }

    void NotasyncMethod(Exception ex)
    {
        CrashDetected.LogCrash(ex, Navigation, "NewImperial");
    }
}

