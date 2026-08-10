using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls.Shapes;
using Mopups.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace PeopleWithResearch;

public partial class T1Questionnaire : ContentPage
{
    // -- State -----------------------------------------------------------------
    private T1FormModel _model = new();
    private T1Answers _state = new();
    private int _section = 1;
    private int _dayIdx = 0;
    private List<T1DayTabDef> _activeDays = new();
    public ObservableCollection<newuserquestionnaire> alluserquestionnaires = new();
    public householdgroup allhouseholdgroup = new();
    bool dayform;
    private int _dayNumber = 0;
    bool missedquestionnaire;

    // -- Init ------------------------------------------------------------------

    public UserNotifications UpdateNotification = new();

    // T1 form constructor
    public T1Questionnaire(ObservableCollection<newuserquestionnaire> questionnairesPassed, householdgroup housegrroupinfopassed)
    {
        InitializeComponent();
        alluserquestionnaires = questionnairesPassed;
        allhouseholdgroup = housegrroupinfopassed;
        _ = LoadAsync();
    }

    // Daily T form constructor (T2-T28)
    public T1Questionnaire(ObservableCollection<newuserquestionnaire> questionnairesPassed, householdgroup housegroupinfopassed, int dayNumber)
    {
        InitializeComponent();
        dayform = true;
        _dayNumber = dayNumber;
        alluserquestionnaires = questionnairesPassed;
        allhouseholdgroup = housegroupinfopassed;
        missedquestionnaire = false;
        _ = LoadAsync();
    }

