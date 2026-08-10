using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Mopups.Services;

namespace PeopleWithResearch
{
    public partial class NotificationQuestionGP : Mopups.Pages.PopupPage
    {
        public QuestionManager questionamanager;
        public AnswerManager answermanager;
        public UserQuestionnaireManager userquestionnairemanager;
        string[] notificationdeatils;
        public Answers SelectedAnswer = new Answers();
        public IncidentsManager incidentsManager;
        public ObservableCollection<Question> questions = new ObservableCollection<Question>();

        public ObservableCollection<Answers> answers = new ObservableCollection<Answers>();
        public UserQuestionnaire newuserquestionnaireid = new UserQuestionnaire();
        public UserQuestionAnswerManager userquestionanswermanager;
        public int weeknumber;
        public ObservableCollection<UserQuestionnaire> userquestionnaires = new ObservableCollection<UserQuestionnaire>();
        public bool dash;

        public string questionnaireid;
        public ObservableCollection<Question> datelistforquestionnaire = new ObservableCollection<Question>();
        public IncidentsManager incidentsmanager;

        //public INotificationHubService notificationHubService;
        //public string HubName;



        public NotificationQuestionGP()
        {
            InitializeComponent();
            questionamanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            incidentsManager = IncidentsManager.DefaultManager;
            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            incidentsmanager = IncidentsManager.DefaultManager;
            userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;

            // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

           
            //notificationHubService = DependencyService.Get<INotificationHubService>();
            //HubName = Constants.NotificationHubName;

            //// TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            //if (Device.RuntimePlatform == Device.Android)
            //{
            //    notificationHubService.ClearTags();
            //    notificationHubService.AddTag(Helpers.Settings.UserKey);
            //    notificationHubService.AddTag(Helpers.Settings.SignUp);
            //}
            //else
            //{

            //    notificationHubService.AddTag(Helpers.Settings.UserKey);
            //    notificationHubService.AddTag(Helpers.Settings.SignUp);
            //}

           // On<iOS>().SetUseSafeArea(false);

            //check if yhey have an incident recorded already

            getdetailsforuser();


            getquestionandanswer();
        }


        public NotificationQuestionGP(string adverttitle)
        {
            InitializeComponent();
            questionamanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            incidentsManager = IncidentsManager.DefaultManager;
            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            incidentsmanager = IncidentsManager.DefaultManager;
            userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;

            studyclosed.IsVisible = true;

            studycloseddetailslbl.Text = "Thank you for your interest in the study. Unfortunately registration is now closed for the " + adverttitle + ". If you believe this to be an error, please contact: support@peoplewith.com";



        }

        async void getdetailsforuser()
        {
            try
            {



                //check if they have incidents

                var checkincidents = await incidentsmanager.Getincidentsforuser();


                //check if they have any questionnaires answers completed

                //if they have no answers it means they have not completed the questionnaire and left the app

                var checkquestionnaires = await userquestionanswermanager.getUserAnswersByUserID();



                if (checkincidents.Count > 0 && checkquestionnaires.Count > 0)
                {
                    //they have completed the task

                    questionstack.IsVisible = false;
                    completedtack.IsVisible = true;

                }
                else if (checkincidents.Count > 0 && checkquestionnaires.Count == 0)
                {
                    //means the started the questionnaire and didnt finish it

                    //so delete the current incident and show the question

                    var deleteincident = incidentsmanager.DeleteIncident(checkincidents[0]);


                    questionstack.IsVisible = true;

                    completedtack.IsVisible = false;
                }
                else
                {
                    questionstack.IsVisible = true;

                    completedtack.IsVisible = false;
                }


            }
            catch (Exception ex)
            {

            }
        }




        public static string GetMonthName(int monthNumber)
        {
            switch (monthNumber)
            {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
                default:
                    return "Invalid Month Number";
            }
        }

        async void getquestionandanswer()
        {
            try
            {
                //get the question and answers

                var question = await questionamanager.getQuestionbysignupcode("IID3");


                if (question.Count > 0)
                {
                    //sort out the labels
                    var info = question[0].QuestionName.Replace("[NAME]", Helpers.Settings.FirstName + ",");

                    questioninfolbl.Text = info;
                    questiondirectionslbl.Text = question[0].Directions;

                    //get answers
                    var answers = await answermanager.getAnswerss(question[0].Id);

                    if (answers.Count > 0)
                    {
                        //order the answers
                        var orderedanswerlist = answers.OrderBy(x => x.Order).ToList();


                        if (orderedanswerlist.Any(x => x.Label == "Neither"))
                        {
                            var finditem = orderedanswerlist.Where(x => x.Label == "Neither").FirstOrDefault();

                            orderedanswerlist.Remove(finditem);
                        }

                        //add the answers in the listview

                        answerlist.ItemsSource = orderedanswerlist;

                    }
                }
            }
            catch (Exception ex)
            {
                var s = ex.StackTrace.ToString();
                var ss = "sds";
            }
        }

        void answerlist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as Answers;


