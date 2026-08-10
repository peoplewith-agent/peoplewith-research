//using CommunityToolkit.Mvvm.Messaging;
//using FreakyKit.Utils;
//using Mopups.Services;
//using Syncfusion.Maui.Buttons;
//using System.Collections.ObjectModel;
//using CommunityToolkit.Maui.Behaviors;
//using CommunityToolkit.Mvvm.Messaging;
//using Microsoft.Maui.Controls.Shapes;
//using Mopups.Services;
//using System;
//using System.Collections.ObjectModel;
//using System.Globalization;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using System.Threading.Tasks;

//namespace PeopleWithResearch;

//public partial class UpdatedQuestionnairesPage : ContentPage
//{
//    public newuserquestionnaire completeuserquestionnaire { get; set; } = new();
//    public ObservableCollection<newuserquestionnaire> alluserquestionnaires = new();
//    public questionnaires questionnairecontent { get; set; } = new();
//    public ObservableCollection<confirmationmessage> DefaultMessage = new();

//    public int rownumber = 0;
//    public newuserquestionnaire SelectedAnswerList = new();

//    private string QuestionidPassed;
//    private bool Failedtoload = false;
//    private bool _isUpdating = false;
//    private bool SubmitAnswered = true;
//    private bool logoutAction = false;
//    public string imageURL = string.Empty; 

//    public QuestionAnswerJson CurrentQuestion
//    {
//        get
//        {
//            var visible = GetVisibleQuestions();
//            return (visible != null && rownumber < visible.Count) ? visible[rownumber] : null;
//        }
//    }

//    private async Task<string> FetchJsonAsync()
//    {
//        using var stream = await FileSystem.OpenAppPackageFileAsync("t1formjson.json");
//        using var reader = new StreamReader(stream);
//        return await reader.ReadToEndAsync();
//    }

//    public UpdatedQuestionnairesPage(string questionnaireid, ObservableCollection<newuserquestionnaire> AllQuestionnaires)
//    {
//        InitializeComponent();
//        alluserquestionnaires = AllQuestionnaires;
//        QuestionidPassed = questionnaireid;
//        getquestionnairedetails(QuestionidPassed);
//    }

//    public UpdatedQuestionnairesPage(newuserquestionnaire questionnairePassed)
//    {
//        InitializeComponent();
//        if (questionnairePassed?.questionnaireid is null)
//        {
//            loadingstack.IsVisible = false;
//            datastack.IsVisible = true;
//            LoadFailed.IsVisible = true;
//            CompletedScrollView.IsVisible = false;
//            return; 
//        }
//        completeuserquestionnaire = questionnairePassed;
//        PopulateCompleted(completeuserquestionnaire.questionnaireid);
//    }

//    async public void CrashDetected(Exception Ex)
//    {
//        try { } catch (Exception) { }
//    }

//    async void PopulateCompleted(string questionnaireid)
//    {
//        try
//        {
//            loadingstack.IsVisible = true;
//            datastack.IsVisible = false;
//            QuestionidPassed = questionnaireid;

//            await Task.Delay(10);

//            var Yeet = new ObservableCollection<questionnaires>();

//            if (QuestionidPassed == "t1_form")
//            {
//                var json = await FetchJsonAsync();

//                if (!string.IsNullOrWhiteSpace(json))
//                {
//                    var questionAnswer = APICalls.Instance.DeserializeNestedJson<QuestionAnswerJson>(json);

//                    Yeet = new ObservableCollection<questionnaires>
//                    {
//                        new questionnaires
//                        {
//                            QuestionAnswerJson = questionAnswer,
//                            QuestionAnswerJsonRaw = json
//                        }
//                    };
//                }
//            }
//            else
//            {
//                Yeet = await APICalls.Instance.GetSingleQuestionnaire(QuestionidPassed);
//            }

//            if (Yeet != null && Yeet.Count > 0)
//            {
//                questionnairecontent = Yeet[0];
//                completedquestionnairetitlelbl.IsVisible = true;
//                completedquestionnairetitlelbl.Text = questionnairecontent.title;


