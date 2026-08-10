using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
//using Microsoft.Azure.Storage;
//using Microsoft.Azure.Storage.Blob;
//using Plugin.Connectivity;
//using SignaturePad.Forms;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Networking;
using Syncfusion.Maui.Buttons;
using Azure.Storage.Blobs;
using Maui.FreakyControls;

namespace PeopleWithResearch
{
    public partial class RegisterPage : ContentPage
    {
        public ObservableCollection<object> heightunit = new ObservableCollection<object>();
        public ObservableCollection<Customheightandweight> heightftlist = new ObservableCollection<Customheightandweight>();
        public ObservableCollection<Customheightandweight> heightcmlist = new ObservableCollection<Customheightandweight>();
        public string heightunitvalue;

        public ObservableCollection<object> weightunit = new ObservableCollection<object>();
        public ObservableCollection<Customheightandweight> weightkglist = new ObservableCollection<Customheightandweight>();
        public ObservableCollection<Customheightandweight> weightstonelist = new ObservableCollection<Customheightandweight>();
        public string weightunitvalue;

        public List<string> genlist = new List<string>();
        public List<string> ethnicitylist = new List<string>();
        public List<string> usinglist = new List<string>();
        public user newuser;
        public UserManager usermanager;
        public double progressamount;

        initialQuestionsManager initialquestionsmanager;
        bool showqestionpage;
        initialQuestionsAnswersManager initialquestionsanswersmanager;
        userInitialQuestionsResponsesManager userinitialquestionresponsesmanager;
        ConsentManager consentmanager;

        public ObservableCollection<initialQuestions> questions = new ObservableCollection<initialQuestions>();
        public ObservableCollection<initialQuestionsAnswers> questionanswers = new ObservableCollection<initialQuestionsAnswers>();
        public ObservableCollection<initialQuestions> completedquestions = new ObservableCollection<initialQuestions>();
        public ObservableCollection<features> FeaturesForReg = new ObservableCollection<features>();
        int numberforfeatures;

        public ObservableCollection<consent> ConsentDetails = new ObservableCollection<consent>();
        public ObservableCollection<consent> ConsentDetailsUnder18 = new ObservableCollection<consent>();

        public List<string> selectedconsentlist = new List<string>();
        public List<string> consentdetailslist = new List<string>();
        bool under18;

        string agepassed;
        bool ageover18forload;



        List<features> orderedFeatureList;

        AdvertManager advertmanager;
        bool hassigned;
        public UserConsentManager userconsentmanger;
        public MedDirectionsManager meddirectionsmanager;
        public string PDFinfostring;

        public QuestionManager questionamanager;
        public AnswerManager answermanager;

        public ObservableCollection<Question> iid32questions = new ObservableCollection<Question>();

        public ObservableCollection<Answers> iid32answers = new ObservableCollection<Answers>();


        public RegisterPage()
        {
            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            InitializeComponent();
        }



        public RegisterPage(user userpassed, ObservableCollection<initialQuestions> questionpassed, ObservableCollection<initialQuestionsAnswers> answerspassed, ObservableCollection<features> Regpassedfeatures)
        {

            InitializeComponent();

            usermanager = UserManager.DefaultManager;
            initialquestionsmanager = initialQuestionsManager.DefaultManager;
            initialquestionsanswersmanager = initialQuestionsAnswersManager.DefaultManager;
            userinitialquestionresponsesmanager = userInitialQuestionsResponsesManager.DefaultManager;
            advertmanager = AdvertManager.DefaultManager;
            userconsentmanger = UserConsentManager.DefaultManager;
            consentmanager = ConsentManager.DefaultManager;
            meddirectionsmanager = MedDirectionsManager.DefaultManager;
            questionamanager = QuestionManager.DefaultManager;
            answermanager = AnswerManager.DefaultManager;
            // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            FeaturesForReg = Regpassedfeatures;


            newuser = userpassed;

            createframe.IsVisible = true;
            numberforfeatures = 0;

            //calulate the number the progress bar goes up or down by
            var totalnum = FeaturesForReg.Count();
            double num = (100 / totalnum);


            //set the progress amount
            progressamount = num;

            topprogress.Progress += progressamount;
            //sort out the order of the features

            orderedFeatureList = FeaturesForReg.OrderBy(x => int.Parse(x.Order)).ToList();



            genlist.Add("Male");
            genlist.Add("Female");
            genlist.Add("Other");

            genderlist.ItemsSource = genlist;


            heightunit.Add("ft in");
            heightunit.Add("cm");
            segmentedcontrolheight.ItemsSource = heightunit;
            segmentedcontrolheight.SelectedItem = heightunit[0];
            heightunitvalue = "ft in";

            weightunit.Add("kg");
            weightunit.Add("st lbs");
            segmentedcontrolweight.ItemsSource = weightunit;
            segmentedcontrolweight.SelectedItem = weightunit[0];
            weightunitvalue = "kg";


            daypicker.MaximumDate = DateTime.Now.AddYears(1);
            daypicker.MinimumDate = DateTime.Now.AddYears(-150);

            // heightlist.ItemsSource = heightftlist;


            for (int i = 1; i <= 250; i++)
            {
                var newcmitem = new Customheightandweight();
                newcmitem.Valuenumber = i.ToString();
                if (i % 10 == 0)
                {
                    // Code to execute for multiples of 10
                    newcmitem.Mainnumber = true;
                    newcmitem.Grayvisible = false;
                }
                else
                {
                    // Code to execute for other values
                    newcmitem.Mainnumber = false;
                    newcmitem.Grayvisible = true;
                }

                heightcmlist.Add(newcmitem);
            }


            //height ft and inches loop

            for (int feet = 0; feet <= 8; feet++)
            {
                for (int inches = 0; inches < 12; inches++)
                {
                    // Output the result
                    var newcmitem = new Customheightandweight();

                    if (inches == 0)
                    {
                        newcmitem.Valuenumber = feet.ToString() + " ft";
                        newcmitem.Mainnumber = true;
                        newcmitem.Grayvisible = false;
                    }
                    else
                    {
                        newcmitem.Valuenumber = feet.ToString() + " ft " + inches + " in";
                        newcmitem.Mainnumber = false;
                        newcmitem.Grayvisible = true;
                    }


                    heightftlist.Add(newcmitem);

                }
            }

            // weightlist.ItemsSource = heightftlist;

            //heightlist.ItemsSource = heightcmlist;
            heightlist.ItemsSource = heightftlist;


            //weight kg list
            for (int i = 1; i <= 500; i++)
            {
                var newcmitem = new Customheightandweight();
                newcmitem.Valuenumber = i.ToString();
                if (i % 10 == 0)
                {
                    // Code to execute for multiples of 10
                    newcmitem.Mainnumber = true;
                    newcmitem.Grayvisible = false;
                }
                else
                {
                    // Code to execute for other values
                    newcmitem.Mainnumber = false;
                    newcmitem.Grayvisible = true;
                }

                weightkglist.Add(newcmitem);
            }


            //weight stone list
            for (int stone = 0; stone <= 100; stone++)
            {
                for (int pounds = 0; pounds < 14; pounds++)
                {
                    // Output the result
                    // Output the result
                    var newcmitem = new Customheightandweight();

                    if (pounds == 0)
                    {
                        newcmitem.Valuenumber = stone.ToString() + " st";
                        newcmitem.Mainnumber = true;
                        newcmitem.Grayvisible = false;
                    }
                    else
                    {
                        newcmitem.Valuenumber = stone.ToString() + " st " + pounds + " lbs";
                        newcmitem.Mainnumber = false;
                        newcmitem.Grayvisible = true;
                    }


                    weightstonelist.Add(newcmitem);
                }
            }

            weightlist.ItemsSource = weightkglist;



            ethnicitylist.Add("White");
            ethnicitylist.Add("White English");
            ethnicitylist.Add("White Welsh");
            ethnicitylist.Add("White Scottish");
            ethnicitylist.Add("White Northern Irish");
            ethnicitylist.Add("White Irish");
            ethnicitylist.Add("White Gypsy or Irish Traveller");
            ethnicitylist.Add("White Other");
            ethnicitylist.Add("Mixed White and Black Caribbean");
            ethnicitylist.Add("Mixed White and Black African");
            ethnicitylist.Add("Mixed White Other");
            ethnicitylist.Add("Asian Indian");
            ethnicitylist.Add("Asian Pakistani");
            ethnicitylist.Add("Asian Bangladeshi");
            ethnicitylist.Add("Asian Chinese");
            ethnicitylist.Add("Asian Other");
            ethnicitylist.Add("Black African ");
            ethnicitylist.Add("Black African American");
            ethnicitylist.Add("Black Caribbean");
            ethnicitylist.Add("Black Other");
            ethnicitylist.Add("Arab");
            ethnicitylist.Add("Hispanic");
            ethnicitylist.Add("Latino");
            ethnicitylist.Add("Native American");
            ethnicitylist.Add("Pacific Islander");
            ethnicitylist.Add("Other");
            ethnicitylist.Add("Prefer not to disclose");

            ethnicitylist.Sort();

            ethnlist.ItemsSource = ethnicitylist;

            //additonal questions
            questions = questionpassed;
            questionanswers = answerspassed;



            var itemcount = 1;

            foreach (var item in questions)
            {

                if (item.Type == "text")
                {
                    //entry
                    item.TypeEntry = true;
                }
                else if (item.Type == "numeric")
                {
                    //number entry
                    item.TypeNumberEntry = true;
                }
                else if (item.Type == "typeSS")
                {

                    var sortedanswers = questionanswers.Where(x => x.Questionid == item.Id);

                    var orderedansweredlist = sortedanswers.OrderBy(x => x.Order).ToList();




                    item.Typess = true;

                    if (orderedansweredlist.Count == 2)
                    {
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;

                        item.Hide3 = false;
                        item.Hide4 = false;
                        item.Hide5 = false;
                        item.Hide6 = false;
                        item.Hide7 = false;
                        item.Hide8 = false;

                        item.Hide9 = false;
                        item.Hide10 = false;
                        item.Hide11 = false;
                        item.Hide12 = false;
                        item.Hide13 = false;
                        item.Hide14 = false;
                        item.Hide15 = false;

                    }
                    else if (orderedansweredlist.Count == 3)
                    {
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Value2 = orderedansweredlist[2].Label;

                        item.Hide3 = true;
                        item.Hide4 = false;
                        item.Hide5 = false;
                        item.Hide6 = false;
                        item.Hide7 = false;
                        item.Hide8 = false;

                        item.Hide9 = false;
                        item.Hide10 = false;
                        item.Hide11 = false;
                        item.Hide12 = false;
                        item.Hide13 = false;
                        item.Hide14 = false;
                        item.Hide15 = false;

                    }
                    else if (orderedansweredlist.Count == 4)
                    {
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Value2 = orderedansweredlist[2].Label;
                        item.Value3 = orderedansweredlist[3].Label;

                        item.Hide3 = true;
                        item.Hide4 = true;
                        item.Hide5 = false;
                        item.Hide6 = false;
                        item.Hide7 = false;
                        item.Hide8 = false;

                        item.Hide9 = false;
                        item.Hide10 = false;
                        item.Hide11 = false;
                        item.Hide12 = false;
                        item.Hide13 = false;
                        item.Hide14 = false;
                        item.Hide15 = false;
                    }
                    else if (orderedansweredlist.Count == 5)
                    {
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Value2 = orderedansweredlist[2].Label;
                        item.Value3 = orderedansweredlist[3].Label;
                        item.Value4 = orderedansweredlist[4].Label;


                        item.Hide3 = true;
                        item.Hide4 = true;
                        item.Hide5 = true;
                        item.Hide6 = false;
                        item.Hide7 = false;
                        item.Hide8 = false;

                        item.Hide9 = false;
                        item.Hide10 = false;
                        item.Hide11 = false;
                        item.Hide12 = false;
                        item.Hide13 = false;
                        item.Hide14 = false;
                        item.Hide15 = false;
                    }
                    else if (orderedansweredlist.Count == 8)
                    {
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Value2 = orderedansweredlist[2].Label;
                        item.Value3 = orderedansweredlist[3].Label;
                        item.Value4 = orderedansweredlist[4].Label;
                        item.Value5 = orderedansweredlist[5].Label;
                        item.Value6 = orderedansweredlist[6].Label;
                        item.Value7 = orderedansweredlist[7].Label;

                        item.Hide3 = true;
                        item.Hide4 = true;
                        item.Hide5 = true;
                        item.Hide6 = true;
                        item.Hide7 = true;
                        item.Hide8 = true;

                        item.Hide9 = false;
                        item.Hide10 = false;
                        item.Hide11 = false;
                        item.Hide12 = false;
                        item.Hide13 = false;
                        item.Hide14 = false;
                        item.Hide15 = false;
                    }
                    else if (orderedansweredlist.Count == 15)
                    {
                        item.Value0 = orderedansweredlist[0].Label;
                        item.Value1 = orderedansweredlist[1].Label;
                        item.Value2 = orderedansweredlist[2].Label;
                        item.Value3 = orderedansweredlist[3].Label;
                        item.Value4 = orderedansweredlist[4].Label;
                        item.Value5 = orderedansweredlist[5].Label;
                        item.Value6 = orderedansweredlist[6].Label;
                        item.Value7 = orderedansweredlist[7].Label;
                        item.Value8 = orderedansweredlist[8].Label;
                        item.Value9 = orderedansweredlist[9].Label;
                        item.Value10 = orderedansweredlist[10].Label;
                        item.Value11 = orderedansweredlist[11].Label;
                        item.Value12 = orderedansweredlist[12].Label;
                        item.Value13 = orderedansweredlist[13].Label;
                        item.Value14 = orderedansweredlist[14].Label;

                        item.Hide3 = true;
                        item.Hide4 = true;
                        item.Hide5 = true;
                        item.Hide6 = true;
                        item.Hide7 = true;
                        item.Hide8 = true;
                        item.Hide9 = true;
                        item.Hide10 = true;
                        item.Hide11 = true;
                        item.Hide12 = true;
                        item.Hide13 = true;
                        item.Hide14 = true;
                        item.Hide15 = true;
                    }




                }

            }





            //order the question
            var questionsordered = questions.OrderBy(x => x.Questionorder);

            foreach (var item in questionsordered)
            {
                //add the row num
                item.Rownum = itemcount++.ToString();

                if (itemcount == 14 || itemcount == 15)
                {
                    item.Activeop = 0.3;
                    item.Isquestionactive = false;
                }
                else
                {
                    item.Activeop = 1;
                    item.Isquestionactive = true;
                }

                item.Question = item.Rownum.ToString() + ". " + item.Question;
            }



            //order the question
            questionnairelist.ItemsSource = questionsordered;

            //getconsentdetailsonload();
        }


