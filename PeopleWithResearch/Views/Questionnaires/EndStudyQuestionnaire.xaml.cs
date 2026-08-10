

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls.Shapes;
using Mopups.Services;

namespace PeopleWithResearch;

public partial class EndStudyQuestionnaire : ContentPage
{
    // -- State -----------------------------------------------------------------
    private EosFormModel _model = new();
    private EosAnswers _state = new();
    private int _section = 1;

    public ObservableCollection<newuserquestionnaire> alluserquestionnaires = new();
    public householdgroup allhouseholdgroup = new();

    // -- Init ------------------------------------------------------------------
    public EndStudyQuestionnaire(
        ObservableCollection<newuserquestionnaire> questionnairesPassed,
        householdgroup housegroupinfopassed)
    {
        InitializeComponent();
        alluserquestionnaires = questionnairesPassed;
        allhouseholdgroup = housegroupinfopassed;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            SetLoading(true);
            var json = await FetchJsonAsync();
            _model = EosFormModel.FromJson(json);
            SetLoading(false);
            InitialPage.IsVisible = true;
        }
        catch
        {
            SetLoading(false);
            FailedView.IsVisible = true;
        }
    }

    private async Task<string> FetchJsonAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("endofstudyform.json");
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
    private void GoToSection(int s)
    {
        _section = s;
        ValidationBanner.IsVisible = false;

        Sec1View.IsVisible = false;
        Sec2View.IsVisible = false;
        Sec3View.IsVisible = false;
        Sec4View.IsVisible = false;

        UpdateProgress();
        UpdateNavButtons();

        switch (s)
        {
            case 1: DiffSection(Sec1Container, _model.QuestionsForSection(1)); Sec1View.IsVisible = true; break;
            case 2: DiffSection(Sec2Container, _model.QuestionsForSection(2)); Sec2View.IsVisible = true; break;
            case 3: DiffSection(Sec3Container, _model.QuestionsForSection(3)); Sec3View.IsVisible = true; break;
            case 4: DiffSection(Sec4Container, _model.QuestionsForSection(4)); Sec4View.IsVisible = true; break;
        }

        _ = Task.Delay(50).ContinueWith(_ =>
            MainThread.BeginInvokeOnMainThread(() =>
                MainScroll.ScrollToAsync(0, 0, false)));
    }

    private void DiffSection(StackLayout container, IEnumerable<EosQuestion> allQuestions)
    {
        var shouldShow = allQuestions
            .Where(q => q.Type != "info")
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
            _ = view.FadeTo(0, 120);
        }

        var currentIds = container.Children
            .OfType<View>()
            .Select(v => v.AutomationId)
            .ToList();

        for (int i = 0; i < shouldShow.Count; i++)
        {
            var q = shouldShow[i];
            if (currentIds.Contains(q.Id)) continue;

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
        var segments = new[] { Seg1, Seg2, Seg3, Seg4 };
        for (int i = 0; i < segments.Length; i++)
            segments[i].Progress = i < _section ? 100 : 0;
    }

    private void UpdateNavButtons()
    {
        bool isLast = _section == _model.TotalSections;
        NextSectionBtn.IsVisible = !isLast;
        SubmitBtn.IsVisible = isLast;
        NextSectionBtn.Text = "Continue";
        SubmitBtn.Text = "Submit Questionnaire";
        backbuttonstack.IsVisible = true;
    }

    // -- Question builder ------------------------------------------------------
    private View BuildQuestion(EosQuestion q)
    {
        return q.Type switch
        {
            "radio" => BuildRadioCard(q),
            "yesno" => BuildRadioCard(q),
            "checkbox" => BuildCheckboxCard(q),
            "date" => BuildDateCard(q),
            "text" => BuildTextCard(q),
            "slider" => BuildSliderCard(q),
            _ => BuildInfoCard(q)
        };
    }

    private View BuildInfoCard(EosQuestion q)
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#EBF5FB"),
            Stroke = Color.FromArgb("#85B7EB"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(14, 12),
            Margin = new Thickness(0, 0, 0, 12),
            Content = new Label
            {
                Text = q.Label,
                FontSize = 13,
                TextColor = Color.FromArgb("#2C6FAC"),
                LineBreakMode = LineBreakMode.WordWrap
            }
        };
    }

    private View BuildRadioCard(EosQuestion q)
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
                    }

                    var container = _section switch
                    {
                        1 => Sec1Container,
                        2 => Sec2Container,
                        3 => Sec3Container,
                        4 => Sec4Container,
                        _ => null
                    };
                    if (container != null)
                        DiffSection(container, _model.QuestionsForSection(_section));
                })
            });

            stack.Children.Add(optRow);
        }

        card.Content = stack;
        return card;
    }

    private View BuildCheckboxCard(EosQuestion q)
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

            var lbl = new Label
            {
                Text = opt.Text,
                FontSize = 13,
                TextColor = Color.FromArgb("#031926"),
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalOptions = LayoutOptions.Center
            };

            Grid.SetColumn(dotBorder, 0);
            Grid.SetColumn(lbl, 1);
            row.Children.Add(dotBorder);
            row.Children.Add(lbl);

            var capturedVal = opt.Value;
            var capturedId = q.Id;
            var capturedRow = row;
            var capturedDot = dotBorder;
            var capturedFill = (BoxView)dotBorder.Content;

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
                    capturedFill.BackgroundColor = nowSel ? Color.FromArgb("#009FE3") : Colors.Transparent;
                    capturedFill.Color = nowSel ? Color.FromArgb("#009FE3") : Colors.Transparent;

                    var container = _section switch
                    {
                        1 => Sec1Container,
                        2 => Sec2Container,
                        3 => Sec3Container,
                        4 => Sec4Container,
                        _ => null
                    };
                    if (container != null)
                        DiffSection(container, _model.QuestionsForSection(_section));
                })
            });

            stack.Children.Add(row);
        }

        card.Content = stack;
        return card;
    }

    private View BuildDateCard(EosQuestion q)
    {
        var entry = new Entry
        {
            Placeholder = q.Placeholder ?? "DD/MM/YYYY",
            Text = _state.Get(q.Id) ?? "",
            FontFamily = "OpenSansSemiBold",
            Keyboard = Keyboard.Numeric,
            FontSize = 14,
            TextColor = Color.FromArgb("#031926"),
            PlaceholderColor = Color.FromArgb("#9CA3AF")
        };

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

    private View BuildTextCard(EosQuestion q)
    {
        var editor = new Editor
        {
            Placeholder = q.Placeholder ?? "Please share your thoughts here...",
            Text = _state.Get(q.Id) ?? "",
            FontFamily = "OpenSansRegular",
            FontSize = 14,
            TextColor = Color.FromArgb("#031926"),
            PlaceholderColor = Color.FromArgb("#9CA3AF"),
            AutoSize = EditorAutoSizeOption.TextChanges,
            MinimumHeightRequest = 100
        };

        var capturedId = q.Id;
        editor.TextChanged += (s, e) => _state.Set(capturedId, e.NewTextValue ?? "");

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
                    new Border
                    {
                        BackgroundColor = Color.FromArgb("#F8F9FA"),
                        Stroke = Color.FromArgb("#E8ECF0"),
                        StrokeThickness = 1,
                        StrokeShape = new RoundRectangle { CornerRadius = 8 },
                        Padding = new Thickness(10, 8),
                        Content = editor
                    }
                }
            }
        };
    }

    private View BuildSliderCard(EosQuestion q)
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

        var stack = new StackLayout { Spacing = 12 };

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

        // Default to 50 so it counts as answered immediately
        int initialVal = _state.HasSlider(q.Id) ? _state.GetSlider(q.Id) : 50;
        _state.SetSlider(q.Id, initialVal);

        var valueLabel = new Label
        {
            Text = initialVal.ToString(),
            FontFamily = "OpenSansSemiBold",
            FontAttributes = FontAttributes.Bold,
            FontSize = 32,
            TextColor = Color.FromArgb("#0D9488"),
            HorizontalOptions = LayoutOptions.Center
        };

        var slider = new Slider
        {
            Minimum = q.Min,
            Maximum = q.Max,
            Value = initialVal,
            MinimumTrackColor = Color.FromArgb("#0D9488"),
            MaximumTrackColor = Color.FromArgb("#E2E8F0"),
            ThumbColor = Color.FromArgb("#0D9488"),
            Margin = new Thickness(0, 4, 0, 0)
        };

        var capturedId = q.Id;
        slider.ValueChanged += (s, e) =>
        {
            int rounded = (int)Math.Round(e.NewValue);
            _state.SetSlider(capturedId, rounded);
            valueLabel.Text = rounded.ToString();
            ValidationBanner.IsVisible = false;
        };

        var minMaxRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star })
        };

        minMaxRow.Children.Add(new Label
        {
            Text = q.MinLabel ?? "0",
            FontSize = 11,
            TextColor = Color.FromArgb("#62727B"),
            HorizontalOptions = LayoutOptions.Start,
            LineBreakMode = LineBreakMode.WordWrap
        });

        var maxLbl = new Label
        {
            Text = q.MaxLabel ?? "100",
            FontSize = 11,
            TextColor = Color.FromArgb("#62727B"),
            HorizontalOptions = LayoutOptions.End,
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalTextAlignment = TextAlignment.End
        };
        Grid.SetColumn(maxLbl, 1);
        minMaxRow.Children.Add(maxLbl);

        stack.Children.Add(valueLabel);
        stack.Children.Add(slider);
        stack.Children.Add(minMaxRow);

        card.Content = stack;
        return card;
    }

    // -- Visibility / branching ------------------------------------------------
    private bool IsVisible(EosQuestion q)
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

        if (expr.Contains(" == "))
        {
            var parts = expr.Split(new[] { " == " }, 2, StringSplitOptions.None);
            var field = parts[0].Trim();
            var value = parts[1].Trim().Trim('\'');
            return _state.Get(field) == value;
        }

        return true;
    }

    // -- Navigation ------------------------------------------------------------
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

        int next = _section + 1;
        if (next <= _model.TotalSections) GoToSection(next);
    }

    private void BackButtonTapped_Tapped(object sender, TappedEventArgs e)
    {
        if (_section == 1)
        {
            Navigation.PopAsync();
            return;
        }
        GoToSection(_section - 1);
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
            var submission = _state.ToSubmission("end_of_study", _model);
            await APICalls.Instance.PostUserQuestionnaire(submission);
            alluserquestionnaires.Add(submission);

            // Mark end of study complete on the household study record
            var studyDetails = allhouseholdgroup.studydetails;
            if (studyDetails != null)
            {
                var activeEvent = studyDetails.t_events?
                    .FirstOrDefault(x => x.t_event_status == "active" ||
                                         x.t_event_status == "complete");

                if (activeEvent != null)
                {
                    activeEvent.t_event_status = "complete";
                    studyDetails.current_phase = "AwaitingSamples";

                    allhouseholdgroup.details = System.Text.Json.JsonSerializer.Serialize(studyDetails);
                    var updateData = new { details = allhouseholdgroup.details };
                    string updateJson = System.Text.Json.JsonSerializer.Serialize(updateData);
                    var url = $"{APICalls.ApplicationURL}householdgroup/householdgroupid/{allhouseholdgroup.householdgroupid}";
                    var content = new StringContent(updateJson, Encoding.UTF8, "application/json");
                    await APICalls.Instance.GetClient().PatchAsync(url, content);
                }
            }

            WeakReferenceMessenger.Default.Send(new UpdateDashCompelted(alluserquestionnaires.ToList()));
            await MopupService.Instance.PushAsync(
                new PopupPageHelper("End of Study Questionnaire Submitted — Thank you for taking part in HOPPER!"));
            await Task.Delay(2500);
            Navigation.RemovePage(this);
            await MopupService.Instance.PopAllAsync(false);
        }
        catch
        {
            await DisplayAlert("Error", "Submission failed. Please try again.", "OK");
        }
    }

    // -- Validation ------------------------------------------------------------
    private bool ValidateSection(int s, out string message)
    {
        var unanswered = _model.QuestionsForSection(s)
            .Where(q => q.Required && IsVisible(q))
            .Where(q => q.Type is "radio" or "yesno"
                        ? !_state.Has(q.Id)
                        : q.Type == "checkbox"
                            ? _state.GetMulti(q.Id).Count == 0
                            : q.Type == "date"
                                ? !_state.Has(q.Id)
                                : q.Type == "slider"
                                    ? !_state.HasSlider(q.Id)
                                    : false)
            .ToList();

        // Date format validation
        foreach (var q in _model.QuestionsForSection(s).Where(q => IsVisible(q) && q.Type == "date"))
        {
            var dateText = _state.Get(q.Id);
            if (!string.IsNullOrWhiteSpace(dateText))
            {
                if (!DateTime.TryParseExact(dateText, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                {
                    message = $"Please enter a valid date for: {q.Label.Split('.')[0]}";
                    return false;
                }
                if (parsed.Date > DateTime.Today)
                {
                    message = $"Please enter a date that is not in the future for: {q.Label.Split('.')[0]}";
                    return false;
                }
            }
        }

        if (unanswered.Any())
        {
            message = $"Please answer: {unanswered.First().Label.Split('.')[0]}";
            return false;
        }

        message = "";
        return true;
    }

    private void OnTryAgainClicked(object sender, EventArgs e)
    {
        FailedView.IsVisible = false;
        _ = LoadAsync();
    }
}

// ===============================================================================
// JSON MODEL
// ===============================================================================

public class EosFormModel
{
    public string FormId { get; set; } = "end_of_study";
    public int TotalSections { get; set; } = 4;
    public List<EosQuestion> Questions { get; set; } = new();

    public IEnumerable<EosQuestion> QuestionsForSection(int s) =>
        Questions.Where(q => q.Section == s).OrderBy(q => q.Order);

    public static EosFormModel FromJson(string json)
    {
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var raw = JsonSerializer.Deserialize<List<EosQuestion>>(json, opts)
                  ?? new List<EosQuestion>();

        var model = new EosFormModel { Questions = raw };

        var maxSection = raw
            .Where(q => q.Type != "info")
            .Select(q => q.Section)
            .DefaultIfEmpty(4)
            .Max();
        model.TotalSections = maxSection;

        return model;
    }
}

public class EosQuestion
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("required")] public bool Required { get; set; }
    [JsonPropertyName("options")] public List<EosOption> Options { get; set; } = new();
    [JsonPropertyName("placeholder")] public string? Placeholder { get; set; }
    [JsonPropertyName("order")] public int Order { get; set; }
    [JsonPropertyName("questionid")] public string QuestionId { get; set; } = "";
    [JsonPropertyName("section")] public int Section { get; set; }
    [JsonPropertyName("section_label")] public string SectionLabel { get; set; } = "";
    [JsonPropertyName("show_if")] public string? ShowIf { get; set; }
    [JsonPropertyName("min")] public double Min { get; set; } = 0;
    [JsonPropertyName("max")] public double Max { get; set; } = 100;
    [JsonPropertyName("min_label")] public string? MinLabel { get; set; }
    [JsonPropertyName("max_label")] public string? MaxLabel { get; set; }
}

