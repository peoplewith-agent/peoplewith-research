using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.Maui.Controls.PlatformConfiguration;
//using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;

namespace PeopleWithResearch
{
    public partial class DashboardConsent : Mopups.Pages.PopupPage
    {
        user userpassedfromdash;
        public ObservableCollection<consent> ConsentDetails = new ObservableCollection<consent>();
        ConsentManager consentmanager;
        public List<string> consentdetailslist = new List<string>();
        public UserManager usermanager;
        public string PDFinfostring;
        public MedDirectionsManager meddirectionsmanager;

        public DashboardConsent()
        {
            InitializeComponent();


            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //On<iOS>().SetUseSafeArea(false);


        }

        public DashboardConsent(user userpassed)
        {
            InitializeComponent();


            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //On<iOS>().SetUseSafeArea(false);

            consentmanager = ConsentManager.DefaultManager;
            usermanager = UserManager.DefaultManager;
            meddirectionsmanager = MedDirectionsManager.DefaultManager;

            userpassedfromdash = userpassed;


            getconsentdetails();


        }

        async Task getconsentdetails()
        {
            try
            {


                //check which consent we need to get

                //check for over and under 18
                var age = userpassedfromdash.Loweragebracket + 5;


                if (age < 18)
                {
                    //under 18
                    ConsentDetails = await consentmanager.getconsentregunder18(userpassedfromdash.Signupid);

                }
                else
                {
                    //over 18
                    ConsentDetails = await consentmanager.getconsentregover18(userpassedfromdash.Signupid);

                }

                if (ConsentDetails.Count > 0)
                {
                    var content = ConsentDetails[0].ConsentContent;

                    var splitcontent = content.Split('|');

                    List<string> stringList = new List<string>(splitcontent);

                    consentdetailslist = stringList;

                    consentlist.ItemsSource = consentdetailslist;
                }

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
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "getconsentdetails");
            }
        }

        void closebtn_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                Navigation.RemovePage(this);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "closebtn_Clicked");
            }
        }

        async void withdrawnbtn_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {


                var result = await DisplayAlert("Withdraw from study", "Are you sure you wish to withdraw (leave) the research project?", "Cancel", "OK");
                if (result)
                {
                    return;
                }
                else
                {

                    var resultt = await DisplayAlert("Confrim Withdrawal", "Please confirm withdrawal from this study by tapping 'ok'. Data captured during your participation is subject to the Terms and Conditions of the project.", "Cancel", "OK");

                    if (resultt)
                    {
                        return;
                    }
                    else
                    {
                        //get user details 
                        var deleteuser = userpassedfromdash;


                        if (deleteuser != null)
                        {
                            deleteuser.RegStatus = "Withdrawn";
                        }


                        // await usermanager.DeleteUser(passedUserPD);

                        await usermanager.AddUser(deleteuser);
                        await Navigation.PushAsync(new Logout("withdrawn"), false);
                        // Navigation.RemovePage(this);
                    }
                }
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "withdrawnbtn_Clicked");
            }
        }

        async void studyinfobtn_Clicked(System.Object sender, System.EventArgs e)
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
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "studyinfobtn_Clicked");
            }
        }
    }
}

