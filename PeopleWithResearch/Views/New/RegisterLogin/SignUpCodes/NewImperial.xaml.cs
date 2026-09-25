using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Mopups.Services;
using PeopleWithResearch;
using Syncfusion.Maui.Core.Carousel;
using Syncfusion.Maui.Core.Internals;
using System.Collections.ObjectModel;
#if IOS
using UIKit;
# endif

namespace PeopleWithResearch;

/// <summary>
/// Code-behind for NewImperial.xaml. Deliberately thin — its jobs are: build the viewmodel
/// for whichever navigation context this page was constructed with (mirrors the four
/// Imperial(...) overloads), forward the things a viewmodel genuinely can't do itself (reset
/// scroll position, pop the page, show a Mopups popup, refresh SfListView controls after
/// navigation) from their events to the actual controls, and forward the health-conditions
/// autocomplete's own SelectionChanged event (a multi-select "pick one, clear the field, add
/// a chip" flow isn't expressible as a plain property binding). Everything else lives in
/// NewImperialViewModel.cs.
/// </summary>
public partial class NewImperial : ContentPage
{
    public NewImperialViewModel viewmodel { get; }
    private bool _isRestoringFluSelection;
    private bool _isRestoringExtraSelection;
    private bool _isRestoringWhatDrugsSelection;
    private bool _isRestoringTobaccoSelection;

    // Lazily built once, then reused — the page's control tree doesn't change shape after
    // InitializeComponent (every section's StackLayout exists throughout; only IsVisible
    // toggles), so there's no need to re-walk it on every navigation.
    private List<Syncfusion.Maui.ListView.SfListView>? _allListViews;

    /// <summary>Was: Imperial().</summary>
    public NewImperial()
    {
        InitializeComponent();
        viewmodel = new NewImperialViewModel(new AlertService());
        BindingContext = viewmodel;
        WireviewmodelEvents();



    }

    /// <summary>Was: Imperial(user, advert, Questionnaire).</summary>
    public NewImperial(user userPassed, advert signupDetailsPassed, Questionnaire questionnairePassed)
    {
        InitializeComponent();
        viewmodel = new NewImperialViewModel(new AlertService());
        BindingContext = viewmodel;
        WireviewmodelEvents();

        viewmodel.ConfigureForHouseholdRegistration(userPassed, signupDetailsPassed, questionnairePassed);
        _ = viewmodel.LoadRegistrationConfigAsync();
    }

    /// <summary>Was: Imperial(user, signupcode).</summary>
    public NewImperial(user userPassed, signupcode signupDetailsPassed)
    {
        InitializeComponent();
        viewmodel = new NewImperialViewModel(new AlertService());
        BindingContext = viewmodel;
        WireviewmodelEvents();

        viewmodel.ConfigureForSignupCode(userPassed, signupDetailsPassed);

        


        _ = viewmodel.LoadRegistrationConfigAsync();
    }

    /// <summary>Was: Imperial(signupcode, householdgroupjsondetails, ObservableCollection&lt;householdgroupjsondetails&gt;, householdgroup).</summary>
    public NewImperial(signupcode signupDetailsPassed, householdgroupjsondetails userInfoPassed,
        ObservableCollection<householdgroupjsondetails> allGroupDetails, householdgroup passedHousehold, bool FromDash = false)
    {
        InitializeComponent();
        if (MopupService.Instance.PopupStack.Count > 0)
            MopupService.Instance.PopAsync();
        viewmodel = new NewImperialViewModel(new AlertService());
        BindingContext = viewmodel;
        WireviewmodelEvents();
        viewmodel.HHRepFromDash = FromDash;
        viewmodel.ConfigureForHouseholdMember(signupDetailsPassed, userInfoPassed, allGroupDetails, passedHousehold);
        _ = viewmodel.LoadRegistrationConfigAsync();
    }



