using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
//using Rg.Plugins.Popup.Extensions;
using Microsoft.Maui.Controls.PlatformConfiguration;
//using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Syncfusion.Maui.TabView;
//using CoreImage;
using Plugin.LocalNotification;

namespace PeopleWithResearch
{
    public partial class MainDashboard : ContentPage
    {
        //public INotificationHubService notificationHubService;
        //public string HubName;
        public List<string> firstitemlist = new List<string>();
        public List<string> seconditemlist = new List<string>();
        public List<string> thirditemlist = new List<string>();

        public ObservableCollection<user> UserList = new ObservableCollection<user>();

        public List<user> userdetailslist = new List<user>();
        public List<user> userdetailslist2 = new List<user>();
        public UserManager usermanager;
        public ObservableCollection<UserQuestionnaire> userquestionnaires = new ObservableCollection<UserQuestionnaire>();
        public UserQuestionnaireManager userquestionnairemanager;
        int weekNumber;
        public IncidentsManager incidentsmanager;
        public PrimaryCareManager primarycaremanager;
        public incidents Userincident;
        public IncidentHistoryManager incidenthistorymanager;

        public ObservableCollection<UserQuestionnaire> completedquestionnaires = new ObservableCollection<UserQuestionnaire>();
        public ObservableCollection<UserQuestionnaire> incompletedquestionnaires = new ObservableCollection<UserQuestionnaire>();
        public ObservableCollection<UserQuestionnaire> Previoususerquestionnaires = new ObservableCollection<UserQuestionnaire>();

        public ObservableCollection<incidents> Allincidents = new ObservableCollection<incidents>();

        public MedDirectionsManager meddirectionsmanager;
        public AdvertManager advermanager;

        public UserQuestionAnswerManager userquestionanswermanager;

        public ObservableCollection<user> getuser = new ObservableCollection<user>();

        public ObservableCollection<advert> getad = new ObservableCollection<advert>();

        public TimeSpan timeUntilNextQuestionnaire = new TimeSpan();
        public MainDashboard()
        {
            InitializeComponent();

            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            incidentsmanager = IncidentsManager.DefaultManager;
            primarycaremanager = PrimaryCareManager.DefaultManager;
            incidenthistorymanager = IncidentHistoryManager.DefaultManager;
            meddirectionsmanager = MedDirectionsManager.DefaultManager;
            advermanager = AdvertManager.DefaultManager;
            userquestionanswermanager = UserQuestionAnswerManager.DefaultManager;
            usermanager = UserManager.DefaultManager;

            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //On<iOS>().SetUseSafeArea(false);


            //notificationHubService = DependencyService.Get<INotificationHubService>();
            //HubName = Constants.NotificationHubName;

            //// TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            //if (Device.RuntimePlatform == Device.Android)
            //{
            //    notificationHubService.ClearTags();
            //    notificationHubService.AddTag(Helpers.Settings.UserKey); 
            //    notificat ionHubService.AddTag(Helpers.Settings.SignUp);
            //}
            //else
            //{

            //    notificationHubService.AddTag(Helpers.Settings.UserKey);
            //    notificationHubService.AddTag(Helpers.Settings.SignUp);
            //}

            Analytics.TrackEvent("PeopleWithResearch Dashboard Opened");

            NetworkService.Instance.ConnectivityChanged += OnConnectivityChanged;

            checknetwork();


            // //refresh the queestionnaire and status
            MessagingCenter.Subscribe<object, object>(this, "refreshdashafterquestionnaire", (sender, args) =>
            {
                // Handle the received message

                try
                {


                    var newcollection = args as ObservableCollection<UserQuestionnaire>;

                    if (newcollection != null)
                    {

                        donebtn.IsVisible = true;
                        completebtn.IsVisible = false;

                        completedquestionnaires.Clear();
                        incompletedquestionnaires.Clear();

                        //check if any weeks are missing

                        for (int i = 1; i <= weekNumber; i++)
                        {

                            if (newcollection.Any(x => x.Score == i.ToString()))
                            {
                                //do nothing as its already in
                            }
                            else
                            {
                                //means they have missed the week
                                var newquestionnaire = new UserQuestionnaire();
                                newquestionnaire.Weeknumber = "Week " + i.ToString();
                                newquestionnaire.Completestring = "Incomplete";
                                newquestionnaire.Score = i.ToString();
                                newquestionnaire.Imagename = "incompletequestionnaires.png";

                                Previoususerquestionnaires.Add(newquestionnaire);
                                incompletedquestionnaires.Add(newquestionnaire);
                            }


                        }


                        foreach (var item in newcollection)
                        {


                            item.Weeknumber = "Week " + item.Score;

                            item.Completestring = "Completed";
                            item.Imagename = "completequestionnaires.png";

                            completedquestionnaires.Add(item);

                            Previoususerquestionnaires.Add(item);

                        }


                        completedlbl.Text = completedquestionnaires.Count.ToString();
                        incompletelbl.Text = incompletedquestionnaires.Count.ToString();


                    }

                }
                catch (Exception ex)
                {

                }
            });


            MessagingCenter.Subscribe<object, object>(this, "updateuserdetails", (sender, args) =>
            {
                // Handle the received message

                try
                {
                    //update the dash with the user new details

                    var newcollection = args as user;

                    if (newcollection != null)
                    {
                        userdetailslist.Clear();

                        welcomelbl.Text = "Hi " + Helpers.Settings.FirstName;

                        //profile section


                        namelbl.Text = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

                        if (namelbl.Text == " ")
                        {
                            namelbl.Text = "Your Profile";
                        }

                        //emaillbl.Text = Helpers.Settings.Email;

                        //user details list

                        var newuserr1 = new user();
                        newuserr1.Title = "Name";

                        var fullname0 = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

                        if (fullname0 == " ")
                        {
                            newuserr1.Role = "--";
                        }
                        else
                        {
                            newuserr1.Role = fullname0;
                        }



                        userdetailslist.Add(newuserr1);

                        var newuserrr0 = new user();
                        newuserrr0.Title = "Email";
                        newuserrr0.Role = Helpers.Settings.Email;

                        userdetailslist.Add(newuserrr0);


                        var newuser0 = new user();
                        newuser0.Title = "Date of Birth";

                        if (Helpers.Settings.Age.Contains("00:00:00"))
                        {
                            var n = Helpers.Settings.Age;

                            var nn = n.Replace("00:00:00", string.Empty);
                            newuser0.Role = nn;

                        }
                        else
                        {
                            newuser0.Role = Helpers.Settings.Age;
                        }


                        userdetailslist.Add(newuser0);

                        var newuser10 = new user();
                        newuser10.Title = "Gender";
                        newuser10.Role = Helpers.Settings.Gender;

                        userdetailslist.Add(newuser10);

                        var newuser20 = new user();
                        newuser20.Title = "Ethnicity";
                        newuser20.Role = Helpers.Settings.Ethnicity;

                        userdetailslist.Add(newuser20);

                        var newuser440 = new user();
                        newuser440.Title = "Phone Number";


                        if (string.IsNullOrEmpty(Helpers.Settings.PhoneNumber))
                        {
                            newuser440.Role = "--";
                        }
                        else
                        {
                            newuser440.Role = Helpers.Settings.PhoneNumber;
                        }



                        userdetailslist.Add(newuser440);


                        var newuser4440 = new user();
                        newuser4440.Title = "Town/City";

                        if (string.IsNullOrEmpty(Helpers.Settings.Town))
                        {
                            newuser4440.Role = "--";
                        }
                        else
                        {
                            newuser4440.Role = Helpers.Settings.Town;
                        }


                        userdetailslist.Add(newuser4440);

                    //    if (Helpers.Settings.Validityconfirmed == "false")
                      //  {
                        //    var exists = userdetailslist.Where(u => u.Title == "National Health Identifier").FirstOrDefault();

                          //  if(exists != null)
                          //  {
                            //    userdetailslist.Remove(exists);

//                                var newuser44401 = new user();
  //                              newuser44401.Title = "National Health Identifer";
    //                            newuser44401.Role = Helpers.Settings.Userepid;
      //                          userdetailslist.Add(newuser44401);
        //                    }
                      //  }

                        //var newuser30 = new User();
                        //newuser30.Title = "Height";

                        //if (string.IsNullOrEmpty(Helpers.Settings.Height))
                        //{
                        //    newuser30.Role = "--";
                        //}
                        //else
                        //{
                        //    newuser30.Role = Helpers.Settings.Height;
                        //}


                        //userdetailslist.Add(newuser30);

                        //var newuser40 = new User();
                        //newuser40.Title = "Weight";


                        //if (string.IsNullOrEmpty(Helpers.Settings.Weight))
                        //{
                        //    newuser40.Role = "--";
                        //}
                        //else
                        //{
                        //    newuser40.Role = Helpers.Settings.Weight;
                        //}


                        foreach (var item in userdetailslist)
                        {
                            if (item.Title == "Height" || item.Title == "Weight")
                            {
                                item.Imagevisible = true;
                            }
                            else
                            {
                                item.Imagevisible = true;
                            }
                        }

                        // userdetailslist.Add(newuser40);

                     //   profilelist.ItemsSource = userdetailslist;

                    }

                }
                catch (Exception ex)
                {

                }
            });


            // Subscribe to the message sent from WithdrawVideoPopUp
            MessagingCenter.Subscribe<WithdrawVideoPopUp>(this, "VideoPageLeft", (sender) =>
            {
                // Handle the message (show the popup)
                ShowPopup();
            });


            welcomelbl.Text = "Hi " + Helpers.Settings.FirstName;

            //check if they have selected a notification time
            checknotificationtime();

            getinfo();

            getalldata();

            // AddNotificationsTags();

            if(string.IsNullOrEmpty(Helpers.Settings.AndroidNote))
            {
                Preferences.Set("androidnotekey", "firstreg");
            }

            


            //profile section
            usermanager = UserManager.DefaultManager;

            namelbl.Text = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

            if (namelbl.Text == " ")
            {
                namelbl.Text = "Your Profile";
            }

            //emaillbl.Text = Helpers.Settings.Email;

            //user details list

            var newuserr = new user();
            newuserr.Title = "Name";

            var fullname = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

            if (fullname == " ")
            {
                newuserr.Role = "--";
            }
            else
            {
                newuserr.Role = fullname;
            }



            userdetailslist.Add(newuserr);

            var newuserrr = new user();
            newuserrr.Title = "Email";
            newuserrr.Role = Helpers.Settings.Email;

            userdetailslist.Add(newuserrr);


            var newuser = new user();
            newuser.Title = "Date of Birth";

            if (Helpers.Settings.Age.Contains("00:00:00"))
            {
                var n = Helpers.Settings.Age;

                var nn = n.Replace("00:00:00", string.Empty);
                newuser.Role = nn;

            }
            else
            {
                newuser.Role = Helpers.Settings.Age;
            }


            userdetailslist.Add(newuser);

            var newuser1 = new user();
            newuser1.Title = "Gender";
            newuser1.Role = Helpers.Settings.Gender;

            userdetailslist.Add(newuser1);

            var newuser2 = new user();
            newuser2.Title = "Ethnicity";
            newuser2.Role = Helpers.Settings.Ethnicity;

            userdetailslist.Add(newuser2);

            var newuser44 = new user();
            newuser44.Title = "Phone Number";


            if (string.IsNullOrEmpty(Helpers.Settings.PhoneNumber))
            {
                newuser44.Role = "--";
            }
            else
            {
                newuser44.Role = Helpers.Settings.PhoneNumber;
            }



            userdetailslist.Add(newuser44);


            var newuser444 = new user();
            newuser444.Title = "Town/City";

            if (string.IsNullOrEmpty(Helpers.Settings.Town))
            {
                newuser444.Role = "--";
            }
            else
            {
                newuser444.Role = Helpers.Settings.Town;
            }


            userdetailslist.Add(newuser444);


            //var newuser3 = new User();
            //newuser3.Title = "Height";

            //if (string.IsNullOrEmpty(Helpers.Settings.Height))
            //{
            //    newuser3.Role = "--";
            //}
            //else
            //{
            //    newuser3.Role = Helpers.Settings.Height;
            //}


            //userdetailslist.Add(newuser3);

            //var newuser4 = new User();
            //newuser4.Title = "Weight";


            //if (string.IsNullOrEmpty(Helpers.Settings.Weight))
            //{
            //    newuser4.Role = "--";
            //}
            //else
            //{
            //    newuser4.Role = Helpers.Settings.Weight;
            //}


            foreach (var item in userdetailslist)
            {
                if (item.Title == "Height" || item.Title == "Weight")
                {
                    item.Imagevisible = true;
                }
                else
                {
                    item.Imagevisible = true;
                }
            }

            // userdetailslist.Add(newuser4);

         


            //second profile list
            //  var nu1 = new User();
            //  nu1.Title = "Dash Settings";

            ////  App.AppSettings.AddOrUpdateValue("dashsettings", "");


            //  if (string.IsNullOrEmpty(Helpers.Settings.Dashsettings))
            //  {
            //      nu1.Role = "--";
            //  }
            //  else
            //  {

            //      var a = Helpers.Settings.Dashsettings;

            //      if (a[0].ToString() == ",")
            //      {
            //          var newstring = Helpers.Settings.Dashsettings.Remove(0, 1);

            //          nu1.Role = newstring.Replace(" ", string.Empty);
            //      }
            //      else
            //      {
            //          nu1.Role = Helpers.Settings.Dashsettings.Replace(" ", string.Empty);
            //      }




            //  }



            //  userdetailslist2.Add(nu1);

            var nu2 = new user();
            nu2.Title = "App Usage";


            if (string.IsNullOrEmpty(Helpers.Settings.Appusing))
            {
                nu2.Role = "--";
            }
            else
            {
                nu2.Role = Helpers.Settings.Appusing;
            }



            userdetailslist2.Add(nu2);

            var nu3 = new user();
            nu3.Title = "Communication Preferences";


            if (string.IsNullOrEmpty(Helpers.Settings.userpreferences))
            {
                nu3.Role = "--";
            }
            else
            {
                nu3.Role = Helpers.Settings.userpreferences;
            }



            userdetailslist2.Add(nu3);

            var nu4 = new user();
            nu4.Title = "Signup Code";


            if (string.IsNullOrEmpty(Helpers.Settings.SignUp))
            {
                nu4.Role = "--";
            }
            else
            {
                nu4.Role = Helpers.Settings.SignUp;
            }

            userdetailslist2.Add(nu4);

            var nu5 = new user();
            nu5.Title = "Clinical Trail";

            if (string.IsNullOrEmpty(Helpers.Settings.Clinicaltrial))
            {
                nu5.Role = "--";
            }
            else
            {
                nu5.Role = Helpers.Settings.Clinicaltrial;
            }


            userdetailslist2.Add(nu5);


            secondprofilelist.ItemsSource = userdetailslist2;
            // profilelist.HeightRequest = 95;

            //first items list

            firstitemlist.Add("Personal Details");
            // firstitemlist.Add("Research Studies");
            //firstitemlist.Add("App Settings");
            firstitemlist.Add("Account Settings");
            firstitemlist.Add("Communication Preferences");

            firstlist.ItemsSource = firstitemlist;
            firstlist.HeightRequest = 95;


            // seconditemlist.Add("All Health Insights");
            //seconditemlist.Add("Notifications");

            //  seconditemlist.Add("Health");
            seconditemlist.Add("Password Reset");
            // seconditemlist.Add("Supplement Insights");

            settingslist.ItemsSource = seconditemlist;
            settingslist.HeightRequest = 40;

            thirditemlist.Add("Terms Of Use");
            //thirditemlist.Add("About");

            thirdlist.ItemsSource = thirditemlist;
            thirdlist.HeightRequest = 85;


            privlist.ItemsSource = thirditemlist;
            privlist.HeightRequest = 40;

            var currentVersion = VersionTracking.CurrentVersion;
            var release = VersionTracking.CurrentBuild;

            versionlbl.Text = "Release Version " + release + " | Build Version " + currentVersion;

            useridlbl.Text = Helpers.Settings.UserKey;



        }