    // Daily T form constructor (T2-T28)
    public T1Questionnaire(ObservableCollection<newuserquestionnaire> questionnairesPassed, householdgroup housegroupinfopassed, int dayNumber, bool missedq)
    {
        InitializeComponent();
        dayform = true;
        _dayNumber = dayNumber;
        alluserquestionnaires = questionnairesPassed;
        allhouseholdgroup = housegroupinfopassed;
        missedquestionnaire = missedq;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            SetLoading(true);
            var json = await FetchJsonAsync();
            _model = T1FormModel.FromJson(json);
            SetLoading(false);

            if (dayform)
            {
                string ordinal = GetOrdinal(_dayNumber);
                string title = $"{ordinal} Day of Daily Symptoms and Samples";
                Questiontitle.Text = title;
                QuestionDescription.Text = "Please complete today's symptoms and sample questionnaire. This should take less than 5 minutes.";
                QuestionnaireTitleLabel.Text = title;
            }
            else
            {
                var welcome = _model.Questions.FirstOrDefault(q => q.Type == "info");
                if (welcome != null)
                {
                    Questiontitle.Text = welcome.SectionLabel;
                    QuestionDescription.Text = welcome.Label;
                    QuestionnaireTitleLabel.Text = welcome.SectionLabel;
                }
            }

            InitialPage.IsVisible = true;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "LoadAsync");
            SetLoading(false);
            FailedView.IsVisible = true;
        }
    }

    private string GetOrdinal(int day)
    {
        return day switch
        {
            1 => "1st",
            2 => "2nd",
            3 => "3rd",
            21 => "21st",
            22 => "22nd",
            23 => "23rd",
            _ => $"{day}th"
        };
    }

    private async Task<string> FetchJsonAsync()
    {
        string fileName = dayform ? "tform.json" : "t1formjson.json";
        using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private void SetLoading(bool loading)
    {
        LoadingView.IsVisible = loading;
        ContentView.IsVisible = false;
        InitialPage.IsVisible = false;
        FailedView.IsVisible = false;
        HeaderView.IsVisible = false;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        InitialPage.IsVisible = false;
        ContentView.IsVisible = true;
        HeaderView.IsVisible = true;
        GoToSection(1);
    }

    // -- Section orchestration -------------------------------------------------
    private void GoToSection(int s, bool preserveScroll = false)
    {
        _section = s;
        ValidationBanner.IsVisible = false;

        Sec1View.IsVisible = false;
        Sec2View.IsVisible = false;
        Sec3View.IsVisible = false;
        Sec4View.IsVisible = false;
        Sec5View.IsVisible = false;

        UpdateProgress();
        UpdateNavButtons();

        if (dayform)
        {
            switch (s)
            {
                case 1:
                    DiffSection(Sec1Container, _model.QuestionsForSection(1)
                        .Where(q => q.Id == "t11_symptoms_y_n"));
                    Sec1View.IsVisible = true;
                    break;
                case 4:
                    BuildSection4();
                    Sec4View.IsVisible = true;
                    break;
                case 2:
                    DiffSection(Sec2Container, GetDayFormSection2Questions());
                    Sec2View.IsVisible = true;
                    break;
                case 3:
                    DiffSection(Sec3Container, _model.QuestionsForSection(3));
                    Sec3View.IsVisible = true;
                    break;
            }
        }
        else
        {
            switch (s)
            {
                case 1: DiffSection(Sec1Container, _model.QuestionsForSection(1)); Sec1View.IsVisible = true; break;
                case 2: DiffSection(Sec2Container, _model.QuestionsForSection(2)); Sec2View.IsVisible = true; break;
                case 3: DiffSection(Sec3Container, _model.QuestionsForSection(3)); Sec3View.IsVisible = true; break;
                case 4: BuildSection4(); Sec4View.IsVisible = true; break;
                case 5: DiffSection(Sec5Container, _model.QuestionsForSection(5)); Sec5View.IsVisible = true; break;
            }
        }

        _ = Task.Delay(50).ContinueWith(_ =>
            MainThread.BeginInvokeOnMainThread(() =>
                MainScroll.ScrollToAsync(0, 0, false)));
    }

    // Single place to define what questions appear in daily form section 2
    // Excludes the symptom grid which is shown separately via BuildSection4
    private IEnumerable<T1Question> GetDayFormSection2Questions() =>
        _model.QuestionsForSection(2).Where(q => q.Type != "symptom_grid");

    private void DiffSection(StackLayout container, IEnumerable<T1Question> allQuestions)
    {
        var shouldShow = allQuestions
         .Where(q => !(q.Type == "info" && (q.Section == 1 || dayform)))
         .Where(IsVisible)
         .ToList();

        var shouldShowIds = shouldShow.Select(q => q.Id).ToHashSet();

        var toRemove = container.Children
            .OfType<View>()
            .Where(v => !shouldShowIds.Contains(v.AutomationId))
            .ToList();

        foreach (var view in toRemove)
        {
            container.Children.Remove(view);
            var captured = view;
            _ = captured.FadeTo(0, 120);
        }

        var currentIds = container.Children
            .OfType<View>()
            .Select(v => v.AutomationId)
            .ToList();

        for (int i = 0; i < shouldShow.Count; i++)
        {
            var q = shouldShow[i];

            if (currentIds.Contains(q.Id))
                continue;

            var newCard = BuildQuestion(q);
            newCard.AutomationId = q.Id;

            bool isFirstLoad = currentIds.Count == 0;
            newCard.Opacity = isFirstLoad ? 1 : 0;

            int insertAt = 0;
            for (int j = 0; j < i; j++)
            {
                var precedingId = shouldShow[j].Id;
                var existing = container.Children
                    .OfType<View>()
                    .FirstOrDefault(v => v.AutomationId == precedingId);
                if (existing != null)
                    insertAt = container.Children.IndexOf(existing) + 1;
            }

            insertAt = Math.Min(insertAt, container.Children.Count);
            container.Children.Insert(insertAt, newCard);

            if (!isFirstLoad)
                _ = newCard.FadeTo(1, 180);
        }
    }

    private void UpdateProgress()
    {
        var segments = new[] { Seg1, Seg2, Seg3, Seg4, Seg5 };

        if (dayform)
        {
            Seg3.IsVisible = false;
            Seg4.IsVisible = false;
            Seg5.IsVisible = false;
            Seg1.IsVisible = true;
            Seg2.IsVisible = true;

            // 3 steps: gate question (1) -> symptom grid (4) -> samples (2)
            // Map to 2 progress segments
            int progressStep = _section switch
            {
                1 => 1,
                4 => 2,
                3 => 3,
                _ => 1
            };

            Seg1.Progress = progressStep >= 1 ? 100 : 0;
            Seg2.Progress = progressStep >= 3 ? 100 : 0;
        }
        else
        {
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i].IsVisible = true;
                segments[i].Progress = i < _section ? 100 : 0;
            }
        }
    }

    private void UpdateNavButtons()
    {
        if (dayform)
        {
            bool isSymptomGrid = _section == 4;
            bool isLastDayForm = _section == 3;

            SectionNavBar.IsVisible = !isSymptomGrid;
            NextSectionBtn.IsVisible = !isLastDayForm && !isSymptomGrid;
            SubmitBtn.IsVisible = isLastDayForm;

            NextSectionBtn.Text = _model.NavLabels.Next;
            SubmitBtn.Text = _model.NavLabels.Submit;
            PrevDayBtn.Text = _model.NavLabels.PrevDay;
            backbuttonstack.IsVisible = true;
            return;
        }

        // T1 form
        bool isLast = _section == _model.TotalSections;
        bool isS4 = _section == 4;

        SectionNavBar.IsVisible = !isS4;
        NextSectionBtn.IsVisible = !isLast && !isS4;
        SubmitBtn.IsVisible = isLast;

        NextSectionBtn.Text = _model.NavLabels.Next;
        SubmitBtn.Text = _model.NavLabels.Submit;
        PrevDayBtn.Text = _model.NavLabels.PrevDay;
        backbuttonstack.IsVisible = true;
    }

    // -- Section 4 - Symptoms by Day ------------------------------------------
    private void BuildSection4()
    {
        var grid = _model.SymptomGrid!;

        SeverityLegend.Children.Clear();
        foreach (var sev in grid.SeverityOptions)
        {
            SeverityLegend.Children.Add(new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 5,
                Children =
                {
                    new BoxView
                    {
                        WidthRequest = 12, HeightRequest = 12, CornerRadius = 3,
                        Color = SeverityBg(sev.Value)
                    },
                    new Label { Text = sev.Text, FontSize = 11, TextColor = Color.FromArgb("#62727B") }
                }
            });
        }

        _activeDays = grid.DayTabs
            .Where(dt => EvalDayTabCondition(dt.ShowIf))
            .ToList();

        _dayIdx = 0;

        NextDayBtn.Text = _model.NavLabels.NextDay;

        var key = _activeDays.Count.ToString();
        if (!grid.DayCountIntros.TryGetValue(key, out var introText))
            grid.DayCountIntros.TryGetValue("default", out introText);

        DayIntroCard.IsVisible = !string.IsNullOrEmpty(introText);
        DayIntroLabel.Text = introText ?? "";

        RenderDayStrip();
        RenderCurrentDay();
    }

    // -- Day rendering ---------------------------------------------------------
    private void RenderDayStrip()
    {
        DayTabStrip.Children.Clear();

        for (int i = 0; i < _activeDays.Count; i++)
        {
            var day = _activeDays[i];
            bool done = _state.DayCompleted(day.Id);
            bool active = i == _dayIdx;

            var pill = new Border
            {
                BackgroundColor = active ? Color.FromArgb("#009FE3")
                                : done ? Color.FromArgb("#EAF3DE")
                                         : Colors.White,
                Stroke = active ? Colors.Transparent
                        : done ? Color.FromArgb("#97C459")
                                : Color.FromArgb("#D0D5DD"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(12, 7),
                MinimumWidthRequest = 70,
                HeightRequest = 36,
                Content = new Label
                {
                    Text = day.Label,
                    FontSize = 11,
                    FontFamily = active ? "OpenSansSemiBold" : "OpenSansRegular",
                    TextColor = active ? Colors.White
                               : done ? Color.FromArgb("#27500A")
                                       : Color.FromArgb("#62727B"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };

            DayTabStrip.Children.Add(pill);
        }
    }

    private async Task RenderCurrentDay()
    {
        if (!_activeDays.Any()) return;

        await MopupService.Instance.PushAsync(new Loadingpopup(), false);
        try
        {
            await Task.Delay(50);

            var day = _activeDays[_dayIdx];
            var grid = _model.SymptomGrid!;
            var prevId = _dayIdx > 0 ? _activeDays[_dayIdx - 1].Id : null;

            ActiveDayLabel.Text = day.Label;
            DayProgressLabel.Text = $"{_dayIdx + 1} / {_activeDays.Count}";

            PrevDayBtn.IsVisible = _activeDays.Count > 1 && _dayIdx > 0;
            PrevDayBtn.IsEnabled = _activeDays.Count > 1 && _dayIdx > 0;
            bool isLastDay = _dayIdx == _activeDays.Count - 1;
            NextDayBtn.Text = isLastDay ? _model.NavLabels.LastDay : _model.NavLabels.NextDay;

            SymptomGroupsContainer.Children.Clear();

            var prevDayLabel = prevId != null ? _activeDays[_dayIdx - 1].Label : "";

            foreach (var grp in grid.SymptomGroups)
            {
                var groupStack = new StackLayout { Spacing = 0 };

                groupStack.Children.Add(new Label
                {
                    Text = grp.Group.ToUpperInvariant(),
                    FontSize = 10,
                    FontFamily = "OpenSansSemiBold",
                    TextColor = Color.FromArgb("#62727B"),
                    Margin = new Thickness(0, 0, 0, 4)
                });

                int GroupTotal = grp.Symptoms.Count;
                int GroupIndex = 0;

                foreach (var sym in grp.Symptoms)
                {
                    GroupIndex++;
                    var fieldId = $"{day.Id}_{sym.Id}";
                    var prevFid = prevId != null ? $"{prevId}_{sym.Id}" : null;

                    int current = _state.GetSev(fieldId);
                    int prev = prevFid != null ? _state.GetSev(prevFid) : -1;
                    bool isLast = GroupIndex == GroupTotal;

                    groupStack.Children.Add(
                        BuildSymptomRow(fieldId, sym.Label, grid.SeverityOptions, current, prev, prevDayLabel, sym.HelperText, isLast));
                }

                var groupCard = new Border
                {
                    BackgroundColor = Colors.White,
                    Stroke = Color.FromArgb("#E8ECF0"),
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 10 },
                    Padding = new Thickness(14, 12),
                    Margin = new Thickness(0, 0, 0, 10),
                    Content = groupStack
                };

                SymptomGroupsContainer.Children.Add(groupCard);
            }

            ImpactContainer.Children.Clear();
            if (grid.DailyImpact?.Questions?.Any() == true)
                BuildImpactQuestions(ImpactContainer, day.Id, grid);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "RenderCurrentDay");
            if (MopupService.Instance.PopupStack.Any(p => p is Loadingpopup))
                await MopupService.Instance.PopAsync();
        }
        finally
        {
            if (MopupService.Instance.PopupStack.Any(p => p is Loadingpopup))
                await MopupService.Instance.PopAsync();
        }
    }

    private View BuildSymptomRow(
        string fieldId, string symptomLabel,
        List<T1SeverityOption> sevOptions, int current, int prev,
        string prevDayLabel = "", string helperText = "", bool isLast = false)
    {
        var wrapper = new StackLayout { Spacing = 0, Padding = new Thickness(0, 8, 0, 0) };

        var titleRow = new Grid
        {
            ColumnSpacing = 6,
            Margin = new Thickness(0, 0, 0, 10),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var symptomLabelView = new Label
        {
            Text = symptomLabel,
            FontSize = 13,
            FontFamily = "OpenSansSemiBold",
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#031926"),
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };

        Grid.SetColumn(symptomLabelView, 0);
        titleRow.Children.Add(symptomLabelView);

        if (!string.IsNullOrWhiteSpace(helperText))
        {
            var infoIcon = new Image
            {
                Source = "syminfo.png",
                WidthRequest = 16,
                HeightRequest = 16,
                VerticalOptions = LayoutOptions.Center,
                Opacity = 0.7
            };

            var capturedHelper = helperText;
            infoIcon.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                    await MopupService.Instance.PushAsync(new Infopopup(symptomLabel, capturedHelper)))
            });

            Grid.SetColumn(infoIcon, 1);
            titleRow.Children.Add(infoIcon);
        }

        wrapper.Children.Add(titleRow);

        var pillBorders = new List<(Border border, Label lbl, int sevInt, string sevValue)>();
        StackLayout? rashFollowUpContainer = null;

        var pillGrid = new Grid { ColumnSpacing = 6 };
        for (int c = 0; c < sevOptions.Count; c++)
            pillGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (int c = 0; c < sevOptions.Count; c++)
        {
            var sev = sevOptions[c];
            int sevInt = int.TryParse(sev.Value, out var n) ? n - 1 : 0;
            bool isSel = current == sevInt;

            var innerLbl = new Label
            {
                Text = sev.Text,
                FontSize = 11,
                FontFamily = isSel ? "OpenSansSemiBold" : "OpenSansRegular",
                FontAttributes = isSel ? FontAttributes.Bold : FontAttributes.None,
                TextColor = isSel ? SeverityFg(sev.Value) : Color.FromArgb("#62727B"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var pill = new Border
            {
                BackgroundColor = isSel ? SeverityBg(sev.Value) : Color.FromArgb("#FAFAFA"),
                Stroke = isSel ? Colors.Transparent : Color.FromArgb("#E0E0E0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(4, 9),
                HorizontalOptions = LayoutOptions.Fill,
                Content = innerLbl
            };

            pillBorders.Add((pill, innerLbl, sevInt, sev.Value));

            var capturedSev = sevInt;
            var capturedId = fieldId;
            var capturedIsRash = fieldId.EndsWith("_rash");

            pill.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    _state.SetSev(capturedId, capturedSev);

                    ValidationBanner.IsVisible = false;
                    DayValidationBanner.IsVisible = false;

                    foreach (var (b, l, si, sv) in pillBorders)
                    {
                        bool sel = si == capturedSev;
                        b.BackgroundColor = sel ? SeverityBg(sv) : Color.FromArgb("#FAFAFA");
                        b.Stroke = sel ? Colors.Transparent : Color.FromArgb("#E0E0E0");
                        l.FontFamily = sel ? "OpenSansSemiBold" : "OpenSansRegular";
                        l.FontAttributes = sel ? FontAttributes.Bold : FontAttributes.None;
                        l.TextColor = sel ? SeverityFg(sv) : Color.FromArgb("#62727B");
                    }

                    if (capturedIsRash && rashFollowUpContainer != null)
                        rashFollowUpContainer.IsVisible = capturedSev > 0;
                })
            });

            Grid.SetColumn(pill, c);
            pillGrid.Children.Add(pill);
        }

        wrapper.Children.Add(pillGrid);

        if (fieldId.EndsWith("_rash"))
        {
            var dayId = fieldId.Replace("_rash", "");
            rashFollowUpContainer = BuildRashFollowUpInline(dayId);
            rashFollowUpContainer.IsVisible = current > 0;
            wrapper.Children.Add(rashFollowUpContainer);
        }

        if (prev >= 0 && !string.IsNullOrEmpty(prevDayLabel))
        {
            var prevSevText = sevOptions.ElementAtOrDefault(prev)?.Text ?? "";

            var prevRow = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 4,
                Margin = new Thickness(0, 6, 0, 0),
                Children =
                {
                    new Image
                    {
                        Source = "enter.png",
                        WidthRequest = 12,
                        HeightRequest = 12,
                        VerticalOptions = LayoutOptions.Center,
                        Opacity = 0.5
                    },
                    new Label
                    {
                        Text = $"{prevDayLabel}: {prevSevText}",
                        FontSize = 10,
                        FontFamily = "OpenSansSemiBold",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#9CA3AF"),
                        VerticalOptions = LayoutOptions.Center
                    }
                }
            };

            wrapper.Children.Add(prevRow);
        }

        if (!isLast)
        {
            wrapper.Children.Add(new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromArgb("#F0F0F0"),
                Margin = new Thickness(0, 10, 0, 0)
            });
        }

        return wrapper;
    }

    private StackLayout BuildRashFollowUpInline(string dayId)
    {
        var wrapper = new StackLayout { Spacing = 0, Margin = new Thickness(0, 8, 0, 0) };

        var locationKey = $"{dayId}_rash_location_y_n";
        var location2Key = $"{dayId}_rash_location_1";

        var locationQ = _model.SymptomGrid!.RashFollowup.Questions
            .FirstOrDefault(q => q.Id == "rash_location_y_n");
        var location2Q = _model.SymptomGrid!.RashFollowup.Questions
            .FirstOrDefault(q => q.Id == "rash_location_1");

        var locationOpts = locationQ?.Options ?? new List<T1Option>
        {
            new T1Option { Value = "1", Text = "Yes" },
            new T1Option { Value = "2", Text = "No, it is all over" }
        };

        var bodyPartContainer = new StackLayout
        {
            Spacing = 6,
            IsVisible = _state.Get(locationKey) == "1",
            Margin = new Thickness(0, 10, 0, 0)
        };

        bodyPartContainer.Children.Add(new Label
        {
            Text = location2Q?.Label ?? "Where was your rash or itch? Tick all that apply.",
            FontFamily = "OpenSansSemiBold",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#031926"),
            LineBreakMode = LineBreakMode.WordWrap,
            Margin = new Thickness(0, 0, 0, 4)
        });

        foreach (var opt in location2Q?.Options ?? new List<T1Option>())
        {
            var isSel = _state.GetMulti(location2Key).Contains(opt.Value);
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection(
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }),
                ColumnSpacing = 12,
                Padding = new Thickness(12, 9),
                BackgroundColor = isSel ? Color.FromArgb("#E6F1FB") : Color.FromArgb("#F8F9FA")
            };

            var dotBorder = new Border
            {
                WidthRequest = 22,
                HeightRequest = 22,
                Stroke = isSel ? Color.FromArgb("#009FE3") : Color.FromArgb("#D0D5DD"),
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle { CornerRadius = 4 },
                VerticalOptions = LayoutOptions.Center,
                Content = new BoxView
                {
                    BackgroundColor = isSel ? Color.FromArgb("#009FE3") : Colors.Transparent,
                    Color = isSel ? Color.FromArgb("#009FE3") : Colors.Transparent
                }
            };

            var lbl = new Label
            {
                Text = opt.Text,
                FontSize = 13,
                TextColor = Color.FromArgb("#031926"),
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap
            };

            Grid.SetColumn(dotBorder, 0);
            Grid.SetColumn(lbl, 1);
            row.Children.Add(dotBorder);
            row.Children.Add(lbl);

            var capturedVal = opt.Value;
            var capturedRow = row;
            var capturedDot = dotBorder;
            var capturedFill = (BoxView)dotBorder.Content;

            row.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    var cur = _state.GetMulti(location2Key);
                    if (cur.Contains(capturedVal)) cur.Remove(capturedVal);
                    else cur.Add(capturedVal);
                    _state.SetMulti(location2Key, cur);

                    bool nowSel = cur.Contains(capturedVal);
                    capturedRow.BackgroundColor = nowSel ? Color.FromArgb("#E6F1FB") : Color.FromArgb("#F8F9FA");
                    capturedDot.Stroke = nowSel ? Color.FromArgb("#009FE3") : Color.FromArgb("#D0D5DD");
                    capturedFill.BackgroundColor = nowSel ? Color.FromArgb("#009FE3") : Colors.Transparent;
                    capturedFill.Color = nowSel ? Color.FromArgb("#009FE3") : Colors.Transparent;
                })
            });

            bodyPartContainer.Children.Add(row);
        }

        var card = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E8ECF0"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(16, 14),
            Margin = new Thickness(0, 0, 0, 12)
        };

        var stack = new StackLayout { Spacing = 8 };

        stack.Children.Add(new Label
        {
            Text = locationQ?.Label ?? "Was your itch or rash in a specific region?",
            FontFamily = "OpenSansSemiBold",
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#031926"),
            LineBreakMode = LineBreakMode.WordWrap,
            Margin = new Thickness(0, 0, 0, 4)
        });

        var locationRows = new List<(Border border, Label lbl, string value)>();

        foreach (var opt in locationOpts)
        {
            var isSel = _state.Get(locationKey) == opt.Value;
            var innerLbl = new Label
            {
                Text = opt.Text,
                FontSize = 14,
                FontFamily = isSel ? "OpenSansSemiBold" : "OpenSansRegular",
                FontAttributes = isSel ? FontAttributes.Bold : FontAttributes.None,
                TextColor = Color.FromArgb("#031926")
            };
            var optRow = new Border
            {
                BackgroundColor = isSel ? Color.FromArgb("#E6F1FB") : Colors.White,
                Stroke = isSel ? Color.FromArgb("#009FE3") : Color.FromArgb("#E8ECF0"),
                StrokeThickness = isSel ? 2 : 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(14, 10),
                Content = innerLbl
            };

            locationRows.Add((optRow, innerLbl, opt.Value));

            var capturedVal = opt.Value;
            optRow.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    _state.Set(locationKey, capturedVal);
                    foreach (var (b, l, v) in locationRows)
                    {
                        bool sel = v == capturedVal;
                        b.BackgroundColor = sel ? Color.FromArgb("#E6F1FB") : Colors.White;
                        b.Stroke = sel ? Color.FromArgb("#009FE3") : Color.FromArgb("#E8ECF0");
                        b.StrokeThickness = sel ? 2 : 1;
                        l.FontFamily = sel ? "OpenSansSemiBold" : "OpenSansRegular";
                        l.FontAttributes = sel ? FontAttributes.Bold : FontAttributes.None;
                        l.TextColor = Color.FromArgb("#031926");
                    }
                    bodyPartContainer.IsVisible = capturedVal == "1";
                })
            });

            stack.Children.Add(optRow);
        }

        stack.Children.Add(bodyPartContainer);
        card.Content = stack;
        wrapper.Children.Add(card);
        return wrapper;
    }

    private void BuildImpactQuestions(StackLayout container, string dayId, T1SymptomGrid grid)
    {
        var actKey = $"{dayId}_activities_impact_y_n";
        var careKey = $"{dayId}_care_impact_y_n";
        var careTypeKey = $"{dayId}_care_impact_type";
        var otcKey = $"{dayId}_otc_drugs_y_n";
        var otcListKey = $"{dayId}_otc_drugs_list";

        var qMap = grid.DailyImpact.Questions.ToDictionary(q => q.Id);

        View BuildYesNoCard(string key, string label, StackLayout? followUp = null)
        {
            var card = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E8ECF0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(16, 14),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var stack = new StackLayout { Spacing = 8 };
            stack.Children.Add(new Label
            {
                Text = label,
                FontFamily = "OpenSansSemiBold",
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#031926"),
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 0, 0, 4)
            });

            var rows = new List<(Border border, Label lbl, string value)>();

            foreach (var opt in new[] { ("1", "Yes"), ("0", "No") })
            {
                bool isSel = _state.Get(key) == opt.Item1;
                var innerLbl = new Label
                {
                    Text = opt.Item2,
                    FontSize = 14,
                    FontFamily = isSel ? "OpenSansSemiBold" : "OpenSansRegular",
                    FontAttributes = isSel ? FontAttributes.Bold : FontAttributes.None,
                    TextColor = Color.FromArgb("#031926")
                };
                var optRow = new Border
                {
                    BackgroundColor = isSel ? Color.FromArgb("#E6F1FB") : Colors.White,
                    Stroke = isSel ? Color.FromArgb("#009FE3") : Color.FromArgb("#E8ECF0"),
                    StrokeThickness = isSel ? 2 : 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    Padding = new Thickness(14, 10),
                    Content = innerLbl
                };
                rows.Add((optRow, innerLbl, opt.Item1));

                var capturedVal = opt.Item1;
                optRow.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        _state.Set(key, capturedVal);
                        foreach (var (b, l, v) in rows)
                        {
                            bool sel = v == capturedVal;
                            b.BackgroundColor = sel ? Color.FromArgb("#E6F1FB") : Colors.White;
                            b.Stroke = sel ? Color.FromArgb("#009FE3") : Color.FromArgb("#E8ECF0");
                            b.StrokeThickness = sel ? 2 : 1;
                            l.FontFamily = sel ? "OpenSansSemiBold" : "OpenSansRegular";
                            l.FontAttributes = sel ? FontAttributes.Bold : FontAttributes.None;
                            l.TextColor = Color.FromArgb("#031926");
                        }
                        if (followUp != null)
                            followUp.IsVisible = capturedVal == "1";
                    })
                });
                stack.Children.Add(optRow);
            }

            if (followUp != null)
                stack.Children.Add(followUp);

            card.Content = stack;
            return card;
        }

        StackLayout BuildCheckboxList(string key, string label, List<T1Option> options)
        {
            var wrapper = new StackLayout { Spacing = 6, IsVisible = false, Margin = new Thickness(0, 8, 0, 0) };

            wrapper.Children.Add(new Label
            {
                Text = label,
                FontFamily = "OpenSansSemiBold",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#031926"),
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 0, 0, 4)
            });

            foreach (var opt in options)
            {
                var isSel = _state.GetMulti(key).Contains(opt.Value);
                var row = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection(
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Star }),
                    ColumnSpacing = 12,
                    Padding = new Thickness(12, 9),
                    BackgroundColor = isSel ? Color.FromArgb("#E6F1FB") : Color.FromArgb("#F8F9FA")
                };

                var dotBorder = new Border
                {
                    WidthRequest = 22,
                    HeightRequest = 22,
                    Stroke = isSel ? Color.FromArgb("#009FE3") : Color.FromArgb("#D0D5DD"),
                    StrokeThickness = 2,
                    StrokeShape = new RoundRectangle { CornerRadius = 4 },
                    VerticalOptions = LayoutOptions.Center,
                    Content = new BoxView
                    {
                        BackgroundColor = isSel ? Color.FromArgb("#009FE3") : Colors.Transparent,
                        Color = isSel ? Color.FromArgb("#009FE3") : Colors.Transparent
                    }
                };

                var lbl = new Label
                {
                    Text = opt.Text,
                    FontSize = 13,
                    TextColor = Color.FromArgb("#031926"),
                    VerticalOptions = LayoutOptions.Center,
                    LineBreakMode = LineBreakMode.WordWrap
                };

                Grid.SetColumn(dotBorder, 0);
                Grid.SetColumn(lbl, 1);
                row.Children.Add(dotBorder);
                row.Children.Add(lbl);

                var capturedVal = opt.Value;
                var capturedRow = row;
                var capturedDot = dotBorder;
                var capturedFill = (BoxView)dotBorder.Content;

                row.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        var cur = _state.GetMulti(key);
                        if (cur.Contains(capturedVal)) cur.Remove(capturedVal);
                        else cur.Add(capturedVal);
                        _state.SetMulti(key, cur);

                        bool nowSel = cur.Contains(capturedVal);
                        capturedRow.BackgroundColor = nowSel ? Color.FromArgb("#E6F1FB") : Color.FromArgb("#F8F9FA");
                        capturedDot.Stroke = nowSel ? Color.FromArgb("#009FE3") : Color.FromArgb("#D0D5DD");
                        capturedFill.BackgroundColor = nowSel ? Color.FromArgb("#009FE3") : Colors.Transparent;
                        capturedFill.Color = nowSel ? Color.FromArgb("#009FE3") : Colors.Transparent;
                    })
                });

                wrapper.Children.Add(row);
            }
            return wrapper;
        }

        var actQ = qMap.GetValueOrDefault("activities_impact_y_n");
        container.Children.Add(BuildYesNoCard(actKey,
            actQ?.Label ?? "Did you take any time off school or work, or skip your usual activities?"));

        var careQ = qMap.GetValueOrDefault("care_impact_y_n");
        var careTypeQ = qMap.GetValueOrDefault("care_impact_type");
        var careFollowUp = BuildCheckboxList(careTypeKey,
            careTypeQ?.Label ?? "Which medical or healthcare support did you seek?",
            careTypeQ?.Options ?? new List<T1Option>());
        careFollowUp.IsVisible = _state.Get(careKey) == "1";
        container.Children.Add(BuildYesNoCard(careKey,
            careQ?.Label ?? "Did you seek any medical or healthcare support?", careFollowUp));

        var otcQ = qMap.GetValueOrDefault("otc_drugs_y_n");
        var otcListQ = qMap.GetValueOrDefault("otc_drugs_list");
        var otcFollowUp = BuildCheckboxList(otcListKey,
            otcListQ?.Label ?? "Which over-the-counter medicines did you take?",
            otcListQ?.Options ?? new List<T1Option>());
        otcFollowUp.IsVisible = _state.Get(otcKey) == "1";
        container.Children.Add(BuildYesNoCard(otcKey,
            otcQ?.Label ?? "Did you take any over-the-counter medicines?", otcFollowUp));
    }

    // -- Generic question builder ----------------------------------------------
    private View BuildQuestion(T1Question q)
    {
        return q.Type switch
        {
            "info" => BuildInfoCard(q),
            "radio" => BuildRadioCard(q),
            "yesno" => BuildRadioCard(q),
            "checkbox" => BuildCheckboxCard(q),
            "date" => BuildDateCard(q),
            "file_upload" => BuildFileCard(q),
            _ => BuildInfoCard(q)
        };
    }

    private View BuildInfoCard(T1Question q)
    {
        var title = !string.IsNullOrEmpty(q.SectionLabel) ? q.SectionLabel : "";
        var body = q.Label;

        return new Border
        {
            BackgroundColor = Color.FromArgb("#EBF5FB"),
            Stroke = Color.FromArgb("#85B7EB"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(14, 12),
            Margin = new Thickness(0, 0, 0, 12),
            Content = new StackLayout
            {
                Spacing = title.Length > 0 ? 4 : 0,
                Children =
                {
                    title.Length > 0
                        ? new Label { Text = title, FontFamily = "OpenSansSemiBold", FontSize = 14, TextColor = Color.FromArgb("#0C447C") }
                        : null!,
                    new Label { Text = body, FontSize = 13, TextColor = Color.FromArgb("#2C6FAC"), LineBreakMode = LineBreakMode.WordWrap }
                }
            }
        };
    }

    private View BuildRadioCard(T1Question q)
    {
        var card = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E8ECF0"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(16, 14),
            Margin = new Thickness(0, 0, 0, 12)
        };

        var stack = new StackLayout { Spacing = 8 };

        stack.Children.Add(new Label
        {
            Text = q.Label,
            FontFamily = "OpenSansSemiBold",
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#031926"),
            LineBreakMode = LineBreakMode.WordWrap,
            Margin = new Thickness(0, 0, 0, 4)
        });

        if (!string.IsNullOrWhiteSpace(q.SubLabel))
        {
            stack.Children.Add(new Label
            {
                Text = q.SubLabel,
                FontSize = 12,
                FontFamily = "OpenSansRegular",
                TextColor = Color.FromArgb("#031926"),
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(5, 0, 5, 8)
            });
        }

        var rows = new List<(Border border, Label label, string value)>();

        foreach (var opt in q.Options)
        {
            var isSel = _state.Get(q.Id) == opt.Value;

            var innerLabel = new Label
            {
                Text = opt.Text,
                FontSize = 14,
                FontAttributes = isSel ? FontAttributes.Bold : FontAttributes.None,
                FontFamily = isSel ? "OpenSansSemiBold" : "OpenSansRegular",
                TextColor = Color.FromArgb("#031926")
            };

            var optRow = new Border
            {
                BackgroundColor = isSel ? Color.FromArgb("#E6F1FB") : Colors.White,
                Stroke = isSel ? Color.FromArgb("#009FE3") : Color.FromArgb("#E8ECF0"),
                StrokeThickness = isSel ? 2 : 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(14, 10),
                Content = innerLabel
            };

            rows.Add((optRow, innerLabel, opt.Value));

            var capturedVal = opt.Value;
            var capturedId = q.Id;
            optRow.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    var previous = _state.Get(capturedId);
                    if (previous != capturedVal)
                        ClearDownstreamState(capturedId);

                    _state.Set(capturedId, capturedVal);
                    ValidationBanner.IsVisible = false;

                    foreach (var (border, lbl, val) in rows)
                    {
                        bool sel = val == capturedVal;
                        border.BackgroundColor = sel ? Color.FromArgb("#E6F1FB") : Colors.White;
                        border.Stroke = sel ? Color.FromArgb("#009FE3") : Color.FromArgb("#E8ECF0");
                        border.StrokeThickness = sel ? 2 : 1;
                        lbl.FontFamily = sel ? "OpenSansSemiBold" : "OpenSansRegular";
                        lbl.FontAttributes = sel ? FontAttributes.Bold : FontAttributes.None;
                        lbl.TextColor = Color.FromArgb("#031926");
                    }

                    var container = _section switch
                    {
                        1 => Sec1Container,
                        2 => Sec2Container,
                        3 => Sec3Container,
                        5 => Sec5Container,
                        _ => null
                    };
                    if (container != null)
                    {
                        var questions = (dayform && _section == 2)
                            ? GetDayFormSection2Questions()
                            : _model.QuestionsForSection(_section);
                        DiffSection(container, questions);
                    }

                    // Daily form: toggle symptom grid when t11_symptoms_y_n changes
                    //if (dayform && capturedId == "t11_symptoms_y_n")
                    //{
                    //    if (capturedVal == "1")
                    //    {
                    //        BuildSection4();
                    //        Sec4View.IsVisible = true;
                    //    }
                    //    else
                    //    {
                    //        Sec4View.IsVisible = false;
                    //    }
                    //}
                })
            });

            stack.Children.Add(optRow);
        }

        card.Content = stack;
        return card;
    }

    private View BuildCheckboxCard(T1Question q)
    {
        var card = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E8ECF0"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(16, 14),
            Margin = new Thickness(0, 0, 0, 12)
        };

        var stack = new StackLayout { Spacing = 8 };

        stack.Children.Add(new Label
        {
            Text = q.Label,
            FontFamily = "OpenSansSemiBold",
            FontAttributes = FontAttributes.Bold,
            FontSize = 15,
            TextColor = Color.FromArgb("#031926"),
            LineBreakMode = LineBreakMode.WordWrap,
            Margin = new Thickness(0, 0, 0, 4)
        });

        var selected = _state.GetMulti(q.Id);

        foreach (var opt in q.Options)
        {
            var isSel = selected.Contains(opt.Value);

            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection(
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }),
                ColumnSpacing = 12,
                Padding = new Thickness(12, 9),
                BackgroundColor = isSel ? Color.FromArgb("#E6F1FB") : Color.FromArgb("#F8F9FA")
            };

            var dotBorder = new Border
            {
                WidthRequest = 22,
                HeightRequest = 22,
                Stroke = isSel ? Color.FromArgb("#009FE3") : Color.FromArgb("#D0D5DD"),
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle { CornerRadius = 4 },
                VerticalOptions = LayoutOptions.Center,
                Content = new BoxView
                {
                    BackgroundColor = isSel ? Color.FromArgb("#009FE3") : Colors.Transparent,
                    Color = isSel ? Color.FromArgb("#009FE3") : Colors.Transparent
                }
            };

            Grid.SetColumn(dotBorder, 0);
            row.Children.Add(dotBorder);

            var lbl = new Label
            {
                Text = opt.Text,
                FontSize = 13,
                TextColor = Color.FromArgb("#031926"),
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(lbl, 1);
            row.Children.Add(lbl);

            var capturedVal = opt.Value;
            var capturedId = q.Id;
            var capturedRow = row;
            var capturedDot = dotBorder;
            var capturedDotFill = (BoxView)dotBorder.Content;

            row.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    var cur = _state.GetMulti(capturedId);
                    if (cur.Contains(capturedVal)) cur.Remove(capturedVal);
                    else cur.Add(capturedVal);
                    _state.SetMulti(capturedId, cur);
                    ValidationBanner.IsVisible = false;

                    bool nowSel = cur.Contains(capturedVal);
                    capturedRow.BackgroundColor = nowSel ? Color.FromArgb("#E6F1FB") : Color.FromArgb("#F8F9FA");
                    capturedDot.Stroke = nowSel ? Color.FromArgb("#009FE3") : Color.FromArgb("#D0D5DD");
                    capturedDotFill.BackgroundColor = nowSel ? Color.FromArgb("#009FE3") : Colors.Transparent;
                    capturedDotFill.Color = nowSel ? Color.FromArgb("#009FE3") : Colors.Transparent;

                    if (_section == 4) RenderCurrentDay();
                    else
                    {
                        var container = _section switch
                        {
                            1 => Sec1Container,
                            2 => Sec2Container,
                            3 => Sec3Container,
                            5 => Sec5Container,
                            _ => null
                        };
                        if (container != null)
                        {
                            var questions = (dayform && _section == 2)
                                ? GetDayFormSection2Questions()
                                : _model.QuestionsForSection(_section);
                            DiffSection(container, questions);
                        }
                    }
                })
            });

            stack.Children.Add(row);
        }

        card.Content = stack;
        return card;
    }

    private View BuildDateCard(T1Question q)
    {
        var entry = new Entry
        {
            Placeholder = q.Placeholder ?? "DD/MM/YYYY",
            Text = _state.Get(q.Id) ?? "",
            FontAttributes = FontAttributes.Bold,
            FontFamily = "OpenSansSemiBold",
            Keyboard = Keyboard.Numeric,
            FontSize = 14,
            TextColor = Color.FromArgb("#031926"),
            PlaceholderColor = Color.FromArgb("#9CA3AF")
        };

        entry.Behaviors.Add(new MaskedBehavior
        {
            Mask = "XX/XX/XXXX",
            UnmaskedCharacter = 'X'
        });

        var capturedId = q.Id;
        entry.TextChanged += (s, e) => _state.Set(capturedId, e.NewTextValue ?? "");

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E8ECF0"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(16, 14),
            Margin = new Thickness(0, 0, 0, 12),
            Content = new StackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = q.Label,
                        FontFamily = "OpenSansSemiBold",
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 15,
                        TextColor = Color.FromArgb("#031926"),
                        LineBreakMode = LineBreakMode.WordWrap
                    },
                    entry
                }
            }
        };
    }


    private View BuildFileCard(T1Question q)
    {
        var capturedId = q.Id;

        var card = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E8ECF0"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(16, 14),
            Margin = new Thickness(0, 0, 0, 12)
        };

        var stack = new StackLayout { Spacing = 12 };

        stack.Children.Add(new Label
        {
            Text = q.Label,
            FontFamily = "OpenSansSemiBold",
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#031926"),
            LineBreakMode = LineBreakMode.WordWrap,
            Margin = new Thickness(0, 0, 0, 4)
        });

        var uploadZone = new Border
        {
            BackgroundColor = Color.FromArgb("#F8F9FA"),
            Stroke = Color.FromArgb("#D0D5DD"),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(8, 16)
        };

        uploadZone.Content = new StackLayout
        {
            Spacing = 6,
            HorizontalOptions = LayoutOptions.Center,
            Children =
           {
               new Image { Source = "infoicon.png", WidthRequest = 28, HeightRequest = 28, HorizontalOptions = LayoutOptions.Center, Opacity = 0.4 },
               new Label { Text = "Tap to upload photo", FontSize = 12, FontFamily = "OpenSansSemiBold", TextColor = Color.FromArgb("#62727B"), HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center },
               new Label { Text = "JPG or PNG", FontSize = 10, TextColor = Color.FromArgb("#9CA3AF"), HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center }
           }
        };

        var captureZone = new Border
        {
            BackgroundColor = Color.FromArgb("#F8F9FA"),
            Stroke = Color.FromArgb("#D0D5DD"),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(8, 16)
        };

        captureZone.Content = new StackLayout
        {
            Spacing = 6,
            HorizontalOptions = LayoutOptions.Center,
            Children =
           {
               new Image { Source = DeviceInfo.Platform == DevicePlatform.Android ? "androidcamera.png" : "applecamera.png", WidthRequest = 28, HeightRequest = 28, HorizontalOptions = LayoutOptions.Center, Opacity = 0.4 },
               new Label { Text = "Tap to capture photo", FontSize = 12, FontFamily = "OpenSansSemiBold", TextColor = Color.FromArgb("#62727B"), HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center },
               new Label { Text = "Use camera to take photo", FontSize = 10, TextColor = Color.FromArgb("#9CA3AF"), HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center }
           }
        };

        var actionZonesGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }),
            ColumnSpacing = 10,
            IsVisible = true
        };

        Grid.SetColumn(uploadZone, 0);
        Grid.SetColumn(captureZone, 1);
        actionZonesGrid.Children.Add(uploadZone);
        actionZonesGrid.Children.Add(captureZone);

        // Loading Indicator View
        var loadingIndicator = new ActivityIndicator
        {
            IsRunning = true,
            Color = Color.FromArgb("#009FE3"),
            WidthRequest = 32,
            HeightRequest = 32,
            HorizontalOptions = LayoutOptions.Center
        };

        var loadingLabel = new Label
        {
            Text = "Uploading photo...",
            FontSize = 12,
            FontFamily = "OpenSansSemiBold",
            TextColor = Color.FromArgb("#62727B"),
            HorizontalOptions = LayoutOptions.Center
        };

        var loadingZone = new Border
        {
            BackgroundColor = Color.FromArgb("#F8F9FA"),
            Stroke = Color.FromArgb("#D0D5DD"),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(8, 20),
            IsVisible = false,
            Content = new StackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Center,
                Children = { loadingIndicator, loadingLabel }
            }
        };

        var thumbnailRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new ColumnDefinition { Width = new GridLength(80) },
                new ColumnDefinition { Width = GridLength.Star }),
            ColumnSpacing = 12,
            IsVisible = false
        };

        var thumbnail = new Image { WidthRequest = 80, HeightRequest = 80, Aspect = Aspect.AspectFill, VerticalOptions = LayoutOptions.Start };
        var thumbBorder = new Border { WidthRequest = 80, HeightRequest = 80, StrokeShape = new RoundRectangle { CornerRadius = 8 }, StrokeThickness = 0, Content = thumbnail };
        var actionsStack = new StackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        var previewBtn = new Button { Text = "Preview", BackgroundColor = Color.FromArgb("#E6F1FB"), TextColor = Color.FromArgb("#185FA5"), CornerRadius = 8, FontFamily = "OpenSansSemiBold", FontSize = 13, HeightRequest = 36, BorderWidth = 0 };
        var deleteBtn = new Button { Text = "Remove", BackgroundColor = Color.FromArgb("#FDECEA"), TextColor = Color.FromArgb("#B71C1C"), CornerRadius = 8, FontFamily = "OpenSansSemiBold", FontSize = 13, HeightRequest = 36, BorderWidth = 0 };

        actionsStack.Children.Add(previewBtn);
        actionsStack.Children.Add(deleteBtn);
        Grid.SetColumn(thumbBorder, 0);
        Grid.SetColumn(actionsStack, 1);
        thumbnailRow.Children.Add(thumbBorder);
        thumbnailRow.Children.Add(actionsStack);

        // Keep track of active display path locally so preview works even before online reload
        string currentLocalOrRemotePath = null;

        var existingPath = _state.Get(capturedId);
        if (!string.IsNullOrEmpty(existingPath))
        {
            currentLocalOrRemotePath = existingPath;
            if (File.Exists(existingPath))
            {
                thumbnail.Source = ImageSource.FromFile(existingPath);
            }
            else
            {
                // If it's stored as an Azure path/URL, build UriImageSource or load via Azure
                var fullUrl = $"https://peoplewithappiamges.blob.core.windows.net/imperial/{existingPath}";
                thumbnail.Source = ImageSource.FromUri(new Uri(fullUrl));
            }

            actionZonesGrid.IsVisible = false;
            thumbnailRow.IsVisible = true;
        }

        // Common method to handle processing both gallery selection and camera capture
        async Task ProcessFileAsync(FileResult photo)
        {
            if (photo == null) return;

            // Show loading state, hide action zones
            actionZonesGrid.IsVisible = false;
            loadingZone.IsVisible = true;

            try
            {
                // Save locally to temporary app directory so preview/thumbnail work instantly without depending on file persistence
                var localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                using (var sourceStream = await photo.OpenReadAsync())
                using (var localStream = File.Create(localFilePath))
                {
                    await sourceStream.CopyToAsync(localStream);
                }

                currentLocalOrRemotePath = localFilePath;

                // Upload to Azure
                var random = new Random();
                var randomNum = random.Next(10000, 999999);
                var imageName = $"{Helpers.Settings.UsersID}_{DateTime.Now:yyyyMMdd}_{_model.FormId}_{randomNum}.png";
                var blobPath = $"testresults/{imageName}";

                var connectionString = "DefaultEndpointsProtocol=https;AccountName=peoplewithappiamges;AccountKey=9maBMGnjWp6KfOnOuXWHqveV4LPKyOnlCgtkiKQOeA+d+cr/trKApvPTdQ+piyQJlicOE6dpeAWA56uD39YJhg==;EndpointSuffix=core.windows.net";
                var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("imperial");
                var blobClient = containerClient.GetBlobClient(blobPath);

                using (var stream = File.OpenRead(localFilePath))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                _state.Set(capturedId, blobPath);
                _state.SetImageFilename(blobPath);

                // Update UI once completed
                thumbnail.Source = ImageSource.FromFile(localFilePath);
                thumbnailRow.IsVisible = true;

                if (ValidationBanner != null)
                    ValidationBanner.IsVisible = false;
            }
            catch (Exception Ex)
            {
                CrashDetected.LogCrash(Ex, Navigation, "ProcessFileAsync");
                actionZonesGrid.IsVisible = true;
            }
            finally
            {
                // Always hide loading indicator when done or on failure
                loadingZone.IsVisible = false;
            }
        }

        uploadZone.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                try
                {
                    var photo = await MediaPicker.PickPhotoAsync();
                    await ProcessFileAsync(photo);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error picking photo: {ex.Message}");
                }
            })
        });

        captureZone.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                try
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        var photo = await MediaPicker.CapturePhotoAsync();
                        await ProcessFileAsync(photo);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error capturing photo: {ex.Message}");
                }
            })
        });

        previewBtn.Clicked += async (s, e) =>
        {
            if (string.IsNullOrEmpty(currentLocalOrRemotePath)) return;

            ImageSource previewSource;
            if (File.Exists(currentLocalOrRemotePath))
            {
                previewSource = ImageSource.FromFile(currentLocalOrRemotePath);
            }
            else
            {
                var fullUrl = $"https://peoplewithappiamges.blob.core.windows.net/imperial/{currentLocalOrRemotePath}";
                previewSource = ImageSource.FromUri(new Uri(fullUrl));
            }

            var previewPage = new ContentPage { BackgroundColor = Color.FromArgb("#CC000000") };
            var previewImg = new Image
            {
                Source = previewSource,
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Margin = new Thickness(20)
            };
            var closeBtn = new Button
            {
                Text = "Close",
                BackgroundColor = Color.FromArgb("#009FE3"),
                TextColor = Colors.White,
                CornerRadius = 10,
                FontFamily = "OpenSansSemiBold",
                HeightRequest = 44,
                Margin = new Thickness(40, 0, 40, 40),
                VerticalOptions = LayoutOptions.End
            };

            closeBtn.Clicked += async (_, __) => await Navigation.PopModalAsync(false);
            previewPage.Content = new Grid { Children = { previewImg, new StackLayout { VerticalOptions = LayoutOptions.End, Children = { closeBtn } } } };
            await Navigation.PushModalAsync(previewPage, false);
        };

        deleteBtn.Clicked += (s, e) =>
        {
            _state.Set(capturedId, "");
            currentLocalOrRemotePath = null;
            thumbnail.Source = null;
            thumbnailRow.IsVisible = false;
            actionZonesGrid.IsVisible = true;
        };

        stack.Children.Add(actionZonesGrid);
        stack.Children.Add(loadingZone);
        stack.Children.Add(thumbnailRow);
        card.Content = stack;
        return card;
    }
    private void ClearDownstreamState(string changedQuestionId)
    {
        if (dayform) return;

        switch (changedQuestionId)
        {
            case "t1_enrolment_route":
                _state.ClearKeysStartingWith("t1_index_y_n_reactive");
                _state.ClearKeysStartingWith("t1_index_y_n_preemptive");
                ClearSections(2, 3, 4, 5);
                Sec2Container.Children.Clear();
                Sec3Container.Children.Clear();
                break;

            case "t1_index_y_n_reactive":
            case "t1_index_y_n_preemptive":
                ClearSections(2, 3, 4, 5);
                Sec2Container.Children.Clear();
                Sec3Container.Children.Clear();
                break;
        }
    }

    private void ClearSections(params int[] sections)
    {
        foreach (var s in sections)
        {
            foreach (var q in _model.QuestionsForSection(s))
                _state.ClearQuestion(q.Id);
        }
        if (Array.Exists(sections, s => s == 4) && _model.SymptomGrid != null)
        {
            foreach (var grp in _model.SymptomGrid.SymptomGroups)
                foreach (var sym in grp.Symptoms)
                    foreach (var day in _activeDays)
                        _state.ClearSev($"{day.Id}_{sym.Id}");
        }
    }

    private bool IsVisible(T1Question q)
    {
        if (string.IsNullOrEmpty(q.ShowIf)) return true;
        return EvalCondition(q.ShowIf);
    }

    private bool EvalCondition(string expr)
    {
        if (string.IsNullOrWhiteSpace(expr)) return true;

        if (expr.Contains(" OR "))
            return expr.Split(new[] { " OR " }, StringSplitOptions.None)
                       .Any(part => EvalCondition(part.Trim()));

        if (expr.Contains(" AND "))
            return expr.Split(new[] { " AND " }, StringSplitOptions.None)
                       .All(part => EvalCondition(part.Trim()));

        if (expr.Contains(" IN ["))
        {
            var parts = expr.Split(new[] { " IN " }, 2, StringSplitOptions.None);
            var field = parts[0].Trim();
            var values = parts[1].Trim('[', ']')
                                 .Split(',')
                                 .Select(v => v.Trim().Trim('\''))
                                 .ToList();
            return values.Contains(_state.Get(field) ?? "");
        }

        if (expr.Contains(" == "))
        {
            var parts = expr.Split(new[] { " == " }, 2, StringSplitOptions.None);
            var field = parts[0].Trim();
            var value = parts[1].Trim().Trim('\'');
            return _state.Get(field) == value;
        }

        if (expr.Contains(" includes "))
        {
            var parts = expr.Split(new[] { " includes " }, 2, StringSplitOptions.None);
            var field = parts[0].Trim();
            var raw = parts[1].Trim();
            var quoteStart = raw.IndexOf('\'');
            var quoteEnd = quoteStart >= 0 ? raw.IndexOf('\'', quoteStart + 1) : -1;
            var value = (quoteStart >= 0 && quoteEnd > quoteStart)
                ? raw.Substring(quoteStart + 1, quoteEnd - quoteStart - 1)
                : raw.Trim('\'');
            return _state.GetMulti(field).Contains(value);
        }

        return true;
    }

    private bool EvalDayTabCondition(string expr)
    {
        if (string.IsNullOrWhiteSpace(expr)) return true;
        if (dayform) return true;
        if (expr.StartsWith("any onset"))
            return _state.Has("t1_symptom_onset_reactive") || _state.Has("t1_symptom_onset_preemptive");
        return EvalCondition(expr);
    }

    // -- Navigation handlers ---------------------------------------------------
    private void OnNextSectionClicked(object sender, EventArgs e)
    {
        ValidationBanner.IsVisible = false;

        if (!ValidateSection(_section, out var msg))
        {
            ValidationText.Text = msg;
            ValidationBanner.IsVisible = true;
            Vibration.Vibrate();
            return;
        }

        if (dayform)
        {
            int next = _section switch
            {
                1 => _state.Get("t11_symptoms_y_n") == "1" ? 4 : 3, // yes -> grid (sec4), no -> samples (sec3)
                4 => 3,  // after symptom grid -> samples
                _ => 3
            };
            GoToSection(next);
            return;
        }

        // T1 form
        int nextSection = _section + 1;
        if (nextSection == 4 && _state.Get("t1_symptoms_y_n_preemptive") == "0")
            nextSection = 5;

        if (nextSection <= _model.TotalSections) GoToSection(nextSection);
    }

    private void BackButtonTapped_Tapped(object sender, TappedEventArgs e)
    {
        if (dayform)
        {
            int prev = _section switch
            {
                1 => -1,
                4 => 1,
                3 => _state.Get("t11_symptoms_y_n") == "1" ? 4 : 1,
                _ => 1
            };
            if (prev == -1) { Navigation.PopAsync(); return; }
            GoToSection(prev);
            return;
        }

        // T1 form
        if (_section == 1)
        {
            Navigation.PopAsync();
            return;
        }

        int prevSection = _section - 1;
        if (prevSection == 4 && _state.Get("t1_symptoms_y_n_preemptive") == "0")
            prevSection = 3;

        GoToSection(prevSection);
    }

    private void OnNextDayClicked(object sender, EventArgs e)
    {
        var grid = _model.SymptomGrid!;
        var day = _activeDays[_dayIdx];

        var unanswered = grid.SymptomGroups
            .SelectMany(g => g.Symptoms)
            .Where(sym => _state.GetSev($"{day.Id}_{sym.Id}") < 0)
            .Select(sym => sym.Label)
            .ToList();

        if (unanswered.Any())
        {
            DayValidationText.Text = $"Please rate all symptoms before continuing. Missing: {unanswered.First()}";
            DayValidationBanner.IsVisible = true;
            Vibration.Vibrate();
            return;
        }

        DayValidationBanner.IsVisible = false;
        _state.MarkDayDone(_activeDays[_dayIdx].Id);

        if (_dayIdx < _activeDays.Count - 1)
        {
            var currentDay = _activeDays[_dayIdx];
            var nextDay = _activeDays[_dayIdx + 1];

            foreach (var grp in grid.SymptomGroups)
                foreach (var sym in grp.Symptoms)
                {
                    var v = _state.GetSev($"{currentDay.Id}_{sym.Id}");
                    if (v >= 0) _state.SetSev($"{nextDay.Id}_{sym.Id}", v);
                }

            foreach (var suffix in new[] { "_activities_impact_y_n", "_care_impact_y_n", "_otc_drugs_y_n" })
            {
                var v = _state.Get($"{currentDay.Id}{suffix}");
                if (v != null) _state.Set($"{nextDay.Id}{suffix}", v);
            }

            foreach (var suffix in new[] { "_care_impact_type", "_otc_drugs_list", "_rash_location_1" })
            {
                var vals = _state.GetMulti($"{currentDay.Id}{suffix}");
                if (vals.Any()) _state.SetMulti($"{nextDay.Id}{suffix}", new List<string>(vals));
            }

            var rashLoc = _state.Get($"{currentDay.Id}_rash_location_y_n");
            if (rashLoc != null) _state.Set($"{nextDay.Id}_rash_location_y_n", rashLoc);

            _dayIdx++;
            RenderDayStrip();
            RenderCurrentDay();
            MainScroll.ScrollToAsync(0, 0, false);
        }
        else
        {
            // Daily form: after last day go to section 2 (samples)
            // T1 form: go to section 5
            GoToSection(dayform ? 2 : 5);
        }
    }

    private void OnPrevDayClicked(object sender, EventArgs e)
    {
        if (_dayIdx > 0) { _dayIdx--; RenderDayStrip(); RenderCurrentDay(); MainScroll.ScrollToAsync(0, 0, false); }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        ValidationBanner.IsVisible = false;

        if (!ValidateSection(_section, out var msg))
        {
            ValidationText.Text = msg;
            ValidationBanner.IsVisible = true;
            Vibration.Vibrate();
            return;
        }

        try
        {
            string formId = dayform ? $"t{_dayNumber}_form" : _model.FormId;
            var submission = _state.ToSubmission(formId, _model, dayform ? _dayNumber : 0);
            await APICalls.Instance.PostUserQuestionnaire(submission);
            alluserquestionnaires.Add(submission);

            // -- Study record update --
            var studyDetails = allhouseholdgroup.studydetails;

            if (studyDetails == null)
            {
                studyDetails = new householdstudyrecord
                {
                    household_id = allhouseholdgroup.householdgroupid,
                    current_phase = "TPhase",
                    t_events = new List<TEvent>()
                };
            }

            studyDetails.t_events ??= new List<TEvent>();

            var activeEvent = studyDetails.t_events
                .FirstOrDefault(x => x.t_event_status == "active");

            if (activeEvent == null)
            {
                activeEvent = new TEvent
                {
                    t_event_id = "TE-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                    t_event_status = "active",
                    scenario = null,
                    trigger_type = Helpers.Settings.SignUp,
                    t1_start_date = DateTime.UtcNow.ToString("g"),
                    members = allhouseholdgroup.userdetailslist?
                        .Select(m => new TEventMember
                        {
                            user_id = m.household_individual_userid,
                            daily_forms_complete = false,
                            daily_forms_stopped_at = null,
                            questionnaires = new List<TQuestionnaire>()
                        }).ToList() ?? new List<TEventMember>()
                };
                studyDetails.t_events.Add(activeEvent);
            }

            var member = activeEvent.members?
                .FirstOrDefault(m => m.user_id == Helpers.Settings.UsersID);


            if(member == null)
            {
                // Member joined after the event was created — add them now
                member = new TEventMember
                {
                    user_id = Helpers.Settings.UsersID,
                    daily_forms_complete = false,
                    daily_forms_stopped_at = null,
                    questionnaires = new List<TQuestionnaire>()
                };
                activeEvent.members.Add(member);
            }

            if (member != null)
            {
                // T1 form uses "T1", daily forms use "T{dayNumber}"
                string questionnaireType = dayform ? $"T{_dayNumber}" : "T1";

                // Remove existing entry for this timepoint if resubmitting
                var existing = member.questionnaires
                    .FirstOrDefault(q => q.questionnaire_type == questionnaireType);
                if (existing != null)
                    member.questionnaires.Remove(existing);

                // lfd_positive: true if ANY LFD selected (not just Scenario A pathogens)
                // IsLfdPositiveForScenarioA: true only for Influenza A/B, RSV, hMPV
                member.questionnaires.Add(new TQuestionnaire
                {
                    questionnaire_type = questionnaireType,
                    date_completed = DateTime.UtcNow.ToString("g"),
                    has_symptoms = _state.HasAtLeastOneSymptom,
                    lfd_result = _state.IsLfdPositiveForScenarioA,
                    latesubmission = missedquestionnaire
                });

                //// Early Scenario A detection on T1 or T2
                //if ((questionnaireType == "T1" || questionnaireType == "T2") && activeEvent.scenario == null)
                //{
                //    if (_state.IsLfdPositiveForScenarioA)
                //        activeEvent.scenario = "A";
                //}

                    // Full scenario determination once T3 is submitted
                    if (questionnaireType == "T3")
                        DetermineAndSetScenario(activeEvent);

                    // If T3 was skipped but we are past day 3, determine scenario from available data
                    if (activeEvent.scenario == null && _dayNumber > 3)
                        DetermineAndSetScenario(activeEvent);

                    // Scenario A: check if this member has hit 3 consecutive symptom-free days
                    if (dayform && activeEvent.scenario == "A")
                        CheckAndSetDailyFormsComplete(member, activeEvent);

                    // Scenario B&C: check if whole household has hit 3 consecutive symptom-free days
                    if (dayform && (activeEvent.scenario == "B" || activeEvent.scenario == "C"))
                    {
                        if (CheckHouseholdThreeDayClear(activeEvent))
                            studyDetails.current_phase = "AwaitingSamples";
                    }

                    // T28 always closes the event
                    if (questionnaireType == "T28")
                    {
                        // activeEvent.t_event_status = "complete";
                        // studyDetails.current_phase = "AwaitingSamples";
                    }
                
            }

            // Patch back to API
            allhouseholdgroup.details = System.Text.Json.JsonSerializer.Serialize(studyDetails);
            var updateData = new { details = allhouseholdgroup.details };
            string updateJson = System.Text.Json.JsonSerializer.Serialize(updateData);
            var url = $"{APICalls.ApplicationURL}householdgroup/householdgroupid/{allhouseholdgroup.householdgroupid}";
            var content = new StringContent(updateJson, Encoding.UTF8, "application/json");
            await APICalls.Instance.GetClient().PatchAsync(url, content);

            WeakReferenceMessenger.Default.Send(new UpdateDashCompelted(alluserquestionnaires.ToList()));

            //Handle Daily Notification Logic 
            await UpdateNotification.ScheduleDailyNotification(true); 

            await MopupService.Instance.PushAsync(new PopupPageHelper("Questionnaire Submitted"));
            await Task.Delay(2500);
            Navigation.RemovePage(this);
            await MopupService.Instance.PopAllAsync(false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, Navigation, "OnSubmitClicked");
            await DisplayAlert("Error", "Submission failed. Please try again.", "OK");
        }
    }

    // Determines Scenario A, B, or C after T3 is submitted by checking all
    // members' T1/T2/T3 LFD results across the household
    private void DetermineAndSetScenario(TEvent activeEvent)
    {
        var triggerForms = new[] { "T1", "T2", "T3" };

        var allTriggerQuestionnaires = activeEvent.members
            .SelectMany(m => m.questionnaires)
            .Where(q => triggerForms.Contains(q.questionnaire_type))
            .ToList();

        // Scenario A: any member had a Scenario A pathogen positive LFD on T1/T2/T3
        // We use lfd_scenario_a flag which was set at submission time
        bool anyScenarioA = allTriggerQuestionnaires.Any(q => q.lfd_result);

        if (anyScenarioA && activeEvent.scenario != "A")
            activeEvent.scenario = "A";
        else if (!anyScenarioA && activeEvent.scenario == null)
            activeEvent.scenario = "C";
        // B is set if lfd_positive but not scenario A - handled at T1/T2 submission
    }

    private void CheckAndSetDailyFormsComplete(TEventMember member, TEvent activeEvent)
    {
        if (member.daily_forms_complete) return;

        if (!DateTime.TryParseExact(activeEvent.t1_start_date,
             new[] { "dd/MM/yy HH:mm", "dd/MM/yyyy HH:mm", "g" },
             System.Globalization.CultureInfo.CurrentCulture,
             System.Globalization.DateTimeStyles.None,
             out DateTime t1Start)) return;

        int dayNumber = (DateTime.Today - t1Start.Date).Days + 1;

        // Must complete at least 14 days regardless
        if (dayNumber < 14) return;

        if (dayNumber == 14)
        {
            var day13Form = member.questionnaires
                .FirstOrDefault(q => q.questionnaire_type == "T13");
            var day14Form = member.questionnaires
                .FirstOrDefault(q => q.questionnaire_type == "T14");

            // Either T13 or T14 symptom-free is enough to stop
            bool pairCleart14 = (day13Form != null && !day13Form.has_symptoms) ||
                             (day14Form != null && !day14Form.has_symptoms);

            if (pairCleart14)
            {
                member.daily_forms_complete = true;
                member.daily_forms_stopped_at = "T14";
            }
            return;
        }

        // Day 15+ - check current pair (T15/T16, T17/T18, T19/T20 etc.)
        // Pair is determined by which pair the current day falls in
        // T15/T16 = pair 1, T17/T18 = pair 2 etc.

        // Only check on even days of each pair (day 16, 18, 20 etc.)
        if ((dayNumber - 14) % 2 != 0) return;

        int pairStart = 15 + (((dayNumber - 15) / 2) * 2); // e.g. day 15 or 16 -> 15, day 17 or 18 -> 17
        int pairEnd = pairStart + 1;

        var pairForm1 = member.questionnaires
            .FirstOrDefault(q => q.questionnaire_type == $"T{pairStart}");
        var pairForm2 = member.questionnaires
            .FirstOrDefault(q => q.questionnaire_type == $"T{pairEnd}");

        // If either form in the pair is submitted and symptom-free, they can stop
        bool pairClear = (pairForm1 != null && !pairForm1.has_symptoms) ||
                         (pairForm2 != null && !pairForm2.has_symptoms);

        if (pairClear)
        {
            member.daily_forms_complete = true;
            member.daily_forms_stopped_at = $"T{dayNumber}";
        }
    }
    // Checks if every member in the household has had 3 consecutive symptom-free days
    // Used for Scenario B&C to trigger household return to AwaitingSamples
    private bool CheckHouseholdThreeDayClear(TEvent activeEvent)
    {
        if (!DateTime.TryParseExact(activeEvent.t1_start_date, "g",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out DateTime t1Start)) return false;

        int dayNumber = (DateTime.Today - t1Start.Date).Days + 1;
        if (dayNumber < 3) return false;

        var lastThreeDays = new[] { dayNumber - 2, dayNumber - 1, dayNumber };

        foreach (var m in activeEvent.members)
        {
            foreach (var day in lastThreeDays)
            {
                var form = m.questionnaires
                    .FirstOrDefault(q => q.questionnaire_type == $"T{day}");
                if (form == null || form.has_symptoms)
                    return false;
            }
        }

        return true;
    }

    private void OnTryAgainClicked(object sender, EventArgs e)
    {
        FailedView.IsVisible = false;
        _ = LoadAsync();
    }

    // -- Validation ------------------------------------------------------------
    private bool ValidateSection(int s, out string message)
    {
        var unanswered = _model.QuestionsForSection(s)
            .Where(q => q.Required && IsVisible(q))
            .Where(q => q.Type is "radio" or "yesno" or "date"
                        ? !_state.Has(q.Id)
                        : q.Type == "checkbox"
                            ? _state.GetMulti(q.Id).Count == 0
                            : q.Type == "file_upload"
                                ? !_state.Has(q.Id) || !File.Exists(_state.Get(q.Id))
                                : false)
            .ToList();

        var dateQuestions = _model.QuestionsForSection(s)
           .Where(q => IsVisible(q) && q.Type == "date")
           .ToList();

        foreach (var q in dateQuestions)
        {
            string dateText = _state.Get(q.Id);
            if (!string.IsNullOrWhiteSpace(dateText))
            {
                string questionLabel = q.Label.Split('.')[0];
                if (DateTime.TryParseExact(dateText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fetcha))
                {
                    if (fetcha.Date > DateTime.Today)
                    {
                        message = $"Please enter a date that is not in the future for: {questionLabel}";
                        return false;
                    }
                    if (fetcha.Date < DateTime.Today.AddYears(-100))
                    {
                        message = $"Please enter a date that is not 100 years or more in the past: {questionLabel}";
                        return false;
                    }
                }
                else
                {
                    message = $"Please enter a valid date for: {questionLabel}";
                    return false;
                }
            }
        }

        if (unanswered.Any())
        {
            var first = unanswered.First();
            message = first.Type == "file_upload"
                ? $"Please upload an image: {first.Label.Split('.')[0]}"
                : $"Please answer: {first.Label.Split('.')[0]}";
            return false;
        }

        message = "";
        return true;
    }

    // -- Colour helpers --------------------------------------------------------
    private static Color SeverityBg(string jsonValue) => jsonValue switch
    {
        "1" => Color.FromArgb("#CFD8DC"),
        "2" => Color.FromArgb("#DCEDC8"),
        "3" => Color.FromArgb("#FFE0B2"),
        "4" => Color.FromArgb("#EF9A9A"),
        _ => Color.FromArgb("#CFD8DC")
    };

    private static Color SeverityFg(string jsonValue) => jsonValue switch
    {
        "1" => Color.FromArgb("#455A64"),
        "2" => Color.FromArgb("#33691E"),
        "3" => Color.FromArgb("#E65100"),
        "4" => Color.FromArgb("#B71C1C"),
        _ => Color.FromArgb("#455A64")
    };
}