//                if (!string.IsNullOrEmpty(completeuserquestionnaire.imagefilename))
//                {
//                    Showimage.IsVisible = true;
//                    imageURL = $"https://peoplewfet/imperial/{completeuserquestionnaire.imagefilename}";
//                    lblImageName.Text = completeuserquestionnaire.imagefilename.Replace("testresults/", "");
//                    userimage.Source = new UriImageSource
//                    {
//                        Uri = new Uri(imageURL),
//                        CachingEnabled = true,
//                        CacheValidity = TimeSpan.FromDays(7)
//                    };
//                }

//                if (QuestionidPassed == "t1_form")
//                {
//                    var checklbl = questionnairecontent.QuestionAnswerJson.FirstOrDefault();

//                    if (checklbl != null)
//                    {
//                        completedquestionnairetitlelbl.Text = checklbl.section_label;
//                    }
//                    else
//                    {
//                        completedquestionnairetitlelbl.IsVisible = false;
//                    }
//                }

//                if (questionnairecontent?.QuestionAnswerJson == null || completeuserquestionnaire?.FeedbackList == null)
//                    return;
//                try
//                {
//                    var processedQuestions = await ReturnCompleted();
//                    BindableLayout.SetItemsSource(CompletedQuestionsLayout, processedQuestions);
//                }
//                catch (Exception ex)
//                {
//                    await DisplayAlert("Error", ex.Message + "\n" + ex.StackTrace, "OK");
//                }
//            }
//            else
//            {
//                Failedtoload = true;
//            }
//        }
//        catch (Exception Ex)
//        {
//            CrashDetected(Ex);
//            Failedtoload = true;
//        }
//        finally
//        {
//            loadingstack.IsVisible = false;
//            datastack.IsVisible = true;
//            LoadFailed.IsVisible = Failedtoload;
//            CompletedScrollView.IsVisible = !Failedtoload;
//        }
//    }

//    private async Task<List<QuestionAnswerJson>> ReturnCompleted()
//    {
//        var allUserAnswerIds = completeuserquestionnaire.FeedbackList
//                        .Where(f => f?.answer != null)
//                        .SelectMany(f => f.answer)
//                        .Select(a => a.answerid)
//                        .ToHashSet();

//    var answeredQuestions = questionnairecontent.QuestionAnswerJson
//        .Where(q =>
//            (q.options != null && q.options.Any(opt => allUserAnswerIds.Contains(opt.answerid))) ||
//            q.questionid == "symptom_grid" ||
//            q.questionid == "rash_followup" ||
//            q.questionid == "daily_impact")
//        .OrderBy(q => q.order)
//        .ToList();

//                    for (int i = 0; i<answeredQuestions.Count; i++)
//                    {
//                        var question = answeredQuestions[i];
//    question.questionnum = $"Question {i + 1} of {answeredQuestions.Count}";
//                        question.notcomplete = false;

//                        if (question.questionid != "symptom_grid" &&
//                            question.questionid != "rash_followup" &&
//                            question.questionid != "daily_impact")
//                        {
//                            if (question.options != null)
//                            {
//                                var filteredOptions = new ObservableCollection<Option>();
//                                foreach (var opt in question.options)
//                                {
//                                    bool checkSelected = allUserAnswerIds.Contains(opt.answerid);
//                                    if (checkSelected)
//                                    {
//                                        opt.selectedss = true;
//                                        opt.selectedms = true;
//                                        filteredOptions.Add(opt);
//                                    }
//                                }
//                                question.options = filteredOptions.ToArray();
//                            }
//                        }

//                        if (question?.symptom_groups != null)
//{
//    var allSymptoms = question.symptom_groups
//        .Where(g => g?.symptoms != null)
//        .SelectMany(g => g.symptoms);

//    foreach (var symptom in allSymptoms)
//    {
//        var matchingFeedbacks = completeuserquestionnaire.FeedbackList
//            .Where(f =>
//            {
//                if (f?.questionid == null)
//                    return false;

