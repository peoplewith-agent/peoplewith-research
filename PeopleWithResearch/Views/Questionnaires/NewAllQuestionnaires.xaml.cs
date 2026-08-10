using FreakyKit.Utils;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace PeopleWithResearch;

public partial class NewAllQuestionnaires : ContentPage
{
    public List<questionnaires> AllQuestionnaires = new(); 
    public List<newuserquestionnaire> AllUserQuestionnaires = new();
    public List<newuserquestionnaire> FilteredUserQuestionnaires = new();
    public ObservableCollection<householdgroup> HouseHoldGroup = new();
    public List<string> MissedList = new();
    public Dictionary<string, string> UserDetailsKey = new();
    public bool ismainuser;
    public NewAllQuestionnaires(ObservableCollection<householdgroup> HouseHoldPassed, List<newuserquestionnaire> UserQuestionnairesPassed, List<questionnaires> QuestionnairesPased,
        List<newuserquestionnaire> UserSpecificQuestionniares, Dictionary<string, string> UserKey)
    {
        InitializeComponent();
        HouseHoldGroup = HouseHoldPassed;
        AllUserQuestionnaires = UserQuestionnairesPassed;
        AllQuestionnaires = QuestionnairesPased;
        FilteredUserQuestionnaires = UserSpecificQuestionniares;
        UserDetailsKey = UserKey;

        HandleDataPassed();
    }

    public NewAllQuestionnaires(ObservableCollection<householdgroup> HouseHoldPassed, List<string> ListPassed)
    {
        InitializeComponent();
        HouseHoldGroup = HouseHoldPassed;
        MissedList = ListPassed;
        BindingContext = this;
        HandleIncomplete(); 
    }

    async Task HandleIncomplete()
    {
        try
        {
            AllTitle.Text = "Missed Forms";
            if(MissedList.Count > 0)
            {
                MissedQuestionsList.ItemsSource = MissedList;
                MissedQuestionsList.IsVisible = true;
                ShowHideError(true);
            }
            else
            {
                ShowHideError(false);
            }             
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "HandleIncomplete");
        }  
    }

    async Task LoadData()
    {
        try
        {
            var questionnairesTask = APICalls.Instance.GetUserQuestionnaires();
            await Task.WhenAll(questionnairesTask);
            AllUserQuestionnaires = questionnairesTask.Result.ToList();

            if (AllUserQuestionnaires == null || !AllUserQuestionnaires.Any())
                await ShowHideError(false);

            var recentItems = AllUserQuestionnaires
        .OrderByDescending(q => q.DateTimeAdded)
        .ToList();

            var AddTitle = new Dictionary<string, string>
                {
                    { "70530492-D1B8-42F5-A851-1C8769288995", "HOPPER Study - Withdrawal Form" },
                    { "CBDA3207-C3BE-4FCB-9633-FED8FE58DAA2", "T1 Samples, Symptoms & Changes" },
                    { "DDD843CF-021B-4557-8824-13C5B4E2EA85", "HOPPER Baseline Sampling Form" }
                };

            var rnd = new Random();
            foreach (var item in recentItems)
            {
                item.FormattedDateTime = item.DateTimeAdded.ToString("MMM dd, HH:mm");
                item.title = AddTitle.TryGetValue(item.questionnaireid, out var title) ? title : item.questionnaireid;
            }

            CompletedQuestionList.ItemsSource = recentItems;
            await ShowHideError(true);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "LoadData");
        }
    } 

    async Task HandleDataPassed()
	{
		try
		{
            var mainUserId = HouseHoldGroup?.FirstOrDefault()?.primaryuserid;
            if (string.IsNullOrEmpty(mainUserId)) return;
            ismainuser = (mainUserId == Helpers.Settings.UsersID) ? true : false;
            CompletedQuestionList.ItemsSource = (ismainuser) ? AllUserQuestionnaires : FilteredUserQuestionnaires; 
            await ShowHideError(true); 
         
        }
		catch (Exception Ex)
		{
            CrashDetected.LogCrash(Ex, Navigation, "HandleDataPassed");
            await ShowHideError(false); 
        }
	}
    private async void CompletedQuestionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
		try
		{
            var tappedItem = e.CurrentSelection.FirstOrDefault() as newuserquestionnaire;
            if (tappedItem == null) return;
            var Questionnaire = AllQuestionnaires.FirstOrDefault(Q => Q.questionnaireid == tappedItem.questionnaireid);
            await Navigation.PushAsync(new NewQuestionnairesPage(tappedItem, Questionnaire, ismainuser, UserDetailsKey), false);
            CompletedQuestionList.SelectedItem = null;
        }
		catch (Exception Ex)
		{
            CrashDetected.LogCrash(Ex, Navigation, "CompletedQuestionList_SelectionChanged");
        }
    }
    private async void CompletedQuestionList_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var tappedItem = e.DataItem as newuserquestionnaire;
            if (tappedItem == null) return;
            var Questionnaire = AllQuestionnaires.FirstOrDefault(Q => Q.questionnaireid == tappedItem.questionnaireid);
            await Navigation.PushAsync(new NewQuestionnairesPage(tappedItem, Questionnaire, ismainuser, UserDetailsKey), false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "CompletedQuestionList_ItemTapped");
        }
    }


    private async Task ShowHideError(bool Success)
    {
        CompletedQuestionList.IsVisible = Success;      
        LoadFailed.IsVisible = !Success;
        //Always false
        loadingstack.IsVisible = false;
        LoadInd.IsRunning = false;    
        return; 
    }

    private async void MissedQuestionsList_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            //missed day item tap
            //passing the bool to know its a missed questionnaire 

            var Tapped = e.DataItem as string;
            if (Tapped == null) return;

            var stringday = Tapped.Replace("T", "");
            if (int.TryParse(stringday, out int day))
            {
                await Navigation.PushAsync(new T1Questionnaire(AllUserQuestionnaires.ToObservable(), HouseHoldGroup[0], day, true), false);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "MissedQuestionsList_ItemTapped");
        }
    }
}