        public MainDashboard(bool openprofile)
        {
            InitializeComponent();

            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            incidentsmanager = IncidentsManager.DefaultManager;
            primarycaremanager = PrimaryCareManager.DefaultManager;
            incidenthistorymanager = IncidentHistoryManager.DefaultManager;
            meddirectionsmanager = MedDirectionsManager.DefaultManager;
            advermanager = AdvertManager.DefaultManager;

            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //On<iOS>().SetUseSafeArea(false);

            //tabsview.SelectedIndex = 1;
            //hometab.ImageSource = ImageSource.FromFile("dashgray.png");
            //profiletab.ImageSource = ImageSource.FromFile("profileactive.png");

            // //refresh the queestionnaire and status
            MessagingCenter.Subscribe<object, object>(this, "refreshdashafterquestionnaire", (sender, args) =>
            {
                // Handle the received message

                try
                {


                    var newcollection = args as ObservableCollection<UserQuestionnaire>;

                    if (newcollection != null)
                    {

                        donebtn.IsVisible = true;
                        completebtn.IsVisible = false;

                        completedquestionnaires.Clear();
                        incompletedquestionnaires.Clear();

                        //check if any weeks are missing

                        for (int i = 1; i <= weekNumber; i++)
                        {

                            if (newcollection.Any(x => x.Score == i.ToString()))
                            {
                                //do nothing as its already in
                            }
                            else
                            {
                                //means they have missed the week
                                var newquestionnaire = new UserQuestionnaire();
                                newquestionnaire.Weeknumber = "Week " + i.ToString();
                                newquestionnaire.Completestring = "Incomplete";
                                newquestionnaire.Score = i.ToString();
                                newquestionnaire.Imagename = "incompletequestionnaires.png";

                                Previoususerquestionnaires.Add(newquestionnaire);
                                incompletedquestionnaires.Add(newquestionnaire);
                            }


                        }


                        foreach (var item in newcollection)
                        {


                            item.Weeknumber = "Week " + item.Score;

                            item.Completestring = "Completed";
                            item.Imagename = "completequestionnaires.png";

                            completedquestionnaires.Add(item);

                            Previoususerquestionnaires.Add(item);

                        }


                        completedlbl.Text = completedquestionnaires.Count.ToString();
                        incompletelbl.Text = incompletedquestionnaires.Count.ToString();


                    }

                }
                catch (Exception ex)
                {

                }
            });


            MessagingCenter.Subscribe<object, object>(this, "updateuserdetails", (sender, args) =>
            {
                // Handle the received message

                try
                {
                    //update the dash with the user new details

                    var newcollection = args as user;

                    if (newcollection != null)
                    {
                        userdetailslist.Clear();

                        welcomelbl.Text = "Hi " + Helpers.Settings.FirstName;

                        //profile section


                        namelbl.Text = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

                        if (namelbl.Text == " ")
                        {
                            namelbl.Text = "Your Profile";
                        }

                        //emaillbl.Text = Helpers.Settings.Email;

                        //user details list

                        var newuserr1 = new user();
                        newuserr1.Title = "Name";

                        var fullname0 = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

                        if (fullname0 == " ")
                        {
                            newuserr1.Role = "--";
                        }
                        else
                        {
                            newuserr1.Role = fullname0;
                        }



                        userdetailslist.Add(newuserr1);

                        var newuserrr0 = new user();
                        newuserrr0.Title = "Email";
                        newuserrr0.Role = Helpers.Settings.Email;

                        userdetailslist.Add(newuserrr0);


                        var newuser0 = new user();
                        newuser0.Title = "Date of Birth";

                        if (Helpers.Settings.Age.Contains("00:00:00"))
                        {
                            var n = Helpers.Settings.Age;

                            var nn = n.Replace("00:00:00", string.Empty);
                            newuser0.Role = nn;

                        }
                        else
                        {
                            newuser0.Role = Helpers.Settings.Age;
                        }


                        userdetailslist.Add(newuser0);

                        var newuser10 = new user();
                        newuser10.Title = "Gender";
                        newuser10.Role = Helpers.Settings.Gender;

                        userdetailslist.Add(newuser10);

                        var newuser20 = new user();
                        newuser20.Title = "Ethnicity";
                        newuser20.Role = Helpers.Settings.Ethnicity;

                        userdetailslist.Add(newuser20);

                        var newuser440 = new user();
                        newuser440.Title = "Phone Number";


                        if (string.IsNullOrEmpty(Helpers.Settings.PhoneNumber))
                        {
                            newuser440.Role = "--";
                        }
                        else
                        {
                            newuser440.Role = Helpers.Settings.PhoneNumber;
                        }



                        userdetailslist.Add(newuser440);


                        var newuser4440 = new user();
                        newuser4440.Title = "Town/City";

                        if (string.IsNullOrEmpty(Helpers.Settings.Town))
                        {
                            newuser4440.Role = "--";
                        }
                        else
                        {
                            newuser4440.Role = Helpers.Settings.Town;
                        }


                        userdetailslist.Add(newuser4440);

                        if (Helpers.Settings.Validityconfirmed == "false")
                        {
                            var newuser44401 = new user();
                            newuser44401.Title = "National Health Identifer";
                            newuser44401.Role = Helpers.Settings.Userepid;
                            userdetailslist.Add(newuser44401);
                        }

                        //var newuser30 = new User();
                        //newuser30.Title = "Height";

                        //if (string.IsNullOrEmpty(Helpers.Settings.Height))
                        //{
                        //    newuser30.Role = "--";
                        //}
                        //else
                        //{
                        //    newuser30.Role = Helpers.Settings.Height;
                        //}


                        //userdetailslist.Add(newuser30);

                        //var newuser40 = new User();
                        //newuser40.Title = "Weight";


                        //if (string.IsNullOrEmpty(Helpers.Settings.Weight))
                        //{
                        //    newuser40.Role = "--";
                        //}
                        //else
                        //{
                        //    newuser40.Role = Helpers.Settings.Weight;
                        //}


                        foreach (var item in userdetailslist)
                        {
                            if (item.Title == "Height" || item.Title == "Weight")
                            {
                                item.Imagevisible = true;
                            }
                            else
                            {
                                item.Imagevisible = true;
                            }
                        }

                        // userdetailslist.Add(newuser40);

                        profilelist.ItemsSource = userdetailslist;

                    }

                }
                catch (Exception ex)
                {

                }
            });


            welcomelbl.Text = "Hi " + Helpers.Settings.FirstName;


            getalldata();
            getinfo();

            //profile section
            usermanager = UserManager.DefaultManager;

            namelbl.Text = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

            if (namelbl.Text == " ")
            {
                namelbl.Text = "Your Profile";
            }

            //emaillbl.Text = Helpers.Settings.Email;

            //user details list

            var newuserr = new user();
            newuserr.Title = "Name";

            var fullname = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

            if (fullname == " ")
            {
                newuserr.Role = "--";
            }
            else
            {
                newuserr.Role = fullname;
            }



            userdetailslist.Add(newuserr);

            var newuserrr = new user();
            newuserrr.Title = "Email";
            newuserrr.Role = Helpers.Settings.Email;

            userdetailslist.Add(newuserrr);


            var newuser = new user();
            newuser.Title = "Date of Birth";

            if (Helpers.Settings.Age.Contains("00:00:00"))
            {
                var n = Helpers.Settings.Age;

                var nn = n.Replace("00:00:00", string.Empty);
                newuser.Role = nn;

            }
            else
            {
                newuser.Role = Helpers.Settings.Age;
            }


            userdetailslist.Add(newuser);

            var newuser1 = new user();
            newuser1.Title = "Gender";
            newuser1.Role = Helpers.Settings.Gender;

            userdetailslist.Add(newuser1);

            var newuser2 = new user();
            newuser2.Title = "Ethnicity";
            newuser2.Role = Helpers.Settings.Ethnicity;

            userdetailslist.Add(newuser2);

            var newuser44 = new user();
            newuser44.Title = "Phone Number";


            if (string.IsNullOrEmpty(Helpers.Settings.PhoneNumber))
            {
                newuser44.Role = "--";
            }
            else
            {
                newuser44.Role = Helpers.Settings.PhoneNumber;
            }



            userdetailslist.Add(newuser44);


            var newuser444 = new user();
            newuser444.Title = "Town/City";

            if (string.IsNullOrEmpty(Helpers.Settings.Town))
            {
                newuser444.Role = "--";
            }
            else
            {
                newuser444.Role = Helpers.Settings.Town;
            }


            userdetailslist.Add(newuser444);

            var newuser3 = new user();
            newuser3.Title = "Height";

            if (string.IsNullOrEmpty(Helpers.Settings.Height))
            {
                newuser3.Role = "--";
            }
            else
            {
                newuser3.Role = Helpers.Settings.Height;
            }


            userdetailslist.Add(newuser3);

            var newuser4 = new user();
            newuser4.Title = "Weight";


            if (string.IsNullOrEmpty(Helpers.Settings.Weight))
            {
                newuser4.Role = "--";
            }
            else
            {
                newuser4.Role = Helpers.Settings.Weight;
            }


            foreach (var item in userdetailslist)
            {
                if (item.Title == "Height" || item.Title == "Weight")
                {
                    item.Imagevisible = true;
                }
                else
                {
                    item.Imagevisible = true;
                }
            }

            userdetailslist.Add(newuser4);

            profilelist.ItemsSource = userdetailslist;


            //second profile list
            //  var nu1 = new User();
            //  nu1.Title = "Dash Settings";

            ////  App.AppSettings.AddOrUpdateValue("dashsettings", "");


            //  if (string.IsNullOrEmpty(Helpers.Settings.Dashsettings))
            //  {
            //      nu1.Role = "--";
            //  }
            //  else
            //  {

            //      var a = Helpers.Settings.Dashsettings;

            //      if (a[0].ToString() == ",")
            //      {
            //          var newstring = Helpers.Settings.Dashsettings.Remove(0, 1);

            //          nu1.Role = newstring.Replace(" ", string.Empty);
            //      }
            //      else
            //      {
            //          nu1.Role = Helpers.Settings.Dashsettings.Replace(" ", string.Empty);
            //      }




            //  }



            //  userdetailslist2.Add(nu1);

            var nu2 = new user();
            nu2.Title = "App Usage";


            if (string.IsNullOrEmpty(Helpers.Settings.Appusing))
            {
                nu2.Role = "--";
            }
            else
            {
                nu2.Role = Helpers.Settings.Appusing;
            }



            userdetailslist2.Add(nu2);

            var nu3 = new user();
            nu3.Title = "Communication Preferences";


            if (string.IsNullOrEmpty(Helpers.Settings.userpreferences))
            {
                nu3.Role = "--";
            }
            else
            {
                nu3.Role = Helpers.Settings.userpreferences;
            }



            userdetailslist2.Add(nu3);

            var nu4 = new user();
            nu4.Title = "Signup Code";


            if (string.IsNullOrEmpty(Helpers.Settings.SignUp))
            {
                nu4.Role = "--";
            }
            else
            {
                nu4.Role = Helpers.Settings.SignUp;
            }

            userdetailslist2.Add(nu4);

            var nu5 = new user();
            nu5.Title = "Clinical Trail";

            if (string.IsNullOrEmpty(Helpers.Settings.Clinicaltrial))
            {
                nu5.Role = "--";
            }
            else
            {
                nu5.Role = Helpers.Settings.Clinicaltrial;
            }


            userdetailslist2.Add(nu5);


            secondprofilelist.ItemsSource = userdetailslist2;
            // profilelist.HeightRequest = 95;

            //first items list

            firstitemlist.Add("Personal Details");
            // firstitemlist.Add("Research Studies");
            //firstitemlist.Add("App Settings");
            firstitemlist.Add("Account Settings");
            firstitemlist.Add("Communication Preferences");

            firstlist.ItemsSource = firstitemlist;
            firstlist.HeightRequest = 95;


            // seconditemlist.Add("All Health Insights");
            // seconditemlist.Add("Notifications");

            //  seconditemlist.Add("Health");
            seconditemlist.Add("Password Reset");
            // seconditemlist.Add("Supplement Insights");

            settingslist.ItemsSource = seconditemlist;
            settingslist.HeightRequest = 40;

            thirditemlist.Add("Terms Of Use");
            //thirditemlist.Add("About");

            thirdlist.ItemsSource = thirditemlist;
            thirdlist.HeightRequest = 85;


            privlist.ItemsSource = thirditemlist;
            privlist.HeightRequest = 40;

            var currentVersion = VersionTracking.CurrentVersion;
            var release = VersionTracking.CurrentBuild;

            versionlbl.Text = "Release Version " + release + " | Build Version " + currentVersion;

            useridlbl.Text = Helpers.Settings.UserKey;



        }

