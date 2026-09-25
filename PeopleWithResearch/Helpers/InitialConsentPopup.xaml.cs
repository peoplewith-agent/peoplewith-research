using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Pages;
using Mopups.Services;

namespace PeopleWithResearch;
public partial class InitialConsentPopup : PopupPage
{
    public event EventHandler<bool>? ConnectivityChanged;
    const string StudyEmail = "hopper-study@imperial.ac.uk";
    const string StudyPhone = "+447889952491";
    public string Filename { get; set; } = string.Empty;
    public List<ConsentDetails> AllConsentForms { get; set; } = new();
    public userconsent UsersConsent = new();
     public ConsentDetails SelectedConsent { get; set; } = new();
    public ObservableCollection<signupcode> SignupCode = new();

    private const string BlobRetrieve = "https://peoplewithappiamges.blob.core.windows.net/consentsignatures/[IMAGEFILENAME]?sv=2020-08-04&si=consentsignatures-194458F3E49&sr=c&sig=yCTwfPyHxvLhSxB4pgXSx5bDNIcHffYlFkdN0bkgecU%3D";

    CrashDetected crashHandler;
    public void NotasyncMethod(Exception Ex) { CrashDetected.LogCrash(Ex, "InitialConsentPopup"); }

    public InitialConsentPopup()
    {
        try
        {
            InitializeComponent();
            BindingContext = this;
            Loaddata(); 
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }

    private async Task<string> FetchJsonAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("consent.json");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    async Task Loaddata()
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
        UsersConsent = consent?.FirstOrDefault();
        SelectedConsent = AllConsentForms.FirstOrDefault(x => x.age == "16+");
        ConsentIDLabel.Text = UsersConsent?.userconsentid ?? "Consent ID not available";
        Filename = UsersConsent?.signaturefilename ?? string.Empty;
        datelbl.Text =$"Uploaded: {UsersConsent.DateCreated.ToString("ddd, dd MMM yyyy HH:mm")}";
        await SetBlobImage();
        await SetCollectionViewData(); 

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
            //var optionalstrings = UsersConsent.consentselection;
            foreach(var item in SelectedConsent.consentcontent)
            {          
               foreach(var subItem in item.sectioncontent)
                {
                    //  if(item.section == "optional")
                    //  {
                    //      subItem.ChckedState = !string.IsNullOrWhiteSpace(optionalstrings) && optionalstrings.Contains(subItem.consentitemid);
                    //  }   
                    //  else
                    //  {
                        subItem.ChckedState = true;
                     //}        

                }
            }   
            DescriptionLabel.Text = SelectedConsent.title;
            ConsentCollectionView.ItemsSource = SelectedConsent.consentcontent;
        }
        catch (Exception ex)
        {
        }
    }

    private async Task SetBlobImage()
    {
         var newblobstring = BlobRetrieve.Replace("[IMAGEFILENAME]", Filename);
         SignatureImage.Source = ImageSource.FromUri(new Uri(newblobstring));
    }



    async void OnEmailTapped(object sender, EventArgs e)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri($"mailto:{StudyEmail}"));
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }

    async void OnPhoneTapped(object sender, EventArgs e)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri($"tel:{StudyPhone}"));
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }

    private async void OnCancelTapped(object sender, EventArgs e)
    {
        try
        {
            await MopupService.Instance.PopAsync();
        }
        catch (Exception Ex) { NotasyncMethod(Ex); }
    }

    protected override bool OnBackgroundClicked()
    {
        return base.OnBackgroundClicked();
    }
}