// ===============================================================================
// JSON MODEL
// ===============================================================================

public class T1FormModel
{
    public string FormId { get; set; } = "t1_form";
    public int TotalSections { get; set; } = 5;
    public List<T1Question> Questions { get; set; } = new();
    public T1SymptomGrid? SymptomGrid { get; set; }
    public T1NavLabels NavLabels { get; set; } = new();

    public IEnumerable<T1Question> QuestionsForSection(int s) =>
        Questions.Where(q => q.Section == s).OrderBy(q => q.Order);

    public static T1FormModel FromJson(string json)
    {
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var raw = JsonSerializer.Deserialize<List<JsonElement>>(json, opts)
                  ?? new List<JsonElement>();

        var model = new T1FormModel();

        foreach (var el in raw)
        {
            var type = el.TryGetProperty("type", out var tp) ? tp.GetString() ?? "" : "";

            if (type == "symptom_grid")
            {
                model.SymptomGrid = JsonSerializer.Deserialize<T1SymptomGrid>(el.GetRawText(), opts);
                model.Questions.Add(new T1Question
                {
                    Id = "symptom_grid",
                    Type = "symptom_grid",
                    Section = el.TryGetProperty("section", out var sec) ? sec.GetInt32() : 4,
                    SectionLabel = el.TryGetProperty("section_label", out var sl) ? sl.GetString() ?? "" : "",
                    Order = el.TryGetProperty("order", out var ord) ? ord.GetInt32() : 18,
                    Label = el.TryGetProperty("label", out var lbl) ? lbl.GetString() ?? "" : "",
                });
            }
            else
            {
                var q = JsonSerializer.Deserialize<T1Question>(el.GetRawText(), opts);
                if (q != null) model.Questions.Add(q);
            }
        }

        // Derive TotalSections from max section in questions (not hardcoded)
        // This means tform.json with 2 sections automatically gives a 2-section form
        var maxSection = model.Questions
            .Where(q => q.Type != "symptom_grid")
            .Select(q => q.Section)
            .DefaultIfEmpty(2)
            .Max();
        model.TotalSections = maxSection;

        model.NavLabels = new T1NavLabels
        {
            Back = "Back",
            Next = "Continue",
            Submit = model.Questions.FirstOrDefault(q => q.Id == "t1_submit")?.Label ?? "Submit Questionnaire",
            PrevDay = "Prev",
            NextDay = "Next Day",
            LastDay = "Done"
        };

        return model;
    }
}