        public MainDashboard(ObservableCollection<UserQuestionnaire> userquestionnairepassed, int wnumber)
        {
            InitializeComponent();

            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            incidentsmanager = IncidentsManager.DefaultManager;
            primarycaremanager = PrimaryCareManager.DefaultManager;
            incidenthistorymanager = IncidentHistoryManager.DefaultManager;
            meddirectionsmanager = MedDirectionsManager.DefaultManager;
            advermanager = AdvertManager.DefaultManager;

            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //On<iOS>().SetUseSafeArea(false);


            welcomelbl.Text = "Hi " + Helpers.Settings.FirstName;

            userquestionnaires = userquestionnairepassed;

            weekNumber = wnumber;


            getweeknumberandquestionnaire();
            getinfo();

            //checkincidents();

            //getquestionnaireandquestions();

            donebtn.IsVisible = true;
            completebtn.IsVisible = false;

            completedquestionnaires.Clear();
            Previoususerquestionnaires.Clear();

            //check if any weeks are missing

            for (int i = 1; i <= weekNumber; i++)
            {

                if (userquestionnaires.Any(x => x.Score == i.ToString()))
                {
                    //do nothing as its already in
                }
                else
                {
                    //means they have missed the week
                    var newquestionnaire = new UserQuestionnaire();
                    newquestionnaire.Weeknumber = "Week " + i.ToString();
                    newquestionnaire.Completestring = "Incomplete";
                    newquestionnaire.Score = i.ToString();
                    newquestionnaire.Imagename = "incompletequestionnaires.png";

                    Previoususerquestionnaires.Add(newquestionnaire);
                    incompletedquestionnaires.Add(newquestionnaire);
                }


            }


            foreach (var item in userquestionnaires)
            {


                item.Weeknumber = "Week " + item.Score;

                item.Completestring = "Completed";
                item.Imagename = "completequestionnaires.png";

                completedquestionnaires.Add(item);

                Previoususerquestionnaires.Add(item);

            }


            completedlbl.Text = completedquestionnaires.Count.ToString();
            incompletelbl.Text = incompletedquestionnaires.Count.ToString();


            //profile section
            usermanager = UserManager.DefaultManager;

            namelbl.Text = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

            if (namelbl.Text == " ")
            {
                namelbl.Text = "Your Profile";
            }

            //emaillbl.Text = Helpers.Settings.Email;

            //user details list

            var newuserr = new user();
            newuserr.Title = "Name";

            var fullname = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

            if (fullname == " ")
            {
                newuserr.Role = "--";
            }
            else
            {
                newuserr.Role = fullname;
            }



            userdetailslist.Add(newuserr);

            var newuserrr = new user();
            newuserrr.Title = "Email";
            newuserrr.Role = Helpers.Settings.Email;

            userdetailslist.Add(newuserrr);


            var newuser = new user();
            newuser.Title = "Date of Birth";

            if (Helpers.Settings.Age.Contains("00:00:00"))
            {
                var n = Helpers.Settings.Age;

                var nn = n.Replace("00:00:00", string.Empty);
                newuser.Role = nn;

            }
            else
            {
                newuser.Role = Helpers.Settings.Age;
            }


            userdetailslist.Add(newuser);

            var newuser1 = new user();
            newuser1.Title = "Gender";
            newuser1.Role = Helpers.Settings.Gender;

            userdetailslist.Add(newuser1);

            var newuser2 = new user();
            newuser2.Title = "Ethnicity";
            newuser2.Role = Helpers.Settings.Ethnicity;

            userdetailslist.Add(newuser2);

            var newuser44 = new user();
            newuser44.Title = "Phone Number";


            if (string.IsNullOrEmpty(Helpers.Settings.PhoneNumber))
            {
                newuser44.Role = "--";
            }
            else
            {
                newuser44.Role = Helpers.Settings.PhoneNumber;
            }



            userdetailslist.Add(newuser44);


            var newuser444 = new user();
            newuser444.Title = "Town/City";

            if (string.IsNullOrEmpty(Helpers.Settings.Town))
            {
                newuser444.Role = "--";
            }
            else
            {
                newuser444.Role = Helpers.Settings.Town;
            }


            userdetailslist.Add(newuser444);

            var newuser3 = new user();
            newuser3.Title = "Height";

            if (string.IsNullOrEmpty(Helpers.Settings.Height))
            {
                newuser3.Role = "--";
            }
            else
            {
                newuser3.Role = Helpers.Settings.Height;
            }


            userdetailslist.Add(newuser3);

            var newuser4 = new user();
            newuser4.Title = "Weight";


            if (string.IsNullOrEmpty(Helpers.Settings.Weight))
            {
                newuser4.Role = "--";
            }
            else
            {
                newuser4.Role = Helpers.Settings.Weight;
            }


            foreach (var item in userdetailslist)
            {
                if (item.Title == "Height" || item.Title == "Weight")
                {
                    item.Imagevisible = true;
                }
                else
                {
                    item.Imagevisible = true;
                }
            }

            userdetailslist.Add(newuser4);

            profilelist.ItemsSource = userdetailslist;


            //second profile list
            //  var nu1 = new User();
            //  nu1.Title = "Dash Settings";

            ////  App.AppSettings.AddOrUpdateValue("dashsettings", "");


            //  if (string.IsNullOrEmpty(Helpers.Settings.Dashsettings))
            //  {
            //      nu1.Role = "--";
            //  }
            //  else
            //  {

            //      var a = Helpers.Settings.Dashsettings;

            //      if (a[0].ToString() == ",")
            //      {
            //          var newstring = Helpers.Settings.Dashsettings.Remove(0, 1);

            //          nu1.Role = newstring.Replace(" ", string.Empty);
            //      }
            //      else
            //      {
            //          nu1.Role = Helpers.Settings.Dashsettings.Replace(" ", string.Empty);
            //      }




            //  }



            //  userdetailslist2.Add(nu1);

            var nu2 = new user();
            nu2.Title = "App Usage";


            if (string.IsNullOrEmpty(Helpers.Settings.Appusing))
            {
                nu2.Role = "--";
            }
            else
            {
                nu2.Role = Helpers.Settings.Appusing;
            }



            userdetailslist2.Add(nu2);

            var nu3 = new user();
            nu3.Title = "Communication Preferences";


            if (string.IsNullOrEmpty(Helpers.Settings.userpreferences))
            {
                nu3.Role = "--";
            }
            else
            {
                nu3.Role = Helpers.Settings.userpreferences;
            }



            userdetailslist2.Add(nu3);

            var nu4 = new user();
            nu4.Title = "Signup Code";


            if (string.IsNullOrEmpty(Helpers.Settings.SignUp))
            {
                nu4.Role = "--";
            }
            else
            {
                nu4.Role = Helpers.Settings.SignUp;
            }

            userdetailslist2.Add(nu4);

            var nu5 = new user();
            nu5.Title = "Clinical Trail";

            if (string.IsNullOrEmpty(Helpers.Settings.Clinicaltrial))
            {
                nu5.Role = "--";
            }
            else
            {
                nu5.Role = Helpers.Settings.Clinicaltrial;
            }


            userdetailslist2.Add(nu5);


            secondprofilelist.ItemsSource = userdetailslist2;
            // profilelist.HeightRequest = 95;

            //first items list

            firstitemlist.Add("Personal Details");
            // firstitemlist.Add("Research Studies");
            //firstitemlist.Add("App Settings");
            firstitemlist.Add("Account Settings");
            firstitemlist.Add("Communication Preferences");

            firstlist.ItemsSource = firstitemlist;
            firstlist.HeightRequest = 95;


            // seconditemlist.Add("All Health Insights");
            // seconditemlist.Add("Notifications");

            //  seconditemlist.Add("Health");
            seconditemlist.Add("Password Reset");
            // seconditemlist.Add("Supplement Insights");

            settingslist.ItemsSource = seconditemlist;
            settingslist.HeightRequest = 40;

            thirditemlist.Add("Terms Of Use");
            //thirditemlist.Add("About");

            thirdlist.ItemsSource = thirditemlist;
            thirdlist.HeightRequest = 85;


            privlist.ItemsSource = thirditemlist;
            privlist.HeightRequest = 40;

            var currentVersion = VersionTracking.CurrentVersion;
            var release = VersionTracking.CurrentBuild;

            versionlbl.Text = "Release Version " + release + " | Build Version " + currentVersion;

            useridlbl.Text = Helpers.Settings.UserKey;



        }