//                var parts = f.questionid.Split('_');
//                var extractedSymptomId = string.Join("_", parts.Skip(1));
//                return extractedSymptomId == symptom.id;
//            }) ?? Enumerable.Empty<Feedback>();

//        var processedSymptomData = matchingFeedbacks
//            .Where(f => f.answer != null)
//            .SelectMany(f => f.answer)
//            .Select(p =>
//            {
//                if (p.answerid == null)
//                    return null;

//                var parts = p.answerid.Split('_');
//                var dayPrefix = parts.FirstOrDefault();
//                var symptomKey = string.Join("_", parts.Skip(1));

//                if (symptomKey != symptom.id)
//                    return null;

//                var matchingOption = question.day_tabs?.FirstOrDefault(o => o.id == dayPrefix);
//                var matchSeverity = question.severity_options?.FirstOrDefault(o => o.value == p.answervalue?.ToString());

//                return new symptomdata
//                {
//                    id = p.answerid,
//                    value = p.answervalue,
//                    label = matchingOption?.label,
//                    text = matchSeverity?.text,
//                    colour = ColourSwitch(p.answervalue?.ToString())
//                };
//            })
//            .Where(data => data != null)
//            .ToList();

//        symptom.SymptomData = new ObservableCollection<symptomdata>(processedSymptomData);
//    }

//    var rashQuestions = answeredQuestions
//        .Where(q => q.questionid == "symptom_grid" && q.rash_followup?.questions != null)
//        .SelectMany(q => q.rash_followup.questions);

//    foreach (var data in rashQuestions)
//    {
//        if (data.options == null) continue;

//        var filteredRashOptions = new ObservableCollection<Option>();
//        foreach (var op in data.options)
//        {
//            bool isSelected = allUserAnswerIds.Contains(op.answerid) ||
//                (question.day_tabs != null && question.day_tabs.Any(tab => allUserAnswerIds.Contains($"{tab.id}_{op.answerid}")));

//            if (isSelected)
//            {
//                op.selectedms = true;
//                op.selectedss = true;
//                filteredRashOptions.Add(op);
//            }
//        }
//        data.options = filteredRashOptions.ToArray(); ;
//    }

//    var dailyQuestions = answeredQuestions
//        .Where(q => q.questionid == "symptom_grid" && q.daily_impact?.questions != null)
//        .SelectMany(q => q.daily_impact.questions);

//    foreach (var data in dailyQuestions)
//    {
//        if (data.options == null) continue;

//        var filteredDailyOptions = new ObservableCollection<Option>();
//        foreach (var op in data.options)
//        {
//            bool isSelected = allUserAnswerIds.Contains(op.answerid) ||
//                (question.day_tabs != null && question.day_tabs.Any(tab => allUserAnswerIds.Contains($"{tab.id}_{op.answerid}")));

//            if (isSelected)
//            {
//                op.selectedms = true;
//                op.selectedss = true;
//                filteredDailyOptions.Add(op);
//            }
//        }
//        data.options = filteredDailyOptions.ToArray();
//    }
//}
//                    }
//                    return answeredQuestions;
//    }
        

//    private static Color ColourSwitch(string jsonValue) => jsonValue switch
//    {
//        "1" => Color.FromHex("#CFD8DC"),
//        "2" => Color.FromHex("#FFF9C4"),
//        "3" => Color.FromHex("#FFCC80"),
//        "4" => Color.FromHex("#EF9A9A"),
//        _ => Color.FromHex("#CFD8DC")
//    };

//    async void getquestionnairedetails(string questionnaireid)
//    {
//        try
//        {
//            loadingstack.IsVisible = true;
//            var result = await APICalls.Instance.GetSingleQuestionnaire(QuestionidPassed);

//            if (result?.Count > 0)
//            {
//                questionnairecontent = result[0];
//                HeaderTitle.Text = questionnairecontent.title;
//                Questiontitle.Text = questionnairecontent.title;
//                QuestionDescription.Text = questionnairecontent.description;

