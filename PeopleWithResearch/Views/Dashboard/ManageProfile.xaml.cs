using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using Newtonsoft.Json;
using Syncfusion.Maui.Core;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PeopleWithResearch;

public partial class ManageProfile : ContentPage
{

    private readonly householdgroupjsondetails UserDetails;
    private readonly ObservableCollection<householdgroupjsondetails> AllUserDetails;
    //bool NameChanged = false;
    //bool EmailChanged = false;
    //bool Relationshipchanged = false; 
    //bool AgeChanged = false;
    List<string> AgeOptions = new();
    List<String> RelationOptions = new();
    public ManageProfile(householdgroupjsondetails details, ObservableCollection<householdgroupjsondetails> AllGroupDetails)
    {
        InitializeComponent();
        UserDetails = details ?? throw new ArgumentNullException(nameof(details));
        AllUserDetails = AllGroupDetails; 
        AgeOptions = Genericlist.AgeOptions;
        RelationOptions = Genericlist.RelationOptions;

        RelationListview.ItemsSource = RelationOptions;
        Agelistview.ItemsSource = AgeOptions;

        if (UserDetails != null)
        {
            var status = UserDetails.household_individual_status ?? string.Empty;
            bool isActive = status.Equals("active", StringComparison.OrdinalIgnoreCase);
            bool isOnboarding = status.Equals("onboarding", StringComparison.OrdinalIgnoreCase);

            //Default
            StatusHelper.ContainerBackground = Color.FromArgb("#84FAB0");
            ActiveDot.Fill = Color.FromArgb("#2D6A53");
            StatusHelper.Stroke = Color.FromArgb("#2D6A53");
            StatusEntry.TextColor = Color.FromArgb("#2D6A53");

            if (!isActive)
            {
                StatusHelper.ContainerBackground = Color.FromArgb("#FFD6D6");
                ActiveDot.Fill = Color.FromArgb("#A31D1D");
                StatusHelper.Stroke = Color.FromArgb("#A31D1D");
                StatusEntry.TextColor = Color.FromArgb("#A31D1D");
            }
            if (isOnboarding)
            {
                StatusHelper.ContainerBackground = Color.FromArgb("#FFEAD2");
                ActiveDot.Fill = Color.FromArgb("#D35400");
                StatusHelper.Stroke = Color.FromArgb("#D35400");             
                StatusEntry.TextColor = Color.FromArgb("#D35400");
            }

            Agelistview.SelectedItem = UserDetails.household_individual_age;
            RelationListview.SelectedItem = UserDetails.household_individual_relationship; 

            var fullname = UserDetails.household_individual_name.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(fullname))
            {
                string[] nameParts = fullname.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (nameParts.Length >= 2)
                {
                    NameEntry.Text = nameParts[0];
                    SurnameEntry.Text = string.Join(" ", nameParts.Skip(1));
                }
                else if (nameParts.Length == 1)
                {
                    NameEntry.Text = nameParts[0];
                    SurnameEntry.Text = string.Empty;
                }
            }
        }

        BindingContext = UserDetails;

