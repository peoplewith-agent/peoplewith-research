using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace PeopleWithResearch
{
    public partial class RegisterWithDetailsPage : ContentPage
    {
        public ObservableCollection<advert> checksignupcodes = new ObservableCollection<advert>();
        public AdvertManager advertmanager;
        public user newuser = new user();
        public PrimaryCareManager primarycaremanager;
        public ObservableCollection<primarycare> emisgplist = new ObservableCollection<primarycare>();

        public ObservableCollection<initialQuestions> questions = new ObservableCollection<initialQuestions>();
        public ObservableCollection<initialQuestionsAnswers> questionanswers = new ObservableCollection<initialQuestionsAnswers>();
        public ObservableCollection<initialQuestions> completedquestions = new ObservableCollection<initialQuestions>();
        public ObservableCollection<features> FeaturesForReg = new ObservableCollection<features>();

        initialQuestionsAnswersManager initialquestionsanswersmanager;
        FeatureManager featuresmanager;
        initialQuestionsManager initialquestionsmanager;
        public double progressamount;
        public primarycare selectedgplocation;

        public RegisterWithDetailsPage()
        {
            InitializeComponent();

            advertmanager = AdvertManager.DefaultManager;
            primarycaremanager = PrimaryCareManager.DefaultManager;
            initialquestionsmanager = initialQuestionsManager.DefaultManager;
            initialquestionsanswersmanager = initialQuestionsAnswersManager.DefaultManager;
            featuresmanager = FeatureManager.DefaultManager;


           // Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            getdoctordetails();


            getinitialquestionandanswers();

            getfeatures();

        }

        public RegisterWithDetailsPage(string signupcodepassed)
        {
            InitializeComponent();

            advertmanager = AdvertManager.DefaultManager;
            primarycaremanager = PrimaryCareManager.DefaultManager;
            initialquestionsmanager = initialQuestionsManager.DefaultManager;
            initialquestionsanswersmanager = initialQuestionsAnswersManager.DefaultManager;
            featuresmanager = FeatureManager.DefaultManager;


          //  Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);


            signuptxt.Text = signupcodepassed;

            getdoctordetails();


            getinitialquestionandanswers();

            getfeatures();

        }

        async void getinitialquestionandanswers()
        {
            try
            {
                questions = await initialquestionsmanager.getinitialQuestions("IID3");

                if (questions.Count != 0)
                {
                    //get the answers for the questions
                    foreach (var item in questions)
                    {
                        var getanswers = await initialquestionsanswersmanager.getinitialAnswers(item.Id);


                        foreach (var it in getanswers)
                        {
                            questionanswers.Add(it);
                        }
                    }



                }
            }
            catch (Exception ex)
            {

            }
        }

        async void getfeatures()
        {
            try
            {
                FeaturesForReg = await featuresmanager.GetSpecficAd("IID3");

                //calulate the number the progress bar goes up or down by
                var totalnum = FeaturesForReg.Count() + 3;
                double num = (100 / totalnum);


                //set the progress amount
                progressamount = num;
                // topprogress.Progress += progressamount;
            }
            catch (Exception ex)
            {

            }
        }


        async void getdoctordetails()
        {
            try
            {


                emisgplist = await primarycaremanager.GetDoctorListEMIS("IID3");
                gplist.ItemsSource = emisgplist;



            }
            catch (Exception ex)
            {

            }
        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            //back button

            try
            {

                if (signupframe.IsVisible == true)
                {

                    Navigation.RemovePage(this);
                    return;
                }
                else if (epidframe.IsVisible == true)
                {
                    epidframe.IsVisible = false;
                    signupframe.IsVisible = true;
                }
                else if (gplocation.IsVisible == true)
                {
                    gplocation.IsVisible = false;
                    epidframe.IsVisible = true;
                }
                else if (confirmframe.IsVisible == true)
                {
                    confirmframe.IsVisible = false;
                    gplocation.IsVisible = true;
                    return;
                }

                topprogress.Progress -= progressamount;
            }
            catch (Exception ex)
            {

            }
        }

        void epidentry_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
        }

        void gplist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {


                var itemtapped = e.DataItem as primarycare;

                newuser.Gpid = itemtapped.Pracno;

                selectedgplocation = itemtapped;

                //if (Platform.CurrentActivity.CurrentFocus != null)
                //{
                //    Platform.CurrentActivity.HideKeyboard(Platform.CurrentActivity.CurrentFocus);
                //}


            }
            catch (Exception ex)
            {

            }
        }

        async void btnmain_Clicked(System.Object sender, System.EventArgs e)
        {
            //next button clicked
            try
            {

                if (signupframe.IsVisible == true)
                {

                    //check sign up code

                    checksignupcodes.Clear();

                    if (String.IsNullOrEmpty(signuptxt.Text))
                    {
                        signupinput.ErrorText = "Please Enter a Signup Code";
                        signupinput.HasError = true;
                        return;
                    }

                    var signupcode = signuptxt.Text.ToUpper().TrimEnd();

                    if (signupcode == "IID32")
                    {
                        signupcode = "IID3";
                        newuser.Changesignupid = signuptxt.Text.ToUpper().TrimEnd();
                    }

                    checksignupcodes = await advertmanager.GetSpecficAd(signupcode);

                    if (checksignupcodes.Count == 0)
                    {
                        signupinput.ErrorText = "Invalid signup code";
                        signupinput.HasError = true;
                        return;
                    }
                    else
                    {

                        if (checksignupcodes[0].StudyOpen == false)
                        {

                          //  var st = checksignupcodes[0].SiteTitle;
                          //  await DisplayAlert("Test", "test", "Ok");
                           Application.Current.MainPage = new NavigationPage(new NotificationQuestionGP(checksignupcodes[0].AdvertTitle));
                            return;
                        }
                        else
                        {

                            newuser.Signupid = signupcode;
                        }
                    }

                    topprogress.Progress += progressamount;
                    signupframe.IsVisible = false;
                    epidframe.IsVisible = true;

                }
                else if (epidframe.IsVisible == true)
                {

                    //add the epid number
                    topprogress.Progress += progressamount;

                    if (string.IsNullOrEmpty(epidentry.Text))
                    {

                        epidinput.HasError = true;
                        epidinput.ErrorText = "Enter EPID Number";


                    }
                    else
                    {

                        newuser.Epid = epidentry.Text;

                        epidframe.IsVisible = false;
                        gplocation.IsVisible = true;

                    }


                }
                else if (gplocation.IsVisible == true)
                {
                    //update the featue count on the next page

                    if (string.IsNullOrEmpty(newuser.Gpid))
                    {
                        Vibration.Vibrate();
                        return;
                    }

                    topprogress.Progress += progressamount;



                    //add in the confirm details

                    confirmsignuplbl.Text = signuptxt.Text.ToUpper().TrimEnd();
                    confirmnhsnumlbl.Text = epidentry.Text;

                    confirmgplocationlbl.Text = selectedgplocation.Address1 + Environment.NewLine + selectedgplocation.Address2 + Environment.NewLine + selectedgplocation.Address3 + Environment.NewLine + selectedgplocation.Postcode;

                    if (selectedgplocation.Partnershipno == selectedgplocation.Pracno)
                    {
                        confirmgplocationnumlbl.Text = selectedgplocation.Partnershipno;
                    }
                    else
                    {
                        confirmgplocationnumlbl.Text = selectedgplocation.Partnershipno + " / " + selectedgplocation.Pracno;
                    }



                    gplocation.IsVisible = false;
                    confirmframe.IsVisible = true;
                    //var additionalfeaturecount = 3;

                  //  await Navigation.PushAsync(new RegisterPage(newuser, questions, questionanswers, FeaturesForReg, additionalfeaturecount, topprogress.Progress), false);

                }
                else if (confirmframe.IsVisible == true)
                {

                    var additionalfeaturecount = 3;

                    //add this line back in
                    await Navigation.PushAsync(new RegisterPage(newuser, questions, questionanswers, FeaturesForReg, additionalfeaturecount, topprogress.Progress), false);


                }




            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message.ToString(), "OK");
                DisplayAlert("Error", ex.StackTrace.ToString(), "OK");
                
                Console.WriteLine(ex.StackTrace);
            }


        }

        void signuptxt_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            //sign up text changed
            try
            {
                signupinput.HasError = false;
            }
            catch (Exception ex)
            {

            }
        }

        void epidentry_TextChanged_1(System.Object sender, TextChangedEventArgs e)
        {
            //epid text changed
            try
            {
                epidinput.HasError = false;
            }
            catch (Exception ex)
            {

            }
        }

        void searchbargp_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            try
            {
                cantlbl1.IsVisible = false;
                cantlbl2.IsVisible = false;


                if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue == null)
                {
                    //  var list = emisgplist.OrderBy(x => x.Practicename);
                    //  gplist.ItemsSource = list;
                    cantlbl1.IsVisible = false;
                    cantlbl2.IsVisible = false;
                    gplist.IsVisible = false;

                }
                else
                {



                    var countofmedcharacters = e.NewTextValue.Length;

                    if (countofmedcharacters < 2)
                    {
                        //do nothing
                        //makes the user input at least 2 characters
                    }
                    else
                    {


                        var collectionone = emisgplist.Where(x => x.Pracno.ToLowerInvariant().StartsWith(e.NewTextValue.ToLowerInvariant()) ||
                        x.Partnershipno.ToLowerInvariant().StartsWith(e.NewTextValue.ToLowerInvariant()));
                        var orderedlist = collectionone.OrderBy(x => x.Practicename);
                        //foreach(var item in alldocslocations)
                        //{
                        //    checkdouble.Add(item.Address1);
                        //}

                        //var allIdentical = checkdouble.Distinct().Count();


                        //var doublecheck = checkdouble.OrderBy(y=> )

                        var count = collectionone.Count();

                        if (count == 0)
                        {
                            cantlbl1.IsVisible = true;
                            cantlbl2.IsVisible = true;
                            gplist.IsVisible = false;
                            // emptyframe.IsVisible = true;
                            // resultsframe.IsVisible = false;
                            //  maindetails.IsVisible = false;
                            // emptyliststack.IsVisible = true;
                        }
                        else
                        {

                            // emptyframe.IsVisible = false;

                            gplist.IsVisible = true;

                            gplist.ItemsSource = collectionone;
                            // gplist.HeightRequest = 500;
                            gplist.IsVisible = true;
                            //  maindetails.IsVisible = false;
                            // emptyliststack.IsVisible = false;
                        }

                    }


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
                await Browser.OpenAsync("https://www.nhs.uk/nhs-services/online-services/find-nhs-number/", new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Hide
                });

            }
            catch (Exception ex)
            {

            }
        }

        void confirmbackbutton_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                confirmframe.IsVisible = false;
                gplocation.IsVisible = true;
            }
            catch (Exception ex)
            {

            }
        }

        private void gplist_ItemTapped_1(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {

                var itemtapped = e.DataItem as primarycare;

                newuser.Gpid = itemtapped.Pracno;

                selectedgplocation = itemtapped;

            }
            catch (Exception ex)
            {

            }
        }

        private void gplist_ScrollStateChanged(object sender, Syncfusion.Maui.ListView.ScrollStateChangedEventArgs e)
        {
            //try
            //{
            //    searchbargp.HideSoftInputAsync(CancellationToken.None);
            //}
            //catch(Exception ex)
            //{

            //}
        }

        private void searchbargp_SearchButtonPressed(object sender, EventArgs e)
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

