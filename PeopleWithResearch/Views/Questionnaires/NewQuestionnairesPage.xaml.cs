using CommunityToolkit.Mvvm.Messaging;
using FreakyKit.Utils;
using Microsoft.AppCenter.Crashes;
using Microsoft.Maui.Graphics.Text;
using Mopups.Services;
using Svg;
using Syncfusion.Maui.Buttons;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PeopleWithResearch;

public partial class NewQuestionnairesPage : ContentPage
{
    public newuserquestionnaire completeuserquestionnaire { get; set; } = new();
    public List<newuserquestionnaire> AllUserQuestionnaires { get; set; } = new();

    public ObservableCollection<QuestionAnswerJson> CompeltedAnswers { get; set; } = new();
    public questionnaires questionnaire { get; set; } = new();
    public ObservableCollection<confirmationmessage> DefaultMessage = new();
    public Dictionary<string, string> UserDetailsKey { get; set; } = new();

    public int rownumber = 0;
    public newuserquestionnaire SelectedAnswerList = new();

    private bool _isUpdating = false;
    private bool SubmitAnswered = true;
    private bool logoutAction = false;
    public string imageURL = string.Empty;
    private string _imageFilename;
    private bool IsPrimaryUser = true;
    public string QuestionnaireID = string.Empty;
    public bool isCompleted = false; 


    HashSet<string> targetTypes = new HashSet<string> { "date", "text", "slider" };