        public RegisterPage(user userpassed, ObservableCollection<initialQuestions> questionpassed, ObservableCollection<initialQuestionsAnswers> answerspassed, ObservableCollection<features> Regpassedfeatures, int additionalfeaturespassed, double passedprogressbaramount)
        {
            try
            {
                InitializeComponent();

                usermanager = UserManager.DefaultManager;
                initialquestionsmanager = initialQuestionsManager.DefaultManager;
                initialquestionsanswersmanager = initialQuestionsAnswersManager.DefaultManager;
                userinitialquestionresponsesmanager = userInitialQuestionsResponsesManager.DefaultManager;
                advertmanager = AdvertManager.DefaultManager;
                userconsentmanger = UserConsentManager.DefaultManager;
                consentmanager = ConsentManager.DefaultManager;
                meddirectionsmanager = MedDirectionsManager.DefaultManager;
                questionamanager = QuestionManager.DefaultManager;
                answermanager = AnswerManager.DefaultManager;
                //   Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

                FeaturesForReg = Regpassedfeatures;


                newuser = userpassed;

                createframe.IsVisible = true;
                numberforfeatures = 0;

                //CrossConnectivity.Current.ConnectivityTypeChanged += async (sender, args) =>
                //{
                //    if (!CrossConnectivity.Current.IsConnected)
                //    {

                //    }
                //    else
                //    {
                //       // Navigation.RemovePage(this);
                //        await DisplayAlert("Network Connection Lost", "Please check your connection", "Ok");
                //    }
                //};

                //calulate the number the progress bar goes up or down by
                var totalnum = FeaturesForReg.Count() + additionalfeaturespassed;
                double num = (100 / totalnum);

                topprogress.AnimationDuration = 3;

                //set the progress amount
                progressamount = num;
                topprogress.Progress = num + passedprogressbaramount;


                


                //topprogress.SetProgress(passedprogressbaramount,0,Easing.BounceIn);

                //sort out the order of the features

                orderedFeatureList = FeaturesForReg.OrderBy(x => int.Parse(x.Order)).ToList();



                genlist.Add("Male");
                genlist.Add("Female");
                genlist.Add("Other");

                genderlist.ItemsSource = genlist;


                heightunit.Add("ft in");
                heightunit.Add("cm");
                segmentedcontrolheight.ItemsSource = heightunit;
                segmentedcontrolheight.SelectedItem = heightunit[0];
                heightunitvalue = "ft in";

                weightunit.Add("kg");
                weightunit.Add("st lbs");
                segmentedcontrolweight.ItemsSource = weightunit;
                segmentedcontrolweight.SelectedItem = weightunit[0];
                weightunitvalue = "kg";


                daypicker.MaximumDate = DateTime.Now.AddYears(1);
                daypicker.MinimumDate = DateTime.Now.AddYears(-150);

                // heightlist.ItemsSource = heightftlist;


                for (int i = 1; i <= 250; i++)
                {
                    var newcmitem = new Customheightandweight();
                    newcmitem.Valuenumber = i.ToString();
                    if (i % 10 == 0)
                    {
                        // Code to execute for multiples of 10
                        newcmitem.Mainnumber = true;
                        newcmitem.Grayvisible = false;
                    }
                    else
                    {
                        // Code to execute for other values
                        newcmitem.Mainnumber = false;
                        newcmitem.Grayvisible = true;
                    }

                    heightcmlist.Add(newcmitem);
                }


                //height ft and inches loop

                for (int feet = 0; feet <= 8; feet++)
                {
                    for (int inches = 0; inches < 12; inches++)
                    {
                        // Output the result
                        var newcmitem = new Customheightandweight();

                        if (inches == 0)
                        {
                            newcmitem.Valuenumber = feet.ToString() + " ft";
                            newcmitem.Mainnumber = true;
                            newcmitem.Grayvisible = false;
                        }
                        else
                        {
                            newcmitem.Valuenumber = feet.ToString() + " ft " + inches + " in";
                            newcmitem.Mainnumber = false;
                            newcmitem.Grayvisible = true;
                        }


                        heightftlist.Add(newcmitem);

                    }
                }

                // weightlist.ItemsSource = heightftlist;

                //heightlist.ItemsSource = heightcmlist;
                heightlist.ItemsSource = heightftlist;


                //weight kg list
                for (int i = 1; i <= 500; i++)
                {
                    var newcmitem = new Customheightandweight();
                    newcmitem.Valuenumber = i.ToString();
                    if (i % 10 == 0)
                    {
                        // Code to execute for multiples of 10
                        newcmitem.Mainnumber = true;
                        newcmitem.Grayvisible = false;
                    }
                    else
                    {
                        // Code to execute for other values
                        newcmitem.Mainnumber = false;
                        newcmitem.Grayvisible = true;
                    }

                    weightkglist.Add(newcmitem);
                }


                //weight stone list
                for (int stone = 0; stone <= 100; stone++)
                {
                    for (int pounds = 0; pounds < 14; pounds++)
                    {
                        // Output the result
                        // Output the result
                        var newcmitem = new Customheightandweight();

                        if (pounds == 0)
                        {
                            newcmitem.Valuenumber = stone.ToString() + " st";
                            newcmitem.Mainnumber = true;
                            newcmitem.Grayvisible = false;
                        }
                        else
                        {
                            newcmitem.Valuenumber = stone.ToString() + " st " + pounds + " lbs";
                            newcmitem.Mainnumber = false;
                            newcmitem.Grayvisible = true;
                        }


                        weightstonelist.Add(newcmitem);
                    }
                }

                weightlist.ItemsSource = weightkglist;



                ethnicitylist.Add("White");
                ethnicitylist.Add("White English");
                ethnicitylist.Add("White Welsh");
                ethnicitylist.Add("White Scottish");
                ethnicitylist.Add("White Northern Irish");
                ethnicitylist.Add("White Irish");
                ethnicitylist.Add("White Gypsy or Irish Traveller");
                ethnicitylist.Add("White Other");
                ethnicitylist.Add("Mixed White and Black Caribbean");
                ethnicitylist.Add("Mixed White and Black African");
                ethnicitylist.Add("Mixed White Other");
                ethnicitylist.Add("Asian Indian");
                ethnicitylist.Add("Asian Pakistani");
                ethnicitylist.Add("Asian Bangladeshi");
                ethnicitylist.Add("Asian Chinese");
                ethnicitylist.Add("Asian Other");
                ethnicitylist.Add("Black African ");
                ethnicitylist.Add("Black African American");
                ethnicitylist.Add("Black Caribbean");
                ethnicitylist.Add("Black Other");
                ethnicitylist.Add("Arab");
                ethnicitylist.Add("Hispanic");
                ethnicitylist.Add("Latino");
                ethnicitylist.Add("Native American");
                ethnicitylist.Add("Pacific Islander");
                ethnicitylist.Add("Other");
                ethnicitylist.Add("Prefer not to disclose");

                ethnicitylist.Sort();

                ethnlist.ItemsSource = ethnicitylist;

                //additonal questions
                questions = questionpassed;
                questionanswers = answerspassed;



                var itemcount = 1;

                foreach (var item in questions)
                {

                    if (item.Type == "text")
                    {
                        //entry
                        item.TypeEntry = true;
                    }
                    else if (item.Type == "numeric")
                    {
                        //number entry
                        item.TypeNumberEntry = true;
                    }
                    else if (item.Type == "typeSS")
                    {

                        var sortedanswers = questionanswers.Where(x => x.Questionid == item.Id);

                        var orderedansweredlist = sortedanswers.OrderBy(x => x.Order).ToList();




                        item.Typess = true;

                        if (orderedansweredlist.Count == 2)
                        {
                            item.Value0 = orderedansweredlist[0].Label;
                            item.Value1 = orderedansweredlist[1].Label;

                            item.Hide3 = false;
                            item.Hide4 = false;
                            item.Hide5 = false;
                            item.Hide6 = false;
                            item.Hide7 = false;
                            item.Hide8 = false;

                            item.Hide9 = false;
                            item.Hide10 = false;
                            item.Hide11 = false;
                            item.Hide12 = false;
                            item.Hide13 = false;
                            item.Hide14 = false;
                            item.Hide15 = false;

                        }
                        else if (orderedansweredlist.Count == 3)
                        {
                            item.Value0 = orderedansweredlist[0].Label;
                            item.Value1 = orderedansweredlist[1].Label;
                            item.Value2 = orderedansweredlist[2].Label;

                            item.Hide3 = true;
                            item.Hide4 = false;
                            item.Hide5 = false;
                            item.Hide6 = false;
                            item.Hide7 = false;
                            item.Hide8 = false;

                            item.Hide9 = false;
                            item.Hide10 = false;
                            item.Hide11 = false;
                            item.Hide12 = false;
                            item.Hide13 = false;
                            item.Hide14 = false;
                            item.Hide15 = false;

                        }
                        else if (orderedansweredlist.Count == 4)
                        {
                            item.Value0 = orderedansweredlist[0].Label;
                            item.Value1 = orderedansweredlist[1].Label;
                            item.Value2 = orderedansweredlist[2].Label;
                            item.Value3 = orderedansweredlist[3].Label;

                            item.Hide3 = true;
                            item.Hide4 = true;
                            item.Hide5 = false;
                            item.Hide6 = false;
                            item.Hide7 = false;
                            item.Hide8 = false;

                            item.Hide9 = false;
                            item.Hide10 = false;
                            item.Hide11 = false;
                            item.Hide12 = false;
                            item.Hide13 = false;
                            item.Hide14 = false;
                            item.Hide15 = false;
                        }
                        else if (orderedansweredlist.Count == 5)
                        {
                            item.Value0 = orderedansweredlist[0].Label;
                            item.Value1 = orderedansweredlist[1].Label;
                            item.Value2 = orderedansweredlist[2].Label;
                            item.Value3 = orderedansweredlist[3].Label;
                            item.Value4 = orderedansweredlist[4].Label;


                            item.Hide3 = true;
                            item.Hide4 = true;
                            item.Hide5 = true;
                            item.Hide6 = false;
                            item.Hide7 = false;
                            item.Hide8 = false;

                            item.Hide9 = false;
                            item.Hide10 = false;
                            item.Hide11 = false;
                            item.Hide12 = false;
                            item.Hide13 = false;
                            item.Hide14 = false;
                            item.Hide15 = false;
                        }
                        else if (orderedansweredlist.Count == 8)
                        {
                            item.Value0 = orderedansweredlist[0].Label;
                            item.Value1 = orderedansweredlist[1].Label;
                            item.Value2 = orderedansweredlist[2].Label;
                            item.Value3 = orderedansweredlist[3].Label;
                            item.Value4 = orderedansweredlist[4].Label;
                            item.Value5 = orderedansweredlist[5].Label;
                            item.Value6 = orderedansweredlist[6].Label;
                            item.Value7 = orderedansweredlist[7].Label;

                            item.Hide3 = true;
                            item.Hide4 = true;
                            item.Hide5 = true;
                            item.Hide6 = true;
                            item.Hide7 = true;
                            item.Hide8 = true;

                            item.Hide9 = false;
                            item.Hide10 = false;
                            item.Hide11 = false;
                            item.Hide12 = false;
                            item.Hide13 = false;
                            item.Hide14 = false;
                            item.Hide15 = false;
                        }
                        else if (orderedansweredlist.Count == 15)
                        {
                            item.Value0 = orderedansweredlist[0].Label;
                            item.Value1 = orderedansweredlist[1].Label;
                            item.Value2 = orderedansweredlist[2].Label;
                            item.Value3 = orderedansweredlist[3].Label;
                            item.Value4 = orderedansweredlist[4].Label;
                            item.Value5 = orderedansweredlist[5].Label;
                            item.Value6 = orderedansweredlist[6].Label;
                            item.Value7 = orderedansweredlist[7].Label;
                            item.Value8 = orderedansweredlist[8].Label;
                            item.Value9 = orderedansweredlist[9].Label;
                            item.Value10 = orderedansweredlist[10].Label;
                            item.Value11 = orderedansweredlist[11].Label;
                            item.Value12 = orderedansweredlist[12].Label;
                            item.Value13 = orderedansweredlist[13].Label;
                            item.Value14 = orderedansweredlist[14].Label;

                            item.Hide3 = true;
                            item.Hide4 = true;
                            item.Hide5 = true;
                            item.Hide6 = true;
                            item.Hide7 = true;
                            item.Hide8 = true;
                            item.Hide9 = true;
                            item.Hide10 = true;
                            item.Hide11 = true;
                            item.Hide12 = true;
                            item.Hide13 = true;
                            item.Hide14 = true;
                            item.Hide15 = true;
                        }





                    }



                }

                var questionsordered = questions.OrderBy(x => x.Questionorder);

                foreach (var item in questionsordered)
                {
                    //add the row num
                    item.Rownum = itemcount++.ToString();

                    if (itemcount == 14 || itemcount == 15)
                    {
                        item.Activeop = 0.3;
                        item.Isquestionactive = false;
                    }
                    else
                    {
                        item.Activeop = 1;
                        item.Isquestionactive = true;
                    }

                    item.Question = item.Rownum.ToString() + ". " + item.Question;
                }



                //order the question
                questionnairelist.ItemsSource = questionsordered;


                // getconsentdetailsonload();
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message.ToString(), "OK");
                DisplayAlert("Error", ex.StackTrace.ToString(), "OK");
            }
        }

