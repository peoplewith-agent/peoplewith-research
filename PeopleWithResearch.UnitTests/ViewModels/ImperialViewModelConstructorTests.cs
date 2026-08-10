// TDD: Written before production code. Will compile after sub-write-code completes.

using System.Collections.ObjectModel;
using FluentAssertions;
using PeopleWithResearch;
using Xunit;

namespace PeopleWithResearch.UnitTests.ViewModels;

/// <summary>
/// Tests for ImperialViewModel constructor overloads.
/// AC1: All business, navigation, and state logic migrated from Imperial.xaml.cs.
/// AC5: 100% feature parity with existing UI design and workflow.
/// </summary>
public class ImperialViewModelConstructorTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // Constructor 1 — default (parameterless)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_Default_SetsCurrentStepToWelcome()
    {
        // Act
        var vm = new ImperialViewModel();

        // Assert
        vm.CurrentStep.Should().Be("welcomestack");
    }

    [Fact]
    public void Constructor_Default_NewUserIsNotNull()
    {
        var vm = new ImperialViewModel();

        vm.newuser.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_Default_HouseholdRepFromRegIsFalse()
    {
        var vm = new ImperialViewModel();

        vm.householdrepFROMREG.Should().BeFalse();
    }

    [Fact]
    public void Constructor_Default_BannerVisibleIsFalse()
    {
        var vm = new ImperialViewModel();

        vm.BannerVisible.Should().BeFalse();
    }

    [Fact]
    public void Constructor_Default_IsBusyIsFalse()
    {
        var vm = new ImperialViewModel();

        vm.IsBusy.Should().BeFalse();
    }

    [Fact]
    public void Constructor_Default_AllregfieldsIsEmpty()
    {
        var vm = new ImperialViewModel();

        vm.Allregfields.Should().NotBeNull();
        vm.Allregfields.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_Default_CurrentFieldIndexIsZero()
    {
        var vm = new ImperialViewModel();

        vm.CurrentFieldIndex.Should().Be(0);
    }

    [Fact]
    public void Constructor_Default_ProgressAmountIsZero()
    {
        var vm = new ImperialViewModel();

        vm.ProgressAmount.Should().Be(0);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Constructor 2 — user + advert + Questionnaire
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_UserAdvertQuestionnaire_SetsUserDetails()
    {
        var testUser = new user { Userid = "test-user-id", Email = "user@example.com" };
        var testAdvert = new advert();
        var testQuestionnaire = new Questionnaire();

        var vm = new ImperialViewModel(testUser, testAdvert, testQuestionnaire);

        vm.userdetails.Should().BeSameAs(testUser);
    }

    [Fact]
    public void Constructor_UserAdvertQuestionnaire_SetsHouseholdRepFromRegToTrue()
    {
        var testUser = new user { Userid = "test-user-id" };
        var testAdvert = new advert();
        var testQuestionnaire = new Questionnaire();

        var vm = new ImperialViewModel(testUser, testAdvert, testQuestionnaire);

        vm.householdrepFROMREG.Should().BeTrue();
    }

    [Fact]
    public void Constructor_UserAdvertQuestionnaire_BannerRemainsHidden()
    {
        var testUser = new user { Userid = "test-user-id" };
        var testAdvert = new advert();
        var testQuestionnaire = new Questionnaire();

        var vm = new ImperialViewModel(testUser, testAdvert, testQuestionnaire);

        vm.BannerVisible.Should().BeFalse();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Constructor 3 — user + signupcode
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_UserSignupcode_SetsUserDetails()
    {
        var testUser = new user { Userid = "test-user-id" };
        var testSignupCode = new signupcode { signupcodeid = "TESTCODE" };

        var vm = new ImperialViewModel(testUser, testSignupCode);

        vm.userdetails.Should().BeSameAs(testUser);
    }

    [Fact]
    public void Constructor_UserSignupcode_SetsSignupCodeDetails()
    {
        var testUser = new user { Userid = "test-user-id" };
        var testSignupCode = new signupcode { signupcodeid = "TESTCODE" };

        var vm = new ImperialViewModel(testUser, testSignupCode);

        vm.signupcodedetails.Should().BeSameAs(testSignupCode);
    }

    [Fact]
    public void Constructor_UserSignupcode_SetsHouseholdRepFromRegToTrue()
    {
        var testUser = new user { Userid = "test-user-id" };
        var testSignupCode = new signupcode { signupcodeid = "TESTCODE" };

        var vm = new ImperialViewModel(testUser, testSignupCode);

        vm.householdrepFROMREG.Should().BeTrue();
    }

    [Fact]
    public void Constructor_UserSignupcode_BannerRemainsHidden()
    {
        var testUser = new user { Userid = "test-user-id" };
        var testSignupCode = new signupcode { signupcodeid = "TESTCODE" };

        var vm = new ImperialViewModel(testUser, testSignupCode);

        vm.BannerVisible.Should().BeFalse();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Constructor 4 — signupcode + householdgroupjsondetails + collection + group
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_HouseholdProxy_SetsHouseholdRepFromRegToFalse()
    {
        var (vm, _, _, _, _) = BuildHouseholdProxyVm();

        vm.householdrepFROMREG.Should().BeFalse();
    }

    [Fact]
    public void Constructor_HouseholdProxy_SetsBannerVisibleToTrue()
    {
        var (vm, _, _, _, _) = BuildHouseholdProxyVm();

        vm.BannerVisible.Should().BeTrue();
    }

    [Fact]
    public void Constructor_HouseholdProxy_BannerTextContainsMemberName()
    {
        var (vm, _, userInfo, _, _) = BuildHouseholdProxyVm();

        vm.BannerText.Should().Contain(userInfo.household_individual_name);
    }

    [Fact]
    public void Constructor_HouseholdProxy_SetsSignupCodeDetails()
    {
        var (vm, signupCode, _, _, _) = BuildHouseholdProxyVm();

        vm.signupcodedetails.Should().BeSameAs(signupCode);
    }

    [Fact]
    public void Constructor_HouseholdProxy_SetsUseInfoForBaseline()
    {
        var (vm, _, userInfo, _, _) = BuildHouseholdProxyVm();

        vm.userinfoforbaseline.Should().BeSameAs(userInfo);
    }

    [Fact]
    public void Constructor_HouseholdProxy_SetsAllGroupDetailsPassed()
    {
        var (vm, _, _, allGroupDetails, _) = BuildHouseholdProxyVm();

        vm.allgroupdetailspassed.Should().BeSameAs(allGroupDetails);
    }

    [Fact]
    public void Constructor_HouseholdProxy_CreatesNewUserObject()
    {
        var (vm, _, _, _, _) = BuildHouseholdProxyVm();

        // Constructor 4 sets userdetails = new user() (does not pass an existing user)
        vm.userdetails.Should().NotBeNull();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Shared state across all constructors
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Constructor_Default")]
    public void AllConstructors_InitialCurrentStep_IsWelcomeStack(string label)
    {
        _ = label; // parameterised label for readability
        var vm = new ImperialViewModel();
        vm.CurrentStep.Should().Be("welcomestack");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static (
        ImperialViewModel vm,
        signupcode signupCode,
        householdgroupjsondetails userInfo,
        ObservableCollection<householdgroupjsondetails> allGroupDetails,
        householdgroup householdGroup
    ) BuildHouseholdProxyVm()
    {
        var signupCode = new signupcode { signupcodeid = "PROXY-CODE" };
        var userInfo = new householdgroupjsondetails
        {
            household_individual_name = "Jane Doe",
            household_individual_email = "jane@example.com"
        };
        var allGroupDetails = new ObservableCollection<householdgroupjsondetails> { userInfo };
        var householdGroup = new householdgroup { householdgroupid = "group-001" };

        var vm = new ImperialViewModel(signupCode, userInfo, allGroupDetails, householdGroup);
        return (vm, signupCode, userInfo, allGroupDetails, householdGroup);
    }
}
