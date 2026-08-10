using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.PlatformConfiguration;
//using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Devices;

namespace PeopleWithResearch
{
    public partial class SignUpConstent : ContentPage
    {

        public ConsentManager consentmanger;
        public ObservableCollection<consent> consentdata = new ObservableCollection<consent>();

        public UserConsentManager userconsentmanager;
        public userconsent UserConsent = new userconsent();

        public string id;
        public string signupcodepassed;
        public string areapassed;

        public SignUpConstent(string signupcode, string area)
        {
            consentmanger = ConsentManager.DefaultManager;
            userconsentmanager = UserConsentManager.DefaultManager;
            //toolbar.IconImageSource = ImageSource.FromFile("canceliconlight.png");

            signupcodepassed = signupcode;
            areapassed = area;


            InitializeComponent();
           // On<iOS>().SetUseSafeArea(false);


            getconsentdata();



        }

        async Task getconsentdata()
        {
            try
            {
                //get consent data
                consentdata = await consentmanger.getconsent(signupcodepassed);

                var questions = new List<string>();

                foreach (var item in consentdata)
                {
                    //page title
                    consenttitlelbl.Text = item.ConsentTitle;

                    consentcontent.Text = item.ConsentContent;

                    //populate question list
                    questions.Add(item.ConsentQuestion1);
                    questions.Add(item.ConsentQuestion2);
                    questions.Add(item.ConsentQuestion3);

                    //button text Consent/dont consent
                    consentbtn.Text = item.ConsentOption1;
                    noconsentbtn.Text = item.ConsentOption2;

                    id = item.Id;

                }

                questionlist.ItemsSource = questions;

            }
            catch (Exception ex)
            {

            }
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {

                //user consent table data added
              //  UserConsent.Consentid = id;


              //  UserConsent.Advertid = signupcodepassed;
             //   UserConsent.Userid = Helpers.Settings.UserKey;
              //  UserConsent.Consented = true;
              //  UserConsent.Area = areapassed;

                await userconsentmanager.adduserconsent(UserConsent);

                await Navigation.PopModalAsync();

                if (areapassed == "Registration")
                {
                    Application.Current.MainPage = new NavigationPage(new WelcomePage());
                    return;
                }
            }
            catch (Exception ex)
            {

            }


        }

   

        async void noconsentbtn_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {

                bool answer;
                Vibration.Vibrate();
                if (areapassed == "EditSignUp" || areapassed == "Registration")
                {
                    answer = await DisplayAlert("Consent Required", "Without your consent you will be unable to continue with your current Sign Up Code", "I Do Not Consent", "Go Back");
                }
                else
                {
                    answer = true;
                }

                if (answer == false)
                {
                    return;
                }
                else
                {
                   // UserConsent.Consentid = id;
                 //   UserConsent.Advertid = signupcodepassed;
//UserConsent.Userid = Helpers.Settings.UserKey;
                 //   UserConsent.Consented = false;
                 //   UserConsent.Area = areapassed;

                    await userconsentmanager.adduserconsent(UserConsent);
                    await Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void questionlist_ItemTapped_1(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                string question = e.DataItem as string;
                consent Consent = consentdata[0];

                if (question == Consent.ConsentQuestion1)
                {
                    if (answerlbl.IsVisible == true && answerlbl.Text == Consent.ConsentAnswer1)
                    {
                        answerlbl.IsVisible = false;
                        questionlist.SelectedItems.Clear();
                    }
                    else
                    {
                        answerlbl.IsVisible = true;
                        answerlbl.Text = Consent.ConsentAnswer1;

                    }
                }
                else if (question == Consent.ConsentQuestion2)
                {
                    if (answerlbl.IsVisible == true && answerlbl.Text == Consent.ConsentAnswer2)
                    {
                        answerlbl.IsVisible = false;
                        questionlist.SelectedItems.Clear();

                    }
                    else
                    {
                        answerlbl.IsVisible = true;
                        answerlbl.Text = Consent.ConsentAnswer2;

                    }
                }
                else if (question == Consent.ConsentQuestion3)
                {
                    if (answerlbl.IsVisible == true && answerlbl.Text == Consent.ConsentAnswer3)
                    {
                        answerlbl.IsVisible = false;
                        questionlist.SelectedItems.Clear();

                    }
                    else
                    {
                        answerlbl.IsVisible = true;
                        answerlbl.Text = Consent.ConsentAnswer3;

                    }
                }

                //Navigation.PushAsync(new Xamarin.Forms.NavigationPage(new ConsentAnswer(Consent, question)));
            }
            catch (Exception ex)
            {

            }
        }
    }
}
