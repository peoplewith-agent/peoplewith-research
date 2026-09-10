

using Azure.Storage.Blobs;
using FreakyKit.Utils;
using Mopups.Services;
using Newtonsoft.Json;
using Syncfusion.Maui.Core;
using Syncfusion.Maui.Core.Internals;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace PeopleWithResearch;

public partial class Imperial : ContentPage
{
    public newuser newuser = new newuser();
    public user userdetails;
    public signupcode signupcodedetails;
    public Questionnaire questionnairedetails;
    public double progressamount;
    public double progressamountquestionnaire;
    public ObservableCollection<RegField> Allregfields = new ObservableCollection<RegField>();
    public ObservableCollection<RegField> AllregfieldsNotRequired = new ObservableCollection<RegField>();
    public List<QuestionModel> Allquesfields = new List<QuestionModel>();
    private int currentFieldIndex = 0;
    private int currentFieldIndexquestionnaire = 0;
    bool SignPadhaddata = false;

    public List<SectionConsent> Groupeddata = new();
    public List<RegField> Over16regfields = new List<RegField>();
    public List<RegField> Over16regfieldsmain = new List<RegField>();
    public List<RegField> Nonrequiredfields = new List<RegField>();
    public List<OptionDetails> GPPracticeLsit = new();
    public OptionDetails SelectedGp = new();
    bool isEditing;
    bool validdob;
    bool validnhsnum;
    public QuestionManager questionamanager;
    public ObservableCollection<OptionDetails> SelectedConditons = new ObservableCollection<OptionDetails>();
    public ObservableCollection<OptionDetails> SelectedMedications = new ObservableCollection<OptionDetails>();
    public string TandCNonReqired = string.Empty;

    public ObservableCollection<RegQuestionAnswerJson> UserSelectedQuestionnaire = new ObservableCollection<RegQuestionAnswerJson>();
    string genderatbirth;
    public RegField menstrualquestion;
    bool PCisEditing = false;
    bool NHSisEditing = false;
    bool householdrepFROMREG;
    //APICalls database = new APICalls();
    ConsentDetails allconsentdetails = null;
    public householdgroupjsondetails userinfoforbaseline;
    public ObservableCollection<householdgroupjsondetails> allgroupdetailspassed = new ObservableCollection<householdgroupjsondetails>();
    public RegField mainuserstacksave;
    public householdgroup allhousehouldgroup;
    public string AddHouseholdURl = string.Empty;
    public UserNotifications AddNotification = new();
    public bool noemailuserreg;

    // public Dictionary<string,string?> UserQuestionnaireDetails = new Dictionary<string, string?>();

    public ObservableCollection<OptionDetails> UserDetails = new ObservableCollection<OptionDetails>();
    public List<QuestionnaireResult> QuestionnaireResults = new List<QuestionnaireResult>();

    private IEnumerable<SfTextInputLayout> NameHelpers => new[]
    { fnhelper,snhelper,emailhelper,passhelper,confirmpasshelper };
    private IEnumerable<SfTextInputLayout> MainUserHelpers => new[]
    { firstfamhelper,firstsurnamehelper,firstemailhelper,firstfamhelper2,firstsurnamehelper2,firstemailhelper2 };
    private IEnumerable<SfTextInputLayout> AddressHelpers => new[]
    { postcodehelper,addressonehelper,townhelper,countyhelper };
    private IEnumerable<SfTextInputLayout> GenderHelpers => new[]
    { dobhelper };
    private IEnumerable<SfTextInputLayout> BodyMetricsHelpers => new[]
    { weighthelper,heightHelper,heightcmhelper };

    private IEnumerable<SfTextInputLayout> HouseHoldHelpers => new[]
    { peoplenumhelper,roomnumhelper,sharedbathroomsnumhelper };


    public List<string> validpostcodelist = new List<string>();


    public Imperial()
    {
        InitializeComponent();
    }

    public Imperial(user userpassed, advert signupdetailspassed, Questionnaire questionnairepassed)
    {
        InitializeComponent();

        userdetails = userpassed;
        //   signupcodedetails = signupdetailspassed;
        // questionnairedetails = questionnairepassed;
        householdrepFROMREG = true;
        LoadRegistrationConfigAsync();

    }

    public Imperial(user userpassed, signupcode signupdetailspassed)
    {
        InitializeComponent();

        userdetails = userpassed;
        signupcodedetails = signupdetailspassed;
        //  questionnairedetails = questionnairepassed;
        householdrepFROMREG = true;
        LoadRegistrationConfigAsync();
    }

    public Imperial(signupcode signupdetailspassed, householdgroupjsondetails userinfopassed, ObservableCollection<householdgroupjsondetails> allgroupdetails, householdgroup passedhousehold)
    {
        InitializeComponent();

        userdetails = new user();
        //userdetails = userpassed;
        signupcodedetails = signupdetailspassed;
        userinfoforbaseline = userinfopassed;
        allgroupdetailspassed = allgroupdetails;
        allhousehouldgroup = passedhousehold;
        //  questionnairedetails = questionnairepassed;

        householdrepFROMREG = false;

        bannerTextLbl.Text = "You are completing this on behalf of " + userinfopassed.household_individual_name;
        bannerinfo.IsVisible = true;

        LoadRegistrationConfigAsync();


        //prepopulate some data
        emailentry.Text = userinfoforbaseline.household_individual_email;

        var splitname = userinfopassed.household_individual_name.Split(' ');
        firstnameentry.Text = splitname[0];
        surnameentry.Text = splitname[1];

        if (userinfoforbaseline.household_individual_age.Contains("10"))
        {
            under10entry.Text = userinfopassed.household_individual_name;
            under10entry.IsEnabled = false;
        }
        else
        {
            over16nameentry.Text = userinfopassed.household_individual_name;
            over16nameentry.IsEnabled = false;
        }


        if (userinfoforbaseline.household_individual_email == "N/A")
        {
            noemailuserreg = true;
            emailhelper.IsVisible = false;
            passhelper.IsVisible = false;
            confirmpasshelper.IsVisible = false;
            passlbl.IsVisible = false;
            passgrid.IsVisible = false;
            telhelper.IsVisible = false;
        }


        emailentry.IsEnabled = false;

        firstnameentry.IsEnabled = false;
        surnameentry.IsEnabled = false;


    }

    private async Task LoadRegistrationConfigAsync()
    {
        try
        {

            if (string.IsNullOrEmpty(signupcodedetails.appdetails))
            {
                return;
            }


            var config = JsonConvert.DeserializeObject<AppConfig>(signupcodedetails.appdetails);


            if (config != null)
            {

                var sortedList = config.RegFields
                    .Where(x => x.Active)
                    .OrderBy(x => int.TryParse(x.Order, out var orderVal) ? orderVal : int.MaxValue)
                    .ToList();

                // Create a new ObservableCollection using the list as the source
                Allregfields = new ObservableCollection<RegField>(sortedList);

                foreach (var item in Allregfields)
                {
                    foreach (var it in item.subFields)
                    {
                        if (it.Required)
                        {
                            it.Label = it.Label + " *";
                        }
                    }
                }



                //remove any items that are not needed if they are not the house rep
                if (householdrepFROMREG == false)
                {
                    mainuserstacksave = Allregfields.Where(x => x.XamlNameArea == "mainuserstack").FirstOrDefault();
                    Allregfields.RemoveAll(x => x.XamlNameArea == "mainuserstack");
                    Allregfields.RemoveAll(x => x.XamlNameArea == "addressstack");
                    Allregfields.RemoveAll(x => x.XamlNameArea == "householdstructurestack");
                }


                AllregfieldsNotRequired = new ObservableCollection<RegField>(
     Allregfields.Where(x => x.Required == false)
 );
                Allregfields = new ObservableCollection<RegField>(
    Allregfields.Where(x => x.Required == true)
);

                var getover16items = Allregfields.Where(x => x.Type == "over16").ToList();

                Over16regfields = getover16items;

                Allregfields.RemoveAll(x => x.Type == "over16");

                //check the optional addition questions
                var requiredfields = Allregfields.Where(x => x.Required == true).ToList();

                //set the progress bar counter
                var count = requiredfields.Count - 1;
                var max = requiredfields.Count - 1;

                topprogress.SegmentCount = count;
                topprogress.Maximum = max;
                topprogress.Minimum = 0;
                progressamount = 1;


                //  topprogress.Progress += progressamount;



                welcometoplbl.Text = "Welcome to the " + config.OverallSettings.StudyName;

                studytitlelbl.Text = config.OverallSettings.StudyTitle;


                welcomesublbl.Text = config.OverallSettings.StudyDescription;

                welcomestack.IsVisible = true;
                topprogress.IsVisible = false;
                nextbtn.Text = "Get Started";




            }


            var stringlistyn = new List<string>();
            stringlistyn.Add("Yes");
            stringlistyn.Add("No");


            hcfirstlist.ItemsSource = stringlistyn;
            medsfirstlist.ItemsSource = stringlistyn;

            //check if is the primary user

            var stringlist = new List<string>();
            stringlist.Add("5 - 10");
            stringlist.Add("11 - 15");
            stringlist.Add("16+");

            familymember1.ItemsSource = stringlist;
            familymember12.ItemsSource = stringlist;


            var stringlistphone = new List<string>();
            stringlistphone.Add("Yes, own phone");
            stringlistphone.Add("No, I'll take part for them");

            usingphone1.ItemsSource = stringlistphone;
            usingphone2.ItemsSource = stringlistphone;


            if (userdetails.Primaryuser == false)
            {
                var removenotprimaryuser = Allregfields.Where(x => x.XamlNameArea == "mainuserstack").FirstOrDefault();
                if (removenotprimaryuser != null)
                {
                    // menstrualquestion = getmenstrual;
                    Allregfields.Remove(removenotprimaryuser);
                }

                var removenotprimaryuseraddress = Allregfields.Where(x => x.XamlNameArea == "addressstack").FirstOrDefault();
                if (removenotprimaryuseraddress != null)
                {
                    // menstrualquestion = getmenstrual;
                    Allregfields.Remove(removenotprimaryuseraddress);
                }
            }

            //// --- Questionnaire Loading ---
            //var configquestionnaire = JsonConvert.DeserializeObject<List<QuestionModel>>(questionnairedetails.Consent4);

            //if (configquestionnaire != null && configquestionnaire.Count > 0)
            //{
            //    // Use the new helper method to convert groups into top-level RegFields
            //    List<RegField> newQuestionSections = ConvertQuestionGroupsToRegFields(configquestionnaire);

            //    if (newQuestionSections.Any())
            //    {
            //        // Append these new Group/Section RegFields to the main registration list
            //     //   Allregfields.AddRange(newQuestionSections);

            //        // Update main progress bar segments based on the new total
            //      //  topprogress.SegmentCount = Allregfields.Count;
            //      //  progressamount = (double)100 / Allregfields.Count;
            //    }

            //    // Keep Allquesfields reference if needed for complex logic, 
            //    // but now it's only used for the nested progress bar calculation.
            //    Allquesfields = configquestionnaire; // Still hold the raw groups if needed later

            //    // Recalculate secondary progress bar based on total number of QuestionModel groups
            //    var AllGroups = configquestionnaire
            //        .Where(g => g.Active)
            //        .ToList();

            //    if (AllGroups.Count > 0)
            //    {
            //        // The secondary progress bar tracks progress through these main groups/sections
            //        topprogress2.SegmentCount = AllGroups.Count;
            //        progressamountquestionnaire = 100.0 / AllGroups.Count;
            //        topprogress2.Progress = 0;
            //    }
            //}


            //var configquestionnaire = JsonConvert.DeserializeObject<List<QuestionModel>>(questionnairedetails.Consent4);

            //if (configquestionnaire != null && configquestionnaire.Count > 0)
            //{

            //    Allquesfields = configquestionnaire;
            //    // Only active groups, sorted by Order
            //    var AllGroups = configquestionnaire
            //        .Where(g => g.Active)
            //        .OrderBy(g => int.TryParse(g.Order, out var orderVal) ? orderVal : int.MaxValue)
            //        .ToList();

            //    // Set progress bar segments based on groups


            //    if (AllGroups.Count > 0)
            //    {
            //        // Each group counts as equal part of 100%
            //        progressamountquestionnaire = 100.0 / AllGroups.Count;
            //        topprogress2.SegmentCount = AllGroups.Count;
            //        // topprogress2.IsVisible = true;

            //        // Start progress at 0
            //        topprogress2.Progress = 0;
            //    }

            //}


            // var configquestionnaire = JsonConvert.DeserializeObject<List<QuestionModel>>(questionnairedetails.Consent4);
            //questionnairelist.ItemsSource = configquestionnaire[0];


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "LoadRegistrationConfigAsync");
        }
    }

    void SetError(Syncfusion.Maui.Core.SfTextInputLayout helper, string message)
    {
        helper.HasError = true;
        helper.ErrorText = message;
    }

    public RegField ConvertQuestionFieldToSubField(QuestionField qField)
    {
        try
        {
            return new RegField
            {
                Id = qField.QuestionId,
                Label = qField.Label,
                Type = qField.Type,
                Required = qField.Required,
                Placeholder = qField.Placeholder,
                Order = qField.Order,
                Active = qField.Active,
                XamlNameArea = qField.XamlNameArea,
                HelpText = qField.Directions, // Using Directions as HelpText for subfields

                // Nested Mapping: Convert List<QuestionAnswer> to List<OptionDetails>
                Options = qField.Answers
                    .Select(a => new OptionDetails
                    {
                        AnswerId = a.AnswerId,
                        Value = a.Value,
                        Text = a.Label
                    })
                    .ToList(),

                // Ensure subFields is null/empty as this is the bottom-level field
                subFields = new List<RegField>()
            };
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "ConvertQuestionFieldToSubField");
            return null;
        }
    }

    public List<RegField> ConvertQuestionGroupsToRegFields(List<QuestionModel> questionGroups)
    {
        try
        {
            var newRegFields = new List<RegField>();

            foreach (var group in questionGroups.Where(g => g.Active))
            {
                // 1. Create the Parent RegField from the QuestionModel (Group)
                var parentRegField = new RegField
                {
                    Id = group.Id,
                    Label = group.Label,
                    Type = "Questionnaire", // Use a specific Type to identify it as a questionnaire section
                    Required = group.Required,
                    Placeholder = group.Placeholder,
                    Order = group.Order,
                    Active = group.Active,
                    XamlNameArea = group.XamlNameArea, // e.g., "questionnairestack"

                    // Initialize subFields list for the questions
                    subFields = new List<RegField>()
                };

                // 2. Convert and add each QuestionField to the Parent's subFields
                var orderedQuestions = group.Fields
                    .Where(f => f.Active)
                    .OrderBy(f => int.TryParse(f.Order, out var orderVal) ? orderVal : int.MaxValue)
                    .ToList();

                foreach (var qField in orderedQuestions)
                {
                    var subField = ConvertQuestionFieldToSubField(qField);
                    parentRegField.subFields.Add(subField);
                }

                // 3. Add the fully populated Group RegField to the new list
                newRegFields.Add(parentRegField);
            }

            // 4. Sort the top-level sections
            return newRegFields
                .OrderBy(x => int.TryParse(x.Order, out var orderVal) ? orderVal : int.MaxValue)
                .ToList();
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "ConvertQuestionGroupsToRegFields");
            return null;
        }
    }

    public void ClearErrors(IEnumerable<SfTextInputLayout> helpers)
    {
        try
        {
            foreach (var h in helpers) h.HasError = false;
        }
        catch (Exception Ex) { CrashDetected.LogCrash(Ex, Navigation, "ClearErrors"); }
    }

    private Task Nextloader(bool OnOff)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                nextbtnloader.IsVisible = OnOff;
                nextbtnloader.IsRunning = OnOff;
                nextbtn.IsEnabled = !OnOff;
            }
            catch (Exception ex)
            {
                CrashDetected.LogCrash(ex, "Nextloader");
            }
        });

        return Task.CompletedTask;
    }


    private async Task ResetScroll()
    {
        try
        {
            if (mainscrollview.ScrollY > 0)
            {
                await mainscrollview.ScrollToAsync(0, 0, true);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "ResetScroll");
        }
    }

    private async Task CreateAccount()
    {
        try
        {
            if (!householdrepFROMREG)
            {
                //this means they are completing for another user
                newuser.userid = userinfoforbaseline.household_individual_userid;
                newuser.primaryuser = false;
                newuser.signupcodegrouping = signupcodedetails.signupcodegrouping;
                newuser.householdgroupid = userinfoforbaseline.household_group_id;
                newuser.signupcodeid = signupcodedetails.signupcodeid;
                newuser.postcode = Helpers.Settings.Postcode;
            }
            else
            {
                newuser.userid = userdetails.Userid;
                newuser.primaryuser = userdetails.Primaryuser;
                newuser.signupcodegrouping = userdetails.Signupcodegrouping;
                newuser.householdgroupid = userdetails.Householdgroupid;
                newuser.signupcodeid = userdetails.Signupid;
            }

            newuser.status = "active";

            var updateData = new
            {
                //userid = newuser.userid,
                firstname = newuser.firstname,
                surname = newuser.surname,
                gender = newuser.gender,
                status = newuser.status,
                ethnicity = newuser.ethnicity,
                email = newuser.email,
                password = newuser.password,
                postcode = newuser.postcode,
                signupcodeid = newuser.signupcodeid,
                signupcodegrouping = newuser.signupcodegrouping,
                primarycareid = newuser.primarycareid,
                primaryuser = newuser.primaryuser,
                householdgroupid = newuser.householdgroupid,
                dateofbirth = newuser.dateofbirth,
                details = newuser.details,
                telephone = newuser.telephone
            };

            // 2. Serialize this anonymous object instead of the 'newuser' class
            string json = System.Text.Json.JsonSerializer.Serialize(updateData);

            // 3. Send to the URL WITH the ID
            var urll = $"{APICalls.ApplicationURL}user/userid/{newuser.userid}";
            StringContent contentt = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await APICalls.Instance.GetClient().PatchAsync(urll, contentt);
            var configuredClient = APICalls.Instance.GetClient();
            var jsonusercheck = JsonConvert.SerializeObject(newuser);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
            }

            if (householdrepFROMREG && newuser.primaryuser)
            {
                await Addhouseholdmembers();
            }
            else
            {
                await Updatehouseholdmembers();
            }

            //upload the questionnaire 
            var serializerOptionsuser = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };


            var newbaslinequestionnaire = new newuserquestionnaire();

            newbaslinequestionnaire.userid = newuser.userid;
            newbaslinequestionnaire.questionnaireid = "b1_individual_questionnaire";

            if (householdrepFROMREG == false)
            {
                //add in additional questions for filling it out for a  different user

                var fillingform = new QuestionnaireResult();
                fillingform.QuestionId = "b_who_filled_form";
                fillingform.AnswerId = "b_who_filled_form_a2";
                fillingform.InternalName = "b_who_filled_form";

                QuestionnaireResults.Add(fillingform);

                var relationship = new QuestionnaireResult();
                relationship.QuestionId = "b_who_filled_form_2";

                //find the relationship

                var fillingForField = mainuserstacksave.subFields.FirstOrDefault(f => f.Id == "familyrelationship");
                if (fillingForField != null)
                {

                    var matchedOption = fillingForField.Options
                  .FirstOrDefault(o => o.Text.Equals(userinfoforbaseline.household_individual_relationship,
                   StringComparison.OrdinalIgnoreCase));

                    if (matchedOption != null)
                    {
                        relationship.AnswerId = matchedOption.AnswerId;
                    }
                    else
                    {
                        //set id as other
                        relationship.AnswerId = "b_who_filled_form_2_4";
                    }
                }

                QuestionnaireResults.Add(relationship);

            }

            //Update questionnaire UserFeedback
            string newquestionnairejson = System.Text.Json.JsonSerializer.Serialize(QuestionnaireResults);
            newbaslinequestionnaire.feedback = newquestionnairejson;


            Uri uri = new Uri($"{APICalls.ApplicationURL}userquestionnaire");
            string jsonuser = System.Text.Json.JsonSerializer.Serialize<newuserquestionnaire>(newbaslinequestionnaire, serializerOptionsuser);
            StringContent contenttt = new StringContent(jsonuser, Encoding.UTF8, "application/json");
            HttpResponseMessage responseeuser = null;

            responseeuser = await configuredClient.PostAsync(uri, contenttt);

            if (!responseeuser.IsSuccessStatusCode)
            {
                var content = await responseeuser.Content.ReadAsStringAsync();
            }

            //add the consent 
            string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=peoplewithappiamges;AccountKey=9maBMGnjWp6KfOnOuXWHqveV4LPKyOnlCgtkiKQOeA+d+cr/trKApvPTdQ+piyQJlicOE6dpeAWA56uD39YJhg==;EndpointSuffix=core.windows.net";

            var backrandom = new System.Random();
            var backrandomnum = backrandom.Next(1000, 10000000);
            var backimagename = newuser.userid + "-" + DateTime.Now.ToString("HHmmssfff") + "-" + backrandomnum + ".png";

            // Parse the connection string and create a blob client
            BlobServiceClient blobServiceClient = new BlobServiceClient(StorageConnectionString);

            // Get a reference to the container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("consentsignatures");

            // Get a reference to the blob
            BlobClient blobClient = containerClient.GetBlobClient(backimagename);

            //Send Signature to Azure 
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                //old 
                //Stream drawingStream = await DrawingView.GetImageStream(drawingpad.Lines, new Size(150, 150),
                //        Microsoft.Maui.Graphics.Colors.Transparent, cts.Token);

                //new 
                Stream drawingStream = await drawingpad.GetImageStream(150, 150, cts.Token);


                if (drawingStream != null)
                {
                    await blobClient.UploadAsync(drawingStream);
                }
            }
            else
            {
                Stream signatureStream = await signpad.GetStreamAsync(Syncfusion.Maui.Core.ImageFileFormat.Png);

                if (signatureStream != null)
                {
                    await blobClient.UploadAsync(signatureStream);
                }
            }



            // Upload the signature image stream to Azure Blob Storage


            //User Consent 
            var UpdateUserConsent = new userconsent();
            UpdateUserConsent.userid = newuser.userid;
            UpdateUserConsent.consentid = newuser.signupcodeid;
            UpdateUserConsent.signaturefilename = backimagename;
            if (!string.IsNullOrEmpty(TandCNonReqired))
            {
                UpdateUserConsent.consentselection = TandCNonReqired;
            }

            //only for childs extra details in consent
            if (under10stack.IsVisible)
            {
                var role = ((SignoffOption)under10rolelist.SelectedItem).label.Trim();

                UpdateUserConsent.additionaldetails = under10entry.Text + "|" + role;
                UpdateUserConsent.consentinput = Helpers.Settings.UsersID;
            }

            //APICalls database = new APICalls();
            await APICalls.Instance.PostUserConsentAsync(UpdateUserConsent);

            //Needs to be changed to mirror the same one in NewImperial if this page is used again
            await MopupService.Instance.PushAsync(new PopupPageHelper(true, true));

            if (householdrepFROMREG)
            {
                //add the user settings
                Preferences.Default.Set("userid", newuser.userid);
                Preferences.Default.Set("firstname", newuser.firstname);
                Preferences.Default.Set("surname", newuser.surname);
                Preferences.Default.Set("signupcode", newuser.signupcodeid);
                Preferences.Default.Set("email", newuser.email);
                Preferences.Default.Set("gender", newuser.gender);
                Preferences.Default.Set("ethnicity", newuser.ethnicity);
                Preferences.Default.Set("age", newuser.dateofbirth);
                Preferences.Default.Set("userpasswordhash", newuser.password);
                Preferences.Default.Set("sideupcodegrouping", newuser.signupcodegrouping);
                Preferences.Default.Set("householdgrouping", newuser.householdgroupid);
                Preferences.Default.Set("primarycardid", newuser.primarycareid);
                Preferences.Default.Set("details", newuser.details);
                Preferences.Default.Set("postcode", newuser.postcode);
                Preferences.Default.Set("isprimaryuser", newuser.primaryuser);
                Preferences.Default.Set("phonenumber", newuser.telephone);
                Preferences.Default.Set("addresslineone", addressoneentry.Text?.Trim() ?? string.Empty);
                Preferences.Default.Set("town", townentry.Text?.Trim() ?? string.Empty);
                Preferences.Default.Set("County", countyentry.Text?.Trim() ?? string.Empty);
                Preferences.Default.Set("devicemanufacturer", DeviceInfo.Manufacturer);
                Preferences.Default.Set("devicemodel", DeviceInfo.Model);
                Preferences.Default.Set("deviceversion", DeviceInfo.VersionString);

                if (newuser.primaryuser)
                {
                    Preferences.Default.Set("primaryuserid", newuser.userid);
                }

            }

            //Create weekly Notification 
            //await AddNotification.ScheduleWeeklyNotification(DateTime.Now);  

            //await App.SetMainPage(new NavigationPage(new MainDashboard()));
            await App.SetMainPage(new ImperialDashboard());
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "CreateAccount");
            await Nextloader(false);
        }
    }

    private async Task RegInitialSetup()
    {
        try
        {

            welcomestack.IsVisible = false;
            registerstack.IsVisible = true;

            nextbtn.Text = "Next";
            topprogress.IsVisible = true;

            currentFieldIndex = 0;

            ShowCurrentStack();
            updateprogress();
            await ResetScroll();

            await Task.Delay(1000);
            await Nextloader(false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "RegInitialSetup");
            await Nextloader(false);
        }
    }
    private async void nextbtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Nextloader(true);
            //Check if either Finish or Get started 
            switch (nextbtn.Text)
            {
                case "Finish":
                    await CreateAccount();
                    return;

                case "Get Started":
                    await RegInitialSetup();
                    return;
            }


            if (currentFieldIndex >= Allregfields.Count)
            {
                nextbtn.Text = "Finish";
                nextbtnloader.IsRunning = false;
                nextbtnloader.IsVisible = false;
                await Nextloader(false);
                return;
            }


            var currentField = Allregfields[currentFieldIndex];
            bool canProceed = true;

            if (currentField.Type == "Questionnaire")
            {
                topprogress2.IsVisible = true;
                // **A. Validate the current sub-question**
                //  canProceed = ValidateQuestionnaireStack(currentField); // Implement this method to check current sub-field

                //  if (canProceed)
                //{
                // **B. Advance the sub-index**
                //currentFieldIndexquestionnaire++;

                //// **C. Check if there are more sub-questions in this group**
                //if (currentFieldIndexquestionnaire < currentField.subFields.Count)
                //{
                //    // Move to the next sub-question (stay on the same parent stack)
                //    // We're done; don't execute the main field advancement logic below.
                //    ShowCurrentStack();
                //    updateprogress();
                //    return; // *** EXIT HERE TO STAY ON THE SAME MAIN STACK ***
                //}
                //else
                //{
                //    topprogress2.IsVisible = false;
                //    // All sub-questions in this group are complete! 
                //    // Fall through to the main field advancement logic (currentFieldIndex++)

                //    // Reset the sub-index for the next group
                //   // currentFieldIndexquestionnaire = 0;

                //    // Add the data from the group
                //   // AddQuestionnaireGroupInfo(currentField); // Implement this method
                //}
                // }

                // return;
            }


            if (currentField.XamlNameArea == "namestack" && currentField.Required)
            {
                canProceed = await ValidateNameStack();

                if (canProceed)
                {
                    AddParticiantInfo();
                }
            }
            else if (currentField.XamlNameArea == "mainuserstack" && currentField.Required)
            {
                canProceed = ValidateFormStack();

                if (canProceed)
                {
                    //Not Needed Here
                    //Addhouseholdmembers();
                }
            }
            else if (currentField.XamlNameArea == "addressstack" && currentField.Required)
            {
                canProceed = await ValidateaddressStack();

                if (canProceed)
                {
                    AddAddressInfo();
                }
            }
            else if (currentField.XamlNameArea == "genderstack" && currentField.Required)
            {
                canProceed = ValidateGenderStack();

                if (canProceed)
                {
                    AddGenderInfo();
                }
            }
            else if (currentField.XamlNameArea == "ethnicitystack" && currentField.Required)
            {
                canProceed = ValidateEthnicityStack();

                if (canProceed)
                {
                    AddEthnicityInfo();
                }
            }
            else if (currentField.XamlNameArea == "bodymetricsstack" && currentField.Required)
            {
                canProceed = ValidatebodymetricsStack();

                if (canProceed)
                {
                    AddBodyMetricsInfo();
                }
            }
            else if (currentField.XamlNameArea == "educationworkstack" && currentField.Required)
            {
                canProceed = ValidateeducationStack();

                if (canProceed)
                {
                    AddEducationWorkInfo();
                }
            }
            else if (currentField.XamlNameArea == "householdstructurestack" && currentField.Required)
            {
                canProceed = ValidateHouseholdstructureStack();

                if (canProceed)
                {
                    AddHouseHoldStructureInfo();
                }
            }
            else if (currentField.XamlNameArea == "nhsnumstack" && currentField.Required)
            {
                canProceed = ValidatenhsnumStack();

                if (canProceed)
                {
                    AddNHSInfo();
                }
            }
            else if (currentField.XamlNameArea == "ristack" && currentField.Required)
            {
                canProceed = ValidateRIStack();

                if (canProceed)
                {
                    AddRIInfo();
                }
            }
            else if (currentField.XamlNameArea == "hcstack" && currentField.Required)
            {
                canProceed = ValidateHealthConditionsStack();

                if (canProceed)
                {
                    AddHealthConditionsInfo();
                }
            }
            else if (currentField.XamlNameArea == "addhcstack")
            {
                canProceed = ValidateAddHealthConditionsStack();

                if (canProceed)
                {
                    AddHealthConditionsADD();
                }
            }
            else if (currentField.XamlNameArea == "medynstack" && currentField.Required)
            {
                canProceed = ValidateMedicationsStack();

                if (canProceed)
                {
                    AddMedicationsInfo();
                }
            }
            else if (currentField.XamlNameArea == "medicationsstack")
            {
                canProceed = ValidateMedicationsADDStack();

                if (canProceed)
                {
                    AddMedicationsADD();
                }
            }
            else if (currentField.XamlNameArea == "rvstack" && currentField.Required)
            {
                canProceed = ValidateRVStack();

                if (canProceed)
                {
                    AddRVInfo();
                }
            }
            else if (currentField.XamlNameArea == "dietstack" && currentField.Required)
            {

                canProceed = ValidateDietStack();

                if (canProceed)
                {
                    AddDietInfo();
                }
            }
            else if (currentField.XamlNameArea == "menstrualstack")
            {
                canProceed = ValidateMenstrualInfo();

                if (canProceed)
                {
                    AddMenstrualInfo();
                }
            }
            else if (currentField.XamlNameArea == "htstack" && currentField.Required)
            {
                canProceed = ValidateHtInfo();

                if (canProceed)
                {
                    AddHtInfo();
                }
            }
            else if (currentField.XamlNameArea == "additionalqstack" && currentField.Required)
            {
                canProceed = validateAQ();

                if (canProceed)
                {
                    AddAddqInfo();
                }
            }
            else if (currentField.XamlNameArea == "antiviralstack")
            {
                canProceed = ValidateAntiViralInfo();

                if (canProceed)
                {
                    AddAntiViralInfo();
                }
            }

            else if (currentField.XamlNameArea == "tobaccostack")
            {
                canProceed = ValidateTobaccoInfo();

                if (canProceed)
                {
                    AddTobaccoInfo();
                }
            }

            else if (currentField.XamlNameArea == "alcoholstack")
            {
                canProceed = ValidateAlcoholInfo();

                if (canProceed)
                {
                    AddAlcoholInfo();
                }
            }

            else if (currentField.XamlNameArea == "drugstack")
            {
                canProceed = ValidateDrugInfo();

                if (canProceed)
                {
                    AddDrugInfo();
                }
            }

            else if (currentField.XamlNameArea == "sleepstack")
            {
                canProceed = ValidateSleepInfo();

                if (canProceed)
                {
                    AddSleepInfo();
                }
            }
            else if (currentField.XamlNameArea == "termsstack" && currentField.Required)
            {
                canProceed = CheckTermsandConditions();

                if (canProceed)
                {
                    AddTandCsInfo();
                }
            }




            if (!canProceed)
            {
                Vibration.Vibrate();
                await Nextloader(false);
                return;
            }


            if (topprogress2.IsVisible)
            {
                if (currentFieldIndexquestionnaire < Allquesfields.Count)
                {
                    SetStackVisibility(Allquesfields[currentFieldIndexquestionnaire].XamlNameArea, false);
                }

                currentFieldIndexquestionnaire++;

                if (currentFieldIndexquestionnaire < Allquesfields.Count)
                {
                    ShowCurrentStack();
                    updateprogress();
                }
                else if (currentFieldIndexquestionnaire == Allquesfields.Count)
                {
                    //hide progress bar and reset 

                    topprogress2.IsVisible = false;
                    currentFieldIndexquestionnaire = Allquesfields.Count - 1;

                    currentFieldIndex++;

                    ShowCurrentStack();
                    updateprogress();
                }
            }
            else
            {
                // hide previous stack
                if (currentFieldIndex < Allregfields.Count)
                {
                    SetStackVisibility(Allregfields[currentFieldIndex].XamlNameArea, false);
                }

                // move to next field
                currentFieldIndex++;

                if (currentFieldIndex < Allregfields.Count)
                {
                    ShowCurrentStack();
                    updateprogress();
                }
                else
                {
                    nextbtn.Text = "Finish";
                }
            }


            if (mainscrollview.ScrollY > 0)
            {
                await mainscrollview.ScrollToAsync(0, 0, true);
            }

            await Nextloader(false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "nextbtn_Clicked");
            await Nextloader(false);
        }
    }


    private void HandleQuestionnaireCompletion()
    {
        try
        {
            // The previous logic incorrectly tried to move to the next registration field here
            // based on the assumption that the questionnaire was just one step in the registration array.

            // If the questionnaire was the LAST part of registration:
            // nextbtn.Text = "Finish";

            // If the questionnaire was a SINGLE FIELD at currentFieldIndex:
            if (currentFieldIndex < Allregfields.Count)
            {
                // Hide the overall "questionnairestack" itself if it was the current registration field
                var completedRegField = Allregfields[currentFieldIndex];
                SetStackVisibility(completedRegField.XamlNameArea, false);

                // Move to the next registration field
                currentFieldIndex++;
            }

            if (currentFieldIndex < Allregfields.Count)
            {
                ShowCurrentStack(); // Show the next main registration stack
                updateprogress();
            }
            else
            {
                // Truly finished everything!
                // DisplayAlert("Done", "You�ve completed everything!", "OK");
                nextbtn.Text = "Finish";
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "HandleQuestionnaireCompletion");
        }
    }


    async void AddParticiantInfo()
    {
        try
        {



            newuser.firstname = firstnameentry.Text.Trim();
            newuser.surname = surnameentry.Text.Trim();
            newuser.email = emailentry.Text.Trim();
            newuser.telephone = telentry.Text.Trim();

            string hashedPassword = await HashPasswordAsync(confirmpassentry.Text);
            newuser.password = hashedPassword;


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "AddParticiantInfo");
        }
    }

    private async Task<string> HashPasswordAsync(string password)
    {
#pragma warning disable CS8603 // Possible null reference return.
        return await Task.Run(() =>
        {
            try
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert the byte array to a hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception Ex)
            {
                // Handle the exception appropriately

                return null;
            }
        });
