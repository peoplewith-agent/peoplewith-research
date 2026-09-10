using CommunityToolkit.Mvvm.Messaging;
using PeoplewithResearch;
using Syncfusion.Maui.Buttons;
using System.Collections.ObjectModel;

namespace PeopleWithResearch;

public partial class QuestionnairePageHardCoded : ContentPage
{
    public ObservableCollection<Question> datelistforquestionnaire = new ObservableCollection<Question>();
    public UserQuestionnaire insertnewuserquestionnaire = new UserQuestionnaire();
    public ObservableCollection<UserQuestionnaire> userquestionnaires = new ObservableCollection<UserQuestionnaire>();
    public int weeknumber;
    public UserQuestionnaireManager userquestionnairemanager;
    public UserQuestionAnswerManager userquestionanswermanager;

    public ObservableCollection<UserQuestionAnswer> completedquestions = new ObservableCollection<UserQuestionAnswer>();
    public bool iid32close;

    public List<string> progresslist = new List<string>();

    double progressamount;

    string q1;
    string q1radio1;
    string q2;
    string q2radio1;
    string q3;
    string q3radio1;
    string q4;
    string q4radio1;
    string q5;
    string q6;
    string q7;
    string q8;
    string q9;
    string q10;
    string q11;
    string q12;
    string q13;
    string q13date;
    string q14;
    string q14date;
    string q15;
    string q15date;
    string q16;
    string q16date;
    string q17;
    string q17date;
    string q18;
    string q18date;
    string q19;
    string q19date;
    string q19entry;
    string q20;
    string q20entry;
    string q21;
    string q21date1;
    string q21date2;
    string q21entry;
    string q22;

    public QuestionnairePageHardCoded()
    {
        InitializeComponent();

    }

    public QuestionnairePageHardCoded(UserQuestionnaire userquestionnairepassed, ObservableCollection<UserQuestionnaire> alluserquestionnairepassed, int wnumber)
    {
        InitializeComponent();

        titlelbl.Text = "Symptom Questionnaire (Weekly Follow-up Study)";
        infolbl.Text = "The third study of diarrhoea and vomiting in the community. We want to know how often people in the UK suffer from diarrhoea or vomiting and the germs that cause this. Thank you for agreeing to fill in this questionnaire. Please read each question carefully before you answer it, and try to answer every question. Please either tick the appropriate box or write your answer in the space provided. The information that you give us will be treated in strict confidence.";

        userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
        userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;

        liststack.IsVisible = true;
        progresslbl.IsVisible = true;
        questionnaireprogressbar.IsVisible = true;

        Shell.SetNavBarIsVisible(this, false);

        getdatesforlist();

        insertnewuserquestionnaire = userquestionnairepassed;

        userquestionnaires = alluserquestionnairepassed;
        weeknumber = wnumber;

        progressamount = 4.5;
    }



    async void getdatesforlist()
    {
        try
        {
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

            q10list.ItemsSource = datelistforquestionnaire;
            q13list.ItemsSource = datelistforquestionnaire;
            q14list.ItemsSource = datelistforquestionnaire;
            q15list.ItemsSource = datelistforquestionnaire;
            q16list.ItemsSource = datelistforquestionnaire;
            q17list.ItemsSource = datelistforquestionnaire;
            q18list.ItemsSource = datelistforquestionnaire;
            q19list.ItemsSource = datelistforquestionnaire;
            q21list1.ItemsSource = datelistforquestionnaire;
            q21list2.ItemsSource = datelistforquestionnaire;
        }
        catch (Exception ex)
        {

        }
    }

