using FreakyKit.Utils;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Networking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.Maui.Controls.Internals.Profile;


namespace PeopleWithResearch
{
    public partial class NewLoginPage : ContentPage
    {
        public event EventHandler<bool> ConnectivityChanged;
        static Regex ValidEmailRegex = CreateValidEmailRegex();
        private bool isawait = false;
        UserNotifications AddNotification = new();

        public NewLoginPage()
        {
            InitializeComponent();
            emailentry.TextChanged += OnEntryTextChanged;
            passwordentry.TextChanged += OnEntryTextChanged;
            ApplyLocalization();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            Titlelbl.Text          = LocalizationManager.Get("Login_WelcomeBack");
            subtitleLbl.Text       = LocalizationManager.Get("Login_SignInSubtitle");
            emailLbl.Text          = LocalizationManager.Get("Login_EmailLabel");
            passwordLbl.Text       = LocalizationManager.Get("Login_PasswordLabel");
            forgotPasswordLbl.Text = LocalizationManager.Get("Login_ForgotPassword");
            Login.Text             = LocalizationManager.Get("Login_SignInButton");
            noAccountSpan.Text     = LocalizationManager.Get("Login_NoAccount") + " ";
            signUpSpan.Text        = LocalizationManager.Get("Login_SignUpLink");
            privacyPolicyLbl.Text  = LocalizationManager.Get("Common_PrivacyPolicy");
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender is Entry entry)
                {
                    if (entry == emailentry)
                    {
                        if (emailhelper.HasError)
                        {
                            emailhelper.HasError = false;
                            emailhelper.ErrorText = null;
                        }
                    }
                    else if (entry == passwordentry)
                    {
                        if (passhelper.HasError)
                        {
                            passhelper.HasError = false;
                            passhelper.ErrorText = null;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "OnEntryTextChanged");
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
        public byte[] GetHash(string data)
        {
            using (var md5 = MD5.Create())
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                return md5.ComputeHash(dataBytes);
            }
        }
        public string ByteArrayToHex(byte[] hash)
        {
            var hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);

            return hex.ToString();
        }
        async void PrivPolicy_Tapped(System.Object sender, System.EventArgs e)
        {
            if (isawait) return; 
            try
            {
                isawait = true;
                await Navigation.PushAsync(new PrivacyPolicyPage(), false);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "PrivPolicy_Tapped");
                isawait = false;
            }
            finally
            {
                isawait = false;
            }
        }