        public MainDashboard(string questionnairecompleted)
        {
            InitializeComponent();

            userquestionnairemanager = UserQuestionnaireManager.DefaultManager;
            incidentsmanager = IncidentsManager.DefaultManager;
            primarycaremanager = PrimaryCareManager.DefaultManager;
            incidenthistorymanager = IncidentHistoryManager.DefaultManager;
            meddirectionsmanager = MedDirectionsManager.DefaultManager;
            advermanager = AdvertManager.DefaultManager;

            //Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            //On<iOS>().SetUseSafeArea(false);



            // //refresh the queestionnaire and status

            MessagingCenter.Subscribe<object>(this, "refreshdashafterquestionnaire", (sender) =>
            {
                // Handle the received message

                try
                {
                    getweeknumberandquestionnaire();

                    checkincidents();

                    getquestionnaireandquestions();
                }
                catch (Exception ex)
                {

                }
            });

            //MessagingCenter.Subscribe<object>(this, "refreshdashafterquestionnaire", async (sender) =>
            // {
            //     try
            //     {
            //         getweeknumberandquestionnaire();

            //         checkincidents();

            //         getquestionnaireandquestions();
            //     }
            //     catch (Exception ex)
            //     {

            //     }
            // });



            welcomelbl.Text = "Hi " + Helpers.Settings.FirstName;


            getweeknumberandquestionnaire();

            checkincidents();

            getquestionnaireandquestions();
            getinfo();


            //profile section
            usermanager = UserManager.DefaultManager;

            namelbl.Text = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

            if (namelbl.Text == " ")
            {
                namelbl.Text = "Your Profile";
            }

            //emaillbl.Text = Helpers.Settings.Email;

            //user details list

            var newuserr = new user();
            newuserr.Title = "Name";

            var fullname = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

            if (fullname == " ")
            {
                newuserr.Role = "--";
            }
            else
            {
                newuserr.Role = fullname;
            }



            userdetailslist.Add(newuserr);

            var newuserrr = new user();
            newuserrr.Title = "Email";
            newuserrr.Role = Helpers.Settings.Email;

            userdetailslist.Add(newuserrr);


            var newuser = new user();
            newuser.Title = "Date of Birth";

            if (Helpers.Settings.Age.Contains("00:00:00"))
            {
                var n = Helpers.Settings.Age;

                var nn = n.Replace("00:00:00", string.Empty);
                newuser.Role = nn;

            }
            else
            {
                newuser.Role = Helpers.Settings.Age;
            }


            userdetailslist.Add(newuser);

            var newuser1 = new user();
            newuser1.Title = "Gender";
            newuser1.Role = Helpers.Settings.Gender;

            userdetailslist.Add(newuser1);

            var newuser2 = new user();
            newuser2.Title = "Ethnicity";
            newuser2.Role = Helpers.Settings.Ethnicity;

            userdetailslist.Add(newuser2);

            var newuser44 = new user();
            newuser44.Title = "Phone Number";


            if (string.IsNullOrEmpty(Helpers.Settings.PhoneNumber))
            {
                newuser44.Role = "--";
            }
            else
            {
                newuser44.Role = Helpers.Settings.PhoneNumber;
            }



            userdetailslist.Add(newuser44);


            var newuser444 = new user();
            newuser444.Title = "Town/City";

            if (string.IsNullOrEmpty(Helpers.Settings.Town))
            {
                newuser444.Role = "--";
            }
            else
            {
                newuser444.Role = Helpers.Settings.Town;
            }


            userdetailslist.Add(newuser444);

            //var newuser3 = new User();
            //newuser3.Title = "Height";

            //if (string.IsNullOrEmpty(Helpers.Settings.Height))
            //{
            //    newuser3.Role = "--";
            //}
            //else
            //{
            //    newuser3.Role = Helpers.Settings.Height;
            //}


            //userdetailslist.Add(newuser3);

            //var newuser4 = new User();
            //newuser4.Title = "Weight";


            //if (string.IsNullOrEmpty(Helpers.Settings.Weight))
            //{
            //    newuser4.Role = "--";
            //}
            //else
            //{
            //    newuser4.Role = Helpers.Settings.Weight;
            //}


            foreach (var item in userdetailslist)
            {
                if (item.Title == "Height" || item.Title == "Weight")
                {
                    item.Imagevisible = true;
                }
                else
                {
                    item.Imagevisible = true;
                }
            }

            // userdetailslist.Add(newuser4);

            profilelist.ItemsSource = userdetailslist;


            //second profile list
            //  var nu1 = new User();
            //  nu1.Title = "Dash Settings";

            ////  App.AppSettings.AddOrUpdateValue("dashsettings", "");


            //  if (string.IsNullOrEmpty(Helpers.Settings.Dashsettings))
            //  {
            //      nu1.Role = "--";
            //  }
            //  else
            //  {

            //      var a = Helpers.Settings.Dashsettings;

            //      if (a[0].ToString() == ",")
            //      {
            //          var newstring = Helpers.Settings.Dashsettings.Remove(0, 1);

            //          nu1.Role = newstring.Replace(" ", string.Empty);
            //      }
            //      else
            //      {
            //          nu1.Role = Helpers.Settings.Dashsettings.Replace(" ", string.Empty);
            //      }




            //  }



            //  userdetailslist2.Add(nu1);

            var nu2 = new user();
            nu2.Title = "App Usage";


            if (string.IsNullOrEmpty(Helpers.Settings.Appusing))
            {
                nu2.Role = "--";
            }
            else
            {
                nu2.Role = Helpers.Settings.Appusing;
            }



            userdetailslist2.Add(nu2);

            var nu3 = new user();
            nu3.Title = "Communication Preferences";


            if (string.IsNullOrEmpty(Helpers.Settings.userpreferences))
            {
                nu3.Role = "--";
            }
            else
            {
                nu3.Role = Helpers.Settings.userpreferences;
            }



            userdetailslist2.Add(nu3);

            var nu4 = new user();
            nu4.Title = "Signup Code";


            if (string.IsNullOrEmpty(Helpers.Settings.SignUp))
            {
                nu4.Role = "--";
            }
            else
            {
                nu4.Role = Helpers.Settings.SignUp;
            }

            userdetailslist2.Add(nu4);

            var nu5 = new user();
            nu5.Title = "Clinical Trail";

            if (string.IsNullOrEmpty(Helpers.Settings.Clinicaltrial))
            {
                nu5.Role = "--";
            }
            else
            {
                nu5.Role = Helpers.Settings.Clinicaltrial;
            }


            userdetailslist2.Add(nu5);


            secondprofilelist.ItemsSource = userdetailslist2;
            // profilelist.HeightRequest = 95;

            //first items list

            firstitemlist.Add("Personal Details");
            // firstitemlist.Add("Research Studies");
            //firstitemlist.Add("App Settings");
            firstitemlist.Add("Account Settings");
            firstitemlist.Add("Communication Preferences");

            firstlist.ItemsSource = firstitemlist;
            firstlist.HeightRequest = 95;


            // seconditemlist.Add("All Health Insights");
            //seconditemlist.Add("Notifications");

            //  seconditemlist.Add("Health");
            seconditemlist.Add("Password Reset");
            // seconditemlist.Add("Supplement Insights");

            settingslist.ItemsSource = seconditemlist;
            settingslist.HeightRequest = 40;

            thirditemlist.Add("Terms Of Use");
            //thirditemlist.Add("About");

            thirdlist.ItemsSource = thirditemlist;
            thirdlist.HeightRequest = 85;


            privlist.ItemsSource = thirditemlist;
            privlist.HeightRequest = 40;

            var currentVersion = VersionTracking.CurrentVersion;
            var release = VersionTracking.CurrentBuild;

            versionlbl.Text = "Release Version " + release + " | Build Version " + currentVersion;

            useridlbl.Text = Helpers.Settings.UserKey;



        }

