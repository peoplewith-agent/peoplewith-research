using CommunityToolkit.Mvvm.Messaging;
using FreakyKit.Utils;
using Microsoft.AppCenter.Analytics;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Networking;
using Mopups.Services;
using Newtonsoft.Json;
using PeopleWithResearch;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PeopleWithResearch
{
    public partial class NewProfileEdit : ContentPage
    {

        UserManager usermanger;
        public ObservableCollection<user> userDetails = new();
        public List<string> ethnicitylist = new List<string>();
        public List<string> genlist = new List<string>();
        public List<string> clintraillist = new List<string>();
        string gendertext;
        string ethtext;
        string usingtext;
        string cttext;

        public ObservableCollection<object> heightall = new ObservableCollection<object>();

        public ObservableCollection<object> heightft = new ObservableCollection<object>();
        public ObservableCollection<object> heightin = new ObservableCollection<object>();
        ObservableCollection<int> selectedIndex = new ObservableCollection<int>() { 1, 2 };

        public ObservableCollection<object> weightall = new ObservableCollection<object>();

        public ObservableCollection<object> weightkg = new ObservableCollection<object>();
        public ObservableCollection<object> weightlbs = new ObservableCollection<object>();

        public List<string> dashlist = new List<string>();
        public List<string> customdashboardlist = new List<string>();
        public List<string> usinglist = new List<string>();

        public List<string> communicationpreflistselected = new List<string>();
        public List<string> communicationpreflist = new List<string>();

        public ObservableCollection<advert> checksignupcodes = new ObservableCollection<advert>();
        public AdvertManager advertmanager;

        public UserConsentManager userconsentmanger;
        public ObservableCollection<userconsent> consentdata = new ObservableCollection<userconsent>();
        public ObservableCollection<userconsent> userconsentdata = new ObservableCollection<userconsent>();


        public DateTime dateoffire = new DateTime();
        public int numday;
        public int daycount;
        public DateTime dateoffirenotday;
        public DateTime dateandtimefornotification;


        public ObservableCollection<object> heightunit = new ObservableCollection<object>();
        public ObservableCollection<Customheightandweight> heightftlist = new ObservableCollection<Customheightandweight>();
        public ObservableCollection<Customheightandweight> heightcmlist = new ObservableCollection<Customheightandweight>();
        public string heightunitvalue;

        public ObservableCollection<object> weightunit = new ObservableCollection<object>();
        public ObservableCollection<Customheightandweight> weightkglist = new ObservableCollection<Customheightandweight>();
        public ObservableCollection<Customheightandweight> weightstonelist = new ObservableCollection<Customheightandweight>();
        public string weightunitvalue;
        public user passeduser;
        static Regex ValidEmailRegex = CreateValidEmailRegex();
        public NewProfileEdit()
        {
            InitializeComponent();
        }

        public NewProfileEdit(user itempassed)
        {
            InitializeComponent();
            passeduser = itempassed;
            GetUserDetails();

            // Use Id (stable English key) so routing works regardless of the UI language
            switch (passeduser.Id ?? passeduser.Title?.ToString())
            {
                //Name Stack
                case "Name":
                    NameStack();
                    break;

                //Email Stack
                case "Email":
                    EmailStack();
                    break;

                //DOB Stack
                case "Date of Birth":
                    DobStack();
                    break;

                //Gender Stack
                case "Gender":
                    GenStack();
                    break;

                //Ethnicity Stack
                case "Ethnicity":
                    EthStack();
                    break;

                //Number Stack
                case "Phone Number":
                    NumberStack();
                    break;

                //Town Stack
                case "Town/City":
                    TownStack();
                    break;

                //NHI Stack
                case "National Health Identifier":
                    EthIDStack();
                    break;

                //Reset Password Stack
                case "Rest Password":
                case "Reset Password":
                    RestPasswordStack();
                    break;

            }



            //weight
            weightunit.Add("kg");
            weightunit.Add("st lbs");
            segmentedcontrolweight.ItemsSource = weightunit;
            // segmentedcontrolweight.SelectedItem = weightunit[0];

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

            if (!string.IsNullOrEmpty(Helpers.Settings.Weight))
            {
                if (Helpers.Settings.Weight.Contains("kg"))
                {
                    weightunitvalue = "kg";
                    weightlist.ItemsSource = weightkglist;
                    segmentedcontrolweight.SelectedItem = weightunit[0];
                }
                else
                {
                    weightunitvalue = "st lbs";
                    // weightunittxt.Text = "Stones & Pounds";
                    weightlist.ItemsSource = weightstonelist;
                    segmentedcontrolweight.SelectedItem = weightunit[1];
                }


            }
            else
            {
                weightunitvalue = "kg";
                weightlist.ItemsSource = weightkglist;
            }

        }

        private async void NameStack()
        {
            try
            {
                Titlelbl.Text = "Edit Name"; 
                btnmain.Text = "Update Name";
                NameEdit.IsVisible = true;
                FirstNameTxt.Text = Helpers.Settings.FirstName;
                SurNameTxt.Text = Helpers.Settings.Surname;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "NameStack");
            }
        }


        private async void EmailStack()
        {
            try
            {
                Titlelbl.Text = "Edit Email";
                btnmain.Text = "Update Email";
                emailstack.IsVisible = true;
                emailregtxt.Text = Helpers.Settings.Email;

            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "EmailStack");
            }
        }

        private async void DobStack()
        {
            try
            {
                DateTime MinDate = DateTime.Now.AddYears(-100);
                daypickernew.MinimumDate = MinDate.Date;
                daypickernew.MaximumDate = DateTime.Now.AddDays(-1).Date;

                Titlelbl.Text = "Edit Date of Birth";
                btnmain.Text = "Update Date of Birth";
                dobstack.IsVisible = true;
                DayPickerStack.IsVisible = true;

                if (string.IsNullOrWhiteSpace(Helpers.Settings.Age))
                {
                    // If empty, set to the MaximumDate (yesterday)
                    daypickernew.SelectedDate = daypickernew.MaximumDate;
                }
                else
                {
                    // If not empty, parse the existing age
                    if (DateTime.TryParse(Helpers.Settings.Age, out DateTime dateofbirth))
                    {
                        daypickernew.SelectedDate = dateofbirth;
                    }
                    else
                    {
                        // Fallback if the string exists but isn't a valid date
                        daypickernew.SelectedDate = daypickernew.MaximumDate;
                    }
                }

            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "DobStack");
            }
        }

        private async void EthStack()
        {
            try
            {
                Titlelbl.Text = "Edit Ethnicity";
                btnmain.Text = "Update Ethnicity";
                ethstack.IsVisible = true;
                //ethnicity
                ethnlist.ItemsSource = Genericlist.EthnicityOptions();

                var Eth = Helpers.Settings.Ethnicity; 
                if (!String.IsNullOrEmpty(Eth))
                {
                    ethnlist.SelectedItem = Eth;
                }
               
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "EthStack");
            }
        }

        private async void GenStack()
        {
            try
            {
                Titlelbl.Text = "Edit Gender";
                btnmain.Text = "Update Gender";
                genderstack.IsVisible = true;
                //Gender
                genlist = Genericlist.GenderOptions;
                genderlist.ItemsSource = genlist; 
                gendertext = Helpers.Settings.Gender;
                if (!string.IsNullOrEmpty(gendertext))
                {
                    if (!genlist.Contains(gendertext))
                    {
                        genderlist.SelectedItem = "Prefer to self-describe";
                        OtherGen.IsVisible = true;
                        GenderTxt.Text = gendertext;
                    }
                    else
                    {
                        genderlist.SelectedItem = gendertext;
                    }
                }

            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "GenStack");
            }
        }

        private async void NumberStack()
        {
            try
            {
                Titlelbl.Text = "Edit Number";
                btnmain.Text = "Update Number";
                phonestack.IsVisible = true;
                mobtxt.Text = Helpers.Settings.PhoneNumber;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "NumberStack");
            }
        }

        private async void TownStack()
        {
            try
            {
                Titlelbl.Text = "Edit Town/City";
                btnmain.Text = "Update Town/City";
                townstack.IsVisible = true;
                towntxt.Text = Helpers.Settings.Town;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "TownStack");
            }
        }

        private async void EthIDStack()
        {
            try
            {
                Titlelbl.Text = "Edit National Health Identifier";
                btnmain.Text = "Update N.H.I";
                epidstack.IsVisible = true;
                epidtxt.Text = Helpers.Settings.Userepid;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "EthIDStack");
            }
        }

        private async void RestPasswordStack()
        {
            try
            {
                Titlelbl.Text = "Reset Password";
                btnmain.Text = "Check Password";
                epidstack.IsVisible = true;
                epidtxt.Text = Helpers.Settings.Userepid;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "RestPasswordStack");
            }
        }


        private async void GetUserDetails()
        {
            try
            {
                var USER = await APICalls.Instance.GetuserDetails(Helpers.Settings.UsersID);
                userDetails = USER != null ? USER : new ObservableCollection<user>();
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "GetUserDetails");
            }
        }
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
        void emailregtxt_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            try
            {
                emailreghint.HasError = false;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "emailregtxt_TextChanged");
            }
        }

        void genderlist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as string;

                GenderHelper.HasError = false;
                GenderHelper.ErrorText = String.Empty;

                if (item != null)
                {
                    gendertext = item;
                    OtherGen.IsVisible = item.Contains("self-describe");
                }               
            }
            catch (Exception Ex)
            {
                 CrashDetected.LogCrash(Ex, Navigation, "genderlist_ItemTapped");
            }
        }

        void ethnlist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as string;
                if (item != null)
                {
                    ethtext = item;
                }
               

            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "ethnlist_ItemTapped");
            }
        }


        async void btnmain_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {


                if (userDetails != null)
                {
                    var UpdateUser = userDetails.FirstOrDefault();
                    var changes = new Dictionary<string, object>();
                    if (UpdateUser == null) return; 

                    if (NameEdit.IsVisible == true)
                    {

                        UpdateUser.FirstName = FirstNameTxt.Text;
                        UpdateUser.Surname = SurNameTxt.Text;

                        changes.Add("firstname", FirstNameTxt.Text);
                        changes.Add("surname", SurNameTxt.Text);

                        Preferences.Set("firstname", FirstNameTxt.Text);
                        Preferences.Set("surname", SurNameTxt.Text);

                    }
                    else if (emailstack.IsVisible == true)
                    {
                        var email = Helpers.Settings.Email; 

                        //email check
                        if (string.IsNullOrEmpty(emailregtxt.Text))
                        {
                            emailreghint.HasError = true;
                            emailreghint.ErrorText = "Please enter an email address";
                            Vibration.Vibrate();
                            emailregtxt.Focus();
                            return;
                        }

                        //check if its a valid email
                        if (EmailIsValid(emailregtxt.Text) == false)
                        {
                            emailreghint.HasError = true;
                            emailreghint.ErrorText = "Please enter a valid email address";
                            Vibration.Vibrate();
                            emailregtxt.Focus();
                            return;
                        }

                        var newEmail = emailregtxt.Text.TrimEnd();

                        if (email == newEmail)
                        {
                            emailreghint.HasError = true;
                            emailreghint.ErrorText = "Email hasn't changed";
                            Vibration.Vibrate();
                            emailregtxt.Focus();
                            return;
                        }
                
                        changes.Add("email", newEmail);
                        Preferences.Set("email", newEmail);

                    }
                    else if (dobstack.IsVisible == true)
                    {

                        var dt = DateTime.Now;

                        var convertage = DateTime.Parse(daypickernew.SelectedDate.ToString());

                        int userAge = Genericlist.CalculateUserAge(convertage);

                        UpdateUser.Age = convertage.ToString("dd/MM/yyyy");

                        UpdateUser.Loweragebracket = userAge - 5;

                        if (UpdateUser.Loweragebracket <= 0)
                        {
                            UpdateUser.Loweragebracket = 0;
                        }
                        UpdateUser.Upperagebracket = userAge + 5;

                        Preferences.Set("age", UpdateUser.Age);
                        string lowerage = UpdateUser.Loweragebracket.ToString();
                        string upperage = UpdateUser.Upperagebracket.ToString();
                        Preferences.Set("loweragekey", lowerage);
                        Preferences.Set("upperagekey", upperage);

                        changes.Add("dateofbirth", UpdateUser.Age);

                    }
                    else if (genderstack.IsVisible == true)
                    {
                        if(genderlist.SelectedItem == null)
                        {
                            Vibration.Vibrate();
                            return;
                        }

                        if (OtherGen.IsVisible == true)
                        {
                            if (string.IsNullOrEmpty(GenderTxt.Text))
                            {
                                GenderHelper.HasError = true;
                                GenderHelper.ErrorText = "Enter Gender"; 
                                Vibration.Vibrate();
                                return;
                            }
                            gendertext = GenderTxt.Text;
                        }

                        UpdateUser.Gender = gendertext;
                        Preferences.Set("gender", gendertext);
                        changes.Add("gender", gendertext);

                    }
                    else if (ethstack.IsVisible == true)
                    {
                        if (ethnlist.SelectedItem == null)
                        {
                            Vibration.Vibrate();
                            return;
                        }

                        UpdateUser.Ethnicity = ethtext;
                        Preferences.Set("ethnicity", ethtext);
                        changes.Add("ethnicity", ethtext);
                    }
                    else if (phonestack.IsVisible == true)
                    {               
                        if (String.IsNullOrEmpty(mobtxt.Text))
                        {
                            mobhint.HasError = true;
                            mobhint.ErrorText = "Enter Phone Number";
                            Vibration.Vibrate();
                            return;
                        }

                        UpdateUser.PhoneNumber = mobtxt.Text;
                        Preferences.Set("phonenumber", mobtxt.Text.Trim());
                        changes.Add("telephone", mobtxt.Text.Trim());
                    }
                    else if (townstack.IsVisible == true)
                    {
                        if (String.IsNullOrEmpty(towntxt.Text))
                        {
                            townhint.HasError = true;
                            townhint.ErrorText = "Enter Phone Number";
                            Vibration.Vibrate();
                            return;
                        }

                        if (UpdateUser?.DetailsList == null || !UpdateUser.DetailsList.Any()) return;

                        var UpdateItem = UpdateUser.DetailsList.FirstOrDefault();
                        if(UpdateItem != null)
                        {
                            UpdateItem.town = towntxt.Text.Trim();
                            UpdateUser.Details = JsonConvert.SerializeObject(new List<object> { UpdateItem });
                            changes.Add("details", UpdateUser.Details);
                            Preferences.Set("town", towntxt.Text.Trim());
                        }
                    
                    }
                    else if (heightstack.IsVisible == true)
                    {
                        UpdateUser.Height = heightlabel.Text;

                        Preferences.Set("height", heightlabel.Text);
                    }
                    else if (weightstack.IsVisible == true)
                    {
                        UpdateUser.Weight = weightlabel.Text;
                        Preferences.Set("weight", weightlabel.Text);
                    }
                    else if(epidstack.IsVisible == true)
                    {

                        if(string.IsNullOrEmpty(epidtxt.Text))
                        {
                            epidtxt.Focus();
                            Vibration.Vibrate();
                            return;
                        }

                        UpdateUser.Epid = epidtxt.Text;
                        Preferences.Set("userepid", epidtxt.Text);
                    }

                    if (changes.Count > 0)
                    {
                        bool success = await APICalls.Instance.UpdateUserData(UpdateUser.Userid, changes);

                        if (success)
                        {
                            await MopupService.Instance.PushAsync(new PopupPageHelper("Profile Updated"));
                            WeakReferenceMessenger.Default.Send(new UpdateProfile("Update"));

                            var pages = Navigation.NavigationStack.ToList();
                            int i = 0;
                            foreach (var page in pages)
                            {
                                if (i == 0)
                                {

                                }
                                else if (i == 1)
                                {
                                    Navigation.RemovePage(page);
                                }
                                else if (i == 2)
                                {
                                    Navigation.RemovePage(page);
                                }
                                else
                                {


                                }

                                i++;
                            }

                        }
                        await Task.Delay(3000);
                        await MopupService.Instance.PopAsync();
                    }
                }
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "btnmain_Clicked");
            }
        }

        async void handledetailsEdit()
        {
            try
            {

            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "handledetailsEdit");
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
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "segmentedcontrolweight_ItemTapped");
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
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "weightlist_ItemTapped");
            }
        }

        void weightlist_Loaded(Object sender, Syncfusion.Maui.ListView.ListViewLoadedEventArgs e)
        {
            try
            {
                //add this back in
             //   (weightlist.LayoutManager as LinearLayout).
             //ScrollToRowIndex(weightlist.DataSource.DisplayItems.IndexOf(weightlist.SelectedItem), Syncfusion.ListView.XForms.ScrollToPosition.Center, false);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "weightlist_Loaded");
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
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "heightlist_ItemTapped");
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
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "segmentedcontrolheight_ItemTapped");
            }
        }

        void heightlist_Loaded(Object sender, Syncfusion.Maui.ListView.ListViewLoadedEventArgs e)
        {
            try
            {
                //add this back in
             //   (heightlist.LayoutManager as LinearLayout).
             //ScrollToRowIndex(heightlist.DataSource.DisplayItems.IndexOf(heightlist.SelectedItem), Syncfusion.ListView.XForms.ScrollToPosition.Center, false);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "heightlist_Loaded");
            }
        }

        private void mobtxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                mobhint.HasError = false;
                mobhint.ErrorText = string.Empty;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "mobtxt_TextChanged");
            }
        }

        private void towntxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                townhint.HasError = false;
                townhint.ErrorText = string.Empty;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "towntxt_TextChanged");
            }
        }
    }
}