public class T1Question
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("sublabel")] public string SubLabel { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("required")] public bool Required { get; set; }
    [JsonPropertyName("options")] public List<T1Option> Options { get; set; } = new();
    [JsonPropertyName("placeholder")] public string? Placeholder { get; set; }
    [JsonPropertyName("order")] public int Order { get; set; }
    [JsonPropertyName("questionid")] public string QuestionId { get; set; } = "";
    [JsonPropertyName("section")] public int Section { get; set; }
    [JsonPropertyName("section_label")] public string SectionLabel { get; set; } = "";
    [JsonPropertyName("show_if")] public string? ShowIf { get; set; }
    [JsonPropertyName("note")] public string? Note { get; set; }

    public T1Question WithPrefix(string dayPrefix)
    {
        var copy = (T1Question)MemberwiseClone();
        copy.Id = $"{dayPrefix}_{Id}";
        if (!string.IsNullOrEmpty(ShowIf))
            copy.ShowIf = ShowIf.Replace(Id, copy.Id);
        return copy;
    }
}

public class T1Option
{
    [JsonPropertyName("answerid")] public string AnswerId { get; set; } = "";
    [JsonPropertyName("value")] public string Value { get; set; } = "";
    [JsonPropertyName("text")] public string Text { get; set; } = "";
}

