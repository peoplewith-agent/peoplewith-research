using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace PeopleWithResearch
{
    public partial class WelcomePage : ContentPage
    {



        public WelcomePage()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);


            gotodash();

        }

        async void gotodash()
        {
            try
            {
                await Task.Delay(3200);

                if (Helpers.Settings.SignUp == "IID32")
                {
                    Application.Current.MainPage = new NavigationPage(new NotificationQuestionGP());
                }
                else
                {
                    //add back in
                    Application.Current.MainPage = new NavigationPage(new MainDashboard());
                }
            }
            catch (Exception ex)
            {

            }



        }
    }
}