    private void WireviewmodelEvents()
    {
        viewmodel.ScrollResetRequested += async () =>
        {
            if (mainscrollview.ScrollY > 0)
            {
                await mainscrollview.ScrollToAsync(0, 0, true);
            }
        };

        viewmodel.RequestClosePage += () => Navigation.RemovePage(this);

        viewmodel.InfoRequested += async (text) =>
        {
            await MopupService.Instance.PushAsync(new Infopopup(text, viewmodel.RegistrationSections));
        };

        viewmodel.SignatureClearRequested += () =>
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                signpad.Clear();
            }
            else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                drawingpad.Clear();
            }
        };

        // Assent signature clear (13-15 age group).
        viewmodel.AssentSignatureClearRequested += () =>
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                assentSignpad.Clear();
            }
            else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                assentDrawingpad.Clear();
            }
        };

        // Used by SubmitAsync (once it's ported) to pull the captured signature image bytes
        // for upload, without the viewmodel needing to hold a reference to either pad.
        viewmodel.RequestSignatureImageStream = async (cancellationToken) =>
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                return await signpad.GetStreamAsync(Syncfusion.Maui.Core.ImageFileFormat.Png);
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                return await drawingpad.GetImageStream(150, 150, cancellationToken);
            }

            return null;
        };

        // Assent signature stream (13-15 age group).
        viewmodel.RequestAssentSignatureImageStream = async (cancellationToken) =>
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                return await assentSignpad.GetStreamAsync(Syncfusion.Maui.Core.ImageFileFormat.Png);
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                return await assentDrawingpad.GetImageStream(150, 150, cancellationToken);
            }

            return null;
        };

        viewmodel.SubmissionCompleted += async () =>
        {
            await MopupService.Instance.PushAsync(new PopupPageHelper(true, viewmodel.HHRepFromDash));
            await App.SetMainPage(new ImperialDashboard());
        };

        viewmodel.SelectionFluRestore += async () =>
        {
            RestoreFluSelection();
        };

        viewmodel.SelecteionExtraRestore += async () =>
        {
            RestoreExtraSelection();
        };

        viewmodel.SelecteionWhatDrugsRestore += async () =>
        {
            RestoreDrugsSelection();
        };


        viewmodel.SelecteionTobaccoRestore += async () =>
        {
            RestoretobaccoSelection();
        };


        // Was: the original's pervasive `xxxlist.RefreshView()` calls, applied every single
        // time a list becomes newly visible — Syncfusion's SfListView doesn't reliably
        // reapply its SelectedItemTemplate (and, per real-world testing, can stop responding
        // to taps at all) after its ItemsSource changes while hidden, particularly on Android.
        // The original called RefreshView() by hand at each individual reveal site; rather
        // than replicate that call-by-call, this listens for any "IsXVisible"-shaped property
        // changing — on the main viewmodel (covers both cross-section navigation, since
        // CurrentSectionKey drives every IsXStackVisible, and within-section reveals like
        // IsGenderMatchVisible) and on Member1/Member2 (mainuserstack's own reveal flags) —
        // and refreshes every list on the page whenever one fires.
        viewmodel.PropertyChanged += OnviewmodelPropertyChanged;
        viewmodel.Member1.PropertyChanged += OnviewmodelPropertyChanged;
        viewmodel.Member2.PropertyChanged += OnviewmodelPropertyChanged;

        // FIX: SfListView.SelectedItems has no accessible setter, so SelectedFluOptions /
        // SelectedExtraOptions can't be TwoWay-bound to it — fluListView/extraListView's
        // SelectionChanged handlers below keep those viewmodel collections in sync instead.
        // This one covers the reverse direction: when the viewmodel clears its mirror
        // collection programmatically (hiding the "take extra nutrients" follow-up), the
        // control's own selection needs clearing too, since nothing else will do it.
        viewmodel.ExtraSelectionClearRequested += () => extraListView.SelectedItems?.Clear();



        WeakReferenceMessenger.Default.Register<ValueChangedMessage<bool>>(this, async (r, m) =>
        {
            await Task.Delay(100);
            GpPracticeAutocomplete.Unfocus();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<ValueChangedMessage<bool>>(this);
    }

    private void OnviewmodelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        string? name = e.PropertyName;
        if (name is not null && name.StartsWith("Is", StringComparison.Ordinal) && name.EndsWith("Visible", StringComparison.Ordinal))
        {
            RefreshAllListViews();
        }
    }

    /// <summary>Was (partly): flulist_SelectionChanged reading flulist.SelectedItems directly
    /// — same pattern here, since SelectedItems can only be read from code, never bound.</summary>
    /// 
    private void FluListView_SelectionChanged(object sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
    {
        if (_isRestoringFluSelection) return;
        if (viewmodel?.SelectedFluOptions == null) return;
        viewmodel.FluGateError = string.Empty;

        UpdateSelectedItems(viewmodel.SelectedFluOptions, e);
    }

    private void ExtraListView_SelectionChanged(object sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
    {
        if (_isRestoringExtraSelection) return;
        if (viewmodel?.SelectedExtraOptions == null) return;
        viewmodel.ExtraError = string.Empty;

        UpdateSelectedItems(viewmodel.SelectedExtraOptions, e);
    }

    private void TobaccoTypesListview_Selectionnewimhanged(object sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
    {
        if (_isRestoringTobaccoSelection) return;
        if (viewmodel?.SelectedSmokeTypesOption == null) return;
        viewmodel.SmokeTypesError = string.Empty;

        UpdateSelectedItems(viewmodel.SelectedSmokeTypesOption, e);
    }

    private void WhatDrugsListview_SelectionChanged(object sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
    {
        if (_isRestoringWhatDrugsSelection) return;
        if (viewmodel?.SelectedWhatDrugsOption == null) return;
        viewmodel.WhatDrugsError = string.Empty;

        UpdateSelectedItems(viewmodel.SelectedWhatDrugsOption, e);
    }

    private static void UpdateSelectedItems(ObservableCollection<OptionDetails> target, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
    {
        if (target == null) return;

        // 1. Remove items that were unselected
        if (e.RemovedItems != null)
        {
            foreach (var item in e.RemovedItems)
            {
                if (item is OptionDetails option)
                {
                    target.Remove(option);
                }
            }
        }

        // 2. Add newly selected items
        if (e.AddedItems != null)
        {
            foreach (var item in e.AddedItems)
            {
                if (item is OptionDetails option && !target.Contains(option))
                {
                    target.Add(option);
                }
            }
        }
    }

    private void RestoreFluSelection()
    {
        if (fluListView.SelectedItems is null) return;

        _isRestoringFluSelection = true;
        try
        {
            fluListView.SelectedItems.Clear();
            foreach (var saved in viewmodel.SelectedFluOptions)
            {
                // Match by value (IEquatable on OptionDetails) so a rebuilt FluOptions
                // instance still resolves to the saved selection.
                var match = viewmodel.FluOptions.FirstOrDefault(o => o.Equals(saved));
                if (match is not null && !fluListView.SelectedItems.Contains(match))
                    fluListView.SelectedItems.Add(match);
            }
        }
        finally
        {
            _isRestoringFluSelection = false;
        }
    }

    private void RestoreExtraSelection()
    {
        if (extraListView.SelectedItems is null) return;

        _isRestoringExtraSelection = true;
        try
        {
            extraListView.SelectedItems.Clear();
            foreach (var saved in viewmodel.SelectedExtraOptions)
            {
                var match = viewmodel.ExtraOptions.FirstOrDefault(o => o.Equals(saved));
                if (match is not null && !extraListView.SelectedItems.Contains(match))
                    extraListView.SelectedItems.Add(match);
            }
        }
        finally
        {
            _isRestoringExtraSelection = false;
        }
    }

    private void RestoreDrugsSelection()
    {
        if (WhatDrugsListview.SelectedItems is null || viewmodel.SelectedWhatDrugsOption is null) return;

        _isRestoringWhatDrugsSelection = true;
        try
        {
            WhatDrugsListview.SelectedItems.Clear();
            foreach (var saved in viewmodel.SelectedWhatDrugsOption)
            {
                var match = viewmodel.WhatDrugsOptions.FirstOrDefault(o => o.Equals(saved));
                if (match is not null && !WhatDrugsListview.SelectedItems.Contains(match))
                    WhatDrugsListview.SelectedItems.Add(match);
            }
        }
        finally
        {
            _isRestoringWhatDrugsSelection = false;
        }
    }

    private void RestoretobaccoSelection()
    {
        if (TobaccoTypesListview.SelectedItems is null || viewmodel.SelectedSmokeTypesOption is null) return;

        _isRestoringTobaccoSelection = true;
        try
        {
            TobaccoTypesListview.SelectedItems.Clear();
            foreach (var saved in viewmodel.SelectedSmokeTypesOption)
            {
                var match = viewmodel.SmokeTypesOptions.FirstOrDefault(o => o.Equals(saved));
                if (match is not null && !TobaccoTypesListview.SelectedItems.Contains(match))
                    TobaccoTypesListview.SelectedItems.Add(match);
            }
        }
        finally
        {
            _isRestoringTobaccoSelection = false;
        }
    }

    //private void FluListView_SelectionChanged(object sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
    //{
    //    SyncSelectedItems(fluListView, viewmodel.SelectedFluOptions);
    //}

    //private void ExtraListView_SelectionChanged(object sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
    //{
    //    SyncSelectedItems(extraListView, viewmodel.SelectedExtraOptions);
    //}

    //private static void SyncSelectedItems(Syncfusion.Maui.ListView.SfListView listView, ObservableCollection<OptionDetails> target)
    //{
    //    target.Clear();
    //    if (listView.SelectedItems is null) return;

    //    foreach (var selected in listView.SelectedItems)
    //    {
    //        if (selected is OptionDetails option)
    //        {
    //            target.Add(option);
    //        }
    //    }
    //}

    //private void RefreshAllListViews()
    //{
    //    _allListViews ??= FindDescendants<Syncfusion.Maui.ListView.SfListView>(this).ToList();

    //    foreach (var listView in _allListViews)
    //    {
    //        listView.RefreshView();
    //    }
    //}

    private void RefreshAllListViews()
    {
        _allListViews ??= FindDescendants<Syncfusion.Maui.ListView.SfListView>(this).ToList();

        foreach (var listView in _allListViews.Where(IsVisibleOnScreen))
        {
            listView.RefreshView();
        }
    }

    private static bool IsVisibleOnScreen(VisualElement element)
    {
        if (element is null || !element.IsLoaded)
            return false;

        Element current = element;
        while (current is VisualElement visualParent)
        {
            if (!visualParent.IsVisible)
                return false;

            current = visualParent.Parent;
        }

        return true;
    }

    private static IEnumerable<T> FindDescendants<T>(Microsoft.Maui.IVisualTreeElement root) where T : class
    {
        foreach (var child in root.GetVisualChildren())
        {
            if (child is T match)
            {
                yield return match;
            }

            foreach (var nested in FindDescendants<T>(child))
            {
                yield return nested;
            }
        }
    }

    /// <summary>Was: drawingpad_DrawingLineCompleted (iOS signature capture).</summary>
    private async void DrawingPad_DrawingLineCompleted(object sender, CommunityToolkit.Maui.Core.DrawingLineCompletedEventArgs e)
    {
        if (drawingpad.Lines is null || drawingpad.Lines.Count == 0)
        {
            viewmodel.SetSignatureCaptured(false);
            return;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var drawingStream = await drawingpad.GetImageStream(150, 150, cts.Token);

        bool isSignatureBlank = drawingStream is null;
        if (!isSignatureBlank)
        {
            using var ms = new MemoryStream();
            await drawingStream!.CopyToAsync(ms);
            isSignatureBlank = ms.Length == 0;
        }

        viewmodel.SetSignatureCaptured(!isSignatureBlank);
    }

    /// <summary>Was: signpad_DrawCompleted (Android signature capture).</summary>
    private async void SignPad_DrawCompleted(object sender, EventArgs e)
    {
        using var signatureStream = await signpad.GetStreamAsync(Syncfusion.Maui.Core.ImageFileFormat.Png);

        bool isSignatureBlank = signatureStream is null;
        if (!isSignatureBlank)
        {
            using var ms = new MemoryStream();
            await signatureStream!.CopyToAsync(ms);
            isSignatureBlank = ms.Length == 0;
        }

        viewmodel.SetSignatureCaptured(!isSignatureBlank);
    }

    /// <summary>Assent DrawingView completed (iOS, 13-15 age group).</summary>
    private async void AssentDrawingPad_DrawingLineCompleted(object sender, CommunityToolkit.Maui.Core.DrawingLineCompletedEventArgs e)
    {
        if (assentDrawingpad.Lines is null || assentDrawingpad.Lines.Count == 0)
        {
            viewmodel.SetAssentSignatureCaptured(false);
            return;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var drawingStream = await assentDrawingpad.GetImageStream(150, 150, cts.Token);

        bool isSignatureBlank = drawingStream is null;
        if (!isSignatureBlank)
        {
            using var ms = new MemoryStream();
            await drawingStream!.CopyToAsync(ms);
            isSignatureBlank = ms.Length == 0;
        }

        viewmodel.SetAssentSignatureCaptured(!isSignatureBlank);
    }

    /// <summary>Assent SfSignaturePad completed (Android, 13-15 age group).</summary>
    private async void AssentSignPad_DrawCompleted(object sender, EventArgs e)
    {
        using var signatureStream = await assentSignpad.GetStreamAsync(Syncfusion.Maui.Core.ImageFileFormat.Png);

        bool isSignatureBlank = signatureStream is null;
        if (!isSignatureBlank)
        {
            using var ms = new MemoryStream();
            await signatureStream!.CopyToAsync(ms);
            isSignatureBlank = ms.Length == 0;
        }

        viewmodel.SetAssentSignatureCaptured(!isSignatureBlank);
    }

    /// <summary>Was: disautocomplete_SelectionChanged's add-to-list half (the "clear the field
    /// after picking" half stays here too, since SfAutocomplete.Clear() is a control method,
    /// not a bindable property).</summary>
    private async void ConditionAutocomplete_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not { Count: > 0 } || e.AddedItems[0] is not OptionDetails item)
        {
            return;
        }

        viewmodel.AddConditionCommand.Execute(item);

        if (sender is Syncfusion.Maui.Inputs.SfAutocomplete autocomplete)
        {
            await DismissAutocompleteAsync(autocomplete);
        }
    }

    /// <summary>Was: medautocomplete_SelectionChanged's add-to-list half — same reasoning as
    /// ConditionAutocomplete_SelectionChanged above.</summary>
    private async void MedicationAutocomplete_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not { Count: > 0 } || e.AddedItems[0] is not OptionDetails item)
        {
            return;
        }

        viewmodel.AddMedicationCommand.Execute(item);

        if (sender is Syncfusion.Maui.Inputs.SfAutocomplete autocomplete)
        {
            await DismissAutocompleteAsync(autocomplete);
        }
    }

    private static async Task DismissAutocompleteAsync(Syncfusion.Maui.Inputs.SfAutocomplete autocomplete)
    {
        autocomplete.IsDropDownOpen = false;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            autocomplete.Unfocus();
            HideKeyboard(autocomplete);
            await Task.Delay(50);
            autocomplete.Clear();
            autocomplete.Unfocus();
            autocomplete.IsDropDownOpen = false;
            HideKeyboard(autocomplete);
            autocomplete.IsEnabled = false;
            await Task.Delay(50);
            autocomplete.IsEnabled = true;
            return;
        }

        autocomplete.Clear();
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            await Task.Delay(100);
        }

        autocomplete.Unfocus();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new PrivacyPolicyPage(), false);
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, "TapGestureRecognizer_Tapped"); 
        }
    }

    private async void SfAutocomplete_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        try
        {
            if (sender is not Syncfusion.Maui.Inputs.SfAutocomplete autocomplete) return;

            autocomplete.IsDropDownOpen = false;


            if(DeviceInfo.Platform == DevicePlatform.iOS)
            {
                GpPracticeAutocomplete.Unfocus();
                HideKeyboard();
            }
            else
            {
                await Task.Delay(150);
                autocomplete.Unfocus();
                GpPracticeAutocomplete.Unfocus();
                HideKeyboard(autocomplete);
            }
  
            //GpPracticeAutocomplete.Text = string.Empty;
        }
        catch (Exception Ex)
        {
            CrashDetected.LogCrash(Ex, "SfAutocomplete_SelectionChanged");
        }
    }


    private static void HideKeyboard(VisualElement? focusedElement = null)
    {
#if ANDROID
    var activity = Platform.CurrentActivity;
    var platformView = focusedElement?.Handler?.PlatformView as Android.Views.View;
    var focusedView = activity?.CurrentFocus ?? platformView;
    if (focusedView is not null)
    {
        var inputMethodManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.InputMethodService)
            as Android.Views.InputMethods.InputMethodManager;
        inputMethodManager?.HideSoftInputFromWindow(focusedView.WindowToken, Android.Views.InputMethods.HideSoftInputFlags.None);
        focusedView.ClearFocus();
    }
