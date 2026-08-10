using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using Plugin.Connectivity;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Networking;

namespace PeopleWithResearch
{
    public partial class ViewquestionnaireProgress : ContentPage
    {
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

        public ObservableCollection<Answers> userincidentdetails = new ObservableCollection<Answers>();

        public ViewquestionnaireProgress()
        {
            InitializeComponent();
        }



        public ViewquestionnaireProgress(ObservableCollection<Answers> incidentpassed, incidents userincidents)
        {
            InitializeComponent();

            //neither passed

            incidenttextlbl.Text = incidentpassed[0].Label;


            neitherstack.IsVisible = true;
            liststack.IsVisible = false;

            var date = userincidents.CreatedAt.ToString("dd MMM");

            neitherdate.Text = date;
            //show a fallback here as they havent done the questionnaire it doesnt need to show
            questionnairelist.IsVisible = false;

            incidentframe.IsVisible = true;
            incidentlbl.IsVisible = true;
        }

        public ViewquestionnaireProgress(ObservableCollection<Question> allquestions, ObservableCollection<Answers> allanswers, ObservableCollection<UserQuestionAnswer> useranswerpassed, string viewonly, List<Question> Orderedquestionsforlist, string empty, incidents passedincident, ObservableCollection<Answers> incidentdetails)
        {
            InitializeComponent();

            //viewonly



            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            Connectivity.ConnectivityChanged += ConnectivityChangedHandler;

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



            userincidentdetails = incidentdetails;

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

        async void getquestionnairedeatails()
        {
            try
            {
                var questionnaire = await questionnairemanager.getQuestionnaireSingle(questions[0].Questionnaireid);


                titlelbl.Text = questionnaire[0].Title;
                infolbl.Text = questionnaire[0].Description;
            }
            catch (Exception ex)
            {

            }
        }

        async void getincident()
        {
            try
            {

                // var findincident = await answermanager.getAnswerforincident(userincident.Weeklyfollowupanswerid);

                incidenttextlbl.Text = userincidentdetails[0].Label;


                if (incidenttextlbl.Text == "Neither")
                {
                    neitherstack.IsVisible = true;
                    liststack.IsVisible = false;

                    var date = userincident.CreatedAt.ToString("dd MMM yyyy");

                    neitherdate.Text = date;
                    //show a fallback here as they havent done the questionnaire it doesnt need to show
                    questionnairelist.IsVisible = false;

                }
                else
                {
                    var date = userincident.CreatedAt.ToString("dd MMM yyyy");

                    alldate.Text = date;

                    alldatelbl.IsVisible = true;
                    alldateframe.IsVisible = true;

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



    }
}