        //Add Notification tags
        
        async void AddNotificationsTags()
        {
            List<string> tags = new List<string>();
            if (!string.IsNullOrEmpty(Helpers.Settings.SignUp))
            {
                tags.Add(Helpers.Settings.SignUp);
            }
            if (!string.IsNullOrEmpty(Helpers.Settings.UserKey))
            {
                tags.Add("TUESDAY");
            }
            if (tags.Count > 0)
            {
                var notificationService = new NotificationService();
                //await notificationService.AddTag(tags);
            }
        }

        async void checknotificationtime()
        {
            try
            {

                if (string.IsNullOrEmpty(Helpers.Settings.CreatedAt))
                {
                    //no notification time selected
                    await Navigation.PushAsync(new SelectNotificationTime(), false);
                    return;
                }

                if (DeviceInfo.Current.Platform == DevicePlatform.Android)
                {
                    //check if notifcations are switched on
                    PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                    // bool isNotificationEnabled = DependencyService.Get<CheckNotifications>().GetApplicationNotificationSettings();
                    if (status == PermissionStatus.Granted)
                    {
                        notificationframe.IsVisible = false;
                    }
                    else
                    {
                        notificationframe.IsVisible = true;
                    }
                }

            }
            catch (Exception ex)
            {
                var s = ex.Message;
                var ss = ex.StackTrace.ToString();

            }
        }

        async void getalldata()
        {
            try
            {
                await getweeknumberandquestionnaire();

                await checkiftheyhavecompletedfullquestionnaire();

                await getquestionnaireandquestions();

                await checkincidents();

                await getconsentdata();
            }
            catch (Exception ex)
            {

            }
        }