    protected override void OnDisappearing()
    {
        try
        {
            base.OnDisappearing();
            if (!isCompleted)
            {
                if (questionnaire?.QuestionAnswerJson != null)
                {
                    foreach (var question in questionnaire.QuestionAnswerJson)
                    {
                        if (question.options != null)
                        {
                            foreach (var option in question.options)
                            {
                                option.selected = false;
                            }
                        }
                    }
                }

                QuestionnaireContent.Content = null;
                BindingContext = null;
                questionnaire = null;
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "OnDisappearing");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(50);
        if (isCompleted)  await PopulateCompleted();
    }

    public QuestionAnswerJson CurrentQuestion
    {
        get
        {
            var visible = GetVisibleQuestions();
            return (visible != null && rownumber < visible.Count) ? visible[rownumber] : null;
        }
    }

    private async Task<string> FetchJsonAsync(string JsontoGet)
    {


        if (JsontoGet == "t1_form")
        {
            JsontoGet = "t1formjson";
        }
        else
        {
            var match = Regex.Match(JsontoGet, @"^t([1-9]|1[0-9]|2[0-8])_form$");
            if (match.Success)
            {
                JsontoGet = "tform";
            }
        }

        using var stream = await FileSystem.OpenAppPackageFileAsync($"{JsontoGet}.json");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public NewQuestionnairesPage(List<newuserquestionnaire> AllQuestionnaires, questionnaires QuestionnairePassed, bool isPrimaryUser)
    {
        InitializeComponent();
        AllUserQuestionnaires = AllQuestionnaires;
        questionnaire = QuestionnairePassed;
        if (questionnaire is null)
        {
            ShowHideData(true);
            return;
        }
        questionnaire?.ApplyTranslations();
        var qid = questionnaire?.questionnaireid ?? string.Empty;
        QuestionTitle.Text    = LocalisedTitle(qid);
        Questiontitle.Text    = LocalisedTitle(qid);
        QuestionDescription.Text = LocalisedDescription(qid);
        IsPrimaryUser = isPrimaryUser;
        DateTimeandCompelted.IsVisible = false;
        ApplyButtonTranslations();
        LoadQuestionnaire();
        BindingContext = this;
    }
    public NewQuestionnairesPage(newuserquestionnaire questionnairePassed, questionnaires Questionnaire, bool isPrimaryUser, Dictionary<string, String> UserKey)
    {
        InitializeComponent();
        if (questionnairePassed is null)
        {
            ShowHideData(true);
            return;
        }
        IsPrimaryUser = isPrimaryUser;
        completeuserquestionnaire = questionnairePassed;
        questionnaire = Questionnaire;
        IsPrimaryUser = isPrimaryUser;
        UserDetailsKey = UserKey;
        isCompleted = true;
        //PopulateCompleted();
    }

    private async Task LoadQuestionnaire()
    {
        try
        {
            loadingstack.IsVisible = true;
            if (questionnaire == null)
            {
                //Fetch Data
            }

            foreach (var question in questionnaire.QuestionAnswerJson)
            {
                // Apply language translations to labels, placeholders and option text
                question.ApplyTranslations();

                if (targetTypes.Contains(question.type) && (question.options == null || question.options.Length == 0))
                {
                    question.HasAnswered = question.type == "slider";
                    question.options = new Option[]
                    {
                        new Option
                        {
                            answerid = question.id,
                            value = "1",
                            text = string.Empty,
                            SliderValue = 50
                        }
                    };
                }
            }

            rownumber = 0;
            await UpdateQuestionUI();
            await ShowHideData();
        }
        catch (Exception Ex)
        {
            await ShowHideData(true);
            CrashDetected.LogCrash(Ex, Navigation, "LoadQuestionnaire");
        }
    }

    private async Task PopulateCompleted()
    {
        try
        {
            loadingstack.IsVisible = true;
            await Task.Delay(200);

            if (completeuserquestionnaire is null)
            {
                await ShowHideData(true);
                return;
            }

            if (questionnaire == null)
            {
                var json = await FetchJsonAsync(completeuserquestionnaire.title);
                if (string.IsNullOrWhiteSpace(json))
                {
                    await ShowHideData(true);
                    return;
                }

                var questionAnswer = APICalls.Instance.DeserializeNestedJson<QuestionAnswerJson>(json);

                questionnaire = new questionnaires
                {
                    questionnaireid = completeuserquestionnaire.questionnaireid,
                    QuestionAnswerJson = questionAnswer,
                    QuestionAnswerJsonRaw = json
                };
            }

            // Apply questionnaire-level translations (title / description) before any UI assignment
            questionnaire?.ApplyTranslations();

            var rawId = !string.IsNullOrEmpty(questionnaire?.questionnaireid)
                ? questionnaire.questionnaireid
                : completeuserquestionnaire.questionnaireid;

            // Apply language translations before rendering the completed view
            if (questionnaire?.QuestionAnswerJson != null)
            {
                foreach (var question in questionnaire.QuestionAnswerJson)
                    question.ApplyTranslations();
            }

            QuestionTitle.Text = LocalisedTitle(rawId);

            //QuestionTitle.Text = !string.IsNullOrEmpty(questionnaire?.title)
            //    ? questionnaire?.title
            //    : completeuserquestionnaire.questionnaireid;

            CompletedDateTime.Text = completeuserquestionnaire.FormattedDateTime;
            CompletedbyBorder.IsVisible = IsPrimaryUser;
            Completedbylbl.Text = UserDetailsKey.TryGetValue(completeuserquestionnaire.userid, out var Name)
                ? Name
                : completeuserquestionnaire.userid;

            var answersByQuestionId = completeuserquestionnaire.FeedbackList
                .Where(x => x?.questionid != null)
                .ToDictionary(x => FixTPrefix(x.questionid));

            var Results = new List<QuestionAnswerJson>();

            foreach (var question in questionnaire.QuestionAnswerJson)
            {
                if (string.Equals(question.type, "symptom_grid", StringComparison.OrdinalIgnoreCase))
                {
                    var processedSymptom = await T1QuestionnaireItemOptimized(question);
                    if (processedSymptom != null)
                        Results.Add(processedSymptom);

                    continue;
                }

                var RemoveDefault = FixTPrefix(question.questionid);

                if (!answersByQuestionId.TryGetValue(RemoveDefault, out var answer)) continue;

                Option[] options;

                if (targetTypes.Contains(question.type))
                {
                    var firstAnswer = answer.answer?.FirstOrDefault();
                    options = new[]
                    {
                    question.type == "date" || question.type == "text"
                        ? new Option { text = firstAnswer?.text }
                        : new Option
                        {
                            SliderValue = int.TryParse(firstAnswer?.answervalue?.ToString(), out var parsedValue)
                                ? parsedValue
                                : 50
                        }
                };
                }
                else
                {
                    if (question.type == "picture")
                    {
                        question.label = questionnaire.questionnaireid == "b1_samples"
                            ? "Image of the lateral flow device you carried out"
                            : "Review uploaded image";
                    }
                    if (question.type == "file_upload")
                    {
                        question.label = "Image of the lateral flow device you carried out";
                    }

                    var submittedAnswerIds = answer.answer?
                        .Select(a => FixTPrefix(a.answerid))
                        .ToHashSet()
                        ?? new HashSet<string>();

                    options = question.options?
                        .Where(opt => submittedAnswerIds.Contains(FixTPrefix(opt.answerid)))
                        .ToArray()
                        ?? Array.Empty<Option>();
                }

                question.id = question.questionid;
                question.options = options;
                question.image = question.type == "picture" || question.type == "file_upload"
                    ? completeuserquestionnaire.imagefilename
                    : string.Empty;
                question.showimage = (question.type == "picture" || question.type == "file_upload") &&
                                    !string.IsNullOrEmpty(completeuserquestionnaire.imagefilename);

                Results.Add(question);
            }

            if (Results.Count == 0)
            {
                await ShowHideData(true);
                return;
            }

            try
            {
                CompeltedAnswers.Clear();
                foreach (var item in Results) CompeltedAnswers.Add(item);

                CompletedCollectionView.ItemsSource = CompeltedAnswers.OrderBy(x => x.order);

                QuestionnaireContent.IsVisible = false;
                ButtonStack.IsVisible = false;
                loadingstack.IsVisible = false;

                CompletedCollectionView.IsVisible = true;
                datastack.IsVisible = true;
            }
            catch (Exception Exp)
            {
                CrashDetected.LogCrash(Exp, Navigation, "PopulateCompleted");
                await ShowHideData(true);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "PopulateCompleted");
            await ShowHideData(true);
        }
    }

    private static string LocalisedTitle(string questionnaireId)
    {
        if (string.IsNullOrEmpty(questionnaireId))
            return string.Empty;

        var match = Regex.Match(questionnaireId, @"^t([1-9]|1[0-9]|2[0-8])_form$", RegexOptions.IgnoreCase);
        if (match.Success)
            return string.Format(LocalizationManager.Get("Questionnaire_DailySymptoms_Title"), match.Groups[1].Value);

        var key = questionnaireId.ToUpperInvariant() switch
        {
            "B1_SAMPLES"                             => "Questionnaire_Baseline_Title",
            "B1_INDIVIDUAL_QUESTIONNAIRE"            => "Questionnaire_Baseline_Title",
            "70530492-D1B8-42F5-A851-1C8769288995"  => "Questionnaire_Withdraw_Title",
            "DDD843CF-021B-4557-8824-13C5B4E2EA85"  => "Questionnaire_B1Samples_Title",
            "B627DF59-7AD8-4832-A407-BF5F85BDE8E0"  => "Questionnaire_EndOfStudy_Title",
            _ => null
        };

        return key != null ? LocalizationManager.Get(key) : questionnaireId;
    }

    private static string LocalisedDescription(string questionnaireId)
    {
        if (string.IsNullOrEmpty(questionnaireId))
            return string.Empty;

        var match = Regex.Match(questionnaireId, @"^t([1-9]|1[0-9]|2[0-8])_form$", RegexOptions.IgnoreCase);
        if (match.Success)
            return LocalizationManager.Get("Questionnaire_DailySymptoms_Description");

        var key = questionnaireId.ToUpperInvariant() switch
        {
            "B1_SAMPLES"                             => "Questionnaire_Baseline_Description",
            "B1_INDIVIDUAL_QUESTIONNAIRE"            => "Questionnaire_Baseline_Description",
            "70530492-D1B8-42F5-A851-1C8769288995"  => "Questionnaire_Withdraw_Description",
            "DDD843CF-021B-4557-8824-13C5B4E2EA85"  => "Questionnaire_B1Samples_Description",
            "B627DF59-7AD8-4832-A407-BF5F85BDE8E0"  => "Questionnaire_EndOfStudy_Description",
            _ => null
        };

        return key != null ? LocalizationManager.Get(key) : string.Empty;
    }

    //private static string FixTPrefix(string id)
    //{
    //    if (string.IsNullOrEmpty(id)) return id;
    //    int idx = id.IndexOf('_');
    //    return idx >= 0 ? id.Substring(idx + 1) : id;
    //}

    private static string FixTPrefix(string id)
    {
        if (string.IsNullOrEmpty(id)) return id;
        if (id.StartsWith("t") && id.Contains("_") && !id.StartsWith("tminus"))
        {
            int idx = id.IndexOf('_');
            return id.Substring(idx + 1);
        }
        return id;
    }


    private async Task ShowHideData(bool failedtoLoad = false)
    {
        try
        {
            loadingstack.IsVisible = false;
            InitialPage.IsVisible = !failedtoLoad;
            LoadFailed.IsVisible = failedtoLoad;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "ShowHideData");
        }
    }

    /// <summary>
    /// Sets all hardcoded button and intro-popup texts to their localised equivalents.
    /// Safe to call before <see cref="LoadQuestionnaire"/> because it only touches
    /// named UI elements that are guaranteed to exist after <see cref="InitializeComponent"/>.
    /// </summary>
    private void ApplyButtonTranslations()
    {
        FooterBackBtn.Text       = LocalizationManager.Get("Common_Back");
        FooterNextBtn.Text       = LocalizationManager.Get("Common_Next");
        FooterSubmitBtn.Text     = LocalizationManager.Get("Common_SubmitQuestionnaire");
        StartQuestionnaireBtn.Text = LocalizationManager.Get("Common_StartQuestionnaire");
    }

    private async Task<QuestionAnswerJson> T1QuestionnaireItemOptimized(QuestionAnswerJson symptom)
    {
        if (symptom == null) return null;

        var feedbackList = completeuserquestionnaire?.FeedbackList ?? new ObservableCollection<Feedback>();
        if (!feedbackList.Any()) return null;

        var allSymptoms = symptom.symptom_groups?
            .Where(g => g?.symptoms != null)
            .SelectMany(g => g.symptoms)
            .ToList() ?? new List<Symptom>();

        if (!allSymptoms.Any()) return null;

        var dayTabsLookup = symptom.day_tabs?.ToDictionary(d => d.id, d => d.label)
            ?? new Dictionary<string, string>();

        var prefixLengths = dayTabsLookup.Keys
            .ToDictionary(k => k, k => k.Length + 1);

        var feedbackMinusT = feedbackList
            .Where(f => f?.questionid != null && f.questionid.Contains('_'))
            .GroupBy(f =>
            {
                var idx = f.questionid.IndexOf('_');
                return f.questionid.Substring(idx + 1);
            })
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        bool containsSymptomData = false;

        foreach (var sym in allSymptoms)
        {
            if (!feedbackMinusT.TryGetValue(sym.id, out var relatedFeedbacks))
                continue;

            var processedSymptomData = new List<symptomdata>();

            foreach (var fb in relatedFeedbacks)
            {
                foreach (var a in fb.answer ?? Enumerable.Empty<Answer>())
                {
                    if (a?.answerid == null) continue;

                    var parts = a.answerid.Split('_');
                    string dayPrefix = parts.Length > 1 ? parts[0] : string.Empty;
                    dayTabsLookup.TryGetValue(dayPrefix, out var dayLabel);

                    processedSymptomData.Add(new symptomdata
                    {
                        id = a.answerid,
                        value = a.answervalue,
                        label = dayLabel,
                        text = a.text,
                        colour = GetShadeColour(a.text),
                        textcolour = GetTextColour(a.text)
                    });
                }
            }

            if (processedSymptomData.Count > 0)
            {
                sym.SymptomData = new ObservableCollection<symptomdata>(processedSymptomData);
                containsSymptomData = true;
            }
            else
            {
                sym.SymptomData ??= new ObservableCollection<symptomdata>();
            }
        }


        if (symptom.rash_followup?.questions != null)
            symptom.rash_followup.questions = await ProcessFollowupOptimized(
                symptom.rash_followup.questions,
                feedbackList,
                dayTabsLookup,
                prefixLengths);

        if (symptom.daily_impact?.questions != null)
            symptom.daily_impact.questions = await ProcessFollowupOptimized(
                symptom.daily_impact.questions,
                feedbackList,
                dayTabsLookup,
                prefixLengths);

        symptom.symptom_groups ??= Array.Empty<SymptomGroup>();
        symptom.rash_followup ??= new rashfollowup { questions = Array.Empty<question>() };
        symptom.daily_impact ??= new rashfollowup { questions = Array.Empty<question>() };
        symptom.type = "symptom_grid";

        if (symptom.symptom_groups == null ||
    !symptom.symptom_groups.Any(g =>
        g?.symptoms != null &&
        g.symptoms.Any(s =>
            s.SymptomData != null &&
            s.SymptomData.Any()
        )
    ))
        {
            return null;
        }

        return symptom;
    }


    private async Task<question[]> ProcessFollowupOptimized(
        question[] questions,
        ObservableCollection<Feedback> feedbackList,
        Dictionary<string, string> dayTabsLookup,
        Dictionary<string, int> prefixLengths)
    {
        if (questions == null) return Array.Empty<question>();
        var processedQuestions = new List<question>();

        foreach (var q in questions)
        {
            if (q?.options == null) continue;

            var templateOptionsMap = q.options
                .Where(o => !string.IsNullOrEmpty(o.answerid))
                .ToDictionary(o => o.answerid, StringComparer.OrdinalIgnoreCase);

            var displayOptions = new List<Option>();
            var matchingAnswers = feedbackList
                .Where(f => f?.questionid != null &&
                            q.id != null &&
                            f.questionid.Contains(q.id, StringComparison.OrdinalIgnoreCase) &&
                            f.answer != null)
                .SelectMany(f => f.answer)
                .Where(a => a?.answerid != null);

            foreach (var answer in matchingAnswers)
            {
                var parts = answer.answerid.Split('_');
                var dayPrefix = parts.Length > 1 ? parts[0] : string.Empty;
                dayTabsLookup.TryGetValue(dayPrefix, out var dayLabel);

                var normalizedAnswerId = parts.Length > 1 &&
                                         prefixLengths.TryGetValue(dayPrefix, out var len)
                    ? answer.answerid.Substring(len)
                    : answer.answerid;

                if (templateOptionsMap.TryGetValue(normalizedAnswerId, out var templateOpt))
                {
                    displayOptions.Add(new Option
                    {
                        answerid = answer.answerid,
                        text = templateOpt.text,
                        value = templateOpt.value,
                        selected = true,
                        title = dayLabel
                    });
                }
            }

            if (!displayOptions.Any()) continue;

            q.options = displayOptions
                .GroupBy(o => new { o.text, o.value })
                .Select(group =>
                {
                    var orderedItems = group.ToList();
                    var titles = orderedItems
                        .Select(x => x.title)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct();
                    var first = orderedItems.First();

                    return new Option
                    {
                        answerid = first.answerid,
                        text = first.text,
                        value = first.value,
                        selected = true,
                        title = string.Join(", ", titles)
                    };
                })
                .ToArray();

            processedQuestions.Add(q);
        }

        return processedQuestions.ToArray();
    }

    private string GetShadeColour(string text)
    {
        return text switch
        {
            "None" => "#CFD8DC",
            "Mild" => "#FFF9C4",
            "Moderate" => "#FFCC80",
            "Severe" => "#EF9A9A",
            _ => "#CFD8DC"
        };
    }

    private string GetTextColour(string text)
    {
        return text switch
        {
            "None" => "#455A64",
            "Mild" => "#F57F17",
            "Moderate" => "#E65100",
            "Severe" => "#455A64",
            _ => "#B71C1C"
        };
    }




    //private async Task PopulateCompleted()
    //{
    //    try
    //    {
    //        loadingstack.IsVisible = true;
    //        await Task.Delay(200);

    //        if (completeuserquestionnaire is null)
    //        {
    //            ShowHideData(true);
    //            return;
    //        }

    //        if (questionnaire == null)
    //        {
    //            var json = await FetchJsonAsync(completeuserquestionnaire.title);
    //            if (string.IsNullOrWhiteSpace(json))
    //            {
    //                ShowHideData(true);
    //                return;
    //            }

    //            var questionAnswer = APICalls.Instance.DeserializeNestedJson<QuestionAnswerJson>(json);

    //            questionnaire = new questionnaires
    //            {
    //                questionnaireid = completeuserquestionnaire.questionnaireid,
    //                QuestionAnswerJson = questionAnswer,
    //                QuestionAnswerJsonRaw = json
    //            };
    //        }

    //        // Title
    //        QuestionTitle.Text = !string.IsNullOrEmpty(questionnaire?.title)
    //            ? questionnaire?.title
    //            : completeuserquestionnaire.questionnaireid;

    //        CompletedDateTime.Text = completeuserquestionnaire.FormattedDateTime;
    //        CompletedbyBorder.IsVisible = IsPrimaryUser;
    //        Completedbylbl.Text = UserDetailsKey.TryGetValue(completeuserquestionnaire.userid, out var Name)
    //            ? Name
    //            : completeuserquestionnaire.userid;

    //        var answersByQuestionId = completeuserquestionnaire.FeedbackList
    //            .Where(x => x?.questionid != null)
    //            .ToDictionary(x => x.questionid);

    //        var Results = new List<QuestionAnswerJson>();

    //        foreach (var question in questionnaire.QuestionAnswerJson)
    //        {
    //            if (question.questionid == "symptom_grid")
    //            {
    //                var processedSymptom = await T1QuestionnaireItemOptimized(question);
    //                if (processedSymptom != null)
    //                    Results.Add(processedSymptom);

    //                continue;
    //            }

    //            if (!answersByQuestionId.TryGetValue(question.questionid, out var answer))
    //                continue;

    //            Option[] options;

    //            if (targetTypes.Contains(question.type))
    //            {
    //                var firstAnswer = answer.answer?.FirstOrDefault();
    //                options = new[]
    //                {
    //                question.type == "date" || question.type == "text"
    //                    ? new Option { text = firstAnswer?.text }
    //                    : new Option
    //                    {
    //                        SliderValue = int.TryParse(firstAnswer?.answervalue?.ToString(), out var parsedValue)
    //                            ? parsedValue
    //                            : 50
    //                    }
    //            };
    //            }
    //            else
    //            {
    //                if (question.type == "picture")
    //                {
    //                    question.label = questionnaire.questionnaireid == "b1_samples"
    //                        ? "Review uploaded image of the lateral flow device you carried out"
    //                        : "Review uploaded image";
    //                }

    //                var submittedAnswerIds = answer.answer?.Select(a => a.answerid).ToHashSet()
    //                    ?? new HashSet<string>();

    //                options = question.options?
    //                    .Where(opt => submittedAnswerIds.Contains(opt.answerid))
    //                    .ToArray()
    //                    ?? Array.Empty<Option>();
    //            }

    //            question.id = question.questionid;
    //            question.options = options;
    //            question.image = question.type == "picture" ? completeuserquestionnaire.imagefilename : string.Empty;
    //            question.showimage = question.type == "picture" && !string.IsNullOrEmpty(completeuserquestionnaire.imagefilename);

    //            Results.Add(question);
    //        }

    //        if (Results == null || Results.Count == 0)
    //        {
    //            ShowHideData(true);
    //            return;
    //        }

    //        try
    //        {
    //            CompeltedAnswers.Clear();

    //            foreach (var item in Results) CompeltedAnswers.Add(item);

    //            try
    //            {
    //                // Convert the data collection into clean, raw JSON text
    //                var rawJsonData = System.Text.Json.JsonSerializer.Serialize(CompeltedAnswers, new System.Text.Json.JsonSerializerOptions
    //                {
    //                    WriteIndented = true // Makes it readable
    //                });

    //                // Copy it directly to the device clipboard so you can paste it anywhere
    //                await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(rawJsonData);

    //                // Optional: Alert yourself so you know it copied successfully
    //                System.Diagnostics.Debug.WriteLine($"RAW DATA COPIED:\n{rawJsonData}");
    //            }
    //            catch (Exception jsonEx)
    //            {
    //                System.Diagnostics.Debug.WriteLine($"Failed to capture raw JSON: {jsonEx.Message}");
    //            }

    //            await Task.Delay(200);
    //            CompletedCollectionView.ItemsSource = null;
    //            CompletedCollectionView.ItemsSource = CompeltedAnswers;

    //            QuestionnaireContent.IsVisible = false;
    //            ButtonStack.IsVisible = false;
    //            loadingstack.IsVisible = false;

    //            CompletedCollectionView.IsVisible = true;
    //            datastack.IsVisible = true;
    //        }
    //        catch (Exception Exp)
    //        {
    //            CrashDetected(Exp);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        CrashDetected(ex);
    //    }
    //}

    //private async Task ShowHideData(bool failedtoLoad = false)
    //{
    //    try
    //    {
    //        loadingstack.IsVisible = false;
    //        InitialPage.IsVisible = !failedtoLoad;
    //        LoadFailed.IsVisible = failedtoLoad;
    //    }
    //    catch (Exception Ex)
    //    {
    //        CrashDetected(Ex);
    //    }
    //}

    //private async Task<QuestionAnswerJson> T1QuestionnaireItemOptimized(QuestionAnswerJson symptom)
    //{
    //    if (symptom == null) return null;

    //    var feedbackList = completeuserquestionnaire?.FeedbackList ?? new ObservableCollection<Feedback>();
    //    if (!feedbackList.Any()) return null;

    //    var allSymptoms = symptom.symptom_groups?
    //        .Where(g => g?.symptoms != null)
    //        .SelectMany(g => g.symptoms)
    //        .ToList() ?? new List<Symptom>();

    //    if (!allSymptoms.Any()) return null;

    //    var dayTabsLookup = symptom.day_tabs?.ToDictionary(d => d.id, d => d.label)
    //        ?? new Dictionary<string, string>();

    //    var prefixLengths = dayTabsLookup.Keys
    //        .ToDictionary(k => k, k => k.Length + 1);

    //    var feedbackMinusT = feedbackList
    //        .Where(f => f?.questionid != null && f.questionid.Contains('_'))
    //        .GroupBy(f =>
    //        {
    //            var idx = f.questionid.IndexOf('_');
    //            return f.questionid.Substring(idx + 1);
    //        })
    //       .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

    //    bool containsSymptomData = false;

    //    foreach (var sym in allSymptoms)
    //    {
    //        if (!feedbackMinusT.TryGetValue(sym.id, out var relatedFeedbacks))
    //            continue;

    //        var processedSymptomData = new List<symptomdata>();

    //        foreach (var fb in relatedFeedbacks)
    //        {
    //            foreach (var a in fb.answer ?? Enumerable.Empty<Answer>())
    //            {
    //                if (a?.answerid == null) continue;

    //                var parts = a.answerid.Split('_');

    //                // FIX: handle single-segment IDs
    //                string dayPrefix = parts.Length > 1 ? parts[0] : string.Empty;

    //                dayTabsLookup.TryGetValue(dayPrefix, out var dayLabel);

    //                var normalizedAnswerId =
    //                    parts.Length > 1 && prefixLengths.TryGetValue(dayPrefix, out var len)
    //                        ? a.answerid.Substring(len)
    //                        : a.answerid;

    //                var shade = GetShadeColour(a.text);
    //                var textColour = GetTextColour(a.text);

    //                processedSymptomData.Add(new symptomdata
    //                {
    //                    id = a.answerid,
    //                    value = a.answervalue,
    //                    label = dayLabel,
    //                    text = a.text,
    //                    colour = shade,
    //                    textcolour = textColour
    //                });
    //            }
    //        }

    //        if (processedSymptomData.Count > 0)
    //        {
    //            sym.SymptomData = new ObservableCollection<symptomdata>(processedSymptomData);
    //            containsSymptomData = true;
    //        }
    //    }

    //    if (!containsSymptomData)
    //        return null;

    //    if (symptom.rash_followup?.questions != null)
    //        symptom.rash_followup.questions = await ProcessFollowupOptimized(symptom.rash_followup.questions, feedbackList, dayTabsLookup, prefixLengths);

    //    if (symptom.daily_impact?.questions != null)
    //        symptom.daily_impact.questions = await ProcessFollowupOptimized(symptom.daily_impact.questions, feedbackList, dayTabsLookup, prefixLengths);

    //    return symptom;
    //}

    //private async Task<question[]> ProcessFollowupOptimized(
    //   question[] questions,
    //   ObservableCollection<Feedback> feedbackList,
    //   Dictionary<string, string> dayTabsLookup,
    //   Dictionary<string, int> prefixLengths)
    //{
    //    if (questions == null) return Array.Empty<question>();

    //    var processedQuestions = new List<question>();

    //    foreach (var q in questions)
    //    {
    //        if (q?.options == null) continue;

    //        var templateOptionsMap = q.options
    //            .Where(o => !string.IsNullOrEmpty(o.answerid))
    //            .ToDictionary(o => o.answerid, StringComparer.OrdinalIgnoreCase);

    //        var displayOptions = new List<Option>();

    //        var matchingAnswers = feedbackList
    //         .Where(f => f?.questionid != null && q.id != null && f.questionid.Contains(q.id, StringComparison.OrdinalIgnoreCase) && f.answer != null)
    //            .SelectMany(f => f.answer)
    //            .Where(a => a?.answerid != null);

    //        foreach (var answer in matchingAnswers)
    //        {
    //            var parts = answer.answerid.Split('_');
    //            var dayPrefix = parts.Length > 1 ? parts[0] : string.Empty;

    //            dayTabsLookup.TryGetValue(dayPrefix, out var dayLabel);

    //            var normalizedAnswerId =
    //                parts.Length > 1 && prefixLengths.TryGetValue(dayPrefix, out var len)
    //                    ? answer.answerid.Substring(len)
    //                    : answer.answerid;

    //            if (templateOptionsMap.TryGetValue(normalizedAnswerId, out var templateOpt))
    //            {
    //                displayOptions.Add(new Option
    //                {
    //                    answerid = answer.answerid,
    //                    text = templateOpt.text,
    //                    value = templateOpt.value,
    //                    selected = true,
    //                    title = dayLabel
    //                });
    //            }
    //        }

    //        if (!displayOptions.Any()) continue;

    //        q.options = displayOptions
    //            .GroupBy(o => new { o.text, o.value })
    //            .Select(group =>
    //            {
    //                var orderedItems = group.ToList();
    //                var titles = orderedItems.Select(x => x.title).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct();
    //                var first = orderedItems.First();

    //                return new Option
    //                {
    //                    answerid = first.answerid,
    //                    text = first.text,
    //                    value = first.value,
    //                    selected = true,
    //                    title = string.Join(", ", titles)
    //                };
    //            })
    //            .ToArray();

    //        processedQuestions.Add(q);
    //    }

    //    return processedQuestions.ToArray();
    //}

    //private Color GetShadeColour(string text)
    //{
    //    return text switch
    //    {
    //        "None" => Color.FromArgb("#CFD8DC"),
    //        "Mild" => Color.FromArgb("#FFF9C4"),
    //        "Moderate" => Color.FromArgb("#FFCC80"),
    //        "Severe" => Color.FromArgb("#EF9A9A"),
    //        _ => Color.FromArgb("#CFD8DC")
    //    };
    //}

    //private Color GetTextColour(string text)
    //{
    //    return text switch
    //    {
    //        "None" => Color.FromArgb("#455A64"),
    //        "Mild" => Color.FromArgb("#F57F17"),
    //        "Moderate" => Color.FromArgb("#E65100"),
    //        "Severe" => Color.FromArgb("#455A64"),
    //        _ => Color.FromArgb("#B71C1C")
    //    };
    //}

    private List<QuestionAnswerJson> GetVisibleQuestions()
    {
        if (questionnaire?.QuestionAnswerJson == null)
            return new List<QuestionAnswerJson>();

        var userTypeFiltered = questionnaire.QuestionAnswerJson
            .Where(q =>
                q != null &&
                (
                    string.IsNullOrWhiteSpace(q.usertype) ||
                    q.usertype.Equals("all", StringComparison.OrdinalIgnoreCase) ||
                    (q.usertype.Equals("primaryuser", StringComparison.OrdinalIgnoreCase) && IsPrimaryUser)
                ))
            .ToList();

        var selectedAnswerIds = userTypeFiltered
            .Where(q => q.options != null)
            .SelectMany(q => q.options!)
            .Where(o => o != null &&
                        o.selected &&
                        !string.IsNullOrWhiteSpace(o.answerid))
            .Select(o => o.answerid.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return userTypeFiltered
            .Where(q =>
                string.IsNullOrWhiteSpace(q.branchinglogic) ||
                q.branchinglogic
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Any(selectedAnswerIds.Contains))
            .OrderBy(q => q.order)
            .ToList();
    }

    private async void submitbtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (!ValidateCurrentQuestion()) return;

            var Confirm = new confirmationmessage()
            {
                confirmationmessageid = "1",
                confirmationmessagetitle = "Thank you for completing the HOPPER Study Samples, Symptoms and changes Form. Please ensure the remaining individuals in your household complete their forms and please continue to sample and complete the forms daily.",
                action = "complete"
            };

            if (questionnaire.Confirmationmessage.Count == 0)
            {
                questionnaire.Confirmationmessage.Add(Confirm);
            }

            var ConfirmMess = questionnaire.Confirmationmessage.FirstOrDefault();
            if (ConfirmMess.action == "image-upload")
            {
                bool checkQuestion = questionnaire.QuestionAnswerJson?
                    .SelectMany(q => q.options ?? Array.Empty<Option>())
                    .Any(op => op.answerid == ConfirmMess.answerid && (op.selected)) ?? false;

                if (!checkQuestion)
                {
                    questionnaire.Confirmationmessage.Clear();
                    questionnaire.Confirmationmessage.Add(Confirm);
                }
            }

            var tcs = new TaskCompletionSource<string>();
            await MopupService.Instance.PushAsync(new ConfirmMessage(questionnaire.Confirmationmessage, tcs) { });
            string QuestionAction = await tcs.Task;

            var submission = new newuserquestionnaire
            {
                questionnaireid = questionnaire.questionnaireid,
                userid = Helpers.Settings.UsersID,
                FeedbackList = new ObservableCollection<Feedback>(),
            };

            if (QuestionAction.Contains(".png"))
            {
                submission.imagefilename = QuestionAction;
            }
            foreach (var question in questionnaire.QuestionAnswerJson)
            {
                var actualQuestionId = !string.IsNullOrEmpty(question.id) ? question.id : question.questionid;

                var answers = new ObservableCollection<Answer>(
                    question.options?
                        .Where(o => o.selected ||
                            ((question.type == "text" || question.type == "date") && !string.IsNullOrWhiteSpace(o.text)) ||
                            (question.type == "slider"))
                        .Select(o => new Answer
                        {
                            answerid = o.answerid,
                            answervalue = (question.type == "text" || question.type == "date")
        ? string.Empty
        : o.value,
                            text = o.text,
                        })
                    ?? Enumerable.Empty<Answer>()
                );

                if (answers.Count > 0)
                {
                    var feedback = new Feedback
                    {
                        questionid = actualQuestionId,
                        answer = answers
                    };

                    submission.FeedbackList.Add(feedback);
                }
            }
            submission.DateTimeAdded = DateTime.Now;
            submission.feedback = System.Text.Json.JsonSerializer.Serialize(submission.FeedbackList);

            SelectedAnswerList = submission;
            SelectedAnswerList = await APICalls.Instance.PostUserQuestionnaire(SelectedAnswerList);

            AllUserQuestionnaires.Add(SelectedAnswerList);
            WeakReferenceMessenger.Default.Send(new UpdateDashCompelted(AllUserQuestionnaires));

            if (QuestionAction == "logout")
            {
                logoutAction = true;             
            }
            else
            {
                Navigation.RemovePage(this);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "submitbtn_Clicked");
        }
        finally
        {
            if (SubmitAnswered)
            {
                await MopupService.Instance.PushAsync(new PopupPageHelper("Questionnaire Completed"));
                await Task.Delay(3000);
                if (logoutAction)
                {
                    var changes = new Dictionary<string, object> { { "status", "Withdrawn" } };
                    bool success = await APICalls.Instance.UpdateUserData(Helpers.Settings.UsersID, changes);
                    if (success)
                    {
                        Newlogout HandleLogout = new Newlogout("Logout");
                    }
                }
                await MopupService.Instance.PopAllAsync(false);
            }         
        }
    }

