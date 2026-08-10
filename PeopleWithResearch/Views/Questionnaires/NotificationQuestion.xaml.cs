using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
//using Rg.Plugins.Popup.Services;
using Microsoft.Maui.Controls.PlatformConfiguration;
//using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using static Microsoft.Maui.ApplicationModel.Permissions;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Mopups.Services;

namespace PeopleWithResearch
{
    public partial class NotificationQuestion : Mopups.Pages.PopupPage
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

        public NotificationQuestion()
        {
            InitializeComponent();

           // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //On<iOS>().SetUseSafeArea(false);
        }

        public NotificationQuestion(string viewonly, string questionnaireidpassed, ObservableCollection<Question> questionspassed, ObservableCollection<Answers> answerspassed)
        {
            InitializeComponent();

           // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

           // On<iOS>().SetUseSafeArea(false);

            questionstack.IsVisible = false;

            userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;
            questionamanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            incidentsManager = IncidentsManager.DefaultManager;

            //this is the userquestionnaire id below

            questionnaireid = questionnaireidpassed;

            questions = questionspassed;

            answers = answerspassed;


            getuserreponses();

            // getquestionandanswersanduserrepsonses();

        }

        public NotificationQuestion(string[] categorydetailspassed)
        {
            InitializeComponent();

           // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

          //  On<iOS>().SetUseSafeArea(false);


            questionamanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            incidentsManager = IncidentsManager.DefaultManager;
            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;

            notificationdeatils = categorydetailspassed;


            //check if the user has completed questionnaire already for the week

            checkifuserhascompletedalready();


            getquestionandanswer();
        }

        async void checkifuserhascompletedalready()
        {
            try
            {
                //split the string to find the weekly number
                var d = notificationdeatils[4];
                string[] num = d.Split(':');

                var endstring = num[1];

                //get the number from the string
                string output = string.Concat(endstring.Where(Char.IsDigit));


                //check if the user has an open incident and update the progress

                var checkincidents = await incidentsmanager.GetSpecficIncident(output.ToString());
                var alluserquestionnaires = await userquestionnairemanager.getUserQuestionnaire();

                if (checkincidents.Count != 0)
                {
                    //check if there is any data in the notes column, if no that means it is neither they have selected
                    if (string.IsNullOrEmpty(checkincidents[0].Notes))
                    {
                        // do nothing
                    }
                    else
                    {


                        //if they have no answers it means they have not completed the questionnaire and left the app
                        var checkanswers = await userquestionanswermanager.getUserAnswers(checkincidents[0].Userquestionnaireid);


                        if (checkincidents.Count > 0 && checkanswers.Count == 0)
                        {
                            //delete the incident
                            var deleteincident = incidentsmanager.DeleteIncident(checkincidents[0]);

                            //delete the questionnaire
                            var finduserquestionnaire = alluserquestionnaires.Where(x => x.Score == output.ToString()).FirstOrDefault();

                            var deletequestionnaire = userquestionnairemanager.DeleteReason(finduserquestionnaire);

                        }
                        //this means they have already completed the questionnaire and filled out the answers
                        else if (checkincidents.Count > 0 && checkanswers.Count > 0)
                        {

                        }
                    }
                }




            }
            catch (Exception ex)
            {

            }

        }


        public NotificationQuestion(string[] categorydetailspassed, ObservableCollection<UserQuestionnaire> userquestionnairepassed, int wnumber)
        {
            InitializeComponent();

          //  Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

           // On<iOS>().SetUseSafeArea(false);


            questionamanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            incidentsManager = IncidentsManager.DefaultManager;
            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;

            notificationdeatils = categorydetailspassed;

            userquestionnaires = userquestionnairepassed;
            weeknumber = wnumber;

            dash = true;

            getquestionandanswer();
        }