public class T1SymptomGrid
{
    [JsonPropertyName("day_tabs")] public List<T1DayTabDef> DayTabs { get; set; } = new();
    [JsonPropertyName("severity_options")] public List<T1SeverityOption> SeverityOptions { get; set; } = new();
    [JsonPropertyName("symptom_groups")] public List<T1SymptomGroupDef> SymptomGroups { get; set; } = new();
    [JsonPropertyName("rash_followup")] public T1RashFollowup RashFollowup { get; set; } = new();
    [JsonPropertyName("daily_impact")] public T1DailyImpact DailyImpact { get; set; } = new();
    [JsonPropertyName("day_count_intros")] public Dictionary<string, string> DayCountIntros { get; set; } = new();
}

public class T1DayTabDef
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("show_if")] public string ShowIf { get; set; } = "";
}

public class T1SeverityOption
{
    [JsonPropertyName("value")] public string Value { get; set; } = "";
    [JsonPropertyName("text")] public string Text { get; set; } = "";
}

public class T1SymptomGroupDef
{
    [JsonPropertyName("group")] public string Group { get; set; } = "";
    [JsonPropertyName("symptoms")] public List<T1SymptomDef> Symptoms { get; set; } = new();
}

public class T1SymptomDef
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("helpertext")] public string HelperText { get; set; } = "";
}