        async Task getconsentdata()
        {
            try
            {
                //dont show it if the notification has popped up
                if (string.IsNullOrEmpty(Helpers.Settings.CreatedAt))
                {
                    // dont show
                }
                else
                {
                    if (getad[0].ConsentReminder == true)
                    {
                        //no notification time selected
                        await Navigation.PushAsync(new DashboardConsent(getuser[0]), false);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }


        async void getinfo()
        {
            try
            {
                //get in study name
                getad = await advermanager.GetSpecficAd(Helpers.Settings.SignUp);

                studynamelbl.Text = getad[0].AdvertTitle;

                //get the stufy information
                var getinfo = await meddirectionsmanager.getinfobyreferralcode(Helpers.Settings.SignUp);


                foreach (var item in getinfo)
                {

                    if (item.Type == "PDF")
                    {
                        item.Img = "pdficon.png";
                    }
                    else if (item.Type == "Video")
                    {
                        // item.Img = "greenvideo.png";
                    }
                    else
                    {
                        item.Img = "webicon.png";
                    }


                }



                infolist.ItemsSource = getinfo;

                if (getinfo.Count < 10)
                {
                    infolist.HeightRequest = 1000;
                }



            }
            catch (Exception ex)
            {
                var s = ex.StackTrace.ToString();
                var ss = "ads";
            }
        }

        async Task checkiftheyhavecompletedfullquestionnaire()
        {
            try
            {
                //check if the user has an open incident and update the progress

                var checkincidents = await incidentsmanager.GetSpecficIncident(weekNumber.ToString());
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
                            var finduserquestionnaire = alluserquestionnaires.Where(x => x.Score == weekNumber.ToString()).FirstOrDefault();

                            var deletequestionnaire = userquestionnairemanager.DeleteReason(finduserquestionnaire);

                        }
                    }
                }


            }
            catch (Exception ex)
            {

            }
        }

        async Task checkincidents()
        {
            try
            {
                //get the userquestionnaire data

                //var username = Helpers.Settings.UserKey;

                ////var alluserquestionnaires = await userquestionnairemanager.getUserQuestionnaire();
                ////userquestionnaires = alluserquestionnaires;
                ///

                //check if the user has an open incident and update the progress

                var checkincidents = await incidentsmanager.GetSpecficIncident(weekNumber.ToString());


                //check if they have completed that weeks questionnaire
                if (userquestionnaires != null)
                {

                    if (userquestionnaires.Any(x => x.Score == weekNumber.ToString()))
                    {
                        //if true this means the user has completed the questionnaire for the week


                        donebtn.IsVisible = true;
                        completebtn.IsVisible = false;

                    }
                    else
                    {
                        //show the button for them to complete the questionnaire
                        donebtn.IsVisible = false;
                        completebtn.IsVisible = true;
                    }

                }
                else
                {
                    donebtn.IsVisible = false;
                    completebtn.IsVisible = true;
                }






                Allincidents = await incidentsmanager.Getincidentsforuser();


                if (checkincidents.Count != 0)
                {

                    Userincident = checkincidents[0];

                    //get the doctor details (primary care)

                    var docdetails = await primarycaremanager.GetDoctorListbasedonGPID();

                    if (docdetails.Count != 0)
                    {

                        practnamelbl.Text = docdetails[0].Practicename;
                        practnamelbl2.Text = docdetails[0].Practicename;

                    }


                    //find out how many items are set to true so you can see the progress
                    int countOfTrueColumns = typeof(incidents)
                    .GetProperties()
                    .Count(property => property.GetValue(checkincidents[0]) is bool value && value);

                    if (countOfTrueColumns == 0)
                    {
                        incidientframe.IsVisible = false;
                    }
                    else
                    {
                        incidientframe.IsVisible = true;
                    }

                    if (countOfTrueColumns == 2)
                    {
                        incidentprogress.Progress = 33;
                        progresslbl.Text = "Please collect your sample kit";
                        collectkitstack.IsVisible = true;
                    }
                    else if (countOfTrueColumns == 4)
                    {
                        if (Userincident.Reportacknowledged == true && Userincident.Invitedtocollectkit == true && Userincident.Kitcollectedgp == true && Userincident.Kitreturnedgp == true)
                        {
                            //show nothing as the gp as updated the db not the user
                        }
                        else
                        {
                            incidentprogress.Progress = 66;
                            progresslbl.Text = "Please return sample to GP";
                            doclationstack.IsVisible = true;
                        }
                    }
                    else
                    {
                        incidientframe.IsVisible = false;
                        // incidentprogress.Progress = 40;
                        //progresslbl.Text = "Kit Returned";
                    }
                    //else if (countOfTrueColumns == 4)
                    //{
                    //    incidentprogress.Progress = 60;
                    //    progresslbl.Text = "Kit Collected";
                    //}
                    //else if (countOfTrueColumns == 5)
                    //{
                    //    incidentprogress.Progress = 80;
                    //    progresslbl.Text = "Kit Returned to Patient";
                    //}
                    //else if (countOfTrueColumns == 6)
                    //{
                    //    incidentprogress.Progress = 100;
                    //    progresslbl.Text = "Collect Kit";
                    //}


                }
                else
                {
                    //hide the progress frame
                    incidientframe.IsVisible = false;
                }

            }
            catch (Exception ex)
            {

            }
        }

        async Task getquestionnaireandquestions()
        {
            try
            {
                completedquestionnaires.Clear();
                incompletedquestionnaires.Clear();

                var username = Helpers.Settings.UserKey;

                var alluserquestionnaires = await userquestionnairemanager.getUserQuestionnaire();

                userquestionnaires = alluserquestionnaires;


                //check if any weeks are missing

                for (int i = 1; i <= weekNumber; i++)
                {

                    if (alluserquestionnaires.Any(x => x.Score == i.ToString()))
                    {
                        //do nothing as its already in
                    }
                    else
                    {
                        //means they have missed the week
                        var newquestionnaire = new UserQuestionnaire();
                        newquestionnaire.Weeknumber = "Week " + i.ToString();
                        newquestionnaire.Completestring = "Incomplete";
                        newquestionnaire.Score = i.ToString();
                        newquestionnaire.Imagename = "incompletequestionnaires.png";

                        Previoususerquestionnaires.Add(newquestionnaire);
                        incompletedquestionnaires.Add(newquestionnaire);
                    }


                }


                foreach (var item in alluserquestionnaires)
                {


                    item.Weeknumber = "Week " + item.Score;

                    item.Completestring = "Completed";
                    item.Imagename = "completequestionnaires.png";

                    completedquestionnaires.Add(item);

                    Previoususerquestionnaires.Add(item);

                }


                completedlbl.Text = completedquestionnaires.Count.ToString();
                incompletelbl.Text = incompletedquestionnaires.Count.ToString();





            }
            catch (Exception ex)
            {

            }
        }


        async Task getweeknumberandquestionnaire()
        {
            try
            {
                //get the user details for the activation date time

                getuser = await usermanager.getUserInfo(Helpers.Settings.UserKey);


                Preferences.Set("validityconfirmed", getuser[0].ValidityConfirmed);
                Preferences.Set("userepid", getuser[0].Epid);



                if (Helpers.Settings.Validityconfirmed == "false")
                {
                    var exists = userdetailslist.Where(u => u.Title == "National Health Identifier").FirstOrDefault();

                    if (exists != null)
                    {
                        userdetailslist.Remove(exists);

                        var newuser44401 = new user();
                        newuser44401.Title = "National Health Identifer";
                        newuser44401.Role = Helpers.Settings.Userepid;
                        userdetailslist.Add(newuser44401);
                    }
                    else
                    {
                        var newuser44401 = new user();
                        newuser44401.Title = "National Health Identifer";
                        newuser44401.Role = Helpers.Settings.Userepid;
                        userdetailslist.Add(newuser44401);
                    }
                }

                profilelist.ItemsSource = userdetailslist;


                //find out what week the user is on
                DateTime userCreationDate = DateTime.Parse(getuser[0].ActivationDT);

                //DateTime userCreationDate = DateTime.Parse(Helpers.Settings.CreatedAt);
                DateTime currentDate = DateTime.Now;

                var plus7days = userCreationDate.AddDays(7);

                weekNumber = (int)Math.Ceiling(currentDate.Subtract(plus7days).TotalDays / 7);

                if (weekNumber < 0)
                {
                    weekNumber = 0;
                }
          
                // Calculate the difference in days

                int differenceInDays = (int)(plus7days - DateTime.Now).TotalDays;

                //add 7 days to created at date to see if zero week has to be shown


           

                // Check if the absolute difference is less than or equal to 7 days
                if (DateTime.Now < plus7days)
                {
                    weekzeroframe.IsVisible = true;
                    mainquestionview.IsVisible = false;
                    progresstextlbl.IsVisible = false;
                    progressgrid.IsVisible = false;
                }
                else if (weekNumber > 52)
                {
                    
                    finishtextlbl.Text = "Thank you for taking part in The third study of infectious intestinal disease in the UK (IID3). You have now completed the study. Your help and participation is greatly appreciated. You can now close the app. If you believe this to be an error, please contact: support@peoplewith.com";
                    mainquestionview.IsVisible = false;
                    fulbl.IsVisible = false;
                    timergrid.IsVisible = false;
                    finishedframe.IsVisible = true;
                    weekNumber = 52;
                }
                else
                {

                    weekzeroframe.IsVisible = false;
                    mainquestionview.IsVisible = true;
                }


                //find out the timer time to the next questionnaire
                DateTime nextQuestionnaireDate = plus7days.AddDays((weekNumber) * 7);
                timeUntilNextQuestionnaire = nextQuestionnaireDate - currentDate;


                weeknumlbl.Text = "Week " + weekNumber.ToString() + " of 52";
                weeknumlbl2.Text = "Week " + weekNumber.ToString() + " of 52";

                //set the progress
                double progress = (double)weekNumber / 52;

                double progrosspercent = progress * 100;

                topprogress.Progress = progrosspercent;

                // Start the timer
                // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                //Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                //{


                // Update the label text with the remaining time
                //  timerlbl.Text = $"Time remaining: {timeRemaining.Days} :, {timeRemaining.Hours} :, {timeRemaining.Minutes} :, {timeRemaining.Seconds}";

                dayslbl.Text = timeUntilNextQuestionnaire.Days.ToString("D2");
                    hourslbl.Text = timeUntilNextQuestionnaire.Hours.ToString("D2");
                    minuteslbl.Text = timeUntilNextQuestionnaire.Minutes.ToString("D2");


                // Return true to continue the timer, or false to stop it
                //    return timeUntilNextQuestionnaire > TimeSpan.Zero;
                //});

                androidnotificationadd();


            }
            catch (Exception ex)
            {

                var e = ex.StackTrace.ToString();
                var s = "sds";

            }
        }




