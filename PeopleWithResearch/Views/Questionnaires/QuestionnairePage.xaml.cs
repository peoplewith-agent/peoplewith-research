using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using PeopleWithResearch;
//using Plugin.Connectivity;
//using Rg.Plugins.Popup.Extensions;
//using Rg.Plugins.Popup.Services;
//using Syncfusion.ListView.XForms;
using Microsoft.Maui.Controls.PlatformConfiguration;
//using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
using Syncfusion.Maui.Sliders;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Picker;

namespace PeopleWithResearch
{
    public partial class QuestionnairePage : ContentPage
    {
        public bool edit;

        public Questionnaire passedquestionnaire;
        public QuestionManager questionmanager;
        public AnswerManager answermanager;
        public UserQuestionAnswerManager userquestionanswermanager;
        public UserQuestionnaireManager userquestionnairemanager;
        public QuestionnaireManager questionnairemanager;

        public ObservableCollection<Question> questions = new ObservableCollection<Question>();

        public ObservableCollection<Question> completedquestions = new ObservableCollection<Question>();

        public ObservableCollection<Answers> answers = new ObservableCollection<Answers>();

        public ObservableCollection<UserQuestionAnswer> currentuseranswers = new ObservableCollection<UserQuestionAnswer>();

        public ObservableCollection<Questionnaire> questionnaireconstent = new ObservableCollection<Questionnaire>();

        public string usercompleting;


        public double percentincrease;

        public ObservableCollection<Question> completedquestionsforprogresspercent = new ObservableCollection<Question>();

        public UserQuestionnaire insertnewuserquestionnaire = new UserQuestionnaire();

        public ObservableCollection<Question> datelistforquestionnaire = new ObservableCollection<Question>();

        public ObservableCollection<UserQuestionnaire> userquestionnaires = new ObservableCollection<UserQuestionnaire>();

        public List<Question> Allquestionlist = new List<Question>();

        public int weeknumber;

        public incidents userincident;

        public bool iid32close;

        public int completequestioncounter;

        public int overallquestioncount;

        public List<string> questioncountlist = new List<string>();


        public QuestionnairePage()
        {
            InitializeComponent();

            //CrossConnectivity.Current.ConnectivityTypeChanged += (sender, args) =>
            //{
            //    if (!CrossConnectivity.Current.IsConnected)
            //    {
            //        mainstack.IsVisible = false;
            //        noconnection.IsVisible = true;
            //    }
            //    else
            //    {
            //        mainstack.IsVisible = true;
            //        noconnection.IsVisible = false;
            //    }
            //};

            Connectivity.ConnectivityChanged += ConnectivityChangedHandler;


            // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);
        }

        public QuestionnairePage(ObservableCollection<Question> allquestions, ObservableCollection<Answers> allanswers, UserQuestionnaire userquestionnairepassed, ObservableCollection<UserQuestionnaire> alluserquestionnairepassed, int wnumber)
        {
            InitializeComponent();

            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //CrossConnectivity.Current.ConnectivityTypeChanged += (sender, args) =>
            //{
            //    if (!CrossConnectivity.Current.IsConnected)
            //    {
            //        mainstack.IsVisible = false;
            //        noconnection.IsVisible = true;
            //    }
            //    else
            //    {
            //        mainstack.IsVisible = true;
            //        noconnection.IsVisible = false;
            //    }
            //};

            Connectivity.ConnectivityChanged += ConnectivityChangedHandler;


            questionmanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;
            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            questionnairemanager = QuestionnaireManager.DefaultManager;

         //   Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            insertnewuserquestionnaire = userquestionnairepassed;

            userquestionnaires = alluserquestionnairepassed;
            weeknumber = wnumber;

          //  On<iOS>().SetUseSafeArea(true);


            // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);




            //titlelbl.Text = passedquestionnaire.Title;
            //infolbl.Text = passedquestionnaire.Description;

            questions = allquestions;
            answers = allanswers;


            titlelbl.Text = "Symptom Questionnaire (Weekly Follow-up Study)";
            infolbl.Text = "The third study of diarrhoea and vomiting in the community. We want to know how often people in the UK suffer from diarrhoea or vomiting and the germs that cause this. Thank you for agreeing to fill in this questionnaire. Please read each question carefully before you answer it, and try to answer every question. Please either tick the appropriate box or write your answer in the space provided. The information that you give us will be treated in strict confidence.";

            //  getquestionnairedeatails();
            getquestionsandanswers();


            // questionnairelist.HeightRequest = questions.Count * 200;
        }



        public QuestionnairePage(ObservableCollection<Question> allquestions, ObservableCollection<Answers> allanswers, UserQuestionnaire userquestionnairepassed)
        {
            InitializeComponent();

            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //CrossConnectivity.Current.ConnectivityTypeChanged += (sender, args) =>
            //{
            //    if (!CrossConnectivity.Current.IsConnected)
            //    {
            //        mainstack.IsVisible = false;
            //        noconnection.IsVisible = true;
            //    }
            //    else
            //    {
            //        mainstack.IsVisible = true;
            //        noconnection.IsVisible = false;
            //    }
            //};

            Connectivity.ConnectivityChanged += ConnectivityChangedHandler;

            questionmanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;
            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            questionnairemanager = QuestionnaireManager.DefaultManager;

          //  Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            insertnewuserquestionnaire = userquestionnairepassed;

          //  On<iOS>().SetUseSafeArea(true);


            // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);




            //titlelbl.Text = passedquestionnaire.Title;
            //infolbl.Text = passedquestionnaire.Description;

            questions = allquestions;
            answers = allanswers;

            getquestionnairedeatails();
            getquestionsandanswers();


            // questionnairelist.HeightRequest = questions.Count * 200;
        }


        public QuestionnairePage(ObservableCollection<Question> allquestions, ObservableCollection<Answers> allanswers, UserQuestionnaire userquestionnairepassed, bool passediid32close)
        {
            InitializeComponent();

            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //CrossConnectivity.Current.ConnectivityTypeChanged += (sender, args) =>
            //{
            //    if (!CrossConnectivity.Current.IsConnected)
            //    {
            //        mainstack.IsVisible = false;
            //        noconnection.IsVisible = true;
            //    }
            //    else
            //    {
            //        mainstack.IsVisible = true;
            //        noconnection.IsVisible = false;
            //    }
            //};

            Connectivity.ConnectivityChanged += ConnectivityChangedHandler;

            iid32close = true;

            questionmanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;
            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            questionnairemanager = QuestionnaireManager.DefaultManager;

         //   Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            insertnewuserquestionnaire = userquestionnairepassed;

          //  On<iOS>().SetUseSafeArea(true);


            // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);




            //titlelbl.Text = passedquestionnaire.Title;
            //infolbl.Text = passedquestionnaire.Description;

            questions = allquestions;
            answers = allanswers;

            titlelbl.Text = "Symptom Questionnaire";
            infolbl.Text = "The third study of diarrhoea and vomiting in the community. We want to know how often people in the UK suffer from diarrhoea or vomiting and the germs that cause this. Thank you for agreeing to fill in this questionnaire. Please read each question carefully before you answer it, and try to answer every question. Please either tick the appropriate box or write your answer in the space provided. The information that you give us will be treated in strict confidence.";

            //getquestionnairedeatails();


            getquestionsandanswers();


            // questionnairelist.HeightRequest = questions.Count * 200;
        }



        public QuestionnairePage(ObservableCollection<Question> allquestions, ObservableCollection<Answers> allanswers, ObservableCollection<UserQuestionAnswer> useranswerpassed, string viewonly, List<Question> Orderedquestionsforlist, string empty, incidents passedincident)
        {
            InitializeComponent();

            //viewonly



            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //CrossConnectivity.Current.ConnectivityTypeChanged += (sender, args) =>
            //{
            //    if (!CrossConnectivity.Current.IsConnected)
            //    {
            //        mainstack.IsVisible = false;
            //        noconnection.IsVisible = true;
            //    }
            //    else
            //    {
            //        mainstack.IsVisible = true;
            //        noconnection.IsVisible = false;
            //    }
            //};
            Connectivity.ConnectivityChanged += ConnectivityChangedHandler;


            questionmanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;
            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            questionnairemanager = QuestionnaireManager.DefaultManager;

            // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            // insertnewuserquestionnaire = userquestionnairepassed;

            //On<iOS>().SetUseSafeArea(true);


            // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);




            //titlelbl.Text = passedquestionnaire.Title;
            //infolbl.Text = passedquestionnaire.Description;

            questions = allquestions;
            answers = allanswers;

            userincident = passedincident;

            //find the incident text

            Allquestionlist = Orderedquestionsforlist;

            getquestionnairedeatails();



            //getquestionsandanswersAddCurrentAnswers();



            savebutton.IsVisible = false;

            progresslbl.IsVisible = false;
            questionnaireprogressbar.IsVisible = false;

            getincident();

            //questionnairelist.IsEnabled = false;

            incidentframe.IsVisible = true;
            incidentlbl.IsVisible = true;


            // questionnairelist.HeightRequest = questions.Count * 200;
        }


        private void ConnectivityChangedHandler(object sender, ConnectivityChangedEventArgs e)
        {
            UpdateConnectivityStatus();
        }

