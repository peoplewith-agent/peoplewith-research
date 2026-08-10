using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace PeopleWithResearch
{
    public partial class Logout : ContentPage
    {
        public string HubName;

        public Logout()
        {
            InitializeComponent();
            logoutuser();
        }

        public Logout(string withdrawn)
        {
            InitializeComponent();
            logoutuserwithdrawnmessage();
        }

        void logoutuser()
        {
            try
            {
                Preferences.Set("usertitle", string.Empty);
                Preferences.Set("firstname", string.Empty);
                Preferences.Set("surname", string.Empty);
                Preferences.Set("gender", string.Empty);
                Preferences.Set("email", string.Empty); ;
                Preferences.Set("password", string.Empty);
                Preferences.Set("addresslineone", string.Empty);
                Preferences.Set("addresslinetwo", string.Empty);
                Preferences.Set("town", string.Empty);
                Preferences.Set("city", string.Empty);
                Preferences.Set("postcode", string.Empty);
                Preferences.Set("phonenumber", string.Empty); ;
                Preferences.Set("mostrecentdiagkey", string.Empty);
                Preferences.Set("userpasswordhash", string.Empty);
                Preferences.Set("hassymptomssetting", string.Empty);
                Preferences.Set("announcementids", string.Empty);
                Preferences.Set("height", string.Empty);
                Preferences.Set("weight", string.Empty);
                Preferences.Set("userid", string.Empty);
                Preferences.Set("launchvideo", false);
                Preferences.Set("advertID", string.Empty);
                Preferences.Set("userpreferences", string.Empty);
                Preferences.Set("signupcode", string.Empty);
                Preferences.Set("dashsettings", string.Empty);
                Preferences.Set("additionalconsent", string.Empty);
                Preferences.Set("createdat", string.Empty);
                Preferences.Set("usergpid", string.Empty);

                var mainPage = new NewMainPage();
                NavigationPage.SetHasNavigationBar(mainPage, false);
                Application.Current.MainPage = new NavigationPage(mainPage);

            }
            catch (Exception ex)
            {

            }
        }

        async void logoutuserwithdrawnmessage()
        {
            try
            {

                withdrawnstack.IsVisible = true;

                await Task.Delay(2000);

                //notificationHubService.AddTag("NEWONEFORTEST");
              //  notificationHubService.ClearTags();


                //clear the user info
                //App.Current.Properties.Clear();
                Preferences.Set("usertitle", "");
                Preferences.Set("firstname", "");
                Preferences.Set("surname", "");
                Preferences.Set("gender", "");
                Preferences.Set("email", "");
                Preferences.Set("password", "");
                Preferences.Set("addresslineone", "");
                Preferences.Set("addresslinetwo", "");
                Preferences.Set("town", "");
                Preferences.Set("city", "");
                Preferences.Set("postcode", "");
                Preferences.Set("phonenumber", "");
                Preferences.Set("mostrecentdiagkey", "");
                Preferences.Set("userpasswordhash", "");
                Preferences.Set("hassymptomssetting", "");
                Preferences.Set("announcementids", "");
                Preferences.Set("height", "");
                Preferences.Set("weight", "");
                // Preferences.Set("update", "");
                Preferences.Set("id", "");
                Preferences.Set("launchvideo", "");
                Preferences.Set("advertID", "");
                Preferences.Set("userpreferences", "");
                Preferences.Set("signupcode", "");
                Preferences.Set("dashsettings", "");
                Preferences.Set("additionalconsent", "");
                Preferences.Set("createdat", "");
                Preferences.Set("usergpid", "");
                //Preferences.Set("walkthrough", "");
                //Application.Current.MainPage = new RootPage(true, false, false);
                Application.Current.MainPage = new NavigationPage(new MainPage());


            }
            catch (Exception ex)
            {

            }
        }
    }
}

