
using Microsoft.AppCenter.Analytics;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using PeopleWithResearch.Resources.Strings;

namespace PeopleWithResearch
{
    //public ObservableCollection<User> checkuser = new ObservableCollection<User>();

    public partial class MainPage : ContentPage
    {
        UserManager usermanager;
        public ObservableCollection<user> checkuser = new ObservableCollection<user>();
        public AdvertManager advertmanager;
        public ObservableCollection<advert> checksignupcodes = new ObservableCollection<advert>();
        initialQuestionsManager initialquestionsmanager;
        public ObservableCollection<initialQuestions> questions = new ObservableCollection<initialQuestions>();
        public ObservableCollection<initialQuestionsAnswers> questionanswers = new ObservableCollection<initialQuestionsAnswers>();
        public ObservableCollection<initialQuestions> completedquestions = new ObservableCollection<initialQuestions>();
        initialQuestionsAnswersManager initialquestionsanswersmanager;
        FeatureManager featuresmanager;
        public ObservableCollection<features> FeaturesForReg = new ObservableCollection<features>();

        public MainPage()
        {
            InitializeComponent();
            try
            {
                usermanager = UserManager.DefaultManager;
                advertmanager = AdvertManager.DefaultManager;
                initialquestionsmanager = initialQuestionsManager.DefaultManager;
                initialquestionsanswersmanager = initialQuestionsAnswersManager.DefaultManager;
                featuresmanager = FeatureManager.DefaultManager;
               
                // Get Metrics
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
                // Height (in pixels)
                var height = mainDisplayInfo.Height;
                //iphone 8 or below
                if (height <= 1334)
                {
                    mainimg.HeightRequest = 150;
                    mainimg.WidthRequest = 150;
                }

                
                Checkifappisupdated();

                Checkifuserisloggedin();

            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "MainPage");
            }
        }