        async void getconsentdetailsonload()
        {
            try
            {
                //over 18
                ConsentDetails = await consentmanager.getconsentregover18(newuser.Signupid);

                //under 18
                ConsentDetailsUnder18 = await consentmanager.getconsentregunder18(newuser.Signupid);


            }
            catch (Exception ex)
            {

            }
        }

        async void getconsentdetails()
        {
            try
            {


                //check for over and under 18
                var age = newuser.Loweragebracket + 5;


                if (age < 18)
                {
                    //under 18
                    ConsentDetails = await consentmanager.getconsentregunder18(newuser.Signupid);



                    childname.IsVisible = true;
                    parentname.IsVisible = true;
                    parentlbl.IsVisible = true;
                    childlbl.IsVisible = true;

                    under18 = true;


                }
                else
                {
                    //over 18
                    ConsentDetails = await consentmanager.getconsentregover18(newuser.Signupid);

                    childname.IsVisible = false;
                    parentname.IsVisible = false;
                    parentlbl.IsVisible = false;
                    childlbl.IsVisible = false;

                    under18 = false;
                }


                if (ConsentDetails.Count > 0)
                {
                    titlelbl.Text = ConsentDetails[0].ConsentTitle;
                    subtitle.Text = ConsentDetails[0].ConsentSubTitle;

                    selectedconsentlist.Clear();

                    var content = ConsentDetails[0].ConsentContent;

                    var splitcontent = content.Split('|');

                    List<string> stringList = new List<string>(splitcontent);

                    consentdetailslist = stringList;

                    consentlist.ItemsSource = consentdetailslist;

                    //get the info button details

                    if (newuser.Changesignupid == "IID32")
                    {
                        //get the IID32 study

                        var pdfcontent = await meddirectionsmanager.getinfobyreferralcodeonreg("IID32");

                        if (pdfcontent.Count > 1)
                        {
                            var findwhichonehaspdf = pdfcontent.FirstOrDefault(x => !string.IsNullOrEmpty(x.Filename));

                            PDFinfostring = findwhichonehaspdf.Filename;
                        }
                        else
                        {

                            PDFinfostring = pdfcontent[0].Filename;
                        }


                        getquestionsandanswers();

                    }
                    else
                    {
                        //get the IID3 study
                        var pdfcontent = await meddirectionsmanager.getinfobyreferralcodeonreg("IID3");

                        if (pdfcontent.Count > 1)
                        {
                            var findwhichonehaspdf = pdfcontent.FirstOrDefault(x => !string.IsNullOrEmpty(x.Filename));

                            PDFinfostring = findwhichonehaspdf.Filename;
                        }
                        else
                        {

                            PDFinfostring = pdfcontent[0].Filename;
                        }
                    }

                }



            }
            catch (Exception ex)
            {

            }
        }