        void tabsview_TabItemTapped(System.Object sender, TabItemTappedEventArgs e)
        {
            try
            {
                if (e.TabItem.Header == "Home")
                {
                    hometab.ImageSource = ImageSource.FromFile("dashblacknew.png");

                    //moretab.ImageSource = ImageSource.FromFile("dashboardmenuinactivedots.png");

                    //  reportstab.ImageSource = ImageSource.FromFile("reportsinactive.png");
                    infotab.ImageSource = ImageSource.FromFile("infoinactive.png");
                    profiletab.ImageSource = ImageSource.FromFile("profileinactive.png");
                }
                else if (e.TabItem.Header == "Reports")
                {
                    //hometab.ImageSource = ImageSource.FromFile("dashboardhomeactive.png");
                    hometab.ImageSource = ImageSource.FromFile("dashgray.png");

                    //   reportstab.ImageSource = ImageSource.FromFile("reportsactive.png");
                    infotab.ImageSource = ImageSource.FromFile("infoinactive.png");
                    profiletab.ImageSource = ImageSource.FromFile("profileinactive.png");
                }
                else if (e.TabItem.Header == "Study Information")
                {
                    hometab.ImageSource = ImageSource.FromFile("dashgray.png");

                    //  reportstab.ImageSource = ImageSource.FromFile("reportsinactive.png");
                    infotab.ImageSource = ImageSource.FromFile("infoactive.png");
                    profiletab.ImageSource = ImageSource.FromFile("profileinactive.png");
                }
                else
                {
                    hometab.ImageSource = ImageSource.FromFile("dashgray.png");

                    //  reportstab.ImageSource = ImageSource.FromFile("reportsinactive.png");
                    infotab.ImageSource = ImageSource.FromFile("infoinactive.png");
                    profiletab.ImageSource = ImageSource.FromFile("profileactive.png");
                }
            }
            catch (Exception ex)
            {

            }
        }

        void TapGestureRecognizer_Tapped_Profile(System.Object sender, System.EventArgs e)
        {
            //PROFILE ICON TAPPED
            try
            {

                tabsview.SelectedIndex = 2;
                hometab.ImageSource = ImageSource.FromFile("dashgray.png");
                infotab.ImageSource = ImageSource.FromFile("infoinactive.png");
                profiletab.ImageSource = ImageSource.FromFile("profileactive.png");

            }
            catch
            {

            }
        }


        async void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            //logout tapped
            Analytics.TrackEvent("Logout Clicked");

            var result = await DisplayAlert("Are you sure", "Are you sure you want to log out?", "Cancel", "Yes");

            if (result == false)
            {
                //Clear Notifiation Tags on Logout
                var notificationService = new NotificationService();
                await notificationService.ClearTagsAsync();

                await Navigation.PushAsync(new Logout(), false);

            }
            else
            {
                return;
            }
        }

        async void TapGestureRecognizer_Tapped_1(System.Object sender, System.EventArgs e)
        {
            try
            {
                //app feedback tapped
                Analytics.TrackEvent("Give Feedback Clicked");

                //  await Xamarin.Essentials.Email.ComposeAsync("App Feedback", "", "support@peoplewith.com");

                var message = new EmailMessage
                {
                    Subject = "App Feedback",
                    Body = "",
                    To = { "support@peoplewith.com" }
                };

                await Email.ComposeAsync(message);
            }
            catch(Exception ex)
            {

            }

        }

        async void thirdlist_ItemTapped(System.Object sender, ItemTappedEventArgs e)
        {
            Analytics.TrackEvent("Terms of Use Clicked");

            var item = (string)e.Item;

            if (item == "Terms Of Use")
            {
                await Navigation.PushAsync(new PrivacyPolicyPage(), false);
            }
            else
            {
                //about
                var truee = true;
                //await Navigation.PushAsync(new AppWalkthrough(truee), false);
            }



        }


        async void firstlist_ItemTapped(System.Object sender, ItemTappedEventArgs e)
        {
            //settings

            //
            if (firstlist.SelectedItem == "Account Settings")
            {
                // await Navigation.PushAsync(new NewUserProfile(), false);

            }
            else if (firstlist.SelectedItem == "Personal Details")
            {
                // await Navigation.PushAsync(new NewSettings(), false);

            }
            else if (firstlist.SelectedItem == "Communication Preferences")
            {
                //await Navigation.PushAsync(new CommPreferences(collectuserpref), false);
            }

        }

        async void secondlist_ItemTapped(System.Object sender, ItemTappedEventArgs e)
        {
            var item = (string)e.Item;

            if (item == "Symptom Insights")
            {
                // await Navigation.PushAsync(new SymptomsInsights(), false);
            }
            else
            {
                //about
                // await Navigation.PushAsync(new MedicationInsights(), false);
            }
        }

        async void profilelist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as user;