        private async void ForgotPass_Tapped(object sender, TappedEventArgs e)
        {
            if (isawait) return;
            try
            {
                isawait = true;
                await Navigation.PushAsync(new ForgotPassword(), false);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "ForgotPass_Tapped");
                isawait = false;
            }
            finally
            {
                isawait = false;
            }
        }

        private async void Login_Clicked(object sender, EventArgs e)
        {
            if (isawait) return;

            try
            {
                isawait = true;
                Login.IsEnabled = false;

                NetworkAccess accessType = Connectivity.Current.NetworkAccess;
                if (accessType == NetworkAccess.Internet)
                {
                    await handleLoginLogicAsync();
                }
                else
                {
                    ConnectivityChanged?.Invoke(this, false);
                    Login.IsEnabled = true;
                    isawait = false;
                }
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "Login_Clicked");
                Login.IsEnabled = true;
                isawait = false;
            }
        }

        // Changed from async void to async Task
        async Task handleLoginLogicAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(emailentry.Text))
                {
                    emailhelper.HasError = true;
                    emailhelper.ErrorText = LocalizationManager.Get("Login_EmailEmpty");
                    Vibration.Vibrate();
                    emailentry.Focus();
                    await LoadING(false);
                    Login.IsEnabled = true;
                    isawait = false;
                    return;
                }

                if (!EmailIsValid(emailentry.Text))
                {
                    emailhelper.ErrorText = LocalizationManager.Get("Login_EmailInvalid");
                    emailhelper.HasError = true;
                    Vibration.Vibrate();
                    emailentry.Focus();
                    await LoadING(false);
                    Login.IsEnabled = true;
                    isawait = false;
                    return;
                }

                if (string.IsNullOrEmpty(passwordentry.Text))
                {
                    passhelper.HasError = true;
                    passhelper.ErrorText = LocalizationManager.Get("Login_PasswordEmpty");
                    Vibration.Vibrate();
                    passwordentry.Focus();
                    await LoadING(false);
                    Login.IsEnabled = true;
                    isawait = false;
                    return;
                }

                passhelper.ErrorText = "";
                passhelper.HasError = false;

                await LoadING(true);

                var user = await APICalls.Instance.CheckEmailExists(emailentry.Text);
                if (user.Count == 0)
                {
                    emailhelper.ErrorText = LocalizationManager.Get("Login_AccountNotFound");
                    emailhelper.HasError = true;
                    Vibration.Vibrate();
                    emailentry.Focus();
                    await LoadING(false);
                    Login.IsEnabled = true;
                    isawait = false;
                    return;
                }

                emailhelper.ErrorText = "";
                emailhelper.HasError = false;

                var Userdetails = user.FirstOrDefault();
                if (Userdetails == null)
                {
                    await LoadING(false);
                    Login.IsEnabled = true;
                    isawait = false;
                    return;
                }
                if (Userdetails.deleted == true)
                {
                    Login.IsEnabled = true;
                    isawait = false;
                    await LoadING(false);
                    await DisplayAlert(LocalizationManager.Get("Login_AccountDeletedTitle"), LocalizationManager.Get("Login_AccountDeletedMsg"), LocalizationManager.Get("Common_OK"));
                    return;
                }
                else if (Userdetails.status == "Onboarding")
                {
                    Login.IsEnabled = true;
                    isawait = false;
                    await LoadING(false);
                    await DisplayAlert(LocalizationManager.Get("Login_OnboardingTitle"), LocalizationManager.Get("Login_OnboardingMsg"), LocalizationManager.Get("Common_OK"));               
                    return;
                }
                else if (string.Equals(Userdetails.status, "Withdrawn", StringComparison.OrdinalIgnoreCase))
                {
                    Login.IsEnabled = true;
                    isawait = false;
                    await LoadING(false);
                    await DisplayAlert(LocalizationManager.Get("Login_WithdrawnTitle"), LocalizationManager.Get("Login_WithdrawnMsg"), LocalizationManager.Get("Common_OK"));             
                    return;
                }

                string passwordtocompare = Userdetails.password;
                Byte[] UserPasswordByte = GetHash(passwordentry.Text);
                string userpassword = ByteArrayToHex(UserPasswordByte);

                if (passwordtocompare != userpassword)
                {
                    passhelper.ErrorText = LocalizationManager.Get("Login_PasswordIncorrect");
                    passhelper.HasError = true;
                    Vibration.Vibrate();
                    passwordentry.Focus();
                    await LoadING(false);
                    Login.IsEnabled = true;
                    isawait = false;
                    return;
                }

                //Email and Password Successful
                Preferences.Default.Set("userid", Userdetails.userid);
                Preferences.Default.Set("firstname", Userdetails.firstname);
                Preferences.Default.Set("surname", Userdetails.surname);
                Preferences.Default.Set("signupcode", Userdetails.signupcodeid);
                Preferences.Default.Set("email", Userdetails.email);
                Preferences.Default.Set("gender", Userdetails.gender);
                Preferences.Default.Set("ethnicity", Userdetails.ethnicity);
                Preferences.Default.Set("age", Userdetails.dateofbirth);
                Preferences.Default.Set("userpasswordhash", Userdetails.password);
                Preferences.Default.Set("sideupcodegrouping", Userdetails.signupcodegrouping);
                Preferences.Default.Set("householdgrouping", Userdetails.householdgroupid);
                Preferences.Default.Set("details", Userdetails.details);
                Preferences.Default.Set("primarycardid", Userdetails.primarycareid);
                Preferences.Default.Set("postcode", Userdetails.postcode);
                Preferences.Default.Set("isprimaryuser", Userdetails.primaryuser);
                Preferences.Default.Set("phonenumber", Userdetails.telephone);
                Preferences.Default.Set("devicemanufacturer", DeviceInfo.Manufacturer);
                Preferences.Default.Set("devicemodel", DeviceInfo.Model);
                Preferences.Default.Set("deviceversion", DeviceInfo.VersionString);

                if (Userdetails.primaryuser)
                {
                    Preferences.Default.Set("primaryuserid", Userdetails.userid);
                }


                if (Userdetails?.DetailsList != null || Userdetails.DetailsList.Any())
                {
                    var DetailsItem = Userdetails.DetailsList.FirstOrDefault();
                    if (DetailsItem != null)
                    {
                        Preferences.Default.Set("addresslineone", DetailsItem.addresslineone);
                        Preferences.Default.Set("town", DetailsItem.town);
                        Preferences.Default.Set("County", DetailsItem.County);
                    }
                }

                if (Userdetails?.NotificationDetails != null || Userdetails.NotificationDetails.Any())
                {
                    var Notif_Item = Userdetails.NotificationDetails.FirstOrDefault();
                    if (Notif_Item != null)
                    {
                        Preferences.Default.Set("notificationtime", Notif_Item.DailyTime);

                        int dailyId = int.TryParse(Notif_Item.DailyId, out int parsedDaily) ? parsedDaily : 0;
                        Preferences.Default.Set("daily_notification_id", dailyId);

                        if (!string.IsNullOrEmpty(Notif_Item.DailyTime))
                        {
                            //handle here else handle on the Dash 
                            await AddNotification.ScheduleDailyNotification();
                        }

                        int weeklyId = int.TryParse(Notif_Item.WeeklyId, out int parsedWeekly) ? parsedWeekly : 0;
                        Preferences.Default.Set("weekly_notification_id", weeklyId);

                        if (await GetStartDate() is DateTime startDate)
                        {
                            await AddNotification.ScheduleWeeklyNotification(ReasonableTime(startDate));
                        }
                    }
                }

                //Re-Create Push Notification Channel 
                var notificationService = new NotificationService();
                await notificationService.AddTag();

                await Task.Delay(500);
                await App.SetMainPage(new ImperialDashboard());
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "handleLoginLogicAsync");
                await LoadING(false);
                Login.IsEnabled = true;
                isawait = false;
            }
        }

        private DateTime ReasonableTime(DateTime originalDate)
        {
            //Greater than 5pm and less than 10am fix 
            if (originalDate.Hour >= 17)
            {
                return originalDate.Date.AddDays(1).AddHours(10);
            }

            if (originalDate.Hour < 10)
            {
                return originalDate.Date.AddHours(10);
            }

            return originalDate;
        }


        private async Task<DateTime?> GetStartDate()
        {
            try
            {
                var householdGroupList = await APICalls.Instance.GetUserHouseholdInfo(Helpers.Settings.HouseholdGrouping);
                var householdGroup = householdGroupList?.FirstOrDefault();

                if (householdGroup == null) return null;

                var GroupDetails = householdGroup.studydetails ?? new householdstudyrecord();
                var activeEvent = GroupDetails.t_events?.FirstOrDefault(x => x.t_event_status == "active");

                if (activeEvent?.t1_start_date == null) return null;

                if (!DateTime.TryParseExact(activeEvent.t1_start_date, "dd/MM/yyyy HH:mm",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime t1Start))
                {
                    return null;
                }

                return t1Start;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "GetStartDate");
                return null;
            }
        }


        private async Task LoadING(bool ShowHide)
        {
            try
            {
                Signinload.IsVisible = ShowHide;
                LoadInd.IsRunning = ShowHide;
                Login.IsVisible = !ShowHide;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "LoadING");
            }
        }

        private async void SignupClicked(object sender, TappedEventArgs e)
        {
            try
            {
                //await Navigation.PushAsync(new RegisterPage)
                Navigation.RemovePage(this);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "SignupClicked");
            }
        }
    }
}

