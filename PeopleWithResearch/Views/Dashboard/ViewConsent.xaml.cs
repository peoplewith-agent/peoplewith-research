using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace PeopleWithResearch;

public partial class ViewConsent : ContentPage
{
    public List<ConsentDetails> AllConsentForms { get; set; } = new();
    public ConsentDetails SelectedConsent { get; set; } = new();
    public userconsent UsersConsent = new();
    public ObservableCollection<signupcode> SignupCode = new();
    public string SelectedAgeGroup { get; set; } = Helpers.Settings.Age;
    public householdgroup HouseholdInfo { get; set; } = new();
    public bool IsHHRep = false;

    //Connection Strings
    public string Filename { get; set; } = string.Empty;
    private const string BlobRetrieve = "https://peoplewithappiamges.blob.core.windows.net/consentsignatures/[IMAGEFILENAME]?sv=2020-08-04&si=consentsignatures-194458F3E49&sr=c&sig=yCTwfPyHxvLhSxB4pgXSx5bDNIcHffYlFkdN0bkgecU%3D";
    public void NotasyncMethod(Exception Ex) { CrashDetected.LogCrash(Ex, "ViewConsent"); }
    public ViewConsent(string HouseholdRepID, householdgroup householdInfoPassed)
    {
        InitializeComponent();
        HouseholdInfo = householdInfoPassed;
        IsHHRep = Helpers.Settings.UsersID == HouseholdRepID;
        LoadConsentData();
    }

    // protected override async void OnAppearing()
    // {
    //     base.OnAppearing();
    //     try
    //     {         
    //         await SetLoadingVisibility(true);
    //         await LoadConsentData();   
    //     }
    //     catch(Exception Ex)
    //     {
    //          NotasyncMethod(Ex); 
    //          await SetLoadingVisibility(false, true);
    //     }
    // }

    private async Task LoadConsentData()
{
    try
    {
        var signupCodeTask = APICalls.Instance.GetSingupCode();
        var userConsentTask = APICalls.Instance.GetUserConsent();

        await Task.WhenAll(signupCodeTask, userConsentTask);

        SignupCode = await signupCodeTask;
        var consent = await userConsentTask;

        var json = SignupCode?.FirstOrDefault()?.consent;

        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var parsedForms = JsonSerializer.Deserialize<List<ConsentDetails>>(json);
                AllConsentForms = parsedForms ?? new List<ConsentDetails>();
            }
            catch (JsonException ex)
            {
                AllConsentForms = new List<ConsentDetails>();
            }
        }
        else
        {
            AllConsentForms = new List<ConsentDetails>();
        }

        string ageGroup = string.Empty;
        if(IsHHRep) ageGroup = "16+";
        else{ ageGroup = HouseholdInfo.userdetailslist.FirstOrDefault(x=> x.household_individual_userid == Helpers.Settings.UsersID)?.household_individual_age  ?? string.Empty; }
        if(string.IsNullOrWhiteSpace(ageGroup)) return;
        SelectedConsent = AllConsentForms.FirstOrDefault(x => x.age == ageGroup);
        UsersConsent = consent?.FirstOrDefault();
        Under10StackLayout.IsVisible = ageGroup != "16+";
        Over16NameEntry.Text = $"{Helpers.Settings.FirstName} {Helpers.Settings.Surname}";
        var signoffParameters = SelectedConsent.signoffparameters;
        Over16NameLabel.Text = signoffParameters.FirstOrDefault().label;
        Over16SignatureLabel.Text = signoffParameters.LastOrDefault().label;
        var listOfRoles = signoffParameters.FirstOrDefault(x => x.type == "dropdown");
        if (listOfRoles is not null)
        {
            Under10RoleListView.ItemsSource = new ObservableCollection<SignoffOption>(listOfRoles.options ?? new List<SignoffOption>());
        }
        ConsentIDLabel.Text = UsersConsent?.userconsentid ?? "Consent ID not available";
        Filename = UsersConsent?.signaturefilename ?? string.Empty;
        await SetBlobImage();
        await SetContentData(); 
        await SetCollectionViewData();
    }
    catch (Exception ex)
    {
        NotasyncMethod(ex);
        await SetLoadingVisibility(false, true);
    }
    finally
    {
        await SetLoadingVisibility(false);
    }
}

private async Task SetBlobImage()
{
        var newblobstring = BlobRetrieve.Replace("[IMAGEFILENAME]", Filename);
        SignatureImage.Source = ImageSource.FromUri(new Uri(newblobstring));
}



    private async Task SetLoadingVisibility(bool isVisible, bool Crash = false)
    {
        if (Crash)
        {
            Content.IsVisible = false; 
            Loading.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            FailedView.IsVisible = true;
        }
        else
        {
            Content.IsVisible = !isVisible;
            Loading.IsVisible = isVisible;
            LoadingIndicator.IsRunning = isVisible;
            FailedView.IsVisible = false;
        }
    }

    private async Task SetContentData()
    {
        try
        {
            datelbl.Text =$"Uploaded: {UsersConsent.DateCreated.ToString("ddd, dd MMM yyyy HH:mm")}";
        }
        catch (Exception ex)
        {
            NotasyncMethod(ex);
        }
    }

    private async Task SetCollectionViewData()
    {
        try
        {

            var optionalstrings = UsersConsent.consentselection;
            foreach(var item in SelectedConsent.consentcontent)
            {          
               foreach(var subItem in item.sectioncontent)
                {
                     if(item.section == "optional")
                     {
                         subItem.ChckedState = !string.IsNullOrWhiteSpace(optionalstrings) && optionalstrings.Contains(subItem.consentitemid);
                     }   
                     else
                     {
                        subItem.ChckedState = true;
                     }        

                }
            }
            DescriptionLabel.Text = SelectedConsent.title;   
            ConsentCollectionView.ItemsSource = SelectedConsent.consentcontent;
        }
        catch (Exception ex)
        {
            NotasyncMethod(ex);
        }
    }
}