        public async Task Checkifuserisloggedin()
        {
            try
            {
                var id = Helpers.Settings.UserKey;
                if (string.IsNullOrEmpty(Helpers.Settings.Email))
                {
                    //do nothing
                }
                else
                {
                    if (Helpers.Settings.SignUp == "IID32")
                    {
                        await Navigation.PushAsync(new NotificationQuestionGP(), false);
                    }
                    else
                    {
                        await Navigation.PushAsync(new MainDashboard(), false);
                    }
                    Navigation.RemovePage(this);
                }
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "Checkifuserisloggedin");
            }
        }



        public async Task Checkifappisupdated()
        {
            try
            {
                var versionCheckService = new VersionCheckService();
                await versionCheckService.CheckForUpdate();
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "Checkifappisupdated");
            }
        }

       
        async void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            try
            {
                //paste button tapped

                if (Clipboard.HasText)
                {
                    // await Clipboard.SetTextAsync("67dc4fde-57c1-403e-8165-01c47794a548");

                    var text = await Clipboard.GetTextAsync();

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        return;
                    }

                    text = text.TrimEnd();

                    pasteentry.Text = text;
                    clearbtn.IsVisible = true;

                    if (text == "IID3" || text == "IID32")
                    {
                        await Navigation.PushAsync(new RegisterWithDetailsPage(text), false);
                        return;
                    }


                    pastebtn.IsVisible = false;
                    loginlbl.IsVisible = false;
                    loginbtn.IsVisible = false;
                    reglbl.IsVisible = false;
                    regbtn.IsVisible = false;



                    checkingstack.IsVisible = true;

                    //check sign up code

                    //check sign up userid pasted

                    checkuser = await usermanager.getUserInfo(text);

                    if (checkuser == null)
                    {
                        loaderlabel.Text = LocalizationManager.Get("Main_InvalidCode");
                        tickloader.Source = ImageSource.FromFile("error.png");
                        tickloader.IsVisible = true;
                        loader.IsVisible = false;
                        tryagainbutton.IsVisible = true;
                        // Analytics.TrackEvent("MainPage - Invalid Research Code");
                        return;

                    }

                    if (checkuser.Count != 0)
                    {
                        await Task.Delay(2000);
                        loaderlabel.Text = LocalizationManager.Get("Main_ValidatingCode");


                        //tick1.Opacity = 1;
                        //label1.Opacity = 1;
                    }
                    else
                    {
                        //code is not in system
                        loaderlabel.Text = LocalizationManager.Get("Main_InvalidCode");
                        tickloader.Source = ImageSource.FromFile("error.png");
                        tickloader.IsVisible = true;
                        loader.IsVisible = false;
                        tryagainbutton.IsVisible = true;
                        return;
                        //tick1.Source = ImageSource.FromFile("error.png");
                        //tick1.Opacity = 1;
                        //label1.Opacity = 1;
                        //label1.Text = "Research Code not found";
                        //loader.IsVisible = false;
                        //tryagainbutton.IsVisible = true;
                        //loaderlabel.Text = "Please try a different Research Code";
                        //tick2.IsVisible = false;
                        //label2.IsVisible = false;
                        //tick3.IsVisible = false;
                        //label3.IsVisible = false;

                    }

                    //check if there is any duplicates 


                    if (checkuser.Count > 1)
                    {
                        //means their is more than one of the user in the db
                    }
                    else
                    {
                        if (checkuser[0].RegStatus == "Onboarding")
                        {
                            //check user epid
                            var checkepid = await usermanager.getUserInfoEPIDandGPID(checkuser[0].Epid, checkuser[0].Gpid);

                            if (checkepid.Count > 1)
                            {
                                //show second error

                                if (checkepid.Any(x => x.RegStatus == "Active"))
                                {
                                    loaderlabel.Text = LocalizationManager.Get("Main_DetailsInUse");
                                    tickloader.Source = ImageSource.FromFile("error.png");
                                    tickloader.IsVisible = true;
                                    loader.IsVisible = false;
                                    tryagainbutton.IsVisible = true;
                                    return;
                                    //tick2.Source = ImageSource.FromFile("error.png");
                                    //tick2.Opacity = 1;
                                    //label2.Opacity = 1;
                                    //label2.Text = "Multiple EPID codes found";
                                    //tick3.IsVisible = false;
                                    //label3.IsVisible = false;
                                    //return;
                                }


                            }


                            ///get the reg layout and structure
                            ///


                            //check if the signup equals iid32 so change it to iid3
                            if (checkuser[0].Signupid == "IID32")
                            {
                                checkuser[0].Signupid = "IID3";
                                checkuser[0].Changesignupid = "IID32";
                            }


                            FeaturesForReg = await featuresmanager.GetSpecficAd(checkuser[0].Signupid);





                            loaderlabel.Text = LocalizationManager.Get("Main_CheckingDuplications");

                            checksignupcodes = await advertmanager.GetSpecficAd(checkuser[0].Signupid);

                            //check if they any additional questions

                            questions = await initialquestionsmanager.getinitialQuestions(checksignupcodes[0].AdvertID);


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

                            // Analytics.TrackEvent("MainPage - Success on copy and paste code");
                            loaderlabel.Text = LocalizationManager.Get("Main_Success");
                            loader.IsVisible = false;
                            tickloader.IsVisible = true;


                            bottomstack.IsVisible = true;
                            btnmain.IsVisible = true;

                            //tick2.Opacity = 1;
                            //label2.Opacity = 1;

                            //tick3.Opacity = 1;
                            //label3.Opacity = 1;


                            //loader.IsVisible = false;
                            //loaderlabel.IsVisible = false;

                        }
                        else
                        {
                            loaderlabel.Text = LocalizationManager.Get("Main_UserAlreadyExists");
                            tickloader.Source = ImageSource.FromFile("error.png");
                            tickloader.IsVisible = true;
                            loader.IsVisible = false;
                            tryagainbutton.IsVisible = true;
                            // Analytics.TrackEvent("MainPage - User Already Exists");
                            return;
                        }


                    }
                }


            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped");
            }
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                 await Navigation.PushAsync(new RegisterWithDetailsPage(), false);

                // Analytics.TrackEvent("Register with details Clicked");
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "Button_Clicked");
            }
        }

        async void TapGestureRecognizer_Tapped_Login(System.Object sender, System.EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new LoginPage());
                //await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                // await Navigation.PushAsync(new RegisterPage(checkuser[0], questions, questionanswers, FeaturesForReg), true);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_Login");
            }
        }

        void tryagainbutton_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {

                checkingstack.IsVisible = false;
                loader.IsVisible = true;
                tryagainbutton.IsVisible = false;
                tickloader.IsVisible = false;
                tickloader.Source = ImageSource.FromFile("tick.png");
                loaderlabel.Text = LocalizationManager.Get("Main_CheckingCode");
                pastebtn.IsVisible = true;
                loginlbl.IsVisible = true;
                loginbtn.IsVisible = true;
                reglbl.IsVisible = true;
                regbtn.IsVisible = true;
                clearbtn.IsVisible = false;
                btnmain.IsVisible = false;

                pasteentry.Text = "";

            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "tryagainbutton_Clicked");
            }
        }

         async void btnmain_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new RegisterPage(checkuser[0], questions, questionanswers, FeaturesForReg), false);

               Analytics.TrackEvent("Registration Signup Start Clicked");
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "btnmain_Clicked");
            }
        }

        void clearbtn_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {

                checkingstack.IsVisible = false;
                loader.IsVisible = true;
                tryagainbutton.IsVisible = false;
                tickloader.IsVisible = false;
                tickloader.Source = ImageSource.FromFile("tick.png");
                loaderlabel.Text = LocalizationManager.Get("Main_CheckingCode");
                pastebtn.IsVisible = true;
                loginlbl.IsVisible = true;
                loginbtn.IsVisible = true;
                reglbl.IsVisible = true;
                regbtn.IsVisible = true;
                clearbtn.IsVisible = false;
                btnmain.IsVisible = false;

                pasteentry.Text = "";


            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "clearbtn_Clicked");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                // Unregister first to guard against double-registration if OnAppearing fires multiple times
                WeakReferenceMessenger.Default.Unregister<LanguageChangedMessage>(this);
                WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
                {
                    MainThread.BeginInvokeOnMainThread(() => RefreshLocalizedText());
                });
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "MainPage_OnAppearing");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try
            {
                WeakReferenceMessenger.Default.Unregister<LanguageChangedMessage>(this);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "MainPage_OnDisappearing");
            }
        }

        void RefreshLocalizedText()
        {
            try
            {
                mainlbl.Text = LocalizationManager.Get("MainPage_ResearchCode");
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "MainPage_RefreshLocalizedText");
            }
        }

        async void btnLanguage_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                var tcs = new TaskCompletionSource<string>();
                await MopupService.Instance.PushAsync(new SelectLanguagePopup(tcs));
                await tcs.Task;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "MainPage_btnLanguage_Clicked");
            }
        }

    }

}
