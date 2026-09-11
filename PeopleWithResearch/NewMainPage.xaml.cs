

using System.Collections.ObjectModel;
using Mopups.Services;
using Newtonsoft.Json;

namespace PeopleWithResearch;

public partial class NewMainPage : ContentPage
{

    //UserManager usermanager;
    public user checkuser = new user();
    //public AdvertManager advertmanager;
    public ObservableCollection<advert> checksignupcodes = new ObservableCollection<advert>();
    public ObservableCollection<signupcode> checksignupcode = new ObservableCollection<signupcode>();

    //public QuestionnaireManager questionnairemanager;
    public ObservableCollection<Questionnaire> questionnairedetails = new ObservableCollection<Questionnaire>();
    private const string PasteText = "Paste Research Code";
    private const string CheckText = "Check Research Code";
    private bool isawait = false;

    Dictionary<string, string> languages = new Dictionary<string, string>(){ {"en", "Select language"}, {"fr", "Sélectionner la langue"}, {"es", "Seleccionar idioma"}, {"de", "Sprache auswählen"}};

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Successshow.IsVisible = false;
        checkingstack.IsVisible = false;
        pastebtn.IsVisible = true;
    }

    public NewMainPage()
	{
		InitializeComponent();

        if(!String.IsNullOrEmpty(Helpers.Settings.SelectedLanguage))
        {
           languageLabel.Text = languages.TryGetValue(Helpers.Settings.SelectedLanguage, out var lang) 
           ? lang : Helpers.Settings.SelectedLanguage;
        }

        //usermanager = UserManager.DefaultManager;
        //advertmanager = AdvertManager.DefaultManager;
        //questionnairemanager = QuestionnaireManager.DefaultManager;


        //checkifuserisloggedin();

    }

    private async void pastebtn_Clicked(object sender, EventArgs e)
    {
        if (isawait) return;
        try
        {
            isawait = true;
            pasteentry.IsEnabled = false;
            var text = string.Empty;

            if (pastebtn.Text == PasteText)
            {
                try
                {
                    if (!Clipboard.HasText) return;
                    text = (await Clipboard.GetTextAsync())?.Trim();
                }
                catch (Exception clipEx)
                {
                    CrashDetected.LogCrash(clipEx, Navigation, "pastebtn_Clicked_Clipboard");
                    await DisplayAlert("Clipboard Unavailable",
                        "We couldn't read your clipboard. Please type your research code instead.", "Ok");
                    return;
                }

                if (string.IsNullOrWhiteSpace(text)) return;
                pasteentry.Text = text;
            }
            else
            {
                text = pasteentry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(text)) return;
            }

            pastebtn.IsVisible = false;
            checkingstack.IsVisible = true;
            resultstack.IsVisible = false;
            clearbtn.IsVisible = false;

            var lookupTask = APICalls.Instance.GetuserDetails(text);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(40));
            var finishedTask = await Task.WhenAny(lookupTask, timeoutTask);

            if (finishedTask == timeoutTask)
            {
                ShowInvalidCodeResult("Request timed out. Please check your connection and try again.");
                return;
            }

            var GetUser = await lookupTask;

            if (GetUser == null || GetUser.Count == 0)
            {
                ShowInvalidCodeResult("Invalid Research Code");
                return;
            }

            checkuser = GetUser.FirstOrDefault();

            if (checkuser == null)
            {
                ShowInvalidCodeResult("Invalid Research Code");
                return;
            }

            if (checkuser.Status == "active")
            {
                checkingstack.IsVisible = false;
                pastebtn.IsVisible = true;
                await DisplayAlert("Registration Active",
                    "This email address is already in use. Try logging in instead.", "Ok");
                return;
            }

            if (checkuser.Signupcodegrouping == "IMPHOPPER")
            {
                var url = APICalls.CheckSignUpCode + "%27" + checkuser.Signupid + "%27";
                var configuredClient = APICalls.Instance.GetClient();

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                HttpResponseMessage response = await configuredClient.GetAsync(url, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    ShowInvalidCodeResult("Something went wrong. Please try again.");
                    return;
                }

                string content = await response.Content.ReadAsStringAsync();
                var userResponse = JsonConvert.DeserializeObject<ApiResponseSignUpCode>(content);
                ObservableCollection<signupcode> users = userResponse?.Value ?? new ObservableCollection<signupcode>();

                if (users.Count == 0)
                {
                    ShowInvalidCodeResult("Invalid Research Code");
                    return;
                }

                checkingstack.IsVisible = false;
                Successshow.IsVisible = true;
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    await Navigation.PushAsync(new NewImperial(checkuser, users[0]), false);
                }
                else
                {
                    await Navigation.PushAsync(new NewImperial(checkuser, users[0]), false);
                   // await Navigation.PushAsync(new Imperial(checkuser, users[0]), false);
                }
                 
                //var imperialPage = await LoadImperialPageAsync(users[0]);
                //await Navigation.PushAsync(imperialPage, false);
            }
            else
            {
                ShowInvalidCodeResult("Invalid Research Code");
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "pastebtn_Clicked");
            checkingstack.IsVisible = false;
            Successshow.IsVisible = false;
            resultstack.IsVisible = false;
            pastebtn.IsVisible = true;
        }
        finally
        {
            pasteentry.IsEnabled = true;
            isawait = false;
        }
    }

    private async Task<Imperial> LoadImperialPageAsync(signupcode users)
    {
        return await Task.Run(() => new Imperial(checkuser, users));
    }

    private void ShowInvalidCodeResult(string message)
    {
        checkingstack.IsVisible = false;
        resultstack.IsVisible = true;
        resultlbl.Text = message;
        resultimg.Source = ImageSource.FromFile("error.png");
        clearbtn.IsVisible = true;
    }

    //private async void pastebtn_Clicked(object sender, EventArgs e)
    //{
    //    if (isawait) return; 
    //    try
    //    {
    //        //paste button tapped
    //        isawait = true; 
    //        pasteentry.IsEnabled = false; 
    //        var text = string.Empty; 

    //        if(pastebtn.Text == PasteText)
    //        {
    //            if (!Clipboard.HasText) return;
    //            text = (await Clipboard.GetTextAsync())?.Trim();
    //            if (string.IsNullOrWhiteSpace(text)) return;      
    //            pasteentry.Text = text;
    //        }
    //        else
    //        {
    //             text = pasteentry.Text;
    //        }

    //        pastebtn.IsVisible = false;
    //        checkingstack.IsVisible = true;

    //        var GetUser = await APICalls.Instance.GetuserDetails(text);

    //        if (GetUser == null || GetUser.Count == 0)
    //        {
    //            checkingstack.IsVisible = false;
    //            resultstack.IsVisible = true;
    //            resultlbl.Text = "Invalid Research Code";
    //            resultimg.Source = ImageSource.FromFile("error.png");
    //            clearbtn.IsVisible = true;
    //            return;

    //        }
    //        checkuser = GetUser.FirstOrDefault(); 

    //        if (checkuser.Status == "active")
    //        {
    //            await DisplayAlert("Registration Active", "This email address is already in use. Try logging in instead.", "Ok");
    //            pastebtn.IsVisible = true;
    //            checkingstack.IsVisible = false;
    //            return;
    //        }
    //        else
    //        {
    //            if (checkuser.Signupcodegrouping == "IMPHOPPER")
    //            {

    //                //get the correct signupcode
    //                var url = APICalls.CheckSignUpCode + "%27" + checkuser.Signupid + "%27";
    //                var configuredClient = APICalls.Instance.GetClient();
    //                HttpResponseMessage response = await configuredClient.GetAsync(url);

    //                if (response.IsSuccessStatusCode)
    //                {
    //                    string content = await response.Content.ReadAsStringAsync();
    //                    var userResponse = JsonConvert.DeserializeObject<ApiResponseSignUpCode>(content);
    //                    ObservableCollection<signupcode> users = userResponse.Value;

    //                    if (users.Count == 0)
    //                    {

    //                    }
    //                    else
    //                    {
    //                        checkingstack.IsVisible = false;
    //                        Successshow.IsVisible = true;
    //                        await Navigation.PushAsync(new Imperial(checkuser, users[0]), false);

    //                    }
    //                }
    //            }
    //        }


    //        //if (Clipboard.HasText)
    //        //{
    //        //    // await Navigation.PushAsync(new Imperial(), false);
    //        //    // return;



    //        //    var text = await Clipboard.GetTextAsync();

    //        //    if (string.IsNullOrWhiteSpace(text))
    //        //    {
    //        //        return;
    //        //    }

    //        //    text = text.TrimEnd();

    //        //    pasteentry.Text = text;


    //        //    pastebtn.IsVisible = false;
    //        //    checkingstack.IsVisible = true;

    //        //    checkuser = await database.GetuserDetails(text);


    //        //    //   checkuser = await usermanager.getUserInfo(text);

    //        //    if (checkuser == null || checkuser.Count == 0)
    //        //    {
    //        //        checkingstack.IsVisible = false;
    //        //        resultstack.IsVisible = true;
    //        //        resultlbl.Text = "Invalid Research Code";
    //        //        resultimg.Source = ImageSource.FromFile("error.png");
    //        //        clearbtn.IsVisible = true;
    //        //        return;

    //        //    }
    //        //    else
    //        //    {

    //        //        if (checkuser[0].Status == "active")
    //        //        {
    //        //            await DisplayAlert("Registration Active", "This email address is already in use. Try logging in instead.", "Ok");
    //        //            pastebtn.IsVisible = true;
    //        //            checkingstack.IsVisible = false;
    //        //            return;
    //        //        }
    //        //        else
    //        //        {

    //        //            if (checkuser[0].Signupcodegrouping == "IMPHOPPER")
    //        //            {

    //        //                //get the correct signupcode

    //        //                var url = APICalls.CheckSignUpCode + "%27" + checkuser[0].Signupid + "%27";
    //        //                var configuredClient = APICalls.GetClient();
    //        //                HttpResponseMessage response = await configuredClient.GetAsync(url);

    //        //                if (response.IsSuccessStatusCode)
    //        //                {
    //        //                    string content = await response.Content.ReadAsStringAsync();
    //        //                    var userResponse = JsonConvert.DeserializeObject<ApiResponseSignUpCode>(content);
    //        //                    ObservableCollection<signupcode> users = userResponse.Value;

    //        //                    if (users.Count == 0)
    //        //                    {

    //        //                    }
    //        //                    else
    //        //                    {
    //        //                        checkingstack.IsVisible = false;
    //        //                        Successshow.IsVisible = true;
    //        //                        await Navigation.PushAsync(new Imperial(checkuser, users[0]), false);
    //        //                        pastebtn.IsVisible = true;
    //        //                    }


    //        //                }



    //        //            }
    //        //        }


    //        //        ////check they are no dub users
    //        //        //var checkepid = await usermanager.getUserInfoEPIDandGPIDandSignupcode(checkuser[0].Epid, checkuser[0].Gpid, checkuser[0].Signupid);

    //        //        //if (checkepid.Count > 1)
    //        //        //{


    //        //        //    if (checkepid.Any(x => x.RegStatus == "Active"))
    //        //        //    {
    //        //        //        resultlbl.Text = "Invalid Research Code";
    //        //        //        resultsublbl.Text = "Details associated to this research code are already in use. Please login to continue. If you believe this is an error, please contact: support@peoplewith.com";
    //        //        //        resultimg.Source = ImageSource.FromFile("error.png");
    //        //        //        checkingstack.IsVisible = false;
    //        //        //        resultstack.IsVisible = true;
    //        //        //        clearbtn.IsVisible = true;
    //        //        //        return;

    //        //        //    }


    //        //        //}


    //        //        ////Go and get sign up code details
    //        //        //checksignupcodes = await advertmanager.GetSpecficAd(checkuser[0].Signupid);


    //        //        ////go and get questionnaire details for additional questions if required

    //        //        //questionnairedetails = await questionnairemanager.getQuestionnairebySignupcode(checkuser[0].Signupid);



    //        //        //if (checkuser[0].Signupid == "IMPHPRU01")
    //        //        //{

    //        //        //    await Navigation.PushAsync(new Imperial(checkuser[0], checksignupcodes[0], questionnairedetails[0]), false);

    //        //        //}


    //        //        checkingstack.IsVisible = false;
    //        //        Successshow.IsVisible = false;
    //        //        pastebtn.IsVisible = true;
    //        //    }

    //        //}


    //    }
    //    catch (Exception Ex)
    //    {
    //        CrashDetected.LogCrash(Ex, Navigation, "pastebtn_Clicked");
    //        checkingstack.IsVisible = false;
    //        Successshow.IsVisible = false;
    //        pastebtn.IsVisible = true;
    //        pasteentry.IsEnabled = true;
    //        isawait = false;
    //        //Analytics.TrackEvent("CRASH - MainPage - paste button - " + ex.StackTrace.ToString());
    //    }
    //    finally
    //    {
    //        pasteentry.IsEnabled = true;
    //        isawait = false;
    //        //Successshow.IsVisible = false;
    //    }
    //}

    private void clearbtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            pasteentry.Text = string.Empty;
            checkingstack.IsVisible = false;
            resultstack.IsVisible = false;
            clearbtn.IsVisible = false;
            pastebtn.IsVisible = true;          

        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "clearbtn_Clicked");
        }
    }

    private async void regbtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            await App.SetMainPage(new NavigationPage(new ImperialDashboard()));
        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "regbtn_Clicked");
        }
    }

    //async void checkifuserisloggedin()
    //{
    //    try
    //    {
    //        //Temporary Signin
    //        //Helpers.Settings.UserKey = "63C5E37F-7A27-4157-89D3-06EFB63D6A00"; 

    //        //var BoolCheck = Preferences.Default.Get("RunOnce", )

    //        var userid = Preferences.Default.Get("userid", string.Empty);

    //        if (!string.IsNullOrEmpty(userid))
    //        {
    //            MainThread.BeginInvokeOnMainThread(async () =>
    //            {
    //                await App.SetMainPage(new NavigationPage(new ImperialDashboard()));
    //            });
             
    //        }
    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //}

    private async void loginbtn_Clicked(object sender, EventArgs e)
    {
        if (isawait) return; 

        try
        {
            isawait = true;
            loginbtn.IsEnabled = false;
            if (Navigation.NavigationStack.LastOrDefault() is not NewLoginPage)
            {
                await Navigation.PushAsync(new NewLoginPage(), false);
            }
            await Task.Delay(2000); 
            loginbtn.IsEnabled = true;
            isawait = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "loginbtn_Clicked");
            loginbtn.IsEnabled = true;
            isawait = false;
        }
    }

    private void pasteentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            pastebtn.Text = string.IsNullOrWhiteSpace(e.NewTextValue) ? PasteText : CheckText;
            pastebtn.IsVisible = true; 
            checkingstack.IsVisible = false;
            resultstack.IsVisible = false;
            clearbtn.IsVisible = false;
        }
        catch( Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "pasteentry_TextChanged");
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            TaskCompletionSource<string> languageCompletionSource = new TaskCompletionSource<string>();
            await MopupService.Instance.PushAsync(new SelectLanguagePopup(languageCompletionSource));
            languageLabel.Text = await languageCompletionSource.Task;
        }
        catch (Exception Ex)
        {
        }
    }
}