                if (item.Title == "Height" || item.Title == "Weight")
                {
                    await Navigation.PushAsync(new ProfileEdit(item));
                }
                else
                {
                    await Navigation.PushAsync(new ProfileEdit(item));
                }

            }
            catch (Exception ex)
            {
                var s = ex.Message.ToString();
                var ss = ex.StackTrace.ToString();
            }
        }

        async void TapGestureRecognizer_Tapped_2(System.Object sender, System.EventArgs e)
        {
            try
            {
                // Track event
             //  Analytics.TrackEvent("Give Feedback Clicked");
                var message = new EmailMessage
                {
                    Subject = "App Feedback",
                    Body = "",
                    To = new List<string> { "support@peoplewith.com" }
                };
                await Email.Default.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Email is not supported on this device
                Console.WriteLine(fnsEx);
                await DisplayAlert("Apologies", "Email Client is not supported on this device.", "OK");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine(ex);
                await DisplayAlert("Error", "An error occurred while trying to send the email.", "OK");
            }
        }

        async void settingslist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as string;

                await Navigation.PushAsync(new ProfileEdit(item));

            }
            catch (Exception ex)
            {

            }
        }

        async void privlist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                Analytics.TrackEvent("Terms of Use Clicked");

                var item = (string)e.DataItem;

                string hidetoolbar = "yes";

                if (item == "Terms Of Use")
                {
                    await Navigation.PushAsync(new PrivacyPolicyPage(), false);
                }
                else
                {
                    //about
                    //var truee = true;
                    //await Navigation.PushAsync(new AppWalkthrough(truee), false);
                }

            }
            catch (Exception ex)
            {

            }
        }

        async void TapGestureRecognizer_Tapped_3(System.Object sender, System.EventArgs e)
        {
            try
            {
                Analytics.TrackEvent("Withdrawn from study clicked");




                //await Navigation.PushAsync(new WithdrawVideoPopUp(), false);
                //return;


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
                        Analytics.TrackEvent("User withdrawn from study");

                        //get user details 
                        var deleteuser = await usermanager.getUserInfo(Helpers.Settings.UserKey);


                        if (deleteuser.Count != 0)
                        {
                            var d = DateTime.Now.ToString("s");

                            deleteuser[0].Title = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            deleteuser[0].RegStatus = "Withdrawn";
                        }


                        // await usermanager.DeleteUser(passedUserPD);

                        //Clear Notifiation Tags on Logout
                        var notificationService = new NotificationService();
                        await notificationService.ClearTagsAsync();

                        await usermanager.AddUser(deleteuser[0]);
                        await Navigation.PushAsync(new Logout("withdrawn"), false);
                        // Navigation.RemovePage(this);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        async void TapGestureRecognizer_Tapped_4(System.Object sender, System.EventArgs e)
        {

            try
            {
                //copy userid to clipboard

                await DisplayAlert("Copied to Clipboard", "PeopleWith Participant Identifier has been copied to your clipboard", "Ok");
                await Clipboard.SetTextAsync(Helpers.Settings.UserKey);

            }
            catch (Exception ex)
            {

            }

        }

        async void completebtn_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {


                Analytics.TrackEvent("Start Questionnaire Button on Dash Clicked");

                //complete questionnaire , takes you to the questionnaire

                //pass through signup code , userquestionnaireid and week number to match notification

                var categoryDetails = "question|IID3|questionnaire|425A4FE9-079D-48B2-8922-1DB6AC00FBE4|Week Number: " + weekNumber.ToString();

                var splitcategory = categoryDetails.Split('|');

                await Navigation.PushAsync(new NotificationQuestion(splitcategory, userquestionnaires, weekNumber), false);




            }
            catch (Exception ex)
            {

            }
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                //collected kit button
                Analytics.TrackEvent("Incidents - Kit Collected Clicked");

                //update the incient

                Userincident.Kitcollectedgp = true;
                Userincident.Kitcollectedpatient = true;

                await incidentsmanager.Addincidents(Userincident);


                //update the incident history table


                var newincident = new incidentHistory();


                newincident.Incidentid = Userincident.Id;
                newincident.Textnotes = "Sample Kit Collected";
                newincident.Userid = Helpers.Settings.UserKey;
                newincident.Username = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

                await incidenthistorymanager.AddincidentHistory(newincident);

                progresslbl.Text = "Please return the test kit to GP";
                //update the progress bar and status

                incidentprogress.Progress = 66;
                collectkitstack.IsVisible = false;
                doclationstack.IsVisible = true;



            }
            catch (Exception ex)
            {

            }
        }

        async void TapGestureRecognizer_Tapped_5(System.Object sender, System.EventArgs e)
        {
            //view all questionnaires

            try
            {

                await Navigation.PushAsync(new AllQuestionnaires(weekNumber, userquestionnaires, Allincidents), false);


            }
            catch (Exception ex)
            {

            }


        }

        async void Button_Clicked_1(System.Object sender, System.EventArgs e)
        {
            try
            {

                Analytics.TrackEvent("Incidents - Sample Kit Returned Clicked");

                Userincident.Kitreturnedgp = true;
                Userincident.Kitcollectedpatient = true;

                await incidentsmanager.Addincidents(Userincident);

                //update the incident history table


                var newincident = new incidentHistory();


                newincident.Incidentid = Userincident.Id;
                newincident.Textnotes = "Sample Kit Returned";
                newincident.Userid = Helpers.Settings.UserKey;
                newincident.Username = Helpers.Settings.FirstName + " " + Helpers.Settings.Surname;

                await incidenthistorymanager.AddincidentHistory(newincident);


                //update the progress bar and status

                progresslbl.Text = "Thank you for returning the test kit";

                incidentprogress.Progress = 99;

                doclationstack.IsVisible = false;
                finishedstack.IsVisible = true;


            }
            catch (Exception ex)
            {

            }
        }

        void Button_Clicked_2(System.Object sender, System.EventArgs e)
        {
            try
            {
                Analytics.TrackEvent("Incidents - Close incidents frame clicked");

                incidientframe.IsVisible = false;
            }
            catch (Exception ex)
            {

            }
        }

        async void infolist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                Analytics.TrackEvent("Study Information Clicked");

                var item = e.DataItem as meddirections;

                if (item.Type == "PDF")
                {
                    var pdflink = "https://peoplewithappiamges.blob.core.windows.net/appimages/appimages/" + item.Filename;

                    await Browser.OpenAsync(pdflink, new BrowserLaunchOptions
                    {
                        LaunchMode = BrowserLaunchMode.SystemPreferred,
                        TitleMode = BrowserTitleMode.Hide
                    });


                }
                //else if (item.Type == "Video")
                //{
                //    //var pdflink = "https://peoplewithappiamges.blob.core.windows.net/appimages/appimages/" + item.Filename;

                //    //string imgPath = pdflink + ".mp4";

                //    //var launchvid = new videosupport();

                //    //launchvid.URL = item.Filename;

                //    //await Navigation.PushModalAsync(new AndroidSingleView(launchvid));
                //}
                else
                {



                    await Browser.OpenAsync(item.Weblink, BrowserLaunchMode.SystemPreferred);
                }



            }
            catch (Exception ex)
            {

            }
        }


        // Show the popup for the user to withdraw when the message is received
        private async void ShowPopup()
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
                        Analytics.TrackEvent("User withdrawn from study");
                        //get user details 
                        var deleteuser = await usermanager.getUserInfo(Helpers.Settings.UserKey);


                        if (deleteuser.Count != 0)
                        {
                            var d = DateTime.Now.ToString("s");

                            deleteuser[0].Title = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            deleteuser[0].RegStatus = "Withdrawn";
                        }


                        // await usermanager.DeleteUser(passedUserPD);

                        await usermanager.AddUser(deleteuser[0]);
                        await Navigation.PushAsync(new Logout("withdrawn"), false);
                        // Navigation.RemovePage(this);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        async void Button_Clicked_3(System.Object sender, System.EventArgs e)
        {
            //turn on notifications button

            try
            {
                if (DeviceInfo.Current.Platform == DevicePlatform.Android)
                {

                    await Permissions.RequestAsync<Permissions.PostNotifications>();

                    //check if notifcations are switched on
                    PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                    // bool isNotificationEnabled = DependencyService.Get<CheckNotifications>().GetApplicationNotificationSettings();
                    if (status == PermissionStatus.Granted)
                    {
                        notificationframe.IsVisible = false;
                    }
                    else
                    {
                        notificationframe.IsVisible = true;
                    }
                }

                // Request notification permissions using DependencyService
                //bool permissionsGranted = await DependencyService.Get<INotificationPermissionService>().RequestNotificationPermissions();

                //if (permissionsGranted)
                //{
                //    // Notification permissions granted
                //    // You can update UI or perform additional actions here
                //    notificationframe.IsVisible = false;
                //}
                //else
                //{
                //    // Notification permissions not granted
                //    // You can handle this case accordingly
                //    notificationframe.IsVisible = false;
                //}


            }
            catch (Exception ex)
            {
                var s = ex.StackTrace.ToString();
                var ss = "sds";
            }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                //find out what week the user is on
                DateTime userCreationDate = DateTime.Parse(getuser[0].ActivationDT);

                //DateTime userCreationDate = DateTime.Parse(Helpers.Settings.CreatedAt);
                DateTime currentDate = DateTime.Now;

                var plus7days = userCreationDate.AddDays(7);

                weekNumber = (int)Math.Ceiling(currentDate.Subtract(plus7days).TotalDays / 7);

                if (weekNumber < 0)
                {
                    weekNumber = 0;
                }
                // Calculate the difference in days

                int differenceInDays = (int)(plus7days - DateTime.Now).TotalDays;

                //add 7 days to created at date to see if zero week has to be shown



                // Check if the absolute difference is less than or equal to 7 days
                if (DateTime.Now < plus7days)
                {
                    weekzeroframe.IsVisible = true;
                    mainquestionview.IsVisible = false;
                    progresstextlbl.IsVisible = false;
                    progressgrid.IsVisible = false;
                }
                else
                {

                    weekzeroframe.IsVisible = false;
                    mainquestionview.IsVisible = true;
                }


                //find out the timer time to the next questionnaire
                DateTime nextQuestionnaireDate = plus7days.AddDays((weekNumber) * 7);
                TimeSpan timeUntilNextQuestionnaire = nextQuestionnaireDate - currentDate;


                weeknumlbl.Text = "Week " + weekNumber.ToString() + " of 52";
                weeknumlbl2.Text = "Week " + weekNumber.ToString() + " of 52";

                //set the progress
                double progress = (double)weekNumber / 52;

                topprogress.Progress = progress;

                // Start the timer
                // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                //Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                //{


                // Update the label text with the remaining time
                //  timerlbl.Text = $"Time remaining: {timeRemaining.Days} :, {timeRemaining.Hours} :, {timeRemaining.Minutes} :, {timeRemaining.Seconds}";

                dayslbl.Text = timeUntilNextQuestionnaire.Days.ToString("D2");
                    hourslbl.Text = timeUntilNextQuestionnaire.Hours.ToString("D2");
                    minuteslbl.Text = timeUntilNextQuestionnaire.Minutes.ToString("D2");


                    // Return true to continue the timer, or false to stop it
                //    return timeUntilNextQuestionnaire > TimeSpan.Zero;
                //});
            }
            catch(Exception ex)
            {

            }

        }

        private async void Button_Clicked_4(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new MainDashboard(), false);
                Navigation.RemovePage(this);
            }
            catch (Exception ex)
            {
            }
        }

        async void androidnotificationadd()
        {
            try
            {

                if (DeviceInfo.Current.Platform == DevicePlatform.Android)
                {

                    if (Helpers.Settings.AndroidNote == "firstreg")
                    {

                        Preferences.Set("androidnotekey", "done");

                        await Permissions.RequestAsync<Permissions.PostNotifications>();

                        setupweeklynotifications();
                    }

                }

            }
            catch(Exception ex)
            {

            }
        }

        private async void OnConnectivityChanged(object sender, bool isConnected)
        {
            if (!isConnected)
            {
                await Navigation.PushAsync(new LostConnectionPage(), false);
            }
        }

        async void checknetwork()
        {
            try
            {
                NetworkAccess accessType = Connectivity.Current.NetworkAccess;

                if (accessType != NetworkAccess.Internet)
                {
                    await Navigation.PushAsync(new LostConnectionPage(), false);
                    Navigation.RemovePage(this);
                    return;
                }
            }
            catch(Exception ex)
            {

            }
        }

        async void setupweeklynotifications()
        {
            try
            {
                //calaulte the time to start notification
                // Parse the string day

                var sd = DateTime.Parse(Helpers.Settings.CreatedAt);
                var currentDate = DateTime.Now;

                var datetimefornotifcation = currentDate + timeUntilNextQuestionnaire;

                var notdescription = "Tap here to complete the IID3 Weekly Follow-up.";

                var title = "IID3 Study Weekly Follow - up - Week: " + weekNumber;
                Random random = new Random();
                int randomId = random.Next(100000, 99999999);

                var notification = new NotificationRequest
                {
                    NotificationId = randomId,
                    Title = title,
                    Description = notdescription,
                    BadgeNumber = 0,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = datetimefornotifcation,
                        RepeatType = NotificationRepeat.Weekly,
                        NotifyRepeatInterval = null
                    }
                };
                LocalNotificationCenter.Current.Show(notification);

            }
            catch (Exception ex)
            {

            }
        }
    }
}