#pragma warning restore CS8603 // Possible null reference return.
    }

    void AddDetail(string text, string value)
    {
        UserDetails.Add(new OptionDetails
        {
            Text = text,
            Value = value?.Trim()
        });
    }

    async void AddAddressInfo()
    {
        try
        {


            newuser.postcode = postcodeentry.Text.Trim();
            UserDetails.Clear();
            AddDetail("addresslineone", addressoneentry.Text.Trim());
            AddDetail("town", townentry.Text.Trim());
            AddDetail("County", countyentry.Text.Trim());
            AddDetail("devicemanufacturer", DeviceInfo.Manufacturer);
            AddDetail("devicemodel", DeviceInfo.Model);
            AddDetail("deviceversion", DeviceInfo.VersionString);

            //string newquestionnairejson = System.Text.Json.JsonSerializer.Serialize(UserDetails);
            //var oldList = JsonConvert.DeserializeObject<List<OptionDetails>>(newquestionnairejson);
            //var result = oldList.ToDictionary(x => x.Text, x => x.Value);
            //var newJson = JsonConvert.SerializeObject(result);

            ////newuser.details = newquestionnairejson;
            //newuser.details = newJson;

            var newDetails = UserDetails.ToDictionary(x => x.Text, x => x.Value);
            var newjson = JsonConvert.SerializeObject(new List<object> { newDetails });
            newuser.details = newjson;

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "AddAddressInfo");
        }
    }

    async Task Addhouseholdmembers()
    {
        try
        {

            //Create Two New HouseHold memebers 
            var newmembers = new ObservableCollection<householdgroupjsondetails>
            {
                new householdgroupjsondetails
                {
                    household_group_id = userdetails.Householdgroupid,
                    household_individual_name = $"{firstfamentry.Text.Trim()} {firstsurnameentry.Text.Trim()}",
                    household_individual_email = emailsectiongrid.IsVisible ? firstemailentry.Text?.Trim() : null,
                    household_individual_status = "Onboarding",
                    household_individual_relationship = ((OptionDetails)familymember1list.SelectedItem).Text.Trim(),
                    household_individual_age = familymember1.SelectedItem.ToString().Trim()
                },
                new householdgroupjsondetails
                {
                    household_group_id = userdetails.Householdgroupid,
                    household_individual_name = $"{firstfamentry2.Text.Trim()} {firstsurnameentry2.Text.Trim()}",
                    household_individual_email = emailsectiongrid2.IsVisible ? firstemailentry2.Text?.Trim() : null,
                    household_individual_status = "Onboarding",
                    household_individual_relationship = ((OptionDetails)familymember1list2.SelectedItem).Text.Trim(),
                    household_individual_age = familymember12.SelectedItem.ToString().Trim()
                }
            };


            // 1. Serialize with explicit "None" formatting to prevent line breaks
            string json = JsonConvert.SerializeObject(newmembers, Newtonsoft.Json.Formatting.None);

            // 2. Force-strip ANY whitespace or line breaks from the JSON string itself
            json = json.Replace("\r", "").Replace("\n", "").Trim();

            // --- DEBUG CHECK: The brackets should be tight: [{"... "}] ---
            Debug.WriteLine($"CHECK JSON: [{json}]");

            // 3. Convert to Base64
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            string base64EncodedMembers = Convert.ToBase64String(jsonBytes);

            // 4. URL Encode (and trim again just in case)
            string safeBase64 = System.Net.WebUtility.UrlEncode(base64EncodedMembers).Trim();

            // 5. Build the final URL and strip any potential breaks again
            string baseUrl = "https://hopper.peoplewith.com/household-individual-alignment.php";
            AddHouseholdURl = $"{baseUrl}?hij={base64EncodedMembers}".Replace("\r", "").Replace("\n", "").Trim();
            //string finalUrl = $"{baseUrl}?hij={base64EncodedMembers}".Replace("\r", "").Replace("\n", "").Trim();

            // 6. Send
            var field = Allregfields[currentFieldIndex];
            if (field.XamlNameArea != "mainuserstack")
            {
                var httpClient = APICalls.Instance.GetClient();
                HttpResponseMessage response = await httpClient.GetAsync(AddHouseholdURl);
                if (!response.IsSuccessStatusCode)
                {
                    var s = response.StatusCode + response.ReasonPhrase;
                }
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Addhouseholdmembers");
        }
    }

    async Task Updatehouseholdmembers()
    {
        try
        {

            //        // 1. Build the study record
            //        var studyRecord = new householdstudyrecord
            //        {
            //            household_id = allhousehouldgroup.householdgroupid,
            //            current_phase = "awaitingbaseline",
            //            t_events = new List<TEvent>
            //{
            //    new TEvent
            //    {
            //        t_event_id = "TE-001",
            //        t_event_status = "active",
            //        scenario = null,
            //        t1_start_date = null,
            //        members = allgroupdetailspassed.Select(m => new TEventMember
            //        {
            //            user_id = m.household_individual_userid,
            //            daily_forms_complete = false,
            //            daily_forms_stopped_at = null,
            //            questionnaires = new List<TQuestionnaire>()
            //        }).ToList()
            //    }
            //}
            //        };

            //var studyRecord = new householdstudyrecord
            //{
            //    household_id = allhousehouldgroup.householdgroupid,
            //    current_phase = "awaitingbaseline",
            //    t_events = new List<TEvent>()
            //};

            //// 2. Serialize study record and assign to details
            //string studyRecordJson = System.Text.Json.JsonSerializer.Serialize(studyRecord);
            //allhousehouldgroup.details = studyRecordJson;

            if (allgroupdetailspassed == null || !allgroupdetailspassed.Any())
            {
                var groupedData = await APICalls.Instance.GetUserHouseholdInfo(newuser?.householdgroupid);
                if (groupedData != null)
                {
                    allhousehouldgroup = groupedData.FirstOrDefault();
                    allgroupdetailspassed = allhousehouldgroup?.userdetailslist;
                }
            }

            if (userinfoforbaseline == null && allgroupdetailspassed != null)
            {
                userinfoforbaseline = allgroupdetailspassed.FirstOrDefault(x => x.household_individual_userid == newuser?.userid);
            }

            var UpdateUser = allgroupdetailspassed?
                .FirstOrDefault(x => x?.household_individual_userid == userinfoforbaseline?.household_individual_userid);

            if (UpdateUser != null)
            {
                UpdateUser.household_individual_status = "active";

                var initalname = $"{firstfamentry.Text?.Trim() ?? string.Empty} {firstsurnameentry.Text?.Trim() ?? string.Empty}".Trim();
                if (!string.IsNullOrEmpty(initalname))
                {
                    UpdateUser.household_individual_name = initalname;
                }
                else
                {
                    UpdateUser.household_individual_name =
                     $"{firstnameentry.Text?.Trim() ?? string.Empty} {surnameentry.Text?.Trim() ?? string.Empty}".Trim();
                }

            }

            var houseRepToRemove = allgroupdetailspassed?
                .FirstOrDefault(x => x?.household_individual_relationship == "Household Rep");

            if (houseRepToRemove != null)
            {
                allgroupdetailspassed.Remove(houseRepToRemove);
            }

            if (allhousehouldgroup != null)
            {
                allhousehouldgroup.groupuserdetails = Newtonsoft.Json.JsonConvert.SerializeObject(allgroupdetailspassed);

                var updateData = new { groupuserdetails = allhousehouldgroup.groupuserdetails };
                string updateJson = System.Text.Json.JsonSerializer.Serialize(updateData);

                if (userinfoforbaseline != null)
                {
                    var url = $"{APICalls.ApplicationURL}householdgroup/householdgroupid/{userinfoforbaseline.household_group_id}";
                    using var content = new StringContent(updateJson, Encoding.UTF8, "application/json");

                    var response = await APICalls.Instance.GetClient().PatchAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("Household group updated successfully");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Update failed: {response.StatusCode} | Body: {errorContent}");
                    }
                }
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Updatehouseholdmembers");
        }
    }
    async void AddGenderInfo()
    {
        try
        {


            newuser.dateofbirth = dateEntry.Text.Trim();
            var gender = genderlist.SelectedItem as OptionDetails;

            if (gender != null)
            {
                newuser.gender = gender.Text;
            }

            if (sexmatchlbl.IsVisible)
            {
                var selectedOption = gendermatchlist.SelectedItem as OptionDetails;

                if (selectedOption != null)
                {
                    // Find the specific answer record by its friendly name
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "genderMatch");

                    if (record != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record.AnswerId = selectedOption.AnswerId;
                    }
                }
            }

            if (genidlbl.IsVisible)
            {
                var selectedOption = genidlist.SelectedItem as OptionDetails;

                if (selectedOption != null)
                {
                    // Find the specific answer record by its friendly name
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "genderidentity");

                    if (record != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record.AnswerId = selectedOption.AnswerId;
                    }
                }
            }



            //check if we need to show the menstural cycle part
            var date = DateTime.Parse(dateEntry.Text);

            int age = DateTime.Now.Year - date.Year;
            if (DateTime.Now.Date < date.AddYears(age)) // birthday not yet reached this year
                age--;

            if (age >= 15 && age <= 55 && newuser.gender == "Female")
            {
                var getmenstrual = AllregfieldsNotRequired.Where(x => x.XamlNameArea == "menstrualstack").FirstOrDefault();



                if (getmenstrual != null && !Allregfields.Any(x => x.XamlNameArea == "menstrualstack"))
                {
                    // Ensure we don't go out of bounds
                    int targetIndex = Convert.ToInt32(getmenstrual.Order);
                    int safeIndex = Math.Min(targetIndex, Allregfields.Count);

                    Allregfields.Insert(safeIndex, getmenstrual);
                }
            }
            else
            {
                //remove it 
                var getmenstrual = Allregfields.Where(x => x.XamlNameArea == "menstrualstack").FirstOrDefault();
                if (getmenstrual != null)
                {
                    // menstrualquestion = getmenstrual;
                    Allregfields.Remove(getmenstrual);
                }


            }


            //check if the user is under 16

            if (age < 16)
            {

                //  var getover16items = Allregfields.Where(x => x.Type == "over16").ToList();

                //  Over16regfields = getover16items;


                //foreach(var item in Over16regfields)
                //  {
                //      Allregfields.Remove(item);
                //  }

                var getover16itemsmain = Allregfields.Where(x => x.Type == "over16main").ToList();

                Over16regfieldsmain = getover16itemsmain;


                foreach (var item in getover16itemsmain)
                {
                    Allregfields.Remove(item);
                }



            }
            else
            {
                //if(Over16regfields.Count != 0)
                //{
                //    foreach(var item in Over16regfields)
                //    {
                //        if (!Allregfields.Contains(item))
                //        {
                //            Allregfields.Add(item);
                //        }
                //    }
                //}

                if (Over16regfieldsmain.Count != 0)
                {
                    foreach (var item in Over16regfieldsmain)
                    {
                        if (!Allregfields.Contains(item))
                        {
                            Allregfields.Add(item);
                        }
                    }
                }


            }

            //    // Store old values before changing the question list
            //    int oldSegmentCount = topprogress.SegmentCount;
            //double currentQuestionIndex = topprogress.Progress; // or whatever variable tracks current progress

            //// Remove question
            ////var getmenstrual = Allregfields.FirstOrDefault(x => x.Id == "menstrualstack");
            ////if (getmenstrual != null)
            ////{
            ////    menstrualquestion = getmenstrual;
            ////    Allregfields.Remove(getmenstrual);
            ////}


            //var onlyrequriedfields = Allregfields.Where(x => x.Required = true ).ToList();


            //// Update new segment count
            //topprogress.SegmentCount = onlyrequriedfields.Count;
            //progressamount = (double)100 / onlyrequriedfields.Count;

            //// Work out how far through the questionnaire the user currently is
            ////double percentComplete = 0;

            ////if (oldSegmentCount > 0)
            ////{
            ////    percentComplete = currentQuestionIndex / (double)oldSegmentCount;
            ////}

            ////// Convert that percentage into the new segment position
            ////currentQuestionIndex = (int)Math.Round(percentComplete * topprogress.SegmentCount);

            ////// Prevent going outside bounds
            ////currentQuestionIndex = Math.Max(1, Math.Min(currentQuestionIndex, topprogress.SegmentCount));

            ////// Update progress control
            ////topprogress.Progress = currentQuestionIndex;

            //topprogress.Progress += progressamount;



        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "AddGenderInfo");
        }
    }

    async void AddEthnicityInfo()
    {
        try
        {
            var ethnicity = ethnicitylist.SelectedItem as OptionDetails;

            if (ethnicity != null)
            {
                newuser.ethnicity = ethnicity.Text;
            }


            if (!string.IsNullOrEmpty(ownwordsethentry.Text))
            {

                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "ethnicityInOwnWords");

                if (record != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record.AnswerValue = ownwordsethentry.Text;
                }
            }


            var selectedOption = uklist.SelectedItem as OptionDetails;

            if (selectedOption != null)
            {
                // Find the specific answer record by its friendly name
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "bornInUK");

                if (record != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record.AnswerId = selectedOption.AnswerId;
                }
            }

            if (movelbl.IsVisible)
            {
                var selectedOption2 = movelist.SelectedItem as OptionDetails;

                if (selectedOption2 != null)
                {
                    // Find the specific answer record by its friendly name
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "moveToUKAge");

                    if (record != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record.AnswerId = selectedOption2.AnswerId;
                    }
                }
            }

            if (countrylbl.IsVisible)
            {
                var selectedOption2 = autocompletecounty.SelectedItem as OptionDetails;

                if (selectedOption2 != null)
                {
                    // Find the specific answer record by its friendly name
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "countryOfOrigin");

                    if (record != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record.AnswerId = selectedOption2.AnswerId;
                    }
                }
            }

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "AddEthnicityInfo");
        }
    }

    async void AddBodyMetricsInfo()
    {
        try
        {


            var selectedOption = weightinputlist.SelectedItem as OptionDetails;

            if (selectedOption != null)
            {
                // Find the specific answer record by its friendly name
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "weightUnit");

                if (record != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record.AnswerId = selectedOption.AnswerId;


                    if (weighthelper.IsVisible)
                    {
                        record.AnswerValue = weightEntry.Text.Trim();
                    }
                }
            }


            //TODO: Add record.Quesitonid = to redcap id for weight and height 
            var selectedOption2 = heightinputlist.SelectedItem as OptionDetails;

            if (selectedOption2 != null)
            {
                // Find the specific answer record by its friendly name
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "heightUnit");

                if (record != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record.AnswerId = selectedOption2.AnswerId;

                    if (heightHelper.IsVisible)
                    {
                        record.AnswerValue = feetEntry.Text.Trim() + inchesEntry.Text.Trim();
                    }
                    else if (heightcmhelper.IsVisible)
                    {
                        record.AnswerValue = heightcmentry.Text.Trim();
                    }
                }
            }


            if (stepslbl.IsVisible)
            {
                var selectedOption3 = stepslist.SelectedItem as OptionDetails;

                if (selectedOption3 != null)
                {
                    // Find the specific answer record by its friendly name
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "averageSteps");

                    if (record != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record.AnswerId = selectedOption3.AnswerId;
                    }
                }
            }

            if (gymlbl.IsVisible)
            {
                var selectedOption3 = gymlist.SelectedItem as OptionDetails;

                if (selectedOption3 != null)
                {
                    // Find the specific answer record by its friendly name
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "muscleMass");

                    if (record != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record.AnswerId = selectedOption3.AnswerId;
                    }
                }
            }

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "AddBodyMetricsInfo");
        }
    }

    async void AddEducationWorkInfo()
    {
        try
        {



            var selectedOption = higheducationlist.SelectedItem as OptionDetails;

            if (selectedOption != null)
            {
                // Find the specific answer record by its friendly name
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "highestEducation");

                if (record != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record.AnswerId = selectedOption.AnswerId;

                }
            }


            var selectedOption2 = currentsituationlist.SelectedItem as OptionDetails;

            if (selectedOption2 != null)
            {
                // Find the specific answer record by its friendly name
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "currentSituation");

                if (record != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record.AnswerId = selectedOption2.AnswerId;

                }
            }


            if (typeworklbl.IsVisible)
            {
                var selectedOption3 = typeworklist.SelectedItem as OptionDetails;

                if (selectedOption3 != null)
                {
                    // Find the specific answer record by its friendly name
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "workType");

                    if (record != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record.AnswerId = selectedOption3.AnswerId;
                    }
                }
            }



        }
        catch (Exception Ex)
        {

        }
    }

    async void AddHouseHoldStructureInfo()
    {
        try
        {




            // Find the specific answer record by its friendly name
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "peopleinhome");

            if (record != null)
            {
                // Update the record with the chosen AnswerId GUID
                record.AnswerValue = peopleentry.Text.Trim();

            }

            var record2 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "rooms");

            if (record2 != null)
            {
                // Update the record with the chosen AnswerId GUID
                record2.AnswerValue = roomentry.Text.Trim();

            }

            var record3 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "bathrooms");

            if (record3 != null)
            {
                // Update the record with the chosen AnswerId GUID
                record3.AnswerValue = sharedbathroomsentry.Text.Trim();

            }



            var selectedOption2 = ventlist.SelectedItem as OptionDetails;

            if (selectedOption2 != null)
            {
                // Find the specific answer record by its friendly name
                var record4 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "ventiliation");

                if (record4 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record4.AnswerId = selectedOption2.AnswerId;

                }
            }



            var selectedOption3 = damplist.SelectedItem as OptionDetails;

            if (selectedOption3 != null)
            {
                // Find the specific answer record by its friendly name
                var record5 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "mould");

                if (record5 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record5.AnswerId = selectedOption3.AnswerId;
                }
            }




        }
        catch (Exception Ex)
        {

        }
    }

    async void AddNHSInfo()
    {
        try
        {



            // Find the specific answer record by its friendly name
            var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "nhsNumber");

            if (record != null)
            {
                // Update the record with the chosen AnswerId GUID
                record.AnswerValue = nhsentry.Text.Trim();

            }

            var selectedOption = gplist.SelectedItem as OptionDetails;

            if (selectedOption != null)
            {
                // Find the specific answer record by its friendly name
                var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "gpRegistered");

                if (record1 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record1.AnswerId = selectedOption.AnswerId;

                }
            }


            if (gpinfolbl.IsVisible)
            {
                var selectedOption2 = gpautocomplete.SelectedItem as OptionDetails;

                if (selectedOption2 != null)
                {
                    // Find the specific answer record by its friendly name
                    var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "gpPracticeName");

                    if (record1 != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record1.AnswerId = selectedOption2.AnswerId;
                        newuser.primarycareid = selectedOption2.AnswerId;

                    }
                }


            }





        }
        catch (Exception Ex)
        {

        }
    }

    async void AddRIInfo()
    {
        try
        {

            var selectedOption = coughlist.SelectedItem as OptionDetails;

            if (selectedOption != null)
            {
                // Find the specific answer record by its friendly name
                var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "coughfield");

                if (record1 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record1.AnswerId = selectedOption.AnswerId;

                }
            }

            var selectedOption2 = hoslist.SelectedItem as OptionDetails;

            if (selectedOption2 != null)
            {
                // Find the specific answer record by its friendly name
                var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "hosfield");

                if (record1 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record1.AnswerId = selectedOption2.AnswerId;

                }
            }

            if (infectionlbl.IsVisible)
            {
                // Find the specific answer record by its friendly name
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "infectionfield");

                if (record != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record.AnswerValue = infectionyearentry.Text.Trim();

                }

                var selectedOption3 = venlist.SelectedItem as OptionDetails;

                if (selectedOption3 != null)
                {
                    // Find the specific answer record by its friendly name
                    var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "venfield");

                    if (record1 != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record1.AnswerId = selectedOption3.AnswerId;

                    }
                }
            }



        }
        catch (Exception Ex)
        {

        }
    }

    async void AddHealthConditionsInfo()
    {
        try
        {
            if (hcfirstlist.SelectedItem.ToString() == "Yes")
            {

                // var additionalconditions = Allregfields.Where(x => x.Type == "otherhc").ToList();

                var additionalconditions = AllregfieldsNotRequired.Where(x => x.Type == "otherhc").FirstOrDefault();



                if (additionalconditions != null && !Allregfields.Any(x => x.XamlNameArea == "otherhc"))
                {
                    // We want it right after the current field
                    int targetIndex = currentFieldIndex + 1;

                    // Safety check: Don't exceed the list bounds
                    int safeIndex = Math.Clamp(targetIndex, 0, Allregfields.Count);

                    Allregfields.Insert(safeIndex, additionalconditions);
                }

                //Over16regfieldsmain = getover16itemsmain;
                foreach (var item in Allregfields)
                {

                }

                //foreach (var item in getover16itemsmain)
                //{
                //    Allregfields.Remove(item);
                //}

            }
            else
            {
                //remove it 
                var otherhcquestion = Allregfields.Where(x => x.Type == "otherhc").FirstOrDefault();
                if (otherhcquestion != null)
                {
                    // menstrualquestion = getmenstrual;
                    Allregfields.Remove(otherhcquestion);
                }

            }



            var selectedOption = hcfirstlist.SelectedItem as string;

            if (selectedOption != null)
            {
                // Find the specific answer record by its friendly name
                var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "diastack");

                if (record1 != null)
                {
                    if (selectedOption == "Yes")
                    {
                        var answerid = record1.QuestionId + "_1";
                        record1.AnswerId = answerid;
                    }
                    else
                    {
                        var answerid = record1.QuestionId + "_2";
                        record1.AnswerId = answerid;
                    }

                    // Update the record with the chosen AnswerId GUID
                    //  record1.AnswerId = selectedOption.AnswerId;

                }
            }


            //// Find the specific answer record by its friendly name
            //var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "diastack");

            //if (record1 != null)
            //{

            //record1.AnswerId = string.Join(", ", SelectedConditons.Select(x => x.AnswerId));

            //}



        }
        catch (Exception Ex)
        {

        }
    }

    async void AddHealthConditionsADD()
    {
        try
        {


            // Find the specific answer record by its friendly name
            var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "diaaddstack");

            if (record1 != null)
            {
                // Update the record with the chosen AnswerId GUID
                record1.AnswerId = string.Join(", ", SelectedConditons.Select(x => x.AnswerId));

            }


            var selectedOption = otherhclist.SelectedItem as OptionDetails;

            if (selectedOption != null)
            {
                // Find the specific answer record by its friendly name
                var record2 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "otherhc");

                if (record2 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record2.AnswerId = selectedOption.AnswerId;

                }
            }

            if (typeotherhclbl.IsVisible)
            {

                var selectedOption2 = typeotherhclist.SelectedItem as OptionDetails;

                if (selectedOption2 != null)
                {
                    // Find the specific answer record by its friendly name
                    var record2 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "affectedsystem");

                    if (record2 != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record2.AnswerId = selectedOption2.AnswerId;

                    }
                }


                var record3 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "otherhealthconditions");

                if (record3 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record3.AnswerValue = otherhcentrytext.Text.Trim();

                }


            }


            if (cancerlbl.IsVisible)
            {
                var selectedOption2 = cancerlist.SelectedItem as OptionDetails;

                if (selectedOption2 != null)
                {
                    // Find the specific answer record by its friendly name
                    var record2 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "pastcancerdiagnosis");

                    if (record2 != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record2.AnswerId = selectedOption2.AnswerId;

                    }
                }


                var selectedOption3 = cancernowlist.SelectedItem as OptionDetails;

                if (selectedOption3 != null)
                {
                    // Find the specific answer record by its friendly name
                    var record2 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "cancerremissionstatus");

                    if (record2 != null)
                    {
                        // Update the record with the chosen AnswerId GUID
                        record2.AnswerId = selectedOption3.AnswerId;

                    }
                }


            }


        }
        catch (Exception Ex)
        {

        }
    }

    async void AddMedicationsInfo()
    {
        try
        {


            // Find the specific answer record by its friendly name
            //var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "medqstack");

            //if (record1 != null)
            //{

            //    record1.AnswerId = string.Join(", ", SelectedConditons.Select(x => x.AnswerId));

            //}


            if (medsfirstlist.SelectedItem.ToString() == "Yes")
            {

                // var additionalconditions = Allregfields.Where(x => x.Type == "otherhc").ToList();

                var additionalconditions = AllregfieldsNotRequired.Where(x => x.XamlNameArea == "medicationsstack").FirstOrDefault();



                if (additionalconditions != null && !Allregfields.Any(x => x.XamlNameArea == "medicationsstack"))
                {
                    // We want it right after the current field
                    int targetIndex = currentFieldIndex + 1;

                    // Safety check: Don't exceed the list bounds
                    int safeIndex = Math.Clamp(targetIndex, 0, Allregfields.Count);

                    Allregfields.Insert(safeIndex, additionalconditions);
                }

                //Over16regfieldsmain = getover16itemsmain;
                foreach (var item in Allregfields)
                {

                }

                //foreach (var item in getover16itemsmain)
                //{
                //    Allregfields.Remove(item);
                //}

            }
            else
            {
                //remove it 
                var otherhcquestion = Allregfields.Where(x => x.XamlNameArea == "medicationsstack").FirstOrDefault();
                if (otherhcquestion != null)
                {
                    // menstrualquestion = getmenstrual;
                    Allregfields.Remove(otherhcquestion);
                }

            }


            var selectedOption3 = medsfirstlist.SelectedItem as string;

            if (selectedOption3 != null)
            {
                // Find the specific answer record by its friendly name
                var record2 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "firstmedstack");

                if (record2 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    // record2.AnswerId = selectedOption3.AnswerId;
                    if (selectedOption3 == "Yes")
                    {
                        var answerid = record2.QuestionId + "_1";
                        record2.AnswerId = answerid;
                    }
                    else
                    {
                        var answerid = record2.QuestionId + "_2";
                        record2.AnswerId = answerid;
                    }

                }
            }




        }
        catch (Exception Ex)
        {

        }
    }


    async void AddMedicationsADD()
    {
        try
        {


            // Find the specific answer record by its friendly name
            var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "medqstack");

            if (record1 != null)
            {
                // Update the record with the chosen AnswerId GUID
                record1.AnswerId = string.Join(", ", SelectedMedications.Select(x => x.AnswerId));

            }


            var selectedOption = othermedlist.SelectedItem as OptionDetails;

            if (selectedOption != null)
            {
                // Find the specific answer record by its friendly name
                var record2 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "othermeds");

                if (record2 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record2.AnswerId = selectedOption.AnswerId;

                }
            }

            if (othermeddetailslbl.IsVisible)
            {



                var record3 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "othermedications");

                if (record3 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record3.AnswerValue = othermedtextentry.Text.Trim();

                }


            }





        }
        catch (Exception Ex)
        {

        }
    }
    async void AddRVInfo()
    {
        try
        {


            var selectedOption = flulist.SelectedItem as OptionDetails;

            if (selectedOption != null)
            {
                // Find the specific answer record by its friendly name
                var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "flufield");

                if (record1 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record1.AnswerId = selectedOption.AnswerId;

                }
            }

            if (fluhelper.IsVisible)
            {

                var record2 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "fludatefield");

                if (record2 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record2.AnswerValue = fluentry.Text.Trim();

                }


                //var selectedOption2 = flunoselist.SelectedItem as OptionDetails;

                //if (selectedOption2 != null)
                //{
                //    // Find the specific answer record by its friendly name
                //    var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "flunosefield");

                //    if (record1 != null)
                //    {
                //        // Update the record with the chosen AnswerId GUID
                //        record1.AnswerId = selectedOption2.AnswerId;

                //    }


                //}




            }



            //var selectedOption3 = covidlist.SelectedItem as OptionDetails;

            //if (selectedOption3 != null)
            //{
            //    // Find the specific answer record by its friendly name
            //    var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "covidfield");

            //    if (record1 != null)
            //    {
            //        // Update the record with the chosen AnswerId GUID
            //        record1.AnswerId = selectedOption3.AnswerId;

            //    }
            //}

            if (coviddatelbl.IsVisible)
            {
                var record2 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "coviddatefield");

                if (record2 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record2.AnswerValue = coviddateentry.Text.Trim();

                }
            }

            //var selectedOption4 = rsvlist.SelectedItem as OptionDetails;

            //if (selectedOption4 != null)
            //{
            //    // Find the specific answer record by its friendly name
            //    var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "rsvfield");

            //    if (record1 != null)
            //    {
            //        // Update the record with the chosen AnswerId GUID
            //        record1.AnswerId = selectedOption4.AnswerId;

            //    }
            //}


            if (rsvdatelbl.IsVisible)
            {
                var record2 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "rsvdatefield");

                if (record2 != null)
                {
                    // Update the record with the chosen AnswerId GUID
                    record2.AnswerValue = rsvdateentry.Text.Trim();

                }
            }


            //var selectedOption5 = othevaclist.SelectedItem as OptionDetails;

            //if (selectedOption5 != null)
            //{
            //    // Find the specific answer record by its friendly name
            //    var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "othervacfield");

            //    if (record1 != null)
            //    {
            //        // Update the record with the chosen AnswerId GUID
            //        record1.AnswerId = selectedOption5.AnswerId;

            //    }
            //}

            //if(othervaclistlbl.IsVisible)
            //{
            //    var selectedOptions = othevaclistlist.SelectedItems?.Cast<OptionDetails>().ToList();

            //    if (selectedOptions != null && selectedOptions.Any())
            //    {
            //        // 2. Find the specific answer record
            //        var record1 = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "otherlistfield");

            //        if (record1 != null)
            //        {
            //            // 3. Join all Selected IDs into a single string separated by commas
            //            record1.AnswerId = string.Join(", ", selectedOptions.Select(x => x.AnswerId));
            //        }
            //    }
            //}


        }
        catch (Exception Ex)
        {

        }
    }

    async void AddDietInfo()
    {
        try
        {
            // 1. dietfield
            var selectedDiet = dietlist.SelectedItem as OptionDetails;
            if (selectedDiet != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "dietfield");
                if (record != null)
                {
                    record.AnswerId = selectedDiet.AnswerId;
                }
            }

            // 2. dietlengthfield (Only if visible)
            if (dietlengthlist.IsVisible)
            {
                var selectedLength = dietlengthlist.SelectedItem as OptionDetails;
                if (selectedLength != null)
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "dietlengthfield");
                    if (record != null)
                    {
                        record.AnswerId = selectedLength.AnswerId;
                    }
                }
            }

            // 3. supplementsfield (The "Any Supplements" question)
            var selectedAny = anylist.SelectedItem as OptionDetails;
            if (selectedAny != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "supplementsfield");
                if (record != null)
                {
                    record.AnswerId = selectedAny.AnswerId;
                }
            }

            // 4. takefield (Only if visible)
            if (takelist.IsVisible)
            {
                var selectedTake = takelist.SelectedItem as OptionDetails;
                if (selectedTake != null)
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "takefield");
                    if (record != null)
                    {
                        record.AnswerId = selectedTake.AnswerId;
                    }
                }
            }

            // 5. nutrientlistfield (Multi-selection list, only if visible)
            if (extralist.IsVisible)
            {
                var selectedNutrients = extralist.SelectedItems?.Cast<OptionDetails>().ToList();
                if (selectedNutrients != null && selectedNutrients.Any())
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "nutrientlistfield");
                    if (record != null)
                    {
                        // Joins multiple Answer IDs into a comma-separated string
                        record.AnswerId = string.Join(", ", selectedNutrients.Select(x => x.AnswerId));
                    }
                }
            }
        }
        catch (Exception Ex)
        {
            // Handle or log error
        }
    }

    async void AddMenstrualInfo()
    {
        try
        {
            // 1. menstrualcyclefield
            var selectedMC = mensturallist.SelectedItem as OptionDetails;
            if (selectedMC != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "menstrualcyclefield");
                if (record != null)
                {
                    record.AnswerId = selectedMC.AnswerId;
                }
            }

            // 2. pregnancyweeksfield
            if (preghelper.IsVisible)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "pregnancyweeksfield");
                if (record != null)
                {

                    record.AnswerValue = pregweeksentry?.Text?.Trim() ?? "";
                }
            }

            // 3. deliverydatefield
            if (ddhelper.IsVisible)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "deliverydatefield");
                if (record != null)
                {

                    record.AnswerValue = pregdateentry?.Text?.Trim() ?? "";
                }
            }
        }
        catch (Exception Ex)
        {
            // Debug.WriteLine(ex.Message);
        }
    }

    async void AddHtInfo()
    {
        try
        {
            // 1. mobilityfield
            var selectedMob = moblist.SelectedItem as OptionDetails;
            if (selectedMob != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "mobilityfield");
                if (record != null)
                {
                    record.AnswerId = selectedMob.AnswerId;
                }
            }

            // 2. selfcarefield
            var selectedSC = sclist.SelectedItem as OptionDetails;
            if (selectedSC != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "selfcarefield");
                if (record != null)
                {
                    record.AnswerId = selectedSC.AnswerId;
                }
            }

            // 3. usualactivitiesfield
            var selectedUC = uclist.SelectedItem as OptionDetails;
            if (selectedUC != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "usualactivitiesfield");
                if (record != null)
                {
                    record.AnswerId = selectedUC.AnswerId;
                }
            }

            // 4. painfield
            var selectedPain = painlist.SelectedItem as OptionDetails;
            if (selectedPain != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "painfield");
                if (record != null)
                {
                    record.AnswerId = selectedPain.AnswerId;
                }
            }

            // 5. anxietydepressionfield
            var selectedDep = deplist.SelectedItem as OptionDetails;
            if (selectedDep != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "anxietydepressionfield");
                if (record != null)
                {
                    record.AnswerId = selectedDep.AnswerId;
                }
            }

            // 6. healthvasfield (Slider)
            var sliderRecord = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "healthvasfield");
            if (sliderRecord != null)
            {
                // Using AnswerValue to store the numerical string, similar to your Entry fields
                sliderRecord.AnswerValue = Math.Round(healthSlider.Value).ToString();
            }
        }
        catch (Exception Ex)
        {
            // Debug.WriteLine(ex.Message);
        }
    }

    async void AddAddqInfo()
    {
        try
        {

            var itemq = addqlist.SelectedItem as OptionDetails;


            if (itemq.Text.Contains("Yes"))
            {
                var getover16 = AllregfieldsNotRequired.Where(x => x.Type == "over16").OrderBy(x => Convert.ToInt32(x.Order)).ToList();


                int targetIndex = Convert.ToInt32(getover16[0].Order);
                int safeIndex = Math.Min(targetIndex, Allregfields.Count);

                var itemqq = Allregfields.FirstOrDefault(x => x.Type == "over16main");
                var index = Allregfields.IndexOf(itemqq);


                foreach (var item in getover16)
                {
                    if (!Allregfields.Contains(item))
                    {
                        index++;
                        Allregfields.Insert(index, item);

                    }
                }

                //foreach (var item in getover16)
                //{
                //    if (!Allregfields.Contains(item))
                //    {
                //        int itemOrder = Convert.ToInt32(item.Order);

                //        // Ensure index is valid at insertion time
                //        int insertIndex = Math.Min(itemOrder, Allregfields.Count);

                //        Allregfields.Insert(insertIndex, item);
                //    }
                //}

            }
            else
            {

                Allregfields.RemoveAll(x => x.Type == "over16");

                //var getover16 = Allregfields.Where(x => x.Type == "over16").ToList();

                //if (getover16 != null)
                //{

                //    foreach (var item in getover16)
                //    {
                //        if (!Allregfields.Contains(item))
                //        {
                //            Allregfields.Remove(item);
                //        }
                //    }
                //}

            }

            if (itemq != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "addqlifestyle");
                if (record != null)
                {
                    record.AnswerId = itemq.AnswerId;
                }
            }
            //check if we need to show the menstural cycle part
            //                var date = DateTime.Parse(dateEntry.Text);

            //            int age = DateTime.Now.Year - date.Year;
            //            if (DateTime.Now.Date < date.AddYears(age)) // birthday not yet reached this year
            //                age--;


            //            //check if the user is under 16

            //            if (age < 16)
            //            {

            //                //var getover16items = Allregfields.Where(x => x.Type == "over16").ToList();

            //                //Over16regfields = getover16items;


            //                //foreach (var item in Over16regfields)
            //                //{
            //                //    Allregfields.Remove(item);
            //                //}

            //            }
            //            else
            //            {
            //                if (Over16regfields.Count != 0)
            //                {
            //                    foreach (var item in Over16regfields)
            //                    {
            //                        if (!Allregfields.Contains(item))
            //                        {
            //                            Allregfields.Add(item);
            //                        }
            //                    }
            //                }


            ////                Allregfields = Allregfields
            ////.Where(x => x.Active)
            ////.OrderBy(x => int.TryParse(x.Order, out var orderVal) ? orderVal : int.MaxValue)
            ////.ToList();
            //            }

            //// Store old values
            //int oldSegmentCount = topprogress.SegmentCount;
            //double oldProgress = topprogress.Progress;

            //// Update segment count
            //topprogress.SegmentCount = Allregfields.Count;

            //double percentComplete = 0;

            //if (oldSegmentCount > 0)
            //{
            //    percentComplete = oldProgress / oldSegmentCount;

            //    //  if user was at 100%, pull them back one step
            //    if (percentComplete >= 1)
            //    {
            //        percentComplete = (oldSegmentCount - 1) / (double)oldSegmentCount;
            //    }
            //}

            //// Apply to new total
            //topprogress.Progress = percentComplete * topprogress.SegmentCount;


        }
        catch (Exception Ex)
        {

        }
    }

    async void AddAntiViralInfo()
    {
        try
        {
            // 1. tobaccoeverfield
            var selectedEver = heardavlist.SelectedItem as OptionDetails;
            if (selectedEver != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "antiviralheardfield");
                if (record != null) record.AnswerId = selectedEver.AnswerId;
            }

            // 2. tobaccotypesfield
            if (usedavlbl.IsVisible)
            {
                var selectedType = usedavlist.SelectedItem as OptionDetails;
                if (selectedType != null)
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "antiviralprescribedfield");
                    if (record != null) record.AnswerId = selectedType.AnswerId;
                }
            }




            var selectedCurrent = futureavlist.SelectedItem as OptionDetails;
            if (selectedCurrent != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "antiviralhospitalfield");
                if (record != null) record.AnswerId = selectedCurrent.AnswerId;
            }





            var selectedFreq = futureavlist2.SelectedItem as OptionDetails;
            if (selectedFreq != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "antiviraldurationfield");
                if (record != null) record.AnswerId = selectedFreq.AnswerId;
            }



            var selectedFreq2 = futureavlist3.SelectedItem as OptionDetails;
            if (selectedFreq2 != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "antiviralpreventionfield");
                if (record != null) record.AnswerId = selectedFreq2.AnswerId;
            }


            var selectedFreq3 = futureavlist4.SelectedItem as OptionDetails;
            if (selectedFreq3 != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "antiviralsideeffectsfield");
                if (record != null) record.AnswerId = selectedFreq3.AnswerId;
            }


        }
        catch (Exception Ex)
        {
            // Debug.WriteLine(ex.Message);
        }
    }

    async void AddTobaccoInfo()
    {
        try
        {
            // 1. tobaccoeverfield
            var selectedEver = smokelist.SelectedItem as OptionDetails;
            if (selectedEver != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "tobaccoeverfield");
                if (record != null) record.AnswerId = selectedEver.AnswerId;
            }

            // 2. tobaccotypesfield
            if (usesmokerlbl.IsVisible)
            {
                var selectedType = usesmokelist.SelectedItem as OptionDetails;
                if (selectedType != null)
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "tobaccotypesfield");
                    if (record != null) record.AnswerId = selectedType.AnswerId;
                }
            }

            // 3. tobaccostartagefield
            if (ageofsmokelbl.IsVisible)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "tobaccostartagefield");
                if (record != null) record.AnswerValue = agesmokeentry?.Text?.Trim() ?? "";
            }

            // 4. tobaccocurrentfield
            if (usesmokelbl.IsVisible)
            {
                var selectedCurrent = usesmokelistr.SelectedItem as OptionDetails;
                if (selectedCurrent != null)
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "tobaccocurrentfield");
                    if (record != null) record.AnswerId = selectedCurrent.AnswerId;
                }
            }

            // 5. tobaccostopagefield
            if (stopsmokelbl.IsVisible)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "tobaccostopagefield");
                if (record != null) record.AnswerValue = stopsmokeentry?.Text?.Trim() ?? "";
            }

            // 6. tobaccofreqfield
            if (smokefreqlbl.IsVisible)
            {
                var selectedFreq = smokefreqlistr.SelectedItem as OptionDetails;
                if (selectedFreq != null)
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "tobaccofreqfield");
                    if (record != null) record.AnswerId = selectedFreq.AnswerId;
                }
            }
        }
        catch (Exception Ex)
        {
            // Debug.WriteLine(ex.Message);
        }
    }

    async void AddAlcoholInfo()
    {
        try
        {
            // 1. alcoholfreqfield
            var selectedFreq = alochollist.SelectedItem as OptionDetails;
            if (selectedFreq != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "alcoholfreqfield");
                if (record != null)
                {
                    record.AnswerId = selectedFreq.AnswerId;
                }
            }

            // 2. alcoholunitsfield
            if (usealochollist.IsVisible)
            {
                var selectedUnits = usealochollist.SelectedItem as OptionDetails;
                if (selectedUnits != null)
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "alcoholunitsfield");
                    if (record != null)
                    {
                        record.AnswerId = selectedUnits.AnswerId;
                    }
                }
            }
        }
        catch (Exception Ex)
        {
            // Debug.WriteLine(ex.Message);
        }
    }

    async void AddDrugInfo()
    {
        try
        {
            // 1. drugseverfield
            var selectedEver = druglist.SelectedItem as OptionDetails;
            if (selectedEver != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "drugseverfield");
                if (record != null) record.AnswerId = selectedEver.AnswerId;
            }

            // 2. drugslistfield
            if (whatdrugslist.IsVisible)
            {
                var selectedType = whatdrugslist.SelectedItem as OptionDetails;
                if (selectedType != null)
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "drugslistfield");
                    if (record != null) record.AnswerId = selectedType.AnswerId;
                }
            }

            // 3. drugsfreqfield
            if (drugoftenlist.IsVisible)
            {
                var selectedFreq = drugoftenlist.SelectedItem as OptionDetails;
                if (selectedFreq != null)
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "drugsfreqfield");
                    if (record != null) record.AnswerId = selectedFreq.AnswerId;
                }
            }

            // 4. drugsbreathingfield
            if (breathinglist.IsVisible)
            {
                var selectedBreathing = breathinglist.SelectedItem as OptionDetails;
                if (selectedBreathing != null)
                {
                    var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "drugsbreathingfield");
                    if (record != null) record.AnswerId = selectedBreathing.AnswerId;
                }
            }
        }
        catch (Exception Ex)
        {
            // Debug.WriteLine(ex.Message);
        }
    }

    async void AddSleepInfo()
    {
        try
        {
            // 1. sleeponsetfield
            var selectedOnset = sleeplist.SelectedItem as OptionDetails;
            if (selectedOnset != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "sleeponsetfield");
                if (record != null) record.AnswerId = selectedOnset.AnswerId;
            }

            // 2. wakedurationfield
            var selectedWake = wakelist.SelectedItem as OptionDetails;
            if (selectedWake != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "wakedurationfield");
                if (record != null) record.AnswerId = selectedWake.AnswerId;
            }

            // 3. sleepproblemfreqfield
            var selectedFreq = nightslist.SelectedItem as OptionDetails;
            if (selectedFreq != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "sleepproblemfreqfield");
                if (record != null) record.AnswerId = selectedFreq.AnswerId;
            }

            // 4. sleepqualityfield
            var selectedQuality = qualitylist.SelectedItem as OptionDetails;
            if (selectedQuality != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "sleepqualityfield");
                if (record != null) record.AnswerId = selectedQuality.AnswerId;
            }

            // 5. impactmoodfield
            var selectedMood = moodlist.SelectedItem as OptionDetails;
            if (selectedMood != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "impactmoodfield");
                if (record != null) record.AnswerId = selectedMood.AnswerId;
            }

            // 6. impactprodfield
            var selectedProd = prodlist.SelectedItem as OptionDetails;
            if (selectedProd != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "impactprodfield");
                if (record != null) record.AnswerId = selectedProd.AnswerId;
            }

            // 7. sleeptroubledfield
            var selectedTroubled = poorsleeplist.SelectedItem as OptionDetails;
            if (selectedTroubled != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "sleeptroubledfield");
                if (record != null) record.AnswerId = selectedTroubled.AnswerId;
            }

            // 8. sleepdurationfield
            var selectedDuration = sleepproblist.SelectedItem as OptionDetails;
            if (selectedDuration != null)
            {
                var record = QuestionnaireResults.FirstOrDefault(a => a.InternalName == "sleepdurationfield");
                if (record != null) record.AnswerId = selectedDuration.AnswerId;
            }
        }
        catch (Exception Ex)
        {
            // Debug.WriteLine(ex.Message);
        }
    }

    async void AddTandCsInfo()
    {
        try
        {
            //Add non Required Field only (Required needed to progress)
            var selectedConsentIds = allconsentdetails.consentcontent
              .SelectMany(section => section.sectioncontent)
              .Where(item => item.ChckedState && !item.required)
              .Select(item => item.consentitemid)
              .ToList();

            if (selectedConsentIds != null)
            {
                TandCNonReqired = string.Join("|", selectedConsentIds);
            }

        }
        catch (Exception Ex)
        {
            // Debug.WriteLine(ex.Message);
        }
    }

    void AddOrUpdateAnswer(string fieldId, string? value, bool isList)
    {
        try
        {
            var field = Allregfields[currentFieldIndex];

            var subField = field.subFields.FirstOrDefault(f => f.Id == fieldId);

            if (subField == null || string.IsNullOrWhiteSpace(value))
                return;

            var existing = UserSelectedQuestionnaire
                .FirstOrDefault(x => x.questionid == subField.questionid);

            if (existing == null)
            {
                UserSelectedQuestionnaire.Add(new RegQuestionAnswerJson
                {
                    questionid = subField.questionid,
                    value = isList ? null : value,
                    answerid = isList ? value : null
                });
            }
            else
            {
                // Update existing
                if (isList)
                    existing.answerid = value;
                else
                    existing.value = value;
            }
        }
        catch (Exception Ex)
        {

        }
    }

    private void ClearAddressbtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            addressonehelper.IsVisible = false;
            townhelper.IsVisible = false;
            countyhelper.IsVisible = false;
            addressoneentry.Text = string.Empty;
            townentry.Text = string.Empty;
            countyentry.Text = string.Empty;
            postcodelist.IsVisible = true;
            ClearAddressbtn.IsVisible = false;
            postcodelist.SelectedItem = null;
            postcodehelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "ClearAddressbtn_Clicked");
        }
    }

    private void ClearAllErrorLabelsAndHelpers()
    {
        try
        {
            //register details
            fnhelper.HasError = false;
            snhelper.HasError = false;
            telhelper.HasError = false;
            emailhelper.HasError = false;
            passhelper.HasError = false;
            confirmpasshelper.HasError = false;

            //household members stack
            firstfamhelper.HasError = false;
            firstsurnamehelper.HasError = false;
            firstemailhelper.HasError = false;
            agemember1error.IsVisible = false;
            typemember1error.IsVisible = false;
            firstcheckboxerror.IsVisible = false;

            firstfamhelper2.HasError = false;
            firstsurnamehelper2.HasError = false;
            firstemailhelper2.HasError = false;
            agemember1error2.IsVisible = false;
            typemember1error2.IsVisible = false;
            secondcheckboxerror.IsVisible = false;


            //address stack
            postcodehelper.HasError = false;

            //genderstack
            dobhelper.HasError = false;
            genderlisterror.IsVisible = false;
            gendermatchlisterror.IsVisible = false;
            sexiderror.IsVisible = false;


            //ethnicity
            ukerror.IsVisible = false;
            moveerror.IsVisible = false;
            countyerror.IsVisible = false;
            etherror.IsVisible = false;


            //height weight and activity
            weighterror.IsVisible = false;
            weighthelper.HasError = false;
            heighterror.IsVisible = false;
            heightHelper.HasError = false;
            heightcmhelper.HasError = false;
            stepserror.IsVisible = false;
            gymerror.IsVisible = false;

            //education
            educationerror.IsVisible = false;
            sitiuationerror.IsVisible = false;
            workerror.IsVisible = false;


            //household structure

            peoplenumhelper.HasError = false;
            sharedbathroomsnumhelper.HasError = false;
            roomnumhelper.HasError = false;
            venterrorlbl.IsVisible = false;
            damperrorlbl.IsVisible = false;


            //nhs num
            nhshelper.HasError = false;
            gpaddresserrorlbl.IsVisible = false;
            gperrorlbl.IsVisible = false;

            //ri 
            cougherrorlbl.IsVisible = false;
            hospitalerrorlbl.IsVisible = false;
            infectionhelper.HasError = false;
            venhoserrorlbl.IsVisible = false;

            //hc
            hashcerrorlbl.IsVisible = false;

            //add hc
            hcadderrorlbl.IsVisible = false;
            hcnotinlisterrorlbl.IsVisible = false;
            bodyparterrorlbl.IsVisible = false;
            otherentryconerrorlbl.IsVisible = false;
            cancererrorlbl.IsVisible = false;
            pastcancererrorlbl.IsVisible = false;

            //medications
            medadderrorlbl.IsVisible = false;

            //add medications
            medchipadderrorlbl.IsVisible = false;
            othermederrorlbl.IsVisible = false;
            othermedentryerrorlbl.IsVisible = false;

            //diet 
            dieterrorlbl.IsVisible = false;
            dietlenghtlbl.IsVisible = false;
            anysuppserrorlbl.IsVisible = false;
            takesuppserrorlbl.IsVisible = false;
            whichsuppserrorlbl.IsVisible = false;

            //menstural
            menstrualerrorlbl.IsVisible = false;
            preghelper.HasError = false;
            ddhelper.HasError = false;

            //ht
            ht1errorlbl.IsVisible = false;
            ht2errorlbl.IsVisible = false;
            ht3errorlbl.IsVisible = false;
            ht4errorlbl.IsVisible = false;
            ht5errorlbl.IsVisible = false;
            ht6errorlbl.IsVisible = false;

            //additonalq
            aqerrorlbl.IsVisible = false;

            //antiviral
            heardaverrorlbl.IsVisible = false;
            usedaverrorlbl.IsVisible = false;
            futureaverrorlbl.IsVisible = false;
            futureaverrorlbl2.IsVisible = false;
            futureaverrorlbl3.IsVisible = false;
            futureaverrorlbl4.IsVisible = false;

            //smoking
            smokeerrorlbl.IsVisible = false;
            usesmokeerrorlbl.IsVisible = false;
            agesmokehelper.HasError = false;
            usingsmokeerrorlbl.IsVisible = false;
            stopsmokehelper.HasError = false;
            smokingerrorlbl.IsVisible = false;

            //alcohol
            alcoholerrorlbl.IsVisible = false;
            usealcoholerrorlbl.IsVisible = false;

            //drugs
            drugserrorlbl.IsVisible = false;
            whatdrugserrorlbl.IsVisible = false;
            drugsoftenerrorlbl.IsVisible = false;
            breathingerrorlbl.IsVisible = false;

            //sleep
            sleeperrorlbl.IsVisible = false;
            wakeerrorlbl.IsVisible = false;
            nightserrorlbl.IsVisible = false;
            qualityerrorlbl.IsVisible = false;
            mooderrorlbl.IsVisible = false;
            proderrorlbl.IsVisible = false;
            poorsleeperrorlbl.IsVisible = false;
            sleepproderrorlbl.IsVisible = false;


        }
        catch (Exception Ex)
        {

        }
    }

    private void updateprogress()
    {
        try
        {
            if (topprogress2.IsVisible)
            {
                topprogress2.Progress += progressamountquestionnaire;
            }
            else
            {


                topprogress.Maximum = Allregfields.Count - 1;
                topprogress.SegmentCount = Allregfields.Count - 1;
                currentFieldIndex = Math.Min(currentFieldIndex, Allregfields.Count - 1);

                topprogress.Progress = currentFieldIndex;

                ClearAllErrorLabelsAndHelpers();

                //topprogress.Maximum = Allregfields.Count;
                //// 1. Update total steps
                //topprogress.SegmentCount = Allregfields.Count;

                //// 2. Keep current index valid
                //currentFieldIndex = Math.Min(currentFieldIndex, Allregfields.Count);

                //// 3. Update progress position
                //topprogress.Progress = currentFieldIndex;

                // topprogress.Progress += progressamount;
            }

        }
        catch (Exception Ex)
        {

        }
    }

    private void updatebackprogress()
    {
        try
        {

            if (topprogress2.IsVisible)
            {
                topprogress2.Progress -= progressamountquestionnaire;
            }
            else
            {


                topprogress.Maximum = Allregfields.Count - 1;
                topprogress.SegmentCount = Allregfields.Count - 1;

                // 2. Ensure currentFieldIndex doesn't drop below 0
                currentFieldIndex = Math.Max(0, currentFieldIndex);

                // 3. Set the progress directly to the index
                topprogress.Progress = currentFieldIndex;
                // topprogress.Progress -= progressamount;
            }

        }
        catch (Exception Ex)
        {

        }
    }


    private async Task ShowCurrentStack()
    {
        try
        {



            var field = Allregfields[currentFieldIndex];
            // var questionfield = Allquesfields[currentFieldIndexquestionnaire];

            if (topprogress2.IsVisible)
            {


            }
            else
            {
                toplbl.Text = field.Label;
                sublbl.Text = field.Placeholder;
                infomainlbl.Text = field.HelpText;
                infomainlbl.IsVisible = string.IsNullOrEmpty(field.HelpText) ? false : true;
                nextbtn.Text = "Next";

                if (field.XamlNameArea == "namestack")
                {
                    //// Safely locate the 'fillingFor' (subField[0] equivalent) field
                    //var fillingForField = field.subFields.FirstOrDefault(f => f.Id == "fillingFor");

                    //if (fillingForField != null)
                    //{
                    //    usinglbl.Text = fillingForField.Label;
                    //    usinglist.ItemsSource = fillingForField.Options;
                    //}
                    //else
                    //{
                    //    // Handle the error: Log it, or set default/empty values
                    //    // usinglbl.Text = "Error: Field Missing";
                    //    // usinglist.ItemsSource = new List<string>();
                    //}

                    //var homeOwnerField = field.subFields.FirstOrDefault(f => f.Id == "homeOwnerNominated");
                    //if (homeOwnerField != null)
                    //{
                    //    studylbl.Text = homeOwnerField.Label;
                    //    studyreplist.ItemsSource = homeOwnerField.Options;
                    //    infostudylbl.Text = homeOwnerField.HelpText;
                    //}

                    //// Safely locate the 'relationship' (subField[1] equivalent) field
                    //var relationshipField = field.subFields.FirstOrDefault(f => f.Id == "relationship");

                    //if (relationshipField != null)
                    //{
                    //    relationlbl.Text = relationshipField.Label;
                    //    relationlist.ItemsSource = relationshipField.Options;
                    //}
                    //else
                    //{
                    //    // Handle the error: Log it, or set default/empty values
                    //    // relationlbl.Text = "Error: Field Missing";
                    //    // relationlist.ItemsSource = new List<string>();
                    //}
                }

                if (field.XamlNameArea == "mainuserstack")
                {

                    if (userdetails.Primaryuser == true)
                    {

                        var fillingForField = field.subFields.FirstOrDefault(f => f.Id == "familyrelationship");

                        if (fillingForField != null)
                        {
                            // usinglbl.Text = fillingForField.Label;
                            //  firstmemeberchips.ItemsSource = fillingForField.Options;
                            familymember1list.ItemsSource = fillingForField.Options;
                            familymember1list2.ItemsSource = fillingForField.Options;
                        }

                    }
                    else
                    {
                        //skip this field
                    }

                    //// Safely locate the 'fillingFor' (subField[0] equivalent) field
                    //var fillingForField = field.subFields.FirstOrDefault(f => f.Id == "fillingFor");

                    //if (fillingForField != null)
                    //{
                    //    usinglbl.Text = fillingForField.Label;
                    //    usinglist.ItemsSource = fillingForField.Options;
                    //}
                    //else
                    //{
                    //    // Handle the error: Log it, or set default/empty values
                    //    // usinglbl.Text = "Error: Field Missing";
                    //    // usinglist.ItemsSource = new List<string>();
                    //}

                    //var homeOwnerField = field.subFields.FirstOrDefault(f => f.Id == "homeOwnerNominated");
                    //if (homeOwnerField != null)
                    //{
                    //    studylbl.Text = homeOwnerField.Label;
                    //    studyreplist.ItemsSource = homeOwnerField.Options;
                    //    infostudylbl.Text = homeOwnerField.HelpText;
                    //}

                    //// Safely locate the 'relationship' (subField[1] equivalent) field
                    //var relationshipField = field.subFields.FirstOrDefault(f => f.Id == "relationship");

                    //if (relationshipField != null)
                    //{
                    //    relationlbl.Text = relationshipField.Label;
                    //    relationlist.ItemsSource = relationshipField.Options;
                    //}
                    //else
                    //{
                    //    // Handle the error: Log it, or set default/empty values
                    //    // relationlbl.Text = "Error: Field Missing";
                    //    // relationlist.ItemsSource = new List<string>();
                    //}
                }

                if (field.XamlNameArea == "genderstack")
                {

                    var dobfield = field.subFields.FirstOrDefault(f => f.Id == "dob");

                    if (dobfield != null)
                    {
                        doblbl.Text = dobfield.Label;
                        infodoblbl.Text = dobfield.HelpText;

                    }

                    var genderfield = field.subFields.FirstOrDefault(f => f.Id == "sexAtBirth");

                    if (genderfield != null)
                    {
                        sexlbl.Text = genderfield.Label;
                        sexsublbl.Text = genderfield.SubLabel;
                        infosexlbl.Text = genderfield.HelpText;
                        genderlist.ItemsSource = genderfield.Options;
                    }

                    var gendermatch = field.subFields.FirstOrDefault(f => f.Id == "genderMatch");

                    if (gendermatch != null)
                    {
                        sexmatchlbl.Text = gendermatch.Label;
                        gendermatchlist.ItemsSource = gendermatch.Options;

                        // Create the record if it doesn't exist yet
                        if (!QuestionnaireResults.Any(a => a.InternalName == "genderMatch"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "genderMatch",
                                QuestionId = gendermatch.questionid,
                                AnswerId = ""
                            });
                        }
                    }


                    var genderid = field.subFields.FirstOrDefault(f => f.Id == "genderidentity");

                    if (genderid != null)
                    {
                        genidlbl.Text = genderid.Label;
                        infogenidlbl.Text = genderid.HelpText;
                        genidlist.ItemsSource = genderid.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "genderidentity"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "genderidentity",
                                QuestionId = genderid.questionid,
                                AnswerId = ""
                            });
                        }
                    }


                }

                if (field.XamlNameArea == "ethnicitystack")
                {
                    var ethfield = field.subFields.FirstOrDefault(f => f.Id == "ethnicityDescription");

                    if (ethfield != null)
                    {
                        ethlbl.Text = ethfield.Label;
                        subethlbl.Text = ethfield.SubLabel;
                        ethnicitylist.ItemsSource = ethfield.Options;

                        var splitht = ethfield.HelpText.Split('|');
                        // var splithti = ethfield.HelpTextInfo.Split('|');


                        infoethlbl.Text = splitht[0];
                        infoethselectlbl.Text = splitht[1];

                    }

                    var ethfieldow = field.subFields.FirstOrDefault(f => f.Id == "ethnicityInOwnWords");

                    if (ethfieldow != null)
                    {
                        ethownwordslbl.Text = ethfieldow.Label;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "ethnicityInOwnWords"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "ethnicityInOwnWords",
                                QuestionId = ethfieldow.questionid,
                                AnswerId = ""
                            });
                        }

                    }

                    var bornuk = field.subFields.FirstOrDefault(f => f.Id == "bornInUK");

                    if (bornuk != null)
                    {
                        uklbl.Text = bornuk.Label;
                        infouklbl.Text = bornuk.HelpText;
                        uklist.ItemsSource = bornuk.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "bornInUK"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "bornInUK",
                                QuestionId = bornuk.questionid,
                                AnswerId = ""
                            });
                        }

                    }

                    var moveuk = field.subFields.FirstOrDefault(f => f.Id == "moveToUKAge");

                    if (moveuk != null)
                    {
                        movelbl.Text = moveuk.Label;
                        movelist.ItemsSource = moveuk.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "moveToUKAge"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "moveToUKAge",
                                QuestionId = moveuk.questionid,
                                AnswerId = ""
                            });
                        }

                    }

                    var country = field.subFields.FirstOrDefault(f => f.Id == "countryOfOrigin");

                    if (country != null)
                    {
                        countrylbl.Text = country.Label;
                        autocompletecounty.ItemsSource = country.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "countryOfOrigin"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "countryOfOrigin",
                                QuestionId = country.questionid,
                                AnswerId = ""
                            });
                        }

                    }

                }


                if (field.XamlNameArea == "bodymetricsstack")
                {

                    var hfield = field.subFields.FirstOrDefault(f => f.Id == "heightUnit");

                    if (hfield != null)
                    {
                        heightinputlbl.Text = hfield.Label;
                        heightinputlist.ItemsSource = hfield.Options;


                        if (!QuestionnaireResults.Any(a => a.InternalName == "heightUnit"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "heightUnit",
                                QuestionId = hfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    var wfield = field.subFields.FirstOrDefault(f => f.Id == "weightUnit");

                    if (wfield != null)
                    {
                        weightinputlbl.Text = wfield.Label;
                        weightinputlist.ItemsSource = wfield.Options;


                        if (!QuestionnaireResults.Any(a => a.InternalName == "weightUnit"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "weightUnit",
                                QuestionId = wfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    var stepsfield = field.subFields.FirstOrDefault(f => f.Id == "averageSteps");

                    if (stepsfield != null)
                    {
                        var date = DateTime.Parse(dateEntry.Text);

                        int age = DateTime.Now.Year - date.Year;
                        if (DateTime.Now.Date < date.AddYears(age)) // birthday not yet reached this year
                            age--;

                        if (age >= 16)
                        {
                            stepslbl.Text = stepsfield.Label;
                            stepslist.ItemsSource = stepsfield.Options;

                            stepslbl.IsVisible = true;
                            stepslist.IsVisible = true;
                        }
                        else
                        {
                            stepslbl.IsVisible = false;
                            stepslist.IsVisible = false;
                        }

                        if (!QuestionnaireResults.Any(a => a.InternalName == "averageSteps"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "averageSteps",
                                QuestionId = stepsfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }


                    var gymfield = field.subFields.FirstOrDefault(f => f.Id == "muscleMass");

                    if (gymfield != null)
                    {
                        var date = DateTime.Parse(dateEntry.Text);

                        int age = DateTime.Now.Year - date.Year;
                        if (DateTime.Now.Date < date.AddYears(age)) // birthday not yet reached this year
                            age--;

                        if (age >= 18)
                        {

                            gymlbl.Text = gymfield.Label;
                            gymlist.ItemsSource = gymfield.Options;
                            gymhelplbl.Text = gymfield.HelpText;

                            gymlbl.IsVisible = true;
                            gymlist.IsVisible = true;
                        }
                        else
                        {
                            gymlbl.IsVisible = false;
                            gymlist.IsVisible = false;
                        }


                        if (!QuestionnaireResults.Any(a => a.InternalName == "muscleMass"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "muscleMass",
                                QuestionId = gymfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }





                }


                if (field.XamlNameArea == "educationworkstack")
                {

                    var highesteducationfield = field.subFields.FirstOrDefault(f => f.Id == "highestEducation");

                    if (highesteducationfield != null)
                    {
                        highesteducationlbl.Text = highesteducationfield.Label;
                        infohighedulbl.Text = highesteducationfield.HelpText;
                        higheducationlist.ItemsSource = highesteducationfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "highestEducation"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "highestEducation",
                                QuestionId = highesteducationfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }


                    var currenteducationfield = field.subFields.FirstOrDefault(f => f.Id == "currentSituation");

                    if (currenteducationfield != null)
                    {
                        currentsituationlbl.Text = currenteducationfield.Label;
                        currentsituationlist.ItemsSource = currenteducationfield.Options;
                        Currentinfolbl.Text = currenteducationfield.HelpText;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "currentSituation"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "currentSituation",
                                QuestionId = currenteducationfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }

                    var typeworkfield = field.subFields.FirstOrDefault(f => f.Id == "workType");

                    if (typeworkfield != null)
                    {
                        typeworklbl.Text = typeworkfield.Label;
                        typeworklist.ItemsSource = typeworkfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "workType"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "workType",
                                QuestionId = typeworkfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }


                }

                if (field.XamlNameArea == "householdstructurestack")
                {

                    var peoplefield = field.subFields.FirstOrDefault(f => f.Id == "peopleinhome");

                    if (peoplefield != null)
                    {
                        peoplelbl.Text = peoplefield.Label;
                        peoplesublbl.Text = peoplefield.SubLabel;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "peopleinhome"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "peopleinhome",
                                QuestionId = peoplefield.questionid,
                                AnswerId = ""
                            });
                        }

                    }


                    var roomsfield = field.subFields.FirstOrDefault(f => f.Id == "rooms");

                    if (roomsfield != null)
                    {
                        roomlbl.Text = roomsfield.Label;
                        roomsublbl.Text = roomsfield.SubLabel;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "rooms"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "rooms",
                                QuestionId = roomsfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    var bathroomsfield = field.subFields.FirstOrDefault(f => f.Id == "bathrooms");

                    if (bathroomsfield != null)
                    {
                        sharedbathroomslbl.Text = bathroomsfield.Label;
                        sharedbathroomssublbl.Text = bathroomsfield.SubLabel;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "bathrooms"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "bathrooms",
                                QuestionId = bathroomsfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }

                    var ventfield = field.subFields.FirstOrDefault(f => f.Id == "ventiliation");

                    if (ventfield != null)
                    {
                        ventlbl.Text = ventfield.Label;
                        ventlist.ItemsSource = ventfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "ventiliation"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "ventiliation",
                                QuestionId = ventfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    var mouldfield = field.subFields.FirstOrDefault(f => f.Id == "mould");

                    if (mouldfield != null)
                    {
                        damplbl.Text = mouldfield.Label;
                        damplist.ItemsSource = mouldfield.Options;


                        if (!QuestionnaireResults.Any(a => a.InternalName == "mould"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "mould",
                                QuestionId = mouldfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                }

                if (field.XamlNameArea == "nhsnumstack")
                {
                    var nhsnumfield = field.subFields.FirstOrDefault(f => f.Id == "nhsNumber");

                    if (nhsnumfield != null)
                    {
                        nhsnumlbl.Text = nhsnumfield.Label;
                        nhssublbl.Text = nhsnumfield.SubLabel;

                        var splitht = nhsnumfield.HelpText.Split('|');
                        // var splithti = ethfield.HelpTextInfo.Split('|');


                        infonhslbl.Text = splitht[0];
                        infonhsselectlbl.Text = splitht[1];


                        if (!QuestionnaireResults.Any(a => a.InternalName == "nhsNumber"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "nhsNumber",
                                QuestionId = nhsnumfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }


                    var gpfield = field.subFields.FirstOrDefault(f => f.Id == "gpRegistered");

                    if (gpfield != null)
                    {
                        gplbl.Text = gpfield.Label;
                        gbsublbl.Text = gpfield.SubLabel;
                        gplist.ItemsSource = gpfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "gpRegistered"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "gpRegistered",
                                QuestionId = gpfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }

                    var gpinfofield = field.subFields.FirstOrDefault(f => f.Id == "gpPracticeName");

                    if (gpinfofield != null)
                    {
                        gpinfolbl.Text = gpinfofield.Label;
                        gpsublbl.Text = gpinfofield.SubLabel;
                        GPPracticeLsit = gpinfofield.Options;
                        gpautocomplete.ItemsSource = gpinfofield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "gpPracticeName"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "gpPracticeName",
                                QuestionId = gpinfofield.questionid,
                                AnswerId = ""
                            });
                        }

                    }



                    ////check if we need to show the menstural cycle part
                    //var date = DateTime.Parse(dateEntry.Text);

                    //int age = DateTime.Now.Year - date.Year;
                    //if (DateTime.Now.Date < date.AddYears(age)) // birthday not yet reached this year
                    //    age--;

                    //if (age >= 15 && age <= 55 && genderatbirth == "Female")
                    //{
                    //    //do nothing as its already added
                    //    var getmenstrual = Allquesfields.Where(x => x.XamlNameArea == "menstrualstack").FirstOrDefault();
                    //    if (getmenstrual == null)
                    //    {

                    //        Allquesfields.Add(menstrualquestion);

                    //        topprogress2.SegmentCount = Allquesfields.Count;
                    //        progressamountquestionnaire = 100.0 / Allquesfields.Count;
                    //        topprogress2.Progress = 0;
                    //    }
                    //}
                    //else
                    //{
                    //    //remove it 
                    //    var getmenstrual = Allquesfields.Where(x => x.XamlNameArea == "menstrualstack").FirstOrDefault();
                    //    if (getmenstrual != null)
                    //    {
                    //        menstrualquestion = getmenstrual;
                    //        Allquesfields.Remove(getmenstrual);
                    //    }

                    //    topprogress2.SegmentCount = Allquesfields.Count;
                    //    progressamountquestionnaire = 100.0 / Allquesfields.Count;
                    //    topprogress2.Progress = 0;

                    //}

                }

                if (field.XamlNameArea == "ristack")
                {

                    var rifield = field.subFields.FirstOrDefault(f => f.Id == "coughfield");

                    if (rifield != null)
                    {


                        coughlbl.Text = rifield.Label;
                        coughlist.ItemsSource = rifield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "coughfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "coughfield",
                                QuestionId = rifield.questionid,
                                AnswerId = ""
                            });
                        }

                    }


                    var hosfield = field.subFields.FirstOrDefault(f => f.Id == "hosfield");

                    if (hosfield != null)
                    {

                        hoslbl.Text = hosfield.Label;
                        hoslist.ItemsSource = hosfield.Options;
                        hossublbl.Text = hosfield.SubLabel;
                        hoshelplbl.Text = hosfield.HelpText;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "hosfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "hosfield",
                                QuestionId = hosfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }


                    var infectionfield = field.subFields.FirstOrDefault(f => f.Id == "infectionfield");

                    if (infectionfield != null)
                    {

                        infectionlbl.Text = infectionfield.Label;
                        infectionsub.Text = infectionfield.SubLabel;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "infectionfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "infectionfield",
                                QuestionId = infectionfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }


                    var venfield = field.subFields.FirstOrDefault(f => f.Id == "venfield");

                    if (venfield != null)
                    {

                        venlbl.Text = venfield.Label;
                        venlist.ItemsSource = venfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "venfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "venfield",
                                QuestionId = venfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }


                }

                if (field.XamlNameArea == "hcstack")
                {


                    var diafield = field.subFields.FirstOrDefault(f => f.Id == "diastack");

                    if (diafield != null)
                    {


                        dialbl.Text = diafield.Label;
                        dialbldirections.Text = diafield.SubLabel;

                        disautocomplete.ItemsSource = diafield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "diastack"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "diastack",
                                QuestionId = diafield.questionid,
                                AnswerId = ""
                            });
                        }


                    }



                }


                if (field.XamlNameArea == "addhcstack")
                {

                    if (hcfirstlist.SelectedItem.ToString() == "No")
                    {
                        //  currentFieldIndex++;
                        //  ShowCurrentStack();
                        // updateprogress();
                    }
                    else
                    {

                        var diafield = field.subFields.FirstOrDefault(f => f.Id == "diaaddstack");

                        if (diafield != null)
                        {


                            diaaddlbl.Text = diafield.Label;
                            diaaddlbldirections.Text = diafield.SubLabel;

                            disautocomplete.ItemsSource = diafield.Options;

                            if (!QuestionnaireResults.Any(a => a.InternalName == "diaaddstack"))
                            {
                                QuestionnaireResults.Add(new QuestionnaireResult
                                {
                                    InternalName = "diaaddstack",
                                    QuestionId = diafield.questionid,
                                    AnswerId = ""
                                });
                            }
                        }





                        var otherdiafield = field.subFields.FirstOrDefault(f => f.Id == "otherhc");

                        if (otherdiafield != null)
                        {


                            otherhclbl.Text = otherdiafield.Label;
                            // dialbldirections.Text = diafield.SubLabel;

                            otherhclist.ItemsSource = otherdiafield.Options;

                            if (!QuestionnaireResults.Any(a => a.InternalName == "otherhc"))
                            {
                                QuestionnaireResults.Add(new QuestionnaireResult
                                {
                                    InternalName = "otherhc",
                                    QuestionId = otherdiafield.questionid,
                                    AnswerId = ""
                                });
                            }


                        }

                        var othertypediafield = field.subFields.FirstOrDefault(f => f.Id == "affectedsystem");

                        if (othertypediafield != null)
                        {


                            typeotherhclbl.Text = othertypediafield.Label;
                            // dialbldirections.Text = diafield.SubLabel;

                            typeotherhclist.ItemsSource = othertypediafield.Options;

                            if (!QuestionnaireResults.Any(a => a.InternalName == "affectedsystem"))
                            {
                                QuestionnaireResults.Add(new QuestionnaireResult
                                {
                                    InternalName = "affectedsystem",
                                    QuestionId = othertypediafield.questionid,
                                    AnswerId = ""
                                });
                            }


                        }

                        var otherhcenter = field.subFields.FirstOrDefault(f => f.Id == "otherhealthconditions");

                        if (otherhcenter != null)
                        {


                            otherhcenterlbl.Text = otherhcenter.Label;


                            otherhcentersublbl.Text = otherhcenter.Placeholder;

                            if (!QuestionnaireResults.Any(a => a.InternalName == "otherhealthconditions"))
                            {
                                QuestionnaireResults.Add(new QuestionnaireResult
                                {
                                    InternalName = "otherhealthconditions",
                                    QuestionId = otherhcenter.questionid,
                                    AnswerId = ""
                                });
                            }


                        }

                        var pastcancer = field.subFields.FirstOrDefault(f => f.Id == "pastcancerdiagnosis");

                        if (pastcancer != null)
                        {


                            cancerlbl.Text = pastcancer.Label;
                            // dialbldirections.Text = diafield.SubLabel;

                            cancerlist.ItemsSource = pastcancer.Options;

                            if (!QuestionnaireResults.Any(a => a.InternalName == "pastcancerdiagnosis"))
                            {
                                QuestionnaireResults.Add(new QuestionnaireResult
                                {
                                    InternalName = "pastcancerdiagnosis",
                                    QuestionId = pastcancer.questionid,
                                    AnswerId = ""
                                });
                            }


                        }

                        var cancernow = field.subFields.FirstOrDefault(f => f.Id == "cancerremissionstatus");

                        if (cancernow != null)
                        {


                            cancernowlbl.Text = cancernow.Label;
                            // dialbldirections.Text = diafield.SubLabel;

                            cancernowlist.ItemsSource = cancernow.Options;

                            if (!QuestionnaireResults.Any(a => a.InternalName == "cancerremissionstatus"))
                            {
                                QuestionnaireResults.Add(new QuestionnaireResult
                                {
                                    InternalName = "cancerremissionstatus",
                                    QuestionId = cancernow.questionid,
                                    AnswerId = ""
                                });
                            }


                        }

                    }


                }

                if (field.XamlNameArea == "medynstack")
                {


                    var medynfield = field.subFields.FirstOrDefault(f => f.Id == "firstmedstack");

                    if (medynfield != null)
                    {


                        medynlbl.Text = medynfield.Label;
                        medynlbldirections.Text = medynfield.SubLabel;

                        // medautocomplete.ItemsSource = medfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "firstmedstack"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "firstmedstack",
                                QuestionId = medynfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }






                }

                if (field.XamlNameArea == "medicationsstack")
                {





                    var medfield = field.subFields.FirstOrDefault(f => f.Id == "medqstack");

                    if (medfield != null)
                    {


                        medlbl.Text = medfield.Label;
                        medlbldirections.Text = medfield.SubLabel;

                        medautocomplete.ItemsSource = medfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "medqstack"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "medqstack",
                                QuestionId = medfield.questionid,
                                AnswerId = ""
                            });
                        }

                    }

                    var otherdiafield = field.subFields.FirstOrDefault(f => f.Id == "othermeds");

                    if (otherdiafield != null)
                    {


                        othermedlbl.Text = otherdiafield.Label;
                        // dialbldirections.Text = diafield.SubLabel;

                        othermedlist.ItemsSource = otherdiafield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "othermeds"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "othermeds",
                                QuestionId = otherdiafield.questionid,
                                AnswerId = ""
                            });
                        }


                    }

                    var otherhcenter = field.subFields.FirstOrDefault(f => f.Id == "othermedications");

                    if (otherhcenter != null)
                    {


                        othermeddetailslbl.Text = otherhcenter.Label;


                        othermeddetailssublbl.Text = otherhcenter.Placeholder;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "othermedications"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "othermedications",
                                QuestionId = otherhcenter.questionid,
                                AnswerId = ""
                            });
                        }


                    }



                }

                if (field.XamlNameArea == "rvstack")
                {

                    var flufield = field.subFields.FirstOrDefault(f => f.Id == "flufield");

                    if (flufield != null)
                    {


                        flulbl.Text = flufield.Label;
                        flubldirections.Text = flufield.SubLabel;

                        flulist.ItemsSource = flufield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "flufield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "flufield",
                                QuestionId = flufield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    //var flunosefield = field.subFields.FirstOrDefault(f => f.Id == "flunosefield");

                    //if (flunosefield != null)
                    //{


                    //    flunoselbl.Text = flunosefield.Label;
                    //    flunoseldirections.Text = flunosefield.SubLabel;

                    //    flunoselist.ItemsSource = flunosefield.Options;

                    //    if (!QuestionnaireResults.Any(a => a.InternalName == "flunosefield"))
                    //    {
                    //        QuestionnaireResults.Add(new QuestionnaireResult
                    //        {
                    //            InternalName = "flunosefield",
                    //            QuestionId = flunosefield.questionid,
                    //            AnswerId = ""
                    //        });
                    //    }
                    //}



                    var fludatefield = field.subFields.FirstOrDefault(f => f.Id == "fludatefield");

                    if (fludatefield != null)
                    {


                        fludatelbl.Text = fludatefield.Label;
                        fludateldirections.Text = fludatefield.SubLabel;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "fludatefield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "fludatefield",
                                QuestionId = fludatefield.questionid,
                                AnswerId = ""
                            });
                        }

                    }

                    //var covidfield = field.subFields.FirstOrDefault(f => f.Id == "covidfield");

                    //if (covidfield != null)
                    //{


                    //    covidlbl.Text = covidfield.Label;
                    //    covidlbldirections.Text = covidfield.SubLabel;

                    //    covidlist.ItemsSource = covidfield.Options;

                    //    if (!QuestionnaireResults.Any(a => a.InternalName == "covidfield"))
                    //    {
                    //        QuestionnaireResults.Add(new QuestionnaireResult
                    //        {
                    //            InternalName = "covidfield",
                    //            QuestionId = covidfield.questionid,
                    //            AnswerId = ""
                    //        });
                    //    }

                    //}

                    var coviddatefield = field.subFields.FirstOrDefault(f => f.Id == "coviddatefield");

                    if (coviddatefield != null)
                    {


                        coviddatelbl.Text = coviddatefield.Label;
                        coviddatelbldirections.Text = coviddatefield.SubLabel;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "coviddatefield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "coviddatefield",
                                QuestionId = coviddatefield.questionid,
                                AnswerId = ""
                            });
                        }

                    }


                    //var rsvfield = field.subFields.FirstOrDefault(f => f.Id == "rsvfield");

                    //if (rsvfield != null)
                    //{


                    //    rsvlbl.Text = rsvfield.Label;
                    //    rsvdirections.Text = rsvfield.SubLabel;
                    //    rsvlist.ItemsSource = rsvfield.Options;


                    //    if (!QuestionnaireResults.Any(a => a.InternalName == "rsvfield"))
                    //    {
                    //        QuestionnaireResults.Add(new QuestionnaireResult
                    //        {
                    //            InternalName = "rsvfield",
                    //            QuestionId = rsvfield.questionid,
                    //            AnswerId = ""
                    //        });
                    //    }

                    //}

                    var rsvdatefield = field.subFields.FirstOrDefault(f => f.Id == "rsvdatefield");

                    if (rsvdatefield != null)
                    {


                        rsvdatelbl.Text = rsvdatefield.Label;
                        rsvdatedirections.Text = rsvdatefield.SubLabel;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "rsvdatefield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "rsvdatefield",
                                QuestionId = rsvdatefield.questionid,
                                AnswerId = ""
                            });
                        }

                    }


                    //var othervacfield = field.subFields.FirstOrDefault(f => f.Id == "othervacfield");

                    //if (othervacfield != null)
                    //{


                    //    othervaclbl.Text = othervacfield.Label;
                    //    othvacdirections.Text = othervacfield.SubLabel;

                    //    othevaclist.ItemsSource = othervacfield.Options;

                    //    if (!QuestionnaireResults.Any(a => a.InternalName == "othervacfield"))
                    //    {
                    //        QuestionnaireResults.Add(new QuestionnaireResult
                    //        {
                    //            InternalName = "othervacfield",
                    //            QuestionId = othervacfield.questionid,
                    //            AnswerId = ""
                    //        });
                    //    }

                    //}

                    //var othervaclistfield = field.subFields.FirstOrDefault(f => f.Id == "otherlistfield");

                    //if (othervaclistfield != null)
                    //{


                    //    othervaclistlbl.Text = othervaclistfield.Label;
                    //    othvaclistdirections.Text = othervaclistfield.SubLabel;

                    //    othevaclistlist.ItemsSource = othervaclistfield.Options;

                    //    if (!QuestionnaireResults.Any(a => a.InternalName == "otherlistfield"))
                    //    {
                    //        QuestionnaireResults.Add(new QuestionnaireResult
                    //        {
                    //            InternalName = "otherlistfield",
                    //            QuestionId = othervaclistfield.questionid,
                    //            AnswerId = ""
                    //        });
                    //    }

                    //}




                }

                if (field.XamlNameArea == "dietstack")
                {
                    // --- dietfield ---
                    var dietfield = field.subFields.FirstOrDefault(f => f.Id == "dietfield");
                    if (dietfield != null)
                    {
                        dietlbl.Text = dietfield.Label;
                        dietbldirections.Text = dietfield.SubLabel;
                        dietbldirections.IsVisible = string.IsNullOrEmpty(dietfield.SubLabel) ? false : true;
                        dietlist.ItemsSource = dietfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "dietfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "dietfield",
                                QuestionId = dietfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- dietlengthfield ---
                    var dietlenghtfield = field.subFields.FirstOrDefault(f => f.Id == "dietlengthfield");
                    if (dietlenghtfield != null)
                    {
                        dietlengthlbl.Text = dietlenghtfield.Label;
                        // dietlengthdirections.Text = dietlenghtfield.SubLabel; // Uncomment if needed
                        dietlengthlist.ItemsSource = dietlenghtfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "dietlengthfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "dietlengthfield",
                                QuestionId = dietlenghtfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- supplementsfield ---
                    var anyfield = field.subFields.FirstOrDefault(f => f.Id == "supplementsfield");
                    if (anyfield != null)
                    {
                        anylbl.Text = anyfield.Label;
                        anylbldirections.Text = anyfield.SubLabel;
                        anylbldirections.IsVisible = string.IsNullOrEmpty(anyfield.SubLabel) ? false : true;
                        anylist.ItemsSource = anyfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "supplementsfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "supplementsfield",
                                QuestionId = anyfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- takefield ---
                    var takefield = field.subFields.FirstOrDefault(f => f.Id == "takefield");
                    if (takefield != null)
                    {
                        takelbl.Text = takefield.Label;
                        takelbldirections.Text = takefield.SubLabel;
                        takelbldirections.IsVisible = string.IsNullOrEmpty(takefield.SubLabel) ? false : true;
                        takelist.ItemsSource = takefield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "takefield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "takefield",
                                QuestionId = takefield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- nutrientlistfield ---
                    var extrafield = field.subFields.FirstOrDefault(f => f.Id == "nutrientlistfield");
                    if (extrafield != null)
                    {
                        extralbl.Text = extrafield.Label;
                        extralbldirections.Text = extrafield.SubLabel;
                        extralbldirections.IsVisible = string.IsNullOrEmpty(extrafield.SubLabel) ? false : true;
                        extralist.ItemsSource = extrafield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "nutrientlistfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "nutrientlistfield",
                                QuestionId = extrafield.questionid,
                                AnswerId = ""
                            });
                        }
                    }
                }

                if (field.XamlNameArea == "menstrualstack")
                {
                    // --- menstrualcyclefield ---
                    var mcfield = field.subFields.FirstOrDefault(f => f.Id == "menstrualcyclefield");
                    if (mcfield != null)
                    {
                        mensturallbl.Text = mcfield.Label;
                        // mensturaldirections.Text = mcfield.SubLabel; // Uncomment if you have a directions label
                        mensturallist.ItemsSource = mcfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "menstrualcyclefield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "menstrualcyclefield",
                                QuestionId = mcfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- pregnancyweeksfield ---
                    var pregfield = field.subFields.FirstOrDefault(f => f.Id == "pregnancyweeksfield");
                    if (pregfield != null)
                    {
                        preglbl.Text = pregfield.Label;
                        // preglblist.ItemsSource = pregfield.Options; // Add this if pregnancyweeks is a list

                        if (!QuestionnaireResults.Any(a => a.InternalName == "pregnancyweeksfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "pregnancyweeksfield",
                                QuestionId = pregfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- deliverydatefield ---
                    var ddfield = field.subFields.FirstOrDefault(f => f.Id == "deliverydatefield");
                    if (ddfield != null)
                    {
                        ddlbl.Text = ddfield.Label;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "deliverydatefield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "deliverydatefield",
                                QuestionId = ddfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }
                }

                if (field.XamlNameArea == "htstack")
                {
                    // --- mobilityfield ---
                    var mobfield = field.subFields.FirstOrDefault(f => f.Id == "mobilityfield");
                    if (mobfield != null)
                    {
                        moblbl.Text = mobfield.Label;
                        mobbldirections.Text = mobfield.SubLabel;
                        mobbldirections.IsVisible = string.IsNullOrEmpty(mobfield.SubLabel) ? false : true;
                        moblist.ItemsSource = mobfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "mobilityfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "mobilityfield",
                                QuestionId = mobfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- selfcarefield ---
                    var scfield = field.subFields.FirstOrDefault(f => f.Id == "selfcarefield");
                    if (scfield != null)
                    {
                        sclbl.Text = scfield.Label;
                        scbldirections.Text = scfield.SubLabel;
                        scbldirections.IsVisible = string.IsNullOrEmpty(scfield.SubLabel) ? false : true;
                        sclist.ItemsSource = scfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "selfcarefield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "selfcarefield",
                                QuestionId = scfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- usualactivitiesfield ---
                    var uafield = field.subFields.FirstOrDefault(f => f.Id == "usualactivitiesfield");
                    if (uafield != null)
                    {
                        uclbl.Text = uafield.Label;
                        uclbldirections.Text = uafield.SubLabel;
                        uclbldirections.IsVisible = string.IsNullOrEmpty(uafield.SubLabel) ? false : true;
                        uclist.ItemsSource = uafield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "usualactivitiesfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "usualactivitiesfield",
                                QuestionId = uafield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- painfield ---
                    var painfield = field.subFields.FirstOrDefault(f => f.Id == "painfield");
                    if (painfield != null)
                    {
                        painlbl.Text = painfield.Label;
                        painlbldirections.Text = painfield.SubLabel;
                        painlbldirections.IsVisible = string.IsNullOrEmpty(painfield.SubLabel) ? false : true;
                        painlist.ItemsSource = painfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "painfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "painfield",
                                QuestionId = painfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- anxietydepressionfield ---
                    var depfield = field.subFields.FirstOrDefault(f => f.Id == "anxietydepressionfield");
                    if (depfield != null)
                    {
                        deplbl.Text = depfield.Label;
                        depbldirections.Text = depfield.SubLabel;
                        depbldirections.IsVisible = string.IsNullOrEmpty(depfield.SubLabel) ? false : true;
                        deplist.ItemsSource = depfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "anxietydepressionfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "anxietydepressionfield",
                                QuestionId = depfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- healthvasfield (Slider) ---
                    var sliderfield = field.subFields.FirstOrDefault(f => f.Id == "healthvasfield");
                    if (sliderfield != null)
                    {
                        sliderlbl.Text = sliderfield.Label;
                        sliderlbldirections.Text = sliderfield.SubLabel;
                        sliderlbldirections.IsVisible = string.IsNullOrEmpty(sliderfield.SubLabel) ? false : true;

                        var answer0 = sliderfield.Options.FirstOrDefault(a => a.Value == "0");
                        var answer100 = sliderfield.Options.FirstOrDefault(a => a.Value == "100");

                        slider0lbl.Text = answer0?.Text;
                        slider100lbl.Text = answer100?.Text;

                        slidernumlbl.Text = "50";

                        if (!QuestionnaireResults.Any(a => a.InternalName == "healthvasfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "healthvasfield",
                                QuestionId = sliderfield.questionid,
                                AnswerId = "" // This will likely be updated by a Slider.ValueChanged event later
                            });
                        }
                    }
                }

                if (field.XamlNameArea == "addressstack")
                {

                    var depfield = field.subFields.FirstOrDefault(f => f.Id == "addressLine1");
                    if (depfield != null)
                    {

                         validpostcodelist = depfield.Options[0].validpostcodesvalues;


                    }


                    if (studyreplist.SelectedItems.Count != 0)
                    {
                        var item = studyreplist.SelectedItem as OptionDetails;

                        if (item.Text == "No")
                        {

                            //  addressonehelper.IsVisible = false;
                            //  townhelper.IsVisible = false;
                            //  countyhelper.IsVisible = false;
                        }
                        else
                        {
                            // addressonehelper.IsVisible = true;
                            // townhelper.IsVisible = true;
                            //  countyhelper.IsVisible = true;
                        }
                    }
                    else
                    {
                        //  addressonehelper.IsVisible = false;
                        // townhelper.IsVisible = false;
                        // countyhelper.IsVisible = false;
                    }

                }

                //if (field.XamlNameArea == "questionnairestack")
                //{

                //    // topprogress2.Progress += progressamountquestionnaire;
                //   // topprogress2.IsVisible = true;
                //    toplbl.Text = questionfield.Label;
                //    sublbl.Text = questionfield.Placeholder;

                //    var rifield = questionfield.Fields.FirstOrDefault(f => f.XamlNameArea == "coughfield");

                //    if (rifield != null)
                //    {


                //        coughlbl.Text = rifield.Label;
                //        coughlist.ItemsSource = rifield.Order;

                //    }


                //    var hosfield = questionfield.Fields.FirstOrDefault(f => f.XamlNameArea == "hosfield");

                //    if (hosfield != null)
                //    {

                //        hoslbl.Text = hosfield.Label;
                //        hoslist.ItemsSource = hosfield.Answers;

                //    }


                //    var infectionfield = questionfield.Fields.FirstOrDefault(f => f.XamlNameArea == "infectionfield");

                //    if (infectionfield != null)
                //    {

                //        infectionlbl.Text = infectionfield.Label;


                //    }


                //    var venfield = questionfield.Fields.FirstOrDefault(f => f.XamlNameArea == "venfield");

                //    if (venfield != null)
                //    {

                //        venlbl.Text = venfield.Label;
                //        venlist.ItemsSource = venfield.Answers;

                //    }


                //    skipbtn.IsVisible = !questionfield.Required;
                //    SetStackVisibility(questionfield.XamlNameArea, true);

                //    return;



                //}

                if (field.XamlNameArea == "additionalqstack")
                {

                    var tobfield = field.subFields.FirstOrDefault(f => f.Id == "addqlifestyle");
                    if (tobfield != null)
                    {
                        aqtitle.Text = tobfield.Label;
                        aqsubtitle.Text = tobfield.SubLabel;
                        addqlist.ItemsSource = tobfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "addqlifestyle"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "addqlifestyle",
                                QuestionId = tobfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }


                }

                if (field.XamlNameArea == "antiviralstack")
                {

                    avinfolbl.Text = field.HelpTextInfo;

                    // --- tobaccoeverfield ---
                    var tobfield = field.subFields.FirstOrDefault(f => f.Id == "antiviralheardfield");
                    if (tobfield != null)
                    {
                        heardavlbl.Text = tobfield.Label;
                        // smokelbldirections.Text = tobfield.SubLabel;
                        heardavlist.ItemsSource = tobfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralheardfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "antiviralheardfield",
                                QuestionId = tobfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- tobaccotypesfield ---
                    var typefield = field.subFields.FirstOrDefault(f => f.Id == "antiviralprescribedfield");
                    if (typefield != null)
                    {
                        usedavlbl.Text = typefield.Label;
                        usedavlist.ItemsSource = typefield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralprescribedfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "antiviralprescribedfield",
                                QuestionId = typefield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- tobaccostartagefield ---
                    var agefield = field.subFields.FirstOrDefault(f => f.Id == "antiviralhospitalfield");
                    if (agefield != null)
                    {
                        futureavlbl.Text = agefield.Label;
                        futureavlist.ItemsSource = agefield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralhospitalfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "antiviralhospitalfield",
                                QuestionId = agefield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- tobaccocurrentfield ---
                    var currentfield = field.subFields.FirstOrDefault(f => f.Id == "antiviraldurationfield");
                    if (currentfield != null)
                    {
                        futureavlbl2.Text = currentfield.Label;
                        futureavlist2.ItemsSource = currentfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "antiviraldurationfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "antiviraldurationfield",
                                QuestionId = currentfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- tobaccostopagefield ---
                    var stopfield = field.subFields.FirstOrDefault(f => f.Id == "antiviralpreventionfield");
                    if (stopfield != null)
                    {
                        futureavlbl3.Text = stopfield.Label;
                        futureavlist3.ItemsSource = stopfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralpreventionfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "antiviralpreventionfield",
                                QuestionId = stopfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- tobaccofreqfield ---
                    var freqfield = field.subFields.FirstOrDefault(f => f.Id == "antiviralsideeffectsfield");
                    if (freqfield != null)
                    {
                        futureavlbl4.Text = freqfield.Label;
                        futureavlist4.ItemsSource = freqfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "antiviralsideeffectsfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "antiviralsideeffectsfield",
                                QuestionId = freqfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }
                }

                if (field.XamlNameArea == "tobaccostack")
                {
                    // --- tobaccoeverfield ---
                    var tobfield = field.subFields.FirstOrDefault(f => f.Id == "tobaccoeverfield");
                    if (tobfield != null)
                    {
                        smokelbl.Text = tobfield.Label;
                        smokelbldirections.Text = tobfield.SubLabel;
                        smokelist.ItemsSource = tobfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccoeverfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "tobaccoeverfield",
                                QuestionId = tobfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- tobaccotypesfield ---
                    var typefield = field.subFields.FirstOrDefault(f => f.Id == "tobaccotypesfield");
                    if (typefield != null)
                    {
                        usesmokerlbl.Text = typefield.Label;
                        usesmokelist.ItemsSource = typefield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccotypesfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "tobaccotypesfield",
                                QuestionId = typefield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- tobaccostartagefield ---
                    var agefield = field.subFields.FirstOrDefault(f => f.Id == "tobaccostartagefield");
                    if (agefield != null)
                    {
                        ageofsmokelbl.Text = agefield.Label;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccostartagefield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "tobaccostartagefield",
                                QuestionId = agefield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- tobaccocurrentfield ---
                    var currentfield = field.subFields.FirstOrDefault(f => f.Id == "tobaccocurrentfield");
                    if (currentfield != null)
                    {
                        usesmokelbl.Text = currentfield.Label;
                        usesmokelistr.ItemsSource = currentfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccocurrentfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "tobaccocurrentfield",
                                QuestionId = currentfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- tobaccostopagefield ---
                    var stopfield = field.subFields.FirstOrDefault(f => f.Id == "tobaccostopagefield");
                    if (stopfield != null)
                    {
                        stopsmokelbl.Text = stopfield.Label;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccostopagefield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "tobaccostopagefield",
                                QuestionId = stopfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- tobaccofreqfield ---
                    var freqfield = field.subFields.FirstOrDefault(f => f.Id == "tobaccofreqfield");
                    if (freqfield != null)
                    {
                        smokefreqlbl.Text = freqfield.Label;
                        smokefreqlistr.ItemsSource = freqfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "tobaccofreqfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "tobaccofreqfield",
                                QuestionId = freqfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }
                }

                if (field.XamlNameArea == "alcoholstack")
                {
                    // --- alcoholfreqfield ---
                    var alfield = field.subFields.FirstOrDefault(f => f.Id == "alcoholfreqfield");
                    if (alfield != null)
                    {
                        alcohollbl.Text = alfield.Label;
                        alochollist.ItemsSource = alfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "alcoholfreqfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "alcoholfreqfield",
                                QuestionId = alfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- alcoholunitsfield ---
                    var typefield = field.subFields.FirstOrDefault(f => f.Id == "alcoholunitsfield");
                    if (typefield != null)
                    {
                        usealochollbl.Text = typefield.Label;
                        usealochollist.ItemsSource = typefield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "alcoholunitsfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "alcoholunitsfield",
                                QuestionId = typefield.questionid,
                                AnswerId = ""
                            });
                        }
                    }
                }


                if (field.XamlNameArea == "drugstack")
                {
                    // --- drugseverfield ---
                    var alfield = field.subFields.FirstOrDefault(f => f.Id == "drugseverfield");
                    if (alfield != null)
                    {
                        druglbl.Text = alfield.Label;
                        druglbldirections.Text = alfield.SubLabel;
                        druglist.ItemsSource = alfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "drugseverfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "drugseverfield",
                                QuestionId = alfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- drugslistfield ---
                    var typefield = field.subFields.FirstOrDefault(f => f.Id == "drugslistfield");
                    if (typefield != null)
                    {
                        whatdrugslbl.Text = typefield.Label;
                        whatdrugslist.ItemsSource = typefield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "drugslistfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "drugslistfield",
                                QuestionId = typefield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- drugsfreqfield ---
                    var oftenfield = field.subFields.FirstOrDefault(f => f.Id == "drugsfreqfield");
                    if (oftenfield != null)
                    {
                        drugoftenlbl.Text = oftenfield.Label;
                        drugoftenlist.ItemsSource = oftenfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "drugsfreqfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "drugsfreqfield",
                                QuestionId = oftenfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- drugsbreathingfield ---

                    var brefield = field.subFields.FirstOrDefault(f => f.Id == "drugssymptomsfield");
                    if (brefield != null)
                    {
                        breathinglbl.Text = brefield.Label;
                        breathinglist.ItemsSource = brefield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "drugssymptomsfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "drugssymptomsfield",
                                QuestionId = brefield.questionid,
                                AnswerId = ""
                            });
                        }
                    }
                }

                if (field.XamlNameArea == "sleepstack")
                {
                    // --- sleeponsetfield ---
                    var alfield = field.subFields.FirstOrDefault(f => f.Id == "sleeponsetfield");
                    if (alfield != null)
                    {
                        sleeplbl.Text = alfield.Label;
                        sleeplist.ItemsSource = alfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "sleeponsetfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "sleeponsetfield",
                                QuestionId = alfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- wakedurationfield ---
                    var typefield = field.subFields.FirstOrDefault(f => f.Id == "wakedurationfield");
                    if (typefield != null)
                    {
                        wakelbl.Text = typefield.Label;
                        wakelist.ItemsSource = typefield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "wakedurationfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "wakedurationfield",
                                QuestionId = typefield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- sleepproblemfreqfield ---
                    var oftenfield = field.subFields.FirstOrDefault(f => f.Id == "sleepproblemfreqfield");
                    if (oftenfield != null)
                    {
                        nightslbl.Text = oftenfield.Label;
                        nightslist.ItemsSource = oftenfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "sleepproblemfreqfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "sleepproblemfreqfield",
                                QuestionId = oftenfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- sleepqualityfield ---
                    var brefield = field.subFields.FirstOrDefault(f => f.Id == "sleepqualityfield");
                    if (brefield != null)
                    {
                        qualitylbl.Text = brefield.Label;
                        qualitylist.ItemsSource = brefield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "sleepqualityfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "sleepqualityfield",
                                QuestionId = brefield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- impactmoodfield ---
                    var moodfield = field.subFields.FirstOrDefault(f => f.Id == "impactmoodfield");
                    if (moodfield != null)
                    {
                        moodlbl.Text = moodfield.Label;
                        moodlist.ItemsSource = moodfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "impactmoodfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "impactmoodfield",
                                QuestionId = moodfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- impactprodfield ---
                    var mood2field = field.subFields.FirstOrDefault(f => f.Id == "impactprodfield");
                    if (mood2field != null)
                    {
                        prodlbl.Text = mood2field.Label;
                        prodlist.ItemsSource = mood2field.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "impactprodfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "impactprodfield",
                                QuestionId = mood2field.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- sleeptroubledfield ---
                    var trofield = field.subFields.FirstOrDefault(f => f.Id == "sleeptroubledfield");
                    if (trofield != null)
                    {
                        poorsleeplbl.Text = trofield.Label;
                        poorsleeplist.ItemsSource = trofield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "sleeptroubledfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "sleeptroubledfield",
                                QuestionId = trofield.questionid,
                                AnswerId = ""
                            });
                        }
                    }

                    // --- sleepdurationfield ---
                    var durfield = field.subFields.FirstOrDefault(f => f.Id == "sleepdurationfield");
                    if (durfield != null)
                    {
                        sleepproblbl.Text = durfield.Label;
                        sleepproblist.ItemsSource = durfield.Options;

                        if (!QuestionnaireResults.Any(a => a.InternalName == "sleepdurationfield"))
                        {
                            QuestionnaireResults.Add(new QuestionnaireResult
                            {
                                InternalName = "sleepdurationfield",
                                QuestionId = durfield.questionid,
                                AnswerId = ""
                            });
                        }
                    }
                }

                if (field.XamlNameArea == "termsstack")
                {
                    if (allconsentdetails == null)
                    {
                        var config = JsonConvert.DeserializeObject<ObservableCollection<ConsentDetails>>(signupcodedetails.consent);
                        under10stack.IsVisible = false;
                        await PopulateConsent(config);
                        MainConsentCollection.ItemsSource = null;
                        MainConsentCollection.ItemsSource = allconsentdetails.consentcontent;
                    }

                }

                //Grouped Data Version
                //var config = JsonConvert.DeserializeObject<ObservableCollection<ConsentDetails>>(signupcodedetails.consent);

                //if (householdrepFROMREG)
                //{
                //    allconsentdetails = config.FirstOrDefault(x => x.age == "16+");
                //}
                //else
                //{
                //    // Add other age logic here
                //}

                //if (allconsentdetails != null)
                //{
                //    foreach (var section in allconsentdetails.consentcontent)
                //    {
                //        foreach (var item in section.sectioncontent)
                //        {
                //            if (item.required)
                //            {
                //                item.requiredlbl = "Required";
                //            }
                //        }
                //    }

                //    Groupeddata = allconsentdetails.consentcontent.Select(s =>
                //        new SectionConsent(s.section, s.sectioncontent)).ToList();

                //    MainConsentCollection.ItemsSource = null;
                //    MainConsentCollection.ItemsSource = Groupeddata;

                //    //MainConsentCollection.InvalidateMeasure();

                //    //if (MainConsentCollection.Parent is Layout parentLayout)
                //    //{
                //    //    parentLayout.InvalidateMeasure();
                //    //}

                //}

                //}
                //}

                if (field.XamlNameArea == "finishstack")
                {
                    nextbtn.Text = "Finish";
                    backbuttonstack.IsVisible = false;
                    topprogress.IsVisible = false;

                    // --- sleeponsetfield ---
                    var alfield = field.subFields.FirstOrDefault(f => f.Id == "completionText");
                    if (alfield != null)
                    {
                        finishtext.Text = alfield.Label;

                        finishsubtext.Text = alfield.SubLabel;


                    }


                }

                //  skipbtn.IsVisible = !field.Required;
                SetStackVisibility(field.XamlNameArea, true);
            }
        }
        catch (Exception Ex)
        {

        }
    }

    private async Task PopulateConsent(ObservableCollection<ConsentDetails> config)
    {
        try
        {
            if (householdrepFROMREG)
            {
                allconsentdetails = config.Where(x => x.age == "16+").FirstOrDefault();
                if (allconsentdetails != null)
                {
                    over16namelbl.Text = allconsentdetails.signoffparameters[0].label;
                    over16signaturelbl.Text = allconsentdetails.signoffparameters[1].label;
                }
            }
            else
            {
                // Match logic against the fresh/current age group
                if (userinfoforbaseline.household_individual_age == "16+")
                {
                    allconsentdetails = config.Where(x => x.age == "16+").FirstOrDefault();
                    if (allconsentdetails != null)
                    {
                        over16namelbl.Text = allconsentdetails.signoffparameters[0].label;
                        over16signaturelbl.Text = allconsentdetails.signoffparameters[1].label;
                    }
                }
                else if (userinfoforbaseline.household_individual_age == "11 - 15")
                {
                    allconsentdetails = config.Where(x => x.age == "11 - 15").FirstOrDefault();
                    if (allconsentdetails != null)
                    {
                        over16namelbl.Text = allconsentdetails.signoffparameters[0].label;
                        over16signaturelbl.Text = allconsentdetails.signoffparameters[1].label;
                    }
                }
                else
                {
                    // 5 - 10
                    allconsentdetails = config.Where(x => x.age == "5 - 10").FirstOrDefault();
                    if (allconsentdetails != null)
                    {
                        var listofroles = allconsentdetails.signoffparameters.Where(x => x.type == "dropdown").FirstOrDefault();

                        under10namelbl.Text = allconsentdetails.signoffparameters[0].label;
                        over16namelbl.Text = allconsentdetails.signoffparameters[1].label;
                        over16signaturelbl.Text = allconsentdetails.signoffparameters[2].label;
                        under10rolelbl.Text = allconsentdetails.signoffparameters[3].label;

                        if (listofroles != null)
                        {
                            under10rolelist.ItemsSource = listofroles.options;
                        }
                        under10stack.IsVisible = true;
                    }
                }
            }

            // 3. Process required labels safely
            if (allconsentdetails != null)
            {
                foreach (var item in allconsentdetails.consentcontent)
                {
                    if (item.sectioncontent == null) continue;

                    foreach (var it in item.sectioncontent)
                    {
                        if (it.required)
                        {
                            it.requiredlbl = "Required";
                        }
                    }
                }
            }

        }
        catch (Exception Ex)
        {

        }
    }


    private void SetStackVisibility(string stackName, bool isVisible)
    {
        try
        {
            // find the stack by name using reflection
            var stackField = GetType().GetField(stackName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (stackField?.GetValue(this) is StackLayout stack)
                stack.IsVisible = isVisible;

            if (stackName == "namestack")
            {
                ClearErrors(NameHelpers);
            }
            else if (stackName == "mainuserstack")
            {
                ClearErrors(MainUserHelpers);
                agemember1error.IsVisible = false;
                typemember1error.IsVisible = false;
                agemember1error2.IsVisible = false;
                typemember1error2.IsVisible = false;
            }
            else if (stackName == "addressstack")
            {
                ClearErrors(AddressHelpers);
            }
            else if (stackName == "genderstack")
            {
                ClearErrors(GenderHelpers);
                genderlisterror.IsVisible = false;
                gendermatchlisterror.IsVisible = false;
                sexiderror.IsVisible = false;
            }
            else if (stackName == "ethnicitystack")
            {
                ukerror.IsVisible = false;
                moveerror.IsVisible = false;
                countyerror.IsVisible = false;
                etherror.IsVisible = false;
            }
            else if (stackName == "bodymetricsstack")
            {
                ClearErrors(BodyMetricsHelpers);
                weighterror.IsVisible = false;
                heighterror.IsVisible = false;
                stepserror.IsVisible = false;
                gymerror.IsVisible = false;
            }
            else if (stackName == "educationworkstack")
            {
                educationerror.IsVisible = false;
                sitiuationerror.IsVisible = false;
                workerror.IsVisible = false;
            }
            else if (stackName == "householdstructurestack")
            {
                ClearErrors(HouseHoldHelpers);
                venterrorlbl.IsVisible = false;
                damperrorlbl.IsVisible = false;
            }
            else if (stackName == "nhsnumstack")
            {
                gperrorlbl.IsVisible = false;
                gpaddresserrorlbl.IsVisible = false;
            }
            else if (stackName == "ristack")
            {
                cougherrorlbl.IsVisible = false;
                hospitalerrorlbl.IsVisible = false;
                venhoserrorlbl.IsVisible = false;
            }
            else if (stackName == "hcstack")
            {

            }
            else if (stackName == "medicationsstack")
            {

            }
            else if (stackName == "rvstack")
            {

            }
            else if (stackName == "dietstack")
            {

            }
            else if (stackName == "menstrualstack")
            {

            }
            else if (stackName == "htstack")
            {

            }

            else if (stackName == "tobaccostack")
            {

            }

            else if (stackName == "alcoholstack")
            {

            }

            else if (stackName == "drugstack")
            {


            }

            else if (stackName == "sleepstack")
            {

            }

        }
        catch (Exception Ex)
        {

        }
    }

    private bool ShowError(SfTextInputLayout layout, Entry entry, string message)
    {
        layout.HasError = true;
        layout.ErrorText = message;
        Vibration.Vibrate();
        //  entry.Focus();
        return false;
    }


    private async Task<bool> ValidateNameStack() // Changed to async Task<bool>
    {
        try
        {
            bool isValid = true;
            //

            // ---- Trim all entry text ----
            firstnameentry.Text = firstnameentry.Text?.Trim();
            surnameentry.Text = surnameentry.Text?.Trim();
            emailentry.Text = emailentry.Text?.Trim();
            firstpasswordentry.Text = firstpasswordentry.Text?.Trim();
            confirmpassentry.Text = confirmpassentry.Text?.Trim();

            // ---- First name ----
            if (string.IsNullOrWhiteSpace(firstnameentry.Text))
                isValid = ShowError(fnhelper, firstnameentry, "Please enter first name");

            // ---- Surname ----
            if (string.IsNullOrWhiteSpace(surnameentry.Text))
                isValid = ShowError(snhelper, surnameentry, "Please enter surname");

            if (householdrepFROMREG)
            {
                // ---- Email basic validation ----
                if (string.IsNullOrEmpty(telentry.Text))
                {
                    isValid = ShowError(telhelper, telentry, "Please enter an phone number");
                }
                else
                {
                    // Remove spaces, dashes, brackets for cleaner validation
                    string phone = telentry.Text.Trim();

                    // Example: allows optional +, digits, spaces, hyphens, parentheses
                    bool validPhone = Regex.IsMatch(
                        phone,
                        @"^\+?[0-9\s\-\(\)]{7,15}$"
                    );

                    if (!validPhone)
                    {
                        isValid = ShowError(telhelper, telentry, "Please enter a valid phone number");
                    }
                }

                // ---- Email basic validation ----
                if (string.IsNullOrEmpty(emailentry.Text))
                {
                    isValid = ShowError(emailhelper, emailentry, "Please enter an email address");
                }
                else if (!EmailIsValid(emailentry.Text))
                {
                    isValid = ShowError(emailhelper, emailentry, "Please enter a valid email address");
                }
                else
                {
                    // add this back in

                    // ---- Database Email Check ----
                    //  We only check the DB if the format is already valid
                    var checkuseremail = await APICalls.Instance.CheckEmailExists(emailentry.Text);

                    if (checkuseremail?.FirstOrDefault() is { primaryuser: true })
                    {
                        await DisplayAlert(
                            "Email address already in use",
                            "This email has been registered to an account.",
                            "Ok");

                        isValid = ShowError(emailhelper, emailentry, "Email already registered");
                    }

                }

            }

            if (noemailuserreg == false)
            {

                // ---- Password required ----
                if (string.IsNullOrEmpty(firstpasswordentry.Text))
                    isValid = ShowError(passhelper, firstpasswordentry, "Please enter a password");

                // ---- Password length ----
                else if (firstpasswordentry.Text.Length < 8)
                    isValid = ShowError(passhelper, firstpasswordentry, "Password must be greater than 8 characters");

                // ---- Password complexity ----
                else
                {
                    if (!Regex.IsMatch(firstpasswordentry.Text, "[A-Z]"))
                        isValid = ShowError(passhelper, firstpasswordentry, "Password must contain at least one uppercase letter.");

                    else if (!Regex.IsMatch(firstpasswordentry.Text, "[0-9]"))
                        isValid = ShowError(passhelper, firstpasswordentry, "Password must contain at least one number.");

                    else if (!Regex.IsMatch(firstpasswordentry.Text, "[!@#$%^&*()_+=\\[{\\]};:<>|./?-]"))
                        isValid = ShowError(passhelper, firstpasswordentry, "Password must contain at least one symbol.");
                }

                // ---- Confirm Password ----
                if (string.IsNullOrEmpty(confirmpassentry.Text))
                {
                    isValid = ShowError(confirmpasshelper, confirmpassentry, "Please confirm your password");
                }
                else if (firstpasswordentry.Text != confirmpassentry.Text)
                {
                    isValid = ShowError(confirmpasshelper, confirmpassentry, "Passwords do not match");
                }
            }

            return isValid;
        }
        catch (Exception Ex)
        {
            // Debug.WriteLine($"Validation Error: {ex.Message}");
            return false;
        }
    }

    private bool ValidateFormStack()
    {
        try
        {
            bool isValid = true;

            //rest these two 
            //firstemailhelper.ErrorText = string.Empty;
            //firstemailhelper2.ErrorText = string.Empty;

            if (string.IsNullOrEmpty(firstfamentry.Text))
            {
                firstfamhelper.HasError = true;
                isValid = false;
            }

            if (string.IsNullOrEmpty(firstsurnameentry.Text))
            {
                firstsurnamehelper.HasError = true;
                isValid = false;
            }


            if (usingphone1.SelectedItems.Count == 0)
            {
                usingphone1error1.IsVisible = true;
                isValid = false;
            }

            if (emailsectiongrid.IsVisible)
            {

                if (string.IsNullOrEmpty(firstemailentry.Text))
                {
                    firstemailhelper.HasError = true;
                    firstemailhelper.ErrorText = "Please enter email address";
                    isValid = false;
                }
                // ---- Email invalid ----
                else if (!EmailIsValid(firstemailentry.Text))
                {
                    firstemailhelper.HasError = true;
                    firstemailhelper.ErrorText = "Please enter a valid email address";
                    isValid = false;
                }
            }

            if (familymember1.SelectedItems.Count == 0)
            {
                agemember1error.IsVisible = true;
                isValid = false;
            }


            if (familymember1list.SelectedItems.Count == 0)
            {
                typemember1error.IsVisible = true;
                isValid = false;
            }

            if (commborder1.IsVisible)
            {

                if (!firsthouselholdcb.IsChecked)
                {
                    firstcheckboxerror.IsVisible = true;
                    isValid = false;
                }
            }


            //member 2
            if (string.IsNullOrEmpty(firstfamentry2.Text))
            {
                firstfamhelper2.HasError = true;
                isValid = false;
            }

            if (string.IsNullOrEmpty(firstsurnameentry2.Text))
            {
                firstsurnamehelper2.HasError = true;
                isValid = false;
            }


            if (usingphone2.SelectedItems.Count == 0)
            {
                usingphone2error2.IsVisible = true;
                isValid = false;
            }

            if (emailsectiongrid2.IsVisible)
            {

                if (string.IsNullOrEmpty(firstemailentry2.Text))
                {
                    firstemailhelper2.HasError = true;
                    firstemailhelper2.ErrorText = "Please enter email address";
                    isValid = false;
                }
                // ---- Email invalid ----
                else if (!EmailIsValid(firstemailentry2.Text))
                {
                    firstemailhelper2.HasError = true;
                    firstemailhelper2.ErrorText = "Please enter a valid email address";
                    isValid = false;
                }
            }


            if (emailsectiongrid.IsVisible && emailsectiongrid2.IsVisible)
            {
                // ---- Check Both emails aren't the same ----
                if (!string.IsNullOrEmpty(firstemailentry2.Text) && !string.IsNullOrEmpty(firstemailentry.Text))
                {
                    if (firstemailentry2.Text == firstemailentry.Text)
                    {
                        firstemailhelper2.HasError = true;
                        firstemailhelper2.ErrorText = "Each family member requires a unique email";
                        isValid = false;
                    }
                }
            }


            if (familymember12.SelectedItems.Count == 0)
            {
                agemember1error2.IsVisible = true;
                isValid = false;
            }


            if (familymember1list2.SelectedItems.Count == 0)
            {
                typemember1error2.IsVisible = true;
                isValid = false;
            }

            if (commborder2.IsVisible)
            {
                if (!secondhouseholdcb.IsChecked)
                {
                    secondcheckboxerror.IsVisible = true;
                    isValid = false;
                }
            }

            //// ---- Using list ----
            //if (usinglist.SelectedItems.Count == 0)
            //{
            //    usingerrorlbl.IsVisible = true;
            //    isValid = false;
            //}

            //// ---- Study list ----
            //if (studyreplist.IsVisible && studyreplist.SelectedItems.Count == 0)
            //{
            //    studyerrorlbl.IsVisible = true;
            //    isValid = false;
            //}

            //// ---- Relation list ----
            //if (relationlist.IsVisible && relationlist.SelectedItems.Count == 0)
            //{
            //    relationerrorlbl.IsVisible = true;
            //    isValid = false;
            //}

            //if(otherrelationhelper.IsVisible)
            //{
            //    if(string.IsNullOrEmpty(otherrelationentry.Text))
            //    {
            //        othergenderhelper.HasError = true;
            //    }
            //}

            return isValid;
        }
        catch (Exception Ex)
        {
            return false;
        }
    }
    private bool ValidateGenderStack()
    {
        try
        {
            bool isValid = true;

            // 1. Check if empty
            if (string.IsNullOrWhiteSpace(dateEntry.Text))
            {
                isValid = false;
                dobhelper.HasError = true;
            }
            else
            {
                // 2. Try to parse the date
                DateTime dob;
                bool parsed = DateTime.TryParseExact(
                    dateEntry.Text,
                    "dd/MM/yyyy",             // matches your masked input
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out dob);

                if (!parsed)
                {
                    isValid = false;
                    dobhelper.HasError = true;
                }
                else if (dob > DateTime.Today) // <-- check if date is in the future
                {
                    isValid = false;
                    //  dobhelper.ErrorText = "Date cannot be in the future";
                    dobhelper.HasError = true;
                }
                else
                {
                    userdetails.Age = dateEntry.Text;
                    isValid = true;
                }
            }


            if (genderlist.SelectedItems.Count == 0)
            {
                genderlisterror.IsVisible = true;
                isValid = false;
            }

            if (gendermatchlist.IsVisible)
            {
                if (gendermatchlist.SelectedItems.Count == 0)
                {
                    gendermatchlisterror.IsVisible = true;
                    isValid = false;
                }
            }

            if (othergenderhelper.IsVisible)
            {
                if (string.IsNullOrEmpty(othergenderentry.Text))
                {
                    othergenderhelper.HasError = true;
                    othergenderhelper.ErrorText = "Please enter gender";
                    isValid = false;
                }
                else
                {
                    userdetails.Gender = othergenderentry.Text;
                }
            }

            if (genidlbl.IsVisible)
            {
                if (genidlist.SelectedItems.Count == 0)
                {
                    sexiderror.IsVisible = true;
                    isValid = false;
                }
            }

            return isValid;
        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private bool ValidateEthnicityStack()
    {
        try
        {
            bool isValid = true;

            if (uklist.SelectedItems.Count == 0)
            {
                ukerror.IsVisible = true;
                isValid = false;
            }

            if (movelbl.IsVisible)
            {
                if (movelist.SelectedItems.Count == 0)
                {
                    moveerror.IsVisible = true;
                    isValid = false;
                }

                if (autocompletecounty.SelectedItem == null)
                {
                    countyerror.IsVisible = true;
                    isValid = false;
                }
            }

            if (ethnicitylist.SelectedItems.Count == 0)
            {
                etherror.IsVisible = true;
                isValid = false;
            }




            return isValid;

        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private bool ValidatedobStack()
    {
        try
        {
            bool isValid = true;


            if (!validdob)
            {
                isValid = false;
            }
            else
            {
                userdetails.Age = dateEntry.Text;
                isValid = true;
            }


            return isValid;
        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private bool ValidatebodymetricsStack()
    {
        try
        {
            bool isValid = true;

            if (weightinputlist.SelectedItems.Count == 0)
            {
                weighterror.IsVisible = true;
                isValid = false;
            }

            if (weighthelper.IsVisible)
            {
                if (string.IsNullOrEmpty(weightEntry.Text))
                {
                    weighthelper.HasError = true;
                    isValid = false;
                }
            }

            if (heightinputlist.SelectedItems.Count == 0)
            {
                heighterror.IsVisible = true;
                isValid = false;
            }

            if (heightHelper.IsVisible)
            {
                if (string.IsNullOrEmpty(feetEntry.Text))
                {
                    heightHelper.HasError = true;
                    isValid = false;
                }

                if (string.IsNullOrEmpty(inchesEntry.Text))
                {
                    heightHelper.HasError = true;
                    isValid = false;
                }
            }

            if (heightcmhelper.IsVisible)
            {
                if (string.IsNullOrEmpty(heightcmentry.Text))
                {
                    heightcmhelper.HasError = true;
                    isValid = false;
                }
            }

            if (stepslbl.IsVisible)
            {
                if (stepslist.SelectedItems.Count == 0)
                {
                    stepserror.IsVisible = true;
                    isValid = false;
                }
            }

            if (gymlbl.IsVisible)
            {
                if (gymlist.SelectedItems.Count == 0)
                {
                    gymerror.IsVisible = true;
                    isValid = false;
                }
            }




            return isValid;

        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private bool ValidateeducationStack()
    {
        try
        {
            bool isValid = true;

            if (higheducationlist.SelectedItems.Count == 0)
            {
                educationerror.IsVisible = true;
                isValid = false;
            }




            if (currentsituationlist.SelectedItems.Count == 0)
            {
                sitiuationerror.IsVisible = true;
                isValid = false;
            }

            if (typeworklbl.IsVisible)
            {

                if (typeworklist.SelectedItems.Count == 0)
                {
                    workerror.IsVisible = true;
                    isValid = false;
                }

            }



            return isValid;

        }
        catch (Exception Ex)
        {
            return false;
        }
    }
    private bool ValidateHouseholdstructureStack()
    {
        try
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(peopleentry.Text))
            {
                peoplenumhelper.HasError = true;
                isValid = false;
            }


            if (string.IsNullOrEmpty(roomentry.Text))
            {
                roomnumhelper.HasError = true;
                isValid = false;
            }

            if (string.IsNullOrEmpty(sharedbathroomsentry.Text))
            {
                sharedbathroomsnumhelper.HasError = true;
                isValid = false;
            }

            if (ventlist.SelectedItems.Count == 0)
            {
                venterrorlbl.IsVisible = true;
                isValid = false;
            }

            if (damplist.SelectedItems.Count == 0)
            {
                damperrorlbl.IsVisible = true;
                isValid = false;
            }





            return isValid;

        }
        catch (Exception Ex)
        {
            return false;
        }
    }
    private bool ValidatenhsnumStack()
    {
        try
        {
            bool isValid = true;


            if (!validnhsnum)
            {
                isValid = false;
                nhshelper.HasError = true;
                nhshelper.ErrorText = "Please enter a valid NHS number";
            }
            else if (string.IsNullOrEmpty(nhsentry.Text))
            {
                isValid = false;
                nhshelper.HasError = true;
                nhshelper.ErrorText = "Please enter a valid NHS number";
            }
            else
            {
                //add un where the user nhs number is stored
                // userdetails.Epid = nhsentry.Text;
                // userdetails.Age = dateEntry.Text;
                isValid = true;
            }

            if (gplist.SelectedItems.Count == 0)
            {
                gperrorlbl.IsVisible = true;
                isValid = false;
            }


            if (gpinfolbl.IsVisible)
            {
                if (gpautocomplete.SelectedItem == null)
                {
                    gpaddresserrorlbl.IsVisible = true;
                    isValid = false;
                }
            }


            return isValid;
        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private bool ValidateRIStack()
    {
        try
        {
            bool isValid = true;




            if (coughlist.SelectedItems.Count == 0)
            {
                cougherrorlbl.IsVisible = true;
                isValid = false;
            }

            if (hoslist.SelectedItems.Count == 0)
            {
                hospitalerrorlbl.IsVisible = true;
                isValid = false;
            }

            if (infectionlbl.IsVisible)
            {
                if (string.IsNullOrEmpty(infectionyearentry.Text))
                {
                    infectionhelper.HasError = true;
                    isValid = false;
                }
                else if (!int.TryParse(infectionyearentry.Text, out int year))
                {
                    // Not a valid number
                    infectionhelper.HasError = true;
                    isValid = false;
                }
                else if (year < 1900 || year > DateTime.Now.Year)
                {
                    // Too old or in the future
                    infectionhelper.HasError = true;
                    isValid = false;
                }
                else
                {
                    infectionhelper.HasError = false;
                }

                if (venlist.SelectedItems.Count == 0)
                {
                    venhoserrorlbl.IsVisible = true;
                    isValid = false;
                }
            }


            return isValid;
        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private bool ValidateHealthConditionsStack()
    {
        try
        {
            bool isValid = true;


            if (hcfirstlist.SelectedItems.Count == 0)
            {
                hashcerrorlbl.IsVisible = true;
                isValid = false;
            }



            return isValid;
        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private bool ValidateAddHealthConditionsStack()
    {
        try
        {
            bool isValid = true;

            //Condition might not be listed 
            //if(conditionschips.ItemsSource == null)
            //{
            //    hcadderrorlbl.IsVisible = true;
            //    isValid = false;
            //}



            if (otherhclist.SelectedItems.Count == 0)
            {
                hcnotinlisterrorlbl.IsVisible = true;
                isValid = false;
            }


            if (typeotherhclbl.IsVisible)
            {
                if (typeotherhclist.SelectedItems.Count == 0)
                {
                    bodyparterrorlbl.IsVisible = true;
                    isValid = false;
                }


                if (string.IsNullOrEmpty(otherhcentrytext.Text))
                {
                    otherentryconerrorlbl.IsVisible = true;
                    isValid = false;
                }
            }

            if (cancerlbl.IsVisible)
            {
                if (cancerlist.SelectedItems.Count == 0)
                {
                    cancererrorlbl.IsVisible = true;
                    isValid = false;
                }

                if (cancernowlist.SelectedItems.Count == 0)
                {
                    pastcancererrorlbl.IsVisible = true;
                    isValid = false;
                }
            }

            return isValid;
        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private bool ValidateMedicationsStack()
    {
        try
        {
            bool isValid = true;

            if (medsfirstlist.SelectedItems.Count == 0)
            {
                medadderrorlbl.IsVisible = true;
                isValid = false;
            }

            return isValid;
        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private bool ValidateMedicationsADDStack()
    {
        try
        {
            bool isValid = true;

            //Medications might not be listed
            //if (medicationschips.ItemsSource == null)
            //{
            //    medchipadderrorlbl.IsVisible = true;
            //    isValid = false;
            //}

            if (othermedlist.SelectedItems.Count == 0)
            {
                othermederrorlbl.IsVisible = true;
                isValid = false;
            }

            if (othermeddetailslbl.IsVisible)
            {
                if (string.IsNullOrEmpty(othermedtextentry.Text))
                {
                    othermedentryerrorlbl.IsVisible = true;
                    isValid = false;
                }
            }

            return isValid;
        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private bool ValidateRVStack()
    {
        try
        {
            bool isValid = true;

            if (flulist.SelectedItems.Count == 0)
            {
                fluerrorlbl.IsVisible = true;
                isValid = false;
            }

            var today = DateTime.Today;
            DateTime? BirthDate = DateTime.TryParse(newuser.dateofbirth, out var tempDate) ? tempDate : null;

            if (fluhelper.IsVisible)
            {
                var text = fluentry.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    SetError(fluhelper, "Enter Value");
                    isValid = false;
                }
                else if (!DateTime.TryParse(text, out var fluDate))
                {
                    SetError(fluhelper, "Enter Valid Date");
                    isValid = false;
                }
                else if (fluDate.Date > today)
                {
                    SetError(fluhelper, "Date cannot be in the future");
                    isValid = false;
                }
                else if (BirthDate.HasValue && fluDate.Date < BirthDate.Value.Date)
                {
                    SetError(fluhelper, "Date cannot be before birth date");
                    isValid = false;
                }
            }


            if (dateEntryCovidJab.IsVisible)
            {
                var text = coviddateentry.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    SetError(dateEntryCovidJab, "Enter Value");
                    isValid = false;
                }
                else if (!DateTime.TryParse(text, out var fluDate))
                {
                    SetError(dateEntryCovidJab, "Enter Valid Date");
                    isValid = false;
                }
                else if (fluDate.Date > today)
                {
                    SetError(dateEntryCovidJab, "Date cannot be in the future");
                    isValid = false;
                }
                else if (BirthDate.HasValue && fluDate.Date < BirthDate.Value.Date)
                {
                    SetError(dateEntryCovidJab, "Date cannot be before birth date");
                    isValid = false;
                }
            }


            if (dateEntryrsv.IsVisible)
            {
                var text = rsvdateentry.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    SetError(dateEntryrsv, "Enter Value");
                    isValid = false;
                }
                else if (!DateTime.TryParse(text, out var fluDate))
                {
                    SetError(dateEntryrsv, "Enter Valid Date");
                    isValid = false;
                }
                else if (fluDate.Date > today)
                {
                    SetError(dateEntryrsv, "Date cannot be in the future");
                    isValid = false;
                }
                else if (BirthDate.HasValue && fluDate.Date < BirthDate.Value.Date)
                {
                    SetError(dateEntryrsv, "Date cannot be before birth date");
                    isValid = false;
                }
            }


            //if (flunoselbl.IsVisible)
            //{
            //    if (flunoselist.SelectedItems.Count == 0)
            //    {
            //        flunoseerrorlbl.IsVisible = true;
            //        isValid = false;
            //    }

            //    if (string.IsNullOrEmpty(fluentry.Text))
            //    {
            //        fluhelper.HasError = true;
            //        isValid = false;
            //    }

            //}

            //if (covidlist.SelectedItems.Count == 0)
            //{
            //    coviderrorlbl.IsVisible = true;
            //    isValid = false;
            //}

            //if(coviddatelbldirections.IsVisible)
            //{
            //    if (string.IsNullOrEmpty(coviddateentry.Text))
            //    {
            //        dateEntryCovidJab.HasError = true;
            //        isValid = false;
            //    }
            //}


            //if (rsvlist.SelectedItems.Count == 0)
            //{
            //    rsverrorlbl.IsVisible = true;
            //    isValid = false;
            //}

            //if(rsvdatelbl.IsVisible)
            //{
            //    if (string.IsNullOrEmpty(rsvdateentry.Text))
            //    {
            //        dateEntryrsv.HasError = true;
            //        isValid = false;
            //    }
            //}

            //if (othevaclist.SelectedItems.Count == 0)
            //{
            //    othvacerrorlbl.IsVisible = true;
            //    isValid = false;
            //}

            //if(othervaclistlbl.IsVisible)
            //{
            //    if (othevaclistlist.SelectedItems.Count == 0)
            //    {
            //        othvaclisterrorlbl.IsVisible = true;
            //        isValid = false;
            //    }
            //}


            return isValid;
        }
        catch (Exception Ex)
        {
            return false;
        }
    }




    private bool ValidateDietStack()
    {
        try
        {
            bool isValid = true;

            // 1. Validate main Diet Selection
            if (dietlist.SelectedItems == null || dietlist.SelectedItems.Count == 0)
            {
                dieterrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                // dieterrorlbl.IsVisible = false;
            }

            // 2. Validate Diet Length (if visible)
            if (dietlengthlist.IsVisible)
            {
                if (dietlengthlist.SelectedItems == null || dietlengthlist.SelectedItems.Count == 0)
                {
                    dietlenghtlbl.IsVisible = true; // Note: using your XAML spelling 'dietlenghtlbl'
                    isValid = false;
                }
                else
                {
                    //dietlenghtlbl.IsVisible = false;
                }
            }

            // 3. Validate "Any Supplements" question
            if (anylist.SelectedItems == null || anylist.SelectedItems.Count == 0)
            {
                anysuppserrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                // anysuppserrorlbl.IsVisible = false;
            }

            // 4. Validate "Take" list (if visible)
            if (takelist.IsVisible)
            {
                if (takelist.SelectedItems == null || takelist.SelectedItems.Count == 0)
                {
                    takesuppserrorlbl.IsVisible = true;
                    isValid = false;
                }
                else
                {
                    // takesuppserrorlbl.IsVisible = false;
                }
            }

            // 5. Validate "Extra" list (if visible)
            if (extralist.IsVisible)
            {
                if (extralist.SelectedItems == null || extralist.SelectedItems.Count == 0)
                {
                    whichsuppserrorlbl.IsVisible = true;
                    isValid = false;
                }
                else
                {
                    //whichsuppserrorlbl.IsVisible = false;
                }
            }

            return isValid;
        }
        catch (Exception Ex)
        {
            // Log exception if necessary
            return false;
        }
    }

    public bool ValidateMenstrualInfo()
    {
        bool isValid = true;

        // 1. Validate Main Menstrual List

        if (mensturallist.SelectedItem == null)
        {
            menstrualerrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            menstrualerrorlbl.IsVisible = false;
        }


        // 2. Validate Pregnancy Weeks (if visible)
        if (preghelper.IsVisible)
        {

            if (string.IsNullOrWhiteSpace(pregweeksentry?.Text))
            {
                preghelper.HasError = true;
                preghelper.ErrorText = "Enter Value";
                isValid = false;
            }
            else
            {
                if (!int.TryParse(pregweeksentry.Text, out int weeks) || weeks < 0 || weeks > 55)
                {
                    preghelper.HasError = true;
                    preghelper.ErrorText = "Enter Value between 0 and 55 weeks";
                    isValid = false;
                }
                else
                {
                    preghelper.HasError = false;
                }
            }
        }

        // 3. Validate Delivery Date (if visible)
        if (ddhelper.IsVisible)
        {

            if (string.IsNullOrWhiteSpace(pregdateentry?.Text) || pregdateentry.Text.Length < 10)
            {
                ddhelper.HasError = true;
                isValid = false;
            }
            else
            {
                ddhelper.HasError = false;
            }
        }

        return isValid;
    }

    public bool ValidateHtInfo()
    {
        bool isValid = true;

        // 1. Validate Mobility List
        if (moblist.SelectedItem == null)
        {
            ht1errorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            ht1errorlbl.IsVisible = false;
        }

        // 2. Validate Self Care List
        if (sclist.SelectedItem == null)
        {
            ht2errorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            ht2errorlbl.IsVisible = false;
        }

        // 3. Validate Usual Activities List
        if (uclist.SelectedItem == null)
        {
            ht3errorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            ht3errorlbl.IsVisible = false;
        }

        // 4. Validate Pain/Discomfort List
        if (painlist.SelectedItem == null)
        {
            ht4errorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            ht4errorlbl.IsVisible = false;
        }

        // 5. Validate Anxiety/Depression List
        // Checking if deplist exists (based on your population logic)
        if (deplist.SelectedItem == null)
        {
            ht5errorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            ht5errorlbl.IsVisible = false;
        }

        if (string.IsNullOrEmpty(slidernumlbl.Text))
        {
            ht6errorlbl.IsVisible = true;
        }

        return isValid;
    }

    public bool validateAQ()
    {
        bool isValid = true;

        // 1. Validate Mobility List
        if (addqlist.SelectedItem == null)
        {
            aqerrorlbl.IsVisible = true;
            isValid = false;
        }


        return isValid;
    }

    public bool ValidateAntiViralInfo()
    {
        bool isValid = true;

        // 1. Ever smoked list
        if (heardavlist.SelectedItem == null)
        {
            heardaverrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            heardaverrorlbl.IsVisible = false;
        }

        // 2. Types of tobacco (if visible)
        if (usedavlist.IsVisible) // Assuming helper name
        {
            if (usedavlist.SelectedItem == null)
            {
                usedaverrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                usedaverrorlbl.IsVisible = false;
            }
        }

        // 3. Start Age (if visible)
        if (futureavlist.IsVisible)
        {
            if (futureavlist.SelectedItem == null)
            {
                futureaverrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                futureaverrorlbl.IsVisible = false;
            }
        }

        // 4. Current smoker list (if visible)
        if (futureavlist2.IsVisible)
        {
            if (futureavlist2.SelectedItem == null)
            {
                futureaverrorlbl2.IsVisible = true;
                isValid = false;
            }
            else
            {
                futureaverrorlbl2.IsVisible = false;
            }
        }

        // 5. Stop Age (if visible)
        if (futureavlist3.IsVisible)
        {
            if (futureavlist3.SelectedItem == null)
            {
                futureaverrorlbl3.IsVisible = true;
                isValid = false;
            }
            else
            {
                futureaverrorlbl3.IsVisible = false;
            }
        }

        // 6. Frequency (if visible)
        if (futureavlist4.IsVisible)
        {
            if (futureavlist4.SelectedItem == null)
            {
                futureaverrorlbl4.IsVisible = true;
                isValid = false;
            }
            else
            {
                futureaverrorlbl4.IsVisible = false;
            }
        }

        return isValid;
    }

    public bool ValidateTobaccoInfo()
    {
        bool isValid = true;

        // 1. Ever smoked list
        if (smokelist.SelectedItem == null)
        {
            smokeerrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            smokeerrorlbl.IsVisible = false;
        }

        // 2. Types of tobacco (if visible)
        if (usesmokelist.IsVisible) // Assuming helper name
        {
            if (usesmokelist.SelectedItem == null)
            {
                usesmokeerrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                usesmokeerrorlbl.IsVisible = false;
            }
        }

        // 3. Start Age (if visible)
        if (ageofsmokelbl.IsVisible)
        {
            if (string.IsNullOrWhiteSpace(agesmokeentry?.Text))
            {
                agesmokehelper.HasError = true;
                isValid = false;
            }
            else
            {
                agesmokehelper.HasError = false;
            }
        }

        // 4. Current smoker list (if visible)
        if (usesmokelbl.IsVisible)
        {
            if (usesmokelistr.SelectedItem == null)
            {
                usingsmokeerrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                usingsmokeerrorlbl.IsVisible = false;
            }
        }

        // 5. Stop Age (if visible)
        if (stopsmokelbl.IsVisible)
        {
            if (string.IsNullOrWhiteSpace(stopsmokeentry?.Text))
            {
                stopsmokehelper.HasError = true;
                isValid = false;
            }
            else
            {
                stopsmokehelper.HasError = false;
            }
        }

        // 6. Frequency (if visible)
        if (smokefreqlbl.IsVisible)
        {
            if (smokefreqlistr.SelectedItem == null)
            {
                smokingerrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                smokingerrorlbl.IsVisible = false;
            }
        }

        return isValid;
    }

    public bool ValidateAlcoholInfo()
    {
        bool isValid = true;

        // 1. Validate Alcohol Frequency
        if (alochollist.SelectedItem == null)
        {
            alcoholerrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            alcoholerrorlbl.IsVisible = false;
        }

        // 2. Validate Alcohol Units (if visible)
        if (usealochollist.IsVisible)
        {
            if (usealochollist.SelectedItem == null)
            {
                usealcoholerrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                usealcoholerrorlbl.IsVisible = false;
            }
        }

        return isValid;
    }

    public bool ValidateDrugInfo()
    {
        bool isValid = true;

        // 1. Validate Drugs Ever Used
        if (druglist.SelectedItem == null)
        {
            drugserrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            drugserrorlbl.IsVisible = false;
        }

        // 2. Validate Which Drugs (if visible)
        if (whatdrugslist.IsVisible)
        {
            if (whatdrugslist.SelectedItem == null)
            {
                whatdrugserrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                whatdrugserrorlbl.IsVisible = false;
            }
        }

        // 3. Validate Drugs Frequency (if visible)
        if (drugoftenlist.IsVisible)
        {
            if (drugoftenlist.SelectedItem == null)
            {
                drugsoftenerrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                drugsoftenerrorlbl.IsVisible = false;
            }
        }

        // 4. Validate Breathing Impact (if visible)
        if (breathinglist.IsVisible)
        {
            if (breathinglist.SelectedItem == null)
            {
                breathingerrorlbl.IsVisible = true;
                isValid = false;
            }
            else
            {
                breathingerrorlbl.IsVisible = false;
            }
        }

        return isValid;
    }

    public bool ValidateSleepInfo()
    {
        bool isValid = true;


        if (sleeplist.SelectedItem == null)
        {
            sleeperrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            sleeperrorlbl.IsVisible = false;
        }


        if (wakelist.SelectedItem == null)
        {
            wakeerrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            wakeerrorlbl.IsVisible = false;
        }



        if (nightslist.SelectedItem == null)
        {
            nightserrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            nightserrorlbl.IsVisible = false;
        }


        if (qualitylist.SelectedItem == null)
        {
            qualityerrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            qualityerrorlbl.IsVisible = false;
        }

        if (moodlist.SelectedItem == null)
        {
            mooderrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            mooderrorlbl.IsVisible = false;
        }

        if (prodlist.SelectedItem == null)
        {
            proderrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            proderrorlbl.IsVisible = false;
        }

        if (poorsleeplist.SelectedItem == null)
        {
            poorsleeperrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            poorsleeperrorlbl.IsVisible = false;
        }

        if (sleepproblist.SelectedItem == null)
        {
            sleepproderrorlbl.IsVisible = true;
            isValid = false;
        }
        else
        {
            sleepproderrorlbl.IsVisible = false;
        }


        return isValid;
    }

    public bool CheckTermsandConditions()
    {
        try
        {
            bool isValid = true;

            foreach (var section in allconsentdetails.consentcontent)
            {
                foreach (var item in section.sectioncontent)
                {
                    item.ShowValidation = true;

                    if (item.required && !item.ChckedState)
                    {

                        // tcerrorlbl.IsVisible = true;
                        isValid = false;
                    }
                }
            }


            if (!tccheckbox.IsChecked)
            {
                tcpwborder.Stroke = Colors.Red;
                tcpwlabel.TextColor = Colors.Red;
                isValid = false;
            }

            if (under10stack.IsVisible)
            {
                if (string.IsNullOrEmpty(under10entry.Text))
                {
                    under10helper.HasError = true;
                    isValid = false;
                }

                if (under10rolelist.SelectedItems.Count == 0)
                {
                    under10roleerrorlbl.IsVisible = true;
                    isValid = false;
                }

                if (under10otherrolehelper.IsVisible)
                {
                    if (string.IsNullOrEmpty(under10otherroleentry.Text))
                    {
                        under10otherrolehelper.HasError = true;
                        isValid = false;
                    }
                }
            }


            if (string.IsNullOrEmpty(over16nameentry.Text))
            {
                over16namehelper.HasError = true;
                isValid = false;
            }

            if (!SignPadhaddata)
            {
                IOSSign.Stroke = Colors.Red;
                AndroidSign.Stroke = Colors.Red;
                signsublbl.TextColor = Colors.Red;
                isValid = false;
            }

            return isValid;
        }
        catch
        {
            return false;
        }
    }


    private static readonly Regex UkPostcodeRegex = new(
    @"^(GIR\s?0AA|(?:(?:[A-PR-UWYZ][0-9][0-9]?|[A-PR-UWYZ][A-HK-Y][0-9][0-9]?|[A-PR-UWYZ][0-9][A-HJKPSTUW]|[A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRV-Y]))\s?[0-9][ABD-HJLNP-UW-Z]{2})$",
    RegexOptions.IgnoreCase);
    private async Task<bool> ValidateaddressStack()
    {
        try
        {
            bool isValid = true;
            postcodehelper.HasError = false;
            string rawPostcode = postcodeentry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rawPostcode) || !UkPostcodeRegex.IsMatch(rawPostcode))
            {
                postcodehelper.HasError = true;
                postcodehelper.ErrorText = "Enter a valid UK postcode";
                isValid = false;

            }
            if (isValid && validpostcodelist != null && validpostcodelist.Count > 0)
            {
                var cleanPostcode = rawPostcode.ToUpper().Replace(" ", "");
                var outwardCode = cleanPostcode.Length > 3 ? cleanPostcode[..^3] : cleanPostcode;
                if (!validpostcodelist.Any(p => p.Equals(outwardCode, StringComparison.OrdinalIgnoreCase)))
                {
                    postcodehelper.HasError = true;
                    postcodehelper.ErrorText = "Sorry, this study is not available in your area";
                    isValid = false;
                }
            }
            if (isValid)
            {

                if (addressoneentry.Text.IsNullOrEmpty())
                {
                    addressonehelper.HasError = true;
                    addressonehelper.ErrorText = "Please add a house number";
                    return false;
                }


                if (postcodelist.SelectedItem is IdealAddress selected)
                {
                    var checkPostcode = await APICalls.Instance.Getuserspostcodes(rawPostcode);
                    if (checkPostcode != null && checkPostcode.Count > 0)
                    {
                        bool matchCase = checkPostcode.Any(x => x.DetailsList != null &&
                            x.DetailsList.Any(d => string.Equals(
                                d.addresslineone?.Trim(),
                                selected.line_1?.Trim(),
                                StringComparison.OrdinalIgnoreCase)));
                        if (matchCase)
                        {
                            postcodehelper.HasError = true;
                            postcodehelper.ErrorText = "An account with this address already exists";
                            isValid = false;
                        }
                    }
                }
                else
                {
                    postcodehelper.HasError = true;
                    postcodehelper.ErrorText = "Please select an address shown below";
                    isValid = false;
                    return false;
                }
            }
            //// Address Line 1
            //if (string.IsNullOrWhiteSpace(addressoneentry.Text) || addressoneentry.Text.Length < 3)
            //{
            //    addressonehelper.HasError = true;
            //    addressonehelper.ErrorText = "Enter a valid address line";
            //    isValid = false;
            //}
            //else
            //{
            //    addressonehelper.HasError = false;
            //}
            //// City
            //if (string.IsNullOrWhiteSpace(townentry.Text) || !Regex.IsMatch(townentry.Text, @"^[A-Za-z\s'-]{2,}$"))
            //{
            //    townhelper.HasError = true;
            //    townhelper.ErrorText = "Enter a valid town or city";
            //    isValid = false;
            //}
            //else
            //{
            //    townhelper.HasError = false;
            //}
            //// County
            //if (string.IsNullOrWhiteSpace(countyentry.Text))
            //{
            //    countyhelper.HasError = true;
            //    countyhelper.ErrorText = "Enter a valid county";
            //    isValid = false;
            //}
            //else
            //{
            //    countyhelper.HasError = false;
            //}
            return isValid;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "ValidateaddressStack");
            return false;
        }
    }


    private void firstnameentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            fnhelper.HasError = false;
        }
        catch (Exception Ex)
        {

        }
    }

    private void surnameentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            snhelper.HasError = false;
        }
        catch (Exception Ex)
        {

        }
    }

    private void othergenderentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            othergenderhelper.HasError = false;
        }
        catch (Exception Ex)
        {

        }
    }

    private void genderlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails;

            if (item.Text == "Other")
            {
                othergenderhelper.IsVisible = true;

            }
            else
            {
                othergenderhelper.IsVisible = false;
                userdetails.Gender = item.Text;
            }
            othergenderhelper.HasError = false;
            genderlisterror.IsVisible = false;
            sexiderror.IsVisible = false;

        }
        catch (Exception Ex)
        {

        }
    }

    private void BackButtonTapped_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

            // If we�re at the very beginning, go back to welcome
            if (currentFieldIndex <= 0)
            {

                if (nextbtn.Text == "Get Started")
                {
                    Navigation.RemovePage(this);
                    return;
                }

                welcomestack.IsVisible = true;
                registerstack.IsVisible = false;
                topprogress.IsVisible = false;

                nextbtn.Text = "Get Started";
                updatebackprogress();

                return;
            }

            if (topprogress2.IsVisible)
            {


                // Hide current field
                if (currentFieldIndexquestionnaire < Allquesfields.Count)
                {
                    var currentField = Allquesfields[currentFieldIndexquestionnaire];
                    SetStackVisibility(currentField.XamlNameArea, false);
                }


                if (currentFieldIndexquestionnaire == 0)
                {
                    topprogress2.IsVisible = false;
                    topprogress2.Progress -= progressamountquestionnaire;
                    currentFieldIndex--;
                }

                // Move back
                currentFieldIndexquestionnaire--;

                // Clamp index (important)
                if (currentFieldIndexquestionnaire < 0)
                    currentFieldIndexquestionnaire = 0;



                // Show previous field
                //  ShowCurrentStack();
                //  updateprogress();



            }
            else
            {


                // Hide the current stack
                var currentField = Allregfields[currentFieldIndex];
                SetStackVisibility(currentField.XamlNameArea, false);
                // Move one step back
                currentFieldIndex--;
            }

            // Show the previous stack
            ShowCurrentStack();

            // Update progress
            updatebackprogress();

            // Update button text
            nextbtn.Text = "Next";




        }
        catch (Exception Ex)
        {

        }
    }

    private void emailentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            emailhelper.HasError = false;
        }
        catch (Exception Ex)
        {

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

    private void UpdateOpacity(Image tickImage, Label textLabel, bool isValid)
    {
        tickImage.Opacity = isValid ? 1.0 : 0.2;
        textLabel.Opacity = isValid ? 1.0 : 0.2;

    }

    private void firstpasswordentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {

            string password = e.NewTextValue;

            passhelper.HasError = false;

            // Check each requirement and update the UI
            bool hasMinChars = password.Length >= 8;
            bool hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+=\[{\]};:<>|./?-]");
            bool hasCapitalLetter = Regex.IsMatch(password, @"[A-Z]");
            bool hasNumber = Regex.IsMatch(password, @"[0-9]");

            // Update UI
            UpdateOpacity(chartick, charlbl, hasMinChars);
            UpdateOpacity(specialtick, speciallbl, hasSpecialChar);
            UpdateOpacity(capitaltick, capitallbl, hasCapitalLetter);
            UpdateOpacity(numtick, numlbl, hasNumber);

            //change the text back to orginal colour not error colour
            charlbl.TextColor = Color.FromArgb("#031926");
            speciallbl.TextColor = Color.FromArgb("#031926");
            capitallbl.TextColor = Color.FromArgb("#031926");
            numlbl.TextColor = Color.FromArgb("#031926");
        }
        catch (Exception Ex)
        {
            //Leave Empty
        }
    }

    private void confirmpassentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            confirmpasshelper.HasError = false;
        }
        catch (Exception Ex)
        {

        }
    }

    private void dateEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            dobhelper.HasError = false;

            if (DateTime.TryParseExact(
           dateEntry.Text,
           "dd/MM/yyyy",
           System.Globalization.CultureInfo.InvariantCulture,
           System.Globalization.DateTimeStyles.None,
           out DateTime dob))
            {
                int age = DateTime.Today.Year - dob.Year;
                if (DateTime.Today < dob.AddYears(age))
                    age--;

                // Show the label if over 13
                gendermatchlist.IsVisible = age > 13;
                sexmatchlbl.IsVisible = age > 13;
                if (age > 13)
                {
                    gendermatchlist.RefreshView();
                }
                //  genidlbl.IsVisible = age > 13;
                //infogenidlbl.IsVisible = age > 13;

                //genidlist.IsVisible = age > 13;
                //sexiderror.IsVisible = age > 13;
            }
            else
            {
                genidlist.SelectedItem = null;
                gendermatchlist.SelectedItem = null;
                gendermatchlist.IsVisible = false;
                sexmatchlbl.IsVisible = false;
                genidlbl.IsVisible = false;
                infogenidlbl.IsVisible = false;
                genidlist.IsVisible = false;
                sexiderror.IsVisible = false;


                // hide if invalid
            }

            //#if ANDROID
            //            var handler = dateEntry.Handler as Microsoft.Maui.Handlers.EntryHandler;
            //            var editText = handler?.PlatformView as AndroidX.AppCompat.Widget.AppCompatEditText;
            //            if (editText != null)
            //            {
            //                editText.EmojiCompatEnabled = false;
            //                editText.SetTextKeepState(dateEntry.Text);
            //            }
            //#endif

            //            if (isEditing)
            //                return;

            //            isEditing = true;

            //            string input = e.NewTextValue;

            //            // Remove any non-numeric characters except '/'
            //            input = new string(input.Where(c => char.IsDigit(c) || c == '/').ToArray());

            //            // Remove existing slashes to reformat correctly
            //            input = input.Replace("/", string.Empty);

            //            // Limit the input to a maximum of 8 numeric characters (DDMMYYYY)
            //            if (input.Length >= 8)
            //            {
            //                input = input.Substring(0, 8);
            //                dateEntry.IsEnabled = false;
            //                dateEntry.IsEnabled = true;
            //            }

            //            // Insert slashes at the appropriate positions
            //            if (input.Length > 2)
            //                input = input.Insert(2, "/");

            //            if (input.Length > 5)
            //                input = input.Insert(5, "/");

            //            // Check for valid date parts and set the text color accordingly
            //            if (input.Length == 10)
            //            {
            //                if (DateTime.TryParseExact(input, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            //                {
            //                    // Check if the date is between 1900 and today's year
            //                    int currentYear = DateTime.Now.Year;
            //                    if (date.Year >= 1900 && date.Year <= currentYear)
            //                    {
            //                        if (date.Date <= DateTime.Now.Date)
            //                        {
            //                            dateEntry.TextColor = Color.FromArgb("#031926"); // Valid date
            //                            validdob = true;

            //                            int age = DateTime.Now.Year - date.Year;
            //                            if (DateTime.Now.Date < date.AddYears(age)) // birthday not yet reached this year
            //                                age--;

            //                            // Show or hide matchgenderlist based on age
            //                            gendermatchlist.IsVisible = age > 13;
            //                            sexmatchlbl.IsVisible = age > 13;
            //                          //  infosexmatchlbl.IsVisible = age > 18;
            //                        }
            //                        else
            //                        {
            //                            dateEntry.TextColor = Colors.Red; // Invalid date range
            //                            validdob = false;
            //                        }

            //                    }
            //                    else
            //                    {
            //                        dateEntry.TextColor = Colors.Red; // Invalid date range
            //                        validdob = false;
            //                    }
            //                }
            //                else
            //                {
            //                    dateEntry.TextColor = Colors.Red; // Invalid date
            //                    validdob = false;
            //                }
            //            }
            //            else
            //            {
            //                dateEntry.TextColor = Color.FromArgb("#031926"); // Intermediate input
            //                validdob = false;
            //            }

            //            dateEntry.Text = input;

            //            // Adjust cursor position
            //            dateEntry.CursorPosition = input.Length;

            //            isEditing = false;
        }
        catch (Exception Ex)
        {

        }
    }

    private async Task<bool> IsValidNhsNumber(string nhsNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nhsNumber))
                return false;

            if (nhsNumber.Length != 10 || !nhsNumber.All(char.IsDigit))
                return false;

            int[] weights = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                sum += (nhsNumber[i] - '0') * weights[i];
            }

            int remainder = sum % 11;
            int checkDigit = 11 - remainder;

            if (checkDigit == 11) checkDigit = 0;
            if (checkDigit == 10) return false;

            return checkDigit == (nhsNumber[9] - '0');
        }
        catch (Exception Ex)
        {
            return false;
        }
    }

    private async void nhsentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry)
            return;
        try
        {
            var newText = e.NewTextValue ?? string.Empty;

            // Strip out the mask spaces to check the raw digit count
            string digitsOnly = new string(newText.Where(char.IsDigit).ToArray());

            // NHS numbers must be exactly 10 digits
            if (digitsOnly.Length == 10)
            {
                bool isValid = await IsValidNhsNumber(digitsOnly);
                validnhsnum = isValid;
                nhshelper.HasError = !isValid;
                nhshelper.ErrorText = isValid ? "" : "Please enter a valid NHS number";
            }
            else
            {
                nhshelper.HasError = false;
                validnhsnum = false;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "nhsentry_TextChanged");
        }
    }

    //    private async void nhsentry_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        if (NHSisEditing || sender is not Entry entry)
    //            return;

    //        var oldText = e.OldTextValue ?? string.Empty;
    //        var newText = e.NewTextValue ?? string.Empty;

    //        if (newText.Length < oldText.Length && oldText.Trim() == newText) return;

    //        try
    //        {
    //            NHSisEditing = true;

    //            string digitsOnly = new string(newText.Where(char.IsDigit).Take(10).ToArray());
    //            string formatted = FormatNhsNumber(digitsOnly);
    //            if (entry.Text != formatted)
    //            {
    //#if ANDROID
    //                var handler = entry.Handler as Microsoft.Maui.Handlers.EntryHandler;
    //                var editText = handler?.PlatformView as AndroidX.AppCompat.Widget.AppCompatEditText;

    //                if (editText != null)
    //                {
    //                    editText.EmojiCompatEnabled = false;
    //                    //editText.SetTextKeepState(formatted);
    //                }
    //                else
    //                {
    //                    entry.Text = formatted;
    //                }
    //#else
    //            entry.Text = formatted;
    //#endif
    //            }

    //            if (digitsOnly.Length == 10)
    //            {
    //                bool isValid = await IsValidNhsNumber(digitsOnly);
    //                validnhsnum = isValid;
    //                nhshelper.HasError = !isValid;
    //                nhshelper.ErrorText = isValid ? "" : "Please enter a valid NHS number";
    //            }
    //            else
    //            {
    //                nhshelper.HasError = false;
    //                validnhsnum = false;
    //            }
    //        }
    //        catch (Exception Ex)
    //        {
    //            CrashDetected.LogCrash(Ex, Navigation, "nhsentry_TextChanged");
    //        }
    //        finally
    //        {
    //            NHSisEditing = false;
    //        }
    //    }

    private static string FormatNhsNumber(string digits)
    {
        if (string.IsNullOrEmpty(digits))
            return string.Empty;

        if (digits.Length > 6)
        {
            return $"{digits[..3]} {digits.Substring(3, 3)} {digits[6..]}";
        }

        if (digits.Length > 3)
        {
            return $"{digits[..3]} {digits[3..]}";
        }

        return digits;
    }



    private void addressoneentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            addressonehelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "addressoneentry_TextChanged");
        }
    }

    private void townentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            townhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "townentry_TextChanged");
        }
    }

    // Add these two fields near your other private fields (e.g. near PCisEditing)
    private CancellationTokenSource _postcodeLookupCts;
    private static readonly HttpClient _postcodeClient = new HttpClient();

    private void postcodeentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {

            if (ClearAddressbtn.IsVisible) ClearAddressbtn.IsVisible = false;

            var entry = sender as Entry;
            if (entry == null || e.NewTextValue == null) return;
#if ANDROID
            var handler = postcodeentry.Handler as Microsoft.Maui.Handlers.EntryHandler;
            var editText = handler?.PlatformView as AndroidX.AppCompat.Widget.AppCompatEditText;
            if (editText != null)
            {
                editText.EmojiCompatEnabled = false;
                editText.SetTextKeepState(entry.Text);
            }
#endif
            if (PCisEditing) return;
            PCisEditing = true;

            PostcodeNoResults.IsVisible = false;
            string input = new string(e.NewTextValue.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
            if (input.Length > 7)
                input = input.Substring(0, 7);
            if (input.Length >= 5)
                input = input.Insert(input.Length - 3, " ");

            postcodehelper.HasError = false;
            postcodeentry.Text = input;
            postcodeentry.CursorPosition = input.Length;
            PCisEditing = false;

            _ = HandlePostcodeLookupAsync(input);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "postcodeentry_TextChanged");
            PCisEditing = false;
        }
    }

    private async Task HandlePostcodeLookupAsync(string formattedPostcode)
    {
        try
        {
            _postcodeLookupCts?.Cancel();
            var cts = new CancellationTokenSource();
            _postcodeLookupCts = cts;

            if (UkPostcodeRegex.IsMatch(formattedPostcode))
            {
                var address = await LookupPostcode(formattedPostcode, cts.Token);

                // A newer keystroke superseded this call, or the field moved on — ignore stale results
                if (cts.IsCancellationRequested || postcodeentry.Text != formattedPostcode) return;

                if (address == null || address.Count == 0)
                {
                    PostcodeNoResults.IsVisible = true;
                    postcodelist.IsVisible = false;
                }
                else
                {
                    postcodelist.ItemsSource = address;
                    postcodelist.IsVisible = true;
                    postcodelist.RefreshView();
                }
            }
            else
            {
                if (formattedPostcode.Length >= 7)
                    PostcodeNoResults.IsVisible = true;

                postcodelist.IsVisible = false;
                addressonehelper.IsVisible = false;
                townhelper.IsVisible = false;
                countyhelper.IsVisible = false;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "HandlePostcodeLookupAsync");
        }
    }

    public async Task<List<IdealAddress>> LookupPostcode(string postcode, CancellationToken cancellationToken = default)
    {
        try
        {
            string apiKey = "ak_mnh4f02ypPRnIXhiTBlDzYkMDFMU5"; // TODO: move this behind your own API, don't ship it client-side
            string encodedPostcode = Uri.EscapeDataString(postcode);
            string url = $"https://api.ideal-postcodes.co.uk/v1/postcodes/{encodedPostcode}?api_key={apiKey}";

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var response = await _postcodeClient.GetAsync(url, linkedCts.Token);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<IdealResponse>(json);
                return data?.result ?? new List<IdealAddress>();
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Postcode lookup failed: {(int)response.StatusCode} {response.ReasonPhrase} | {errorBody}");
            return new List<IdealAddress>();
        }
        catch (OperationCanceledException)
        {
            // Expected when a newer keystroke supersedes this call, or the timeout fires — not a real error
            return null;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "LookupPostcode");
            return null;
        }
    }

    //    private async void postcodeentry_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        try
    //        {

    //            var entry = sender as Entry;
    //            if (entry == null || e.NewTextValue == null) return;

    //#if ANDROID
    //            var handler = postcodeentry.Handler as Microsoft.Maui.Handlers.EntryHandler;
    //            var editText = handler?.PlatformView as AndroidX.AppCompat.Widget.AppCompatEditText;
    //            if (editText != null)
    //            {
    //                editText.EmojiCompatEnabled = false;
    //                editText.SetTextKeepState(entry.Text);
    //            }
    //#endif
    //            if (PCisEditing) return;
    //            PCisEditing = true;
    //            PostcodeNoResults.IsVisible = false;
    //            string input = new string(e.NewTextValue.Where(char.IsLetterOrDigit).ToArray()).ToUpper();

    //            if (input.Length > 7)
    //                input = input.Substring(0, 7);

    //            if (input.Length > 5)
    //            {
    //                input = input.Insert(input.Length - 3, " ");
    //            }

    //            postcodehelper.HasError = false;

    //            postcodeentry.Text = input;
    //            postcodeentry.CursorPosition = input.Length;

    //            if (UkPostcodeRegex.IsMatch(postcodeentry.Text))
    //            {
    //                //TODO: add back in 
    //                //valid postocde lookup 
    //                var address = await LookupPostcode(postcodeentry.Text);


    //                if (address == null)
    //                {
    //                    PostcodeNoResults.IsVisible = true;
    //                }
    //                else
    //                {
    //                    postcodelist.ItemsSource = address;
    //                    postcodelist.IsVisible = true;
    //                    postcodelist.RefreshView();
    //                }
    //            }
    //            else
    //            {
    //                //TODO: add in error or no address label
    //                if(input.Length > 4)
    //                {
    //                    PostcodeNoResults.IsVisible = true;
    //                }
    //                postcodelist.IsVisible = false;
    //                addressonehelper.IsVisible = false;
    //                townhelper.IsVisible = false;
    //                countyhelper.IsVisible = false;
    //            }

    //            PCisEditing = false;
    //        }
    //        catch (Exception Ex)
    //        {
    //            CrashDetected.LogCrash(Ex, Navigation, "postcodeentry_TextChanged");
    //            PCisEditing = false;
    //        }
    //    }

    //    public async Task<List<IdealAddress>> LookupPostcode(string postcode)
    //    {
    //        try
    //        {
    //            string apiKey = "ak_mnh4f02ypPRnIXhiTBlDzYkMDFMU5";
    //            string url = $"https://api.ideal-postcodes.co.uk/v1/postcodes/{postcode}?api_key={apiKey}";

    //            using (HttpClient client = new HttpClient())
    //            {
    //                var response = await client.GetAsync(url);
    //                if (response.IsSuccessStatusCode)
    //                {
    //                    var json = await response.Content.ReadAsStringAsync();
    //                    var data = JsonConvert.DeserializeObject<IdealResponse>(json);
    //                    return data.result;
    //                }
    //            }
    //            return new List<IdealAddress>();
    //        }
    //        catch(Exception Ex)
    //        {
    //            CrashDetected.LogCrash(Ex, Navigation, "LookupPostcode");
    //            return null;
    //        }
    //    }

    private void usinglist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails;

            usingerrorlbl.IsVisible = false;

            if (item.Text == "Someone else")
            {
                relationlbl.IsVisible = true;
                relationlist.IsVisible = true;

                studyreplist.IsVisible = false;
                studylbl.IsVisible = false;
                infostudylbl.IsVisible = false;
                studyreplist.SelectedItem = null;
            }
            else
            {
                relationlist.SelectedItem = null;
                relationlbl.IsVisible = false;
                relationlist.IsVisible = false;


                studyreplist.IsVisible = true;
                studylbl.IsVisible = true;
                infostudylbl.IsVisible = true;


            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "usinglist_ItemTapped");
        }
    }

    private void relationlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails;

            relationerrorlbl.IsVisible = false;



            if (item.Text.Contains("Other"))
            {
                otherrelationhelper.IsVisible = true;
            }
            else
            {
                otherrelationhelper.IsVisible = false;
            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "relationlist_ItemTapped");
        }
    }

    private void studyreplist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails;

            studyerrorlbl.IsVisible = false;


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "studyreplist_ItemTapped");
        }
    }

    private void uklist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails; ;

            ukerror.IsVisible = false;
            countyerror.IsVisible = false;

            if (item.Text == "No")
            {
                movelbl.IsVisible = true;
                movelist.IsVisible = true;

                countrylbl.IsVisible = true;
                autocompletecounty.IsVisible = true;

                movelist.RefreshView();
            }
            else
            {
                movelist.SelectedItem = null;
                movelbl.IsVisible = false;
                movelist.IsVisible = false;
                moveerror.IsVisible = false;

                countrylbl.IsVisible = false;
                autocompletecounty.IsVisible = false;
                autocompletecounty.SelectedItem = null;

            }

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "uklist_ItemTapped");
        }
    }

    private async void autocompletecounty_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        try
        {
            await Task.Delay(100);
            autocompletecounty.Unfocus();
            countyerror.IsVisible = false;

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "autocompletecounty_SelectionChanged");
        }
    }

    private void weightinputlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails; ;


            if (item.Text.Contains("kg"))
            {
                weighthelper.IsVisible = true;
                weightunitendlbl.Text = "kg";
            }
            else if (item.Text.Contains("st"))
            {
                weighthelper.IsVisible = true;
                weightunitendlbl.Text = "st";
            }
            else
            {
                weighthelper.IsVisible = false;
            }
            weighthelper.HasError = false;
            weighterror.IsVisible = false;

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "weightinputlist_ItemTapped");
        }
    }

    private void heightinputlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails; ;


            if (item.Text.Contains("cm"))
            {
                heightHelper.IsVisible = false;
                heightcmhelper.IsVisible = true;
                // weightunitendlbl.Text = "kg";
            }
            else if (item.Text.Contains("Feet"))
            {
                heightHelper.IsVisible = true;
                heightcmhelper.IsVisible = false;
                // weightunitendlbl.Text = "st";
            }
            else
            {
                heightHelper.IsVisible = false;
                heightcmhelper.IsVisible = false;
            }
            heightHelper.HasError = false;
            heightcmhelper.HasError = false;
            heighterror.IsVisible = false;

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "heightinputlist_ItemTapped");
        }
    }

    private void currentsituationlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails; ;


            if (item.Text.Contains("Full-time employed")
        || item.Text.Contains("Part-time employed")
        || item.Text.Contains("Doing unpaid or voluntary work")
        || item.Text.Contains("Homemaker"))
            {
                typeworklbl.IsVisible = true;
                typeworklist.IsVisible = true;

                typeworklist.RefreshView();
            }
            else
            {
                typeworklist.IsVisible = false;
                typeworklbl.IsVisible = false;
                typeworklist.SelectedItem = null;
                workerror.IsVisible = false;
            }

            sitiuationerror.IsVisible = false;

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "currentsituationlist_ItemTapped");
        }
    }

    private async void gpautocomplete_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        try
        {

            gpaddresserrorlbl.IsVisible = false;
            var Item = e.CurrentSelection?.FirstOrDefault() as OptionDetails;
            if (Item != null)
            {
                SelectedGp = Item;
                GPPracticeName.Text = Item.Text;
                GpSelectedView.IsVisible = true;
                gpautocomplete.IsVisible = false;
                await Task.Delay(100);
                gpautocomplete.Unfocus();
                gpautocomplete.IsEnabled = false;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "gpautocomplete_SelectionChanged");
            gpautocomplete.IsEnabled = true;
        }
    }

    private void gplist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            gperrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;

            if (item.Text.Contains("Yes"))
            {
                gpinfolbl.IsVisible = true;
                gpautocomplete.IsVisible = true;
                gpsublbl.IsVisible = true;

                // infogbpostcodelbl.IsVisible = true;
            }
            else
            {
                gpinfolbl.IsVisible = false;
                gpautocomplete.IsVisible = false;
                gpaddresserrorlbl.IsVisible = false;
                gpsublbl.IsVisible = false;
                // infogbpostcodelbl.IsVisible = false;

            }



        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "gplist_ItemTapped");
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

            if (sender is Label tappedLabel)
            {
                string text = tappedLabel.Text;


                await MopupService.Instance.PushAsync(new Infopopup(text, Allregfields) { });

            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped");
        }
    }

    private async void disautocomplete_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        try
        {

            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as OptionDetails;

                // Add only if not already in the list
                if (!SelectedConditons.Any(x => x.Text == item.Text))
                {
                    SelectedConditons.Add(item);
                    hcadderrorlbl.IsVisible = false;
                }
                //  conditionslist.ItemsSource = SelectedConditons;


            }

            conditionschips.ItemsSource = SelectedConditons;



            // Check for cancer
            bool hasCancer = SelectedConditons
       .Any(x => x.Value != null && x.Value.Contains("Cancer", StringComparison.OrdinalIgnoreCase));

            cancerlbl.IsVisible = hasCancer;
            cancerlist.IsVisible = hasCancer;
            cancernowlbl.IsVisible = hasCancer;
            cancernowlist.IsVisible = hasCancer;
            if (hasCancer)
            {
                cancerlist.RefreshView();
                cancernowlist.RefreshView();
            }

            await Task.Delay(100);

            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            disautocomplete.Clear();
            disautocomplete.Unfocus();
            // clears text

            //});


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "disautocomplete_SelectionChanged");
        }

    }

    private async void medautocomplete_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        try
        {

            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as OptionDetails;

                // Add only if not already in the list
                if (!SelectedMedications.Any(x => x.Text == item.Text))
                {
                    SelectedMedications.Add(item);
                    medchipadderrorlbl.IsVisible = false;
                }

                //  conditionslist.ItemsSource = SelectedConditons;
            }

            medicationschips.ItemsSource = SelectedMedications;


            await Task.Delay(100);

            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            medautocomplete.Clear();
            medautocomplete.Unfocus();
            // clears text

            //});


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "medautocomplete_SelectionChanged");
        }
    }


    private void healthSlider_ValueChanged(object sender, Syncfusion.Maui.Sliders.SliderValueChangedEventArgs e)
    {
        try
        {

            if (e == null) return;

            var value = Math.Round(e.NewValue);

            if (ht6errorlbl != null)
                ht6errorlbl.IsVisible = false;

            if (slidernumlbl != null)
                slidernumlbl.Text = value.ToString();

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "healthSlider_ValueChanged");
        }
    }

    private void gendermatchlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails;
            //   othergenderhelper.HasError = false;


            if (item.Text == "No")
            {
                genidlbl.IsVisible = true;
                infogenidlbl.IsVisible = true;
                genidlist.IsVisible = true;

            }
            else
            {
                genidlbl.IsVisible = false;
                infogenidlbl.IsVisible = false;
                genidlist.IsVisible = false;
            }

            sexiderror.IsVisible = false;
            gendermatchlisterror.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "gendermatchlist_ItemTapped");
        }
    }

    private void movelist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as OptionDetails;
            moveerror.IsVisible = false;

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "movelist_ItemTapped");
        }
    }

    private void ethnicitylist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as OptionDetails;

            etherror.IsVisible = false;


            if (item != null)
            {
                userdetails.Ethnicity = item.Text;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "ethnicitylist_ItemTapped");
        }
    }

    private void countyentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            countyhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "countyentry_TextChanged");
        }
    }

    private void genidlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            sexiderror.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "genidlist_ItemTapped");
        }
    }

    //private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    //{
    //    try
    //    {
    //        await Launcher.OpenAsync("https://www.nhs.uk/service-search/find-a-gp");
    //    }
    //    catch(Exception Ex)
    //    {

    //    }
    //}

    private void hoslist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            hospitalerrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;

            if (item.Text.Contains("Yes"))
            {
                infectionlbl.IsVisible = true;
                infectionsub.IsVisible = true;
                infectionhelper.IsVisible = true;
                venlbl.IsVisible = true;
                venlist.IsVisible = true;
                venlist.RefreshView();
            }
            else
            {
                infectionlbl.IsVisible = false;
                infectionsub.IsVisible = false;
                infectionhelper.IsVisible = false;
                venlbl.IsVisible = false;
                venlist.IsVisible = false;
                venhoserrorlbl.IsVisible = false;
                infectionhelper.HasError = false;

            }



        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "hoslist_ItemTapped");
        }
    }

    private void OnRemoveChipTapped(object sender, EventArgs e)
    {
        try
        {
            if (sender is Image image && image.BindingContext is OptionDetails item)
            {
                SelectedConditons.Remove(item);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "OnRemoveChipTapped");
        }
    }

    private void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        try
        {
            if (sender is Image image && image.BindingContext is OptionDetails item)
            {
                SelectedMedications.Remove(item);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_2");
        }
    }


    private void covidlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails;

            // usingerrorlbl.IsVisible = false;

            if (item.Text == "Yes")
            {
                coviddatelbl.IsVisible = true;
                coviddatelbldirections.IsVisible = true;
                dateEntryCovidJab.IsVisible = true;



            }
            else
            {

                coviddatelbl.IsVisible = false;
                coviddatelbldirections.IsVisible = false;
                dateEntryCovidJab.IsVisible = false;



            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "covidlist_ItemTapped");
        }
    }

    private void rsvlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails;

            // usingerrorlbl.IsVisible = false;

            if (item.Text == "Yes")
            {
                rsvdatelbl.IsVisible = true;
                rsvdatedirections.IsVisible = true;
                dateEntryrsv.IsVisible = true;



            }
            else
            {

                rsvdatelbl.IsVisible = false;
                rsvdatedirections.IsVisible = false;
                dateEntryrsv.IsVisible = false;



            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "rsvlist_ItemTapped");
        }
    }

    private void othevaclist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            var item = e.DataItem as OptionDetails;

            // usingerrorlbl.IsVisible = false;

            if (item.Text == "Yes")
            {
                othervaclistlbl.IsVisible = true;
                othvaclistdirections.IsVisible = true;
                othevaclistlist.IsVisible = true;



            }
            else
            {

                othervaclistlbl.IsVisible = false;
                othvaclistdirections.IsVisible = false;
                othevaclistlist.IsVisible = false;



            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "othevaclist_ItemTapped");
        }
    }

    private void dietlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            dieterrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;
            if (item == null) return;

            // Get the index from the ListView's data source
            var listView = sender as Syncfusion.Maui.ListView.SfListView;
            int index = listView.DataSource.DisplayItems.IndexOf(item);

            // usingerrorlbl.IsVisible = false;

            if (index == 1 || index == 2 || index == 3)
            {
                dietlengthlbl.IsVisible = true;
                dietlengthlist.IsVisible = true;

                dietlengthlist.RefreshView();


            }
            else
            {

                dietlengthlbl.IsVisible = false;
                dietlengthlist.IsVisible = false;

                dietlenghtlbl.IsVisible = false;


            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "dietlist_ItemTapped");
        }
    }

    private void anylist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            anysuppserrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;

            // usingerrorlbl.IsVisible = false;

            if (item.Text == "Yes")
            {
                takelbl.IsVisible = true;
                takelbldirections.IsVisible = true;
                takelist.IsVisible = true;
                extralbl.IsVisible = true;
                extralbldirections.IsVisible = true;
                extralist.IsVisible = true;

                takelist.RefreshView();
                extralist.RefreshView();


            }
            else
            {

                takelbl.IsVisible = false;
                takelbldirections.IsVisible = false;
                takelist.IsVisible = false;
                extralbl.IsVisible = false;
                extralbldirections.IsVisible = false;
                extralist.IsVisible = false;

                takesuppserrorlbl.IsVisible = false;
                whichsuppserrorlbl.IsVisible = false;

            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "anylist_ItemTapped");
        }
    }

    private void mensturallist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            menstrualerrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;
            if (item == null) return;

            // Get the index from the ListView's data source
            var listView = sender as Syncfusion.Maui.ListView.SfListView;
            int index = listView.DataSource.DisplayItems.IndexOf(item);

            // usingerrorlbl.IsVisible = false;

            if (index == 4)
            {
                preglbl.IsVisible = true;
                preghelper.IsVisible = true;

                ddlbl.IsVisible = false;
                ddhelper.IsVisible = false;

            }
            else if (index == 5)
            {

                preglbl.IsVisible = false;
                preghelper.IsVisible = false;

                ddlbl.IsVisible = true;
                ddhelper.IsVisible = true;

            }
            else
            {

                preglbl.IsVisible = false;
                preghelper.IsVisible = false;

                ddlbl.IsVisible = false;
                ddhelper.IsVisible = false;
            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "mensturallist_ItemTapped");
        }
    }

    private void otherrelationentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            otherrelationhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "otherrelationentry_TextChanged");
        }
    }

    private void firstfamentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            firstfamhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "firstfamentry_TextChanged");
        }
    }

    private void firstsurnameemail_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            firstsurnamehelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "firstsurnameemail_TextChanged");
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

    private void familymember1_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {

        try
        {
            agemember1error.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "familymember1_ItemTapped");
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

    private void firstfamentry2_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            firstfamhelper2.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "firstfamentry2_TextChanged");
        }
    }

    private void firstsurnameentry2_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            firstsurnamehelper2.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "firstsurnameentry2_TextChanged");
        }
    }

    private void firstemailentry2_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            firstemailhelper2.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "firstemailentry2_TextChanged");
        }
    }

    private void familymember12_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            agemember1error2.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "familymember12_ItemTapped");
        }
    }

    private void familymember1list2_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            typemember1error2.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "familymember1list2_ItemTapped");
        }
    }

    private void postcodelist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var location = e.DataItem as IdealAddress;
            addressoneentry.Text = location.line_1;
            townentry.Text = !string.IsNullOrEmpty(location.line_2) ? location.line_2 : location.post_town;
            countyentry.Text = location.County;
            addressonehelper.IsVisible = true;
            townhelper.IsVisible = true;
            countyhelper.IsVisible = true;
            postcodelist.IsVisible = false;
            postcodelist.SelectedItem = null;
            ClearAddressbtn.IsVisible = true;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "postcodelist_ItemTapped");
        }
    }

    private void inchesEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            heightHelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "inchesEntry_TextChanged");
        }
    }

    private void heightcmentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            heightcmhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "heightcmentry_TextChanged");
        }
    }

    private void stepslist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            stepserror.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "stepslist_ItemTapped");
        }
    }

    private void gymlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            gymerror.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "gymlist_ItemTapped");
        }
    }

    private void higheducationlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            educationerror.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "higheducationlist_ItemTapped");
        }
    }


    private void typeworklist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            workerror.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "typeworklist_ItemTapped");
        }
    }

    private void weightEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            weighthelper.HasError = false;
        }
        catch (Exception Ex)
        {
        }
    }

    private void TapGestureRecognizer_Tapped_3(object sender, TappedEventArgs e)
    {
        try
        {
            // tcerrorlbl.IsVisible = false;
            //consent gird tapped
            var layout = (BindableObject)sender;
            var item = (ConsentItem)layout.BindingContext;

            if (item != null)
            {
                item.ChckedState = !item.ChckedState;
            }

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped_3");
        }
    }

    //private void TapGestureRecognizer_Tapped_3(object sender, EventArgs e)
    //{
    //    // Toggles the standalone checkbox
    //    tccheckbox.IsChecked = !tccheckbox.IsChecked;
    //}

    private void EmailBorder_Tapped(object sender, EventArgs e)
    {
        emailcheck.IsChecked = !emailcheck.IsChecked;
    }

    private void TapGestureRecognizer_Tapped_4(object sender, TappedEventArgs e)
    {
        tccheckbox.IsChecked = !tccheckbox.IsChecked;

        tcpwborder.Stroke = Color.FromArgb("#D1E8FF");
        tcpwlabel.TextColor = Color.FromArgb("#009fe3");
    }

    private async void drawingpad_DrawingLineCompleted(object sender, CommunityToolkit.Maui.Core.DrawingLineCompletedEventArgs e)
    {
        try
        {
            // Check if lines exist directly on the control first
            if (drawingpad.Lines == null || drawingpad.Lines.Count == 0)
            {
                SignPadhaddata = false;
                return;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var drawingStream = await drawingpad.GetImageStream(150, 150, cts.Token);

            bool isSignatureBlank = drawingStream == null;

            if (!isSignatureBlank)
            {
                // Copy to MemoryStream to safely check length if stream is unseekable
                using var ms = new MemoryStream();
                await drawingStream.CopyToAsync(ms);
                isSignatureBlank = ms.Length == 0;
            }

            if (!isSignatureBlank)
            {
                SignPadhaddata = true;
                IOSSign.Stroke = Color.FromArgb("#BFDBF7");
                signsublbl.TextColor = Color.FromArgb("#031926");
            }
            else
            {
                SignPadhaddata = false;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "drawingpad_DrawingLineCompleted");
        }
    }

    private async void signpad_DrawCompleted(object sender, EventArgs e)
    {
        try
        {
            using var signatureStream = await signpad.GetStreamAsync(Syncfusion.Maui.Core.ImageFileFormat.Png);

            bool isSignatureBlank = signatureStream == null;

            if (!isSignatureBlank)
            {
                using var ms = new MemoryStream();
                await signatureStream.CopyToAsync(ms);
                isSignatureBlank = ms.Length == 0;
            }

            if (!isSignatureBlank)
            {
                SignPadhaddata = true;
                AndroidSign.Stroke = Color.FromArgb("#BFDBF7");
                signsublbl.TextColor = Color.FromArgb("#031926");
            }
            else
            {
                SignPadhaddata = false;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "signpad_DrawCompleted");
        }
    }

    private void over16nameentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            over16namehelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "over16nameentry_TextChanged");
        }
    }

    private void hcfirstlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            hashcerrorlbl.IsVisible = false;

            var item = e.DataItem as string;

            if (item == "Yes")
            {
                //diahowtoaddlbl.IsVisible = true;
                //disautocomplete.IsVisible = true;
                //conditionschips.IsVisible = true;
            }
            else
            {
                //diahowtoaddlbl.IsVisible = false;
                //disautocomplete.IsVisible = false;
                //conditionschips.IsVisible = false;
            }

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "hcfirstlist_ItemTapped");
        }
    }

    private void otherhclist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            hcnotinlisterrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;

            if (item.Text == "Yes")
            {
                typeotherhclbl.IsVisible = true;
                typeotherhclist.IsVisible = true;
                otherhcenterlbl.IsVisible = true;
                otherhcentersublbl.IsVisible = true;
                otherhcentry.IsVisible = true;
                typeotherhclist.RefreshView();

            }
            else
            {
                typeotherhclbl.IsVisible = false;
                typeotherhclist.IsVisible = false;
                otherhcenterlbl.IsVisible = false;
                otherhcentersublbl.IsVisible = false;
                otherhcentry.IsVisible = false;

                bodyparterrorlbl.IsVisible = false;
                otherentryconerrorlbl.IsVisible = false;
                cancererrorlbl.IsVisible = false;
                pastcancererrorlbl.IsVisible = false;
            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "otherhclist_ItemTapped");
        }
    }

    private void othermedlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            othermederrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;

            if (item.Text == "Yes")
            {
                othermeddetailslbl.IsVisible = true;
                othermeddetailssublbl.IsVisible = true;
                othermedsentry.IsVisible = true;

            }
            else
            {
                othermeddetailslbl.IsVisible = false;
                othermeddetailssublbl.IsVisible = false;
                othermedsentry.IsVisible = false;
                othermedentryerrorlbl.IsVisible = false;

            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "othermedlist_ItemTapped");
        }
    }

    private void flulist_SelectionChanged(object sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
    {
        try
        {
            // Use e.AddedItems and e.RemovedItems if you want to be specific,
            // but checking the whole list is safest for UI visibility:
            var selectedItems = flulist.SelectedItems.Cast<OptionDetails>().ToList();

            bool hasCovid = selectedItems.Any(x => x.Text.Contains("COVID"));
            bool hasRSV = selectedItems.Any(x => x.Text.Contains("RSV"));
            bool fluCount = selectedItems.Any(x => x.Text.Contains("Flu"));
            //bool hasBothFluDoses = fluCount >= 2;

            fludatelbl.IsVisible = fluCount;
            fludateldirections.IsVisible = fluCount;
            fluhelper.IsVisible = fluCount;
            fluhelper.HasError = false;

            // Update UI
            coviddatelbl.IsVisible = hasCovid;
            coviddatelbldirections.IsVisible = hasCovid;
            dateEntryCovidJab.IsVisible = hasCovid;
            dateEntryCovidJab.HasError = false;

            rsvdatelbl.IsVisible = hasRSV;
            rsvdatedirections.IsVisible = hasRSV;
            dateEntryrsv.IsVisible = hasRSV;
            dateEntryrsv.HasError = false;
            fluerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "flulist_SelectionChanged");
        }
    }

    private void smokelist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            smokeerrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;

            if (item.Text == "Yes")
            {
                usesmokerlbl.IsVisible = true;
                usesmokelist.IsVisible = true;
                ageofsmokelbl.IsVisible = true;
                agesmokehelper.IsVisible = true;
                usesmokelbl.IsVisible = true;
                usesmokelistr.IsVisible = true;
                smokefreqlbl.IsVisible = true;
                smokefreqlistr.IsVisible = true;

                usesmokelist.RefreshView();
                usesmokelistr.RefreshView();
                smokefreqlistr.RefreshView();
            }
            else
            {
                usesmokerlbl.IsVisible = false;
                usesmokelist.IsVisible = false;
                ageofsmokelbl.IsVisible = false;
                agesmokehelper.IsVisible = false;
                usesmokelbl.IsVisible = false;
                usesmokelistr.IsVisible = false;
                smokefreqlbl.IsVisible = false;
                smokefreqlistr.IsVisible = false;

                stopsmokelbl.IsVisible = false;
                stopsmokehelper.IsVisible = false;

                usesmokeerrorlbl.IsVisible = false;
                usingsmokeerrorlbl.IsVisible = false;
                smokingerrorlbl.IsVisible = false;
            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "smokelist_ItemTapped");
        }
    }

    private void usesmokelistr_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            usingsmokeerrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;

            if (item.Text == "No")
            {

                stopsmokelbl.IsVisible = true;
                stopsmokehelper.IsVisible = true;
            }
            else
            {
                stopsmokelbl.IsVisible = false;
                stopsmokehelper.IsVisible = false;

            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "usesmokelistr_ItemTapped");
        }
    }

    private void alochollist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            alcoholerrorlbl.IsVisible = false;


            var item = e.DataItem as OptionDetails;

            if (item.Text == "Daily" || item.Text == "Weekly" || item.Text == "Occasionally")
            {
                usealochollbl.IsVisible = true;
                usealochollist.IsVisible = true;
                alcoholimg.IsVisible = true;

                usealochollist.RefreshView();
            }
            else
            {
                usealochollbl.IsVisible = false;
                usealochollist.IsVisible = false;
                alcoholimg.IsVisible = false;

                usealcoholerrorlbl.IsVisible = false;
            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "alochollist_ItemTapped");
        }
    }

    private void druglist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            drugserrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;

            if (item.Text == "Yes")
            {
                whatdrugslbl.IsVisible = true;
                whatdrugslist.IsVisible = true;
                drugoftenlbl.IsVisible = true;
                drugoftenlist.IsVisible = true;
                breathinglbl.IsVisible = true;
                breathinglist.IsVisible = true;

                whatdrugslist.RefreshView();
                drugoftenlist.RefreshView();
                breathinglist.RefreshView();
            }
            else
            {
                whatdrugslbl.IsVisible = false;
                whatdrugslist.IsVisible = false;
                drugoftenlbl.IsVisible = false;
                drugoftenlist.IsVisible = false;
                breathinglbl.IsVisible = false;
                breathinglist.IsVisible = false;

                whatdrugserrorlbl.IsVisible = false;
                drugsoftenerrorlbl.IsVisible = false;
                breathingerrorlbl.IsVisible = false;
            }


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "druglist_ItemTapped");
        }
    }

    private void telentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            telhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "telentry_TextChanged");
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
            CrashDetected.LogCrash(Ex, Navigation, "firsthouselholdcb_CheckedChanged");
        }
    }

    private void secondhouseholdcb_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {

        try
        {
            secondcheckboxerror.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "secondhouseholdcb_CheckedChanged");
        }
    }

    private void peopleentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            peoplenumhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "peopleentry_TextChanged");
        }
    }

    private void roomentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {

            roomnumhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "roomentry_TextChanged");
        }
    }

    private void sharedbathroomsentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            sharedbathroomsnumhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "sharedbathroomsentry_TextChanged");
        }
    }

    private void ventlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            venterrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "ventlist_ItemTapped");
        }
    }

    private void damplist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            damperrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "damplist_ItemTapped");
        }
    }

    private void coughlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            cougherrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "coughlist_ItemTapped");
        }
    }

    private void infectionyearentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            infectionhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "infectionyearentry_TextChanged");
        }
    }

    private void venlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            venhoserrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "venlist_ItemTapped");
        }
    }

    private void disautocomplete_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(disautocomplete.Text))
            {
                hcadderrorlbl.IsVisible = false;

            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "disautocomplete_PropertyChanged");
        }
    }

    private void cancerlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            cancererrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "cancerlist_ItemTapped");
        }
    }

    private void otherhcentrytext_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            otherentryconerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "otherhcentrytext_TextChanged");
        }
    }

    private void cancernowlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            pastcancererrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "cancernowlist_ItemTapped");
        }
    }

    private void medsfirstlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            medadderrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "medsfirstlist_ItemTapped");
        }
    }

    private void medautocomplete_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(medautocomplete.Text))
            {
                medchipadderrorlbl.IsVisible = false;

            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "medautocomplete_PropertyChanged");
        }
    }

    private void othermedtextentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            othermedentryerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "othermedtextentry_TextChanged");
        }
    }

    private void fluentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {

            fluhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "fluentry_TextChanged");
        }
    }

    private void coviddateentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {

            dateEntryCovidJab.HasError = false;
            sexiderror.IsVisible = false;

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "coviddateentry_TextChanged");
        }
    }

    private void rsvdateentry_TextChanged(object sender, TextChangedEventArgs e)
    {

        try
        {

            dateEntryrsv.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "rsvdateentry_TextChanged");
        }
    }

    private void dietlengthlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            dietlenghtlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "dietlengthlist_ItemTapped");
        }
    }

    private void takelist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            takesuppserrorlbl.IsVisible = false;

            // var item = e.DataItem as OptionDetails;

            // usingerrorlbl.IsVisible = false;


        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "takelist_ItemTapped");
        }
    }

    private void extralist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {


            // var item = e.DataItem as OptionDetails;

            // usingerrorlbl.IsVisible = false;

            whichsuppserrorlbl.IsVisible = false;

        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "extralist_ItemTapped");
        }
    }

    private void pregweeksentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {

            preghelper.HasError = false;

            if (sender is not Entry entry) return;

            string inputValue = e.NewTextValue;

            if (string.IsNullOrEmpty(inputValue)) return;

            if (inputValue.Contains('.') || inputValue.Contains(','))
            {
                string cleanValue = inputValue.Replace(".", "").Replace(",", "");
                entry.Text = cleanValue;
                entry.CursorPosition = Math.Min(e.OldTextValue?.Length ?? 0, cleanValue.Length);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "pregweeksentry_TextChanged");
        }
    }

    private void pregdateentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            ddhelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "pregdateentry_TextChanged");
        }
    }

    private void moblist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            ht1errorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "moblist_ItemTapped");
        }
    }

    private void sclist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            ht2errorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "sclist_ItemTapped");
        }
    }

    private void uclist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            ht3errorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "uclist_ItemTapped");
        }
    }

    private void painlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            ht4errorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "painlist_ItemTapped");
        }
    }

    private void deplist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            ht5errorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "deplist_ItemTapped");
        }
    }

    private void addqlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            aqerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "addqlist_ItemTapped");
        }
    }

    private void usesmokelist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            usesmokeerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "usesmokelist_ItemTapped");
        }
    }

    private void agesmokeentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            agesmokehelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "agesmokeentry_TextChanged");
        }
    }

    private void stopsmokeentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            stopsmokehelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "stopsmokeentry_TextChanged");
        }
    }

    private void smokefreqlistr_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            smokingerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "smokefreqlistr_ItemTapped");
        }
    }

    private void usealochollist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            usealcoholerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "usealochollist_ItemTapped");
        }
    }

    private void whatdrugslist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            whatdrugserrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "whatdrugslist_ItemTapped");
        }
    }

    private void drugoftenlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            drugsoftenerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "drugoftenlist_ItemTapped");
        }
    }

    private void breathinglist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            breathingerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "breathinglist_ItemTapped");
        }
    }

    private void sleeplist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            sleeperrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "sleeplist_ItemTapped");
        }
    }

    private void wakelist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            wakeerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "wakelist_ItemTapped");
        }
    }

    private void nightslist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            nightserrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "nightslist_ItemTapped");
        }
    }

    private void qualitylist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            qualityerrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "qualitylist_ItemTapped");
        }
    }

    private void moodlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            mooderrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "moodlist_ItemTapped");
        }
    }

    private void prodlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            proderrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "prodlist_ItemTapped");
        }
    }

    private void poorsleeplist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            poorsleeperrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "poorsleeplist_ItemTapped");
        }
    }

    private void sleepproblist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            sleepproderrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "sleepproblist_ItemTapped");
        }
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                signpad.Clear();
            }
            else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                drawingpad.Clear();
            }

            //nextbtn.BackgroundColor = Colors.LightGray;
            SignPadhaddata = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Button_Clicked");
        }
    }

    private void under10entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            under10helper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "under10entry_TextChanged");
        }
    }

    private void under10otherroleentry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            under10otherrolehelper.HasError = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "under10otherroleentry_TextChanged");
        }
    }

    private void under10rolelist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var item = e.DataItem as SignoffOption;

            under10roleerrorlbl.IsVisible = false;

            if (item.label.Contains("Other"))
            {
                under10otherrolehelper.IsVisible = true;
            }
            else
            {

                under10otherrolehelper.IsVisible = false;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "under10rolelist_ItemTapped");
        }
    }

    private void heardavlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            heardaverrorlbl.IsVisible = false;
            usedaverrorlbl.IsVisible = false;

            var item = e.DataItem as OptionDetails;

            if (item.Text == "Yes")
            {
                usedavlbl.IsVisible = true;
                usedavlist.IsVisible = true;
                usedavlist.RefreshView();
            }
            else
            {
                usedavlbl.IsVisible = false;
                usedavlist.IsVisible = false;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "heardavlist_ItemTapped");
        }
    }

    private void usedavlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            usedaverrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "usedavlist_ItemTapped");
        }
    }

    private void futureavlist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            futureaverrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "futureavlist_ItemTapped");
        }
    }

    private void futureavlist2_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            futureaverrorlbl2.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "futureavlist2_ItemTapped");
        }
    }

    private void futureavlist3_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            futureaverrorlbl3.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "futureavlist3_ItemTapped");
        }
    }

    private void futureavlist4_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            futureaverrorlbl4.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "futureavlist4_ItemTapped");
        }
    }

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            //Clear SelectedGp 
            gpautocomplete.IsEnabled = true;
            SelectedGp = null;
            GpSelectedView.IsVisible = false;
            gpautocomplete.IsVisible = true;
            Task.Delay(100);
            gpautocomplete.Clear();

        }
        catch (Exception Ex)
        {
            gpautocomplete.IsEnabled = true;
            CrashDetected.LogCrash(Ex, Navigation, "ImageButton_Clicked");
        }
    }

    private void typeotherhclist_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            bodyparterrorlbl.IsVisible = false;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "typeotherhclist_ItemTapped");
        }
    }

    private void usingphone1_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            usingphone1error1.IsVisible = false;

            var item = e.DataItem as string;


            if (item.Contains("Yes"))
            {
                emailsectiongrid.IsVisible = true;
                commborder1.IsVisible = true;
            }
            else
            {
                emailsectiongrid.IsVisible = false;
                commborder1.IsVisible = false;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void usingphone2_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {

            usingphone2error2.IsVisible = false;

            var item = e.DataItem as string;


            if (item.Contains("Yes"))
            {
                emailsectiongrid2.IsVisible = true;
                commborder2.IsVisible = true;
            }
            else
            {
                emailsectiongrid2.IsVisible = false;
                commborder2.IsVisible = false;
            }
        }
        catch (Exception ex)
        {

        }
    }
}