//                if (questionnairecontent.QuestionAnswerJson?.Count > 0)
//                {
//                    rownumber = 0;
//                    await UpdateQuestionUI();
//                }
//            }
//            else
//            {
//                Failedtoload = true;
//            }
//        }
//        catch (Exception ex)
//        {
//            CrashDetected(ex);
//        }
//        finally
//        {
//            loadingstack.IsVisible = false;
//            InitialPage.IsVisible = !Failedtoload;
//            LoadFailed.IsVisible = Failedtoload;
//        }
//    }
//    private List<QuestionAnswerJson> GetVisibleQuestions()
//    {
//        if (questionnairecontent?.QuestionAnswerJson == null)
//            return new List<QuestionAnswerJson>();

//        var all = questionnairecontent.QuestionAnswerJson;

//        var selectedAnswerIds = all
//            .Where(q => q.options != null)
//            .SelectMany(q => q.options)
//            .Where(o => o.selectedss || o.selectedms)
//            .Select(o => o.answerid)
//            .ToHashSet(StringComparer.OrdinalIgnoreCase);

//        if (completeuserquestionnaire?.FeedbackList != null)
//        {
//            var historicalIds = completeuserquestionnaire.FeedbackList
//                .Where(f => f?.answer != null)
//                .SelectMany(f => f.answer)
//                .Where(a => !string.IsNullOrEmpty(a.answerid))
//                .Select(a => a.answerid);

//            foreach (var id in historicalIds)
//            {
//                selectedAnswerIds.Add(id);
//            }
//        }

//        return all.Where(q =>
//            string.IsNullOrWhiteSpace(q.branchinglogic) ||
//            q.branchinglogic.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
//                .Any(logicId =>
//                {
//                    var trimmed = logicId.Trim();

//                    if (selectedAnswerIds.Contains(trimmed))
//                        return true;

//                    return selectedAnswerIds.Any(selected =>
//                        selected.StartsWith(trimmed + "_", StringComparison.OrdinalIgnoreCase) ||
//                        selected.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
//                })
//        )
//        .OrderBy(q => q.order)
//        .ToList();
//    }
//    private async Task UpdateQuestionUI()
//    {
//        _isUpdating = true;
//        try
//        {
//            var visibleQuestions = GetVisibleQuestions();
//            if (visibleQuestions.Count == 0) return;

//            if (rownumber >= visibleQuestions.Count)
//            {
//                rownumber = Math.Max(0, visibleQuestions.Count - 1);
//            }

//            var currentQuestion = visibleQuestions[rownumber];
//            currentQuestion.questionnum = $"Question {rownumber + 1} of {visibleQuestions.Count}";

//            bool isFirst = rownumber == 0;
//            bool isLast = rownumber == visibleQuestions.Count - 1;

//            await MainThread.InvokeOnMainThreadAsync(() =>
//            {
//                var templateSelector = (DataTemplateSelector)Resources["QuestionnaireSelector"];
//                var template = templateSelector.SelectTemplate(currentQuestion, this);
//                if (template != null)
//                {
//                    var view = (View)template.CreateContent();
//                    view.BindingContext = currentQuestion;
//                    ActiveQuestionPresenter.Content = view;
//                }

//                FooterBackBtn.IsVisible = !isFirst;
//                FooterNextBtn.IsVisible = !isLast;
//                FooterSubmitBtn.IsVisible = isLast && currentQuestion.HasAnswered;
//                FooterRequiredLabel.IsVisible = currentQuestion.ShowRequired;
//            });
//        }
//        catch (Exception ex)
//        {
//            CrashDetected(ex);
//        }
//        finally
//        {
//            _isUpdating = false;
//        }
//    }
//    private async void Nextbtn_Clicked(object sender, EventArgs e)
//    {
//        if (_isUpdating) return;
//        var visibleCount = GetVisibleQuestions().Count;

//        if (CurrentQuestion != null && CurrentQuestion.required)
//        {
//            bool answered = CurrentQuestion.HasAnswered;
//            CurrentQuestion.ColourBorder = answered ? Colors.White : Colors.Red;
//            CurrentQuestion.ShowRequired = !answered;
//            FooterRequiredLabel.IsVisible = !answered;