public class T1RashFollowup
{
    [JsonPropertyName("note")] public string Note { get; set; } = "";
    [JsonPropertyName("questions")] public List<T1Question> Questions { get; set; } = new();
}

public class T1DailyImpact
{
    [JsonPropertyName("note")] public string Note { get; set; } = "";
    [JsonPropertyName("questions")] public List<T1Question> Questions { get; set; } = new();
}

public class T1NavLabels
{
    public string Back { get; set; } = "Back";
    public string Next { get; set; } = "Next";
    public string Submit { get; set; } = "Submit Questionnaire";
    public string PrevDay { get; set; } = "Prev";
    public string NextDay { get; set; } = "Next Day";
    public string LastDay { get; set; } = "Done";
}

// ===============================================================================
// ANSWER STORE
// ===============================================================================

public class T1Answers
{
    private readonly Dictionary<string, string> _single = new();
    private readonly Dictionary<string, string> _date = new();
    private readonly Dictionary<string, List<string>> _multi = new();
    private readonly Dictionary<string, int> _severity = new();
    private readonly HashSet<string> _doneDays = new();

    public string? Get(string key) => _single.TryGetValue(key, out var v) ? v : null;
    public void Set(string key, string value) => _single[key] = value;
    public bool Has(string key) => _single.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v);

    public string? dateGet(string key) => _date.TryGetValue(key, out var v) ? v : null;
    public void dateSet(string key, string value) => _date[key] = value;
    public bool dateHas(string key) => _date.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v);

    public List<string> GetMulti(string key) => _multi.TryGetValue(key, out var v) ? v : new();
    public void SetMulti(string key, List<string> values) => _multi[key] = values;

    public int GetSev(string key) => _severity.TryGetValue(key, out var v) ? v : -1;
    public void SetSev(string key, int value) => _severity[key] = value;

    public void MarkDayDone(string dayId) => _doneDays.Add(dayId);
    public bool DayCompleted(string dayId) => _doneDays.Contains(dayId);

    private string? _imageFilename;
    public void SetImageFilename(string filename) => _imageFilename = filename;

    public void ClearQuestion(string key)
    {
        _single.Remove(key);
        _multi.Remove(key);
        _severity.Remove(key);
    }

    public void ClearSev(string key) => _severity.Remove(key);

    public void ClearKeysStartingWith(string prefix)
    {
        foreach (var key in _single.Keys.Where(k => k.StartsWith(prefix)).ToList())
            _single.Remove(key);
        foreach (var key in _multi.Keys.Where(k => k.StartsWith(prefix)).ToList())
            _multi.Remove(key);
        foreach (var key in _severity.Keys.Where(k => k.StartsWith(prefix)).ToList())
            _severity.Remove(key);
    }

    // Scenario A pathogens by their value in the JSON options
    private static readonly HashSet<string> ScenarioAPathogens = new()
    {
        "2", // Influenza A
        "3", // Influenza B
        "4", // RSV (Respiratory Syncytial Virus)
        "6"  // hMPV (Human Metapneumovirus)
    };

    // True if selected LFD is specifically one of the Scenario A pathogens
    // Checks both T1 form (t1_lfd_pos_virus) and daily form (t11_lfd_pos_virus) question ids
    public bool IsLfdPositiveForScenarioA =>
        GetMulti("t1_lfd_pos_virus").Any(v => ScenarioAPathogens.Contains(v)) ||
        GetMulti("t11_lfd_pos_virus").Any(v => ScenarioAPathogens.Contains(v));

    // True if ANY LFD pathogen was selected, regardless of whether it is Scenario A
    // This is what gets stored as lfd_positive on TQuestionnaire
    public bool IsLfdPositive =>
        GetMulti("t1_lfd_pos_virus").Any() ||
        GetMulti("t11_lfd_pos_virus").Any();

    // True if member has at least one symptom rated above None (severity > 0)
    public bool HasAtLeastOneSymptom =>
        _severity.Any(kvp => kvp.Value > 0);

    public newuserquestionnaire ToSubmission(string formId, T1FormModel model, int dayNumber = 0)
    {
        var feedbacks = new ObservableCollection<Feedback>();

        // For daily forms replace t11 prefix with the actual day e.g. t6_symptoms_y_n
        string NormaliseKey(string key) =>
            dayNumber > 0 ? key.Replace("t11_", $"t{dayNumber}_") : key;

        string ResolveText(string key, string val)
        {
            var q = model.Questions.FirstOrDefault(x => x.Id == key);
            if (q != null)
            {
                if (q.Type == "date" || q.Type == "file_upload") return val;
                return q.Options?.FirstOrDefault(o => o.Value == val)?.Text ?? val;
            }
            if (model.SymptomGrid != null)
            {
                if (key.EndsWith("_y_n"))
                    return val == "1" ? "Yes" : (val == "0" ? "No" : val);
                var rashQ = model.SymptomGrid.RashFollowup.Questions.FirstOrDefault(x => key.EndsWith($"_{x.Id}"));
                if (rashQ != null) return rashQ.Options?.FirstOrDefault(o => o.Value == val)?.Text ?? val;
                var impactQ = model.SymptomGrid.DailyImpact.Questions.FirstOrDefault(x => key.EndsWith($"_{x.Id}"));
                if (impactQ != null) return impactQ.Options?.FirstOrDefault(o => o.Value == val)?.Text ?? val;
            }
            return val;
        }

        foreach (var (k, v) in _single)
        {
            string normKey = NormaliseKey(k);
            feedbacks.Add(new Feedback
            {
                questionid = normKey,
                answer = new ObservableCollection<Answer>
            {
                new Answer { answerid = $"{normKey}_{v}", answervalue = v, text = ResolveText(k, v) }
            }
            });
        }

        foreach (var (k, vals) in _multi)
        {
            string normKey = NormaliseKey(k);
            feedbacks.Add(new Feedback
            {
                questionid = normKey,
                answer = new ObservableCollection<Answer>(
                    vals.Select(v => new Answer { answerid = $"{normKey}_{v}", answervalue = v, text = ResolveText(k, v) }))
            });
        }

        foreach (var (k, sev) in _severity)
        {
            string normKey = NormaliseKey(k);
            string sevValue = (sev + 1).ToString();
            string sevText = model.SymptomGrid?.SeverityOptions.ElementAtOrDefault(sev)?.Text ?? sevValue;
            feedbacks.Add(new Feedback
            {
                questionid = normKey,
                answer = new ObservableCollection<Answer>
            {
                new Answer { answerid = normKey, answervalue = sevValue, text = sevText }
            }
            });
        }

        var submission = new newuserquestionnaire
        {
            questionnaireid = formId,
            userid = Helpers.Settings.UsersID,
            DateTimeAdded = DateTime.Now,
            FeedbackList = feedbacks,
            imagefilename = (!string.IsNullOrEmpty(_imageFilename)) ? _imageFilename : string.Empty
        };

        submission.feedback = System.Text.Json.JsonSerializer.Serialize(feedbacks);
        return submission;
    }
}
