// TDD: Written before production code. Will compile after sub-write-code completes.

using FluentAssertions;
using PeopleWithResearch;
using Xunit;

namespace PeopleWithResearch.UnitTests.ViewModels;

/// <summary>
/// Tests for ImperialViewModel INotifyPropertyChanged behaviour.
/// Verifies SetProperty raises PropertyChanged correctly for each bindable property.
/// AC1: INPC wiring is part of the migrated state logic.
/// </summary>
public class ImperialViewModelStateTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // IsBusy
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsBusy_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.IsBusy = true;

        raised.Should().Contain(nameof(ImperialViewModel.IsBusy));
    }

    [Fact]
    public void IsBusy_SetToSameValue_DoesNotRaisePropertyChanged()
    {
        var vm = new ImperialViewModel();
        vm.IsBusy = false; // already false by default
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.IsBusy = false; // same value — SetProperty should short-circuit

        raised.Should().NotContain(nameof(ImperialViewModel.IsBusy));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // BannerText / BannerVisible
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void BannerText_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.BannerText = "You are completing this on behalf of Test Member";

        raised.Should().Contain(nameof(ImperialViewModel.BannerText));
    }

    [Fact]
    public void BannerVisible_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.BannerVisible = true;

        raised.Should().Contain(nameof(ImperialViewModel.BannerVisible));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // ProgressAmount
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ProgressAmount_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.ProgressAmount = 50;

        raised.Should().Contain(nameof(ImperialViewModel.ProgressAmount));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // StepTitle / StepSubtitle / StepHelpText / HelpTextVisible
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void StepTitle_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.StepTitle = "Your Details";

        raised.Should().Contain(nameof(ImperialViewModel.StepTitle));
    }

    [Fact]
    public void StepSubtitle_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.StepSubtitle = "Tell us about yourself";

        raised.Should().Contain(nameof(ImperialViewModel.StepSubtitle));
    }

    [Fact]
    public void HelpTextVisible_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.HelpTextVisible = true;

        raised.Should().Contain(nameof(ImperialViewModel.HelpTextVisible));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // CurrentStep also raises CurrentFieldIndex (via SetProperty linkage)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CurrentFieldIndex_WhenChanged_RaisesPropertyChangedForCurrentStep()
    {
        var vm = new ImperialViewModel();
        vm.Allregfields.Add(new RegField { XamlNameArea = "namestack", FieldTitle = "Name" });
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.CurrentFieldIndex = 1;

        // Per plan: CurrentFieldIndex setter calls OnPropertyChanged(nameof(CurrentStep))
        raised.Should().Contain(nameof(ImperialViewModel.CurrentStep));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Validation error properties (INPC)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void NameStackError_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.NameStackError = true;

        raised.Should().Contain(nameof(ImperialViewModel.NameStackError));
    }

    [Fact]
    public void NameStackErrorMessage_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.NameStackErrorMessage = "First name is required";

        raised.Should().Contain(nameof(ImperialViewModel.NameStackErrorMessage));
    }

    [Fact]
    public void GenderStackError_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.GenderStackError = true;

        raised.Should().Contain(nameof(ImperialViewModel.GenderStackError));
    }

    [Fact]
    public void AddressStackError_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.AddressStackError = true;

        raised.Should().Contain(nameof(ImperialViewModel.AddressStackError));
    }

    [Fact]
    public void TandCsStackError_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.TandCsStackError = true;

        raised.Should().Contain(nameof(ImperialViewModel.TandCsStackError));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // NoEmailUser / entry enabled flags (Constructor 4 feature)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor4_WithNaEmail_SetsNoEmailUserTrue()
    {
        var userInfo = new householdgroupjsondetails
        {
            household_individual_name = "Test Member",
            household_individual_email = "N/A"
        };

        var vm = new ImperialViewModel(
            new signupcode { signupcodeid = "CODE" },
            userInfo,
            new System.Collections.ObjectModel.ObservableCollection<householdgroupjsondetails>(),
            new householdgroup());

        vm.noemailuserreg.Should().BeTrue();
    }

    [Fact]
    public void Constructor4_WithNaEmail_SetsEmailEntryEnabledFalse()
    {
        var userInfo = new householdgroupjsondetails
        {
            household_individual_name = "Test Member",
            household_individual_email = "N/A"
        };

        var vm = new ImperialViewModel(
            new signupcode { signupcodeid = "CODE" },
            userInfo,
            new System.Collections.ObjectModel.ObservableCollection<householdgroupjsondetails>(),
            new householdgroup());

        vm.EmailEntryEnabled.Should().BeFalse();
    }

    [Fact]
    public void Constructor1_EmailEntryEnabled_IsTrue()
    {
        var vm = new ImperialViewModel();

        vm.EmailEntryEnabled.Should().BeTrue();
    }

    [Fact]
    public void Constructor3_EmailEntryEnabled_IsTrue()
    {
        var vm = new ImperialViewModel(
            new user { Userid = "u1" },
            new signupcode { signupcodeid = "CODE" });

        vm.EmailEntryEnabled.Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // IsQuestionnairePhase
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsQuestionnairePhase_DefaultsToFalse()
    {
        var vm = new ImperialViewModel();

        vm.IsQuestionnairePhase.Should().BeFalse();
    }

    [Fact]
    public void IsQuestionnairePhase_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.IsQuestionnairePhase = true;

        raised.Should().Contain(nameof(ImperialViewModel.IsQuestionnairePhase));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // IsUnder10User (controls HouseholdMembersStepView branch — plan note 2)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsUnder10User_DefaultsToFalse()
    {
        var vm = new ImperialViewModel();

        vm.IsUnder10User.Should().BeFalse();
    }

    [Fact]
    public void IsUnder10User_WhenChanged_RaisesPropertyChanged()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.IsUnder10User = true;

        raised.Should().Contain(nameof(ImperialViewModel.IsUnder10User));
    }
}