                SelectedAnswer = item;


            }
            catch (Exception ex)
            {

            }
        }



        async void btnnotclicked_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {

                //submit button clicked

                if (string.IsNullOrEmpty(SelectedAnswer.Id))
                {
                    await DisplayAlert("Error - Select Answer", "Please choose an answer", "OK");
                    return;
                }

                //add a new user questionnaire id and a new incidents

                //var newuserquestionnaireid = new UserQuestionnaire();
                newuserquestionnaireid.Userid = Helpers.Settings.UserKey;
                newuserquestionnaireid.Questionnaireid = "425A4FE9-079D-48B2-8922-1DB6AC00FBE4";

                //split the string to find the weekly number
                //var d = notificationdeatils[4];
                //string[] num = d.Split(':');

                //var endstring = num[1];

                //get the number from the string
                //string output = string.Concat(endstring.Where(Char.IsDigit));

                //add the week number in as the score
                newuserquestionnaireid.Score = "0";


                await userquestionnairemanager.AddUserQuestionnaire(newuserquestionnaireid);

                var newincident = new incidents();

                newincident.Userid = Helpers.Settings.UserKey;
                newincident.Userquestionnaireid = newuserquestionnaireid.Id;


                newincident.Week = "0";

                newincident.Weeklyfollowupanswerid = SelectedAnswer.Id;

                newincident.Notes = "Active";

                newincident.Reportacknowledged = true;
                newincident.Invitedtocollectkit = true;
                newincident.Kitcollectedgp = true;
                newincident.Kitreturnedgp = true;
                newincident.Kitcollectedpatient = true;
                newincident.Kitreturnedgp = true;
                newincident.Kitreturnedpatient = true;

                await incidentsManager.Addincidents(newincident);



                loadingstack.IsVisible = true;
                questionstack.IsVisible = false;
                closeimg.IsVisible = false;


                questions = await questionamanager.getQuestions("425A4FE9-079D-48B2-8922-1DB6AC00FBE4");

                var getallanswers = await answermanager.getAllAnswerss();

                foreach (var item in questions)
                {
                    var getanswers = getallanswers.Where(x => x.Questionid == item.Id);

                    foreach (var i in getanswers)
                    {
                        answers.Add(i);
                    }
                }

                var iid32 = true;

               // await Navigation.PushAsync(new QuestionnairePage(questions, answers, newuserquestionnaireid, iid32), false);


                Analytics.TrackEvent("Questionnaire(IID32) - First answer Submitted");

            }
            catch (Exception ex)
            {
                Analytics.TrackEvent("CRASH - NotificationQuestion - submit button - " + ex.StackTrace.ToString());
            }
        }

        async void btnthanksclicked_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                //get the questions and answers for the question if they have selected the right value



                if (SelectedAnswer.Label == "Neither")
                {
                    //close page
                    await MopupService.Instance.PopAsync(true);
                }
                else
                {
                    //get the questionnaire details
                    thanksstack.IsVisible = false;
                    loadingstack.IsVisible = true;


                    questions = await questionamanager.getQuestions(notificationdeatils[3]);

                    var getallanswers = await answermanager.getAllAnswerss();

                    foreach (var item in questions)
                    {
                        var getanswers = getallanswers.Where(x => x.Questionid == item.Id);

                        foreach (var i in getanswers)
                        {
                            answers.Add(i);
                        }
                    }


                    //await PopupNavigation.Instance.PopAsync(true);


                    if (dash == true)
                    {
                      //  await Navigation.PushAsync(new QuestionnairePage(questions, answers, newuserquestionnaireid, userquestionnaires, weeknumber), false);

                        var pages = Navigation.NavigationStack.ToList();
                        int ii = 0;
                        foreach (var page in pages)
                        {

                            if (ii == 0)
                            {

                            }
                            else if (ii == 1)
                            {
                                //Navigation.RemovePage(this);
                            }
                            else if (ii == 2)
                            {
                                Navigation.RemovePage(this);
                            }
                            else if (ii == 3)
                            {
                                // Navigation.RemovePage(this);
                            }
                            else
                            {

                            }

                            ii++;
                        }


                    }
                    else
                    {

                      //  await Navigation.PushAsync(new QuestionnairePage(questions, answers, newuserquestionnaireid), false);

                        var pages = Navigation.NavigationStack.ToList();
                        int ii = 0;
                        foreach (var page in pages)
                        {

                            if (ii == 0)
                            {

                            }
                            else if (ii == 1)
                            {
                                //Navigation.RemovePage(this);
                            }
                            else if (ii == 2)
                            {
                                Navigation.RemovePage(this);
                            }
                            else if (ii == 3)
                            {
                                Navigation.RemovePage(this);
                            }
                            else
                            {

                            }

                            ii++;
                        }

                    }

                    Analytics.TrackEvent("Questionnaire - Neither or continue to questionnaire clicked");

                    // Navigation.RemovePage(this);
                    //await Navigation.PopModalAsync();
                }

            }
            catch (Exception ex)
            {

                var s = ex.StackTrace.ToString();
                var ss = "dbsdchs";

            }
        }

        async void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            try
            {

                await MopupService.Instance.PopAsync(true);


            }
            catch (Exception ex)
            {

            }
        }

        async void btnthanksNoIncident_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {

              //  await Navigation.PushAsync(new MainDashboard(), false);
                await MopupService.Instance.PopAsync(true);
            }
            catch (Exception ex)
            {

            }

        }

        async void btnwrong_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                await MopupService.Instance.PopAsync(true);
            }
            catch (Exception ex)
            {

            }
        }
    }
}