    private void q1group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q1 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q1lbl.IsVisible = true;
                q1entry.IsVisible = true;
                q1lbl1.IsVisible = true;
                q1radiogroup.IsVisible = true;
            }
            else
            {
                q1lbl.IsVisible = false;
                q1entry.IsVisible = false;
                q1lbl1.IsVisible = false;
                q1radiogroup.IsVisible = false;
            }

            if(!progresslist.Contains("q1"))
            {
                progresslist.Add("q1");
                questionnaireprogressbar.Progress += progressamount;
            }

        }
        catch (Exception ex)
        {

        }
    }

    private void q2group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q2 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q2stack.IsVisible = true;
            }
            else
            {
                q2stack.IsVisible = false;
            }

            if (!progresslist.Contains("q2"))
            {
                progresslist.Add("q2");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q3group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q3 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q3stack.IsVisible = true;
            }
            else
            {
                q3stack.IsVisible = false;
            }

            if (!progresslist.Contains("q3"))
            {
                progresslist.Add("q3");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q4group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q4 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q4stack.IsVisible = true;
            }
            else
            {
                q4stack.IsVisible = false;
            }

            if (!progresslist.Contains("q4"))
            {
                progresslist.Add("q4");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q13group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q13 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q13stack.IsVisible = true;
            }
            else
            {
                q13stack.IsVisible = false;
            }

            if (!progresslist.Contains("q13"))
            {
                progresslist.Add("q13");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q14group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q14 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q14stack.IsVisible = true;
            }
            else
            {
                q14stack.IsVisible = false;
            }

            if (!progresslist.Contains("q14"))
            {
                progresslist.Add("q14");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q15group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q15 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q15stack.IsVisible = true;
            }
            else
            {
                q15stack.IsVisible = false;
            }

            if (!progresslist.Contains("q15"))
            {
                progresslist.Add("q15");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q16group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q16 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q16stack.IsVisible = true;
            }
            else
            {
                q16stack.IsVisible = false;
            }

            if (!progresslist.Contains("q16"))
            {
                progresslist.Add("q16");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q17group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q17 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q17stack.IsVisible = true;
            }
            else
            {
                q17stack.IsVisible = false;
            }

            if (!progresslist.Contains("q17"))
            {
                progresslist.Add("q17");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q18group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q18 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q18stack.IsVisible = true;
            }
            else
            {
                q18stack.IsVisible = false;
            }

            if (!progresslist.Contains("q18"))
            {
                progresslist.Add("q18");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q19group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q19 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q19stack.IsVisible = true;
            }
            else
            {
                q19stack.IsVisible = false;
            }

            if (!progresslist.Contains("q19"))
            {
                progresslist.Add("q19");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q20group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q20 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q20stack.IsVisible = true;
            }
            else
            {
                q20stack.IsVisible = false;
            }

            if (!progresslist.Contains("q20"))
            {
                progresslist.Add("q20");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q21group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q21 = e.CurrentItem.Text;

            if (e.CurrentItem.Text == "Yes")
            {
                q21stack.IsVisible = true;
            }
            else
            {
                q21stack.IsVisible = false;
            }

            if (!progresslist.Contains("q21"))
            {
                progresslist.Add("q21");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    async void savebutton_Clicked(object sender, EventArgs e)
    {
        try
        {

            //  savebutton.IsEnabled = false;

            //check if each question has been answered

            if (string.IsNullOrEmpty(q1))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q2))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q3))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q4))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q5))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q6))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q7))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q8))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q9))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q10))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            //else if (string.IsNullOrEmpty(q11))
            //{
            //    Vibration.Vibrate();
            //    await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
            //    return;
            //}
            //else if (string.IsNullOrEmpty(q12))
            //{
            //    Vibration.Vibrate();
            //    await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
            //    return;
            //}
            else if (string.IsNullOrEmpty(q13))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q14))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q15))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q16))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q17))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q18))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q19))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q20))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q21))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }
            else if (string.IsNullOrEmpty(q22))
            {
                Vibration.Vibrate();
                await DisplayAlert("Complete Questionnaire", "Please complete all the questions", "Ok");
                return;
            }


            //all questions answered

          

            //disable it now so they can enter multiple entries and enable it at the end
            savebutton.IsVisible = false;
            uploadingquestionnairestack.IsVisible = true;
            scroll.IsEnabled = false;

            //add the user questionnaire to get the questionnaire id
            insertnewuserquestionnaire.Complete = "User";
            insertnewuserquestionnaire.Isactive = false;
            insertnewuserquestionnaire.DateComplete = DateTime.Now.ToString("dd/MM/yyyy");
            insertnewuserquestionnaire.TimeComplete = DateTime.Now.ToShortTimeString();

            await userquestionnairemanager.AddUserQuestionnaire(insertnewuserquestionnaire);


            //update the question with answers

            var question1 = new UserQuestionAnswer();

            question1.Questionid = "299B003E-1E57-4880-9E35-C76D112C0273";
            question1.Userid = Helpers.Settings.UserKey;

            if (q1 == "Yes")
            {
                question1.Answerid = "C9B6A8CA-636C-4D74-A00D-E6241F310712";
                question1.Score = q1entry.Text + "|" + q1radio1;
            }
            else if (q1 == "No")
            {
                question1.Answerid = "CDA36F8E-891F-4193-BA70-E29DAA2C2D5C";
            }
            else
            {
                question1.Answerid = "235CDD1F-B102-4780-8545-24D14781EFFC";
            }

            completedquestions.Add(question1);

            //////
            var question2 = new UserQuestionAnswer();

            question2.Questionid = "360B3C33-429A-4252-9023-4B3F9D0FB9C2";
            question2.Userid = Helpers.Settings.UserKey;

            if (q2 == "Yes")
            {
                question2.Answerid = "EEAB8B6F-AD2D-4638-BDA5-512E0E974275";
                question2.Score = q2entry.Text + "|" + q2radio1;
            }
            else if (q2 == "No")
            {
                question2.Answerid = "06A3E632-EBC6-41A3-B66A-1712F5804870";
            }
            else
            {
                question2.Answerid = "20B8AFA3-DE48-4F0C-B3BB-850CA3657E83";
            }

            completedquestions.Add(question2);

            //////
            var question3 = new UserQuestionAnswer();

            question3.Questionid = "90E24B52-3EB8-4BAE-8817-73925519BE35";
            question3.Userid = Helpers.Settings.UserKey;

            if (q3 == "Yes")
            {
                question3.Answerid = "0E81AF06-5DC9-478E-85CE-F93259CB8EFC";
                question3.Score = q3entry.Text + "|" + q3radio1;
            }
            else if (q3 == "No")
            {
                question3.Answerid = "B76B5DAD-E215-4707-8B7E-F5D3E9F4EF93";
            }
            else
            {
                question3.Answerid = "CB24D689-1857-44E0-BFD0-2AE89B0C478B";
            }

            completedquestions.Add(question3);

            //////
            var question4 = new UserQuestionAnswer();

            question4.Questionid = "2B8E9730-B1B3-45A3-8343-6BA6C1FC2DFA";
            question4.Userid = Helpers.Settings.UserKey;

            if (q4 == "Yes")
            {
                question4.Answerid = "97CB1905-2F8C-43E8-8B40-0317DECB52F3";
                question4.Score = q4entry.Text + "|" + q4radio1;
            }
            else if (q4 == "No")
            {
                question4.Answerid = "C3B0A06D-836C-45B4-87CF-8989C62476FE";
            }
            else
            {
                question4.Answerid = "E2AE132F-D1AF-47CB-A033-6A2C4CC54D71";
            }

            completedquestions.Add(question4);

            //////
            var question5 = new UserQuestionAnswer();

            question5.Questionid = "89BECE51-6D4E-4F0F-B04D-4FF406478C2A";
            question5.Userid = Helpers.Settings.UserKey;

            if (q5 == "Yes")
            {
                question5.Answerid = "D701DF98-2555-4F64-9820-C09C174709F7";
            }
            else if (q5 == "No")
            {
                question5.Answerid = "AE1A275A-63A9-49FA-897A-2EB8E5E80449";
            }
            else
            {
                question5.Answerid = "6D5495F4-CF3C-45E1-BFE5-964934D8470C";
            }

            completedquestions.Add(question5);


            //////
            var question6 = new UserQuestionAnswer();

            question6.Questionid = "1A5C2903-B3D0-4F59-A2F7-63B5255A8559";
            question6.Userid = Helpers.Settings.UserKey;

            if (q6 == "Yes")
            {
                question6.Answerid = "CB511F63-AC0F-4925-BA6D-CFDA74256BE3";
            }
            else if (q6 == "No")
            {
                question6.Answerid = "8F48B7AF-0504-44A1-B872-C99071FA70EF";
            }
            else
            {
                question6.Answerid = "393D0AE2-2103-4365-8724-D091C73E99F5";
            }


            completedquestions.Add(question6);

            //////
            var question7 = new UserQuestionAnswer();

            question7.Questionid = "3FB120D9-4371-4AE1-8E48-21A8AD399AD7";
            question7.Userid = Helpers.Settings.UserKey;

            if (q7 == "Yes")
            {
                question7.Answerid = "2BC5110E-A6A1-42A4-B8B6-F4ABF507849C";
            }
            else if (q7 == "No")
            {
                question7.Answerid = "4EBF24B5-3359-4A33-90AE-4D2EBE9828AC";
            }
            else
            {
                question7.Answerid = "4499A9C0-45CA-444F-BB2D-FD33B82D2740";
            }


            completedquestions.Add(question7);


            //////
            var question8 = new UserQuestionAnswer();

            question8.Questionid = "87E277A8-4D36-4394-9086-42964980C72B";
            question8.Userid = Helpers.Settings.UserKey;

            if (q8 == "Yes")
            {
                question8.Answerid = "85B0F421-6767-484C-88EA-2A9B1E674882";
            }
            else if (q8 == "No")
            {
                question8.Answerid = "F3AC80A6-549F-4213-8974-4F902E994DFA";
            }
            else
            {
                question8.Answerid = "1AB4A06C-4348-4512-9F8C-83AD6B4A8062";
            }


            completedquestions.Add(question8);


            //////
            var question9 = new UserQuestionAnswer();

            question9.Questionid = "D1CD1263-0853-4566-A6D9-8F1DEE51DDCA";
            question9.Userid = Helpers.Settings.UserKey;

            if (q9 == "Yes")
            {
                question9.Answerid = "D9F5152C-C08B-422E-91E0-762B8EF1ECC8";
            }
            else if (q9 == "No")
            {
                question9.Answerid = "D5EAEBDE-D38E-4344-B903-A3F1B1931217";
            }
            else
            {
                question9.Answerid = "16BC190B-1D84-474D-B98A-A22837BEF429";
            }


            completedquestions.Add(question9);

            //////
            var question10 = new UserQuestionAnswer();

            question10.Questionid = "2E971D9B-88B5-4E73-8A23-7F1FC3AF4124";
            question10.Userid = Helpers.Settings.UserKey;
            question10.Answerid = "DCAA521E-12FD-4C1F-A0C1-164E077DFB35";
            question10.ResponseDate = q10;

            completedquestions.Add(question10);

            //////
            var question11 = new UserQuestionAnswer();

            question11.Questionid = "749F3931-E4C9-4652-A323-AC72DF48730E";
            question11.Userid = Helpers.Settings.UserKey;
            question11.Answerid = "8EA8D2E1-D299-401D-B34D-5EBEB6A3E000";
            question11.Score = q11;

            completedquestions.Add(question11);

            //////
            var question12 = new UserQuestionAnswer();

            question12.Questionid = "E2387676-D557-41A8-A781-890252AD8829";
            question12.Userid = Helpers.Settings.UserKey;
            question12.Answerid = "7EE2B9AE-632C-45C8-AB83-518863505F26";
            question12.Score = q12;

            completedquestions.Add(question12);

            //////
            var question13 = new UserQuestionAnswer();

            question13.Questionid = "E0860FD9-9770-41D7-B901-F172F9D4FC0B";
            question13.Userid = Helpers.Settings.UserKey;
            if (q13 == "Yes")
            {
                question13.Answerid = "9EEBACD2-EE26-4933-AD72-0A30AC8F1C5A";
                question13.ResponseDate = q13date;
            }
            else if (q13 == "No")
            {
                question13.Answerid = "4628E73D-DC07-4405-91B4-EE22C08E7211";
            }
            else
            {
                question13.Answerid = "3126CC67-BEC2-45C0-BDD7-27DD81D64100";
            }
            

            completedquestions.Add(question13);


            //////
            var question14 = new UserQuestionAnswer();

            question14.Questionid = "EB276FFC-CD1C-454D-8BEC-D95960015E75";
            question14.Userid = Helpers.Settings.UserKey;
            if (q14 == "Yes")
            {
                question14.Answerid = "3B476E1E-B309-45BE-AA45-F766E0D450BE";
                question14.ResponseDate = q14date;
            }
            else if (q14 == "No")
            {
                question14.Answerid = "9E869551-3DA1-43C4-83DE-34CEB176338B";
            }
            else
            {
                question14.Answerid = "98F0429C-A28F-42BE-89B0-776387224C3D";
            }


            completedquestions.Add(question14);


            //////
            var question15 = new UserQuestionAnswer();

            question15.Questionid = "38E2A954-ED33-4A28-B1C5-328D42128CB9";
            question15.Userid = Helpers.Settings.UserKey;
            if (q15 == "Yes")
            {
                question15.Answerid = "E3DDD382-78D8-4B3D-AE6C-54B6BE95DFDF";
                question15.ResponseDate = q15date;
            }
            else if (q15 == "No")
            {
                question15.Answerid = "DACD314E-CBA3-4B23-9EC9-D67637F81CF8";
            }
            else
            {
                question15.Answerid = "A6B05411-E7DC-4428-B012-A3BFDDA58A7E";
            }


            completedquestions.Add(question15);

            //////
            var question16 = new UserQuestionAnswer();

            question16.Questionid = "3AD4BA3D-69FE-40D9-BF01-6DF90B6A72B9";
            question16.Userid = Helpers.Settings.UserKey;
            if (q16 == "Yes")
            {
                question16.Answerid = "07D377D4-8D18-493A-A137-57FD9D96899C";
                question16.ResponseDate = q16date;
            }
            else if (q16 == "No")
            {
                question16.Answerid = "2F322F9A-34AE-407B-99D9-978146868F5B";
            }
            else
            {
                question16.Answerid = "1493059D-B96B-4F7B-9442-F09CC4848DBA";
            }


            completedquestions.Add(question16);

            //////
            var question17 = new UserQuestionAnswer();

            question17.Questionid = "7460084A-F989-4CE8-A138-9342DB7F50CD";
            question17.Userid = Helpers.Settings.UserKey;
            if (q17 == "Yes")
            {
                question17.Answerid = "05F166DD-C3F9-4973-950C-8A5DCDF0BAB0";
                question17.ResponseDate = q17date;
            }
            else if (q17 == "No")
            {
                question17.Answerid = "28BE8ABC-C41A-4538-AE57-A361B70D9239";
            }
            else
            {
                question17.Answerid = "F0518F6B-DAD3-4CC1-97B0-62012A1DDD48";
            }


            completedquestions.Add(question17);

            //////
            var question18 = new UserQuestionAnswer();

            question18.Questionid = "C374142D-121D-4CD8-90AC-053D443A1654";
            question18.Userid = Helpers.Settings.UserKey;
            if (q18 == "Yes")
            {
                question18.Answerid = "F09A0E3A-EBF3-486E-90E8-732AEEC23D9E";
                question18.ResponseDate = q18date;
            }
            else if (q18 == "No")
            {
                question18.Answerid = "E40ECD5C-6266-46E0-825B-20FC7E1A15F5";
            }
            else
            {
                question18.Answerid = "209BCA58-E501-4607-9B3D-9418D73F2E21";
            }


            completedquestions.Add(question18);

            //////
            var question19 = new UserQuestionAnswer();

            question19.Questionid = "48995C7C-9E27-4980-9C80-B2EC6E4B3F35";
            question19.Userid = Helpers.Settings.UserKey;
            if (q19 == "Yes")
            {
                question19.Answerid = "D3FBBAEB-89D0-42A5-909E-8094061DC42C";
                question19.ResponseDate = q19date;
                question19.Score = q19entry;
            }
            else if (q19 == "No")
            {
                question19.Answerid = "11D43527-C2EA-46EB-9986-7F211918B7B5";
            }
            else
            {
                question19.Answerid = "7D3C320E-F0E2-4E39-8956-CD73933BA969";
            }


            completedquestions.Add(question19);


            //////
            var question20 = new UserQuestionAnswer();

            question20.Questionid = "DA1993BA-C908-498B-A0BA-1D25D0506A23";
            question20.Userid = Helpers.Settings.UserKey;
            if (q20 == "Yes")
            {
                question20.Answerid = "8967DBDF-AF52-4B2D-9723-734CE33176C4";
                
                question20.Score = q20entry;
            }
            else if (q20 == "No")
            {
                question20.Answerid = "F3C8074D-707F-4106-B387-990A757EBF1F";
            }
            else
            {
                question20.Answerid = "728C3DDE-B146-498C-A3CF-C8C220E3A310";
            }


            completedquestions.Add(question20);

            //////
            var question21 = new UserQuestionAnswer();

            question21.Questionid = "DA2932F3-016C-4C2A-8565-C659B74B2FB6";
            question21.Userid = Helpers.Settings.UserKey;
            if (q21 == "Yes")
            {
                question21.Answerid = "D8F6E8AE-5066-4EB1-BE91-9142031D2853";
                question21.ResponseDate = q21date1 + "|" + q21date2;
                question21.Score = q21entry;
            }
            else if (q21 == "No")
            {
                question21.Answerid = "10EA5E74-6811-4A72-9D17-067C1D638447";
            }
            else
            {
                question21.Answerid = "891EDD64-0237-46CA-9748-98CD76F2B262";
            }


            completedquestions.Add(question21);

            //////
            var question22 = new UserQuestionAnswer();

            question22.Questionid = "41032920-2B92-455C-AC53-23B144DBB33A";
            question22.Userid = Helpers.Settings.UserKey;
            if (q22 == "Yes")
            {
                question22.Answerid = "608EC1B7-3AA6-4564-9AC8-B81EE99EC210";
               
            }
            else if (q22 == "No")
            {
                question22.Answerid = "05EFD604-AC52-499C-9C6C-EF0209E9842C";
            }
          


            completedquestions.Add(question22);



            foreach(var item in completedquestions)
            {
                item.Isactive = true;
                item.DateComplete = DateTime.Now.ToString("dd/MM/yyyy");
                item.TimeComplete = DateTime.Now.ToShortTimeString();
                item.Userquestionnaireid = insertnewuserquestionnaire.Id;



                await userquestionanswermanager.AddUserQuestionAnswer(item);


            }

            //check for iid32 close

            questionnaireprogressbar.IsVisible = false;
            progresslbl.IsVisible = false;
            liststack.IsVisible = false;
            scroll.IsEnabled = true;
            completeimg.Source = ImageSource.FromFile("successtick.png");
            completetext.Text = "Questionnaire Completed";
            successstack.IsVisible = true;




        }
        catch (Exception ex)
        {
            var s = ex.Message;
            var ss = ex.StackTrace;
        }
    }

    private void q1radiogroup_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q1radio1 = e.CurrentItem.Text;


        }
        catch (Exception ex)
        {

        }
    }

    private void q2radio_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q2radio1 = e.CurrentItem.Text;


        }
        catch (Exception ex)
        {

        }
    }

    private void q3radio_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q3radio1 = e.CurrentItem.Text;


        }
        catch (Exception ex)
        {

        }
    }

    private void q4radio_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q4radio1 = e.CurrentItem.Text;


        }
        catch (Exception ex)
        {

        }
    }

    private void q10list_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as Question;

            var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


            //convert the date string into date

            var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

            DateTime selectedDate = DateTime.Parse(datestring);
            string formattedDate = selectedDate.ToString("dd/MM/yyyy");

            q10 = formattedDate;

            if (!progresslist.Contains("q10"))
            {
                progresslist.Add("q10");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch(Exception ex)
        {

        }
    }

    private void ExtendedEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            q11 = e.NewTextValue;

            if (!progresslist.Contains("q11"))
            {
                progresslist.Add("q11");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch(Exception ex)
        {

        }
    }

    private void ExtendedEntry_TextChanged_1(object sender, TextChangedEventArgs e)
    {
        try
        {
            q12 = e.NewTextValue;

            if (!progresslist.Contains("q12"))
            {
                progresslist.Add("q12");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q13list_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as Question;

            var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


            //convert the date string into date

            var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

            DateTime selectedDate = DateTime.Parse(datestring);
            string formattedDate = selectedDate.ToString("dd/MM/yyyy");

            q13date = formattedDate;

         
        }
        catch (Exception ex)
        {

        }
    }

    private void q14list_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as Question;

            var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


            //convert the date string into date

            var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

            DateTime selectedDate = DateTime.Parse(datestring);
            string formattedDate = selectedDate.ToString("dd/MM/yyyy");

            q14date = formattedDate;


        }
        catch (Exception ex)
        {

        }
    }

    private void q15list_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as Question;

            var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


            //convert the date string into date

            var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

            DateTime selectedDate = DateTime.Parse(datestring);
            string formattedDate = selectedDate.ToString("dd/MM/yyyy");

            q15date = formattedDate;

         
        }
        catch (Exception ex)
        {

        }
    }

    private void q16list_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as Question;

            var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


            //convert the date string into date

            var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

            DateTime selectedDate = DateTime.Parse(datestring);
            string formattedDate = selectedDate.ToString("dd/MM/yyyy");

            q16date = formattedDate;

          
        }
        catch (Exception ex)
        {

        }
    }

    private void q17list_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as Question;

            var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


            //convert the date string into date

            var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

            DateTime selectedDate = DateTime.Parse(datestring);
            string formattedDate = selectedDate.ToString("dd/MM/yyyy");

            q17date = formattedDate;
        }
        catch (Exception ex)
        {

        }
    }

    private void q18list_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as Question;

            var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


            //convert the date string into date

            var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

            DateTime selectedDate = DateTime.Parse(datestring);
            string formattedDate = selectedDate.ToString("dd/MM/yyyy");

            q18date = formattedDate;
        }
        catch (Exception ex)
        {

        }
    }

    private void q19list_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as Question;

            var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


            //convert the date string into date

            var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

            DateTime selectedDate = DateTime.Parse(datestring);
            string formattedDate = selectedDate.ToString("dd/MM/yyyy");

            q19date = formattedDate;
        }
        catch (Exception ex)
        {

        }
    }

    private void ExtendedEntry_TextChanged_2(object sender, TextChangedEventArgs e)
    {
        try
        {
            q19entry = e.NewTextValue;
        }
        catch(Exception ex)
        {

        }
    }

    private void ExtendedEntry_TextChanged_3(object sender, TextChangedEventArgs e)
    {
        try
        {
            q20entry = e.NewTextValue;
        }
        catch (Exception ex)
        {

        }
    }

    private void q21list1_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as Question;

            var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


            //convert the date string into date

            var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

            DateTime selectedDate = DateTime.Parse(datestring);
            string formattedDate = selectedDate.ToString("dd/MM/yyyy");

           q21date1 = formattedDate;
        }
        catch (Exception ex)
        {

        }
    }

    private void q21list2_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as Question;

            var finditem = datelistforquestionnaire.Where(x => x.Dateday == item.Dateday).Where(x => x.Datemonth == item.Datemonth).FirstOrDefault();


            //convert the date string into date

            var datestring = finditem.Dateday + " " + finditem.Datemonth + " " + finditem.Dateyear;

            DateTime selectedDate = DateTime.Parse(datestring);
            string formattedDate = selectedDate.ToString("dd/MM/yyyy");

            q21date2 = formattedDate;
        }
        catch (Exception ex)
        {

        }
    }

    private void ExtendedEntry_TextChanged_4(object sender, TextChangedEventArgs e)
    {
        try
        {
            q21entry = e.NewTextValue;
        }
        catch (Exception ex)
        {

        }
    }

    private void q22group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q22 = e.CurrentItem.Text;

            if (!progresslist.Contains("q22"))
            {
                progresslist.Add("q22");
                questionnaireprogressbar.Progress += progressamount;
            }


        }
        catch (Exception ex)
        {

        }
    }

    private void q5group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q5 = e.CurrentItem.Text;

            if (!progresslist.Contains("q5"))
            {
                progresslist.Add("q5");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q6group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q6 = e.CurrentItem.Text;

            if (!progresslist.Contains("q6"))
            {
                progresslist.Add("q6");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void q7group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q7 = e.CurrentItem.Text;

            if (!progresslist.Contains("q7"))
            {
                progresslist.Add("q7");
                questionnaireprogressbar.Progress += progressamount;
            }

        }
        catch (Exception ex)
        {

        }
    }

    private void q8group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q8 = e.CurrentItem.Text;

            if (!progresslist.Contains("q8"))
            {
                progresslist.Add("q8");
                questionnaireprogressbar.Progress += progressamount;
            }

        }
        catch (Exception ex)
        {

        }
    }

    private void q9group_CheckedChanged(object sender, Syncfusion.Maui.Buttons.CheckedChangedEventArgs e)
    {
        try
        {
            q9 = e.CurrentItem.Text;

            if (!progresslist.Contains("q9"))
            {
                progresslist.Add("q9");
                questionnaireprogressbar.Progress += progressamount;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void Button_Clicked(object sender, EventArgs e)
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

                //MessagingCenter.Send<object, object>(this, "refreshdashafterquestionnaire", userquestionnaires);
                WeakReferenceMessenger.Default.Send(new refreshdashafterquestionnaire(userquestionnaires));


                Application.Current.MainPage = new NavigationPage(new MainDashboard());


                Navigation.RemovePage(this);
            }
            //going to this page as it stops the time glitching


            

        }
        catch (Exception ex)
        {

        }
    }
}