        NameEntry.TextChanged += Generic_TextChanged;
        SurnameEntry.TextChanged += Generic_TextChanged;
        EmailEntry.TextChanged += Generic_TextChanged;
    }

    private void Generic_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            Editbtn.IsEnabled = true;
            Instructionlbl.IsVisible = false;
            if (sender == NameEntry)
            {
                SetHelpers(NameHelper, NameEntry, string.Empty);
                //NameChanged = true;
            }
            else if (sender == SurnameEntry)
            {
                SetHelpers(SurnameHelper, SurnameEntry, string.Empty);
                //NameChanged = true;
            }
            else if (sender == EmailEntry)
            {
                SetHelpers(EmailHelper, EmailEntry, string.Empty);
                //EmailChanged = true;
            }
            ButtonStack.IsVisible = true; 
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Generic_TextChanged");
        }
    }

    private void Showerrorlbl(Label lbl)
    {
        lbl.IsVisible = true;
        Vibration.Vibrate(); 
    }
    private void SetHelpers(SfTextInputLayout input, Entry field, string Msg, bool Set = false)
    {
        input.HasError = Set;
        input.ErrorText = Msg;

        if (Set)
        {
            Vibration.Vibrate();
            field.Focus();
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

    private bool SubmitHasError()
    {
        try
        {
            string cleanName = NameEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(NameEntry.Text))
            {
                SetHelpers(NameHelper, NameEntry, "Enter a first name", true);
                return true;
            }

            if (string.IsNullOrEmpty(SurnameEntry.Text))
            {
                SetHelpers(SurnameHelper, SurnameEntry, "Enter a surname", true);
                return true;
            }

            if (string.IsNullOrEmpty(EmailEntry.Text))
            {
                SetHelpers(EmailHelper, EmailEntry, "Please enter an email address", true);
                return true;
            }

            if (!EmailIsValid(EmailEntry.Text))
            {
                SetHelpers(EmailHelper, EmailEntry, "Please enter a valid email address", true);
                return true;
            }

            if (Agelistview.SelectedItem is not string Selected)
            {
                Showerrorlbl(AgeError);
                return true;
            }

            if (RelationListview.SelectedItem is not string SelectedItem)
            {
                Showerrorlbl(RelationError);
                return true;
            }

            return false;

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "SubmitHasError");
            return true; 
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            var Button = sender as Button;
            if (Button == null) return; 
            if(Button.Text == "Enable Edit")
            {
                Editbtn.Text = "Save Changes";
                NameEntry.IsReadOnly = false;
                SurnameEntry.IsReadOnly = false;
                EmailEntry.IsReadOnly = false;
                Agelistview.IsEnabled = true;
                RelationListview.IsEnabled = true;
                Instructionlbl.Text = "Edit a field to enable 'Save Changes'";
                Editbtn.IsEnabled = false; 
                return; 
            }

            if (SubmitHasError())
            {
                Editbtn.IsEnabled = false;
                return;
            }

                Editbtn.IsEnabled = false;

            var itemUpdate = AllUserDetails.FirstOrDefault(f =>
                f.household_individual_userid == UserDetails.household_individual_userid);

            if (itemUpdate == null)
            {
                Editbtn.IsEnabled = true;
                await DisplayAlert("Update Failed", "We couldn't retrieve this specific users data. Please try again later. If the problem persists, please contact support.", "OK");
                return;
            }

            itemUpdate.household_individual_name = $"{NameEntry.Text?.Trim()} {SurnameEntry.Text?.Trim()}";
            itemUpdate.household_individual_email = EmailEntry.Text?.Trim();
            itemUpdate.household_individual_age = Agelistview.SelectedItem?.ToString();
            itemUpdate.household_individual_relationship = RelationListview.SelectedItem?.ToString();
            //Rest from Upper
            itemUpdate.household_individual_status = itemUpdate.household_individual_status.ToLower();

            bool isSuccessful = await APICalls.Instance.UpdateHouseholdFeedback(AllUserDetails);

            if (!isSuccessful)
            {
                Editbtn.IsEnabled = true;
                await DisplayAlert("Save Failed", "We couldn't save your data. Please try again later. If the problem persists, please contact support.", "OK");
                return;
            }
            

            await MopupService.Instance.PushAsync(new PopupPageHelper("Profile Updated"));
            WeakReferenceMessenger.Default.Send(new UpdateHouseHoldGroup(AllUserDetails));

            await Task.Delay(1500);
            await MopupService.Instance.PopAllAsync(false);

            await Navigation.PopAsync();
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Button_Clicked");
        }
    }

    private void ItemTappedEvent(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        Editbtn.IsEnabled = true;
        Instructionlbl.IsVisible = false; 
        if (sender == Agelistview)
        {
            //AgeChanged = true;
            AgeError.IsVisible = false;
        }
        else if(sender == RelationListview)
        {
            //Relationshipchanged = true;
            RelationError.IsVisible = false;
        }
        ButtonStack.IsVisible = true;
    }


    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(UserIDEntry.Text);

            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                var toast = Toast.Make("Copied to clipboard");
                await toast.Show();
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped");
        }     
    }
}