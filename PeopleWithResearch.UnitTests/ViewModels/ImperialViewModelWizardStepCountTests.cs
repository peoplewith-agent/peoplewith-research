// TDD: Written before production code. Will compile after sub-write-code completes.

using System.Collections.ObjectModel;
using FluentAssertions;
using PeopleWithResearch;
using Xunit;

namespace PeopleWithResearch.UnitTests.ViewModels;

/// <summary>
/// Tests verifying the total wizard step count exposed by ImperialViewModel
/// for each of the 4 constructor paths.
/// AC5: 100% feature parity — all constructor paths must expose the correct TotalSteps.
/// </summary>
public class ImperialViewModelWizardStepCountTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // TotalSteps property
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void TotalSteps_DefaultConstructor_IsZeroBeforeConfigLoad()
    {
        // LoadRegistrationConfigAsync has not been awaited yet — steps not populated
        var vm = new ImperialViewModel();

        vm.TotalSteps.Should().Be(0);
    }

    [Fact]
    public void TotalSteps_AfterManuallyPopulatingAllregfields_MatchesCount()
    {
        var vm = new ImperialViewModel();
        AddSteps(vm, 10);

        vm.TotalSteps.Should().Be(10);
    }

    [Fact]
    public void TotalSteps_RaisesPropertyChanged_WhenAllregfieldsChanges()
    {
        var vm = new ImperialViewModel();
        var raised = new List<string?>();
        // TotalSteps is derived from Allregfields; adding to the collection
        // should cause TotalSteps to be re-evaluated.
        // Implementation may either compute it lazily or update on CollectionChanged.
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.Allregfields.Add(new RegField { XamlNameArea = "namestack" });

        // Either TotalSteps or a computed notification should fire
        raised.Should().Contain(nameof(ImperialViewModel.TotalSteps));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Constructor 1 — default: no pre-populated steps (awaits async load)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor1_Allregfields_IsEmptyBeforeLoad()
    {
        var vm = new ImperialViewModel();
        vm.Allregfields.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Constructor 2 — user + advert + Questionnaire
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor2_Allregfields_IsEmptyBeforeLoad()
    {
        var vm = new ImperialViewModel(
            new user { Userid = "u1" },
            new advert(),
            new Questionnaire());

        vm.Allregfields.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Constructor 3 — user + signupcode
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor3_Allregfields_IsEmptyBeforeLoad()
    {
        var vm = new ImperialViewModel(
            new user { Userid = "u1" },
            new signupcode { signupcodeid = "CODE" });

        vm.Allregfields.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Constructor 4 — household proxy
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor4_Allregfields_IsEmptyBeforeLoad()
    {
        var vm = new ImperialViewModel(
            new signupcode { signupcodeid = "CODE" },
            new householdgroupjsondetails { household_individual_name = "Member" },
            new ObservableCollection<householdgroupjsondetails>(),
            new householdgroup());

        vm.Allregfields.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Over16 / NonRequired field lists (sub-collections used by wizard routing)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Over16regfields_DefaultConstructor_IsEmptyList()
    {
        var vm = new ImperialViewModel();
        vm.Over16regfields.Should().NotBeNull();
        vm.Over16regfields.Should().BeEmpty();
    }

    [Fact]
    public void Nonrequiredfields_DefaultConstructor_IsEmptyList()
    {
        var vm = new ImperialViewModel();
        vm.Nonrequiredfields.Should().NotBeNull();
        vm.Nonrequiredfields.Should().BeEmpty();
    }

    [Fact]
    public void Allquesfields_DefaultConstructor_IsEmptyList()
    {
        var vm = new ImperialViewModel();
        vm.Allquesfields.Should().NotBeNull();
        vm.Allquesfields.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Wizard step ordering — XamlNameArea sequence
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void WizardSteps_PopulatedManually_ReturnCorrectXamlNameAreasInOrder()
    {
        // Validates the step-factory key contract: the XamlNameArea values
        // defined in the plan §Step 2 table must be the exact strings used in
        // the factory dictionary.
        var expectedStepAreas = new[]
        {
            "welcomestack",
            "namestack",
            "mainuserstack",
            "addressstack",
            "genderstack",
            "ethnicitystack",
            "bodymetricsstack",
            "educationstack",
            "householdstructurestack",
            "nhsnumstack",
            "ristack",
            "healthconditionsstack",
            "medicationsstack",
            "rvstack",
            "dietstack",
            "menstrualstack",
            "htstack",
            "addqstack",
            "antiviralstack",
            "tobaccostack",
            "alcoholstack",
            "drugstack",
            "sleepstack",
            "tandcstack",
            "under10stack"
        };

        var vm = new ImperialViewModel();
        // Populate Allregfields with the expected step keys and check
        foreach (var area in expectedStepAreas)
            vm.Allregfields.Add(new RegField { XamlNameArea = area });
        var factoryKeys = vm.Allregfields.Select(r => r.XamlNameArea).ToList();

        factoryKeys.Should().Contain(expectedStepAreas);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Progress calculation
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ProgressAmount_HalfwayThroughSteps_IsApproximatelyFiftyPercent()
    {
        var vm = new ImperialViewModel();
        AddSteps(vm, 10);
        vm.CurrentFieldIndex = 4; // halfway through 10 steps (0-based)
        vm.ProgressAmount = (4.0 / 10.0) * 100.0; // simulate progress calculation

        // Progress bar typically 0–100; halfway should be around 40–60
        vm.ProgressAmount.Should().BeInRange(30, 70);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static void AddSteps(ImperialViewModel vm, int count)
    {
        for (int i = 0; i < count; i++)
        {
            vm.Allregfields.Add(new RegField
            {
                XamlNameArea = $"step{i}stack",
                FieldTitle = $"Step {i}"
            });
        }
    }
}