//            if (!answered)
//            {
//                Vibration.Vibrate();
//                return;
//            }
//        }

//        if (rownumber < visibleCount - 1)
//        {
//            rownumber++;
//            await UpdateQuestionUI();
//        }
//    }

//    private async void backbtn_Clicked(object sender, EventArgs e)
//    {
//        if (_isUpdating) return;

//        if (CurrentQuestion != null)
//        {
//            CurrentQuestion.ColourBorder = Colors.White;
//            CurrentQuestion.ShowRequired = false;
//        }
//        FooterRequiredLabel.IsVisible = false;

//        if (rownumber > 0)
//        {
//            rownumber--;
//            await UpdateQuestionUI();
//        }
//    }

//    private async void submitbtn_Clicked(object sender, EventArgs e)
//    {
//        try
//        {
//            SubmitAnswered = true;
//            var visibleCount = GetVisibleQuestions().Count;

//            if (CurrentQuestion != null && CurrentQuestion.required)
//            {
//                SubmitAnswered = CurrentQuestion.HasAnswered;
//                CurrentQuestion.ColourBorder = SubmitAnswered ? Colors.White : Colors.Red;
//                CurrentQuestion.ShowRequired = !SubmitAnswered;
//                FooterRequiredLabel.IsVisible = !SubmitAnswered;

//                if (!SubmitAnswered)
//                {
//                    Vibration.Vibrate();
//                    return;
//                }
//            }

//            var Confirm = new confirmationmessage()
//            {
//                confirmationmessageid = "1",
//                confirmationmessagetitle = "Thank you for completing the HOPPER Study Samples, Symptoms and changes Form. Please ensure the remaining individuals in your household complete their forms and please continue to sample and complete the forms daily.",
//                action = "complete"
//            };

//            if (questionnairecontent.Confirmationmessage.Count == 0)
//            {
//                questionnairecontent.Confirmationmessage.Add(Confirm);
//            }

//            var ConfirmMess = questionnairecontent.Confirmationmessage.FirstOrDefault();
//            if (ConfirmMess.action == "image-upload")
//            {
//                bool checkQuestion = questionnairecontent.QuestionAnswerJson?
//                    .SelectMany(q => q.options ?? Array.Empty<Option>())
//                    .Any(op => op.answerid == ConfirmMess.answerid && (op.selectedss || op.selectedms)) ?? false;

//                if (!checkQuestion)
//                {
//                    questionnairecontent.Confirmationmessage.Clear();
//                    questionnairecontent.Confirmationmessage.Add(Confirm);
//                }
//            }

//            var tcs = new TaskCompletionSource<string>();
//            await MopupService.Instance.PushAsync(new ConfirmMessage(questionnairecontent.Confirmationmessage, tcs) { });
//            string QuestionAction = await tcs.Task;

//            var submission = new newuserquestionnaire
//            {
//                questionnaireid = questionnairecontent.questionnaireid,
//                userid = Helpers.Settings.UsersID,
//                FeedbackList = new ObservableCollection<Feedback>(),
//            };

//            if (QuestionAction.Contains(".png"))
//            {
//                submission.imagefilename = QuestionAction;
//            }

//            foreach (var question in questionnairecontent.QuestionAnswerJson)
//            {
//                var actualQuestionId = !string.IsNullOrEmpty(question.id) ? question.id : question.questionid;

//                var feedback = new Feedback
//                {
//                    questionid = actualQuestionId,
//                    answer = new ObservableCollection<Answer>(
//                        question.options?
//                            .Where(o => o.selectedss || o.selectedms)
//                            .Select(o => new Answer { answerid = o.answerid, answervalue = o.value, text = o.text, })
//                        ?? Enumerable.Empty<Answer>()
//                    )
//                };
//                submission.DateTimeAdded = DateTime.Now;
//                submission.FeedbackList.Add(feedback);
//                submission.feedback = System.Text.Json.JsonSerializer.Serialize(submission.FeedbackList);
//            }
          
//            SelectedAnswerList = submission;
//            SelectedAnswerList = await APICalls.Instance.PostUserQuestionnaire(SelectedAnswerList);

