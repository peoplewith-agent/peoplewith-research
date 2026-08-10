using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Mopups.Pages;
using Mopups.Services;

namespace PeopleWithResearch
{
    public partial class AllQuestionnaires : ContentPage
    {
        public ObservableCollection<UserQuestionnaire> Previoususerquestionnaires = new ObservableCollection<UserQuestionnaire>();
        public UserQuestionnaireManager userquestionnairemanager;
        public int weeknumber;
        public ObservableCollection<UserQuestionnaire> alluserquestionnaires = new ObservableCollection<UserQuestionnaire>();
        public ObservableCollection<incidents> allincidents = new ObservableCollection<incidents>();
        public AnswerManager answermanager;
        public ObservableCollection<Question> questions = new ObservableCollection<Question>();

        public ObservableCollection<Answers> answers = new ObservableCollection<Answers>();
        public QuestionManager questionmanager;



        public AllQuestionnaires()
        {
            InitializeComponent();


            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;

            getuserquestionnaires();
        }

        public AllQuestionnaires(int weeknumberpassed, ObservableCollection<UserQuestionnaire> userquestionnairespassed, ObservableCollection<incidents> allincidentspassed)
        {
            InitializeComponent();

            weeknumber = weeknumberpassed;

            alluserquestionnaires = userquestionnairespassed;

            allincidents = allincidentspassed;

            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            questionmanager = QuestionManager.DefaultManager;

            getuserquestionnaires();


            getthequestionsandanswers();
        }

        async void getthequestionsandanswers()
        {
            try
            {
                //load the questions and answers once for every incident , instead of loading it every time for each weeklu questionnaire

                questions = await questionmanager.getQuestions(alluserquestionnaires[0].Questionnaireid);

                var newviewonlyquestions = new ObservableCollection<Question>();

                foreach (var it in questions)
                {
                    newviewonlyquestions.Add(it);
                }

                var getallanswers = await answermanager.getAllAnswerss();

                foreach (var item in questions)
                {
                    var getanswers = getallanswers.Where(x => x.Questionid == item.Id);

                    foreach (var i in getanswers)
                    {
                        answers.Add(i);
                    }
                }




            }
            catch (Exception ex)
            {

            }
        }


        async void getuserquestionnaires()
        {
            try
            {
                //var username = Helpers.Settings.UserKey;

                //var alluserquestionnaires = await userquestionnairemanager.getUserQuestionnaire();




                //check if any weeks are missing

                for (int i = 1; i <= weeknumber; i++)
                {

                    if (alluserquestionnaires.Any(x => x.Score == i.ToString()))
                    {
                        //do nothing as its already in
                    }
                    else
                    {
                        //means they have missed the week
                        var newquestionnaire = new UserQuestionnaire();
                        newquestionnaire.Weeknumber = "Week " + i.ToString();
                        newquestionnaire.Completestring = "Incomplete";
                        newquestionnaire.Score = i.ToString();
                        newquestionnaire.Imagename = "incompletequestionnaires.png";
                        newquestionnaire.Imagevis = false;

                        Previoususerquestionnaires.Add(newquestionnaire);
                    }


                }


                foreach (var item in alluserquestionnaires)
                {


                    item.Weeknumber = "Week " + item.Score;

                    item.Completestring = "Completed";
                    item.Imagename = "completequestionnaires.png";
                    item.Imagevis = true;

                    Previoususerquestionnaires.Add(item);

                }

                var orderedPrevioususerquestionnaires = Previoususerquestionnaires.OrderBy(x => Convert.ToInt32(x.Score));

                previousquestionnairelist.ItemsSource = orderedPrevioususerquestionnaires;

                previousquestionnairelist.HeightRequest = orderedPrevioususerquestionnaires.Count() * 100;



            }
            catch (Exception ex)
            {

            }
        }

        async void previousquestionnairelist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {

                //var item = e.DataItem as UserQuestionnaire;

                //if (item.Imagename == "incompletequestionnaires.png")
                //{

                //}
                //else
                //{
                   
                //   // await Navigation.PushPopupAsync(new NotificationQuestion("Viewonly", item.Id, questions, answers), false);

                //   // var popupPage = new NotificationQuestion("Viewonly", item.Id, questions, answers);
                //    await MopupService.Instance.PushAsync(new NotificationQuestion("Viewonly", item.Id, questions, answers), false);

                //}


            }
            catch (Exception ex)
            {

            }
        }
    }
}

