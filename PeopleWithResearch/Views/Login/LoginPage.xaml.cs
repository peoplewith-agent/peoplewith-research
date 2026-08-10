using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
//using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
//using Plugin.Connectivity;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Networking;
using Microsoft.AppCenter.Analytics;
namespace PeopleWithResearch
{
    public partial class LoginPage : ContentPage
    {
        public user newuser = new user();
        public string agestring;
        public string passwordtocompare;
        public string userpassword;
        UserManager usermanager;

        public ObservableCollection<user> LogginedInUsers = new ObservableCollection<user>();

        public LoginPage()
        {
            InitializeComponent();

            usermanager = UserManager.DefaultManager;




          //  Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, true);
        }


        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            //login button clicked
            try
            {

                emailtxt.Text = emailtxt.Text.TrimEnd();

                //email check
                if (string.IsNullOrEmpty(emailtxt.Text))
                {
                    emailhint.HasError = true;
                    emailhint.ErrorText = "Please enter an email address";
                    Vibration.Vibrate();
                    emailtxt.Focus();
                    return;
                }

                //password check
                if (string.IsNullOrEmpty(passtxt.Text))
                {
                    passhint.HasError = true;
                    passhint.ErrorText = "Please enter an password";
                    Vibration.Vibrate();
                    passtxt.Focus();
                    return;
                }


                //check if its a valid email
                if (EmailIsValid(emailtxt.Text) == false)
                {
                   // emailhint.ErrorColor = Colors.Red;
                    emailhint.HasError = true;
                    emailhint.ErrorText = "Please enter a valid email address";
                    Vibration.Vibrate();
                    emailtxt.Focus();
                    return;
                }


                //check password validation
                if (passtxt.Text.Length < 8)
                {

                    // BusyIndicator.IsRunning = false;
                    passtxt.Focus();
                    passhint.HasError = true;
                    passhint.ErrorText = "Password must be greater than 8 characters";
                    Vibration.Vibrate();
                    //  btnAddtwo.IsBusy = false;
                    return;
                }



                var hasNumber = new Regex(@"[0-9]+");
                var hasUpperChar = new Regex(@"[A-Z]+");
                var hasMiniMaxChars = new Regex(@".{8,15}");
                var hasLowerChar = new Regex(@"[a-z]+");
                var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

                if (!hasLowerChar.IsMatch(passtxt.Text))
                {
                    passtxt.Focus();
                    passhint.HasError = true;
                    passhint.ErrorText = "Password should contain at least one lower case letter.";
                    Vibration.Vibrate();
                    //passworderrorlbl.IsVisible = true;
                    //passworderrorlbl.Text = "Password should contain at least one lower case letter.";
                    return;
                }
                else if (!hasUpperChar.IsMatch(passtxt.Text))
                {
                    passtxt.Focus();
                    passhint.HasError = true;
                    passhint.ErrorText = "Password should contain at least one upper case letter.";
                    Vibration.Vibrate();
                    // passworderrorlbl.IsVisible = true;
                    //  passworderrorlbl.Text = "Password should contain at least one upper case letter.";
                    return;
                }
                else if (!hasMiniMaxChars.IsMatch(passtxt.Text))
                {
                    passtxt.Focus();
                    passhint.HasError = true;
                    passhint.ErrorText = "Password should not be lesser than 8 or greater than 15 characters.";
                    Vibration.Vibrate();
                    //passworderrorlbl.IsVisible = true;
                    //passworderrorlbl.Text = "Password should not be lesser than 8 or greater than 15 characters.";
                    return;
                }
                else if (!hasNumber.IsMatch(passtxt.Text))
                {
                    passtxt.Focus();
                    passhint.HasError = true;
                    passhint.ErrorText = "Password should contain at least one numeric value.";
                    Vibration.Vibrate();
                    //passworderrorlbl.IsVisible = true;
                    //passworderrorlbl.Text = "Password should contain at least one numeric value.";
                    return;
                }


                //  loginstack.IsVisible = false;
                //   firstregstack.IsVisible = true;


                //check internet
                if (!Connectivity.Current.NetworkAccess.HasFlag(NetworkAccess.Internet))
                {
                    await DisplayAlert("No Internet Connection", "Please check your connection", "OK");
                    //loginbtn.IsEnabled = true;
                }
                else
                {
                    LogginedInUsers.Clear();
                    string username = emailtxt.Text.ToLower();

                    // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                    //if (Device.RuntimePlatform == Device.Android)
                    //{
                    //    var newsymss = await usermanager.CurrentClient.InvokeApiAsync("getuseremail?email=" + username, null, HttpMethod.Get, await App.LoadAPIKeyAsync(), null);
                    //    var responseContentt = await newsymss.Content.ReadAsStringAsync();
                    //    var user = JsonConvert.DeserializeObject<ObservableCollection<user>>(responseContentt);
                    //    foreach (var item in user)
                    //    {
                    //        LogginedInUsers.Add(item);
                    //    }


                    //}
                    //else
                    //{
                    //    LogginedInUsers = await usermanager.checkUser(username); //checks if the user exists given the emeai address
                    //}

                    LogginedInUsers = await usermanager.checkUser(username);


                    if (LogginedInUsers.Count == 0)
                    {
                        await DisplayAlert("Invalid Username or Password", "The details entered are incorrect", "OK");

                    }

                    else
                    {
                        passwordtocompare = LogginedInUsers[0].Password;
                        Byte[] UserPasswordByte = usermanager.GetHash(passtxt.Text);
                        userpassword = usermanager.ByteArrayToHex(UserPasswordByte);

                    }



                    if (LogginedInUsers[0].RegStatus == "Removed")
                    {
                        await DisplayAlert("Account Disabled", "Your account has been disabled", "OK");

                    }
                    else if (LogginedInUsers[0].RegStatus == "Deleted")
                    {
                        await DisplayAlert("Account Deleted", "Your account has been deleted", "OK");
                    }
                    else if (LogginedInUsers[0].RegStatus == "Withdrawn")
                    {
                        await DisplayAlert("Account Withdrawn", "Login has been unsuccessful because you have withdrawn from the study. If this is an error, please contact: support@peoplewith.com", "OK");
                    }
                    else
                    {

                        if (passwordtocompare == userpassword)
                        {
                            // await Navigation.PopModalAsync();
                            Preferences.Set("id", LogginedInUsers[0].Id);
                            Preferences.Set("usertitle", LogginedInUsers[0].Title);
                            Preferences.Set("firstname", LogginedInUsers[0].FirstName);
                            Preferences.Set("surname", LogginedInUsers[0].Surname);
                            Preferences.Set("gender", LogginedInUsers[0].Gender);
                            Preferences.Set("email", LogginedInUsers[0].Email);
                            Preferences.Set("password", LogginedInUsers[0].Password);
                            Preferences.Set("age", LogginedInUsers[0].Age);
                            Preferences.Set("ethnicity", LogginedInUsers[0].Ethnicity);
                            Preferences.Set("addresslineone", LogginedInUsers[0].AddressLineOne);
                            Preferences.Set("addresslinetwo", LogginedInUsers[0].AddressLineTwo);
                            Preferences.Set("town", LogginedInUsers[0].Town);
                            Preferences.Set("city", LogginedInUsers[0].City);
                            Preferences.Set("postcode", LogginedInUsers[0].Postcode);
                            Preferences.Set("phonenumber", LogginedInUsers[0].PhoneNumber);
                            Preferences.Set("loweragekey", LogginedInUsers[0].Loweragebracket);
                            Preferences.Set("upperagekey", LogginedInUsers[0].Upperagebracket);
                            Preferences.Set("userpasswordhash", LogginedInUsers[0].Password);
                            Preferences.Set("rotationsetting", "On");
                            Preferences.Set("height", LogginedInUsers[0].Height);
                            Preferences.Set("launchvideo", "seen");
                            Preferences.Set("weight", LogginedInUsers[0].Weight);
                            Preferences.Set("signupcode", LogginedInUsers[0].Signupid);
                            Preferences.Set("clinicaltrial", "Yes");
                            Preferences.Set("createdat", LogginedInUsers[0].Createdat.ToString());
                            Preferences.Set("usergpid", LogginedInUsers[0].Gpid);
                            Preferences.Set("androidnotekey", "firstreg");
                            Preferences.Set("notificationtime", LogginedInUsers[0].NotificationTime);

                            //getcloudusermednotifications();
                            //getclouduserappointmentnotifications();

                            //  await Navigation.PopModalAsync();
                            //   await Navigation.PopToRootAsync();
                            // await Navigation.PopModalAsync();

                            //Application.Current.MainPage = new NavigationPage(new AdvertNewReg());
                            //await Navigation.PushAsync(new NavigationPage(new NewDashPage()));

                            var notificationService = new NotificationService();
                            await notificationService.AddTag();

                            if (LogginedInUsers[0].Signupid == "IID32")
                            {
                                Application.Current.MainPage = new NavigationPage(new NotificationQuestionGP());
                            }
                            else
                            {

                                Application.Current.MainPage = new NavigationPage(new MainDashboard());
                            }
                            //await Navigation.PushAsync(new NavigationPage(new ModernDash()));

                            Analytics.TrackEvent("Login Page - User logged in");
                            //Navigation.RemovePage(this);

                            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                            if (Device.RuntimePlatform == Device.Android)
                            {


                                //MessagingCenter.Send<AdvertNewPopUpLogin>(this, "androidload");
                            }
                            else
                            {

                            }

                        }
                        else
                        {
                            await DisplayAlert("Invalid Password", "Please check your password", "OK");
                        }

                    }



                    //if (preferenceitem == null)
                    //{
                    //    var navigationPage = new NavigationPage(new UserPreferencesList());
                    //    Device.BeginInvokeOnMainThread(async () =>
                    //    {
                    //        try
                    //        {
                    //            await Navigation.PushModalAsync(navigationPage);
                    //        }
                    //        catch (Exception) { }

                    //    });

                    //}


                    //  loginbtn.IsEnabled = true;

                }



            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "Button_Clicked");
            }
        }

        void passtxt_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
            try
            {
                passhint.HasError = false;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "passtxt_TextChanged");
            }
        }

        void emailtxt_TextChanged(System.Object sender,TextChangedEventArgs e)
        {
            try
            {
                emailhint.HasError = false;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "emailtxt_TextChanged");
            }         
        }

        static Regex ValidEmailRegex = CreateValidEmailRegex();

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

        async void toolbar_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                Navigation.RemovePage(this);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "toolbar_Clicked");
            }
        }

        void forgotpass_Clicked(System.Object sender, System.EventArgs e)
        {

            try
            {

                loginstack.IsVisible = false;

                forgotpasswordstack.IsVisible = true;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "forgotpass_Clicked");
            }
        }

        void emailfptxt_TextChanged(System.Object sender, TextChangedEventArgs e)
        {
        }

        async void passwordresetbtn_Clicked(System.Object sender, System.EventArgs e)
        {
            //password reset email
            try
            {

                if (string.IsNullOrEmpty(emailfptxt.Text))
                {
                    emailfphint.HasError = true;
                    emailfphint.ErrorText = "Please enter an email address";
                    Vibration.Vibrate();
                    emailfptxt.Focus();
                    return;
                }

                passwordresetbtn.IsEnabled = false;
                string emailtorest = emailfptxt.Text.TrimEnd();
                var UserToReset = await usermanager.checkUser(emailtorest);

                //If user exists, send password reset email
                if (UserToReset.Count > 0)
                {


                    HttpClient hTTPClient = new HttpClient();

                    String json = String.Format(@"{{""Email"":""{0}""}}", emailtorest);

                    StringContent mail_content = new StringContent(json);

                    //  await DisplayAlert("Password Reset Email Sent", "If the email is present in our database - you will receive a password reset email", "OK");
                    //  Navigation.RemovePage(this);


                    try
                    {
                        var response = await hTTPClient.PostAsync("https://peoplewithfunction.azurewebsites.net/api/ResetPassword", mail_content);


                        //If success - inform user and return to login
                        if (response.IsSuccessStatusCode)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(content);


                            //success message
                            await DisplayAlert("Success", "Please check your emails - you will receive a password reset email", "OK");

                            forgotpasswordstack.IsVisible = false;
                            loginstack.IsVisible = true;
                        }
                    }

                    catch (Exception ex)
                    {
                        await DisplayAlert("Oops", "Something went wrong - Please try again", "OK");
                        Vibration.Vibrate();
                        // Debug.WriteLine(ex.ToString());
                    }
                }

                else

                {
                    //inform even if email is not in the database - makes it easier to determine users in the database
                    await DisplayAlert("Password Reset", "If the email is present in our database - you will receive a password reset email", "OK");
                    forgotpasswordstack.IsVisible = false;
                    loginstack.IsVisible = true;
                    // Navigation.RemovePage(this);
                }

                passwordresetbtn.IsEnabled = true;
                Analytics.TrackEvent("Login Page - Forgot Password Clicked");
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "passwordresetbtn_Clicked");
                passwordresetbtn.IsEnabled = true;
            }


        }

        async void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new PrivacyPolicyPage(), false);
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped");
            }
        }
    }
}

