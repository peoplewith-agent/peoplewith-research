// TDD: Written before production code. Will compile after sub-write-code completes.

using System.Collections.ObjectModel;
using FluentAssertions;
using PeopleWithResearch;
using Xunit;

namespace PeopleWithResearch.UnitTests.ViewModels;

/// <summary>
/// Tests for ImperialViewModel wizard navigation state machine.
/// CurrentFieldIndex, CurrentStep, ProgressAmount, CanGoNext / CanGoBack boundaries.
/// AC1: Navigation logic migrated from Imperial.xaml.cs.
/// AC5: Feature parity for all wizard step flows.
/// </summary>
public class ImperialViewModelNavigationTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // GoNext — advances CurrentFieldIndex
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NextAsync_WhenOnWelcomeStep_AdvancesToFirstRegField()
    {
        var vm = BuildVmWithSteps(stepCount: 3);
        // Step 0 == welcomestack; first reg field is at index 0 of Allregfields
        vm.CurrentStep = "welcomestack";

        await vm.NextAsync();

        vm.CurrentStep.Should().Be(vm.Allregfields[0].XamlNameArea);
    }

    [Fact]
    public async Task NextAsync_IncreasesCurrentFieldIndex()
    {
        var vm = BuildVmWithSteps(stepCount: 5);
        LoadIntoFirstRegStep(vm);
        int before = vm.CurrentFieldIndex;

        await vm.NextAsync();

        vm.CurrentFieldIndex.Should().Be(before + 1);
    }

    [Fact]
    public async Task NextAsync_UpdatesProgressAmount_WhenStepsExist()
    {
        var vm = BuildVmWithSteps(stepCount: 4);
        LoadIntoFirstRegStep(vm);

        await vm.NextAsync();

        vm.ProgressAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task NextAsync_AtLastStep_DoesNotIncrementBeyondBounds()
    {
        var vm = BuildVmWithSteps(stepCount: 2);
        // Position at last field
        vm.CurrentFieldIndex = vm.Allregfields.Count - 1;

        // NextAsync at the final step triggers CreateAccount, not increment
        // We verify the index does not go out-of-range (≥ Count)
        int expected = vm.Allregfields.Count - 1;
        // Call NextAsync — it should handle the final step gracefully
        var action = async () => await vm.NextAsync();
        await action.Should().NotThrowAsync();
        vm.CurrentFieldIndex.Should().BeLessOrEqualTo(vm.Allregfields.Count - 1);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GoBack — decrements CurrentFieldIndex
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task BackAsync_DecrementsCurrentFieldIndex()
    {
        var vm = BuildVmWithSteps(stepCount: 3);
        LoadIntoFirstRegStep(vm);
        await vm.NextAsync();  // move to index 1
        int before = vm.CurrentFieldIndex;

        await vm.BackAsync();

        vm.CurrentFieldIndex.Should().Be(before - 1);
    }

    [Fact]
    public async Task BackAsync_AtFirstRegField_ReturnToWelcomeStep()
    {
        var vm = BuildVmWithSteps(stepCount: 2);
        LoadIntoFirstRegStep(vm); // index == 0

        await vm.BackAsync();

        vm.CurrentStep.Should().Be("welcomestack");
    }

    [Fact]
    public async Task BackAsync_AtWelcomeStep_DoesNotDecrementBelowZero()
    {
        var vm = BuildVmWithSteps(stepCount: 2);
        vm.CurrentStep = "welcomestack";

        var action = async () => await vm.BackAsync();
        await action.Should().NotThrowAsync();

        vm.CurrentFieldIndex.Should().BeGreaterOrEqualTo(0);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // CurrentStep tracks Allregfields XamlNameArea correctly
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NextAsync_SetsCurrentStepToXamlNameAreaOfNewField()
    {
        var vm = BuildVmWithSteps(stepCount: 3);
        LoadIntoFirstRegStep(vm);

        await vm.NextAsync();

        vm.CurrentStep.Should().Be(vm.Allregfields[vm.CurrentFieldIndex].XamlNameArea);
    }

    [Fact]
    public async Task BackAsync_SetsCurrentStepToXamlNameAreaOfPreviousField()
    {
        var vm = BuildVmWithSteps(stepCount: 3);
        LoadIntoFirstRegStep(vm);
        await vm.NextAsync(); // move to index 1

        await vm.BackAsync(); // back to index 0

        vm.CurrentStep.Should().Be(vm.Allregfields[0].XamlNameArea);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Progress amount boundary conditions
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ProgressAmount_AtStart_IsZeroOrMinimal()
    {
        var vm = BuildVmWithSteps(stepCount: 5);

        vm.ProgressAmount.Should().BeInRange(0, 0.05);
    }

    [Fact]
    public async Task ProgressAmount_AfterAllSteps_IsOneOrHundred()
    {
        // Progress is typically expressed as 0–100 for SfLinearProgressBar
        var vm = BuildVmWithSteps(stepCount: 3);
        LoadIntoFirstRegStep(vm);

        // Advance through all steps
        for (int i = 0; i < vm.Allregfields.Count - 1; i++)
            await vm.NextAsync();

        vm.ProgressAmount.Should().BeGreaterThan(50);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // INotifyPropertyChanged — CurrentStep raises change notification
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NextAsync_RaisesPropertyChangedForCurrentStep()
    {
        var vm = BuildVmWithSteps(stepCount: 3);
        LoadIntoFirstRegStep(vm);

        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        await vm.NextAsync();

        raised.Should().Contain(nameof(ImperialViewModel.CurrentStep));
    }

    [Fact]
    public async Task BackAsync_RaisesPropertyChangedForCurrentStep()
    {
        var vm = BuildVmWithSteps(stepCount: 3);
        LoadIntoFirstRegStep(vm);
        await vm.NextAsync();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        await vm.BackAsync();

        raised.Should().Contain(nameof(ImperialViewModel.CurrentStep));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Wizard step ordering — 4 constructor paths all start at welcomestack
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor1_InitialStep_IsWelcomeStack()
    {
        var vm = new ImperialViewModel();
        vm.CurrentStep.Should().Be("welcomestack");
    }

    [Fact]
    public void Constructor2_InitialStep_IsWelcomeStack()
    {
        var vm = new ImperialViewModel(
            new user { Userid = "u1" },
            new advert(),
            new Questionnaire());

        vm.CurrentStep.Should().Be("welcomestack");
    }

    [Fact]
    public void Constructor3_InitialStep_IsWelcomeStack()
    {
        var vm = new ImperialViewModel(
            new user { Userid = "u1" },
            new signupcode { signupcodeid = "CODE" });

        vm.CurrentStep.Should().Be("welcomestack");
    }

    [Fact]
    public void Constructor4_InitialStep_IsWelcomeStack()
    {
        var vm = new ImperialViewModel(
            new signupcode { signupcodeid = "CODE" },
            new householdgroupjsondetails { household_individual_name = "Test Member" },
            new ObservableCollection<householdgroupjsondetails>(),
            new householdgroup());

        vm.CurrentStep.Should().Be("welcomestack");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // TotalSteps reflects Allregfields count
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void TotalSteps_ReflectsAllregfieldsCount()
    {
        var vm = BuildVmWithSteps(stepCount: 7);

        vm.TotalSteps.Should().Be(7);
    }

    [Fact]
    public void TotalSteps_DefaultVm_IsZeroUntilConfigLoaded()
    {
        // Before LoadRegistrationConfigAsync runs, the list is empty
        var vm = new ImperialViewModel();

        vm.TotalSteps.Should().Be(0);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a ViewModel pre-populated with <paramref name="stepCount"/> synthetic RegField steps.
    /// Simulates the state after LoadRegistrationConfigAsync has completed.
    /// </summary>
    private static ImperialViewModel BuildVmWithSteps(int stepCount)
    {
        var vm = new ImperialViewModel();
        for (int i = 0; i < stepCount; i++)
        {
            vm.Allregfields.Add(new RegField
            {
                XamlNameArea = $"step{i}stack",
                FieldTitle = $"Step {i}",
                FieldSubtitle = $"Subtitle {i}"
            });
        }
        return vm;
    }

    /// <summary>
    /// Sets the ViewModel into the state where the first reg field is active
    /// (i.e., user has moved past the welcome screen).
    /// </summary>
    private static void LoadIntoFirstRegStep(ImperialViewModel vm)
    {
        vm.CurrentFieldIndex = 0;
        vm.CurrentStep = vm.Allregfields[0].XamlNameArea;
    }
}
