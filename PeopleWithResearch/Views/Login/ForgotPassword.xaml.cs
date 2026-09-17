using Azure;
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
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace PeopleWithResearch
{
    public partial class ForgotPassword : ContentPage
    {
        public event EventHandler<bool> ConnectivityChanged;
        static Regex ValidEmailRegex = CreateValidEmailRegex();
        private bool isawait = false; 

        public ForgotPassword()
        {
            InitializeComponent();
            emailentry.TextChanged += OnEntryTextChanged;
            Titlelbl.Text = LocalizationManager.Get("ForgotPwd_Title");
            Descriptivelbl.Text = LocalizationManager.Get("ForgotPwd_Description");
        }

        public ForgotPassword(string IsReset)
        {
            InitializeComponent();
            emailentry.TextChanged += OnEntryTextChanged;
            Titlelbl.Text = LocalizationManager.Get("ForgotPwd_ResetTitle");
            Descriptivelbl.Text = LocalizationManager.Get("ForgotPwd_ResetDescription");
            OtherOne.IsVisible = false;
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender is Entry entry)
                {

                    if (emailhelper.HasError)
                    {
                        emailhelper.HasError = false;
                        emailhelper.ErrorText = null;
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
        async void PrivPolicy_Tapped(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (Navigation.NavigationStack.LastOrDefault() is not PrivacyPolicyPage)
                {
                    await Navigation.PushAsync(new PrivacyPolicyPage(), false);
                }
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "PrivPolicy_Tapped");
            }
        }

        private async void Submit_Clicked(object sender, EventArgs e)
        {
            try
            {
                //Connectivity Changed 
                NetworkAccess accessType = Connectivity.Current.NetworkAccess;
                if (accessType == NetworkAccess.Internet)
                {
                    //Limit No. of Taps 
                    Submitbtn.IsEnabled = false;
                    handleLoginLogic();
                    await Task.Delay(1000);
                    Submitbtn.IsEnabled = true;
                }
                else
                {
                    var isConnected = accessType == NetworkAccess.Internet;
                    ConnectivityChanged?.Invoke(this, isConnected);
                }
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "Submit_Clicked");
            }
        }

        async void handleLoginLogic()
        {
            try
            {
                if (string.IsNullOrEmpty(emailentry.Text))
                {
                    emailhelper.HasError = true;
                    emailhelper.ErrorText = LocalizationManager.Get("ForgotPwd_ErrorEmpty");
                    Vibration.Vibrate();
                    emailentry.Focus();
                    return;
                }

                if (!EmailIsValid(emailentry.Text))
                {
                    emailhelper.ErrorText = LocalizationManager.Get("ForgotPwd_ErrorInvalid");
                    emailhelper.HasError = true;
                    Vibration.Vibrate();
                    emailentry.Focus();
                    return;
                }

                await LoadING(true);

                var user = await APICalls.Instance.CheckEmailExists(emailentry.Text);
                if (user.Count == 0)
                {
                    emailhelper.ErrorText = LocalizationManager.Get("ForgotPwd_ErrorNotFound");
                    emailhelper.HasError = true;
                    Vibration.Vibrate();
                    emailentry.Focus();
                    await LoadING(false);
                    return;
                }

                emailhelper.ErrorText = "";
                emailhelper.HasError = false;

                var Userdetails = user.FirstOrDefault();
                if (Userdetails == null)
                {
                    await LoadING(false);
                    return; 
                }
                if (Userdetails.deleted == true)
                {
                    await DisplayAlert(LocalizationManager.Get("ForgotPwd_AlertDeletedTitle"), LocalizationManager.Get("ForgotPwd_AlertDeletedMsg"), LocalizationManager.Get("ForgotPwd_AlertOk"));
                    await LoadING(false);
                    return;
                }
                else if (Userdetails.status == "Onboarding")
                {
                    await DisplayAlert(LocalizationManager.Get("ForgotPwd_AlertOnboardingTitle"), LocalizationManager.Get("ForgotPwd_AlertOnboardingMsg"), LocalizationManager.Get("ForgotPwd_AlertOk"));
                    await LoadING(false);
                    return;
                }
                else if (Userdetails.status == "Withdrawn")
                {
                    await DisplayAlert(LocalizationManager.Get("ForgotPwd_AlertWithdrawnTitle"), LocalizationManager.Get("ForgotPwd_AlertWithdrawnMsg"), LocalizationManager.Get("ForgotPwd_AlertOk"));
                    await LoadING(false);
                    return;
                }

                bool Check = await APICalls.Instance.PasswordRest(emailentry.Text);
                await LoadING(false);
                if (Check)
                {
                    //Successfully Sent
                    await DisplayAlert(LocalizationManager.Get("ForgotPwd_AlertSuccessTitle"), LocalizationManager.Get("ForgotPwd_AlertSuccessMsg"), LocalizationManager.Get("ForgotPwd_AlertOk"));
                    Navigation.RemovePage(this);
                }
                else
                {
                    await DisplayAlert(LocalizationManager.Get("ForgotPwd_AlertFailTitle"), LocalizationManager.Get("ForgotPwd_AlertFailMsg"), LocalizationManager.Get("ForgotPwd_AlertOk"));
                }

            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "handleLoginLogic");
                await LoadING(false);
            }
        }

        private async Task LoadING(bool ShowHide)
        {
            try
            {
                Signinload.IsVisible = ShowHide;
                LoadInd.IsRunning = ShowHide;
                Submitbtn.IsVisible = !ShowHide;
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
                Navigation.RemovePage(this);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "SignupClicked");
            }
        }

        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                //Connectivity Changed 
                NetworkAccess accessType = Connectivity.Current.NetworkAccess;
                if (accessType == NetworkAccess.Internet)
                {
                    if (Email.Default.IsComposeSupported)
                    {
                        string userId = !string.IsNullOrWhiteSpace(Helpers.Settings.UsersID) ? Helpers.Settings.UsersID : "[Add userid if known]";
                        
                        var message = new EmailMessage
                        {
                            Subject = "Having trouble resetting password",
                            //Body = $"Userid: {userId} | Email: {email}",
                            Body = $"Userid: {userId}",
                            BodyFormat = EmailBodyFormat.PlainText,
                            To = new List<string> { "hopper-study@peoplewith.com" }
                        };

                        await Email.Default.ComposeAsync(message);
                    }
                }
                else
                {
                    var isConnected = accessType == NetworkAccess.Internet;
                    ConnectivityChanged?.Invoke(this, isConnected);
                }
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped");
            }
        }
    }
}

