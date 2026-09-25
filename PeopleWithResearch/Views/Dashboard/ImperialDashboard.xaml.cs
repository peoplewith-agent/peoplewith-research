
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FreakyKit.Utils;
using Microsoft.Azure.NotificationHubs;
using Mopups.Services;
using Newtonsoft.Json;
using PeoplewithResearch;
using Plugin.LocalNotification;
using Plugin.LocalNotification;
using Syncfusion.Maui.Inputs;

namespace PeopleWithResearch;

public partial class ImperialDashboard : ContentPage
{

    public ObservableCollection<householdgroup> Allhouseholdgroupinfo = new ();
    public ObservableCollection<householdgroupjsondetails> Allhouseholdgroupinfodetails = new ();
    public List<questionnaires> AllQuestionnaires = new();
    public List<string> MissedList = new(); 
    public Dictionary<string, string> UserDetailsKey { get; set; } = new();

    public event EventHandler<bool> ConnectivityChanged;

    private readonly INotificationSettingsService _notificationService;

    public UserNotifications AddNotification = new(); 
    private List<newuserquestionnaire> allQuestionnairesOrdered = new();
    private List<newuserquestionnaire> AllUserQuestionnaires = new();
    private List<newuserquestionnaire> MemberSpecificQuestionnaires = new();
    public TEventMember Member = new(); 
    public bool ismainuser;
    public bool TempHideNotif = false;
    public DateTime T1Start = new();

  //  public ObservableCollection<newuserquestionnaire> allQuestionnairesOrdered = new();


    public Dictionary<string, string> QuestionnaireTitle = new ()
    {
        { "70530492-D1B8-42F5-A851-1C8769288995", "HOPPER Study - Withdrawal Form" },
        { "CBDA3207-C3BE-4FCB-9633-FED8FE58DAA2", "T1 Samples, Symptoms & Changes" },
        { "DDD843CF-021B-4557-8824-13C5B4E2EA85", "HOPPER Baseline Sampling Form" },
        { "B627DF59-7AD8-4832-A407-BF5F85BDE8E0", "HOPPER End of Study Form" }
    };

    public int dayNumber;

    private bool reload;

    public List<String> CompletedColours = new List<String>
    { "#F8F4E3", "#E6A8AD", "#F3C096" , "#F1E09F", "#B7CEB0", "#A8D1DF", "#C7B9D6"};
    public ImperialDashboard()
    {
        InitializeComponent();

        BindingContext = this;

        // Preferences.Default.Set("primaryuserid", Helpers.Settings.UsersID);
        //Preferences.Default.Set("firstname", "Mark Harry");
        //Preferences.Default.Set("signupcode", "HOPPERCR");


        studyidlbl.Text = Helpers.Settings.UsersID;
        welcomelbl.Text = LocalizationManager.Get("Dashboard_HiPrefix") + " " + Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;
        activeProfileChipName.Text = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

        // Localise static tab headers and "Logged in as" span
        ApplyDashboardLocalization();

      //  checkifappisupdated();

        GetHouseholdData();
        GetInformationDetails();
        //GetProfileData();
        checknotificationEnabled();

        //-- Removed for now add after t-Forms
        //SelectNotificationTime(); 

        //RegisterDevice();
        //var one = new newuser();

        //one.FirstName = "Seran Hakki";
        //one.Status = "Completed";
        //      one.Studyinfo = "HP.021" + " . " + "All complete";
        //      one.Studyactiveimage = "tick.png";

        //      newuserlist.Add(one);

        //      var two = new newuser();

        //      two.FirstName = "John Smith";
        //      two.Status = "Awaiting Baseline";
        //      two.Studyinfo = "HP.022" + " . " + "Awaiting Baseline";
        //      two.Studyactiveimage = "error.png";
        //      two.Showbaseline = true;

        //      newuserlist.Add(two);

        //      var three = new newuser();

        //      three.FirstName = "Mark Harry";
        //      three.Status = "Completed";
        //      three.Studyinfo = "HP.023" + " . " + "All complete";
        //      three.Studyactiveimage = "tick.png";

        //      newuserlist.Add(three);

        //      profilelist.ItemsSource = newuserlist;

        

        WeakReferenceMessenger.Default.Register<UpdateHouseHoldGroup>(this, async (r, o) =>
        {
            //Allhouseholdgroupinfodetails = o.Value;
            //profilelist.ItemsSource = Allhouseholdgroupinfodetails;
            //profilelist.RefreshView();

            GetHouseholdData();
        });

        WeakReferenceMessenger.Default.Register<RefreshNotificationDash>(this, async (r, o) =>
        {
            checknotificationEnabled(); 
        });

        WeakReferenceMessenger.Default.Register<UpdateDashCompelted>(this, async (r, o) =>
        {
            reload = true;
            AllUserQuestionnaires = o.Value;
            //await RecentComeplted();
        });

        WeakReferenceMessenger.Default.Register<UpdateProfile>(this, async (r, o) =>
        {
            await GetProfileData(); 
        });

        // WeakReferenceMessenger.Default.Register<ReloadProfileMessage>(this, async (r, o) =>
        // {
        //     await GetProfileData();
        // });


        if (string.IsNullOrEmpty(Helpers.Settings.SelectedLanguage))
        {
            //Commented out for now 
            //SelectedLangugage();
        }

        if(Helpers.Settings.ShowConsentScreen)
        {
            InitialConsentScreen();
        }
    }

