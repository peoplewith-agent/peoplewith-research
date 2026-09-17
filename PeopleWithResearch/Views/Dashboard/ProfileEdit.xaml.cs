using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
//using Plugin.Connectivity;
//using Rg.Plugins.Popup.Extensions;
//using Syncfusion.ListView.XForms;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Networking;
using CommunityToolkit.Mvvm.Messaging;
using PeoplewithResearch;
//using static Android.Net.Http.SslCertificate;


namespace PeopleWithResearch
{
    public partial class ProfileEdit : ContentPage
    {

        UserManager usermanger;
        public ObservableCollection<user> LogginedInUsers = new ObservableCollection<user>();
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

        public ProfileEdit()
        {
            InitializeComponent();
        }

        public ProfileEdit(string itempassed)
        {
            InitializeComponent();

            var newuser = new user();
            passeduser = newuser;

            userconsentmanger = UserConsentManager.DefaultManager;
            advertmanager = AdvertManager.DefaultManager;
            usermanger = UserManager.DefaultManager;


            if (itempassed == "Notifications")
            {
                notificationstack.IsVisible = true;
            }
            else if (itempassed == "Password Reset")
            {
                passwordreset.IsVisible = true;
            }

        }

        public ProfileEdit(user itempassed)
        {
            InitializeComponent();
            passeduser = itempassed;
            advertmanager = AdvertManager.DefaultManager;
            usermanger = UserManager.DefaultManager;
            userconsentmanger = UserConsentManager.DefaultManager;

            genlist.Add("Male");
            genlist.Add("Female");
            genlist.Add("Prefer not to say");
            genlist.Add("Prefer to self-describe");

            genderlist.ItemsSource = genlist;

            //ethnicity

            ethnicitylist.Add("White: British or Irish");
            ethnicitylist.Add("White: Other");
            ethnicitylist.Add("Mixed: White and Black Caribbean");
            ethnicitylist.Add("Mixed: White and Black African");
            ethnicitylist.Add("Mixed: White and Asian");
            ethnicitylist.Add("Other Mixed");
            ethnicitylist.Add("Asian or Asian British: Indian");
            ethnicitylist.Add("Asian or Asian British: Pakistani");
            ethnicitylist.Add("Asian or Asian British: Bangladeshi");
            ethnicitylist.Add("Other Asian");
            ethnicitylist.Add("Black or Black British: Black Carribbean");
            ethnicitylist.Add("Black or Black British: Black African");
            ethnicitylist.Add("Black or Black British: Other Black");
            ethnicitylist.Add("Another Group: Chinese");
            ethnicitylist.Add("Other ethnic group");


            ethnlist.ItemsSource = ethnicitylist;



            DateTime MinDate = DateTime.Now.AddYears(-100);
            daypickernew.MinimumDate = MinDate.Date;
            daypickernew.MaximumDate = DateTime.Now.AddDays(-1).Date;

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




            //height



            // Use Id (stable English key) so routing works regardless of the UI language
            var profileItemId = itempassed.Id ?? itempassed.Title;

            if (profileItemId == "Name")
            {
                namestack.IsVisible = true;
                firstnametxt.Text = Helpers.Settings.FirstName;
                surnametxt.Text = Helpers.Settings.Surname;
            }
            else if (profileItemId == "Email")
            {
                emailstack.IsVisible = true;
                emailregtxt.Text = Helpers.Settings.Email;
            }
            else if (profileItemId == "Date of Birth")
            {
                dobstack.IsVisible = true;
                DayPickerStack.IsVisible = true;
                DOBField.IsVisible = false;

                var day = DateTime.Parse(Helpers.Settings.Age).ToString("dd");
                var month = DateTime.Parse(Helpers.Settings.Age).ToString("MM");
                var year = DateTime.Parse(Helpers.Settings.Age).ToString("yyyy");

                var dateofbirth = DateTime.Parse(Helpers.Settings.Age);

                daypickernew.SelectedDate = dateofbirth;

                DayEntry.Text = day;
                MonthEntry.Text = month;
                YearEntry.Text = year;

                btnmain.IsVisible = false;

            }
            else if (profileItemId == "Gender")
            {
                genderstack.IsVisible = true;
                gendertext = Helpers.Settings.Gender;


                if (!genlist.Contains(Helpers.Settings.Gender))
                {
                    genderlist.SelectedItem = "Prefer to self-describe";
                    genderentry.IsVisible = true;
                    genderentrylabel.IsVisible = true;

                    genderentry.Text = Helpers.Settings.Gender;
                }
                else
                {
                    genderlist.SelectedItem = Helpers.Settings.Gender;
                }
            }
            else if (profileItemId == "Ethnicity")
            {
                ethstack.IsVisible = true;
                ethtext = Helpers.Settings.Ethnicity;
                ethnlist.SelectedItem = Helpers.Settings.Ethnicity;
            }
            else if (profileItemId == "Phone Number")
            {
                phonestack.IsVisible = true;
                mobtxt.Text = Helpers.Settings.PhoneNumber;
            }
            else if (profileItemId == "Town/City")
            {
                townstack.IsVisible = true;
                towntxt.Text = Helpers.Settings.Town;
            }
            else if (profileItemId == "Height")
            {


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

                heightunit.Add("ft in");
                heightunit.Add("cm");
                segmentedcontrolheight.ItemsSource = heightunit;

                if (!string.IsNullOrEmpty(Helpers.Settings.Height))
                {
                    if (Helpers.Settings.Height.Contains("cm"))
                    {
                        heightunitvalue = "cm";
                        heightlist.ItemsSource = heightcmlist;
                        segmentedcontrolheight.SelectedItem = heightunit[1];
                    }
                    else
                    {
                        heightunitvalue = "ft in";
                        // weightunittxt.Text = "Stones & Pounds";
                        heightlist.ItemsSource = heightftlist;
                        segmentedcontrolheight.SelectedItem = heightunit[0];
                    }


                }
                else
                {
                    heightunitvalue = "ft in";
                    weightlist.ItemsSource = heightftlist;
                }

                heightstack.IsVisible = true;

                if (!string.IsNullOrWhiteSpace(Helpers.Settings.Height))
                {
                    heightlabel.Text = Helpers.Settings.Height;

                    if (Helpers.Settings.Height.Contains("cm"))
                    {
                        var removelabel = Helpers.Settings.Height.Replace("cm", "").Trim();

                        var getindex = heightcmlist.IndexOf(heightcmlist.Where(x => x.Valuenumber == removelabel).FirstOrDefault());

                        heightlist.SelectedItem = heightcmlist[getindex];
                    }
                    else
                    {
                        var getindex = heightftlist.IndexOf(heightftlist.Where(x => x.Valuenumber == Helpers.Settings.Height).FirstOrDefault());

                        heightlist.SelectedItem = heightftlist[getindex];
                    }
                }







            }
            else if (profileItemId == "Weight")
            {
                weightstack.IsVisible = true;

                if (!string.IsNullOrWhiteSpace(Helpers.Settings.Weight))
                {
                    weightlabel.Text = Helpers.Settings.Weight;

                    if (Helpers.Settings.Weight.Contains("kg"))
                    {
                        var removelabel = Helpers.Settings.Weight.Replace("kg", "").Trim();

                        var getindex = weightkglist.IndexOf(weightkglist.Where(x => x.Valuenumber == removelabel).FirstOrDefault());

                        weightlist.SelectedItem = weightkglist[getindex];

                    }
                    else
                    {
                        // var removelabel = Helpers.Settings.Weight.Replace("st", "").Replace("lbs", "").Trim();

                        var getindex = weightstonelist.IndexOf(weightstonelist.Where(x => x.Valuenumber == Helpers.Settings.Weight).FirstOrDefault());

                        weightlist.SelectedItem = weightstonelist[getindex];


                    }


                    //weighttxt.Text = Helpers.Settings.Weight;
                }
            }

            else if (profileItemId == "National Health Identifer" || profileItemId == "National Health Identifier")
            {
                epidstack.IsVisible = true;
                epidtxt.Text = Helpers.Settings.Userepid;
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


        void emailregtxt_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            try
            {
                emailreghint.HasError = false;
            }
            catch (Exception ex)
            {

            }
        }

        void TapGestureRecognizer_Tapped_1(System.Object sender, System.EventArgs e)
        {
            DayPickerStack.IsVisible = true;
            DOBField.IsVisible = false;
            btnmain.IsVisible = false;
            ConfirmDOB.IsVisible = true;
        }

        void DayEntry_Focused(System.Object sender, FocusEventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DayPickerStack.IsVisible = true;
                    DOBField.IsVisible = false;
                });
            }
            catch (Exception ex)
            {

            }
        }

        void ConfirmDOB_Clicked(System.Object sender, System.EventArgs e)
        {
          //  DayEntry.Text = daypickernew.SelectedDate.ToString("dd");
          //  MonthEntry.Text = daypickernew.SelectedDate.ToString("MM");
           // YearEntry.Text = daypickernew.SelectedDate.ToString("yyyy");

            DayPickerStack.IsVisible = false;
            DOBField.IsVisible = true;

            btnmain.IsVisible = true;
            ConfirmDOB.IsVisible = false;
        }

        void genderlist_ItemTapped(System.Object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
        {
            try
            {
                var item = e.DataItem as string;

                gendertext = item;


                if (item.Contains("self-describe"))
                {
                    genderentry.IsVisible = true;
                    genderentrylabel.IsVisible = true;
                }
                else
                {
                    genderentry.IsVisible = false;
                    genderentrylabel.IsVisible = false;
                }

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

                ethtext = item;

            }
            catch (Exception ex)
            {

            }
        }

        async void btnmain_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {

                //save button

                //get user details
                var getuser = await usermanger.getUserInfo(Helpers.Settings.UserKey);

                if (getuser != null)
                {
                    //only update certain details
                    if (namestack.IsVisible == true)
                    {

                        getuser[0].FirstName = firstnametxt.Text;
                        getuser[0].Surname = surnametxt.Text;

                        Preferences.Set("firstname", firstnametxt.Text);
                        Preferences.Set("surname", surnametxt.Text);


                        ///update the name on the dash

                        //MessagingCenter.Send<object>(this, "refreshname");
                        WeakReferenceMessenger.Default.Send(new refreshname("refreshname"));

                    }
                    else if (emailstack.IsVisible == true)
                    {
                        //email check
                        if (string.IsNullOrEmpty(emailregtxt.Text))
                        {
                            emailreghint.HasError = true;
                            emailreghint.ErrorText = LocalizationManager.Get("ProfileEdit_EmailEmpty");
                            Vibration.Vibrate();
                            emailregtxt.Focus();
                            return;
                        }

                        //check if its a valid email
                        if (EmailIsValid(emailregtxt.Text) == false)
                        {
                           // emailreghint.ErrorColor = Colors.Red;
                            emailreghint.HasError = true;
                            emailreghint.ErrorText = LocalizationManager.Get("ProfileEdit_EmailInvalid");
                            Vibration.Vibrate();
                            emailregtxt.Focus();
                            return;
                        }



                        //check if the user is already in the system

                        emailregtxt.Text = emailregtxt.Text.TrimEnd();

                        string username = emailregtxt.Text.ToLower();

                        // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                        //if (Device.RuntimePlatform == Device.Android)
                        //{
                        //    var newsymss = await usermanger.CurrentClient.InvokeApiAsync("getuseremail?email=" + username, null, HttpMethod.Get, await App.LoadAPIKeyAsync(), null);
                        //    var responseContentt = await newsymss.Content.ReadAsStringAsync();
                        //    var user = JsonConvert.DeserializeObject<ObservableCollection<User>>(responseContentt);
                        //    foreach (var item in user)
                        //    {
                        //        LogginedInUsers.Add(item);
                        //    }
                        //}
                        //else
                        //{
                        //    LogginedInUsers = await usermanger.checkUser(username); //checks if the user exists given the emeai address
                        //}

                        LogginedInUsers = await usermanger.checkUser(username);

                        if (LogginedInUsers.Count > 0)
                        {
                            if (LogginedInUsers[0].RegStatus == "PendingSignpost")
                            {


                            }
                            else
                            {

                                emailreghint.HasError = true;
                                emailreghint.ErrorText = LocalizationManager.Get("ProfileEdit_EmailInUse");
                                Vibration.Vibrate();
                                emailregtxt.Focus();
                                return;


                            }
                        }

                        getuser[0].Email = emailregtxt.Text;

                        Preferences.Set("email", emailregtxt.Text);


                    }
                    else if (dobstack.IsVisible == true)
                    {
                        var dt = DateTime.Now;

                        var convertage = DateTime.Parse(daypickernew.SelectedDate.ToString());


                        if (convertage.Date == dt.Date)
                        {
                            Vibration.Vibrate();
                            return;
                        }

                        int userAge = usermanger.CalculateUserAge(convertage);

                        getuser[0].Age = convertage.ToString("dd/MM/yyyy");

                        getuser[0].Loweragebracket = userAge - 5;

                        if (getuser[0].Loweragebracket <= 0)
                        {
                            getuser[0].Loweragebracket = 0;
                        }
                        getuser[0].Upperagebracket = userAge + 5;

                        Preferences.Set("age", getuser[0].Age);
                        string lowerage = getuser[0].Loweragebracket.ToString();
                        string upperage = getuser[0].Upperagebracket.ToString();
                        Preferences.Set("loweragekey", lowerage);
                        Preferences.Set("upperagekey", upperage);

                    }
                    else if (genderstack.IsVisible == true)
                    {
                        if (genderentry.IsVisible == true)
                        {
                            gendertext = genderentry.Text;
                            getuser[0].Gender = gendertext;
                        }
                        else
                        {

                            getuser[0].Gender = gendertext;
                        }

                        Preferences.Set("gender", gendertext);
                    }
                    else if (ethstack.IsVisible == true)
                    {
                        getuser[0].Ethnicity = ethtext;

                        Preferences.Set("ethnicity", ethtext);
                    }
                    else if (phonestack.IsVisible == true)
                    {
                        getuser[0].PhoneNumber = mobtxt.Text;

                        Preferences.Set("phonenumber", mobtxt.Text);
                    }
                    else if (townstack.IsVisible == true)
                    {
                        getuser[0].AddressLineOne = towntxt.Text;

                        Preferences.Set("town", towntxt.Text);
                    }
                    else if (heightstack.IsVisible == true)
                    {
                        getuser[0].Height = heightlabel.Text;

                        Preferences.Set("height", heightlabel.Text);
                    }
                    else if (weightstack.IsVisible == true)
                    {
                        getuser[0].Weight = weightlabel.Text;

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

                        getuser[0].Epid = epidtxt.Text;

                        Preferences.Set("userepid", epidtxt.Text);
                    }

                    if (passwordreset.IsVisible == true)
                    {
                        string passwordtocompare;
                        string userpassword;

                        if (string.IsNullOrEmpty(passwordold.Text))
                        {
                            passworderror.Text = LocalizationManager.Get("ProfileEdit_EnterCurrentPassword");
                            Vibration.Vibrate();
                            return;
                        }
                        else
                        {
                            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                            //if (Device.RuntimePlatform == Device.Android)
                            //{
                            //    var email = Helpers.Settings.Email;
                            //    var newsymss = await usermanger.CurrentClient.InvokeApiAsync("getuserpass?userid=" + Helpers.Settings.UserKey, null, HttpMethod.Get, await App.LoadAPIKeyAsync(), null);
                            //    var responseContentt = await newsymss.Content.ReadAsStringAsync();
                            //    var user = JsonConvert.DeserializeObject<ObservableCollection<User>>(responseContentt);

                            //    foreach (var item in user)
                            //    {
                            //        LogginedInUsers.Add(item);
                            //    }

                            //}
                            //else
                            //{
                            //    LogginedInUsers = await usermanger.checkUser(Helpers.Settings.Email);
                            //}

                            LogginedInUsers = await usermanger.checkUser(Helpers.Settings.Email);

                            passeduser = LogginedInUsers[0];
                            passwordtocompare = LogginedInUsers[0].Password;
                            Byte[] UserPasswordByte = usermanger.GetHash(passwordold.Text);
                            userpassword = usermanger.ByteArrayToHex(UserPasswordByte);

                            if (passwordtocompare == userpassword)
                            {
                                if (string.IsNullOrEmpty(passwordnew.Text) || string.IsNullOrEmpty(passwordnew.Text))
                                {
                                    passworderror.Text = LocalizationManager.Get("ProfileEdit_EnterNewPassword");
                                    Vibration.Vibrate();
                                    return;
                                }
                                else
                                {
                                    if (passwordnew.Text != newpasswordcheck.Text)
                                    {
                                        passworderror.Text = LocalizationManager.Get("ProfileEdit_PasswordMismatch");
                                        Vibration.Vibrate();
                                        return;
                                    }
                                    else
                                    {

                                        if (passwordnew.Text.Length < 8)
                                        {
                                            passworderror.Text = LocalizationManager.Get("ProfileEdit_PasswordTooShort");
                                            Vibration.Vibrate();
                                            return;
                                        }
                                        else
                                        {
                                            var hasNumber = new Regex(@"[0-9]+");
                                            var hasUpperChar = new Regex(@"[A-Z]+");
                                            var hasMiniMaxChars = new Regex(@".{8,15}");
                                            var hasLowerChar = new Regex(@"[a-z]+");
                                            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?-]");

                                            if (!hasMiniMaxChars.IsMatch(passwordnew.Text))
                                            {
                                                passworderror.Text = LocalizationManager.Get("ProfileEdit_PasswordLength");
                                                Vibration.Vibrate();
                                                return;
                                            }
                                            else
                                            {
                                                if (!hasNumber.IsMatch(passwordnew.Text))
                                                {
                                                    passworderror.Text = LocalizationManager.Get("ProfileEdit_PasswordNeedsNumber");
                                                    Vibration.Vibrate();
                                                    return;
                                                }
                                                else
                                                {
                                                    if (!hasUpperChar.IsMatch(passwordnew.Text))
                                                    {
                                                        passworderror.Text = LocalizationManager.Get("ProfileEdit_PasswordNeedsUpper");
                                                        Vibration.Vibrate();
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        if (!hasLowerChar.IsMatch(passwordnew.Text))
                                                        {
                                                            passworderror.Text = LocalizationManager.Get("ProfileEdit_PasswordNeedsLower");
                                                            Vibration.Vibrate();
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            if (!hasSymbols.IsMatch(passwordnew.Text))
                                                            {
                                                                passworderror.Text = LocalizationManager.Get("ProfileEdit_PasswordNeedsSymbol");
                                                                Vibration.Vibrate();
                                                                return;
                                                            }
                                                            else
                                                            {
                                                                Byte[] BytesHashed = usermanger.GetHash(passwordnew.Text);
                                                                passeduser.Password = usermanger.ByteArrayToHex(BytesHashed);
                                                                await usermanger.AddUser(passeduser);
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }


                                    }
                                }
                            }
                            else
                            {
                                Vibration.Vibrate();
                                passworderror.Text = LocalizationManager.Get("ProfileEdit_PasswordCurrentIncorrect");
                                return;
                            }

                        }


                    }



                    else
                    {
                        //update user db
                        await usermanger.AddUser(getuser[0]);
                    }

                    //if (researchcode.IsVisible = true && consentpassed == false)
                    //{


                    //}

                    //update the user details on dash page
                    //MessagingCenter.Send<object, object>(this, "updateuserdetails", getuser[0]);

                    bool openprofile = true;

                    await Navigation.PushAsync(new MainDashboard(), false);

                    var pages = Navigation.NavigationStack.ToList();
                    int i = 0;
                    foreach (var page in pages)
                    {

                        if (i == 0)
                        {

                        }
                        else if (i == 1)
                        {
                            //do nothing
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

        void weightlist_Loaded(Object sender, Syncfusion.Maui.ListView.ListViewLoadedEventArgs e)
        {
            try
            {
                //add this back in
             //   (weightlist.LayoutManager as LinearLayout).
             //ScrollToRowIndex(weightlist.DataSource.DisplayItems.IndexOf(weightlist.SelectedItem), Syncfusion.ListView.XForms.ScrollToPosition.Center, false);
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

        void heightlist_Loaded(Object sender, Syncfusion.Maui.ListView.ListViewLoadedEventArgs e)
        {
            try
            {
                //add this back in
             //   (heightlist.LayoutManager as LinearLayout).
             //ScrollToRowIndex(heightlist.DataSource.DisplayItems.IndexOf(heightlist.SelectedItem), Syncfusion.ListView.XForms.ScrollToPosition.Center, false);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
