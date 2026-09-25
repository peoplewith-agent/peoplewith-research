using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using Syncfusion.Maui.Buttons;

namespace PeopleWithResearch;

public partial class B1Questionnaire : ContentPage
{
    private List<B1Question> _allQuestions = new();
    public ObservableCollection<newuserquestionnaire> alluserquestionnaires = new();
    private int rownumber = 0;
    private bool _isUpdating = false;

    // TODO: wire this to your actual primary-user check, e.g. Helpers.Settings.IsPrimaryUser
    private bool IsPrimaryUser = true;

    public B1Question CurrentQuestion
    {
        get
        {
            var visible = GetVisibleQuestions();
            return (visible != null && rownumber < visible.Count) ? visible[rownumber] : null;
        }
    }

    public B1Questionnaire(ObservableCollection<newuserquestionnaire> questionnairesPassed)
    {
        InitializeComponent();
        alluserquestionnaires = questionnairesPassed;
        LoadQuestionnaire();
    }

    private async Task<string> FetchJsonAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("b1_samples.json");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private async void LoadQuestionnaire()
    {
        try
        {
            loadingstack.IsVisible = true;
            datastack.IsVisible = false;

 string questionnaireId = "DDD843CF-021B-4557-8824-13C5B4E2EA85";
            var jsonList = await APICalls.Instance.GetSingleQuestionnaire(questionnaireId);

            var QuestionnaireItem = jsonList?.FirstOrDefault();
            if (QuestionnaireItem?.QuestionAnswerJsonRaw != null)
            {
                _allQuestions = JsonSerializer.Deserialize<List<B1Question>>(QuestionnaireItem.QuestionAnswerJsonRaw) ?? new List<B1Question>();   
            }


            // var json = await FetchJsonAsync();
            // _allQuestions = JsonSerializer.Deserialize<List<B1Question>>(json) ?? new List<B1Question>();

            rownumber = 0;
            await UpdateQuestionUI();
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "LoadQuestionnaire");
        }
        finally
        {
            loadingstack.IsVisible = false;
            datastack.IsVisible = true;
        }
    }

    private List<B1Question> GetVisibleQuestions()
    {
        if (_allQuestions == null) return new List<B1Question>();

        IsPrimaryUser = !string.IsNullOrWhiteSpace(Helpers.Settings.PrimaryUserID);
               
        var usertypeFiltered = _allQuestions
            .Where(q => q.usertype == "all" || (q.usertype == "primaryuser" && IsPrimaryUser))
            .ToList();

        var selectedAnswerIds = usertypeFiltered
            .Where(q => q.options != null)
            .SelectMany(q => q.options)
            .Where(o => o.selected)
            .Select(o => o.answerid)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return usertypeFiltered.Where(q =>
            string.IsNullOrWhiteSpace(q.branchinglogic) ||
            q.branchinglogic.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(logicId => selectedAnswerIds.Contains(logicId.Trim())))
            .OrderBy(q => q.order)
            .ToList();
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

            var templateSelector = (DataTemplateSelector)Resources["B1Selector"];
            var template = templateSelector.SelectTemplate(current, this);
            if (template != null)
            {
                var view = (View)template.CreateContent();
                view.BindingContext = current;
                ActiveQuestionPresenter.Content = view;
            }

            FooterBackBtn.IsVisible = !isFirst;
            FooterNextBtn.IsVisible = !isLast;
            FooterSubmitBtn.IsVisible = isLast && current.HasAnswered;
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

    private bool ValidateCurrentQuestion()
    {
        var q = CurrentQuestion;
        if (q == null || !q.required) return true;

        bool answered = q.type switch
        {
            "picture" => q.HasImage,
            _ => q.options?.Any(o => o.selected) ?? false
        };

        q.HasAnswered = answered; // keep the flag in sync too, since footer logic still reads it
        q.ColourBorder = answered ? Colors.White : Colors.Red;
        q.ShowRequired = !answered;
        FooterRequiredLabel.IsVisible = !answered;

        if (!answered)
            Vibration.Vibrate();

        return answered;
    }

    // Multiselection
    private void ExtendedCheckbox_StateChanged(object sender, Syncfusion.Maui.Buttons.StateChangedEventArgs e)
    {
        try
        {
            if (_isUpdating) return;
            if (sender is ExtendedCheckbox cb && cb.BindingContext is B1Option opt)
            {
                opt.selected = e.IsChecked ?? false;

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
        catch (Exception Ex) { CrashDetected.LogCrash(Ex, Navigation, "ExtendedCheckbox_StateChanged"); }
    }

    // Dropdown (rendered as a single-select list)
    private void MauiRadio_StateChanged(object sender, Syncfusion.Maui.Buttons.StateChangedEventArgs e)
    {
        try
        {
            if (_isUpdating) return;

            if (e.IsChecked == true && sender is SfRadioButton rb && rb.BindingContext is B1Option selectedOption)
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

    // Picture
    private async void TakePhoto_Clicked(object sender, EventArgs e)
    {
        await CapturePicture(useCamera: true);
    }

    private async void ChooseFromGallery_Clicked(object sender, EventArgs e)
    {
        await CapturePicture(useCamera: false);
    }

    private string _imageFilename; // tracks the blob path, mirrors T1Answers._imageFilename

    private async Task CapturePicture(bool useCamera)
    {
        try
        {
            FileResult photo = useCamera
                ? await MediaPicker.Default.CapturePhotoAsync()
                : await MediaPicker.Default.PickPhotoAsync();

            if (photo == null) return;

            // On Android, photo.FileName can be a full content-URI path or null.
            // Use Path.GetFileName to strip any directory components, and fall back
            // to a timestamped name to avoid an invalid path crash.
            var safeFileName = string.IsNullOrWhiteSpace(photo.FileName)
                ? $"photo_{DateTime.Now:yyyyMMddHHmmss}.jpg"
                : Path.GetFileName(photo.FileName);

            var cacheDir = FileSystem.CacheDirectory;
            Directory.CreateDirectory(cacheDir); // no-op if it already exists (safe on iOS too)
            var localPath = Path.Combine(cacheDir, safeFileName);

            using (var sourceStream = await photo.OpenReadAsync())
            using (var localFileStream = File.OpenWrite(localPath))
            {
                await sourceStream.CopyToAsync(localFileStream);
            }

            if (CurrentQuestion != null)
            {
                CurrentQuestion.LocalImagePath = localPath;
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
        // Same container, folder, and filename pattern as T1QuestionnairePage.BuildFileCard
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

    private async void Submitbtn_Clicked(object sender, EventArgs e)
    {
        PopupPageHelper uploadingPopup = null;

        try
        {
            if (!ValidateCurrentQuestion()) return;

            FooterSubmitBtn.IsEnabled = false;

            uploadingPopup = new PopupPageHelper("Uploading Questionnaire...");
            await MopupService.Instance.PushAsync(uploadingPopup);

            var pictureQuestion = _allQuestions.FirstOrDefault(q => q.type == "picture");
            if (pictureQuestion != null && !string.IsNullOrEmpty(pictureQuestion.LocalImagePath))
            {
                try
                {
                    var blobPath = await UploadPictureAsync(pictureQuestion.LocalImagePath);
                    pictureQuestion.UploadedFileName = blobPath;
                    _imageFilename = blobPath;
                }
                catch (Exception uploadEx)
                {
                    await MopupService.Instance.PopAsync();
                    await DisplayAlert("Error", "Could not upload image. Please try submitting again. " + uploadEx.Message, "OK");
                    return;
                }
            }

            var feedbackList = new ObservableCollection<Feedback>();

            foreach (var q in _allQuestions)
            {
                var answers = new ObservableCollection<Answer>();

                if (q.type == "picture")
                {
                    if (!string.IsNullOrEmpty(q.UploadedFileName))
                    {
                        answers.Add(new Answer
                        {
                            answerid = q.questionid,
                            answervalue = q.UploadedFileName,
                            text = q.UploadedFileName
                        });
                    }
                }
                else if (q.options != null)
                {
                    foreach (var opt in q.options.Where(o => o.selected))
                    {
                        answers.Add(new Answer
                        {
                            answerid = opt.answerid,
                            answervalue = opt.value,
                            text = opt.text
                        });
                    }
                }

                if (answers.Any())
                {
                    feedbackList.Add(new Feedback
                    {
                        questionid = q.questionid,
                        answer = answers
                    });
                }
            }

            var submission = new newuserquestionnaire
            {
                questionnaireid = "b1_samples",
                userid = Helpers.Settings.UsersID,
                FeedbackList = feedbackList,
                DateTimeAdded = DateTime.Now,
                imagefilename = (!string.IsNullOrEmpty(_imageFilename)) ? _imageFilename : string.Empty
            };
            submission.feedback = System.Text.Json.JsonSerializer.Serialize(submission.FeedbackList);

            await APICalls.Instance.PostUserQuestionnaire(submission);

            WeakReferenceMessenger.Default.Send(new UpdateDashCompelted(alluserquestionnaires.ToList()));

            uploadingPopup.UpdateMessage("Successfully Uploaded!");
            await Task.Delay(2000);

            Navigation.RemovePage(this);
            await MopupService.Instance.PopAsync();
        }
        catch (Exception Ex)
        {
            if (MopupService.Instance.PopupStack.Any(p => p == uploadingPopup))
                await MopupService.Instance.PopAsync();

            CrashDetected.LogCrash(Ex, Navigation, "Submitbtn_Clicked");
        }
        finally
        {
            FooterSubmitBtn.IsEnabled = true;
        }
    }
}