public class EosOption
{
    [JsonPropertyName("answerid")] public string AnswerId { get; set; } = "";
    [JsonPropertyName("value")] public string Value { get; set; } = "";
    [JsonPropertyName("text")] public string Text { get; set; } = "";
}

// ===============================================================================
// ANSWER STORE
// ===============================================================================

public class EosAnswers
{
    private readonly Dictionary<string, string> _single = new();
    private readonly Dictionary<string, List<string>> _multi = new();
    private readonly Dictionary<string, int> _sliders = new();

    public string? Get(string key) => _single.TryGetValue(key, out var v) ? v : null;
    public void Set(string key, string value) => _single[key] = value;
    public bool Has(string key) => _single.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v);

    public List<string> GetMulti(string key) => _multi.TryGetValue(key, out var v) ? v : new();
    public void SetMulti(string key, List<string> values) => _multi[key] = values;

    public int GetSlider(string key) => _sliders.TryGetValue(key, out var v) ? v : 50;
    public void SetSlider(string key, int value) => _sliders[key] = value;
    public bool HasSlider(string key) => _sliders.ContainsKey(key);

    public newuserquestionnaire ToSubmission(string formId, EosFormModel model)
    {
        var feedbacks = new ObservableCollection<Feedback>();

        string ResolveText(string key, string val)
        {
            var q = model.Questions.FirstOrDefault(x => x.Id == key);
            if (q == null) return val;
            if (q.Type == "date" || q.Type == "text" || q.Type == "slider") return val;
            return q.Options?.FirstOrDefault(o => o.Value == val)?.Text ?? val;
        }

        foreach (var (k, v) in _single)
        {
            feedbacks.Add(new Feedback
            {
                questionid = k,
                answer = new ObservableCollection<Answer>
                {
                    new Answer { answerid = $"{k}_{v}", answervalue = v, text = ResolveText(k, v) }
                }
            });
        }

        foreach (var (k, vals) in _multi)
        {
            feedbacks.Add(new Feedback
            {
                questionid = k,
                answer = new ObservableCollection<Answer>(
                    vals.Select(v => new Answer
                    {
                        answerid = $"{k}_{v}",
                        answervalue = v,
                        text = ResolveText(k, v)
                    }))
            });
        }

        foreach (var (k, v) in _sliders)
        {
            feedbacks.Add(new Feedback
            {
                questionid = k,
                answer = new ObservableCollection<Answer>
                {
                    new Answer { answerid = $"{k}_{v}", answervalue = v.ToString(), text = v.ToString() }
                }
            });
        }

        var submission = new newuserquestionnaire
        {
            questionnaireid = formId,
            userid = Helpers.Settings.UsersID,
            DateTimeAdded = DateTime.Now,
            FeedbackList = feedbacks,
            imagefilename = string.Empty
        };

        submission.feedback = System.Text.Json.JsonSerializer.Serialize(feedbacks);
        return submission;
    }
}