//            alluserquestionnaires.Add(SelectedAnswerList);
//            WeakReferenceMessenger.Default.Send(new UpdateDashCompelted(alluserquestionnaires));

//            if (QuestionAction == "logout")
//            {
//                logoutAction = true;             
//            }
//            else
//            {
//                Navigation.RemovePage(this);
//            }
//        }
//        catch (Exception ex)
//        {
//            CrashDetected(ex);
//        }
//        finally
//        {
//            if (SubmitAnswered)
//            {
//                await MopupService.Instance.PushAsync(new PopupPageHelper("Questionnaire Completed"));
//                await Task.Delay(3000);
//                if (logoutAction)
//                {
//                    var changes = new Dictionary<string, object> { { "status", "Withdrawn" } };
//                    bool success = await APICalls.Instance.UpdateUserData(Helpers.Settings.UsersID, changes);
//                    if (success)
//                    {
//                        Newlogout HandleLogout = new Newlogout("Logout");
//                    }
//                }
//                await MopupService.Instance.PopAllAsync(false);
//            }         
//        }
//    }

//    private async void ExtendedCheckbox_StateChanged(object sender, Syncfusion.Maui.Buttons.StateChangedEventArgs e)
//    {
//        try
//        {
//            if (_isUpdating) return;
//            if (sender is ExtendedCheckbox cb && cb.BindingContext is Option opt)
//            {
//                opt.selectedms = e.IsChecked ?? false;

//                if (CurrentQuestion != null)
//                {
//                    CurrentQuestion.HasAnswered = CurrentQuestion.options?.Any(o => o.selectedms) ?? false;
//                    CurrentQuestion.ShowRequired = false;
//                    CurrentQuestion.ColourBorder = Colors.White;
//                    FooterRequiredLabel.IsVisible = false;

//                    await UpdateQuestionUI();
//                }
//            }
//        }
//        catch (Exception Ex)
//        {
//            CrashDetected(Ex);
//        }
//    }

//    private async void MauiRadio_StateChanged(object sender, Syncfusion.Maui.Buttons.StateChangedEventArgs e)
//    {
//        try
//        {
//            if (_isUpdating) return;

//            if (e.IsChecked == true && sender is SfRadioButton rb && rb.BindingContext is Option selectedOption)
//            {
//                if (CurrentQuestion?.options != null)
//                {
//                    foreach (var opt in CurrentQuestion.options)
//                    {
//                        opt.selectedss = (opt.answerid == selectedOption.answerid);
//                    }
//                    CurrentQuestion.ShowRequired = false;
//                    CurrentQuestion.ColourBorder = Colors.White;
//                    CurrentQuestion.HasAnswered = true;
//                    FooterRequiredLabel.IsVisible = false;

//                    await UpdateQuestionUI();
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            CrashDetected(ex);
//        }
//    }

//    private async void DateEntry_TextChanged(object sender, TextChangedEventArgs e)
//    {
//        try
//        {
//            if (CurrentQuestion != null)
//            {
//                bool HasValue = !string.IsNullOrWhiteSpace(e.NewTextValue);
//                CurrentQuestion.HasAnswered = HasValue;
//                CurrentQuestion.ShowRequired = false;
//                CurrentQuestion.ColourBorder = Colors.White;
//                FooterRequiredLabel.IsVisible = false;

//                await UpdateQuestionUI();
//            }
//        }
//        catch (Exception ex)
//        {
//            CrashDetected(ex);
//        }
//    }

//    private void Button_Clicked(object sender, EventArgs e)
//    {
//        try
//        {
//            datastack.IsVisible = true;
//            InitialPage.IsVisible = false;
//            MainCollectionview.IsVisible = !Failedtoload;
//            LoadFailed.IsVisible = Failedtoload;
//        }
//        catch (Exception Ex)
//        {
//            CrashDetected(Ex);
//        }
//    }

//    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
//    {
//        try
//        {
//            await MopupService.Instance.PushAsync(new ShowImage(imageURL), false);
//        }
//        catch(Exception Ex)
//        {

//        }
//    }
//}