        async void getuserreponses()
        {
            try
            {
                loadingstack.IsVisible = true;
                //get the user repsonses to the questions
                var newviewonlyquestions = new ObservableCollection<Question>();

                foreach (var it in questions)
                {
                    newviewonlyquestions.Add(it);
                }

                var usersanswers = await userquestionanswermanager.getUserAnswers(questionnaireid);

                //get the incident for that week as well using the user questionnaire id

                var getincident = await incidentsManager.GetSpecficIncidentbyuserquestionnaireid(questionnaireid);

                var findincident = await answermanager.getAnswerforincident(getincident[0].Weeklyfollowupanswerid);

                if (findincident != null)
                {
                    if (findincident[0].Label == "Neither")
                    {
                        await Navigation.PushAsync(new ViewquestionnaireProgress(findincident, getincident[0]), false);
                        //  await Navigation.PushAsync(new QuestionnairePage(newviewonlyquestions, answers, usersanswers, "viewonly", orderedquestions, "empty", getincident[0]), false);
                        await MopupService.Instance.PopAsync(true);
                    }
                    else
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


                        var counterforfirstlist = 0;

                        var allquestionsandanswers = new ObservableCollection<QuestionANDAnswers>();

                        var orderedquestions = questions.OrderBy(x => x.Questionorder).ToList();

                        foreach (var item in orderedquestions)
                        {
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

                                var checkanswer = usersanswers.Where(x => x.Questionid == item.Id).FirstOrDefault();

                                if (checkanswer != null)
                                {
                                    //check the index of the answer

                                    //this means their is a current answer

                                    var getanswervalue = answers.Where(x => x.Id == checkanswer.Answerid).FirstOrDefault();

                                    //find the index

                                    var indexofday = orderedansweredlist.FindIndex(s => s.Id == getanswervalue.Id);

                                    if (indexofday == 0)
                                    {
                                        item.Ischecked0 = true;
                                    }
                                    else if (indexofday == 1)
                                    {
                                        item.Ischecked1 = true;
                                    }
                                    else if (indexofday == 2)
                                    {
                                        item.Ischecked2 = true;
                                    }
                                    else if (indexofday == 3)
                                    {
                                        item.Ischecked3 = true;
                                    }
                                    else if (indexofday == 4)
                                    {
                                        item.Ischecked4 = true;
                                    }
                                    else if (indexofday == 5)
                                    {
                                        item.Ischecked5 = true;
                                    }
                                    else if (indexofday == 6)
                                    {
                                        item.Ischecked6 = true;
                                    }
                                    else if (indexofday == 7)
                                    {
                                        item.Ischecked7 = true;
                                    }
                                    else if (indexofday == 8)
                                    {
                                        item.Ischecked8 = true;
                                    }
                                    else if (indexofday == 9)
                                    {
                                        item.Ischecked9 = true;
                                    }

                                }


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


                                var checkanswer = usersanswers.Where(x => x.Questionid == item.Id).FirstOrDefault();

                                if (checkanswer != null)
                                {
                                    item.Entryanswer = checkanswer.Score;
                                }

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

                                    var checkanswer = usersanswers.Where(x => x.Questionid == item.Id).FirstOrDefault();

                                    var gettherightanswer = answers.Where(x => x.Id == checkanswer.Answerid).FirstOrDefault();

                                    if (checkanswer != null)
                                    {

                                        if (gettherightanswer.Label == "Yes")
                                        {
                                            item.AddVisible = true;
                                            item.Ischecked0 = true;
                                            item.Ischecked1 = false;
                                            item.Ischecked2 = false;

                                            item.AddVisible = true;
                                            item.AddVisible2 = true;

                                            //get the date

                                            //var splitday = checkanswer.ResponseDate.Split('/');

                                            //string monthName = GetMonthName(Convert.ToInt32(splitday[1]));

                                            //// Check if the string starts with '0'
                                            //if (splitday[0].StartsWith("0"))
                                            //{
                                            //    // Remove the '0' from the start
                                            //    splitday[0] = splitday[0].Substring(1);
                                            //}

                                            //var indexofdate = datelistforquestionnaire.IndexOf(datelistforquestionnaire.Where(x => x.Dateday == splitday[0]).Where(x => x.Datemonth == monthName).Where(x => x.Dateyear == splitday[2]).FirstOrDefault());

                                            //item.Singledateselecteditem = item.Datelist[indexofdate];

                                            ////add and remove the date from collection so selected date is first

                                            //var dateselected = item.Datelist[indexofdate];

                                            //datelistforquestionnaire.RemoveAt(indexofdate);
                                            //datelistforquestionnaire.Insert(0, dateselected);

                                            item.Datelabel = checkanswer.ResponseDate;

                                            item.Aqentry = checkanswer.Score;


                                        }
                                        else if (gettherightanswer.Label == "No")
                                        {
                                            item.AddVisible = false;
                                            item.Ischecked0 = false;
                                            item.Ischecked1 = true;
                                            item.Ischecked2 = false;
                                        }
                                        else
                                        {
                                            item.AddVisible = false;
                                            item.Ischecked0 = false;
                                            item.Ischecked1 = false;
                                            item.Ischecked2 = true;
                                        }

                                    }





                                }
                                else
                                {
                                    item.Addquestion = item.Notes;
                                    item.TypeSSAdd = true;
                                    item.Answerid = orderedansweredlist[0].Id;
                                    item.Value0 = orderedansweredlist[0].Label;
                                    item.Value1 = orderedansweredlist[1].Label;
                                    item.Value2 = orderedansweredlist[2].Label;


                                    var checkanswer = usersanswers.Where(x => x.Questionid == item.Id).FirstOrDefault();

                                    var gettherightanswer = answers.Where(x => x.Id == checkanswer.Answerid).FirstOrDefault();

                                    if (checkanswer != null)
                                    {

                                        if (gettherightanswer.Label == "Yes")
                                        {
                                            item.AddVisible = true;
                                            item.Ischecked0 = true;
                                            item.Ischecked1 = false;
                                            item.Ischecked2 = false;

                                            //get the date

                                            item.Datelabel = checkanswer.ResponseDate;

                                            //var splitday = checkanswer.ResponseDate.Split('/');

                                            //string monthName = GetMonthName(Convert.ToInt32(splitday[1]));

                                            //// Check if the string starts with '0'
                                            //if (splitday[0].StartsWith("0"))
                                            //{
                                            //    // Remove the '0' from the start
                                            //    splitday[0] = splitday[0].Substring(1);
                                            //}

                                            //var indexofdate = datelistforquestionnaire.IndexOf(datelistforquestionnaire.Where(x => x.Dateday == splitday[0]).Where(x => x.Datemonth == monthName).Where(x => x.Dateyear == splitday[2]).FirstOrDefault());

                                            //item.Singledateselecteditem = item.Datelist[indexofdate];

                                            ////add and remove the date from collection so selected date is first

                                            //var dateselected = item.Datelist[indexofdate];

                                            //datelistforquestionnaire.RemoveAt(indexofdate);
                                            //datelistforquestionnaire.Insert(0, dateselected);


                                        }
                                        else if (gettherightanswer.Label == "No")
                                        {
                                            item.AddVisible = false;
                                            item.Ischecked0 = false;
                                            item.Ischecked1 = true;
                                            item.Ischecked2 = false;
                                        }
                                        else
                                        {
                                            item.AddVisible = false;
                                            item.Ischecked0 = false;
                                            item.Ischecked1 = false;
                                            item.Ischecked2 = true;
                                        }

                                    }


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


                                //populating user answers
                                //fill in the questions completed

                                var checkanswer = usersanswers.Where(x => x.Questionid == item.Id).FirstOrDefault();

                                if (checkanswer != null)
                                {
                                    //check the index of the answer

                                    //this means their is a current answer

                                    var getanswervalue = answers.Where(x => x.Id == checkanswer.Answerid).FirstOrDefault();

                                    //find the index

                                    var indexofday = orderedansweredlist.FindIndex(s => s.Id == getanswervalue.Id);

                                    if (indexofday == 0)
                                    {
                                        item.Ischecked0 = true;
                                    }
                                    else if (indexofday == 1)
                                    {
                                        item.Ischecked1 = true;
                                    }
                                    else if (indexofday == 2)
                                    {
                                        item.Ischecked2 = true;
                                    }


                                    if (!string.IsNullOrEmpty(checkanswer.Score))
                                    {
                                        if (checkanswer.Score.Contains("|"))
                                        {
                                            //two values
                                            var splitanswer = checkanswer.Score.Split('|');

                                            item.SSVis = true;
                                            item.TypeSS1 = true;
                                            item.Aqentry = splitanswer[0];

                                            if (splitanswer[1] == "Yes")
                                            {
                                                item.UsingCheckedss0 = true;
                                            }
                                            else if (splitanswer[1] == "No")
                                            {
                                                item.UsingCheckedss1 = true;
                                            }
                                            else
                                            {
                                                item.UsingCheckedss2 = true;
                                            }
                                        }
                                    }


                                }




                            }

                            else if (item.Type == "typeDate")
                            {

                                var checkanswer = usersanswers.Where(x => x.Questionid == item.Id).FirstOrDefault();

                                if (checkanswer != null)
                                {
                                    item.Datelabel = checkanswer.ResponseDate;

                                    //var splitday = checkanswer.ResponseDate.Split('/');

                                    //string monthName = GetMonthName(Convert.ToInt32(splitday[1]));

                                    //// Check if the string starts with '0'
                                    //if (splitday[0].StartsWith("0"))
                                    //{
                                    //    // Remove the '0' from the start
                                    //    splitday[0] = splitday[0].Substring(1);
                                    //}

                                    //var indexofdate = datelistforquestionnaire.IndexOf(datelistforquestionnaire.Where(x => x.Dateday == splitday[0]).Where(x => x.Datemonth == monthName).Where(x => x.Dateyear == splitday[2]).FirstOrDefault());

                                    //item.Singledateselecteditem = item.Datelist[indexofdate];

                                    ////add and remove the date from collection so selected date is first

                                    //var dateselected = item.Datelist[indexofdate];

                                    //datelistforquestionnaire.RemoveAt(indexofdate);
                                    //datelistforquestionnaire.Insert(0,dateselected);
                                    //item.Dateop = 0.2;
                                }




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

                                var checkanswer = usersanswers.Where(x => x.Questionid == item.Id).FirstOrDefault();

                                var gettherightanswer = answers.Where(x => x.Id == checkanswer.Answerid).FirstOrDefault();

                                if (checkanswer != null)
                                {

                                    if (gettherightanswer.Label == "Yes")
                                    {
                                        item.AddVisible = true;
                                        item.Ischecked0 = true;
                                        item.Ischecked1 = false;
                                        item.Ischecked2 = false;

                                        var splitdate = checkanswer.ResponseDate.Split('|');

                                        item.Datelabel = splitdate[0];
                                        item.Datelabel2 = splitdate[1];


                                    }
                                    else if (gettherightanswer.Label == "No")
                                    {
                                        item.AddVisible = false;
                                        item.Ischecked0 = false;
                                        item.Ischecked1 = true;
                                        item.Ischecked2 = false;
                                    }
                                    else
                                    {
                                        item.AddVisible = false;
                                        item.Ischecked0 = false;
                                        item.Ischecked1 = false;
                                        item.Ischecked2 = true;
                                    }
                                }


                            }
                            else if (item.Type == "typeSSNumeric")
                            {
                                item.TypeSSNumeric = true;
                                item.Addquestion = item.Notes;


                                item.Answerid = orderedansweredlist[0].Id;
                                item.Value0 = orderedansweredlist[0].Label;
                                item.Value1 = orderedansweredlist[1].Label;
                                item.Value2 = orderedansweredlist[2].Label;

                                var checkanswer = usersanswers.Where(x => x.Questionid == item.Id).FirstOrDefault();

                                var gettherightanswer = answers.Where(x => x.Id == checkanswer.Answerid).FirstOrDefault();

                                if (checkanswer != null)
                                {

                                    if (gettherightanswer.Label == "Yes")
                                    {
                                        item.AddVisible = true;
                                        item.Ischecked0 = true;
                                        item.Ischecked1 = false;
                                        item.Ischecked2 = false;
                                        item.Aqentry = checkanswer.Score;
                                    }
                                    else if (gettherightanswer.Label == "No")
                                    {
                                        item.AddVisible = false;
                                        item.Ischecked0 = false;
                                        item.Ischecked1 = true;
                                        item.Ischecked2 = false;
                                    }
                                    else
                                    {
                                        item.AddVisible = false;
                                        item.Ischecked0 = false;
                                        item.Ischecked1 = false;
                                        item.Ischecked2 = true;
                                    }
                                }

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


                        await Navigation.PushAsync(new ViewquestionnaireProgress(newviewonlyquestions, answers, usersanswers, "viewonly", orderedquestions, "empty", getincident[0], findincident), false);
                        //  await Navigation.PushAsync(new QuestionnairePage(newviewonlyquestions, answers, usersanswers, "viewonly", orderedquestions, "empty", getincident[0]), false);
                        await MopupService.Instance.PopAsync(true);
                        //  loadingstack.IsVisible = false;

                    }
                }



            }
            catch (Exception ex)
            {
                var e = ex.StackTrace.ToString();

                loadingstack.IsVisible = false;
                somethingwrong.IsVisible = true;

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

                var question = await questionamanager.getQuestionbysignupcode(notificationdeatils[1]);


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

                        //add the answers in the listview

                        answerlist.ItemsSource = orderedanswerlist;

                    }
                }
            }
            catch (Exception ex)
            {

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

        async void getquestionandanswersanduserrepsonses()
        {
            try
            {
                //get the questions and answers
                loadingstack.IsVisible = true;

                questions = await questionamanager.getQuestions(questionnaireid);

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

                //get the user repsonses to the questions


                var usersanswers = await userquestionanswermanager.getUserAnswers(questionnaireid);


                //await Navigation.PushAsync(new QuestionnairePage(newviewonlyquestions, answers, usersanswers, "viewonly"), false);
                await MopupService.Instance.PopAsync(true);
                //  loadingstack.IsVisible = false;


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
                var d = notificationdeatils[4];
                string[] num = d.Split(':');

                var endstring = num[1];

                //get the number from the string
                string output = string.Concat(endstring.Where(Char.IsDigit));

                //add the week number in as the score
                newuserquestionnaireid.Score = output;


                await userquestionnairemanager.AddUserQuestionnaire(newuserquestionnaireid);

                var newincident = new incidents();

                newincident.Userid = Helpers.Settings.UserKey;
                newincident.Userquestionnaireid = newuserquestionnaireid.Id;


                newincident.Week = output;

                newincident.Weeklyfollowupanswerid = SelectedAnswer.Id;


                if (SelectedAnswer.Label != "Neither")
                {
                    newincident.Notes = "Active";
                    await incidentsManager.Addincidents(newincident);

                    questionstack.IsVisible = false;
                    closeimg.IsVisible = false;
                    thanksstack.IsVisible = true;

                }
                else
                {

                    await incidentsManager.Addincidents(newincident);

                    thanksstackNoIncident.IsVisible = true;
                    questionstack.IsVisible = false;
                    closeimg.IsVisible = false;

                }

                Analytics.TrackEvent("Questionnaire - First answer Submitted");

            }
            catch (Exception ex)
            {
                var s = ex.StackTrace.ToString();
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
                        await Navigation.PushAsync(new QuestionnairePageHardCoded(newuserquestionnaireid, userquestionnaires, weeknumber), false);
                       // await Navigation.PushAsync(new QuestionnairePage(questions, answers, newuserquestionnaireid, userquestionnaires, weeknumber), false);

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
                        await Navigation.PushAsync(new QuestionnairePageHardCoded(), false);
                       // await Navigation.PushAsync(new QuestionnairePage(questions, answers, newuserquestionnaireid), false);

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

                await Navigation.PushAsync(new MainDashboard(), false);
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