    private void ApplyDashboardLocalization()
    {
        try
        {
            hometab.Header    = LocalizationManager.Get("Tab_Home");
            infotab.Header    = LocalizationManager.Get("Tab_Information");
            profiletab.Header = LocalizationManager.Get("Tab_Profile");
            loggedInAsSpan.Text = LocalizationManager.Get("Dashboard_LoggedInAs") + " ";
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "ApplyDashboardLocalization");
        }
    }

    private async Task SelectedLangugage()
    {
        try
        {
            await Task.Delay(5000); 
            await MopupService.Instance.PushAsync(new SelectLanguagePopup());
        }
        catch (Exception ex)
        {
        }
    }

    private async Task InitialConsentScreen()
    {
        try
        {
            await Task.Delay(5000); 
            await MopupService.Instance.PushAsync(new InitialConsentPopup());
        }
        catch (Exception ex)
        {
        }
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (reload)
        {
            reload = false;
            await GetHouseholdData(); 
            //await gethouseholddata();
            //await RecentComeplted();
        }
    }

    //async void checkifappisupdated()
    //{
    //    try
    //    {
    //        var versionCheckService = new VersionCheckService();
    //        bool Check = await versionCheckService.CheckForUpdate();
    //        if (Check)
    //        {
    //            await Navigation.PushAsync(new UpdatePage(), false);
    //        }
    //    }
    //    catch(Exception ex)
    //    {

    //    }
    //}


    async void ReloadDashfromSwitchProfile()
    {
        try
        {
            //Preferences.Default.Set("firstname", "Mark Harry");
            //Preferences.Default.Set("signupcode", "HOPPERCR");

            studyidlbl.Text = Helpers.Settings.UsersID;
          //  welcomelbl.Text = "Hi, " + Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

            GetHouseholdData();
    
        }
        catch(Exception ex)
        {

        }
    }

    async void RegisterDevice()
    {
        try
        {
            var notificationService = new NotificationService();
            await notificationService.AddTag();
        }
        catch (Exception Ex) 
        {
            CrashDetected.LogCrash(Ex, Navigation, "RegisterDevice");
        }
    }

    async Task GetHouseholdData()
    {
        try
        {
            //remove the line below and change back, only for harry testing
            //Allhouseholdgroupinfo = await APICalls.Instance.GetUserHouseholdInfo("HG-HP-165");

          //  Allhouseholdgroupinfo = await APICalls.Instance.GetUserHouseholdInfo(Helpers.Settings.HouseholdGrouping);
            var householdGroupList = await APICalls.Instance.GetUserHouseholdInfo(Helpers.Settings.HouseholdGrouping);
            var HouseHoldGroup = householdGroupList?.FirstOrDefault();

            if (HouseHoldGroup == null) { HideDashboardLoader(); return; }

            Allhouseholdgroupinfo = householdGroupList;
            Allhouseholdgroupinfodetails = HouseHoldGroup.userdetailslist ?? new ObservableCollection<householdgroupjsondetails>();

            var mainUserId = Allhouseholdgroupinfo?.FirstOrDefault()?.primaryuserid;
            if (string.IsNullOrEmpty(mainUserId)) { HideDashboardLoader(); return; }
            ismainuser = (mainUserId == Helpers.Settings.UsersID) ? true : false;

            var mainUser = new householdgroupjsondetails();
            if (mainUserId == Helpers.Settings.UsersID)
            {
                mainUser.household_individual_name = Helpers.Settings.FirstName;
                mainUser.household_individual_userid = Helpers.Settings.UsersID;


                addinghouseborder.IsVisible = true;

                activestack.IsVisible = true;
                rolestack.IsVisible = true;
                householdIcon.IsVisible = false;
                householdRepName.IsVisible = false;
            }
            else
            {
                TypeImage.Source = "team.png";
                UserType.Text = "Participant";

                activestack.IsVisible = false;
                rolestack.IsVisible = false;
              //  householdIcon.IsVisible = true;
                householdRepName.IsVisible = true;

                addinghouseborder.IsVisible = false;
            }


        

                var householdRepList = await APICalls.Instance.GetuserDetails(mainUserId);
                var hhRepGot = householdRepList?.FirstOrDefault();

                if (hhRepGot != null)
                {
                    mainUser.household_individual_name = $"{hhRepGot.FirstName} {hhRepGot.Surname}";
                    mainUser.household_individual_userid = hhRepGot.Userid;
                mainUser.household_individual_email = hhRepGot.Email;
                    householdreplbl.Text = mainUser.household_individual_name;
                }
            

            mainUser.mainuser = true;
            mainUser.household_group_id = HouseHoldGroup.householdgroupid;
            mainUser.household_individual_status = "active";
            mainUser.household_individual_age = "16+";
            mainUser.household_individual_relationship = "Household Rep";
            mainUser.Studyactiveimage = "greentick.png";
            mainUser.ListOpacity = 1;
            mainUser.ShowBaselineText = false;
            mainUser.LastActiveDate = null;
            mainUser.QuestionnairesCompleted = string.Empty;

            // Insert MainUser Into HouseGold
            Allhouseholdgroupinfodetails.Insert(0, mainUser);

            //Create Dictionary oF UserId && Name 
            UserDetailsKey = Allhouseholdgroupinfo?.FirstOrDefault()?.userdetailslist?.Where(m => m != null
            && !string.Equals(m.household_individual_status, "Withdrawn", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(m.household_individual_userid))
            .ToDictionary(
            user => user.household_individual_userid.ToString(),
            user => user.household_individual_name) ?? new Dictionary<string, string>();

            if (UserDetailsKey.Count == 0)
            {
                AllUserQuestionnaires = new List<newuserquestionnaire>();
                HideDashboardLoader();
                return;
            }


            var questionnaireTasks = UserDetailsKey.Keys.Select(APICalls.Instance.GetUserQuestionnairesbyUserid);
            //var questionnaireTasks = UserDetailsKey.Select(APICalls.Instance.GetUserQuestionnairesbyUserid);
            var results = await Task.WhenAll(questionnaireTasks);

            AllUserQuestionnaires = results
                .Where(r => r != null)
                .SelectMany(r => r)
                .OrderByDescending(q => q.DateTimeAdded)
                .ToList();

            // Update Styles for each Member 
            foreach (var item in Allhouseholdgroupinfodetails)
            {
                if (item == null) continue;

                bool isRep = string.Equals(item.household_individual_relationship, "Household Rep", StringComparison.OrdinalIgnoreCase);
                bool isWithdrawn = string.Equals(item.household_individual_status, "Withdrawn", StringComparison.OrdinalIgnoreCase);
                bool isOnboarding = string.Equals(item.household_individual_status, "Onboarding", StringComparison.OrdinalIgnoreCase);
                bool isActive = string.Equals(item.household_individual_status, "Active", StringComparison.OrdinalIgnoreCase);

                // needs study team to activate — over 16 (age = "16+") with no real email on file
                bool isOver16 = string.Equals(item.household_individual_age?.Trim(), "16+", StringComparison.OrdinalIgnoreCase);
                bool hasNoEmail = string.IsNullOrWhiteSpace(item.household_individual_email)
                    || item.household_individual_email.Trim().StartsWith("N/A", StringComparison.OrdinalIgnoreCase);
                item.ShowContactStudyTeam = !isWithdrawn && isOver16 && hasNoEmail;

                item.Studyactiveimage = isActive ? "greentick.png" : isOnboarding ? "error.png" : "logout.png";
                item.Studyinfo = $"{item.household_individual_userid} | {TranslateHouseholdStatus(item.household_individual_status)} | {item.household_individual_age}";

                if (isWithdrawn)
                {
                    WithdrawnStyle(item);
                }
                else if (isRep)
                {
                    HouseRepStyle(item);

                    //move here so only house rep sees this detail
                    ActiveOrOnboardingStyle(item, AllUserQuestionnaires, isOnboarding, isActive, isRep);
                }
                else
                {
                    ActiveOrOnboardingStyle(item, AllUserQuestionnaires, isOnboarding, isActive, isRep);
                }

                if(item.household_individual_userid == Helpers.Settings.UsersID)
                {

                }
            }

            var sortedList = Allhouseholdgroupinfodetails
                .OrderBy(x => string.Equals(x.household_individual_relationship, "Household Rep", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(x => x.household_individual_status)
                .ToList();

            profilelist.ItemsSource = sortedList;
            profilelist.RefreshView();

            int activeCount = Allhouseholdgroupinfodetails.Count(x => string.Equals(x.household_individual_status, "Active", StringComparison.OrdinalIgnoreCase));
            familycountlbl.Text = activeCount.ToString();

            addimage1.Source = activeCount >= 1 ? "adduserblue.png" : "addusergray.png";
            addimage2.Source = activeCount >= 2 ? "adduserblue.png" : "addusergray.png";
            addimage3.Source = activeCount >= 3 ? "adduserblue.png" : "addusergray.png";

            percentpb.Progress = activeCount;

            findoutstudyprogress();
            await RecentComeplted();      
            await GetProfileData();
            await SelectNotificationTime(Member, T1Start);

            // Hide the loading overlay — all data has been loaded
            HideDashboardLoader();


            //Test Local Notification
            //var Notification = new NotificationRequest()
            //{
            //    NotificationId = 0,
            //    Title = "Test",
            //    Description = "Body",
            //    BadgeNumber = 0,
            //    Sound = DeviceInfo.Platform == DevicePlatform.Android ? "pwjingo" : "pwjingo.aiff",
            //    Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions
            //    {
            //        Priority = Plugin.LocalNotification.AndroidOption.AndroidPriority.Max,
            //        Ongoing = false,
            //        ChannelId = "pwr_notifications",
            //    },
            //    Schedule = new NotificationRequestSchedule
            //    {
            //        NotifyTime = DateTime.Now.AddSeconds(20),
            //        RepeatType = NotificationRepeat.Daily,
            //        NotifyRepeatInterval = null
            //    }
            //};

            //await LocalNotificationCenter.Current.Show(Notification);


        }
        catch (Exception Ex)
        {
            // Hide loader on error so the user is never stuck on the loading screen
            HideDashboardLoader();
            CrashDetected.LogCrash(Ex, Navigation, "GetHouseholdData");
        }
    }

    private void HideDashboardLoader()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            dashboardActivityIndicator.IsRunning = false;
            dashboardLoadingOverlay.IsVisible = false;
        });
    }

    private static string TranslateHouseholdStatus(string englishStatus)
    {
        return englishStatus?.ToLower() switch
        {
            "active"      => LocalizationManager.Get("Household_StatusActive"),
            "onboarding"  => LocalizationManager.Get("Household_StatusOnboarding"),
            "withdrawn"   => LocalizationManager.Get("Household_StatusWithdrawn"),
            _             => englishStatus ?? string.Empty
        };
    }

    private void WithdrawnStyle(householdgroupjsondetails item)
    {
        item.ListOpacity = 0.7;
        item.ShowActions = false;
        item.ShowBaselineText = false;
        item.BaselineButtonOpacity = 1;
        item.BaselineButtonEnabled = false;
        item.ManageButtonOpacity = 1;
        item.ManageButtonEnabled = false;
        item.LastActiveDate = null;
        item.QuestionnairesCompleted = "-";
        ResetBaselineStatus(item);
    }

    private void HouseRepStyle(householdgroupjsondetails item)
    {
        item.ListOpacity = 1;
        item.ShowBaselineText = false;
        item.BaselineButtonOpacity = 1;
        item.BaselineButtonEnabled = false;
        item.ManageButtonOpacity = 1;
        item.ManageButtonEnabled = true;
        item.LastActiveDate = null;
        item.QuestionnairesCompleted = "-";
        ResetBaselineStatus(item);
    }

    private void ActiveOrOnboardingStyle(householdgroupjsondetails item, List<newuserquestionnaire> allQuestionnaires, bool isOnboarding, bool isActive, bool isRep)
    {
        var memberQuestionnaires = allQuestionnaires
            .Where(q => q.userid == item.household_individual_userid)
            .ToList();

        item.QuestionnairesCompleted = memberQuestionnaires.Count > 0
            ? memberQuestionnaires.Count.ToString()
            : "--";

        var newestQuestionnaire = memberQuestionnaires.FirstOrDefault();
        item.LastActiveDate = newestQuestionnaire != null
            ? newestQuestionnaire.DateTimeAdded.ToString("dd MMM yyyy")
            : "--";

        item.ListOpacity = 1;

        if (isRep)
        {
            item.ShowDetails = true;
            item.ShowActions = false;
        }
        else
        {
            item.ShowDetails = true;
            item.ShowActions = true;
        }

        var mainUserId = Allhouseholdgroupinfo?.FirstOrDefault()?.primaryuserid;
        if (string.IsNullOrEmpty(mainUserId)) return;
        ismainuser = (mainUserId == Helpers.Settings.PrimaryUserID) ? true : false;
        bool isCurrentUser = item.household_individual_userid == Helpers.Settings.UsersID;

        if (!ismainuser)
        {
            item.ShowActions = false;
            item.ShowDetails = false;
            item.ShowSwitchProfile = false;
            item.ShowActiveProfile = false;
            if(isCurrentUser)
            {
                houserepaccessborder.IsVisible = item.household_rep_access == "pending";
            }
        }
        else
        {

            if(isCurrentUser)
            {
                item.ShowActiveProfile = true;
                item.ShowSwitchProfile = false;

                welcomelbl.Text = LocalizationManager.Get("Dashboard_HiPrefix") + " " + item.household_individual_name;
                activeProfileChipName.Text = item.household_individual_name;

            }
            else
            {
                item.ShowActiveProfile = false;

                if (item.household_rep_access == "pending")
                {

                    item.ShowActions = false;
                    item.ShowSwitchProfile = false;
                    item.ShowAwaitingBaseline = false;
                }
                else
                {
                    item.ShowSwitchProfile = true;
                    item.ShowActions = true;
                }
            }
        }


        if (Helpers.Settings.SignUp == "HOPPERCR")
        {

        }


            // ---- Baseline Form status ----
            bool baselineFormCompleted = !isOnboarding;
        item.BaselineFormStatusText = baselineFormCompleted ? LocalizationManager.Get("Common_StatusCompleted") : LocalizationManager.Get("Common_StatusPending");
        item.BaselineFormBorderColor = new SolidColorBrush(Color.FromArgb(baselineFormCompleted ? "#009fe3" : "#eeeeee"));

        if (ismainuser)
        {
            item.ShowAwaitingBaseline = false;
        }
        else
        {
            item.ShowAwaitingBaseline = baselineFormCompleted;
        }
        item.BaselineFormTextColor = baselineFormCompleted ? "#009fe3" : "#031926";

        // ---- Baseline Samples status ----
        bool baselineSamplesCompleted = memberQuestionnaires.Any(q =>
            string.Equals(q.questionnaireid, "b1_samples", StringComparison.OrdinalIgnoreCase));

        item.BaselineSamplesStatusText = baselineSamplesCompleted ? LocalizationManager.Get("Common_StatusCompleted") : LocalizationManager.Get("Common_StatusPending");
        item.BaselineSamplesBorderColor = new SolidColorBrush(Color.FromArgb(baselineSamplesCompleted ? "#009fe3" : "#eeeeee"));
        item.BaselineSamplesTextColor = baselineSamplesCompleted ? "#009fe3" : "#031926";

        if (isOnboarding)
        {
            item.ShowBaselineText = true;
            item.BaselineButtonOpacity = 1;
            item.BaselineButtonEnabled = true;
            item.ManageButtonOpacity = 0.2;
            item.ManageButtonEnabled = false;
            item.SendReminderText = LocalizationManager.Get("Household_SendEmailReminder");
        }
        else if (isActive)
        {
            item.ShowBaselineText = false;
            item.BaselineButtonOpacity = 0.2;
            item.BaselineButtonEnabled = false;
            item.ManageButtonOpacity = 1;
            item.ManageButtonEnabled = true;
            item.SendReminderText = LocalizationManager.Get("Household_NudgeNotification");
        }
    }

    private void ResetBaselineStatus(householdgroupjsondetails item)
    {
        item.BaselineFormStatusText = "-";
        item.BaselineFormStatusColor = "#9ca3af";
        item.BaselineFormStatusIcon = null;
        item.BaselineSamplesStatusText = "-";
        item.BaselineSamplesStatusColor = "#9ca3af";
        item.BaselineSamplesStatusIcon = null;
    }

    //async Task gethouseholddata()
    //{
    //    try
    //    {
    //        Allhouseholdgroupinfo = await APICalls.Instance.GetUserHouseholdInfo(Helpers.Settings.HouseholdGrouping);

    //        if (Allhouseholdgroupinfo != null)
    //        {
    //            Allhouseholdgroupinfodetails = Allhouseholdgroupinfo[0].userdetailslist;

    //            var mainUserId = Allhouseholdgroupinfo?.FirstOrDefault()?.primaryuserid;
    //            if (string.IsNullOrEmpty(mainUserId)) return; 

    //            var mainUser = new householdgroupjsondetails();

    //            if (mainUserId == Helpers.Settings.UsersID)
    //            {
    //                // Main User logged in 
    //                mainUser.household_individual_name = Helpers.Settings.FirstName;
    //                mainUser.household_individual_userid = Helpers.Settings.UsersID;
    //            }
    //            else
    //            {
    //                TypeImage.Source = "team.png"; 
    //                UserType.Text = "Participant";
    //                // Get main user details
    //                var householdRep = await APICalls.Instance.GetuserDetails(mainUserId);
    //                var hhRepGot = householdRep?.FirstOrDefault();

    //                if (hhRepGot != null)
    //                {
    //                    mainUser.household_individual_name = $"{hhRepGot.FirstName} {hhRepGot.Surname}";
    //                    mainUser.household_individual_userid = hhRepGot.Userid;
    //                }
    //            }


    //            var HHGRoupID = Allhouseholdgroupinfo.FirstOrDefault()?.householdgroupid;

    //            mainUser.mainuser = true;
    //            mainUser.household_group_id = HHGRoupID;
    //            mainUser.household_individual_status = "active";
    //            mainUser.household_individual_age = "Over 18";
    //            mainUser.household_individual_relationship = "Household Rep";
    //            mainUser.Studyactiveimage = "greentick.png";
    //            mainUser.ListOpacity = 1;
    //            mainUser.ShowBaselineText = false;
    //            mainUser.LastActiveDate = null;
    //            mainUser.QuestionnairesCompleted = string.Empty;
    //            mainUser.Studyinfo = $"{mainUser.household_individual_userid} . {mainUser.household_individual_relationship} . {mainUser.household_individual_status}";

    //            Allhouseholdgroupinfodetails.Insert(0, mainUser);


    //            var AllUserIDs = Allhouseholdgroupinfo?.FirstOrDefault()?.userdetailslist?
    //    .Where(m => m != null && !string.Equals(m.household_individual_status, "Withdrawn", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(m.household_individual_userid))
    //    .Select(m => m.household_individual_userid)
    //    .Distinct()
    //    .ToList() ?? new List<string>();

    //            if (AllUserIDs.Count == 0)
    //            {
    //                allQuestionnairesOrdered = new List<newuserquestionnaire>();
    //                return;
    //            }

    //            var questionnaireTasks = AllUserIDs
    //                .Select(APICalls.Instance.GetUserQuestionnairesbyUserid)
    //                .ToList();

    //            var results = await Task.WhenAll(questionnaireTasks);

    //            allQuestionnairesOrdered = results
    //                .Where(r => r != null)
    //                .SelectMany(r => r)
    //                .OrderByDescending(q => q.DateTimeAdded)
    //                .ToList();

    //            foreach (var item in Allhouseholdgroupinfo[0].userdetailslist)
    //            {
    //                bool isRep = item.household_individual_relationship.Equals("Household Rep", StringComparison.OrdinalIgnoreCase);
    //                bool isWithdrawn = item.household_individual_status.Equals("Withdrawn", StringComparison.OrdinalIgnoreCase);
    //                bool isOnboarding = item.household_individual_status.Equals("Onboarding", StringComparison.OrdinalIgnoreCase);
    //                bool isActive = item.household_individual_status.Equals("Active", StringComparison.OrdinalIgnoreCase);

    //                // Status icon
    //                item.Studyactiveimage = isActive ? "greentick.png"
    //                                      : isOnboarding ? "error.png"
    //                                      : "logout.png";

    //                // Study info label
    //                item.Studyinfo = $"{item.household_individual_userid} � " +
    //                                  $"{item.household_individual_status}";

    //                // Withdrawn: fade card, hide everything
    //                if (isWithdrawn)
    //                {
    //                    item.ListOpacity = 0.7;
    //                    item.ShowActions = false;
    //                    item.ShowBaselineText = false;
    //                    item.BaselineButtonOpacity = 1;
    //                    item.BaselineButtonEnabled = false;
    //                    item.ManageButtonOpacity = 1;
    //                    item.ManageButtonEnabled = false;
    //                    item.LastActiveDate = null;
    //                    item.QuestionnairesCompleted = "-";
    //                    item.BaselineFormStatusText = "-";
    //                    item.BaselineFormStatusColor = "#9ca3af";
    //                    item.BaselineFormStatusIcon = null;
    //                    item.BaselineSamplesStatusText = "-";
    //                    item.BaselineSamplesStatusColor = "#9ca3af";
    //                    item.BaselineSamplesStatusIcon = null;
    //                    continue;
    //                }

    //                // Household Rep: no actions, no stats
    //                if (isRep)
    //                {
    //                    item.ListOpacity = 1;
    //                    item.ShowBaselineText = false;
    //                    item.BaselineButtonOpacity = 1;
    //                    item.BaselineButtonEnabled = false;
    //                    item.ManageButtonOpacity = 1;
    //                    item.ManageButtonEnabled = false;
    //                    item.LastActiveDate = null;
    //                    item.QuestionnairesCompleted = "-";
    //                    item.BaselineFormStatusText = "-";
    //                    item.BaselineFormStatusColor = "#9ca3af";
    //                    item.BaselineFormStatusIcon = null;
    //                    item.BaselineSamplesStatusText = "-";
    //                    item.BaselineSamplesStatusColor = "#9ca3af";
    //                    item.BaselineSamplesStatusIcon = null;
    //                    continue;
    //                }

    //                // ---- Populate stats for non-rep, non-withdrawn members only ----
    //                var memberQuestionnaires = allQuestionnairesOrdered
    //                    .Where(q => q.userid == item.household_individual_userid)
    //                    .ToList();

    //                item.QuestionnairesCompleted = memberQuestionnaires.Count.ToString();

    //                if (memberQuestionnaires.Count == 0)
    //                {
    //                    item.QuestionnairesCompleted = "--";
    //                }

    //                var newestQuestionnaire = memberQuestionnaires.FirstOrDefault();
    //                item.LastActiveDate = newestQuestionnaire != null
    //                    ? newestQuestionnaire.DateTimeAdded.ToString("dd MMM yyyy")
    //                    : "--";

    //                item.ListOpacity = 1;
    //                item.ShowActions = true;

    //                // ---- Baseline Form status ----
    //                bool baselineFormCompleted = !isOnboarding;
    //                item.BaselineFormStatusText = baselineFormCompleted ? "Completed" : "Pending";
    //                item.BaselineFormBorderColor = new SolidColorBrush(Color.FromArgb(baselineFormCompleted ? "#009fe3" : "#eeeeee"));
    //                item.ShowAwaitingBaseline = baselineFormCompleted ? false : true;
    //                item.BaselineFormTextColor = baselineFormCompleted ? "#009fe3" : "#031926";
    //                item.ShowActions = (isRep || isWithdrawn) ? false : true; 
                

    //                // ---- Baseline Samples status ----
    //                bool baselineSamplesCompleted = memberQuestionnaires.Any(q =>
    //                    q.questionnaireid.Equals("b1_samples", StringComparison.OrdinalIgnoreCase));

    //                item.BaselineSamplesStatusText = baselineSamplesCompleted ? "Completed" : "Pending";
    //                item.BaselineSamplesBorderColor = new SolidColorBrush(Color.FromArgb(baselineSamplesCompleted ? "#009fe3" : "#eeeeee"));
    //                item.BaselineSamplesTextColor = baselineSamplesCompleted ? "#009fe3" : "#031926";
            
                 

    //                if (isOnboarding)
    //                {
    //                    item.ShowBaselineText = true;
    //                    item.BaselineButtonOpacity = 1;
    //                    item.BaselineButtonEnabled = true;
    //                    item.ManageButtonOpacity = 0.2;
    //                    item.ManageButtonEnabled = false;
    //                    item.SendReminderText = "Send Email Reminder";
    //                }
    //                else if (isActive)
    //                {
    //                    item.ShowBaselineText = false;
    //                    item.BaselineButtonOpacity = 0.2;
    //                    item.BaselineButtonEnabled = false;
    //                    item.ManageButtonOpacity = 1;
    //                    item.ManageButtonEnabled = true;
    //                    item.SendReminderText = "Nudge User - Notification";
    //                }
    //            }

    //            var sortedList = Allhouseholdgroupinfodetails
    //                .OrderBy(x => x.household_individual_relationship == "Household Rep" ? 0 : 1)
    //                .ThenBy(x => x.household_individual_status)
    //                .ToList();

    //            profilelist.ItemsSource = sortedList;
    //            profilelist.RefreshView();

    //            var activeCount = Allhouseholdgroupinfodetails.Count(x => x.household_individual_status.Equals("Active", StringComparison.OrdinalIgnoreCase));

    //            familycountlbl.Text = activeCount.ToString();

    //            addimage1.Source = activeCount >= 1 ? "adduserblue.png" : "addusergray.png";
    //            addimage2.Source = activeCount >= 2 ? "adduserblue.png" : "addusergray.png";
    //            addimage3.Source = activeCount >= 3 ? "adduserblue.png" : "addusergray.png";

    //            percentpb.Progress = activeCount;

    //            //Get all Questionnaire Data 


    //            RecentComeplted();
    //            findoutstudyprogress();

    //        }
    //    }
    //    catch (Exception Ex)
    //    {
    //        // log ex
    //    }
    //}

    private async Task checknotificationEnabled()
    {
        try
        {
            if (!TempHideNotif)
            {
                bool isEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();
                EnableNotificationStack.IsVisible = !isEnabled;
            }
            else
            {
                EnableNotificationStack.IsVisible = false; 
            }

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "checknotificationEnabled");
        }
    }

    private async Task SelectNotificationTime(TEventMember Item, DateTime t1Start)
    {
        try
        {
            if (Item?.questionnaires == null || !Item.questionnaires.Any()) return;
            bool isT1Complete = Item.questionnaires.Any(q => q.questionnaire_type == "T1");
            if (!isT1Complete) return;

            var notificationTime = Preferences.Get("notificationtime", string.Empty);
            if (string.IsNullOrEmpty(notificationTime))
            {
                await MopupService.Instance.PushAsync(new SelectNotificationTime(), false);
            }

            if (Preferences.Get("weekly_notification_id", 0) == 0)
            {
                await AddNotification.ScheduleWeeklyNotification(t1Start);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "SelectNotificationTime");
        }
    }

    private DateTime ReasonableTime(DateTime originalDate)
    {
        //Greater than 5pm and less than 10am fix 
        if (originalDate.Hour >= 17)
        {
            return originalDate.Date.AddDays(1).AddHours(10);
        }

        if (originalDate.Hour < 10)
        {
            return originalDate.Date.AddHours(10);
        }

        return originalDate;
    }
    async void findoutstudyprogress()
    {
        try
        {
            //make sure evyerthing is hidden first for refresh
            t1questionnairebordermain.IsVisible = false;
            stage1awaitingbaseline.IsVisible = false;
            stage2basleinesample2.IsVisible = false;
            stage2basleinesample2helper.IsVisible = false;
            completedtaskborder.IsVisible = false;
            tsamplingborder.IsVisible = false;
            //stage one - await baseline 



            if (DateTime.Now.Date >= new DateTime(2027, 4, 23))
            {
                // T28 done - check if end of study form completed
                bool completedEndOfStudy = allQuestionnairesOrdered
                    .Any(q => q.userid == Helpers.Settings.UsersID
                           && q.questionnaireid == "end_of_study");

                if (!completedEndOfStudy)
                {
                    // Show end of study form card
                    endofstudyborder.IsVisible = true;
                    tsamplingborder.IsVisible = false;
                    completedtaskborder.IsVisible = false;
                    return;
                }
                else
                {
                    //Destroy Local Notifications (Basically Awaitin) 
                    if (Preferences.Default.Get("daily_notification_id", 0) != 0 || Preferences.Default.Get("weekly_notification_id", 0) != 0)
                    {
                        await AddNotification.UpdateUserTime(true);
                    }

                    endofstudyborder.IsVisible = false;
                    // Everything complete
                    studycompleteborder.IsVisible = true;
                    return;
                }
            }



            if (Helpers.Settings.SignUp == "HOPPERCR")
            {

                //_allHouseholdGroup.groupuserdetails = JsonConvert.SerializeObject(_allGroupDetailsPassed);


                // -- Study record update --
                var studyDetails1 = Allhouseholdgroupinfo[0].studydetails;

                if (studyDetails1 == null)
                {
                    studyDetails1 = new householdstudyrecord
                    {
                        household_id = Allhouseholdgroupinfo[0].householdgroupid,
                        current_phase = "TPhase",
                        t_events = new List<TEvent>()
                    };



                    studyDetails1.t_events ??= new List<TEvent>();

                    var activeEvent1 = studyDetails1.t_events
                        .FirstOrDefault(x => x.t_event_status == "active");

                    if (activeEvent1 == null)
                    {
                        activeEvent1 = new TEvent
                        {
                            t_event_id = "TE-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                            t_event_status = "active",
                            scenario = null,
                            trigger_type = "HOPPERCR",
                            t1_start_date = DateTime.UtcNow.ToString("g"),
                            members = Allhouseholdgroupinfo[0].userdetailslist?
                                .Select(m => new TEventMember
                                {
                                    user_id = m.household_individual_userid,
                                    daily_forms_complete = false,
                                    daily_forms_stopped_at = null,
                                    questionnaires = new List<TQuestionnaire>()
                                }).ToList() ?? new List<TEventMember>()
                        };
                        studyDetails1.t_events.Add(activeEvent1);
                    }


                    Allhouseholdgroupinfo[0].details = System.Text.Json.JsonSerializer.Serialize(studyDetails1);


                    //insert into db


                    var updateData = new { details = Allhouseholdgroupinfo[0].details };
                    string updateJson = System.Text.Json.JsonSerializer.Serialize(updateData);


                    string url = $"{APICalls.ApplicationURL}householdgroup/householdgroupid/{Allhouseholdgroupinfo[0].householdgroupid}";
                    using var content = new StringContent(updateJson, Encoding.UTF8, "application/json");
                    var response = await APICalls.Instance.GetClient().PatchAsync(url, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        _ = await response.Content.ReadAsStringAsync();
                    }



                }
            }



      
                //check if less than 3 are active


                var studyDetailscheck = Allhouseholdgroupinfo[0].studydetails;

            if (Helpers.Settings.SignUp == "HOPPERCTPE")
            {

                if (studyDetailscheck == null)
                {

                    //has not started t forms so still at waiting stage




                    var activeCount = Allhouseholdgroupinfodetails.Where(x => x.household_individual_status.Equals("Active", StringComparison.OrdinalIgnoreCase)).ToList();

                    if (activeCount.Count < 3)
                    {
                        stage1awaitingbaseline.IsVisible = true;
                        return;
                    }



                    //stage 2 basline samples


                    //if (activeCount.Any(user => !allQuestionnairesOrdered.Any(q => q.userid == user.household_individual_userid && q.questionnaireid == "b1_samples")))
                    //{
                    //    stage2basleinesample2.IsVisible = true;
                    //    return;
                    //}

                    var usersMissingB1Samples = activeCount.Where(user => !AllUserQuestionnaires.Any(q => q.userid == user.household_individual_userid && q.questionnaireid == "b1_samples")).ToList();


                    if (usersMissingB1Samples.Any())
                    {


                        // Inspect the missing users here

                        var names = string.Join(", ", usersMissingB1Samples.Select(u => u.household_individual_name));

                        var formatted = new FormattedString();

                        formatted.Spans.Add(new Span
                        {
                            TextColor = Color.FromHex("#031926"),
                            Text = "The following household members still need to complete baseline samples:\n\n"
                        });

                        for (int i = 0; i < usersMissingB1Samples.Count; i++)
                        {
                            var user = usersMissingB1Samples[i];

                            formatted.Spans.Add(new Span
                            {
                                Text = "• ",
                                TextColor = Color.FromHex("#031926")
                            });

                            formatted.Spans.Add(new Span
                            {
                                Text = user.household_individual_name,
                                TextColor = Color.FromHex("#031926"),
                                FontAttributes = FontAttributes.Bold
                            });

                            // only add newline if NOT last item
                            if (i < usersMissingB1Samples.Count - 1)
                            {
                                formatted.Spans.Add(new Span
                                {
                                    Text = "\n\n",
                                    TextColor = Color.FromHex("#031926")
                                });
                            }
                        }

                        nameslist.FormattedText = formatted;
                        // nameslist.Text = "The following household members still need to complete baseline samples " + names;


                        stage2basleinesample2.IsVisible = true;
                        stage2basleinesample2helper.IsVisible = true;
                        return;
                    }
                }
            }


       


            //tform stage

            var studyDetails = Allhouseholdgroupinfo[0].studydetails;
            if (studyDetails == null)
            {

                //has not started t forms so still at waiting stage

                t1questionnairebordermain.IsVisible = true;
                return;
            }


            var activeEvent = studyDetails.t_events?
      .FirstOrDefault(x => x.t_event_status == "active");

            if (activeEvent == null || Helpers.Settings.SignUp == "HOPPERCTPE")
            {
                //has not started t forms so still at waiting stage
                t1questionnairebordermain.IsVisible = true;
                return;
            }


            // Work out what day they are on
            if (!DateTime.TryParseExact(activeEvent.t1_start_date, "dd/MM/yyyy HH:mm",
         System.Globalization.CultureInfo.InvariantCulture,
         System.Globalization.DateTimeStyles.None, out DateTime t1Start)) return;



            T1Start = ReasonableTime(t1Start); 
            dayNumber = (DateTime.Today - t1Start.Date).Days + 1;
            int totalDays = 28;
            string todaysForm = $"T{dayNumber}";

            // Find the current user's member record
            var member = activeEvent.members?
                .FirstOrDefault(m => m.user_id == Helpers.Settings.UsersID);
            Member = member;

            if (member == null)
            {
                //this is needed if the user has been added late on

                member = new TEventMember();
                member.user_id = Helpers.Settings.UsersID;
                member.daily_forms_complete = false;
                member.daily_forms_stopped_at = null;
                member.questionnaires = new List<TQuestionnaire>();
            }


            var missedDays = GetMissedQuestionnaireDays(member, dayNumber);

            bool hasMissedDays = missedDays != null && missedDays.Count > 0;

            //missedlist.IsVisible = hasMissedDays;
            MissedQuestions.IsVisible = hasMissedDays;

            if (hasMissedDays)
            {
                MissedList = missedDays.Select(d => $"T{d}").ToList();
                MissingQNumber.Text = $"{missedDays.Count()} Pending Forms"; 
                //missedlist.ItemsSource = missedDays.Select(d => $"T{d}").ToList();
            }


            // Update progress bars regardless of scenario
            int firstBarProgress = Math.Min(dayNumber, 14);
            int secondBarProgress = dayNumber > 14 ? dayNumber - 14 : 0;
            firstpb.Progress = firstBarProgress;
            secondpb.Progress = secondBarProgress;

            if(dayNumber > 28)
            {
                dayNumber = 28;
            }

            daylbl.Text = $"Day {dayNumber}";
            tdayinfotext.Text = GetDashboardText(activeEvent, dayNumber);


            //waiting info
            waitingfirstpb.Progress = firstBarProgress;
            waitingsecondpb.Progress = secondBarProgress;
            waitingdaylbl.Text = $"Day {dayNumber}";




          ////  Day 28 - T28 form required for everyone
            if (dayNumber >= 28)
                {
                    bool completedT28 = member.questionnaires
                        .Any(q => q.questionnaire_type == "T28");

                    if (!completedT28)
                    {
                        // Show T28 form card
                        tsamplingborder.IsVisible = true;
                        completedtaskborder.IsVisible = false;
                        daylbl.Text = "Day 28";
                        return;
                    }

                    // T28 done - check if end of study form completed
                    bool completedEndOfStudy = allQuestionnairesOrdered
                        .Any(q => q.userid == Helpers.Settings.UsersID
                               && q.questionnaireid == "end_of_study");

                    if (!completedEndOfStudy)
                    {
                        // Show end of study form card
                        endofstudyborder.IsVisible = true;
                        tsamplingborder.IsVisible = false;
                        completedtaskborder.IsVisible = false;
                        return;
                    }

                // Everything complete
                     studycompleteborder.IsVisible = true;
                    tsamplingborder.IsVisible = false;
                    endofstudyborder.IsVisible = false;
                    return;
                }

            //if (dayNumber == 1)
            //{
            //    // Has this member submitted today's form
            //    bool completedToday = member.questionnaires
            //        .Any(q => q.questionnaire_type == todaysForm);

            //    if (completedToday)
            //    {
            //        // Show completed card
            //        completedtaskborder.IsVisible = true;
            //        t1questionnairebordermain.IsVisible = false;
            //       // tsamplingborder.IsVisible = false;
            //    }
            //    else
            //    {
            //        // Show the sampling/questionnaire card
            //        t1questionnairebordermain.IsVisible = true;
            //        completedtaskborder.IsVisible = false;

            //        //// First bar: days 1-14, caps at 14
            //        //int firstBarProgress = Math.Min(dayNumber, 14);

            //        //// Second bar: only fills once past day 14
            //        //int secondBarProgress = dayNumber > 14 ? dayNumber - 14 : 0;

            //        //firstpb.Progress = firstBarProgress;
            //        //secondpb.Progress = secondBarProgress;

            //        //// Update the day label
            //        //daylbl.Text = $"Day {dayNumber}";
            //    }

            //    return;
            //}


            if (dayNumber >= 1 && dayNumber <= 3)
            {
                // Has this member submitted today's form
                bool completedToday = member.questionnaires
                    .Any(q => q.questionnaire_type == todaysForm);

                if (completedToday)
                {
                    // Show completed card
                    completedtaskborder.IsVisible = true;
                    tsamplingborder.IsVisible = false;
                }
                else
                {
                    // Show the sampling/questionnaire card
                    tsamplingborder.IsVisible = true;
                    completedtaskborder.IsVisible = false;

                    //// First bar: days 1-14, caps at 14
                    //int firstBarProgress = Math.Min(dayNumber, 14);

                    //// Second bar: only fills once past day 14
                    //int secondBarProgress = dayNumber > 14 ? dayNumber - 14 : 0;

                    //firstpb.Progress = firstBarProgress;
                    //secondpb.Progress = secondBarProgress;

                    //// Update the day label
                    //daylbl.Text = $"Day {dayNumber}";
                }

                return;
            }

            //after this part we need to check what scenario 
            if (activeEvent.scenario == null)
            {
                // Has this member submitted today's form
                bool completedToday = member.questionnaires
                    .Any(q => q.questionnaire_type == todaysForm);

                if (completedToday)
                {
                    // Show completed card
                    completedtaskborder.IsVisible = true;
                    tsamplingborder.IsVisible = false;
                }
                else
                {
                    // Show the sampling/questionnaire card
                    tsamplingborder.IsVisible = true;
                    completedtaskborder.IsVisible = false;

                    //// First bar: days 1-14, caps at 14
                    //int firstBarProgress = Math.Min(dayNumber, 14);

                    //// Second bar: only fills once past day 14
                    //int secondBarProgress = dayNumber > 14 ? dayNumber - 14 : 0;

                    //firstpb.Progress = firstBarProgress;
                    //secondpb.Progress = secondBarProgress;

                    //// Update the day label
                    //daylbl.Text = $"Day {dayNumber}";
                }

                return;
            }


            if(activeEvent.scenario == "A")
            {
                // Has this member submitted today's form
                bool completedToday = member.questionnaires
                    .Any(q => q.questionnaire_type == todaysForm);

                if (completedToday)
                {
                    // Show completed card
                    completedtaskborder.IsVisible = true;
                    tsamplingborder.IsVisible = false;
                }
                else
                {


                    bool checkiftheyhavefinished = member.daily_forms_complete;

                    if (checkiftheyhavefinished)
                    {
                        waitingfor28border.IsVisible = true;
                        tsamplingborder.IsVisible = false;
                        completedtaskborder.IsVisible = false;
                    }
                    else
                    {



                        // Show the sampling/questionnaire card
                        tsamplingborder.IsVisible = true;
                        completedtaskborder.IsVisible = false;
                    }

                    //// First bar: days 1-14, caps at 14
                    //int firstBarProgress = Math.Min(dayNumber, 14);

                    //// Second bar: only fills once past day 14
                    //int secondBarProgress = dayNumber > 14 ? dayNumber - 14 : 0;

                    //firstpb.Progress = firstBarProgress;
                    //secondpb.Progress = secondBarProgress;

                    //// Update the day label
                    //daylbl.Text = $"Day {dayNumber}";
                }

                return;
            }
            else
            {
                // Scenario B and C
                bool completedTodayBC = false;
                foreach (var q in member.questionnaires)
                {
                    System.Diagnostics.Debug.WriteLine($"comparing '{q.questionnaire_type}' == '{todaysForm}' : {q.questionnaire_type == todaysForm}");
                    if (q.questionnaire_type == todaysForm)
                    {
                        completedTodayBC = true;
                        break;
                    }
                }

                // Only check for household clear if today's form is done
                // This gives everyone the chance to complete today before closing the event
                if (!completedTodayBC)
                {
                    bool householdCleared = CheckHouseholdThreeDayClear(activeEvent);

                    if (householdCleared)
                    {
                        if (activeEvent.t_event_status != "complete")
                        {
                            activeEvent.t_event_status = "complete";
                           // studyDetails.current_phase = "AwaitingSamples";

                            Allhouseholdgroupinfo[0].details = System.Text.Json.JsonSerializer.Serialize(studyDetails);
                            var updateData = new { details = Allhouseholdgroupinfo[0].details };
                            string updateJson = System.Text.Json.JsonSerializer.Serialize(updateData);
                            var url = $"{APICalls.ApplicationURL}householdgroup/householdgroupid/{Allhouseholdgroupinfo[0].householdgroupid}";
                            var content = new StringContent(updateJson, Encoding.UTF8, "application/json");
                            await APICalls.Instance.GetClient().PatchAsync(url, content);

                            //Destroy Daily Notifications as symptom free
                            if (Preferences.Default.Get("daily_notification_id", 0) != 0 || Preferences.Default.Get("weekly_notification_id", 0) != 0)
                            {
                                await AddNotification.UpdateUserTime(true);
                            }

                            t1questionnairebordermain.IsVisible = true;
                            waitinglbl.Text = LocalizationManager.Get("Dashboard_SamplingSymptoms");
                        }
                        else
                        {
                            // Already marked complete on a previous load
                            completedtaskborder.IsVisible = true;
                            tsamplingborder.IsVisible = false;
                        }

                       
                        return;
                    }
                    else
                    {
                        tsamplingborder.IsVisible = true;
                      //  completedtaskborder.IsVisible = completedTodayBC;
                    }
                }

                // Not yet cleared - show today's form or completed card
                tsamplingborder.IsVisible = !completedTodayBC;
                completedtaskborder.IsVisible = completedTodayBC;
                return;
            }

            //Run this code Last so the popup shows after content loaded
            //await SelectNotificationTime(member);
        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "findoutstudyprogress");
        }
    }

    private bool CheckHouseholdThreeDayClear(TEvent activeEvent)
    {
        if (!DateTime.TryParseExact(activeEvent.t1_start_date,
          new[] { "dd/MM/yy HH:mm", "dd/MM/yyyy HH:mm", "g" },
          System.Globalization.CultureInfo.CurrentCulture,
          System.Globalization.DateTimeStyles.None,
          out DateTime t1Start)) return false;

        int dayNumber = (DateTime.Today - t1Start.Date).Days + 1;
        if (dayNumber < 3) return false;

        foreach (var m in activeEvent.members)
        {
            var submittedDays = m.questionnaires
                .Select(q => q.questionnaire_type)
                .Where(t => t.StartsWith("T") && int.TryParse(t[1..], out _))
                .Select(t => int.Parse(t[1..]))
                .Where(d => d >= 4)
                .OrderByDescending(d => d)
                .ToList();

            bool memberCleared = false;

            // No daily forms submitted - treat as symptom-free
            if (submittedDays.Count == 0)
            {
                memberCleared = true;
            }
            else
            {
                for (int i = 0; i < submittedDays.Count - 1; i++)
                {
                    int d3 = submittedDays[i];
                    int d2 = d3 - 1;
                    int d1 = d3 - 2;

                    if (d1 <= 3 || d2 <= 3 || d3 <= 3) continue;

                    var form3 = m.questionnaires.FirstOrDefault(q => q.questionnaire_type == $"T{d3}");
                    var form2 = m.questionnaires.FirstOrDefault(q => q.questionnaire_type == $"T{d2}");
                    var form1 = m.questionnaires.FirstOrDefault(q => q.questionnaire_type == $"T{d1}");

                    if (form1 != null && form2 != null && form3 != null &&
                        !form1.has_symptoms && !form2.has_symptoms && !form3.has_symptoms)
                    {
                        memberCleared = true;
                        break;
                    }
                }
            }

            if (!memberCleared) return false;
        }

        return true;
    }

    private string GetDashboardText(TEvent activeEvent, int dayNumber)
    {
        bool isReactive = activeEvent.trigger_type == "HOPPERCR";
        bool isEarlyDays = dayNumber <= 3;

        // D1-D3 Reactive
        if (isReactive && isEarlyDays)
            return "Please collect any samples due today using the kits we sent you. Once you have collected your samples, tap here to complete today's questionnaire.";

        // D1-D3 Pre-emptive
        if (!isReactive && isEarlyDays)
            return "This is because you, or someone else in your household, has reported symptoms.\n\nPlease collect any samples due today using the kits we sent you. Once you have collected your samples, tap here to complete today's questionnaire.";

        // D4-D27 Reactive (Scenario A)
        if (isReactive && !isEarlyDays && activeEvent.scenario == "A")
            return "Please check whether you need to collect any samples today. Tap here to complete today's questionnaire.";

        // D4-D27 Pre-emptive Scenario A (positive LFD)
        if (!isReactive && !isEarlyDays && activeEvent.scenario == "A")
            return "This is because you, or someone else in your household, has received a positive result on a lateral flow device (LFD) test for one of the viruses being studied in HOPPER.\n\nTap here to complete today's questionnaire.";

        // D4-D27 Scenario B&C (no positive LFD)
        samplinglbl.Text = " of your triggered survey period";
        return "This is because you, or someone else in your household, reported symptoms but did not have a positive lateral flow device (LFD) test result for any of the viruses being studied in HOPPER.\n\nTap here to complete today's questionnaire.\n\nOnce no one in your household has reported symptoms for 3 consecutive days, your household will return to the waiting stage.";
    }


    List<int> GetMissedQuestionnaireDays(TEventMember member, int dayNumber, int totalDays = 28)
    {
        if (member?.questionnaires == null)
        {
            return new List<int>();
        }

        // Pull out the day numbers already completed, e.g. "T5" -> 5
        var completedDays = member.questionnaires
          .Select(q => q?.questionnaire_type)
          .Where(t => !string.IsNullOrEmpty(t) && t.StartsWith("T", StringComparison.OrdinalIgnoreCase) && t.Length > 1)
          .Select(t => int.TryParse(t.Substring(1), out var d) ? d : (int?)null)
          .Where(d => d.HasValue)
          .Select(d => d.Value)
          .ToHashSet();

        // Only check days that have already passed - today isn't "missed" yet
        int lastDayToCheck = Math.Min(dayNumber - 1, totalDays);
        var missedDays = new List<int>();

        for (int day = 1; day <= lastDayToCheck; day++)
        {
            if (!completedDays.Contains(day))
            {
                missedDays.Add(day);
            }
        }

        return missedDays;
    }

    //async void gethouseholddata()
    //{
    //    try
    //    {

    //        //var householdTask = APICalls.Instance.GetUserHouseholdInfo(Helpers.Settings.UsersID);
    //        //var questionnairesTask = APICalls.Instance.GetUserQuestionnaires();

    //        //await Task.WhenAll(householdTask, questionnairesTask);

    //        //Allhouseholdgroupinfo = householdTask.Result;
    //        //AllUserQuestionnaires = questionnairesTask.Result;

    //        // 1. Fetch household info first
    //        Allhouseholdgroupinfo = await APICalls.Instance.GetUserHouseholdInfo(Helpers.Settings.UsersID);

    //        // 2. Get all member IDs from the household
    //        var memberIds = Allhouseholdgroupinfo?[0]?.userdetailslist
    //              .Where(m => !m.household_individual_status.Equals("Withdrawn", StringComparison.OrdinalIgnoreCase))
    //            .Select(m => m.household_individual_userid)
    //            .ToList() ?? new List<string>();

    //        // Also include the primary user if not already in the list
    //        if (!memberIds.Contains(Helpers.Settings.UsersID))
    //            memberIds.Add(Helpers.Settings.UsersID);

    //        // 3. Fetch all questionnaires in parallel
    //        var questionnaireTasks = memberIds
    //            .Select(id => APICalls.Instance.GetUserQuestionnairesbyUserid(id))
    //            .ToList();

    //        await Task.WhenAll(questionnaireTasks);


    //        if(questionnaireTasks != null)
    //        {
    //            var allQuestionnaires = new List<newuserquestionnaire>();

    //            for (int i = 0; i < memberIds.Count; i++)
    //            {
    //                var memberId = memberIds[i];
    //               // var member = Allhouseholdgroupinfo[0].userdetailslist.First(m => m.household_individual_userid == memberId);
    //                var results = questionnaireTasks[i].Result;

    //                foreach (var q in results)
    //                {
    //                  //  q.DisplayMemberName = member.household_individual_name; // add this prop to your model
    //                    allQuestionnaires.Add(q);
    //                }
    //            }

    //            // Sort by most recently completed
    //            var sorted = allQuestionnaires
    //                .OrderByDescending(q => q.DateTimeAdded)
    //                .ToList();

    //            CompletedQuestionList.ItemsSource = sorted;


    //        }


    //        if (Allhouseholdgroupinfo != null)
    //        {


    //            Allhouseholdgroupinfodetails = Allhouseholdgroupinfo[0].userdetailslist;

    //            //add the main user rep
    //            //  if (Helpers.Settings.IsPrimaryUser)
    //            //  {
    //            var mainuser = new householdgroupjsondetails();
    //            mainuser.household_group_id = Allhouseholdgroupinfo[0].userdetailslist[0].household_group_id;
    //            mainuser.household_individual_name = Helpers.Settings.FirstName;
    //            mainuser.household_individual_userid = Helpers.Settings.UsersID;
    //            mainuser.household_individual_status = "active";
    //            mainuser.household_individual_age = "Over 18";
    //            mainuser.household_individual_relationship = "Household Rep";
    //            mainuser.Studyactiveimage = "greentick.png";
    //            mainuser.ListOpacity = 1;
    //            mainuser.Studyinfo = mainuser.household_individual_userid + " . " + mainuser.household_individual_relationship + " . " + mainuser.household_individual_status;
    //            Allhouseholdgroupinfodetails.Insert(0, mainuser);

    //            foreach (var item in Allhouseholdgroupinfo[0].userdetailslist)
    //            {
    //                bool isRep = item.household_individual_relationship.Equals("Household Rep", StringComparison.OrdinalIgnoreCase);
    //                bool isWithdrawn = item.household_individual_status.Equals("Withdrawn", StringComparison.OrdinalIgnoreCase);
    //                bool isOnboarding = item.household_individual_status.Equals("Onboarding", StringComparison.OrdinalIgnoreCase);
    //                bool isActive = item.household_individual_status.Equals("Active", StringComparison.OrdinalIgnoreCase);
    //                //  bool hasBaseline = item.household_hascompletedbaseline?.Trim().ToLower() == "true";

    //                // Status icon
    //                item.Studyactiveimage = isActive ? "greentick.png"
    //                                      : isOnboarding ? "error.png"
    //                                      : "logout.png"; // withdrawn

    //                // Study info label
    //                item.Studyinfo = $"{item.household_individual_userid} � " +
    //                                 $"{item.household_individual_relationship} � " +
    //                                 $"{item.household_individual_status}";

    //                // Withdrawn: fade card, hide everything except name/icon/status
    //                if (isWithdrawn)
    //                {
    //                    item.ListOpacity = 0.7;
    //                    item.ShowActions = false;
    //                    item.ShowBaselineText = false;
    //                    item.BaselineButtonOpacity = 1;
    //                    item.BaselineButtonEnabled = false;
    //                    item.ManageButtonOpacity = 1;
    //                    item.ManageButtonEnabled = false;
    //                    continue;
    //                }

    //                // Household Rep: no actions, no baseline text
    //                if (isRep)
    //                {
    //                    item.ListOpacity = 1;
    //                    item.ShowActions = false;
    //                    item.ShowBaselineText = false;
    //                    item.BaselineButtonOpacity = 1;
    //                    item.BaselineButtonEnabled = false;
    //                    item.ManageButtonOpacity = 1;
    //                    item.ManageButtonEnabled = false;
    //                    continue;
    //                }

    //                // All other statuses � show the action bar
    //                item.ListOpacity = 1;
    //                item.ShowActions = true;

    //                if (isOnboarding)
    //                {
    //                    // Onboarding: hide baseline text, dim baseline + manage buttons
    //                    item.ShowBaselineText = true;
    //                    item.BaselineButtonOpacity = 1;
    //                    item.BaselineButtonEnabled = true;
    //                    item.ManageButtonOpacity = 0.2;
    //                    item.ManageButtonEnabled = false;
    //                    item.SendReminderText = "Send Email Reminder";
    //                }
    //                else if (isActive)
    //                {

    //                    // Completed baseline: hide text, dim baseline button only
    //                    item.ShowBaselineText = false;
    //                    item.BaselineButtonOpacity = 0.2;
    //                    item.BaselineButtonEnabled = false;
    //                    item.ManageButtonOpacity = 1;
    //                    item.ManageButtonEnabled = true;
    //                    item.SendReminderText = "Nudge User - Notification";


    //                }
    //            }

    //            // }

    //            var sortedList = Allhouseholdgroupinfodetails
    //.OrderBy(x => x.household_individual_status)
    //.ToList();


    //            profilelist.ItemsSource = sortedList;

    //        }

    //            //            profilelist.ItemsSource = sortedList;
    //            //            //familycountlbl.Text = sortedList.Count.ToString();


    //            //            // 1. Get the total count
    //            //            double totalUsers = Allhouseholdgroupinfodetails.Count;

    //            //            // 2. Get the count of active users
    //            //            double activeCount = Allhouseholdgroupinfodetails.Count(x => x.household_individual_status.Equals("Active", StringComparison.OrdinalIgnoreCase));

    //            //            // 3. Calculate percentage
    //            //            double percentageActive = 0;
    //            //            if (totalUsers > 0)
    //            //            {
    //            //                percentageActive = (activeCount / totalUsers) * 100;
    //            //            }

    //            //            percentpb.Progress = percentageActive;
    //            //            percentlbl.Text = percentageActive.ToString("F0") + "%";


    //            //            // Progress bar fills one segment per active member
    //            //            percentpb.Progress = activeCount;
    //            //            familycountlbl.Text = activeCount.ToString();
    //            //            // Member icons - blue for each active slot, gray otherwise
    //            //            addimage1.Source = activeCount >= 1 ? "adduserblue.png" : "addusergray.png";
    //            //            addimage2.Source = activeCount >= 2 ? "adduserblue.png" : "addusergray.png";
    //            //            addimage3.Source = activeCount >= 3 ? "adduserblue.png" : "addusergray.png";

    //            //        }

    //            //Load 3 Comeplted 
    //            //await RecentComeplted();             

    //    }
    //    catch (Exception Ex)
    //    {

    //    }
    //    finally
    //    {
    //        //await DisplayAlert("DashLoaded", "Loaded", "Ok");
    //    }
    //}

    async Task RecentComeplted()
    {
        try
        {
            AllQuestionnaires = await APICalls.Instance.GetAllQuestionnaire();
            if (AllQuestionnaires == null) return;

            if (AllUserQuestionnaires.Count > 0)
            {



                var rnd = new Random();
                foreach (var item in AllUserQuestionnaires)
                {
                    item.FormattedDateTime = item.DateTimeAdded.ToString("dd MMM, HH:mm");                  
                    item.title = QuestionnaireTitle.TryGetValue(item.questionnaireid, out var title) ? title : item.questionnaireid;
                    item.DisplayTitle = TitleToDisplay(item.title); 
                    item.ShowCompletedBy = ismainuser;
                    item.CompletedBy = UserDetailsKey.TryGetValue(item.userid, out var Name) ? Name : item.userid;
                }

                 MemberSpecificQuestionnaires = AllUserQuestionnaires
                            .Where(q => q.userid == Helpers.Settings.UsersID)
                            .ToList();

                  //var recentItems = (ismainuser ? AllUserQuestionnaires : MemberSpecificQuestionnaires)?
                  //.OrderByDescending(q => q.DateTimeAdded)
                  //.Take(3)
                  //.ToList() ?? new List<newuserquestionnaire>();
                //Fix Refresh
                //CompletedQuestionList.ItemsSource = null;
                //CompletedQuestionList.ItemsSource = recentItems;
                CompletedQuestions.IsVisible = MemberSpecificQuestionnaires.Count > 0;
            }
            else
            {
                //CompletedQuestions.IsVisible = false;
            }
        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "RecentComeplted");
        }
    }

    private string TitleToDisplay(string currentTitle)
    {
        if (string.IsNullOrEmpty(currentTitle))
            return string.Empty;

        var match = Regex.Match(currentTitle, @"^t([1-9]|1[0-9]|2[0-8])_form$");

        return currentTitle switch
        {
            "b1_samples" => "Baseline Sampling Form",
            "b1_individual_questionnaire" => "Baseline Sampling Form",
            "70530492-D1B8-42F5-A851-1C8769288995" => "Withdraw Form",
            "DDD843CF-021B-4557-8824-13C5B4E2EA85" => "B1 Samples List",
            _ when match.Success => $"Day {match.Groups[1].Value} Daily Symptoms and Samples",
            _ => currentTitle
        };
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            //add new member button clicked


            await Navigation.PushAsync(new Addnewmember(Allhouseholdgroupinfo[0]), false);


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Button_Clicked");
        }
    }

    private async Task GetInformationDetails()
    {
        try
        {
            var userList = await APICalls.Instance.GetSingupCode();
            var firstUser = userList?.FirstOrDefault();

            if (firstUser?.informationlist == null)
            {
                infolist.ItemsSource = new ObservableCollection<InformationDetails>();
                return;
            }

            var sourceList = firstUser.informationlist.ToList();

            foreach (var item in sourceList)
            {
                // Apply translation overrides first so the localised values are
                // in place before any further title/type checks run.
                ApplyInformationTranslation(item);

                // Detect FAQ entries by the (possibly already-translated) title
                if (item.title != null && item.title.Contains("Frequently", StringComparison.OrdinalIgnoreCase))
                {
                    item.type = "FAQ's";
                    item.img = "question.png";
                    item.ColorTheme = "#868F96";
                }

                // Phone/email titles are driven by localisation strings regardless of language
                if (item.type == "phone") item.title = LocalizationManager.Get("Info_CallUs");
                if (item.type == "email") item.title = item.description != null && item.description.Contains("imperial") ? LocalizationManager.Get("Info_GeneralEnquiries") : LocalizationManager.Get("Info_TechnicalSupport");
            }


            var contacts = sourceList.Where(item => item.type == "email" || item.type == "phone").OrderByDescending(item => item.type == "phone");
            var generalInfo = sourceList.Where(item => item.type != "email" && item.type != "phone");

            Contactlist.ItemsSource = new ObservableCollection<InformationDetails>(contacts);
            infolist.ItemsSource = new ObservableCollection<InformationDetails>(generalInfo);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "GetInformationDetails");
        }
    }

    /// <summary>
    /// Overwrites the mutable title, description and link fields on an
    /// <see cref="InformationDetails"/> item with the best available
    /// translation for the user's currently selected language.
    /// Falls back to the base (English) value when no translation exists.
    /// </summary>
    private static void ApplyInformationTranslation(InformationDetails item)
    {
        // LocalisedTitle/Description/Link already contain the fallback logic.
        if (item.LocalisedTitle != null)       item.title       = item.LocalisedTitle;
        if (item.LocalisedDescription != null) item.description = item.LocalisedDescription;
        if (item.LocalisedLink != null)        item.link        = item.LocalisedLink;
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) 
    {
        try
        {
            //  var questionnaireid = "DDD843CF-021B-4557-8824-13C5B4E2EA85";
            //  await Navigation.PushAsync(new NewQuestionnairesPage(questionnaireid, AllUserQuestionnaires), false);


            //check if user has already completed questionnaire

            bool alreadyCompleted = AllUserQuestionnaires.Any(q => q.questionnaireid == "b1_samples" && q.userid == Helpers.Settings.UsersID);


            if (alreadyCompleted)
            {
                await DisplayAlert(
    LocalizationManager.Get("Dashboard_BaselineSamplesFormTitle"),
    LocalizationManager.Get("Dashboard_BaselineSamplesFormMsg"),
    LocalizationManager.Get("Common_OK"));
            }
            else
            {


                await Navigation.PushAsync(new B1Questionnaire(AllUserQuestionnaires.ToObservable()), false);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped");
        }
    }

    private async void Seeall_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new NewAllQuestionnaires(Allhouseholdgroupinfo, AllUserQuestionnaires, AllQuestionnaires, MemberSpecificQuestionnaires, UserDetailsKey), false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Seeall_Tapped");
        }
    }

    private async void WithdrawfromStudy(object sender, TappedEventArgs e)
    {
        try
        {
            bool result = await DisplayAlert(LocalizationManager.Get("Dashboard_WithdrawAlertTitle"),
                        LocalizationManager.Get("Dashboard_WithdrawAlertMsg"),
                        LocalizationManager.Get("Dashboard_WithdrawButton"),
                        LocalizationManager.Get("Common_Cancel")); 
            if (result)
            {

                var question = AllQuestionnaires.FirstOrDefault(q => q.questionnaireid == "70530492-D1B8-42F5-A851-1C8769288995"); 
                if(question == null ) return;
                await Navigation.PushAsync(new NewQuestionnairesPage(AllUserQuestionnaires, question, ismainuser), false);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "WithdrawfromStudy");
            CrashDetected.LogCrash(Ex, Navigation, "WithdrawfromStudy");
            CrashDetected.LogCrash(Ex, Navigation, "WithdrawfromStudy");
        }
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        try
        {
            bool Question = await DisplayAlert("Start Sampling", "Are you sure you want to start triggered sampling in your household now?", "Yes", "No");
            if (!Question) return;
            await Navigation.PushAsync(new T1Questionnaire(AllUserQuestionnaires.ToObservable(), Allhouseholdgroupinfo[0]), false);
         //   var questionnaireid = "CBDA3207-C3BE-4FCB-9633-FED8FE58DAA2";
         //   await Navigation.PushAsync(new NewQuestionnairesPage(questionnaireid, AllUserQuestionnaires), false);
        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_1");
        }
    }


    async Task GetProfileData()
    {
        try
        {
            var firstName = Helpers.Settings.FirstName?.Trim();
            var surname = Helpers.Settings.Surname?.Trim();

            //welcomelbl.Text = string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(surname)
            //    ? "Welcome back!"
            //    : $"Hi {firstName} {surname}".Trim();
            var ProfileData = Genericlist.GetProfileItems(); 
            profiledetailslist.ItemsSource = ProfileData;
            //Set Content to visible or Empty Stack
            profiledetailslist.IsVisible = ProfileData.Any();
            PersonalInfoEmpty.IsVisible = !ProfileData.Any();

            bool Set = (dayNumber == 0) ? false : true; 
            var settingItems = await Genericlist.GetSettingItems(Set);

            Initialslbl.Text = SetInitials(firstName, surname);
            emaillbl.Text = !string.IsNullOrEmpty(Helpers.Settings.Email) ? Helpers.Settings.Email : "Example@gmail.com";
            var version = (DeviceInfo.Current.Platform == DevicePlatform.iOS) ?
                  AppInfo.BuildString.ToString() : AppInfo.VersionString.ToString();
            Versionlbl.Text = $"(Release Version: {version})";
            useridprofilelbl.Text = !string.IsNullOrEmpty(Helpers.Settings.UsersID) ? Helpers.Settings.UsersID : "Pending";

            if (settingItems != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Settingslist.ItemsSource = settingItems;
                    Settingslist.IsVisible = settingItems.Any();
                    SettingsInfoEmpty.IsVisible = !settingItems.Any();
                });
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "GetProfileData");
        }
    }

    private static string SetInitials(string firstName, string surname)
    {
        var firstInitial = !string.IsNullOrWhiteSpace(firstName) ? firstName[0].ToString() : "";
        var lastInitial = !string.IsNullOrWhiteSpace(surname) ? surname[0].ToString() : "";
        var initials = $"{firstInitial}{lastInitial}";
        return !string.IsNullOrEmpty(initials) ? initials.ToUpper() : "PW";
    }

    //private async void profilelist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    //{
    //    try
    //    {

    //       // var stream = await FileSystem.OpenAppPackageFileAsync("Infosheet510.pdf");


    //        var stream = await FileSystem.OpenAppPackageFileAsync("Infosheet510.pdf");

    //        await Navigation.PushModalAsync(new pdfpage(stream));
    //        // pdfViewer.LoadDocument(stream);

    //    }
    //    catch (Exception Ex)
    //    //     var notification = new NotificationRequest
    //    //     {
    //    //         NotificationId = 67,
    //    //         Title = "Test Notification",
    //    //         Description = "This is a Test",
    //    //         BadgeNumber = 0,
    //    //         //Sound = DeviceInfo.Platform == DevicePlatform.Android ? "pwjingo" : "pwjingo.aiff",
    //    //         Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions
    //    //         {
    //    //             Priority = Plugin.LocalNotification.AndroidOption.AndroidPriority.Max, 
    //    //             Ongoing = false,
    //    //             ChannelId = "pwr_notifications",

    //    //         },
    //    //         Schedule = new NotificationRequestSchedule
    //    //         {
    //    //             NotifyTime = DateTime.Now.AddSeconds(30),
    //    //             RepeatType = NotificationRepeat.No,
    //    //             NotifyRepeatInterval = null
    //    //         }
    //    //     };

    //    //     await LocalNotificationCenter.Current.Show(notification);
    //    // }
    //    // catch (Exception Ex)
    //    {
    //        CrashDetected.LogCrash(Ex, Navigation, "profilelist_ItemTapped");
    //    }
    //}

    private async void CheckNotifications()
    {
        try
        {
            bool isEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();

            if (!isEnabled)
            {
                await Launcher.Default.OpenAsync("app-settings:");
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "CheckNotifications");
        }
    }

    private void tabsview_TabItemTapped(object sender, Syncfusion.Maui.TabView.TabItemTappedEventArgs e)
    {
        try
        {
            ResetTabs();

            // Compare by reference (x:Name) so it works regardless of language
            var tapped = e.TabItem;
            if      (ReferenceEquals(tapped, hometab))    SetActiveTab(hometab,    "dashiconactive.png");
            else if (ReferenceEquals(tapped, infotab))    SetActiveTab(infotab,    "dashexploreactive.png");
            else if (ReferenceEquals(tapped, profiletab)) SetActiveTab(profiletab, "dashbrowseactive.png");

                //case "Questions":
                //    SetActiveTab(additionalquestionstab, "questiondashblack.png");
                //    break;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "tabsview_TabItemTapped");
        }
    }

    private void ResetTabs()
    {
        // Images
        hometab.ImageSource = ImageSource.FromFile("dashiconinactive.png");
        infotab.ImageSource = ImageSource.FromFile("dashexploreinactive.png");
        profiletab.ImageSource = ImageSource.FromFile("dashbrowseinactive.png");
        //additionalquestionstab.ImageSource = ImageSource.FromFile("questiondashgrey.png");

        // Text color
        var inactiveColor = Color.FromArgb("#b3babd");

        hometab.TextColor = inactiveColor;
        infotab.TextColor = inactiveColor;
        profiletab.TextColor = inactiveColor;
        //additionalquestionstab.TextColor = inactiveColor;

        // Font
        hometab.FontFamily = "HankenGroteskRegular";
        infotab.FontFamily = "HankenGroteskRegular";
        profiletab.FontFamily = "HankenGroteskRegular";
        //additionalquestionstab.FontFamily = "HankenGroteskRegular";
    }

    private void SetActiveTab(dynamic tab, string activeImage)
    {
        tab.ImageSource = ImageSource.FromFile(activeImage);
        tab.TextColor = Color.FromArgb("#031926");
        tab.FontAttributes = FontAttributes.Bold; 
        tab.FontFamily = "HankenGroteskBold";
    }



    private async void profiledetailslist_Tapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var Item = e.DataItem as user;
            if (Item == null) return;
            //if(Item.Id == "Name") return; 
            await Navigation.PushAsync(new NewProfileEdit(Item), false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "profiledetailslist_Tapped");
        }
    }


   

    private async void Settingslist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var Item = e.DataItem as user;
            if (Item == null) return;

            var itemId = Item.Id ?? Item.Title; 

            if(itemId == "Sign-up Code") return;
            if(itemId == "HouseHold ID") return;
            if (itemId == "Notifications")
            {
                if (Item.Role == LocalizationManager.Get("Settings_Disabled") || Item.Role == "Disabled")
                {
                    AppInfo.ShowSettingsUI(); 
                    return; 
                }
                else
                {
                    return;
                }
            }
            if (itemId == "Reset Password")
            {
                await Navigation.PushAsync(new ForgotPassword("Reset"), false);
                return;
            }
            if (itemId == "Notification Schedule")
            {
                try
                {
                    await MopupService.Instance.PushAsync(new SelectNotificationTime(), false);
                }
                catch (Exception popupEx)
                {
                    CrashDetected.LogCrash(popupEx, Navigation, "Settingslist_ItemTapped_NotifSchedule");
                }
                return; 
            }
            if(itemId == "Consent")
            {
                var primaryHousehold = Allhouseholdgroupinfo?.FirstOrDefault();
                if (primaryHousehold == null)  return;
                await Navigation.PushAsync(new ViewConsent(primaryHousehold.primaryuserid, primaryHousehold), false);
                return;
            }

            if (itemId == "Select Language")
            {
                // Use TCS pattern so we can detect a change and rebuild the page tree
                var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                string previousCode = Helpers.Settings.SelectedLanguage ?? "en";
                await MopupService.Instance.PushAsync(new SelectLanguagePopup(tcs));
                string selectedCode = await tcs.Task;
                if (selectedCode != previousCode)
                {
                    LocalizationManager.SetLanguage(selectedCode);
                    await App.SetMainPage(new NavigationPage(new ImperialDashboard()));
                }
                return;
            }
            await Navigation.PushAsync(new NewProfileEdit(Item), false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Settingslist_ItemTapped");
        }
    }

    private async void Logout_Clicked(object sender, EventArgs e)
    {
        try
        {
            //Connectivity Changed 
            NetworkAccess accessType = Connectivity.Current.NetworkAccess;
            if (accessType == NetworkAccess.Internet)
            {
                LogoutBtn.IsEnabled = false;
                bool Answer = await DisplayAlert(LocalizationManager.Get("Dashboard_LogoutTitle"), LocalizationManager.Get("Dashboard_LogoutMsg"), LocalizationManager.Get("Dashboard_LogoutButton"), LocalizationManager.Get("Common_Cancel"));
                if (Answer)
                {
                    Newlogout HandleLogout = new Newlogout("Logout");
                }
                else
                {
                    LogoutBtn.IsEnabled = true;
                }
            }
            else
            {
                var isConnected = accessType == NetworkAccess.Internet;
                ConnectivityChanged?.Invoke(this, isConnected);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Logout_Clicked");
        }
    }

    private async void MessageTeam_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (Email.Default.IsComposeSupported)
            {
                string userId = !string.IsNullOrWhiteSpace(Helpers.Settings.UsersID) ? Helpers.Settings.UsersID : "[Add userid if known]";

                var message = new EmailMessage
                {
                    Subject = "Get in touch",
                    Body = $"Userid: {userId}",
                    BodyFormat = EmailBodyFormat.PlainText,
                    To = new List<string> { "hopper-study@peoplewith.com" }
                };

                await Email.Default.ComposeAsync(message);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "MessageTeam_Clicked");
        }
    }

    private async void T1Questionnaire_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new T1Questionnaire(AllUserQuestionnaires.ToObservable(), Allhouseholdgroupinfo[0]), false);
        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "T1Questionnaire_Clicked");
        }
    }

    private async void infolist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var ItemTapped = e.DataItem as InformationDetails;
            if (ItemTapped == null) return;

            switch (ItemTapped.type) 
            {
                case "pdf":

                    var stream = await FileSystem.OpenAppPackageFileAsync(ItemTapped.link);
                    await Navigation.PushModalAsync(new pdfpage(stream));
                    break;

                //case "video":
                //    await DisplayAlert("Video", "Play video", "Ok"); 
                //    break;

                case "email":
                    if (Email.Default.IsComposeSupported)
                    {
                        string userId = !string.IsNullOrWhiteSpace(Helpers.Settings.UsersID) ? Helpers.Settings.UsersID : "[Add userid if known]";

                        var message = new EmailMessage
                        {
                            Subject = "Study Information",
                            Body = $"Userid: {userId}",
                            BodyFormat = EmailBodyFormat.PlainText,
                            To = new List<string> { "support@peoplewith.com" }
                        };

                        await Email.Default.ComposeAsync(message);
                    }
                    break;

                case "phone":
                    if (PhoneDialer.Default.IsSupported)
                    {
                        PhoneDialer.Default.Open(ItemTapped.link);
                    }
                    break;
                    

                case "FAQ's":
                    await Navigation.PushAsync(new FAQ_s(), false);
                    break;

                default:
                    await Browser.Default.OpenAsync(ItemTapped.link, BrowserLaunchMode.SystemPreferred);
                    break;
            }

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "infolist_ItemTapped");
        }
    }

    private async void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(Helpers.Settings.UsersID);

            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                var toast = Toast.Make("Copied to clipboard");
                await toast.Show();
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_2");
        }
    }

    public Command CompleteBaselineCommand => new Command(async (param) =>
    {
        try
        {
            var item = param as householdgroupjsondetails;
            if (item == null || !item.BaselineButtonEnabled) return;



            await Navigation.PushAsync(new Imperial(), false);

        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "CompleteBaselineCommand");
        }
        

    });

    private async void TapGestureRecognizer_Tapped_3(object sender, TappedEventArgs e)
    {
        var border = sender as Border;
        var item = border?.BindingContext as householdgroupjsondetails;
        if (item == null || !item.BaselineButtonEnabled) return;

        try
        {
            await MopupService.Instance.PushAsync(new Infopopup("Loading", item));

            var signUpCode = Uri.EscapeDataString(Helpers.Settings.SignUp ?? string.Empty);
            var url = $"{APICalls.CheckSignUpCode}%27{signUpCode}%27";

            var configuredClient = APICalls.Instance.GetClient();
            HttpResponseMessage response = await configuredClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return;

            string content = await response.Content.ReadAsStringAsync();
            var userResponse = JsonConvert.DeserializeObject<ApiResponseSignUpCode>(content);
            var users = userResponse?.Value;

            if (users == null || users.Count == 0) return;

            if (item.household_individual_age == "5 - 10" || item.household_individual_age == "0 - 5")
            {
                var stream = await FileSystem.OpenAppPackageFileAsync("Infosheet510.pdf");
                await Navigation.PushModalAsync(new pdfpage(stream));
            }

            if (Allhouseholdgroupinfo == null || Allhouseholdgroupinfo.Count == 0) return;

            await Navigation.PushAsync(
            new NewImperial(users[0], item, Allhouseholdgroupinfodetails, Allhouseholdgroupinfo[0], true),
            false);
        }
        catch (Exception ex)
        {
            CrashDetected.LogCrash(ex, Navigation, "TapGestureRecognizer_Tapped_3");
        }
        finally
        {
            try
            {
                if (MopupService.Instance.PopupStack.Count > 0)
                    await Task.WhenAny(MopupService.Instance.PopAsync(), Task.Delay(5000));
            }
            catch (Exception popEx)
            {
                CrashDetected.LogCrash(popEx, Navigation, "TapGestureRecognizer_Tapped_3 - PopAsync");
            }
        }
    }

    private async void TapGestureRecognizer_Tapped_4(object sender, TappedEventArgs e)
    {

            if (sender is not Border border || border.BindingContext is not householdgroupjsondetails item)
            {
                return;
            }

            try
            {
                //TODO: Get Harry to Review
                if(item.household_individual_email.Contains("N/A"))
                {
                    await DisplayAlert("No Email Sent", "Cannot send a notification to the account you are acting on behalf of", "OK");                    return;
                    return;
                }

                if(item.household_individual_age == "0 - 5")
                {
                    await DisplayAlert("No Notification Sent", "Cannot send notification to individuals aged 5 or below", "OK");
                    return;
                }
                
                var url = $"{APICalls.SendNudgeNotification}{item.household_individual_userid}";
                var response = await APICalls.Instance.GetClient().GetAsync(url);
                string responseContent = await response.Content.ReadAsStringAsync();

                bool LimitHit = responseContent?.Contains("Limit Hit") ?? false;
                bool isSuccess = response.IsSuccessStatusCode && !LimitHit;

                string title, message;

                if (LimitHit)
                {
                    title = "Notification Limit Hit";
                    message = "Only one reminder is permitted every 6 hours.";
                }
                else
                {
                    title = isSuccess ? "Notification Sent" : "Notification Failed";
                    string statusWord = isSuccess ? "has" : "has not";
                    message = $"Notification {statusWord} been sent to {item.household_individual_name}.";
                }

                await DisplayAlert(title, message, "OK");

            }
            
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_4");
        }
    }

    private async void TapGestureRecognizer_Tapped_5(object sender, TappedEventArgs e)
    {
        try
        {
            //manage user
            var border = sender as Border;
            var item = border?.BindingContext as householdgroupjsondetails;
            if (item == null || !item.ManageButtonEnabled) return;

            await Navigation.PushAsync(new ManageProfile(item, Allhouseholdgroupinfodetails));

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_5");
        }
    }

    private async void TapGestureRecognizer_Tapped_6(object sender, TappedEventArgs e)
    {
        try
        {

           bool Question = await DisplayAlert("Daily Symptoms & Sampling", "Would you like to complete today's symptom and sampling questionnaire?", "Yes", "No");
           if (!Question) return;

            if (dayNumber == 1)
            {
                await Navigation.PushAsync(new T1Questionnaire(AllUserQuestionnaires.ToObservable(), Allhouseholdgroupinfo[0]), false);
            }
            else
            {

                await Navigation.PushAsync(new T1Questionnaire(AllUserQuestionnaires.ToObservable(), Allhouseholdgroupinfo[0], dayNumber), false);
            }
            //   var questionnaireid = "CBDA3207-C3BE-4FCB-9633-FED8FE58DAA2";
            //   await Navigation.PushAsync(new NewQuestionnairesPage(questionnaireid, AllUserQuestionnaires), false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_6");
        }

    }

    private async void TapGestureRecognizer_Tapped_7(object sender, TappedEventArgs e)
    {
        try
        {
            //end of study form
            await Navigation.PushAsync(new EndStudyQuestionnaire(AllUserQuestionnaires.ToObservable(), Allhouseholdgroupinfo[0]), false);
            //   var questionnaireid = "CBDA3207-C3BE-4FCB-9633-FED8FE58DAA2";
            //   await Navigation.PushAsync(new NewQuestionnairesPage(questionnaireid, AllUserQuestionnaires), false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_7");
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

    private void Enable_Clicked(object sender, EventArgs e)
    {
        try
        {
            AppInfo.ShowSettingsUI();
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Enable_Clicked");
        }
    }

    private void NotNow_Clicked(object sender, EventArgs e)
    {
        try
        {
            TempHideNotif = true;
            checknotificationEnabled(); 
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "NotNow_Clicked");
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new FAQ_s(), false);
           // await MopupService.Instance.PushAsync(new SelectNotificationTime(), false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Button_Clicked_1");
        }
    }

    private async void missedlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            //missed day item tap
            //passing the bool to know its a missed questionnaire 

            var Tapped = e.DataItem as string;
            if (Tapped == null) return;

           bool Question = await DisplayAlert("Missed Symptoms & Sampling", "Would you like to complete the following missed symptom and sampling questionnaire?", "Yes", "No");
           if (!Question) return;
           
            var stringday = Tapped.Replace("T", "");
            if (int.TryParse(stringday, out int day))
            {
                await Navigation.PushAsync(new T1Questionnaire(AllUserQuestionnaires.ToObservable(), Allhouseholdgroupinfo[0], day, true), false);
            }
        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "missedlist_ItemTapped");
        }
    }
    
    private async void Contactlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var ItemTapped = e.DataItem as InformationDetails;
            if (ItemTapped == null) return;

            switch (ItemTapped.type)
            { 

                case "email":
                    if (Email.Default.IsComposeSupported)
                    {
                        string userId = !string.IsNullOrWhiteSpace(Helpers.Settings.UsersID) ? Helpers.Settings.UsersID : "[Add userid if known]";

                        var message = new EmailMessage
                        {
                            Subject = ItemTapped.title,
                            Body = $"Userid: {userId}",
                            BodyFormat = EmailBodyFormat.PlainText,
                            To = new List<string> { ItemTapped.description }
                        };

                        await Email.Default.ComposeAsync(message);
                    }
                    break;

                case "phone":
                    if (PhoneDialer.Default.IsSupported)
                    {
                        PhoneDialer.Default.Open(ItemTapped.description);
                    }
                    break;

                default:
                    break;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Contactlist_ItemTapped");
        }
    }

    private async void TapGestureRecognizer_Tapped_8(object sender, TappedEventArgs e)
    {
        try
        {

            //switch profile

            if (sender is not Border border || border.BindingContext is not householdgroupjsondetails item)
            {
                return;
            }

            //switch profile
           // Helpers.Settings.UsersID = item.household_individual_userid;
            Preferences.Default.Set("userid", item.household_individual_userid);

            await MopupService.Instance.PushAsync(new Infopopup("Loading"));
            await Navigation.PushAsync(new ImperialDashboard(), false);

            await Task.Delay(1500);
            Navigation.RemovePage(this);
            await MopupService.Instance.PopAllAsync(false);

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_8");
        }
    }

    private async void MissedQuestionnaires_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
           await Navigation.PushAsync(new NewAllQuestionnaires(Allhouseholdgroupinfo, MissedList), false);       
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "MissedQuestionnaires_Tapped");
        }
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        try
        {
            //grant access button clicked
            bool confirm = await DisplayAlert(LocalizationManager.Get("Dashboard_ConfirmAccessTitle"), LocalizationManager.Get("Dashboard_ConfirmAccessMsg"), LocalizationManager.Get("Dashboard_ConfirmAccessYes"), LocalizationManager.Get("Dashboard_ConfirmAccessNo"));
            if (!confirm)
            {
                return;
            }

            var itemUpdate = Allhouseholdgroupinfodetails.FirstOrDefault(f =>
                f.household_individual_userid == Helpers.Settings.UsersID);
            //Rest from Upper
            itemUpdate.household_rep_access = "active";
            bool isSuccessful = await APICalls.Instance.UpdateHouseholdFeedback(Allhouseholdgroupinfodetails);
            if (!isSuccessful)
            {
                houserepaccessborder.IsVisible = true;
            }
            else
            {
                houserepaccessborder.IsVisible = false;
                await DisplayAlert(LocalizationManager.Get("Dashboard_AccessGrantedTitle"), LocalizationManager.Get("Dashboard_AccessGrantedMsg"), LocalizationManager.Get("Common_OK"));
            }
        }
        catch (Exception ex)
        {
        }
    }

    private void TapGestureRecognizer_Tapped_9(object sender, TappedEventArgs e)
    {
        try
        {
            if (PhoneDialer.Default.IsSupported)
            {
                PhoneDialer.Default.Open("+447889952493");
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_9");
        }
    }

    private async void Button_Clicked1(object sender, EventArgs e)
    {
        try
        {
            await MopupService.Instance.PushAsync(new SelectLanguagePopup());
        }
        catch (Exception Ex)
        {
        }
    }
}