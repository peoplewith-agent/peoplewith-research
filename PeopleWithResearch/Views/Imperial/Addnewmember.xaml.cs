using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Mopups.Services;
using Newtonsoft.Json;


namespace PeopleWithResearch;

public partial class Addnewmember : ContentPage
{
    householdgroup householdgroupdetailspassed = new householdgroup();
    private bool _isSubmitting;
    public Addnewmember()
	{
		InitializeComponent();
	}


    public Addnewmember(householdgroup householdgroupdetails)
    {
        InitializeComponent();

        householdgroupdetailspassed = householdgroupdetails;

        var stringlist = new List<string>
        {
            "0 - 4",
            "5 - 12",
            "13 - 15",
            "16+"
        };

        familyagelist.ItemsSource = stringlist;


        var relationships = new List<string>
        {
            LocalizationManager.Get("AddMember_RelParentGuardian"),
            LocalizationManager.Get("AddMember_RelPartnerSpouse"),
            LocalizationManager.Get("AddMember_RelChild"),
            LocalizationManager.Get("AddMember_RelOther")
        };

        familymember1list.ItemsSource = relationships;


        var stringlistphone = new List<string>
        {
            LocalizationManager.Get("AddMember_PhoneYes"),
            LocalizationManager.Get("AddMember_PhoneNo")
        };

        usingphone1.ItemsSource = stringlistphone;

        // Localize placeholders and error text
        firstfamentry.Placeholder = LocalizationManager.Get("AddMember_FirstNamePlaceholder");
        firstsurnameentry.Placeholder = LocalizationManager.Get("AddMember_SurnamePlaceholder");
        firstemailentry.Placeholder = LocalizationManager.Get("AddMember_EmailPlaceholder");
        firstfamhelper.ErrorText = LocalizationManager.Get("Common_PleaseComplete");
        firstsurnamehelper.ErrorText = LocalizationManager.Get("Common_PleaseComplete");
        firstemailhelper.ErrorText = LocalizationManager.Get("Common_PleaseComplete");

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


    async Task<bool> AddValidation()
    {
        try
        {
            if (string.IsNullOrEmpty(firstfamentry.Text))
            {
                Vibration.Vibrate();
                firstfamhelper.HasError = true;
                return false;
            }

            if (string.IsNullOrEmpty(firstsurnameentry.Text))
            {
                Vibration.Vibrate();
                firstsurnamehelper.HasError = true;
                return false;
            }

            if (usingphone1.SelectedItems.Count == 0)
            {
                Vibration.Vibrate();
                usingphone1error1.IsVisible = true;
                return false;
            }

            if (email1lbl.IsVisible)
            {

                if (string.IsNullOrEmpty(firstemailentry.Text))
                {
                    Vibration.Vibrate();
                    firstemailhelper.HasError = true;
                    return false;
                }
                // ---- Email invalid ----
                else if (!EmailIsValid(firstemailentry.Text))
                {
                    Vibration.Vibrate();
                    firstemailhelper.HasError = true;
                    firstemailhelper.ErrorText = LocalizationManager.Get("AddMember_EmailInvalid");
                    return false;
                }

                //check if email isnt already added

                if (householdgroupdetailspassed.userdetailslist.Any(x => x.household_individual_email == firstemailentry.Text))
                {
                    Vibration.Vibrate();
                    firstemailhelper.HasError = true;
                    firstemailhelper.ErrorText = LocalizationManager.Get("AddMember_EmailFamilyExists");
                    return false;
                }


                var checkuseremail = await APICalls.Instance.CheckEmailExists(firstemailentry.Text);


                if (checkuseremail?.Count > 0)
                {
                    Vibration.Vibrate();
                    firstemailhelper.HasError = true;
                    firstemailhelper.ErrorText = LocalizationManager.Get("AddMember_EmailExists");
                    return false;
                }

            }

            if (familyagelist.SelectedItems.Count == 0)
            {
                Vibration.Vibrate();
                agemember1error.IsVisible = true;
                return false;
            }


            if (familymember1list.SelectedItems.Count == 0)
            {
                Vibration.Vibrate();
                typemember1error.IsVisible = true;
                return false;
            }

            if (email1lbl.IsVisible)
            {
                if (!firsthouselholdcb.IsChecked)
                {
                    Vibration.Vibrate();
                    firstcheckboxerror.IsVisible = true;
                    return false;
                }
            }
            //All Pass 
            return true; 
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "AddValidation");
            return false; 
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (_isSubmitting) return;
        _isSubmitting = true;
        //add new member button 

        //Stops Send Tap
        var addButton = sender as Button;
        if (addButton != null) addButton.IsEnabled = false;

        try
        {
            //check validation on member
            bool Vaidation = await AddValidation();
            if (!Vaidation) return; 
            
          //  var newmembers = new ObservableCollection<householdgroupjsondetails>();

            var newone = new householdgroupjsondetails();
            newone.household_group_id = householdgroupdetailspassed.householdgroupid;
            newone.household_individual_name = firstfamentry.Text.Trim() + " " + firstsurnameentry.Text.Trim();
            newone.household_individual_email = email1lbl.IsVisible ? firstemailentry.Text?.Trim() : null;
            newone.household_individual_status = "onboarding";
            // Map selected localized relationship back to English for DB
            var relEnglish = new[] { "I am their parent/guardian", "I am their partner/spouse", "I am their child", "Other" };
            var relLocalized = new[] {
                LocalizationManager.Get("AddMember_RelParentGuardian"),
                LocalizationManager.Get("AddMember_RelPartnerSpouse"),
                LocalizationManager.Get("AddMember_RelChild"),
                LocalizationManager.Get("AddMember_RelOther")
            };
            var selectedRel = familymember1list.SelectedItem?.ToString()?.Trim() ?? "";
            int relIdx = Array.IndexOf(relLocalized, selectedRel);
            newone.household_individual_relationship = relIdx >= 0 ? relEnglish[relIdx] : selectedRel;
            newone.household_individual_age = familyagelist.SelectedItem.ToString().Trim();
            householdgroupdetailspassed.userdetailslist.Add(newone);


            var uploadList = householdgroupdetailspassed.userdetailslist
    .Where(x => x.household_individual_relationship != "Household Rep")
    .ToList();

            string json = System.Text.Json.JsonSerializer.Serialize(uploadList);
            householdgroupdetailspassed.groupuserdetails = json;

            //householdgroupdetailspassed.userdetailslist.Add(newone);

            // 1. Serialize with explicit "None" formatting to prevent line breaks
            //  string json = JsonConvert.SerializeObject(newmembers, Newtonsoft.Json.Formatting.None);

            // 2. Force-strip ANY whitespace or line breaks from the JSON string itself
            json = json.Replace("\r", "").Replace("\n", "").Trim();

            // --- DEBUG CHECK: The brackets should be tight: [{"... "}] ---
            //Debug.WriteLine($"CHECK JSON: [{json}]");

            // 3. Convert to Base64
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            string base64EncodedMembers = Convert.ToBase64String(jsonBytes);

            // 4. URL Encode (and trim again just in case)
            string safeBase64 = System.Net.WebUtility.UrlEncode(base64EncodedMembers).Trim();

            // 5. Build the final URL and strip any potential breaks again
            string baseUrl = "https://hopper.peoplewith.com/household-individual-alignment.php";
            string finalUrl = $"{baseUrl}?hij={base64EncodedMembers}".Replace("\r", "").Replace("\n", "").Trim();

            //Debug.WriteLine($"CHECK FINAL URL: {finalUrl}");

            // 6. Send
            var httpClient = APICalls.Instance.GetClient();
            HttpResponseMessage response = await httpClient.GetAsync(finalUrl);

            if (response.IsSuccessStatusCode)
            {
                // Success!
                //  Debug.WriteLine("Payload sent successfully");
            }
            else
            {
                var s = response.StatusCode + response.ReasonPhrase;
                //Debug.WriteLine($"Failed to send: {response.StatusCode}");
            }

            await MopupService.Instance.PushAsync(new PopupPageHelper(LocalizationManager.Get("AddMember_NewMemberAdded")) { });

            await Task.Delay(1500);

            //  await Navigation.PushAsync(new AllSymptoms(SymptomsPassed, userfeedbacklistpassed));

            await Navigation.PushAsync(new ImperialDashboard(), false);

            await MopupService.Instance.PopAllAsync(false);
            Navigation.RemovePage(this);
        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Button_Clicked");
        }
        finally
        {
            _isSubmitting = false;
            if (addButton != null) addButton.IsEnabled = true;
        }
    }

    private void firstfamentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            firstfamhelper.HasError = false;
        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "firstfamentry_TextChanged");
        }
    }


    private void firstsurnameentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            firstsurnamehelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "firstsurnameentry_TextChanged");
        }
    }

    private void firstemailentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            firstemailhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "firstemailentry_TextChanged");
        }
    }

    private void familyagelist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            agemember1error.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "familyagelist_ItemTapped");
        }
    }

    private void familymember1list_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            typemember1error.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "familymember1list_ItemTapped");
        }
    }

    private void usingphone1_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            usingphone1error1.IsVisible = false;

            var item = e.DataItem as string;

            // Index 0 = "Yes, own phone" in any language
            int tappedIndex = usingphone1.ItemsSource is List<string> phoneList ? phoneList.IndexOf(item) : -1;
            bool isOwnPhone = tappedIndex == 0;

            if (isOwnPhone)
            {
                firstemailhelper.IsVisible = true;
                email1lbl.IsVisible = true;
                email2lbl.IsVisible = true;
                commborder1.IsVisible = true;
            }
            else
            {
                firstemailhelper.IsVisible = false;
                email1lbl.IsVisible = false;
                email2lbl.IsVisible = false;
                commborder1.IsVisible = false;
                firstcheckboxerror.IsVisible = false;
            }
        }
        catch(Exception ex)
        {

        }
    }

    private void firsthouselholdcb_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            firstcheckboxerror.IsVisible = false;
        }
        catch (Exception Ex)
        {
           
        }
    }
}