#elif IOS 
        var keyWindow = UIKit.UIApplication.SharedApplication.ConnectedScenes
            .OfType<UIWindowScene>()
            .SelectMany(scene => scene.Windows)
            .FirstOrDefault(window => window.IsKeyWindow);
        keyWindow?.EndEditing(true);
#endif
    }

  private bool _isReverting;

// private void WeightEntry_TextChanged(object sender, TextChangedEventArgs e)
// {
//     if (_isReverting) return;

//     var entry = (Entry)sender;

//     try
//     {
//         var weight = e.NewTextValue;
//         if (string.IsNullOrWhiteSpace(weight)) return;

//         var getSelectedunit = viewmodel.WeightUnitSuffix;
//         if (getSelectedunit == null) return;

//         // Allow partial numeric input (e.g. "12.") to pass through untouched,
//         // otherwise the user can never type a decimal point.
//         if (!double.TryParse(weight, out double weightValue))
//         {
//             if (IsPartialNumeric(weight)) return;
//             RevertText(entry, e.OldTextValue);
//             return;
//         }

//         bool isValid;

//         if (getSelectedunit.Contains("kg", StringComparison.OrdinalIgnoreCase))
//         {
//             isValid = weightValue >= 0.1 && weightValue <= 128.0;
//         }
//         else if (getSelectedunit.Equals("st", StringComparison.OrdinalIgnoreCase))
//         {
//             isValid = weightValue >= 0.0157 && weightValue <= 20.1565;
//         }
//         else
//         {
//             isValid = true; 
//         }

//         if (!isValid)
//         {
//             RevertText(entry, e.OldTextValue);
//         }
//     }
//     catch (Exception ex)
//     {
//         // consider logging ex here
//     }
// }

// private void RevertText(Entry entry, string oldValue)
// {
//     _isReverting = true;
//     entry.Text = oldValue;
//     entry.CursorPosition = oldValue?.Length ?? 0;
//     _isReverting = false;
// }

// private static bool IsPartialNumeric(string text)
// {
//     return text.Count(c => c == '.') <= 1 &&
//            text.All(c => char.IsDigit(c) || c == '.');
// }
}