    private async void TextEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (CurrentQuestion != null)
            {
                await CheckDateText(e.NewTextValue);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "TextEntry_TextChanged");
        }
    }

    private async void DateEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (CurrentQuestion != null)
            {
                await CheckDateText(e.NewTextValue);
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "DateEntry_TextChanged");
        }
    }

    private async Task CheckDateText(string NewTextValue)
    {
        try
        {
            bool hasText = !string.IsNullOrWhiteSpace(NewTextValue);
            var Current = CurrentQuestion;
            if (Current is not null)
            {
                Current.HasAnswered = hasText;
                Current.ShowRequired = false;
                Current.ColourBorder = Colors.White;
                FooterRequiredLabel.IsVisible = false;
                CurrentQuestion.Haserror = false;
                var Firstoption = Current.options.FirstOrDefault();
                if (Firstoption != null)
                {
                    Firstoption.Haserror = false;
                    Firstoption.Errortext = string.Empty; 
                }
            }
            


            RefreshFooterState();
        }
        catch(Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "CheckDateText");
        }
    }

    private async Task UpdateQuestionUI()
    {
        _isUpdating = true;
        try
        {
            var visible = GetVisibleQuestions();
            if (visible.Count == 0) return;

            if (rownumber >= visible.Count)
                rownumber = Math.Max(0, visible.Count - 1);

            var current = visible[rownumber];
            current.questionnum = $"Question {rownumber + 1} of {visible.Count}";

            bool isFirst = rownumber == 0;
            bool isLast = rownumber == visible.Count - 1;

            var templateSelector = (DataTemplateSelector)Resources["QuestionnaireSelector"];
            var template = templateSelector.SelectTemplate(current, this);
            if (template != null)
            {
                var view = (View)template.CreateContent();
                view.BindingContext = current;
                QuestionnaireContent.Content = view;
            }

            FooterBackBtn.IsVisible = !isFirst;
            FooterNextBtn.IsVisible = !isLast;
            FooterSubmitBtn.IsVisible = isLast && (current.ShowRequired || !current.required);
            FooterRequiredLabel.IsVisible = current.ShowRequired;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void RefreshFooterState()
    {
        var visible = GetVisibleQuestions();
        var current = (rownumber < visible.Count) ? visible[rownumber] : null;
        if (current == null) return;

        bool isLast = rownumber == visible.Count - 1;
        FooterNextBtn.IsVisible = !isLast;
        FooterSubmitBtn.IsVisible = isLast && current.HasAnswered;
    }

    private async void Nextbtn_Clicked(object sender, EventArgs e)
    {
        if (_isUpdating) return;
        if (!ValidateCurrentQuestion()) return;

        var visibleCount = GetVisibleQuestions().Count;
        if (rownumber < visibleCount - 1)
        {
            rownumber++;
            await UpdateQuestionUI();
        }
    }

    private async void Backbtn_Clicked(object sender, EventArgs e)
    {
        if (_isUpdating) return;

        if (CurrentQuestion != null)
        {
            CurrentQuestion.ColourBorder = Colors.White;
            CurrentQuestion.ShowRequired = false;
        }
        FooterRequiredLabel.IsVisible = false;

        if (rownumber > 0)
        {
            rownumber--;
            await UpdateQuestionUI();
        }
    }

    private bool ValidateDate(string? date, QuestionAnswerJson Question)
    {
        if (!DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            ShowHideError(Question, "Invalid date, must be 'DD/MM/YYYY'", true); 
            return false;
        }

        DateTime today = DateTime.Today;

        if (parsedDate.Date > today)
        {
            ShowHideError(Question, "Please enter a date that is not in the future", true);
            return false;
        }
        if (parsedDate.Date < today.AddYears(-100))
        {
            ShowHideError(Question, "Please enter a date that is not 100 years or more in the past", true);
            return false;
        }

        return true;
    }

    private bool ValidateText(string? text, QuestionAnswerJson Question)
    {
        if (string.IsNullOrEmpty(text))
        {
            ShowHideError(Question, "Please enter a value", true);
            return false;
        }
        return true; 
    }

    private async Task ShowHideError(QuestionAnswerJson Question, string msg, bool Show = false)
    {
        if(Question is not null)
        {
            Question.Haserror = Show;
            var firstOption = Question.options.FirstOrDefault();
            if(firstOption is not null)
            {
                firstOption.Errortext = msg;
                firstOption.Haserror = Show;
            }
        }
    }

    private bool ValidateCurrentQuestion()
    {
        var q = CurrentQuestion;
        if (q == null) return true;


        if (q.type != "date" && !q.required) return true;

        string? textValue = q.options?.FirstOrDefault()?.text;

        // Optional date questions can be empty, but invalid dates should not be saved
        if (q.type == "date" && string.IsNullOrEmpty(textValue) && !q.required)
            return true;

        bool answered = q.type switch
        {
            //"picture" => q.HasImage,
            "date" =>  ValidateDate(textValue, q),
            "text" =>  ValidateText(textValue, q),
            _ => q.options?.Any(o => o.selected) ?? false
        };

        q.HasAnswered = answered;
        if (!q.Haserror)
        {
            q.ColourBorder = answered ? Colors.White : Colors.Red;
            q.ShowRequired = !answered;
            FooterRequiredLabel.IsVisible = !answered;
        }
       

        if (!answered) Vibration.Vibrate();

        return answered;
    }

    private void MauiRadio_StateChanged(object sender, Syncfusion.Maui.Buttons.StateChangedEventArgs e)
    {
        try
        {
            if (_isUpdating) return;

            if (e.IsChecked == true && sender is SfRadioButton rb && rb.BindingContext is Option selectedOption)
            {
                if (CurrentQuestion?.options != null)
                {
                    foreach (var opt in CurrentQuestion.options)
                        opt.selected = (opt.answerid == selectedOption.answerid);

                    CurrentQuestion.ShowRequired = false;
                    CurrentQuestion.ColourBorder = Colors.White;
                    CurrentQuestion.HasAnswered = true;
                    FooterRequiredLabel.IsVisible = false;

                    RefreshFooterState();
                }
            }
        }
        catch (Exception Ex) { CrashDetected.LogCrash(Ex, Navigation, "MauiRadio_StateChanged"); }
    }

    private async void TakePhoto_Clicked(object sender, EventArgs e)
    {
        await CapturePicture(useCamera: true);
    }

    private async void ChooseFromGallery_Clicked(object sender, EventArgs e)
    {
        await CapturePicture(useCamera: false);
    }

    private async Task CapturePicture(bool useCamera)
    {
        try
        {
            FileResult photo = useCamera
                ? await MediaPicker.Default.CapturePhotoAsync()
                : await MediaPicker.Default.PickPhotoAsync();

            if (photo == null) return;

            var localPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var sourceStream = await photo.OpenReadAsync())
            using (var localFileStream = File.OpenWrite(localPath))
            {
                await sourceStream.CopyToAsync(localFileStream);
            }

            if (CurrentQuestion != null)
            {
                CurrentQuestion.image = localPath;
                CurrentQuestion.HasAnswered = true;
                CurrentQuestion.ShowRequired = false;
                CurrentQuestion.ColourBorder = Colors.White;
                FooterRequiredLabel.IsVisible = false;

                //var blobPath = await UploadPictureAsync(localPath);
                //CurrentQuestion.UploadedFileName = blobPath;
                //_imageFilename = blobPath;

                RefreshFooterState();
            }
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "CapturePicture");
        }
    }

    private async Task<string> UploadPictureAsync(string localPath)
    {
        var random = new Random();
        var randomNum = random.Next(10000, 999999);
        var imageName = $"{Helpers.Settings.UsersID}_{DateTime.Now:yyyyMMdd}_b1_samples_{randomNum}.png";
        var blobPath = $"testresults/{imageName}";
        var connectionString = "DefaultEndpointsProtocol=https;AccountName=peoplewithappiamges;AccountKey=9maBMGnjWp6KfOnOuXWHqveV4LPKyOnlCgtkiKQOeA+d+cr/trKApvPTdQ+piyQJlicOE6dpeAWA56uD39YJhg==;EndpointSuffix=core.windows.net";
        var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient("imperial");
        var blobClient = containerClient.GetBlobClient(blobPath);
        using (var stream = File.OpenRead(localPath))
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        return blobPath;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            loadingstack.IsVisible = true;
            InitialPage.IsVisible = false;
            LoadFailed.IsVisible = false;
            datastack.IsVisible = false; 
            await Task.Delay(200);
            loadingstack.IsVisible = false; 
            datastack.IsVisible = true; 
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "Button_Clicked");
            loadingstack.IsVisible = false;
            datastack.IsVisible = false; 
            LoadFailed.IsVisible = true;
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (_isUpdating) return;
            if (sender is Border cb && cb.BindingContext is Option opt)
            {
                opt.selected = !opt.selected;

                if (CurrentQuestion != null)
                {
                    CurrentQuestion.HasAnswered = CurrentQuestion.options?.Any(o => o.selected) ?? false;
                    CurrentQuestion.ShowRequired = false;
                    CurrentQuestion.ColourBorder = Colors.White;
                    FooterRequiredLabel.IsVisible = false;

                    RefreshFooterState();
                }
            }
        }
        catch (Exception Ex) { CrashDetected.LogCrash(Ex, Navigation, "TapGestureRecognizer_Tapped"); }
    }

    private async void CompletedCollectionView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        var ItemTapped = e.DataItem as QuestionAnswerJson;
        if (ItemTapped is not null)
        {
            var Type = ItemTapped.type; 
            if (Type == "picture" || Type == "file_upload")
            {
                if (!string.IsNullOrEmpty(ItemTapped.image))
                {
                    await MopupService.Instance.PushAsync(new ShowImage(ItemTapped.imageURI), false);
                }
            }
        }
    }

}