        async void getquestionsandanswers()
        {
            try
            {
                iid32questions = await questionamanager.getQuestions("425A4FE9-079D-48B2-8922-1DB6AC00FBE4");

                var getallanswers = await answermanager.getAllAnswerss();

                foreach (var item in iid32questions)
                {
                    var getanswers = getallanswers.Where(x => x.Questionid == item.Id);

                    foreach (var i in getanswers)
                    {
                        iid32answers.Add(i);
                    }
                }

                var iid32 = true;
            }
            catch (Exception ex)
            {
                var s = ex.StackTrace.ToString();
                var ss = "sds";
            }
        }


        void emailtxt_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            try
            {
                emailreghint.HasError = false;

            }
            catch (Exception ex)
            {

            }
        }

        void passtxt_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            try
            {
                passreghint.HasError = false;
            }
            catch (Exception ex)
            {

            }
        }

        void tccheckbox_CheckedChanged(System.Object sender, Microsoft.Maui.Controls.CheckedChangedEventArgs e)
        {
            try
            {
                tcerror.IsVisible = false;
            }
            catch (Exception ex)
            {

            }
        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            //back button tapped
            try
            {



                //check if it is the first page
                if (numberforfeatures == 0)
                {
                    Navigation.RemovePage(this);
                    return;
                }

                if (dobframe.IsVisible == true)
                {
                    dobframe.IsVisible = false;
                    createframe.IsVisible = true;

                    numberforfeatures--;

                    topprogress.Progress -= progressamount;

                    return;
                }

                else if (emisconsentframe.IsVisible == true)
                {
                    emisconsentframe.IsVisible = false;
                    dobframe.IsVisible = true;

                    numberforfeatures--;

                    topprogress.Progress -= progressamount;

                    return;
                }

                else if (emisconsentframe2.IsVisible == true)
                {
                    emisconsentframe2.IsVisible = false;
                    emisconsentframe.IsVisible = true;

                    numberforfeatures--;

                    topprogress.Progress -= progressamount;

                    return;
                }

                else if (emisconsentframe3.IsVisible == true)
                {

                    emisconsentframe3.IsVisible = false;
                    emisconsentframe2.IsVisible = true;

                    numberforfeatures--;

                    topprogress.Progress -= progressamount;

                    return;
                }
                else if (genframe.IsVisible == true)
                {
                    genframe.IsVisible = false;
                    emisconsentframe3.IsVisible = true;

                    numberforfeatures--;

                    topprogress.Progress -= progressamount;

                    return;

                }
                else if (heightframe.IsVisible == true)
                {
                    heightframe.IsVisible = false;
                    genframe.IsVisible = true;

                    numberforfeatures--;

                    topprogress.Progress -= progressamount;

                    return;
                }
                else if (weightframe.IsVisible == true)
                {
                    weightframe.IsVisible = false;
                    heightframe.IsVisible = true;

                    numberforfeatures--;

                    topprogress.Progress -= progressamount;

                    return;
                }
                else if (ethframe.IsVisible == true)
                {
                    ethframe.IsVisible = false;

                    if (orderedFeatureList.Count == 8)
                    {
                        genframe.IsVisible = true;
                    }
                    else
                    {
                        weightframe.IsVisible = true;
                    }

                    numberforfeatures--;

                    topprogress.Progress -= progressamount;

                    return;
                }
                else if (questionsframe.IsVisible == true)
                {
                    questionsframe.IsVisible = false;

                    if (orderedFeatureList.Count == 6)
                    {
                        emisconsentframe3.IsVisible = true;
                    }
                    else
                    {
                        ethframe.IsVisible = true;
                    }



                    numberforfeatures--;

                    topprogress.Progress -= progressamount;

                    btnmain.Text = "Next";

                    return;
                }


                var getfeature = orderedFeatureList[numberforfeatures - 1];




                foreach (var frame in Content.GetVisualTreeDescendants().OfType<Frame>())
                {
                    frame.IsVisible = false;
                }


                // var getfeature = orderedFeatureList[numberforfeatures - 1];


                var nameofstack = getfeature.Featurestackname;

                btnmain.Text = "Next";

                Frame stackLayout = this.FindByName<Frame>(nameofstack);
                Button Findskipbutton = this.FindByName<Button>("btnskip");


                //for listviews within frame you need to do it this way to get the list to show


                if (stackLayout != null)
                {
                    // Access the stackLayout as needed
                    stackLayout.IsVisible = true;

                    //check if the part is required or not

                    //if not we can show the skip button

                    if (getfeature.Isrequired == false)
                    {
                        Findskipbutton.IsVisible = true;
                    }
                    else
                    {
                        Findskipbutton.IsVisible = false;
                    }

                }

                numberforfeatures--;

                topprogress.Progress -= progressamount;
            }
            catch (Exception ex)
            {

            }
        }

        void heightlist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                //height list tapped

                var item = e.DataItem as Customheightandweight;




                if (heightunitvalue == "cm")
                {
                    heightlabel.Text = item.Valuenumber + " cm";
                }
                else
                {
                    //ft and inches

                    //check for whole numbers at add in ft

                    if (!item.Valuenumber.Contains("ft"))
                    {
                        heightlabel.Text = item.Valuenumber + " ft 0 in";
                    }
                    else
                    {
                        heightlabel.Text = item.Valuenumber;
                    }
                }


            }
            catch (Exception ex)
            {

            }
        }

        void segmentedcontrolheight_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as string;
                heightlabel.Text = " ";

                if (item == "cm")
                {
                    heightunitvalue = "cm";
                    // weightunittxt.Text = "kg";
                    heightlist.ItemsSource = heightcmlist;

                }
                else
                {
                    heightunitvalue = "ft in";
                    // weightunittxt.Text = "Stones & Pounds";
                    heightlist.ItemsSource = heightftlist;


                }
            }
            catch (Exception ex)
            {

            }
        }

        void genderlist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as string;
                newuser.Gender = item;
            }
            catch (Exception ex)
            {

            }
        }

        static Regex ValidEmailRegex = CreateValidEmailRegex();

        /// <summary>
        /// Regex for a valid email
        /// </summary>
        /// <returns>The valid email regex.</returns>
        private static Regex CreateValidEmailRegex()
        {

            string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return new Regex(validEmailPattern, RegexOptions.IgnoreCase);
        }

        internal static bool EmailIsValid(string emailAddress)
        {
            bool isValid = ValidEmailRegex.IsMatch(emailAddress);

            return isValid;
        }


        async void btnmain_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                //next button


                //check any validation needed on the area

                if (createframe.IsVisible == true)
                {

                    //check email address

                    if (string.IsNullOrEmpty(emailregtxt.Text))
                    {
                        emailreghint.HasError = true;
                        emailreghint.ErrorText = "Please enter an email address";
                        Vibration.Vibrate();
                        emailregtxt.Focus();
                        return;
                    }

                    if (emailregtxt.Text.EndsWith(" "))
                    {
                        string trimmed = emailregtxt.Text.Trim();
                        emailregtxt.Text = trimmed;
                    }

                    if (emailregtxt.Text.StartsWith(" "))
                    {
                        string trimmed = emailregtxt.Text.TrimStart();
                        emailregtxt.Text = trimmed;
                    }

                    //check if its a valid email
                    if (EmailIsValid(emailregtxt.Text) == false)
                    {
                        //emailreghint.ErrorColor = Colors.Red;
                        emailreghint.HasError = true;
                        emailreghint.ErrorText = "Please enter a valid email address";
                        Vibration.Vibrate();
                        emailregtxt.Focus();
                        return;
                    }



                    // password check
                    if (string.IsNullOrEmpty(passregtxt.Text))
                    {
                        passreghint.HasError = true;
                        passreghint.ErrorText = "Please enter an password";
                        Vibration.Vibrate();
                        passregtxt.Focus();
                        return;
                    }





                    //check password validation
                    if (passregtxt.Text.Length < 8)
                    {

                        // BusyIndicator.IsRunning = false;
                        passregtxt.Focus();
                        passreghint.HasError = true;
                        passreghint.ErrorText = "Password must be greater than 8 characters";
                        Vibration.Vibrate();
                        //  btnAddtwo.IsBusy = false;
                        return;
                    }


                    var hasNumber = new Regex(@"[0-9]+");
                    var hasUpperChar = new Regex(@"[A-Z]+");
                    var hasMiniMaxChars = new Regex(@".{8,15}");
                    var hasLowerChar = new Regex(@"[a-z]+");
                    var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?-]");


                    if (!hasUpperChar.IsMatch(passregtxt.Text))
                    {
                        passregtxt.Focus();
                        passreghint.HasError = true;
                        passreghint.ErrorText = "Password should contain at least one upper case letter.";
                        Vibration.Vibrate();
                        // passworderrorlbl.IsVisible = true;
                        //  passworderrorlbl.Text = "Password should contain at least one upper case letter.";
                        return;
                    }

                    else if (!hasNumber.IsMatch(passregtxt.Text))
                    {
                        passregtxt.Focus();
                        passreghint.HasError = true;
                        passreghint.ErrorText = "Password should contain at least one numeric value.";
                        Vibration.Vibrate();
                        //passworderrorlbl.IsVisible = true;
                        //passworderrorlbl.Text = "Password should contain at least one numeric value.";
                        return;
                    }


                    else if (!hasSymbols.IsMatch(passregtxt.Text))
                    {
                        passregtxt.Focus();
                        passreghint.HasError = true;
                        passreghint.ErrorText = "Password should contain at least one symbol.";
                        Vibration.Vibrate();
                        //passworderrorlbl.IsVisible = true;
                        //passworderrorlbl.Text = "Password should contain at least one numeric value.";
                        return;
                    }


                    //terms and conditions check
                    if (tccheckbox.IsChecked == false)
                    {
                        tcerror.IsVisible = true;
                        tcerror.Text = "Terms and Conditions must be accepted";
                        Vibration.Vibrate();
                        return;
                    }



                    //check if the user is already in the system
                    emailregtxt.Text = emailregtxt.Text.TrimEnd();

                    string username = emailregtxt.Text.ToLower();

                    var LogginedInUsers = await usermanager.checkUser(username);

                    if (LogginedInUsers.Count > 0)
                    {
                        //email already exists for existing user
                        emailreghint.HasError = true;
                        emailreghint.ErrorText = "Email address already in use.";
                        Vibration.Vibrate();
                        emailregtxt.Focus();
                        return;
                    }



                    //add the email and password to the new user
                    newuser.Email = emailregtxt.Text;

                    ///Converts password input to text and then hashes using MD5
                    Byte[] BytesHashed = usermanager.GetHash(passregtxt.Text);
                    newuser.Password = usermanager.ByteArrayToHex(BytesHashed);


                    if (emailcheck.IsChecked)
                    {
                        newuser.EmailNotifications = true;
                    }
                    else
                    {
                        newuser.EmailNotifications = false;
                    }


                }
                else if (emisconsentframe.IsVisible == true)
                {
                    //nothing needed
                    emisconsentframe.IsVisible = false;
                    emisconsentframe2.IsVisible = true;
                    topprogress.Progress += progressamount;
                    numberforfeatures++;
                    return;

                }
                else if (emisconsentframe2.IsVisible == true)
                {


                    //if (checkbox1.IsChecked == true && checkbox2.IsChecked == true && checkbox3.IsChecked == true && checkbox4.IsChecked == true && checkbox5.IsChecked == true && checkbox6.IsChecked == true && checkbox7.IsChecked == true)
                    //{
                    //    // all selected 
                    //}
                    //else
                    //{
                    //    Vibration.Vibrate();
                    //    return;
                    //}

                    //check all items in the listview has been selected


                    if (selectedconsentlist.Count == consentdetailslist.Count)
                    {
                        //all selected ok
                    }
                    else
                    {

                        // Check if all items without 'Optional' are selected
                        var allNonOptionalSelected = selectedconsentlist.Where(item => !item.Contains("(Optional)")).ToList();


                        if (allNonOptionalSelected.Count >= consentdetailslist.Count - 3)
                        {

                            //means they have left the optional one blank and can go on
                            emisconsentframe2.IsVisible = false;
                            emisconsentframe3.IsVisible = true;
                            topprogress.Progress += progressamount;
                            numberforfeatures++;
                            return;


                        }


                        await DisplayAlert("Attention", "It looks like you missed some. Please review and agree to all before proceeding", "OK");
                        return;
                    }

                    emisconsentframe2.IsVisible = false;
                    emisconsentframe3.IsVisible = true;
                    topprogress.Progress += progressamount;
                    numberforfeatures++;

                    return;

                }
                else if (emisconsentframe3.IsVisible == true)
                {
                    //add the signature to the db

                    //signpad.Save();

                    if (under18 == true)
                    {
                        if (string.IsNullOrEmpty(nameofchildentry.Text))
                        {
                            await DisplayAlert("Child Name Missing", "Please fill out the child's name", "OK");
                            return;
                        }

                        if (string.IsNullOrEmpty(nameofparententry.Text))
                        {
                            await DisplayAlert("Parent Name Missing", "Please fill out the parents name", "OK");
                            return;
                        }
                    }


                    if (signpad == null)
                    {
                        await DisplayAlert("Signature Missing", "Please sign the signature pad", "OK");
                        return;
                    }
                    else
                    {
                        // all signed ok
                        hassigned = true;
                    }

                    btnmain.IsEnabled = false;
                    emisconsentframe3.IsVisible = false;

                    if (orderedFeatureList.Count == 6)
                    {
                        questionsframe.IsVisible = true;


                        if (newuser.Changesignupid == "IID32")
                        {
                            questionnairelbl.Text = "Symptom Questionnaire";
                            btnmain.Text = "Next";
                        }
                        else
                        {
                            questionnairelbl.Text = "Baseline Questionnaire";
                            btnmain.Text = "Finish";
                        }
                    }
                    else
                    {
                        genframe.IsVisible = true;
                    }

                    btnmain.IsEnabled = true;
                    topprogress.Progress += progressamount;
                    numberforfeatures++;
                    return;

                }
                else if (genframe.IsVisible == true)
                {
                    if (string.IsNullOrEmpty(newuser.Gender))
                    {
                        Vibration.Vibrate();
                        return;
                    }
                    else
                    {
                        //gender added when user taps on list 
                    }


                    genframe.IsVisible = false;
                    //  heightframe.IsVisible = true;
                    topprogress.Progress += progressamount;
                    numberforfeatures++;
                    //  btnskip.IsVisible = true;

                    if (orderedFeatureList.Count == 8)
                    {
                        ethframe.IsVisible = true;
                    }
                    else
                    {
                        heightframe.IsVisible = true;
                    }


                    return;

                }

                else if (heightframe.IsVisible == true)
                {
                    //for iid3 height and weight are not required so skip button is visible

                    if (!string.IsNullOrWhiteSpace(heightlabel.Text))
                    {
                        newuser.Height = heightlabel.Text;
                    }

                    heightframe.IsVisible = false;
                    weightframe.IsVisible = true;
                    topprogress.Progress += progressamount;
                    numberforfeatures++;
                    btnskip.IsVisible = true;
                    return;

                }

                else if (weightframe.IsVisible == true)
                {
                    //for iid3 height and weight are not required so skip button is visible

                    if (!string.IsNullOrWhiteSpace(weightlabel.Text))
                    {
                        newuser.Weight = weightlabel.Text;
                    }

                    weightframe.IsVisible = false;
                    ethframe.IsVisible = true;
                    topprogress.Progress += progressamount;
                    btnskip.IsVisible = false;
                    numberforfeatures++;
                    return;
                }
                else if (dobframe.IsVisible == true)
                {
                    //check if the date isnt today so they have selected a date
                    if (daypicker.SelectedDate >= DateTime.Now.Date)
                    {
                        Vibration.Vibrate();
                        return;
                    }

                    //add user age
                   // newuser.Age = daypicker.SelectedDate.ToString("dd/MM/yyyy");

                    var convertage = DateTime.Parse(daypicker.SelectedDate.ToString());
                    int userAge = usermanager.CalculateUserAge(convertage);

                    newuser.Loweragebracket = userAge - 5;

                    if (newuser.Loweragebracket <= 0)
                    {
                        newuser.Loweragebracket = 0;

                    }
                    newuser.Upperagebracket = userAge + 5;



                    //get consent details
                    getconsentdetails();




                    dobframe.IsVisible = false;
                    emisconsentframe.IsVisible = true;
                    topprogress.Progress += progressamount;
                    numberforfeatures++;
                    return;


                }

                else if (ethframe.IsVisible == true)
                {

                    if (string.IsNullOrEmpty(newuser.Ethnicity))
                    {
                        Vibration.Vibrate();
                        return;
                    }
                    else
                    {
                        //ethnicity added when the user taps the list
                    }

                    ethframe.IsVisible = false;
                    questionsframe.IsVisible = true;

                    if (newuser.Changesignupid == "IID32")
                    {
                        questionnairelbl.Text = "Symptom Questionnaire";
                    }
                    else
                    {
                        questionnairelbl.Text = "Baseline Questionnaire";
                    }

                    topprogress.Progress += progressamount;
                    numberforfeatures++;
                    btnmain.Text = "Finish";
                    return;

                }
                //check if the questions are completed
                else if (questionsframe.IsVisible == true)
                {
                    var newquestioncount = 0;

                    //check if the completed question count is 11 , if they have 11 and have selected self employyed to number 10 then they only need 11

                    if (completedquestions.Any(x => x.Question.Contains("12.")))
                    {

                        //find the response for question number 10

                        var num10question = completedquestions.Where(x => x.Question.Contains("12.")).FirstOrDefault();

                        if (num10question == null)
                        {
                            //they havent answered it
                        }
                        else
                        {
                            if (num10question.Answerid.Contains("Please skip"))
                            {

                                //check if there is any of the questions that do not need to answered included in the completed questions
                                var num11 = completedquestions.Where(x => x.Question.Contains("13")).FirstOrDefault();

                                if (num11 != null)
                                {
                                    completedquestions.Remove(num11);
                                }

                                var num12 = completedquestions.Where(x => x.Question.Contains("14")).FirstOrDefault();

                                if (num12 != null)
                                {
                                    completedquestions.Remove(num12);
                                }


                                newquestioncount = 13;
                            }
                            else
                            {
                                newquestioncount = 15;
                            }
                        }

                    }
                    else
                    {
                        newquestioncount = 15;
                    }


                    //check if all questions are completed

                    if (completedquestions.Count() != newquestioncount)
                    {
                        //Vibration.Vibrate();
                        await DisplayAlert("Incomplete Questionnaire", "Please complete all questions", "OK");
                        return;
                    }
                    else
                    {
                        //get the users first name here

                        //find the name question
                        var findfirstquestion = completedquestions.Where(x => x.Question.Contains("your Name")).FirstOrDefault();

                        var firstquestion = findfirstquestion.Entryanswer;

                        if (firstquestion.Contains(" "))
                        {
                            //split answer
                            var splitname = firstquestion.Split(' ');
                            newuser.FirstName = splitname[0];
                            newuser.Surname = splitname[1];
                        }
                        else
                        {
                            newuser.FirstName = firstquestion;
                        }


                        //find the gender
                        var findgenderquestion = completedquestions.Where(x => x.Question.Contains("Are you")).FirstOrDefault();


                        if (findgenderquestion.Answerid == "Prefer to self-describe")
                        {
                            newuser.Gender = findgenderquestion.Entryanswer;
                        }
                        else
                        {
                            newuser.Gender = findgenderquestion.Answerid;
                        }



                        //find the ethnicity
                        var findethquestion = completedquestions.Where(x => x.Question.Contains("ethnic group")).FirstOrDefault();

                        newuser.Ethnicity = findethquestion.Answerid;

                        //find the phone number
                        var findphonenumquestion = completedquestions.Where(x => x.Question.Contains("telephone number")).FirstOrDefault();

                        newuser.PhoneNumber = findphonenumquestion.Entryanswer;

                        //find the address
                        var findaddressquestion = completedquestions.Where(x => x.Question.Contains("Address")).FirstOrDefault();

                        newuser.AddressLineOne = findaddressquestion.Entryanswer;

                        //remove the questions from the completed questions as they do not need to go into the user inital reponses table
                        if (newuser.Changesignupid == "IID32")
                        {

                        }
                        else
                        {
                            completedquestions.Remove(findgenderquestion);
                            completedquestions.Remove(findethquestion);
                        }
                    }


                    if (newuser.Changesignupid == "IID32")
                    {
                        bool iid32 = true;

                        questionsframe.IsVisible = false;
                        topgrid.IsVisible = false;
                        btnmain.IsVisible = false;
                        loadingquestionsframe.IsVisible = true;


                        var newuserquestionnaireid = new UserQuestionnaire();

                        //  await Navigation.PushAsync(new QuestionnairePageGP(iid32questions, iid32answers, newuserquestionnaireid, iid32, completedquestions, questionanswers, newuser, signpad, ConsentDetails, under18, nameofchildentry.Text, nameofparententry.Text, consentdetailslist, selectedconsentlist), false);

                        await Navigation.PushAsync(new QuestionnairePageHardCoded32(iid32questions, iid32answers, newuserquestionnaireid, iid32, completedquestions, questionanswers, newuser, signpad, ConsentDetails, under18, nameofchildentry.Text, nameofparententry.Text, consentdetailslist, selectedconsentlist), false);

                        questionsframe.IsVisible = true;
                        loadingquestionsframe.IsVisible = false;
                        topgrid.IsVisible = true;
                        btnmain.IsVisible = true;

                        // Xamarin.Forms.Application.Current.MainPage = new Xamarin.Forms.NavigationPage(new QuestionnairePageGP(iid32questions, iid32answers, newuserquestionnaireid, iid32));
                        return;
                    }
                }


                if (btnmain.Text == "Finish")
                {
                    if (newuser.Changesignupid == "IID32")
                    {
                        bool iid32 = true;

                        var newuserquestionnaireid = new UserQuestionnaire();

                        // await Navigation.PushAsync(new QuestionnairePageGP(iid32questions, iid32answers, newuserquestionnaireid, iid32, completedquestions, newuser), false);

                        // Xamarin.Forms.Application.Current.MainPage = new Xamarin.Forms.NavigationPage(new QuestionnairePageGP(iid32questions, iid32answers, newuserquestionnaireid, iid32));
                        return;
                    }

                    //so the user cant keep tapping the button
                    btnmain.IsEnabled = false;

                    newuser.Make = DeviceInfo.Platform.ToString();
                    newuser.Model = DeviceInfo.Model.ToString();
                    newuser.RegStatus = "Active";
                    newuser.Clinicalinfo = true;


                    //update the created at date

                    var cdate = DateTimeOffset.Now;

                    // Specify the desired format using custom format specifier
                    string formattedDate = cdate.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");

                    // Specify the format using custom format specifier
                    string format = "yyyy-MM-dd HH:mm:ss.fff zzz";

                    // Parse the formatted string into a DateTimeOffset object
                    DateTimeOffset parsedDate = DateTimeOffset.Parse(formattedDate);

                    //added now by the notification select time
                    // newuser.ActivationDT = formattedDate.ToString();


                    //check if you new to revert back to the orginal iid32 sign up code
                    if (!string.IsNullOrEmpty(newuser.Changesignupid))
                    {
                        newuser.Signupid = newuser.Changesignupid;
                        newuser.ActivationDT = formattedDate.ToString();
                    }



                        //add the value to the dashboard column to allow the consent to appear in the dash

                        newuser.Dashboard = "True";
                    newuser.Referral = "IID3";

                    //add the user to the db
                    await usermanager.AddUser(newuser);

                    //update the helpers
                    Preferences.Set("id", newuser.Id);
                    Preferences.Set("usertitle", newuser.Title);
                    Preferences.Set("firstname", newuser.FirstName);
                    Preferences.Set("surname", newuser.Surname);
                    Preferences.Set("gender", newuser.Gender);
                    Preferences.Set("email", newuser.Email);
                    Preferences.Set("password", newuser.Password);
                    // var agecal = newuser.Loweragebracket + 5;
                    Preferences.Set("age", newuser.Age);
                    Preferences.Set("ethnicity", newuser.Ethnicity);
                    Preferences.Set("addresslineone", newuser.AddressLineOne);
                    Preferences.Set("addresslinetwo", newuser.AddressLineTwo);
                    Preferences.Set("town", newuser.AddressLineOne);
                    Preferences.Set("city", newuser.City);
                    Preferences.Set("postcode", newuser.Postcode);
                    Preferences.Set("phonenumber", newuser.PhoneNumber);
                    string lowerage = newuser.Loweragebracket.ToString();
                    string upperage = newuser.Upperagebracket.ToString();
                    Preferences.Set("loweragekey", lowerage);
                    Preferences.Set("upperagekey", upperage);
                    Preferences.Set("userpasswordhash", newuser.Password);
                    Preferences.Set("rotationsetting", "On");
                    Preferences.Set("height", newuser.Height);
                    Preferences.Set("weight", newuser.Weight);

                    if (!string.IsNullOrEmpty(newuser.Changesignupid))
                    {
                        Preferences.Set("signupcode", newuser.Changesignupid);
                    }
                    else
                    {
                        Preferences.Set("signupcode", newuser.Signupid);
                    }

                    Preferences.Set("dashsettings", newuser.Dashboard);
                    Preferences.Set("appusing", newuser.Role);
                    Preferences.Set("clinicaltrial", "Yes");
                    // Preferences.Set("createdat", newuser.ActivationDT.ToString());

                    Preferences.Set("usergpid", newuser.Gpid);

                    //add the activation date to this helper so you can determine the date the user signed up
                    Preferences.Set("createdatdateonly", formattedDate.ToString());
                    Preferences.Set("androidnotekey", "firstreg");
                    //if (newuser.Clinicaltrails == true)
                    //{
                    //    Preferences.Set("clinicaltrial", "Yes");
                    //}
                    //else
                    //{
                    //    Preferences.Set("clinicaltrial", "No");
                    //}

                    Analytics.TrackEvent("Research App - Registration Signup Button Complete");


                    //check if they any additional questions that need to be added to the db

                    if (completedquestions.Count > 0)
                    {


                        foreach (var item in completedquestions)
                        {

                            var insertnewuserquestionnaire = new userInitialQuestionsResponses();

                            insertnewuserquestionnaire.Userid = Helpers.Settings.UserKey;
                            insertnewuserquestionnaire.DateComplete = DateTime.Now.ToString("dd/MM/yyyy");
                            insertnewuserquestionnaire.TimeComplete = DateTime.Now.ToString("HH:mm");
                            insertnewuserquestionnaire.Active = true;
                            insertnewuserquestionnaire.Questionid = item.Id;

                            if (item.Type == "typeSS")
                            {
                                //single selection
                                //get the right answer id

                                var getanswerid = questionanswers.Where(x => x.Questionid == item.Id).ToList();

                                var gettherightanswer = getanswerid.Where(x => x.Label == item.Answerid).FirstOrDefault();

                                insertnewuserquestionnaire.Answerid = gettherightanswer.Id;
                            }
                            else
                            {
                                //numeric and text entry
                                insertnewuserquestionnaire.Answervalue = item.Entryanswer;
                            }

                            //insert into db
                            await userinitialquestionresponsesmanager.Addresponse(insertnewuserquestionnaire);

                        }
                    }


                    //get the user consent
                    getuserconsent();


                    //check if they have used the signature pad and upload the image
                    if (hassigned == true)
                    {
                        //upload the signature
                        Addconsentandsign();
                    }

                    //show the animation page and dashboard
                    await welcomepage();


                }
                else
                {
                    //go to the next part

                    numberforfeatures++;

                    shownextfeature(numberforfeatures);
                }


            }
            catch (Exception ex)
            {

            }
        }

        async Task welcomepage()
        {

            if (Helpers.Settings.SignUp == "IID32")
            {
                Application.Current.MainPage = new NavigationPage(new NotificationQuestionGP());
                return;
            }
            else
            {


                Application.Current.MainPage = new NavigationPage(new WelcomePage());
                return;
            }
        }

        public async Task Addconsentandsign()
        {
            try
            {
                string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=peoplewithappiamges;AccountKey=9maBMGnjWp6KfOnOuXWHqveV4LPKyOnlCgtkiKQOeA+d+cr/trKApvPTdQ+piyQJlicOE6dpeAWA56uD39YJhg==;EndpointSuffix=core.windows.net";

                var backrandom = new Random();
                var backrandomnum = backrandom.Next(1000, 10000000);
                var backimagename = Helpers.Settings.UserKey + "-" + DateTime.Now.ToString("HHmmssfff") + "-" + backrandomnum + ".jpg";

                // Parse the connection string and create a blob client
                BlobServiceClient blobServiceClient = new BlobServiceClient(StorageConnectionString);

                // Get a reference to the container
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("consentsignatures");

                // Get a reference to the blob
                BlobClient blobClient = containerClient.GetBlobClient(backimagename);

                //// Get the signature as a Stream
                Stream signatureStream = await signpad.GetImageStreamAsync(SignatureImageFormat.Png);

                // Upload the signature image stream to Azure Blob Storage
                //await blobClient.UploadAsync(signatureStream);
                if (signatureStream != null)
                {
                    await blobClient.UploadAsync(signatureStream);
                }

                //old code below

                //var backrandom = new Random();
                //var backrandomnum = backrandom.Next(1000, 10000000);

                //var backimagename = Helpers.Settings.UserKey + "-" + DateTime.Now.TimeOfDay.ToString() + "-" + backrandomnum + ".jpg";

                //CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=peoplewithappiamges;AccountKey=9maBMGnjWp6KfOnOuXWHqveV4LPKyOnlCgtkiKQOeA+d+cr/trKApvPTdQ+piyQJlicOE6dpeAWA56uD39YJhg==;EndpointSuffix=core.windows.net");
                //var client = cloudStorageAccount.CreateCloudBlobClient();
                //CloudBlobContainer container = null;

                //container = client.GetContainerReference("consentsignatures");

                //// Get the signature as a Stream
                //Stream signatureStream = await signpad.GetImageStreamAsync(SignatureImageFormat.Png);


                ////back image
                //var backblob = container.GetBlockBlobReference(backimagename);
                //// blockblob.Properties.ContentType = photo.ContentType;


                //if (signatureStream != null)
                //{
                //    await backblob.UploadFromStreamAsync(signatureStream);
                //}


                //add the user consent to the table

                var uc = new userconsent();

             //   uc.Userid = Helpers.Settings.UserKey;
            //    uc.Advertid = newuser.Signupid;
             //   uc.Consented = true;
            //    uc.Area = "Registration";

                if (ConsentDetails.Count > 0)
                {
                   // uc.Consentid = ConsentDetails[0].Id;
                }

                if (under18 == true)
                {
                   // uc.Notes = nameofchildentry.Text + "|" + nameofparententry.Text;
                }


                //capture and store the consent form options in the db
                string[] consentresultstring = { };
                // Convert the array to a List
                List<string> consentList = new List<string>(consentresultstring);
                var itemcount = 0;

                foreach (var item in consentdetailslist)
                {
                    // Check if the consentDetail is in SelectedConsentDetails
                    bool isSelected = selectedconsentlist.Contains(item);


                    if (isSelected)
                    {
                        consentList.Add(itemcount.ToString() + ":1");
                    }
                    else
                    {
                        consentList.Add(itemcount.ToString() + ":0");
                    }

                    itemcount++;
                }

                // Convert the List to a single string
                string resultString = string.Join(",", consentList);

              //  uc.ConsentSelections = resultString;

                await userconsentmanger.adduserconsent(uc);
            }
            catch (Exception ex)
            {

            }
        }

        async void getuserconsent()
        {
            try
            {
                //check if the sign up needs consent from the user
                string signup = Helpers.Settings.SignUp;
                string area = "Registration";

                var checksignupcode = await advertmanager.GetSpecficAd(newuser.Signupid);

                if (checksignupcode.Count > 0)
                {
                    if (checksignupcode[0].Consent == true)
                    {
                        //needs consent
                        await Navigation.PushModalAsync(new SignUpConstent(signup, area), false);
                    }
                }



            }
            catch (Exception ex)
            {

            }
        }


        async void shownextfeature(int numforfeatures)
        {

            try
            {

                foreach (var frame in Content.GetVisualTreeDescendants().OfType<Frame>())
                {
                    frame.IsVisible = false;
                }


                var getfeature = orderedFeatureList[numforfeatures];


                var nameofstack = getfeature.Featurestackname;


                Frame stackLayout = this.FindByName<Frame>(nameofstack);
                Button Findskipbutton = this.FindByName<Button>("btnskip");



                if (stackLayout != null)
                {
                    // Access the stackLayout as needed
                    stackLayout.IsVisible = true;

                    //check if the part is required or not

                    //if not we can show the skip button

                    if (getfeature.Isrequired == false)
                    {
                        Findskipbutton.IsVisible = true;
                    }
                    else
                    {
                        Findskipbutton.IsVisible = false;
                    }

                }


                //check if its the last page
                Button Findnextbutton = this.FindByName<Button>("btnmain");

                if (numforfeatures == orderedFeatureList.Count - 1)
                {
                    Findnextbutton.Text = "Finish";
                }

                topprogress.Progress += progressamount;

            }
            catch (Exception ex)
            {

            }



        }

        void segmentedcontrolweight_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as string;
                weightlabel.Text = " ";

                if (item == "kg")
                {
                    weightunitvalue = "kg";
                    // weightunittxt.Text = "kg";
                    weightlist.ItemsSource = weightkglist;

                }
                else
                {
                    weightunitvalue = "st lbs";
                    // weightunittxt.Text = "Stones & Pounds";
                    weightlist.ItemsSource = weightstonelist;


                }
            }
            catch (Exception ex)
            {

            }
        }

        void weightlist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as Customheightandweight;


                if (weightunitvalue == "kg")
                {
                    weightlabel.Text = item.Valuenumber + " kg";
                }
                else
                {

                    weightlabel.Text = item.Valuenumber;

                }

            }
            catch (Exception ex)
            {

            }
        }


        void TapGestureRecognizer_Tapped_5(System.Object sender, System.EventArgs e)
        {
            //try
            //{
            //    if (checkbox1.IsChecked == true)
            //    {
            //        checkbox1.IsChecked = false;
            //    }
            //    else
            //    {
            //        checkbox1.IsChecked = true;
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }

        void TapGestureRecognizer_Tapped_6(System.Object sender, System.EventArgs e)
        {
            //try
            //{
            //    if (checkbox2.IsChecked == true)
            //    {
            //        checkbox2.IsChecked = false;
            //    }
            //    else
            //    {
            //        checkbox2.IsChecked = true;
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }

        void TapGestureRecognizer_Tapped_7(System.Object sender, System.EventArgs e)
        {
            //try
            //{
            //    if (checkbox3.IsChecked == true)
            //    {
            //        checkbox3.IsChecked = false;
            //    }
            //    else
            //    {
            //        checkbox3.IsChecked = true;
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }

        void TapGestureRecognizer_Tapped_8(System.Object sender, System.EventArgs e)
        {
            //try
            //{
            //    if (checkbox4.IsChecked == true)
            //    {
            //        checkbox4.IsChecked = false;
            //    }
            //    else
            //    {
            //        checkbox4.IsChecked = true;
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }

        void TapGestureRecognizer_Tapped_9(System.Object sender, System.EventArgs e)
        {
            //try
            //{
            //    if (checkbox5.IsChecked == true)
            //    {
            //        checkbox5.IsChecked = false;
            //    }
            //    else
            //    {
            //        checkbox5.IsChecked = true;
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }

        void TapGestureRecognizer_Tapped_10(System.Object sender, System.EventArgs e)
        {
            //try
            //{
            //    if (checkbox6.IsChecked == true)
            //    {
            //        checkbox6.IsChecked = false;
            //    }
            //    else
            //    {
            //        checkbox6.IsChecked = true;
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }

        void TapGestureRecognizer_Tapped_11(System.Object sender, System.EventArgs e)
        {
            //try
            //{
            //    if (checkbox7.IsChecked == true)
            //    {
            //        checkbox7.IsChecked = false;
            //    }
            //    else
            //    {
            //        checkbox7.IsChecked = true;
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }

        void TapGestureRecognizer_Tapped_12(System.Object sender, System.EventArgs e)
        {
            try
            {
                //hastappedonsignpad = true;
            }
            catch (Exception ex)
            {

            }
        }

        void ethnlist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as string;

                newuser.Ethnicity = item;
            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedEntry_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            //number entry additional questions

            try
            {

                var label = (ExtendedEntry)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


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

        void ExtendedRadioButton_StateChanged(System.Object sender, StateChangedEventArgs e)
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

                    getitem.Showadditionalgenderdropdown = false;


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


                    //check no10 question because some of the questions depend on number 10 answer
                    var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (checkifcontains.Question.Contains("12"))
                    {
                        //means they selected the self employed

                        foreach (var item in questions)
                        {
                            item.Isquestionactive = true;
                            item.Activeop = 1;
                        }

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

                if (e.IsChecked == false)
                {

                }
                else
                {
                    var label = (ExtendedRadioButton)sender;

                    var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();

                    getitem.Showadditionalgenderdropdown = false;

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

                    //check no10 question because some of the questions depend on number 10 answer
                    var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (checkifcontains.Question.Contains("12"))
                    {
                        //means they selected the self employed

                        foreach (var item in questions)
                        {
                            item.Isquestionactive = true;
                            item.Activeop = 1;
                        }

                    }



                }



            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedRadioButton_StateChanged_2(System.Object sender, StateChangedEventArgs e)
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

                    getitem.Showadditionalgenderdropdown = false;

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


                    //check no10 question because some of the questions depend on number 10 answer
                    var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (checkifcontains.Question.Contains("12"))
                    {
                        //find out which questions to update 

                        var question11 = questions.Where(x => x.Question.Contains("13")).FirstOrDefault();

                        question11.Activeop = 0.3;
                        question11.Isquestionactive = false;

                        var question12 = questions.Where(x => x.Question.Contains("14")).FirstOrDefault();

                        question12.Activeop = 0.3;
                        question12.Isquestionactive = false;

                    }


                }



            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedRadioButton_StateChanged_3(System.Object sender, StateChangedEventArgs e)
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

                    //check if it is the gender question and show the additional drop down for gender
                    var checkifcontains = completedquestions.Where(x => x.Id == getitem.Id).FirstOrDefault();

                    if (checkifcontains.Question.Contains("Are you"))
                    {
                        getitem.Showadditionalgenderdropdown = true;
                    }



                }



            }
            catch (Exception ex)
            {

            }
        }

        void ExtendedRadioButton_StateChanged_4(System.Object sender, StateChangedEventArgs e)
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

        void ExtendedRadioButton_StateChanged_5(System.Object sender, StateChangedEventArgs e)
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

        void ExtendedRadioButton_StateChanged_6(System.Object sender, StateChangedEventArgs e)
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

        void ExtendedRadioButton_StateChanged_7(System.Object sender, StateChangedEventArgs e)
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

        void ExtendedEntry_TextChanged_1(System.Object sender, TextChangedEventArgs e)
        {
            //text entry

            try
            {

                var label = (ExtendedEntry)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


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

        void consentlist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {

                var item = e.DataItem as string;


                if (selectedconsentlist.Contains(item))
                {
                    selectedconsentlist.Remove(item);
                }
                else
                {
                    selectedconsentlist.Add(item);
                }

            }
            catch (Exception ex)
            {

            }
        }

        async void TapGestureRecognizer_Tapped_1(System.Object sender, System.EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new PrivacyPolicyPage(), false);
            }
            catch (Exception ex)
            {

            }
        }

        void btnskip_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (heightframe.IsVisible == true)
                {
                    heightframe.IsVisible = false;
                    weightframe.IsVisible = true;
                    topprogress.Progress += progressamount;
                    numberforfeatures++;
                }
                else if (weightframe.IsVisible == true)
                {
                    weightframe.IsVisible = false;
                    ethframe.IsVisible = true;
                    btnskip.IsVisible = false;
                    topprogress.Progress += progressamount;
                    numberforfeatures++;
                }
            }
            catch (Exception ex)
            {

            }
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                var pdflink = "https://peoplewithappiamges.blob.core.windows.net/appimages/appimages/" + PDFinfostring;

                await Browser.OpenAsync(pdflink, new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Hide
                });


            }
            catch (Exception ex)
            {

            }
        }

        void genderentry_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            try
            {
                //gender entry

                var label = (ExtendedEntry)sender;

                var getitem = questions.Where(x => x.Id == label.IDValue).FirstOrDefault();


                getitem.Entryanswer = e.NewTextValue;


            }
            catch (Exception ex)
            {

            }
        }

        async void setupweeklynotifications()
        {
            try
            {

            }
            catch(Exception ex)
            {

            }
        }
    }
}