        private void UpdateConnectivityStatus()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                mainstack.IsVisible = false;
                noconnection.IsVisible = true;
            }
            else
            {
                mainstack.IsVisible = true;
                noconnection.IsVisible = false;
            }
        }

        // Clean up event subscription to avoid memory leaks
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Connectivity.ConnectivityChanged -= ConnectivityChangedHandler;
        }

        async void getincident()
        {
            try
            {

                var findincident = await answermanager.getAnswerforincident(userincident.Weeklyfollowupanswerid);

                incidenttextlbl.Text = findincident[0].Label;


                if (incidenttextlbl.Text == "Neither")
                {
                    neitherstack.IsVisible = true;
                    liststack.IsVisible = false;

                    var date = userincident.CreatedAt.ToString("dd MMM");

                    neitherdate.Text = date;
                    //show a fallback here as they havent done the questionnaire it doesnt need to show
                    questionnairelist.IsVisible = false;

                }
                else
                {
                    questionnairelist.ItemsSource = Allquestionlist;
                    //getquestionsandanswers();
                    liststack.IsVisible = true;
                    liststack.IsEnabled = false;
                }

            }
            catch (Exception ex)
            {

            }
        }


        async void getquestionnairedeatails()
        {
            try
            {
                //var questionnaire = await questionnairemanager.getQuestionnaireSingle(questions[0].Questionnaireid);


                //titlelbl.Text = questionnaire[0].Title;
                //infolbl.Text = questionnaire[0].Description;


                titlelbl.Text = "Symptom Questionnaire (Weekly Follow-up Study)";
                infolbl.Text = "The third study of diarrhoea and vomiting in the community. We want to know how often people in the UK suffer from diarrhoea or vomiting and the germs that cause this. Thank you for agreeing to fill in this questionnaire. Please read each question carefully before you answer it, and try to answer every question. Please either tick the appropriate box or write your answer in the space provided. The information that you give us will be treated in strict confidence.";

            }
            catch (Exception ex)
            {

            }
        }

        //public QuestionnairePage(Questionnaire questionnaire, ObservableCollection<Question> allquestions, ObservableCollection<Answers> allanswers)
        //{
        //    InitializeComponent();

        //    CrossConnectivity.Current.ConnectivityTypeChanged += (sender, args) =>
        //    {
        //        if (!CrossConnectivity.Current.IsConnected)
        //        {
        //            mainstack.IsVisible = false;
        //            noconnection.IsVisible = true;
        //        }
        //        else
        //        {
        //            mainstack.IsVisible = true;
        //            noconnection.IsVisible = false;
        //        }
        //    };


        //    questionmanager = QuestionManager.DefaultManager;
        //    answermanager = AnswerManager.DefaultManager;
        //    userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;
        //    userquestionnairemanager = UserQuestionnaireManager.DefaultManager;


        //    // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

        //    passedquestionnaire = questionnaire;

        //    titlelbl.Text = passedquestionnaire.Title;
        //    infolbl.Text = passedquestionnaire.Description;

        //    questions = allquestions;
        //    answers = allanswers;

        //    getquestionsandanswers();


        //    if (passedquestionnaire.Consent1 != null)
        //    {
        //        //add new items for questionnaire constent
        //        var newone = new Questionnaire();
        //        newone.Description = passedquestionnaire.Consent1;
        //        questionnaireconstent.Add(newone);
        //    }

        //    if (passedquestionnaire.Consent2 != null)
        //    {
        //        var newonee = new Questionnaire();
        //        newonee.Description = passedquestionnaire.Consent2;
        //        questionnaireconstent.Add(newonee);
        //    }

        //    if (passedquestionnaire.Consent3 != null)
        //    {
        //        var newoneee = new Questionnaire();
        //        newoneee.Description = passedquestionnaire.Consent3;
        //        questionnaireconstent.Add(newoneee);
        //    }

        //    if (passedquestionnaire.Consent4 != null)
        //    {
        //        var newoneeee = new Questionnaire();
        //        newoneeee.Description = passedquestionnaire.Consent4;
        //        questionnaireconstent.Add(newoneeee);
        //    }

        //    agreelist.ItemsSource = questionnaireconstent;
        //    agreelist.HeightRequest = questionnaireconstent.Count * 75;

        //    constentlbl.Text = passedquestionnaire.Info;
        //    //constentlbl.Text = "You have been invited to participate in this research study titled " + passedquestionnaire.Title + ". This research is being done by PeopleWith Ltd. Your partication in this survey is voluntary. By participating in this survey, you are agreeing to provide the most honest answers you can. Any responses you provide will be anonymised.";

        //    agreestack.IsVisible = true;
        //    Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);



        //    // questionnairelist.HeightRequest = questions.Count * 200;
        //}


        public QuestionnairePage(Questionnaire questionnaire, ObservableCollection<Question> allquestions, ObservableCollection<Answers> allanswers, ObservableCollection<UserQuestionAnswer> currentanswers, bool viewonly)
        {
            InitializeComponent();

            //CrossConnectivity.Current.ConnectivityTypeChanged += (sender, args) =>
            //{
            //    if (!CrossConnectivity.Current.IsConnected)
            //    {
            //        mainstack.IsVisible = false;
            //        noconnection.IsVisible = true;
            //    }
            //    else
            //    {
            //        mainstack.IsVisible = true;
            //        noconnection.IsVisible = false;
            //    }
            //};

            Connectivity.ConnectivityChanged += ConnectivityChangedHandler;

            questionmanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;
            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;

            // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            edit = true;

            passedquestionnaire = questionnaire;

            titlelbl.Text = passedquestionnaire.Title;
            infolbl.Text = passedquestionnaire.Description;

            titlelbl.IsVisible = true;
            infolbl.IsVisible = true;

            questions = allquestions;
            answers = allanswers;

            currentuseranswers = currentanswers;

            getquestionsandanswersAddCurrentAnswers();

            //agreelist.IsVisible = false;
            liststack.IsVisible = true;


            //questionnairelist.HeightRequest = questions.Count * 200;

            questionnairelist.IsEnabled = viewonly;

            if (viewonly == false)
            {
                toolbar.Text = "";
            }



        }

        [Obsolete]
        async void getquestionsandanswersAddCurrentAnswers()
        {
            try
            {

                //set the dates
                // Get the current date
                DateTime currentDate = DateTime.Now;

                // Generate a list of the last 30 days
                List<DateTime> last30Days = Enumerable.Range(0, 30)
                    .Select(i => currentDate.AddDays(-i))
                    .ToList();

                //last30Days.Reverse();

                // Print the list of the last 30 days
                foreach (DateTime date in last30Days)
                {
                    var newdate = new Question();

                    newdate.Dateday = date.Day.ToString();
                    newdate.Datemonth = date.ToString("MMM");
                    newdate.Dateyear = date.ToString("yyyy");

                    datelistforquestionnaire.Add(newdate);
                }

                //get the questions for questionnaire


                //questions = await questionmanager.getQuestions(passedquestionnaire.Id);
                //answers = await answermanager.getAllAnswerss();

                //create a new collection for all questions and answers, then group by the question id and name

                //need to a counter to see the first and last question so i can hide headers and change margins
                var counterforfirstlist = 0;

                var allquestionsandanswers = new ObservableCollection<QuestionANDAnswers>();


                var orderedquestions = questions.OrderBy(x => x.Questionorder).ToList();

                //add in two headers for questions

                var firstheader = new Question();
                firstheader.Headertext = "This section asks you about your symptoms you had during your recent illness";
                firstheader.QuestionName = "";
                firstheader.Headerinfo = true;
                firstheader.Type = "HeaderInfo";
                firstheader.Id = "dsds";

                orderedquestions.Insert(0, firstheader);

                var secondheader = new Question();
                secondheader.Headertext = "This section asks about your travel in the ten days before you became ill.";
                secondheader.QuestionName = " ";
                secondheader.Headerinfo = true;
                secondheader.Type = "HeaderInfo";
                secondheader.Id = "dsds";

                orderedquestions.Insert(21, secondheader);

                foreach (var item in orderedquestions)
                {
                  
                    item.Datelist = datelistforquestionnaire;
                    item.Dateop = 1;
                    item.Backgroundcolourchange = Colors.White;
                    item.Groupheadervis = true;

                    item.Buttonborder = Colors.Transparent;
                    item.Buttonborder1 = Colors.Transparent;
                    item.Buttonborder2 = Colors.Transparent;
                    item.Buttonborder3 = Colors.Transparent;
                    item.Buttonborder4 = Colors.Transparent;
                    item.Buttonbackground = Colors.Transparent;
                    item.Answerid = string.Empty;

                    item.BottomMarginNum = new Thickness(5, 0, 5, 20);

                    //item.GroupKey = item.Id + "1";

                    var sortedanswers = answers.Where(x => x.Questionid == item.Id);

                    var orderedansweredlist = sortedanswers.OrderBy(x => x.Order).ToList();

                    if (item.Type == "type03")
                    {
                        item.Type03 = true;

                        item.Label0 = orderedansweredlist[0].Label;
                        item.Value0 = orderedansweredlist[0].Answervalue;
                        item.Answerid = orderedansweredlist[0].Id;

                        item.Label1 = orderedansweredlist[1].Label;
                        item.Value1 = orderedansweredlist[1].Answervalue;
                        item.Answerid = orderedansweredlist[1].Id;

                        item.Label2 = orderedansweredlist[2].Label;
                        item.Value2 = orderedansweredlist[2].Answervalue;
                        item.Answerid = orderedansweredlist[2].Id;

                        item.Label3 = orderedansweredlist[3].Label;
                        item.Value3 = orderedansweredlist[3].Answervalue;
                        item.Answerid = orderedansweredlist[3].Id;
                    }
                    else if (item.Type == "type15")
                    {
                        item.Type15 = true;

                        item.Label0 = orderedansweredlist[0].Label;
                        item.Value0 = orderedansweredlist[0].Answervalue;
                        item.Answerid = orderedansweredlist[0].Id;

                        item.Label1 = orderedansweredlist[1].Label;
                        item.Value1 = orderedansweredlist[1].Answervalue;


                        item.Label2 = orderedansweredlist[2].Label;
                        item.Value2 = orderedansweredlist[2].Answervalue;


                        item.Label3 = orderedansweredlist[3].Label;
                        item.Value3 = orderedansweredlist[3].Answervalue;


                        item.Label4 = orderedansweredlist[4].Label;
                        item.Value4 = orderedansweredlist[4].Answervalue;


                    }
                    else if (item.Type == "type110")
                    {
                        item.Type110 = true;
                        item.Answerid = orderedansweredlist[0].Id;

                        item.Label0 = orderedansweredlist[0].Label;
                        item.Value0 = orderedansweredlist[0].Answervalue;
                        item.Value1 = orderedansweredlist[1].Answervalue;
                        item.Value2 = orderedansweredlist[2].Answervalue;
                        item.Value3 = orderedansweredlist[3].Answervalue;
                        item.Value4 = orderedansweredlist[4].Answervalue;
                        item.Value5 = orderedansweredlist[5].Answervalue;
                        item.Value6 = orderedansweredlist[6].Answervalue;
                        item.Value7 = orderedansweredlist[7].Answervalue;
                        item.Value8 = orderedansweredlist[8].Answervalue;
                        item.Value9 = orderedansweredlist[9].Answervalue;
                        item.Label9 = orderedansweredlist[10].Label;

                    }
                    else if (item.Type == "typeSS")
                    {
                        item.Typess = true;
                        item.Answerid = orderedansweredlist[0].Id;
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;

                        if (orderedansweredlist.Count >= 3)
                        {
                            item.Value2 = orderedansweredlist[2].Label;
                            item.Checkboxthree = true;
                        }

                        if (orderedansweredlist.Count >= 4)
                        {
                            item.Value3 = orderedansweredlist[3].Label;
                            item.Checkboxfour = true;
                        }

                        if (orderedansweredlist.Count >= 5)
                        {
                            item.Value4 = orderedansweredlist[4].Label;
                            item.Checkboxfive = true;
                        }

                        if (orderedansweredlist.Count >= 6)
                        {
                            item.Value5 = orderedansweredlist[5].Label;
                            item.Checkboxsix = true;
                        }

                        if (orderedansweredlist.Count >= 7)
                        {
                            item.Value6 = orderedansweredlist[6].Label;
                            item.Checkboxseven = true;
                        }

                        if (orderedansweredlist.Count >= 8)
                        {
                            item.Value7 = orderedansweredlist[7].Label;
                            item.Checkboxeight = true;
                        }

                        if (orderedansweredlist.Count >= 9)
                        {
                            item.Value8 = orderedansweredlist[8].Label;
                            item.Checkboxnine = true;
                        }

                        if (orderedansweredlist.Count >= 10)
                        {
                            item.Value9 = orderedansweredlist[9].Label;
                            item.Checkboxten = true;
                        }


                    }
                    else if (item.Type == "typeMS")
                    {
                        item.Typems = true;
                        item.Answerid = orderedansweredlist[0].Id;
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;

                        if (orderedansweredlist.Count >= 3)
                        {
                            item.Value2 = orderedansweredlist[2].Label;
                            item.Checkboxthree = true;
                        }

                        if (orderedansweredlist.Count >= 4)
                        {
                            item.Value3 = orderedansweredlist[3].Label;
                            item.Checkboxfour = true;
                        }

                        if (orderedansweredlist.Count >= 5)
                        {
                            item.Value4 = orderedansweredlist[4].Label;
                            item.Checkboxfive = true;
                        }

                        if (orderedansweredlist.Count >= 6)
                        {
                            item.Value5 = orderedansweredlist[5].Label;
                            item.Checkboxsix = true;
                        }

                        if (orderedansweredlist.Count >= 7)
                        {
                            item.Value6 = orderedansweredlist[6].Label;
                            item.Checkboxseven = true;
                        }

                        if (orderedansweredlist.Count >= 8)
                        {
                            item.Value7 = orderedansweredlist[7].Label;
                            item.Checkboxeight = true;
                        }

                        if (orderedansweredlist.Count >= 9)
                        {
                            item.Value8 = orderedansweredlist[8].Label;
                            item.Checkboxnine = true;
                        }
                        if (orderedansweredlist.Count >= 10)
                        {
                            item.Value9 = orderedansweredlist[9].Label;
                            item.Checkboxten = true;
                        }



                    }
                    else if (item.Type == "typeNumeric" || item.Type == "numeric")
                    {
                        item.Answerid = orderedansweredlist[0].Id;
                        item.TypeEntry = true;

                        item.Value0 = orderedansweredlist[0].Label;

                    }

                    else if (item.Type == "typeSSAdd")
                    {
                        item.DateAnswer = DateTime.Now;

                        if (item.Notes.Contains("|"))
                        {
                            //that means there is 2 additional questions

                            var splitnotes = item.Notes.Split('|');

                            item.Addquestion = splitnotes[0];
                            item.Addquestion2 = splitnotes[1];

                            item.TypeSSAdd = true;
                            item.TypeSSAdd = true;

                            item.Answerid = orderedansweredlist[0].Id;
                            item.Value0 = orderedansweredlist[0].Label;
                            item.Value1 = orderedansweredlist[1].Label;
                            item.Value2 = orderedansweredlist[2].Label;



                        }
                        else
                        {


                            item.Addquestion = item.Notes;
                            item.TypeSSAdd = true;
                            item.Answerid = orderedansweredlist[0].Id;
                            item.Value0 = orderedansweredlist[0].Label;
                            item.Value1 = orderedansweredlist[1].Label;
                            item.Value2 = orderedansweredlist[2].Label;
                        }

                    }

                    else if (item.Type == "typeSS1" || item.Type == "typeSS2")
                    {
                        //this is the first section where the header is not needed for each question except for the first 
                        //type 1 has the 2 additional questions type 2 has none
                        item.BottomMarginNum = new Thickness(5, 0, 5, 5);


                        if (counterforfirstlist == 0)
                        {
                            //do nothing as we need the full title just split the question a little


                            item.QuestionName = item.QuestionName.Replace('|', '\n');

                        }
                        else
                        {
                            var splitqn = item.QuestionName.Split('|');

                            if (splitqn.Length == 1)
                            {
                                item.QuestionName = splitqn[0];
                            }
                            else
                            {
                                item.QuestionName = splitqn[1];
                            }


                        }

                        item.Headervis = false;
                        item.TypeSS1 = true;
                        item.Answerid = orderedansweredlist[0].Id;
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Checkboxthree = true;

                        if (orderedansweredlist.Count == 3)
                        {
                            item.Value2 = orderedansweredlist[2].Label;
                        }
                        else
                        {
                            //hasnt got third value
                            item.Checkboxthree = false;
                        }

                        counterforfirstlist++;


                        if (counterforfirstlist == 9)
                        {
                            item.BottomMarginNum = new Thickness(5, 0, 5, 20);
                        }
                    }

                    else if (item.Type == "typeDate")
                    {
                        item.DateAnswer = DateTime.Now;
                        item.TypeDate = true;
                    }
                    else if (item.Type == "typeSS3")
                    {
                        item.DateAnswer = DateTime.Now;
                        item.TypeSS3 = true;

                        var splitnotes = item.Notes.Split('|');

                        item.Addquestion = splitnotes[0];
                        item.Addquestion2 = splitnotes[1];

                        item.Answerid = orderedansweredlist[0].Id;
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Value2 = orderedansweredlist[2].Label;
                    }
                    else if (item.Type == "typeSSNumeric")
                    {
                        item.TypeSSNumeric = true;
                        item.Addquestion = item.Notes;


                        item.Answerid = orderedansweredlist[0].Id;
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Value2 = orderedansweredlist[2].Label;
                    }

                    else if (item.Type == "typeDoubleDate")
                    {
                        item.DateAnswer = DateTime.Now;
                        item.TypeDoubleDate = true;
                    }

                    else if (item.Type == "text")
                    {

                        item.TypeEntryOnly = true;
                    }
                    else if (item.Type == "HeaderInfo")
                    {
                        item.Headerinfo = true;
                        item.Backgroundcolourchange = Color.FromArgb("#EFF0F7");
                        item.Backgroundcolourchange = Colors.White;
                        item.Groupheadervis = false;
                    }


                    foreach (var it in answers)
                    {
                        var newitem = new QuestionANDAnswers();

                        newitem.Questionid = item.Id;
                        newitem.QuestionName = item.QuestionName;
                        newitem.Type = item.Type;
                        newitem.Answerid = it.Id;
                        newitem.Answervalue = it.Answervalue;
                        //// newitem.Order = it.Order;

                        allquestionsandanswers.Add(newitem);

                    }

                    //order the answer



                    item.Answerlist = answers;
                }


                //order questions
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    // Perform UI-related operations here

                    questionnairelist.ItemsSource = orderedquestions;
                });



            }
            catch (Exception ex)
            {
                var s = ex.StackTrace.ToString();
            }
        }


        async void getquestionsandanswers()
        {
            try
            {


                //set the dates
                // Get the current date
                DateTime currentDate = DateTime.Now;

                // Generate a list of the last 30 days
                List<DateTime> last30Days = Enumerable.Range(0, 30)
                    .Select(i => currentDate.AddDays(-i))
                    .ToList();

                //last30Days.Reverse();

                // Print the list of the last 30 days
                foreach (DateTime date in last30Days)
                {
                    var newdate = new Question();

                    newdate.Dateday = date.Day.ToString();
                    newdate.Datemonth = date.ToString("MMM");
                    newdate.Dateyear = date.ToString("yyyy");

                    datelistforquestionnaire.Add(newdate);
                }

                //get the questions for questionnaire


                //questions = await questionmanager.getQuestions(passedquestionnaire.Id);
                //answers = await answermanager.getAllAnswerss();

                //create a new collection for all questions and answers, then group by the question id and name

                //need to a counter to see the first and last question so i can hide headers and change margins
                var counterforfirstlist = 0;

                var allquestionsandanswers = new ObservableCollection<QuestionANDAnswers>();


                var orderedquestions = questions.OrderBy(x => x.Questionorder).ToList();

                //add in two headers for questions

                var firstheader = new Question();
                firstheader.Headertext = "This section asks you about your symptoms you had during your recent illness";
                firstheader.QuestionName = "";
                firstheader.Headerinfo = true;
                firstheader.Type = "HeaderInfo";
                firstheader.Id = "dsds";

                orderedquestions.Insert(0, firstheader);

                var secondheader = new Question();
                secondheader.Headertext = "This section asks about your travel in the ten days before you became ill.";
                secondheader.QuestionName = " ";
                secondheader.Headerinfo = true;
                secondheader.Type = "HeaderInfo";
                secondheader.Id = "dsds";

                orderedquestions.Insert(21, secondheader);

                foreach (var item in orderedquestions)
                {
                   
                    item.Backgroundcolourchange = Colors.White;
                    item.Groupheadervis = true;

                    item.Datelist = datelistforquestionnaire;

                    item.Buttonborder = Colors.Transparent;
                    item.Buttonborder1 = Colors.Transparent;
                    item.Buttonborder2 = Colors.Transparent;
                    item.Buttonborder3 = Colors.Transparent;
                    item.Buttonborder4 = Colors.Transparent;
                    item.Buttonbackground = Colors.Transparent;
                    item.Answerid = string.Empty;

                    item.BottomMarginNum = new Thickness(5, 0, 5, 20);

                    //item.GroupKey = item.Id + "1";

                    var sortedanswers = answers.Where(x => x.Questionid == item.Id);

                    var orderedansweredlist = sortedanswers.OrderBy(x => x.Order).ToList();

                    if (item.Type == "type03")
                    {
                        item.Type03 = true;

                        item.Label0 = orderedansweredlist[0].Label;
                        item.Value0 = orderedansweredlist[0].Answervalue;
                        item.Answerid = orderedansweredlist[0].Id;

                        item.Label1 = orderedansweredlist[1].Label;
                        item.Value1 = orderedansweredlist[1].Answervalue;
                        item.Answerid = orderedansweredlist[1].Id;

                        item.Label2 = orderedansweredlist[2].Label;
                        item.Value2 = orderedansweredlist[2].Answervalue;
                        item.Answerid = orderedansweredlist[2].Id;

                        item.Label3 = orderedansweredlist[3].Label;
                        item.Value3 = orderedansweredlist[3].Answervalue;
                        item.Answerid = orderedansweredlist[3].Id;
                    }
                    else if (item.Type == "type15")
                    {
                        item.Type15 = true;

                        item.Label0 = orderedansweredlist[0].Label;
                        item.Value0 = orderedansweredlist[0].Answervalue;
                        item.Answerid = orderedansweredlist[0].Id;

                        item.Label1 = orderedansweredlist[1].Label;
                        item.Value1 = orderedansweredlist[1].Answervalue;


                        item.Label2 = orderedansweredlist[2].Label;
                        item.Value2 = orderedansweredlist[2].Answervalue;


                        item.Label3 = orderedansweredlist[3].Label;
                        item.Value3 = orderedansweredlist[3].Answervalue;


                        item.Label4 = orderedansweredlist[4].Label;
                        item.Value4 = orderedansweredlist[4].Answervalue;


                    }
                    else if (item.Type == "type110")
                    {
                        item.Type110 = true;
                        item.Answerid = orderedansweredlist[0].Id;

                        item.Label0 = orderedansweredlist[0].Label;
                        item.Value0 = orderedansweredlist[0].Answervalue;
                        item.Value1 = orderedansweredlist[1].Answervalue;
                        item.Value2 = orderedansweredlist[2].Answervalue;
                        item.Value3 = orderedansweredlist[3].Answervalue;
                        item.Value4 = orderedansweredlist[4].Answervalue;
                        item.Value5 = orderedansweredlist[5].Answervalue;
                        item.Value6 = orderedansweredlist[6].Answervalue;
                        item.Value7 = orderedansweredlist[7].Answervalue;
                        item.Value8 = orderedansweredlist[8].Answervalue;
                        item.Value9 = orderedansweredlist[9].Answervalue;
                        item.Label9 = orderedansweredlist[10].Label;

                    }
                    else if (item.Type == "typeSS")
                    {
                        item.Typess = true;
                        item.Answerid = orderedansweredlist[0].Id;
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;

                        if (orderedansweredlist.Count >= 3)
                        {
                            item.Value2 = orderedansweredlist[2].Label;
                            item.Checkboxthree = true;
                        }

                        if (orderedansweredlist.Count >= 4)
                        {
                            item.Value3 = orderedansweredlist[3].Label;
                            item.Checkboxfour = true;
                        }

                        if (orderedansweredlist.Count >= 5)
                        {
                            item.Value4 = orderedansweredlist[4].Label;
                            item.Checkboxfive = true;
                        }

                        if (orderedansweredlist.Count >= 6)
                        {
                            item.Value5 = orderedansweredlist[5].Label;
                            item.Checkboxsix = true;
                        }

                        if (orderedansweredlist.Count >= 7)
                        {
                            item.Value6 = orderedansweredlist[6].Label;
                            item.Checkboxseven = true;
                        }

                        if (orderedansweredlist.Count >= 8)
                        {
                            item.Value7 = orderedansweredlist[7].Label;
                            item.Checkboxeight = true;
                        }

                        if (orderedansweredlist.Count >= 9)
                        {
                            item.Value8 = orderedansweredlist[8].Label;
                            item.Checkboxnine = true;
                        }

                        if (orderedansweredlist.Count >= 10)
                        {
                            item.Value9 = orderedansweredlist[9].Label;
                            item.Checkboxten = true;
                        }


                    }
                    else if (item.Type == "typeMS")
                    {
                        item.Typems = true;
                        item.Answerid = orderedansweredlist[0].Id;
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;

                        if (orderedansweredlist.Count >= 3)
                        {
                            item.Value2 = orderedansweredlist[2].Label;
                            item.Checkboxthree = true;
                        }

                        if (orderedansweredlist.Count >= 4)
                        {
                            item.Value3 = orderedansweredlist[3].Label;
                            item.Checkboxfour = true;
                        }

                        if (orderedansweredlist.Count >= 5)
                        {
                            item.Value4 = orderedansweredlist[4].Label;
                            item.Checkboxfive = true;
                        }

                        if (orderedansweredlist.Count >= 6)
                        {
                            item.Value5 = orderedansweredlist[5].Label;
                            item.Checkboxsix = true;
                        }

                        if (orderedansweredlist.Count >= 7)
                        {
                            item.Value6 = orderedansweredlist[6].Label;
                            item.Checkboxseven = true;
                        }

                        if (orderedansweredlist.Count >= 8)
                        {
                            item.Value7 = orderedansweredlist[7].Label;
                            item.Checkboxeight = true;
                        }

                        if (orderedansweredlist.Count >= 9)
                        {
                            item.Value8 = orderedansweredlist[8].Label;
                            item.Checkboxnine = true;
                        }
                        if (orderedansweredlist.Count >= 10)
                        {
                            item.Value9 = orderedansweredlist[9].Label;
                            item.Checkboxten = true;
                        }



                    }
                    else if (item.Type == "typeNumeric" || item.Type == "numeric")
                    {
                        item.Answerid = orderedansweredlist[0].Id;
                        item.TypeEntry = true;

                        item.Value0 = orderedansweredlist[0].Label;

                    }

                    else if (item.Type == "typeSSAdd")
                    {
                        item.DateAnswer = DateTime.Now;

                        if (item.Notes.Contains("|"))
                        {
                            //that means there is 2 additional questions

                            var splitnotes = item.Notes.Split('|');

                            item.Addquestion = splitnotes[0];
                            item.Addquestion2 = splitnotes[1];

                            item.TypeSSAdd = true;
                            item.TypeSSAdd = true;

                            item.Answerid = orderedansweredlist[0].Id;
                            item.Value0 = orderedansweredlist[0].Label;
                            item.Value1 = orderedansweredlist[1].Label;
                            item.Value2 = orderedansweredlist[2].Label;



                        }
                        else
                        {


                            item.Addquestion = item.Notes;
                            item.TypeSSAdd = true;
                            item.Answerid = orderedansweredlist[0].Id;
                            item.Value0 = orderedansweredlist[0].Label;
                            item.Value1 = orderedansweredlist[1].Label;
                            item.Value2 = orderedansweredlist[2].Label;
                        }

                    }

                    else if (item.Type == "typeSS1" || item.Type == "typeSS2")
                    {
                        //this is the first section where the header is not needed for each question except for the first 
                        //type 1 has the 2 additional questions type 2 has none
                        item.BottomMarginNum = new Thickness(5, 0, 5, 5);


                        if (counterforfirstlist == 0)
                        {
                            //do nothing as we need the full title just split the question a little


                            item.QuestionName = item.QuestionName.Replace('|', '\n').Replace("\n", "\n\n");

                        }
                        else
                        {
                            var splitqn = item.QuestionName.Split('|');

                            if (splitqn.Length == 1)
                            {
                                item.QuestionName = splitqn[0];
                            }
                            else
                            {
                                item.QuestionName = splitqn[1];
                            }


                        }

                        item.Headervis = false;
                        item.TypeSS1 = true;
                        item.Answerid = orderedansweredlist[0].Id;
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Checkboxthree = true;

                        if (orderedansweredlist.Count == 3)
                        {
                            item.Value2 = orderedansweredlist[2].Label;
                        }
                        else
                        {
                            //hasnt got third value
                            item.Checkboxthree = false;
                        }

                        counterforfirstlist++;


                        if (counterforfirstlist == 9)
                        {
                            item.BottomMarginNum = new Thickness(5, 0, 5, 20);
                        }
                    }

                    else if (item.Type == "typeDate")
                    {
                        item.DateAnswer = DateTime.Now;
                        item.TypeDate = true;
                    }
                    else if (item.Type == "typeSS3")
                    {
                        item.DateAnswer = DateTime.Now;
                        item.TypeSS3 = true;

                        var splitnotes = item.Notes.Split('|');

                        item.Addquestion = splitnotes[0];
                        item.Addquestion2 = splitnotes[1];

                        item.Answerid = orderedansweredlist[0].Id;
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Value2 = orderedansweredlist[2].Label;
                    }
                    else if (item.Type == "typeSSNumeric")
                    {
                        item.TypeSSNumeric = true;
                        item.Addquestion = item.Notes;


                        item.Answerid = orderedansweredlist[0].Id;
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Value2 = orderedansweredlist[2].Label;
                    }

                    else if (item.Type == "typeDoubleDate")
                    {
                        item.DateAnswer = DateTime.Now;
                        item.TypeDoubleDate = true;
                    }

                    else if (item.Type == "text")
                    {

                        item.TypeEntryOnly = true;
                    }

                    else if (item.Type == "HeaderInfo")
                    {
                        item.Headerinfo = true;
                        item.Backgroundcolourchange = Color.FromArgb("#EFF0F7");
                        item.Backgroundcolourchange = Colors.White;
                        item.Groupheadervis = false;
                    }

                    foreach (var it in answers)
                    {
                        var newitem = new QuestionANDAnswers();

                        newitem.Questionid = item.Id;
                        newitem.QuestionName = item.QuestionName;
                        newitem.Type = item.Type;
                        newitem.Answerid = it.Id;
                        newitem.Answervalue = it.Answervalue;
                        //// newitem.Order = it.Order;

                        allquestionsandanswers.Add(newitem);

                    }

                    //order the answer



                    item.Answerlist = answers;
                }


                //order questions


                questionnairelist.ItemsSource = orderedquestions;

                liststack.IsVisible = true;
                progresslbl.IsVisible = true;
                questionnaireprogressbar.IsVisible = true;

                //find out how much each answer should increase the progress bar everytime

                double percentage = (double)100 / orderedquestions.Count;

                // percentincrease = (double)percentage / 100;

                percentincrease = percentage;

                //  agreestack.IsVisible = false;
                //  liststack.IsVisible = true;


            }
            catch (Exception ex)
            {
                var e = ex.StackTrace.ToString();
            }
        }


        async void populatelistwithusercurrentanswers()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedButton_Clicked(System.Object sender, System.EventArgs e)
        {
            //0-3 button clicked

            try
            {

                var label = (ExtendedButton)sender;

                var getitem = questions.Where(x => x.Answerid == label.IDValue).FirstOrDefault();


                getitem.Buttonborder = Color.FromArgb("#0F9FE2");
                getitem.Buttonborder1 = Colors.Transparent;
                getitem.Buttonborder2 = Colors.Transparent;
                getitem.Buttonborder3 = Colors.Transparent;
                getitem.Buttonborder4 = Colors.Transparent;
                // getitem.Buttonbackground = Color.FromHex("#bbe6fa");

                getitem.Entryanswer = label.Text;

                completedquestions.Add(getitem);

            }
            catch (Exception ex)
            {

            }
        }

        async void ToolbarItem_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {

                toolbar.IsEnabled = false;

                if (toolbar.Text == "")
                {
                    return;
                }
                //save button clicked


                //check if the questionnaire is complete or not

                //if if isnt complete see how many questions are completed and missed


                //group the question id

                var groupquestionscount = completedquestions.GroupBy(x => x.Id).Count();


                completedquestioncount.Text = groupquestionscount.ToString() + "/" + questions.Count.ToString();

                var ntcompletequestions = questions.Count - groupquestionscount;

                notcompletedquestioncount.Text = ntcompletequestions.ToString() + "/" + questions.Count.ToString();


                //if their are any missed questions dont let them submit it
                if (ntcompletequestions > 0)
                {
                    Vibration.Vibrate();
                    await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");

                    return;
                }

                //all question completed

                completeimg.Source = ImageSource.FromFile("successtick.png");
                completetext.Text = "Questionnaire Completed";
                //if (groupquestionscount == questions.Count)
                //{
                //    completeimg.Source = ImageSource.FromFile("successtick.png");
                //    completetext.Text = "Questionnaire Completed";
                //}
                //else
                //{
                //    completeimg.Source = ImageSource.FromFile("canceltick.png");
                //    completetext.Text = "Incomplete Questionnaire";


                //    scorelbl.IsVisible = false;
                //    scorebtn.IsVisible = false;
                //    scoreline.IsVisible = false;
                //}


                if (edit == true)
                {

                }
                else
                {
                    //add a new user questionnaire first
                    //var insertnewuserquestionnaire = new UserQuestionnaire();

                    //   insertnewuserquestionnaire.Userid = Helpers.Settings.UserKey;
                    //   insertnewuserquestionnaire.Questionnaireid = passedquestionnaire.Id;

                    //calulate the percentage completed
                    double num = Convert.ToDouble(groupquestionscount) / Convert.ToDouble(questions.Count);

                    insertnewuserquestionnaire.Complete = usercompleting;

                    if (groupquestionscount == questions.Count)
                    {
                        //questionnaire is completed doesnt need to be active
                        insertnewuserquestionnaire.Isactive = false;
                    }
                    else
                    {
                        insertnewuserquestionnaire.Isactive = true;
                    }

                    insertnewuserquestionnaire.DateComplete = DateTime.Now.ToString("dd/MM/yyyy");
                    insertnewuserquestionnaire.TimeComplete = DateTime.Now.ToShortTimeString();

                    await userquestionnairemanager.AddUserQuestionnaire(insertnewuserquestionnaire);

                    //add user questionnaire into collection



                    //add score here if nessecary
                    //insertnewuserquestionnaire.Score = "";


                    //update the user answers in the user answer table

                    foreach (var item in completedquestions)
                    {
                        var newuserquestionnaire = new UserQuestionAnswer();

                        newuserquestionnaire.Userid = Helpers.Settings.UserKey;
                        newuserquestionnaire.Questionid = item.Id;
                        newuserquestionnaire.Notes = string.Empty;

                        //ms 
                        if (item.Typems == true)
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;

                            if (gettherightanswer.Label.Contains("Please specify below"))
                            {
                                newuserquestionnaire.Notes = item.Notes;
                            }
                        }

                        //ss
                        else if (item.Typess == true)
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;

                            if (gettherightanswer.Label.Contains("Please specify below"))
                            {
                                newuserquestionnaire.Notes = item.Entryanswer;
                            }

                        }
                        //slider 1-10
                        else if (item.Type110 == true)
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Answervalue == item.Entryanswer).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;
                        }

                        //additonal ss with number and ss
                        else if (item.Type == "typeSS1")
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;


                            if (string.IsNullOrEmpty(item.Aqtype1number) && string.IsNullOrEmpty(item.Aqtype1ss))
                            {
                                //do nothing
                            }
                            else
                            {

                                newuserquestionnaire.Score = item.Aqtype1number + "|" + item.Aqtype1ss;
                            }
                        }

                        else if (item.Type == "typeSS2")
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;
                        }

                        else if (item.Type == "typeSSAdd")
                        {

                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;


                            //check if the label is yes only
                            //if the date is null is it todays date because the user hasnt changed the date picker

                            if (gettherightanswer.Label == "Yes")
                            {

                                if (string.IsNullOrEmpty(item.Aqdate))
                                {
                                    var dt = DateTime.Now.Date;
                                    item.Aqdate = dt.ToString("dd/MM/yyyy");
                                }

                                if (item.Notes.Contains("|"))
                                {
                                    //date and number entry

                                    newuserquestionnaire.ResponseDate = item.Aqdate;
                                    newuserquestionnaire.Score = item.Aqentry;

                                }
                                else
                                {
                                    //just date entry



                                    newuserquestionnaire.ResponseDate = item.Aqdate;
                                }
                            }



                        }

                        else
                        {
                            //find the answer id
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            if (item.TypeEntry == true)
                            {
                                newuserquestionnaire.Score = item.Entryanswer;
                            }

                            //if the answer list is greater than one check which value it is

                            if (getanswerid.Count > 1)
                            {
                                //check if answervalue or label is null first
                                var answervalueisnull = getanswerid.Where(x => x.Answervalue == null).ToList();

                                var answerlabelisnull = getanswerid.Where(x => x.Label == null).ToList();


                                if (answervalueisnull.Count > 0)
                                {
                                    //do the opposite as the items are all in the labels
                                    var gettherightanswer = answervalueisnull.Where(x => x.Label == item.Entryanswer).FirstOrDefault();

                                    newuserquestionnaire.Answerid = gettherightanswer.Id;
                                }
                                else if (answerlabelisnull.Count > 0)
                                {
                                    var gettherightanswer = answerlabelisnull.Where(x => x.Answervalue == item.Entryanswer).FirstOrDefault();

                                    newuserquestionnaire.Answerid = gettherightanswer.Id;
                                }
                                else
                                {
                                    var gettherightanswer = getanswerid.Where(x => x.Answervalue == item.Entryanswer).FirstOrDefault();

                                    newuserquestionnaire.Answerid = gettherightanswer.Id;
                                }





                            }
                            else
                            {
                                newuserquestionnaire.Answerid = getanswerid[0].Id;
                            }
                        }



                        newuserquestionnaire.Isactive = true;
                        //  newuserquestionnaire.Notes = string.Empty;
                        newuserquestionnaire.DateComplete = DateTime.Now.ToString("dd/MM/yyyy");
                        newuserquestionnaire.TimeComplete = DateTime.Now.ToShortTimeString();
                        newuserquestionnaire.Userquestionnaireid = insertnewuserquestionnaire.Id;




                        await userquestionanswermanager.AddUserQuestionAnswer(newuserquestionnaire);

                    }



                    //update the user questionnaire


                    questionnaireprogressbar.IsVisible = false;
                    progresslbl.IsVisible = false;

                   // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);
                    liststack.IsVisible = false;
                    successstack.IsVisible = true;
                    //   await scroll.ScrollToAsync(0, 0, false);
                }

                toolbar.IsEnabled = true;

            }
            catch (Exception ex)
            {
                var s = ex.StackTrace.ToString();
            }
        }



        void ExtendedButton_Clicked_1(System.Object sender, System.EventArgs e)
        {
            try
            {
                var label = (ExtendedButton)sender;

                var getitem = questions.Where(x => x.Answerid == label.IDValue).FirstOrDefault();


                getitem.Buttonborder = Colors.Transparent;
                getitem.Buttonborder1 = Color.FromArgb("#0F9FE2");
                getitem.Buttonborder2 = Colors.Transparent;
                getitem.Buttonborder3 = Colors.Transparent;
                getitem.Buttonborder4 = Colors.Transparent;

                getitem.Entryanswer = label.Text;

                completedquestions.Add(getitem);
            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedButton_Clicked_2(System.Object sender, System.EventArgs e)
        {
            try
            {
                var label = (ExtendedButton)sender;

                var getitem = questions.Where(x => x.Answerid == label.IDValue).FirstOrDefault();


                getitem.Buttonborder = Colors.Transparent;
                getitem.Buttonborder1 = Colors.Transparent;
                getitem.Buttonborder2 = Color.FromArgb("#0F9FE2");
                getitem.Buttonborder3 = Colors.Transparent;
                getitem.Buttonborder4 = Colors.Transparent;

                getitem.Entryanswer = label.Text;

                completedquestions.Add(getitem);
            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedButton_Clicked_3(System.Object sender, System.EventArgs e)
        {
            try
            {
                var label = (ExtendedButton)sender;

                var getitem = questions.Where(x => x.Answerid == label.IDValue).FirstOrDefault();


                getitem.Buttonborder = Colors.Transparent;
                getitem.Buttonborder1 = Colors.Transparent;
                getitem.Buttonborder2 = Colors.Transparent;
                getitem.Buttonborder3 = Color.FromArgb("#0F9FE2");
                getitem.Buttonborder4 = Colors.Transparent;

                getitem.Entryanswer = label.Text;

                completedquestions.Add(getitem);
            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedButton_Clicked_4(System.Object sender, System.EventArgs e)
        {
            try
            {
                var label = (ExtendedButton)sender;

                var getitem = questions.Where(x => x.Answerid == label.IDValue).FirstOrDefault();


                getitem.Buttonborder = Colors.Transparent;
                getitem.Buttonborder1 = Colors.Transparent;
                getitem.Buttonborder2 = Colors.Transparent;
                getitem.Buttonborder3 = Colors.Transparent;
                getitem.Buttonborder4 = Color.FromArgb("#0F9FE2");

                getitem.Entryanswer = label.Text;

                completedquestions.Add(getitem);
            }
            catch (Exception ex)
            {

            }
        }

        static bool ContainsNumber(string input)
        {
            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    return true;
                }
            }
            return false;
        }

        void Entry_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            try
            {

                var label = (ExtendedEntry)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();



                //percent check if answer only if they have typed
                if (ContainsNumber(e.NewTextValue))
                {
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);

                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }
                }



                getitem.Entryanswer = e.NewTextValue;

                if (completedquestions.Contains(getitem))
                {
                    completedquestions.Remove(getitem);
                    completedquestions.Add(getitem);
                }
                else
                {

                    completedquestions.Add(getitem);
                }

            }
            catch (Exception ex)
            {

            }
        }

        void rangeslider_ValueChanging(System.Object sender, RangeSliderValueChangingEventArgs e)
        {
            try
            {

                var label = (ExtendedRangeSlider)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                getitem.Entryanswer = e.NewRangeStart.ToString();


                if (completedquestions.Contains(getitem))
                {
                    completedquestions.Remove(getitem);
                    completedquestions.Add(getitem);
                }
                else
                {

                    completedquestions.Add(getitem);
                }


            }
            catch (Exception ex)
            {

            }
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            //got it button clicked
            try
            {

                if (iid32close == true)
                {
                    Application.Current.MainPage = new NavigationPage(new NotificationQuestionGP());
                    Navigation.RemovePage(this);
                }
                else
                {

                    //update the dash

                    MessagingCenter.Send<object, object>(this, "refreshdashafterquestionnaire", userquestionnaires);


                   Application.Current.MainPage = new NavigationPage(new MainDashboard());


                    Navigation.RemovePage(this);
                }
                //going to this page as it stops the time glitching


                //  await Navigation.PushAsync(new MainDashboard(userquestionnaires, weeknumber), false);


                //var pages = Navigation.NavigationStack.ToList();
                //int i = 0;
                //foreach (var page in pages)
                //{

                //    if (i == 0)
                //    {

                //    }
                //    else if (i == 1)
                //    {
                //        //do nothing
                //    }
                //    //else if (i == 2)
                //    //{
                //    //    //do nothing
                //    //}
                //    else
                //    {
                //        Navigation.RemovePage(page);

                //    }

                //    i++;
                //}




                //  await Navigation.PushAsync(new MainQuestionnaireView(), false);

            }
            catch (Exception ex)
            {

            }
        }

        void SfRadioButton_StateChanged(System.Object sender, StateChangedEventArgs e)
        {
            try
            {

                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();



                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    //percent check if answer
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);
                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }

                    if (completedquestions.Contains(getitem))
                    {

                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);


                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);

                    }

                    completequestioncounter++;
                }


                //if (completedquestions.Contains(checkifcontains))
                //{
                //    //if (e.IsChecked.Value)
                //    //{
                //    //    getitem.Ischecked0 = true;
                //    //}
                //    //else
                //    //{
                //    //    completedquestions.Remove(checkifcontains);
                //    //    getitem.Ischecked0 = false;
                //    //}

                //    completedquestions.Remove(checkifcontains);


                //    // getitem.UsingChecked0 = false;
                //    // completedquestions.Remove(checkifcontains);
                //    // completedquestions.Add(getitem);
                //}
                //else
                //{

                //}

                ////getitem.Ischecked0 = true;
                ////getitem.Ischecked1 = false;
                ////getitem.Ischecked2 = false;
                ////getitem.Ischecked3 = false;

                ////getitem.Entryanswer = label.Text;
                //getitem.ValueSelectedNum = 0;
                //getitem.Answerid = label.Text;

                //var newone = new Question();

                //newone.Answerid = label.Text;
                //newone.Id = getitem.Id;
                //newone.Typess = true;

                //completedquestions.Add(newone);




            }
            catch (Exception ex)
            {

            }
        }


        void SfRadioButton_StateChanged1(System.Object sender, StateChangedEventArgs e)
        {
            try
            {

                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                    //percent check if answer
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);
                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }
                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();


                    if (completedquestions.Contains(getitem))
                    {

                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);

                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }

                    completequestioncounter++;
                }


            }
            catch (Exception ex)
            {

            }
        }

        void SfRadioButton_StateChanged2(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                    if (getitem.Value2.Contains("Please specify below"))
                    {
                        getitem.Entryvis = true;
                    }
                    else
                    {
                        getitem.Entryvis = false;
                    }


                    //percent check if answer
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);
                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }
                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();


                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }

                    completequestioncounter++;
                }


            }
            catch (Exception ex)
            {

            }
        }

        void SfRadioButton_StateChanged3(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                    if (getitem.Value3.Contains("Please specify below"))
                    {
                        getitem.Entryvis = true;
                    }
                    else
                    {
                        getitem.Entryvis = false;
                    }

                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    //percent check if answer
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);
                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }

                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                }


            }
            catch (Exception ex)
            {

            }
        }

        void SfRadioButton_StateChanged4(System.Object sender, StateChangedEventArgs e)
        {
            try
            {

                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                    if (getitem.Value4.Contains("Please specify below"))
                    {
                        getitem.Entryvis = true;
                    }
                    else
                    {
                        getitem.Entryvis = false;
                    }

                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    //percent check if answer
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);
                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }

                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                }


            }
            catch (Exception ex)
            {

            }
        }

        void SfRadioButton_StateChanged5(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                    if (getitem.Value5.Contains("Please specify below"))
                    {
                        getitem.Entryvis = true;
                    }
                    else
                    {
                        getitem.Entryvis = false;
                    }
                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    //percent check if answer
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);
                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }

                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        void SfRadioButton_StateChanged6(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                    if (getitem.Value6.Contains("Please specify below"))
                    {
                        getitem.Entryvis = true;
                    }
                    else
                    {
                        getitem.Entryvis = false;
                    }
                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    //percent check if answer
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);
                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }

                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        void SfRadioButton_StateChanged7(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                    if (getitem.Value7.Contains("Please specify below"))
                    {
                        getitem.Entryvis = true;
                    }
                    else
                    {
                        getitem.Entryvis = false;
                    }
                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    //percent check if answer
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);
                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }

                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                }


            }
            catch (Exception ex)
            {

            }
        }

        void SfRadioButton_StateChanged8(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                    if (getitem.Value8.Contains("Please specify below"))
                    {
                        getitem.Entryvis = true;
                    }
                    else
                    {
                        getitem.Entryvis = false;
                    }
                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    //percent check if answer
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);
                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }

                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }


        void SfRadioButton_StateChanged9(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                    if (getitem.Value9.Contains("Please specify below"))
                    {
                        getitem.Entryvis = true;
                    }
                    else
                    {
                        getitem.Entryvis = false;
                    }
                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    //percent check if answer
                    var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (check == null)
                    {
                        completedquestionsforprogresspercent.Add(getitem);
                        questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                    }

                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        void SfCheckBox_StateChanged(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                var label = (ExtendedSFCheckBox)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id && x.Answerid == label.Text).FirstOrDefault();



                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }

                if (completedquestions.Contains(checkifcontains))
                {
                    if (e.IsChecked.Value)
                    {
                        getitem.UsingChecked0 = true;
                    }
                    else
                    {
                        completedquestions.Remove(checkifcontains);
                        getitem.UsingChecked0 = false;
                    }


                    // getitem.UsingChecked0 = false;
                    // completedquestions.Remove(checkifcontains);
                    // completedquestions.Add(getitem);
                }
                else
                {
                    getitem.UsingChecked0 = true;

                    //getitem.Entryanswer = label.Text;
                    getitem.ValueSelectedNum = 0;
                    getitem.Answerid = label.Text;

                    var newone = new Question();

                    newone.Answerid = label.Text;
                    newone.Id = getitem.Id;
                    newone.Typems = true;

                    completedquestions.Add(newone);
                }


            }
            catch (Exception ex)
            {

            }
        }

        void SfCheckBox_StateChanged_1(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                var label = (ExtendedSFCheckBox)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id && x.Answerid == label.Text).FirstOrDefault();


                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }

                if (completedquestions.Contains(checkifcontains))
                {
                    if (e.IsChecked.Value)
                    {
                        getitem.UsingChecked1 = true;
                    }
                    else
                    {
                        completedquestions.Remove(checkifcontains);
                        getitem.UsingChecked1 = false;
                    }


                    // getitem.UsingChecked0 = false;
                    // completedquestions.Remove(checkifcontains);
                    // completedquestions.Add(getitem);
                }
                else
                {
                    getitem.UsingChecked1 = true;

                    //getitem.Entryanswer = label.Text;
                    getitem.ValueSelectedNum = 1;
                    getitem.Answerid = label.Text;

                    var newone = new Question();

                    newone.Answerid = label.Text;
                    newone.Id = getitem.Id;
                    newone.Typems = true;

                    completedquestions.Add(newone);
                }


            }
            catch (Exception ex)
            {

            }
        }

        void SfCheckBox_StateChanged_2(System.Object sender, StateChangedEventArgs e)
        {

            try
            {


                var label = (ExtendedSFCheckBox)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id && x.Answerid == label.Text).FirstOrDefault();



                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }

                if (completedquestions.Contains(checkifcontains))
                {
                    if (e.IsChecked.Value)
                    {
                        getitem.UsingChecked2 = true;

                        if (getitem.Value2.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = true;
                        }
                    }
                    else
                    {
                        completedquestions.Remove(checkifcontains);
                        getitem.UsingChecked2 = false;

                        if (getitem.Value2.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = false;
                        }
                    }


                    // getitem.UsingChecked0 = false;
                    // completedquestions.Remove(checkifcontains);
                    // completedquestions.Add(getitem);
                }
                else
                {
                    getitem.UsingChecked2 = true;

                    //getitem.Entryanswer = label.Text;
                    getitem.ValueSelectedNum = 2;
                    getitem.Answerid = label.Text;


                    var newone = new Question();

                    newone.Answerid = label.Text;
                    newone.Id = getitem.Id;
                    newone.Typems = true;


                    if (getitem.Value2.Contains("Please specify below"))
                    {
                        getitem.Entryvisms = true;
                        newone.Notes = getitem.Notes;
                    }

                    completedquestions.Add(newone);
                }


            }
            catch (Exception ex)
            {

            }

        }

        void SfCheckBox_StateChanged_3(System.Object sender, StateChangedEventArgs e)
        {

            try
            {


                var label = (ExtendedSFCheckBox)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id && x.Answerid == label.Text).FirstOrDefault();


                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }


                if (completedquestions.Contains(checkifcontains))
                {
                    if (e.IsChecked.Value)
                    {
                        getitem.UsingChecked3 = true;

                        if (getitem.Value3.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = true;
                        }
                    }
                    else
                    {
                        completedquestions.Remove(checkifcontains);
                        getitem.UsingChecked3 = false;

                        if (getitem.Value3.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = false;
                        }
                    }


                    // getitem.UsingChecked0 = false;
                    // completedquestions.Remove(checkifcontains);
                    // completedquestions.Add(getitem);
                }
                else
                {
                    getitem.UsingChecked3 = true;



                    //getitem.Entryanswer = label.Text;
                    // getitem.ValueSelectedNum = 0;
                    var newone = new Question();

                    newone.Answerid = label.Text;
                    newone.Id = getitem.Id;
                    newone.Typems = true;

                    if (getitem.Value3.Contains("Please specify below"))
                    {
                        getitem.Entryvisms = true;
                        newone.Notes = getitem.Notes;
                    }

                    completedquestions.Add(newone);
                }


            }
            catch (Exception ex)
            {

            }

        }

        void SfCheckBox_StateChanged_4(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                var label = (ExtendedSFCheckBox)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id && x.Answerid == label.Text).FirstOrDefault();


                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }

                if (completedquestions.Contains(checkifcontains))
                {
                    if (e.IsChecked.Value)
                    {
                        getitem.UsingChecked4 = true;

                        if (getitem.Value4.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = true;
                        }
                    }
                    else
                    {
                        completedquestions.Remove(checkifcontains);
                        getitem.UsingChecked4 = false;

                        if (getitem.Value4.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = false;
                        }
                    }


                    // getitem.UsingChecked0 = false;
                    // completedquestions.Remove(checkifcontains);
                    // completedquestions.Add(getitem);
                }
                else
                {
                    getitem.UsingChecked4 = true;



                    //getitem.Entryanswer = label.Text;
                    // getitem.ValueSelectedNum = 0;

                    var newone = new Question();

                    newone.Answerid = label.Text;
                    newone.Id = getitem.Id;
                    newone.Typems = true;

                    if (getitem.Value4.Contains("Please specify below"))
                    {
                        getitem.Entryvisms = true;
                        newone.Notes = getitem.Notes;
                    }

                    completedquestions.Add(newone);
                }


            }
            catch (Exception ex)
            {

            }

        }

        void SfCheckBox_StateChanged_5(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                var label = (ExtendedSFCheckBox)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();



                var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id && x.Answerid == label.Text).FirstOrDefault();


                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }

                if (completedquestions.Contains(checkifcontains))
                {
                    if (e.IsChecked.Value)
                    {
                        getitem.UsingChecked5 = true;

                        if (getitem.Value5.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = true;
                        }
                    }
                    else
                    {
                        completedquestions.Remove(checkifcontains);
                        getitem.UsingChecked5 = false;

                        if (getitem.Value5.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = false;
                        }
                    }


                    // getitem.UsingChecked0 = false;
                    // completedquestions.Remove(checkifcontains);
                    // completedquestions.Add(getitem);
                }
                else
                {
                    getitem.UsingChecked5 = true;



                    //getitem.Entryanswer = label.Text;
                    // getitem.ValueSelectedNum = 0;

                    var newone = new Question();

                    newone.Answerid = label.Text;
                    newone.Id = getitem.Id;
                    newone.Typems = true;

                    if (getitem.Value5.Contains("Please specify below"))
                    {
                        getitem.Entryvisms = true;
                        newone.Notes = getitem.Notes;
                    }

                    completedquestions.Add(newone);
                }


            }
            catch (Exception ex)
            {

            }

        }

        void SfCheckBox_StateChanged_6(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                var label = (ExtendedSFCheckBox)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id && x.Answerid == label.Text).FirstOrDefault();


                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }


                if (completedquestions.Contains(checkifcontains))
                {
                    if (e.IsChecked.Value)
                    {
                        getitem.UsingChecked6 = true;

                        if (getitem.Value6.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = true;
                        }
                    }
                    else
                    {
                        completedquestions.Remove(checkifcontains);
                        getitem.UsingChecked6 = false;

                        if (getitem.Value6.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = false;
                        }
                    }


                    // getitem.UsingChecked0 = false;
                    // completedquestions.Remove(checkifcontains);
                    // completedquestions.Add(getitem);
                }
                else
                {
                    getitem.UsingChecked6 = true;



                    //getitem.Entryanswer = label.Text;
                    // getitem.ValueSelectedNum = 0;

                    var newone = new Question();

                    newone.Answerid = label.Text;
                    newone.Id = getitem.Id;
                    newone.Typems = true;

                    if (getitem.Value6.Contains("Please specify below"))
                    {
                        getitem.Entryvisms = true;
                        newone.Notes = getitem.Notes;
                    }

                    completedquestions.Add(newone);
                }


            }
            catch (Exception ex)
            {

            }

        }

        void SfCheckBox_StateChanged_7(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                var label = (ExtendedSFCheckBox)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id && x.Answerid == label.Text).FirstOrDefault();


                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }

                if (completedquestions.Contains(checkifcontains))
                {
                    if (e.IsChecked.Value)
                    {
                        getitem.UsingChecked7 = true;

                        if (getitem.Value7.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = true;
                        }
                    }
                    else
                    {
                        completedquestions.Remove(checkifcontains);
                        getitem.UsingChecked7 = false;

                        if (getitem.Value7.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = true;
                        }
                    }


                    // getitem.UsingChecked0 = false;
                    // completedquestions.Remove(checkifcontains);
                    // completedquestions.Add(getitem);
                }
                else
                {
                    getitem.UsingChecked7 = true;




                    //getitem.Entryanswer = label.Text;
                    // getitem.ValueSelectedNum = 0;
                    var newone = new Question();

                    newone.Answerid = label.Text;
                    newone.Id = getitem.Id;
                    newone.Typems = true;

                    if (getitem.Value7.Contains("Please specify below"))
                    {
                        getitem.Entryvisms = true;
                        newone.Notes = getitem.Notes;
                    }

                    completedquestions.Add(newone);
                }


            }
            catch (Exception ex)
            {

            }

        }

        void SfCheckBox_StateChanged_8(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                var label = (ExtendedSFCheckBox)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();



                var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id && x.Answerid == label.Text).FirstOrDefault();


                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }

                if (completedquestions.Contains(checkifcontains))
                {
                    if (e.IsChecked.Value)
                    {
                        getitem.UsingChecked8 = true;

                        if (getitem.Value8.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = true;
                        }
                    }
                    else
                    {
                        completedquestions.Remove(checkifcontains);
                        getitem.UsingChecked8 = false;

                        if (getitem.Value8.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = false;
                        }
                    }


                    // getitem.UsingChecked0 = false;
                    // completedquestions.Remove(checkifcontains);
                    // completedquestions.Add(getitem);
                }
                else
                {
                    getitem.UsingChecked8 = true;




                    //getitem.Entryanswer = label.Text;
                    // getitem.ValueSelectedNum = 0;
                    var newone = new Question();

                    newone.Answerid = label.Text;
                    newone.Id = getitem.Id;
                    newone.Typems = true;

                    if (getitem.Value8.Contains("Please specify below"))
                    {
                        getitem.Entryvisms = true;
                        newone.Notes = getitem.Notes;
                    }

                    completedquestions.Add(newone);
                }


            }
            catch (Exception ex)
            {

            }

        }

        void SfCheckBox_StateChanged_9(System.Object sender, StateChangedEventArgs e)
        {
            try
            {


                var label = (ExtendedSFCheckBox)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id && x.Answerid == label.Text).FirstOrDefault();


                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }


                if (completedquestions.Contains(checkifcontains))
                {
                    if (e.IsChecked.Value)
                    {
                        getitem.UsingChecked9 = true;

                        if (getitem.Value9.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = true;
                        }
                    }
                    else
                    {
                        completedquestions.Remove(checkifcontains);
                        getitem.UsingChecked9 = false;

                        if (getitem.Value9.Contains("Please specify below"))
                        {
                            getitem.Entryvisms = true;
                        }

                    }


                    // getitem.UsingChecked0 = false;
                    // completedquestions.Remove(checkifcontains);
                    // completedquestions.Add(getitem);
                }
                else
                {
                    getitem.UsingChecked9 = true;



                    //getitem.Entryanswer = label.Text;
                    // getitem.ValueSelectedNum = 0;

                    var newone = new Question();

                    newone.Answerid = label.Text;
                    newone.Id = getitem.Id;
                    newone.Typems = true;

                    if (getitem.Value9.Contains("Please specify below"))
                    {
                        getitem.Entryvisms = true;
                        newone.Notes = getitem.Notes;
                    }

                    completedquestions.Add(newone);
                }


            }
            catch (Exception ex)
            {

            }

        }

        async void nextbtn_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                //if (agreelist.SelectedItems.Count == 0)
                //{
                //    Vibration.Vibrate();
                //    return;
                //}
                //else
                //{

                //   // agreestack.IsVisible = false;

                //    //  loadingstack.IsVisible = true;
                // //   await Navigation.PushPopupAsync(new Popuploadingpage("Questionnaire"), false);

                //    titlelbl.IsVisible = true;
                //    infolbl.IsVisible = true;
                //    liststack.Opacity = 0;
                //    // liststack.IsVisible = true;
                //    //liststack.IsVisible = true;



                //    await Task.Delay(2000);

                //    Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, true);
                //    // loadingstack.IsVisible = false;
                //    liststack.IsVisible = true;
                //    questionnaireprogressbar.IsVisible = true;
                //    progresslbl.IsVisible = true;

                //    liststack.Opacity = 1;
                //    await PopupNavigation.Instance.PopAsync();


                //}

            }
            catch (Exception ex)
            {

            }
        }

        void agreelist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {

                var item = e.DataItem as Questionnaire;

                if (item.Description.Contains("own"))
                {
                    usercompleting = "User";
                }
                else if (item.Description.Contains("else"))
                {
                    usercompleting = "Someone Else";
                }
                else if (item.Description.Contains("Health"))
                {
                    usercompleting = "HCP";
                }


            }
            catch (Exception ex)
            {

            }
        }

        void cancelbtn_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                Navigation.RemovePage(this);
            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedRadioButton_StateChanged(System.Object sender, StateChangedEventArgs e)
        {
            try
            {
                //additonal single yes selection clicked
                //show the additonal question added on , normally is just the date

                var label = (ExtendedRadioButton)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                if (e.IsChecked == false)
                {

                    getitem.Aqentry = string.Empty;
                    getitem.Aqdate = string.Empty;

                    if (getitem.Notes.Contains("|"))
                    {

                        getitem.AddVisible = false;
                        getitem.AddVisible2 = false;
                    }
                    else
                    {
                        getitem.AddVisible = false;
                    }
                }
                else
                {
                    if (getitem.Notes.Contains("|"))
                    {
                        getitem.AddVisible = true;
                        getitem.AddVisible2 = true;
                    }
                    else
                    {
                        getitem.AddVisible = true;
                    }


                }




            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedRadioButton_StateChanged_1(System.Object sender, StateChangedEventArgs e)
        {
            try
            {
                var label = (ExtendedRadioButton)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();



                if (e.IsChecked == false)
                {
                    if (getitem.Type == "typeSS1")
                    {

                        getitem.SSVis = false;
                    }

                    getitem.Aqtype1ss = string.Empty;
                    getitem.Aqtype1number = string.Empty;

                    //check if the list has any items with the id
                    // Check if the list contains the ID
                    if (questioncountlist.Contains(getitem.Id))
                    {
                        // Remove all occurrences of the ID from the list
                        questioncountlist.RemoveAll(item => item == getitem.Id);
                    }

                }
                else
                {
                    if (getitem.Type == "typeSS1")
                    {
                        getitem.SSVis = true;
                    }



                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }

                    //add 3 item as there is three options now
                    questioncountlist.Add(getitem.Id);
                    questioncountlist.Add(getitem.Id);
                    questioncountlist.Add(getitem.Id);
                }

            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedEntry_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            //single selection additonal questions of number and single selction

            try
            {

                var label = (ExtendedEntry)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                getitem.Aqtype1number = e.NewTextValue.ToString();

            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedRadioButton_StateChanged_2(System.Object sender, StateChangedEventArgs e)
        {
            //single selection additonal questions of number and single selction

            try
            {
                var label = (ExtendedRadioButton)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                getitem.Aqtype1ss = "Yes";
            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedRadioButton_StateChanged_3(System.Object sender, StateChangedEventArgs e)
        {
            //single selection additonal questions of number and single selction

            try
            {
                var label = (ExtendedRadioButton)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                getitem.Aqtype1ss = "No";
            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedRadioButton_StateChanged_4(System.Object sender, StateChangedEventArgs e)
        {
            //single selection additonal questions of number and single selction

            try
            {
                var label = (ExtendedRadioButton)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                getitem.Aqtype1ss = "Not Sure";
            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedRadioButton_StateChanged_5(System.Object sender, StateChangedEventArgs e)
        {
            try
            {

                var label = (ExtendedRadioButton)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                if (e.IsChecked == false)
                {
                    if (questioncountlist.Contains(getitem.Id))
                    {
                        // Remove all occurrences of the ID from the list
                        questioncountlist.RemoveAll(item => item == getitem.Id);
                    }
                }
                else
                {


                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();


                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }

                    questioncountlist.Add(getitem.Id);
                }
            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedRadioButton_StateChanged_6(System.Object sender, StateChangedEventArgs e)
        {
            try
            {
                var label = (ExtendedRadioButton)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                if (e.IsChecked == false)
                {
                    if (questioncountlist.Contains(getitem.Id))
                    {
                        // Remove all occurrences of the ID from the list
                        questioncountlist.RemoveAll(item => item == getitem.Id);
                    }
                }
                else
                {


                    // var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();


                    if (completedquestions.Contains(getitem))
                    {
                        completedquestions.Remove(getitem);
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }
                    else
                    {
                        getitem.Answerid = label.Text;
                        completedquestions.Add(getitem);
                    }

                    questioncountlist.Add(getitem.Id);
                }
            }
            catch (Exception ex)
            {

            }
        }

        void daypicker_DateSelected(System.Object sender, DateTimePickerSelectionChangedEventArgs e)
        {
            try
            {

                var label = (ExtendedSFDatePicker)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                DateTime selectedDate = (DateTime)e.NewValue;
                string formattedDate = selectedDate.ToString("dd/MM/yyyy");

                getitem.Aqdate = formattedDate;



            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedEntry_TextChanged_1(System.Object sender, TextChangedEventArgs e)
        {
            try
            {
                var label = (ExtendedEntry)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                getitem.Aqentry = e.NewTextValue;

            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedEntry_TextChanged_2(System.Object sender, TextChangedEventArgs e)
        {
            try
            {
                //other free entry answer

                var label = (ExtendedEntry)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                getitem.Entryanswer = e.NewTextValue;

            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedEntry_TextChanged_3(System.Object sender, TextChangedEventArgs e)
        {
            try
            {
                //other free entry answer

                var label = (ExtendedEntry)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                //update the completed question

                var getitemfromcq = completedquestions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                getitemfromcq.Notes = e.NewTextValue;

                getitem.Entryanswer = e.NewTextValue;

            }
            catch (Exception ex)
            {

            }
        }

        async void Button_Clicked_1(System.Object sender, System.EventArgs e)
        {
            //save button

            try
            {

                savebutton.IsEnabled = false;

                //save button clicked


                //check if the questionnaire is complete or not

                //if if isnt complete see how many questions are completed and missed


                //group the question id



                var newquestioncount = questions.Count - 2;

                var groupquestionscount = completedquestions.GroupBy(x => x.Id).Count();

                int countIfYes = completedquestions.Count(question => question.QuestionName.Contains("If you answered 'Yes'"));

                var completedquestionsafterifout = groupquestionscount - countIfYes;

                int countIfYesAllQuestions = questions.Count(question => question.QuestionName.Contains("If you answered 'Yes'"));


                var removeifquestions = questions.Count - countIfYesAllQuestions;


                //remove the 2 question header templates

                removeifquestions = removeifquestions - 2;

                completedquestioncount.Text = groupquestionscount.ToString() + "/" + questions.Count.ToString();

                var ntcompletequestions = removeifquestions - completedquestionsafterifout;

                notcompletedquestioncount.Text = ntcompletequestions.ToString() + "/" + questions.Count.ToString();


                //if their are any missed questions dont let them submit it
                if (ntcompletequestions > 0)
                {
                    Vibration.Vibrate();
                    await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                    savebutton.IsEnabled = true;
                    return;
                }

                //all question completed

                completeimg.Source = ImageSource.FromFile("successtick.png");
                completetext.Text = "Questionnaire Completed";

                //if (completedquestionsafterifout == removeifquestions)
                //{
                //    completeimg.Source = ImageSource.FromFile("successtick.png");
                //    completetext.Text = "Questionnaire Completed";
                //}
                //else
                //{
                //    completeimg.Source = ImageSource.FromFile("canceltick.png");
                //    completetext.Text = "Incomplete Questionnaire";


                //    scorelbl.IsVisible = false;
                //    scorebtn.IsVisible = false;
                //    scoreline.IsVisible = false;
                //}


                if (edit == true)
                {

                }
                else
                {
                    //add a new user questionnaire first
                    //  var insertnewuserquestionnaire = new UserQuestionnaire();

                    //insertnewuserquestionnaire.Userid = Helpers.Settings.UserKey;
                    //insertnewuserquestionnaire.Questionnaireid = passedquestionnaire.Id;

                    //calulate the percentage completed
                    double num = Convert.ToDouble(groupquestionscount) / Convert.ToDouble(questions.Count);

                    insertnewuserquestionnaire.Complete = "User";

                    if (groupquestionscount == questions.Count)
                    {
                        //questionnaire is completed doesnt need to be active
                        insertnewuserquestionnaire.Isactive = false;
                    }
                    else
                    {
                        insertnewuserquestionnaire.Isactive = true;
                    }

                    insertnewuserquestionnaire.DateComplete = DateTime.Now.ToString("dd/MM/yyyy");
                    insertnewuserquestionnaire.TimeComplete = DateTime.Now.ToShortTimeString();

                    await userquestionnairemanager.AddUserQuestionnaire(insertnewuserquestionnaire);


                    //add the new questionnaire to the collection so you can update the dash
                    userquestionnaires.Add(insertnewuserquestionnaire);

                    //add score here if nessecary
                    //insertnewuserquestionnaire.Score = "";


                    //update the user answers in the user answer table

                    foreach (var item in completedquestions)
                    {
                        var newuserquestionnaire = new UserQuestionAnswer();

                        newuserquestionnaire.Userid = Helpers.Settings.UserKey;
                        newuserquestionnaire.Questionid = item.Id;
                        newuserquestionnaire.Notes = string.Empty;

                        //ms 
                        if (item.Typems == true)
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;

                            if (gettherightanswer.Label.Contains("Please specify below"))
                            {
                                newuserquestionnaire.Notes = item.Notes;
                            }
                        }

                        //ss
                        else if (item.Typess == true)
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;

                            if (gettherightanswer.Label.Contains("Please specify below"))
                            {
                                newuserquestionnaire.Notes = item.Entryanswer;
                            }

                        }
                        //slider 1-10
                        else if (item.Type110 == true)
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Answervalue == item.Entryanswer).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;
                        }

                        //additonal ss with number and ss
                        else if (item.Type == "typeSS1")
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;


                            if (string.IsNullOrEmpty(item.Aqtype1number) && string.IsNullOrEmpty(item.Aqtype1ss))
                            {
                                //do nothing
                            }
                            else
                            {

                                newuserquestionnaire.Score = item.Aqtype1number + "|" + item.Aqtype1ss;
                            }
                        }

                        else if (item.Type == "typeSS2")
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;
                        }

                        else if (item.Type == "typeSSAdd")
                        {

                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;


                            //check if the label is yes only
                            //if the date is null is it todays date because the user hasnt changed the date picker

                            if (gettherightanswer.Label == "Yes")
                            {

                                if (string.IsNullOrEmpty(item.Aqdate))
                                {
                                    var dt = DateTime.Now.Date;
                                    item.Aqdate = dt.ToString("dd/MM/yyyy");
                                }

                                if (item.Notes.Contains("|"))
                                {
                                    //date and number entry

                                    newuserquestionnaire.ResponseDate = item.Aqdate;
                                    newuserquestionnaire.Score = item.Aqentry;

                                }
                                else
                                {
                                    //just date entry



                                    newuserquestionnaire.ResponseDate = item.Aqdate;
                                }
                            }



                        }

                        else if (item.Type == "typeSS3")
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;

                            if (string.IsNullOrEmpty(item.Aqtype3date1) && string.IsNullOrEmpty(item.Aqtype3date2))
                            {
                                //do nothing
                            }
                            else
                            {

                                newuserquestionnaire.ResponseDate = item.Aqtype3date1 + "|" + item.Aqtype3date2;
                                newuserquestionnaire.Score = item.Aqentry;
                            }
                        }
                        else if (item.Type == "numeric")
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = getanswerid[0].Id;

                            newuserquestionnaire.Score = item.Entryanswer;
                        }

                        else if (item.Type == "typeSSNumeric")
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = gettherightanswer.Id;

                            newuserquestionnaire.Score = item.Aqentry;
                        }

                        else if (item.Type == "typeDate")
                        {
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            //var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                            newuserquestionnaire.Answerid = getanswerid[0].Id;

                            newuserquestionnaire.ResponseDate = item.Aqdate;
                        }

                        else if (item.Type == "HeaderInfo")
                        {
                            //do nothing
                        }


                        else
                        {
                            //find the answer id
                            var getanswerid = answers.Where(x => x.Questionid == item.Id).ToList();

                            if (item.TypeEntry == true)
                            {
                                newuserquestionnaire.Score = item.Entryanswer;
                            }

                            //if the answer list is greater than one check which value it is

                            if (getanswerid.Count > 1)
                            {
                                //check if answervalue or label is null first
                                var answervalueisnull = getanswerid.Where(x => x.Answervalue == null).ToList();

                                var answerlabelisnull = getanswerid.Where(x => x.Label == null).ToList();


                                if (answervalueisnull.Count > 0)
                                {
                                    //do the opposite as the items are all in the labels
                                    var gettherightanswer = answervalueisnull.Where(x => x.Label == item.Entryanswer).FirstOrDefault();

                                    newuserquestionnaire.Answerid = gettherightanswer.Id;
                                }
                                else if (answerlabelisnull.Count > 0)
                                {
                                    var gettherightanswer = answerlabelisnull.Where(x => x.Answervalue == item.Entryanswer).FirstOrDefault();

                                    newuserquestionnaire.Answerid = gettherightanswer.Id;
                                }
                                else
                                {
                                    var gettherightanswer = getanswerid.Where(x => x.Answervalue == item.Entryanswer).FirstOrDefault();

                                    newuserquestionnaire.Answerid = gettherightanswer.Id;
                                }





                            }
                            else
                            {
                                newuserquestionnaire.Answerid = getanswerid[0].Id;
                            }
                        }



                        newuserquestionnaire.Isactive = true;
                        //  newuserquestionnaire.Notes = string.Empty;
                        newuserquestionnaire.DateComplete = DateTime.Now.ToString("dd/MM/yyyy");
                        newuserquestionnaire.TimeComplete = DateTime.Now.ToShortTimeString();
                        newuserquestionnaire.Userquestionnaireid = insertnewuserquestionnaire.Id;




                        await userquestionanswermanager.AddUserQuestionAnswer(newuserquestionnaire);

                    }



                    //update the user questionnaire

                    if (iid32close == true)
                    {
                        Application.Current.MainPage = new NavigationPage(new NotificationQuestionGP());
                        Navigation.RemovePage(this);
                    }
                    else
                    {


                        questionnaireprogressbar.IsVisible = false;
                        progresslbl.IsVisible = false;

                        // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);
                        liststack.IsVisible = false;
                        successstack.IsVisible = true;
                    }

                    Analytics.TrackEvent("Questionnaire Submitted");
                    //   await scroll.ScrollToAsync(0, 0, false);
                }

                savebutton.IsEnabled = true;

            }
            catch (Exception ex)
            {
                Analytics.TrackEvent("CRASH - QuestionnairePage - submit button - " + ex.StackTrace.ToString());
            }



        }

        //void daypickerstart_DateSelected(System.Object sender, Syncfusion.XForms.Pickers.DateChangedEventArgs e)
        //{

        //    try
        //    {

        //        var label = (ExtendedSFDatePicker)sender;

        //        var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


        //        DateTime selectedDate = (DateTime)e.NewValue;
        //        string formattedDate = selectedDate.ToString("dd/MM/yyyy");

        //        getitem.Aqtype3date1 = formattedDate;



        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}

        //void daypickerend_DateSelected(System.Object sender, Syncfusion.XForms.Pickers.DateChangedEventArgs e)
        //{

        //    try
        //    {

        //        var label = (ExtendedSFDatePicker)sender;

        //        var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


        //        DateTime selectedDate = (DateTime)e.NewValue;
        //        string formattedDate = selectedDate.ToString("dd/MM/yyyy");

        //        getitem.Aqtype3date2 = formattedDate;



        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}

        void ExtendedSFListview_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {

            try
            {
                //type date only

                var label = (ExtendedSFListview)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                var item = e.DataItem as Question;

                var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


                //convert the date string into date

                var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

                DateTime selectedDate = DateTime.Parse(datestring);
                string formattedDate = selectedDate.ToString("dd/MM/yyyy");

                getitem.Aqdate = formattedDate;

                //percent check if answer
                var check = completedquestionsforprogresspercent.Where(x => x.Id == getitem.Id).FirstOrDefault();

                if (check == null)
                {
                    completedquestionsforprogresspercent.Add(getitem);
                    questionnaireprogressbar.Progress = questionnaireprogressbar.Progress + percentincrease;
                }


                if (completedquestions.Contains(getitem))
                {
                    completedquestions.Remove(getitem);
                    completedquestions.Add(getitem);
                }
                else
                {

                    completedquestions.Add(getitem);
                }



            }
            catch (Exception ex)
            {

            }


        }

        void ExtendedSFListview_ItemTapped_1(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {

                //type SSAdd

                var label = (ExtendedSFListview)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                var item = e.DataItem as Question;

                var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


                //convert the date string into date

                var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

                DateTime selectedDate = DateTime.Parse(datestring);
                string formattedDate = selectedDate.ToString("dd/MM/yyyy");

                getitem.Aqdate = formattedDate;



            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedSFListview_ItemTapped_2(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            //type ss3 from date 

            try
            {

                //type SSAdd

                var label = (ExtendedSFListview)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                var item = e.DataItem as Question;

                var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


                //convert the date string into date

                var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

                DateTime selectedDate = DateTime.Parse(datestring);
                string formattedDate = selectedDate.ToString("dd/MM/yyyy");

                getitem.Aqtype3date1 = formattedDate;


                var c = completedquestions.Count();



            }
            catch (Exception ex)
            {

            }

        }

        void ExtendedSFListview_ItemTapped_3(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            //type ss3 to date

            try
            {

                //type SSAdd

                var label = (ExtendedSFListview)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                var item = e.DataItem as Question;

                var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


                //convert the date string into date

                var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

                DateTime selectedDate = DateTime.Parse(datestring);
                string formattedDate = selectedDate.ToString("dd/MM/yyyy");

                getitem.Aqtype3date2 = formattedDate;



            }
            catch (Exception ex)
            {

            }

        }

        async void ExtendedEntry_Completed(object sender, EventArgs e)
        {
            try
            {
                if (sender is ExtendedEntry entry)
                {
                    entry.Unfocus();
                    
                   // entry.IsEnabled = false;
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
