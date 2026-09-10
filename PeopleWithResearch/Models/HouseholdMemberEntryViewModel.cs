using CommunityToolkit.Mvvm.ComponentModel;
using PeopleWithResearch;
using System.Collections.ObjectModel;

namespace PeopleWithResearch;

/// <summary>
/// One "add a household member" entry — the mainuserstack section on the original Imperial
/// page has two of these side by side (originally distinguished by having no suffix vs. a
/// "2" suffix on every control name, e.g. firstfamentry / firstfamentry2). Same fields, same
/// validation, same conditional show/hide rules, so it's one class used twice
/// (NewImperialViewModel.Member1 / .Member2) instead of ~20 duplicated properties.
/// </summary>
public partial class HouseholdMemberEntryViewModel : ObservableObject
{
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _firstNameError = string.Empty;

    [ObservableProperty] private string _surname = string.Empty;
    [ObservableProperty] private string _surnameError = string.Empty;

    // "Yes, own phone" / "No, I'll take part for them" — a plain string list in the original
    // (set once from a hard-coded list in LoadRegistrationConfigAsync, not server config), so
    // this binds a bare string, not an OptionDetails.
    [ObservableProperty] private ObservableCollection<string> _phoneOptions = new();
    [ObservableProperty] private string? _selectedPhoneOption;
    [ObservableProperty] private string _phoneError = string.Empty;

    [ObservableProperty] private bool _isEmailSectionVisible;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _emailError = string.Empty;

    // "5 - 10" / "11 - 15" / "16+" — also a plain string list, same reasoning as PhoneOptions.
    [ObservableProperty] private ObservableCollection<string> _ageOptions = new();
    [ObservableProperty] private string? _selectedAgeOption;
    [ObservableProperty] private string _ageError = string.Empty;

    // Relationship IS server-config-driven (from the "familyrelationship" subfield), so this
    // one is OptionDetails like everywhere else.
    [ObservableProperty] private ObservableCollection<OptionDetails> _relationshipOptions = new();
    [ObservableProperty] private OptionDetails? _selectedRelationshipOption;
    [ObservableProperty] private string _relationshipError = string.Empty;

    [ObservableProperty] private bool _isConsentVisible;
    [ObservableProperty] private bool _isConsentChecked;
    [ObservableProperty] private string _consentError = string.Empty;

    partial void OnFirstNameChanged(string value) => FirstNameError = string.Empty;
    partial void OnSurnameChanged(string value) => SurnameError = string.Empty;
    partial void OnEmailChanged(string value) => EmailError = string.Empty;
    partial void OnIsConsentCheckedChanged(bool value) => ConsentError = string.Empty;

    partial void OnSelectedPhoneOptionChanged(string? value)
    {
        if (value is null) return;
        PhoneError = string.Empty;

        bool hasOwnPhone = value.Contains("Yes");
        IsEmailSectionVisible = hasOwnPhone;
        IsConsentVisible = hasOwnPhone;
    }

    partial void OnSelectedAgeOptionChanged(string? value)
    {
        if (value is not null) AgeError = string.Empty;
    }

    partial void OnSelectedRelationshipOptionChanged(OptionDetails? value)
    {
        if (value is not null) RelationshipError = string.Empty;
    }

    /// <summary>Was the per-member half of ValidateFormStack().</summary>
    public bool Validate()
    {
        bool isValid = true;

        if (string.IsNullOrEmpty(FirstName))
        {
            FirstNameError = "Please complete";
            isValid = false;
        }

        if (string.IsNullOrEmpty(Surname))
        {
            SurnameError = "Please complete";
            isValid = false;
        }

        if (SelectedPhoneOption is null)
        {
            PhoneError = "Select an option";
            isValid = false;
        }

        if (IsEmailSectionVisible)
        {
            if (string.IsNullOrEmpty(Email))
            {
                EmailError = "Please enter email address";
                isValid = false;
            }
            else if (!NewImperialViewModel.IsEmailValid(Email))
            {
                EmailError = "Please enter a valid email address";
                isValid = false;
            }
        }

        if (SelectedAgeOption is null)
        {
            AgeError = "Select an option";
            isValid = false;
        }

        if (SelectedRelationshipOption is null)
        {
            RelationshipError = "Select an option";
            isValid = false;
        }

        if (IsConsentVisible && !IsConsentChecked)
        {
            ConsentError = "Please agree to terms";
            isValid = false;
        }

        return isValid;
    }

    public void ClearErrors()
    {
        FirstNameError = string.Empty;
        SurnameError = string.Empty;
        PhoneError = string.Empty;
        EmailError = string.Empty;
        AgeError = string.Empty;
        RelationshipError = string.Empty;
        ConsentError = string.Empty;
    }
}
