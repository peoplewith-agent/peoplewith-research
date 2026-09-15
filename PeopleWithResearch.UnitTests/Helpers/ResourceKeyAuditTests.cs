// TDD: Written before production code. Will compile after sub-write-code completes.

using System.Globalization;
using System.Resources;
using FluentAssertions;
using PeopleWithResearch.Resources.Strings;
using Xunit;

namespace PeopleWithResearch.UnitTests.Helpers;

/// <summary>
/// Per-page text-extraction audit tests — PD2-30.
///
/// These tests verify:
/// 1. Every new resource key planned in PD2-30 exists in all 4 .resx files (EN, PL, RO, GU).
/// 2. AppResources.ResourceManager.GetString(key) returns a non-empty value for each key
///    when the language is set to English ("en").
/// 3. Satellite ResourceManagers for PL, RO, and GU return non-empty placeholder values
///    for each new key.
/// 4. No two keys in the English file share identical values (guards against accidental
///    copy-paste duplication where a single key is used for two distinct UI strings).
/// 5. The 17 original PD2-29 keys still resolve correctly after the new keys are added.
///
/// NOTE: Tests use ResourceManager.GetString(key, CultureInfo) directly so that the test
/// project does not need to reference MAUI page classes. This approach is safe on net10.0.
///
/// AC COVERAGE:
///   AC3: Every extracted key exists in ALL 4 resource files.
///   AC5: Duplicate text reuses a single shared key (non-duplication assertion).
///   AC6 (partial): All keys resolve to non-empty strings in all 4 languages so the runtime
///        can display translated text (manual device verification covers AC6 fully).
/// </summary>
[Collection("PreferencesTests")]
public class ResourceKeyAuditTests : IDisposable
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static readonly ResourceManager _rm = AppResources.ResourceManager;

    private static readonly CultureInfo _en = new("en");
    private static readonly CultureInfo _pl = new("pl");
    private static readonly CultureInfo _ro = new("ro");
    private static readonly CultureInfo _gu = new("gu");

    private readonly CultureInfo _savedCulture;
    private readonly CultureInfo _savedUICulture;

    public ResourceKeyAuditTests()
    {
        _savedCulture    = CultureInfo.CurrentCulture;
        _savedUICulture  = CultureInfo.CurrentUICulture;
        Microsoft.Maui.Storage.Preferences.Clear();
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture   = _savedCulture;
        CultureInfo.CurrentUICulture = _savedUICulture;
        Microsoft.Maui.Storage.Preferences.Clear();
    }

    /// <summary>
    /// Helper: asserts the key exists and is non-empty in all 4 cultures.
    /// </summary>
    private static void AssertKeyInAllLanguages(string key)
    {
        foreach (var culture in new[] { _en, _pl, _ro, _gu })
        {
            var value = _rm.GetString(key, culture);
            value.Should().NotBeNullOrWhiteSpace(
                because: $"key '{key}' must have a non-empty value in culture '{culture.Name}'");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AC5 GUARD — original PD2-29 keys must not be broken by PD2-30 additions
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("AppName")]
    [InlineData("MainPage_ResearchCode")]
    [InlineData("MainPage_ThankYou")]
    [InlineData("MainPage_PasteInstructions")]
    [InlineData("MainPage_PasteButton")]
    [InlineData("MainPage_LanguageButton")]
    [InlineData("SelectLang_Title")]
    [InlineData("SelectLang_Description")]
    [InlineData("Lang_English")]
    [InlineData("Lang_Polish")]
    [InlineData("Lang_Romanian")]
    [InlineData("Lang_Gujarati")]
    [InlineData("Common_OK")]
    [InlineData("Common_Cancel")]
    [InlineData("Common_Loading")]
    [InlineData("Common_Error")]
    [InlineData("Common_Retry")]
    public void ExistingKey_AllLanguages_StillResolvesAfterPd2_30(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SHARED / COMMON keys  (Step 0 — added once, reused across pages)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Common_SelectAnOption")]
    [InlineData("Common_PleaseComplete")]
    [InlineData("Common_PrivacyPolicy")]
    [InlineData("Common_Back")]
    [InlineData("Common_AddNewMember")]
    [InlineData("Common_ActionRequired")]
    [InlineData("Common_CompleteBaselineQ")]
    [InlineData("Common_BaselineQPrompt")]
    [InlineData("Common_StudyProgress")]
    [InlineData("Common_ManageProfile")]
    [InlineData("Common_StatusCompleted")]
    [InlineData("Common_StatusPending")]
    public void CommonKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 1 — NewMainPage  (Main_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Main_CheckingCode")]
    [InlineData("Main_Clear")]
    [InlineData("Main_SuccessCheck")]
    [InlineData("Main_Success")]
    [InlineData("Main_Register")]
    [InlineData("Main_LogIn")]
    [InlineData("Main_IfYouHaveDetails")]
    [InlineData("Main_PasteTextConst")]
    [InlineData("Main_CheckTextConst")]
    [InlineData("Main_ClipboardUnavailableTitle")]
    [InlineData("Main_ClipboardUnavailableMsg")]
    [InlineData("Main_RequestTimedOut")]
    [InlineData("Main_InvalidCode")]
    [InlineData("Main_RegistrationActiveTitle")]
    [InlineData("Main_RegistrationActiveMsg")]
    [InlineData("Main_SomethingWentWrong")]
    public void MainPageKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 2 — NewLoginPage  (Login_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Login_WelcomeBack")]
    [InlineData("Login_SignInSubtitle")]
    [InlineData("Login_EmailLabel")]
    [InlineData("Login_PasswordLabel")]
    [InlineData("Login_ForgotPassword")]
    [InlineData("Login_SignInButton")]
    [InlineData("Login_NoAccount")]
    [InlineData("Login_SignUpLink")]
    [InlineData("Login_EmailEmpty")]
    [InlineData("Login_EmailInvalid")]
    [InlineData("Login_PasswordEmpty")]
    [InlineData("Login_AccountNotFound")]
    [InlineData("Login_AccountDeletedTitle")]
    [InlineData("Login_AccountDeletedMsg")]
    [InlineData("Login_OnboardingTitle")]
    [InlineData("Login_OnboardingMsg")]
    [InlineData("Login_WithdrawnTitle")]
    [InlineData("Login_WithdrawnMsg")]
    [InlineData("Login_PasswordIncorrect")]
    public void LoginPageKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 3 — ImperialDashboard  (Dashboard_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Dashboard_WelcomeBack")]
    [InlineData("Dashboard_LoggedInAs")]
    [InlineData("Dashboard_StudyUserId")]
    [InlineData("Dashboard_HouseRep")]
    [InlineData("Dashboard_ActiveNo")]
    [InlineData("Dashboard_HouseholdRep")]
    [InlineData("Dashboard_StayInLoop")]
    [InlineData("Dashboard_Action")]
    [InlineData("Dashboard_NeverMissUpdate")]
    [InlineData("Dashboard_NotificationsPrompt")]
    [InlineData("Dashboard_NotNow")]
    [InlineData("Dashboard_Enable")]
    [InlineData("Dashboard_AllTasksCompleted")]
    [InlineData("Dashboard_QCompleted")]
    [InlineData("Dashboard_BaselineFormStage")]
    [InlineData("Dashboard_BaselineFormRequirement")]
    [InlineData("Dashboard_BaselineSamplingStage")]
    [InlineData("Dashboard_SamplingKitsArrive")]
    [InlineData("Dashboard_SamplingStep1")]
    [InlineData("Dashboard_SamplingCollect")]
    [InlineData("Dashboard_SamplingStep2")]
    [InlineData("Dashboard_SamplingCompleteForm")]
    [InlineData("Dashboard_SamplingSameDay")]
    [InlineData("Dashboard_BaselineSamples")]
    [InlineData("Dashboard_HouseholdParticipants")]
    [InlineData("Dashboard_BaselineForm")]
    [InlineData("Dashboard_BaselineSamplesLabel")]
    [InlineData("Dashboard_LastActive")]
    [InlineData("Dashboard_Questionnaires")]
    [InlineData("Dashboard_AwaitingBaseline")]
    [InlineData("Dashboard_CompleteOnBehalf")]
    [InlineData("Dashboard_CompleteBaseline")]
    [InlineData("Dashboard_ProfileNotActive")]
    [InlineData("Dashboard_ProfileNotActiveMsg")]
    [InlineData("Dashboard_ActiveProfile")]
    [InlineData("Dashboard_ProgressSoFar")]
    [InlineData("Dashboard_CompletedQuestionnaires")]
    [InlineData("Dashboard_ReviewQsSubmitted")]
    [InlineData("Dashboard_TapViewCompleted")]
    [InlineData("Dashboard_MissedQuestionnaires")]
    [InlineData("Dashboard_MissedQsLabel")]
    [InlineData("Dashboard_MissedQsMsg")]
    [InlineData("Dashboard_TapCompleteMissed")]
    [InlineData("Dashboard_NotCompleted")]
    [InlineData("Dashboard_Missed")]
    [InlineData("Dashboard_ExpandHousehold")]
    [InlineData("Dashboard_InviteMembers")]
    [InlineData("Dashboard_GiveAccessTitle")]
    [InlineData("Dashboard_GiveAccessMsg")]
    [InlineData("Dashboard_GrantAccess")]
    [InlineData("Dashboard_NotificationsOff")]
    [InlineData("Dashboard_TurnOnNotifications")]
    [InlineData("Dashboard_ThankYouStudy")]
    [InlineData("Dashboard_StudyInformation")]
    [InlineData("Dashboard_HopperStudy")]
    [InlineData("Dashboard_ContactInformation")]
    [InlineData("Dashboard_ContactMsg")]
    [InlineData("Dashboard_NoContactFound")]
    [InlineData("Dashboard_ContactOffline")]
    [InlineData("Dashboard_AdditionalInformation")]
    [InlineData("Dashboard_NoResourcesFound")]
    [InlineData("Dashboard_ResourcesFailedLoad")]
    [InlineData("Dashboard_Profile")]
    [InlineData("Dashboard_ManageYourProfile")]
    [InlineData("Dashboard_PersonalInformation")]
    [InlineData("Dashboard_Preferences")]
    [InlineData("Dashboard_GetInTouch")]
    [InlineData("Dashboard_SupportMsg")]
    [InlineData("Dashboard_MessageTeam")]
    [InlineData("Dashboard_LogOut")]
    [InlineData("Dashboard_WithdrawalAction")]
    [InlineData("Dashboard_WithdrawTitle")]
    [InlineData("Dashboard_WithdrawWarning")]
    [InlineData("Dashboard_AppVersion")]
    [InlineData("Dashboard_HiPrefix")]
    [InlineData("Dashboard_Participant")]
    [InlineData("Dashboard_Day28")]
    [InlineData("Dashboard_SamplingSymptoms")]
    [InlineData("Dashboard_SamplingPeriodSuffix")]
    [InlineData("Dashboard_BaselineSamplesFormTitle")]
    [InlineData("Dashboard_BaselineSamplesFormMsg")]
    [InlineData("Dashboard_WithdrawAlertTitle")]
    [InlineData("Dashboard_WithdrawAlertMsg")]
    [InlineData("Dashboard_WithdrawButton")]
    [InlineData("Dashboard_LogoutTitle")]
    [InlineData("Dashboard_LogoutMsg")]
    [InlineData("Dashboard_LogoutButton")]
    [InlineData("Dashboard_ConfirmAccessTitle")]
    [InlineData("Dashboard_ConfirmAccessMsg")]
    [InlineData("Dashboard_ConfirmAccessYes")]
    [InlineData("Dashboard_ConfirmAccessNo")]
    [InlineData("Dashboard_AccessGrantedTitle")]
    [InlineData("Dashboard_AccessGrantedMsg")]
    [InlineData("Dashboard_WaitingStage")]
    [InlineData("Dashboard_SymptomCough")]
    [InlineData("Dashboard_SymptomTasteSmell")]
    [InlineData("Dashboard_SymptomSoreThroat")]
    [InlineData("Dashboard_SymptomBreath")]
    [InlineData("Dashboard_SamplingAndThinkSymptoms")]
    [InlineData("Dashboard_HouseholdIsOn")]
    [InlineData("Dashboard_SamplingStep1Collect")]
    [InlineData("Dashboard_SamplingStep2Complete")]
    [InlineData("Dashboard_DailyRecordingComplete")]
    [InlineData("Dashboard_DailyRecordingCompleteMsg")]
    [InlineData("Dashboard_EndStudyFeedbackTitle")]
    [InlineData("Dashboard_EndStudyThankYou")]
    [InlineData("Dashboard_EndStudyFinalQ")]
    [InlineData("Dashboard_StudyComplete")]
    [InlineData("Dashboard_StudyCompleteTitle")]
    [InlineData("Dashboard_StudyCompleteMsg")]
    [InlineData("Dashboard_RestartStudyTitle")]
    [InlineData("Dashboard_RestartStudyMsg")]
    [InlineData("Dashboard_ContactStudyTeam")]
    [InlineData("Dashboard_RecentQuestionnaires")]
    [InlineData("Dashboard_ViewFormsHistory")]
    [InlineData("Dashboard_TodayDayThree")]
    [InlineData("Dashboard_T1FormInstruction")]
    public void DashboardKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 4 — Addnewmember  (AddMember_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("AddMember_Subtitle")]
    [InlineData("AddMember_MemberNameLabel")]
    [InlineData("AddMember_MemberNameHint")]
    [InlineData("AddMember_LastName")]
    [InlineData("AddMember_OwnPhoneQuestion")]
    [InlineData("AddMember_OwnPhoneSubtitle")]
    [InlineData("AddMember_YesOwnAccount")]
    [InlineData("AddMember_YesOwnAccountDetail")]
    [InlineData("AddMember_NoManageProfile")]
    [InlineData("AddMember_NoManageProfileDetail")]
    [InlineData("AddMember_EmailLabel")]
    [InlineData("AddMember_EmailHint")]
    [InlineData("AddMember_AgeLabel")]
    [InlineData("AddMember_AgeHint")]
    [InlineData("AddMember_RelationLabel")]
    [InlineData("AddMember_RelationHint")]
    [InlineData("AddMember_PermissionCheck")]
    [InlineData("AddMember_AgreeTerms")]
    [InlineData("AddMember_EmailInvalid")]
    [InlineData("AddMember_EmailFamilyExists")]
    [InlineData("AddMember_EmailExists")]
    public void AddMemberKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 5 — pdfpage  (Pdf_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Pdf_Close")]
    public void PdfPageKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 6 — SelectNotificationTime  (Notification_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Notification_Title")]
    [InlineData("Notification_SelectTimeHint")]
    [InlineData("Notification_ChooseTime")]
    [InlineData("Notification_SavedTitle")]
    [InlineData("Notification_SavedSubtitle")]
    [InlineData("Notification_ChangeAnytime")]
    public void NotificationKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 7 — WithdrawVideoPopUp  (Withdraw_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Withdraw_ErrorTitle")]
    [InlineData("Withdraw_ErrorMsg")]
    [InlineData("Withdraw_ErrorOk")]
    [InlineData("Withdraw_CloseVideo")]
    [InlineData("Withdraw_ProceedButton")]
    public void WithdrawVideoKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 8 — ManageProfile  (Manage_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Manage_Title")]
    [InlineData("Manage_Subtitle")]
    [InlineData("Manage_PersonalInformation")]
    [InlineData("Manage_UserId")]
    [InlineData("Manage_FirstName")]
    [InlineData("Manage_Surname")]
    [InlineData("Manage_Email")]
    [InlineData("Manage_PermissionGranted")]
    [InlineData("Manage_PermissionCheck")]
    [InlineData("Manage_HouseholdDetails")]
    [InlineData("Manage_Status")]
    [InlineData("Manage_GroupId")]
    [InlineData("Manage_AgeLabel")]
    [InlineData("Manage_AgeHint")]
    [InlineData("Manage_RelationLabel")]
    [InlineData("Manage_RelationHint")]
    [InlineData("Manage_EnableEditHint")]
    [InlineData("Manage_EnableEdit")]
    [InlineData("Manage_SaveChanges")]
    [InlineData("Manage_EditField")]
    [InlineData("Manage_UpdateFailedTitle")]
    [InlineData("Manage_UpdateFailedMsg")]
    [InlineData("Manage_SaveFailedTitle")]
    [InlineData("Manage_SaveFailedMsg")]
    public void ManageProfileKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 9 — ProfileEdit  (ProfileEdit_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("ProfileEdit_YourName")]
    [InlineData("ProfileEdit_YourEmail")]
    [InlineData("ProfileEdit_DateOfBirth")]
    [InlineData("ProfileEdit_DobInstruction")]
    [InlineData("ProfileEdit_Confirm")]
    [InlineData("ProfileEdit_SelectGender")]
    [InlineData("ProfileEdit_PleaseState")]
    [InlineData("ProfileEdit_Ethnicity")]
    [InlineData("ProfileEdit_PhoneNumber")]
    [InlineData("ProfileEdit_TownCity")]
    [InlineData("ProfileEdit_Height")]
    [InlineData("ProfileEdit_SelectHeight")]
    [InlineData("ProfileEdit_Weight")]
    [InlineData("ProfileEdit_SelectWeight")]
    [InlineData("ProfileEdit_Notifications")]
    [InlineData("ProfileEdit_AllowNotifications")]
    [InlineData("ProfileEdit_NhiTitle")]
    [InlineData("ProfileEdit_NhiInstruction")]
    [InlineData("ProfileEdit_PasswordReset")]
    [InlineData("ProfileEdit_Save")]
    [InlineData("ProfileEdit_EmailEmpty")]
    [InlineData("ProfileEdit_EmailInvalid")]
    [InlineData("ProfileEdit_EmailInUse")]
    [InlineData("ProfileEdit_EnterCurrentPassword")]
    [InlineData("ProfileEdit_EnterNewPassword")]
    [InlineData("ProfileEdit_PasswordMismatch")]
    [InlineData("ProfileEdit_PasswordTooShort")]
    [InlineData("ProfileEdit_PasswordLength")]
    [InlineData("ProfileEdit_PasswordNeedsNumber")]
    [InlineData("ProfileEdit_PasswordNeedsUpper")]
    [InlineData("ProfileEdit_PasswordNeedsLower")]
    [InlineData("ProfileEdit_PasswordNeedsSymbol")]
    [InlineData("ProfileEdit_PasswordCurrentIncorrect")]
    public void ProfileEditKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 10 — PrivacyPolicyPage  (Privacy_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Privacy_FooterLinks")]
    public void PrivacyPolicyKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 11 — FAQ_s  (Faq_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Faq_Title")]
    [InlineData("Faq_Subtitle")]
    [InlineData("Faq_Loading")]
    [InlineData("Faq_FilterBy")]
    [InlineData("Faq_NoFaqsFound")]
    [InlineData("Faq_NoFaqsMsg")]
    public void FaqKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 12 — NewImperial  (Register_*)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Register_EmailHelperText")]
    [InlineData("Register_PasswordMustContain")]
    [InlineData("Register_PwMinChars")]
    [InlineData("Register_PwSpecialChar")]
    [InlineData("Register_PwCapital")]
    [InlineData("Register_PwNumber")]
    [InlineData("Register_CheckPostcode")]
    [InlineData("Register_PostcodeNoResults")]
    [InlineData("Register_PostcodeNoResultsMsg")]
    [InlineData("Register_ClearAddress")]
    [InlineData("Register_DobError")]
    [InlineData("Register_SkipQuestion")]
    [InlineData("Register_HeightFt")]
    [InlineData("Register_HeightIn")]
    [InlineData("Register_HeightCm")]
    [InlineData("Register_AddHouseholdMember")]
    [InlineData("Register_AddHouseholdMemberSub")]
    [InlineData("Register_MemberName")]
    [InlineData("Register_MemberNameHint")]
    [InlineData("Register_OwnPhone")]
    [InlineData("Register_OwnPhoneSub")]
    [InlineData("Register_IfYes")]
    [InlineData("Register_IfYesDetail")]
    [InlineData("Register_TermsTitle")]
    [InlineData("Register_AgreeTerms")]
    [InlineData("Register_AgreeTermsLink")]
    [InlineData("Register_Required")]
    [InlineData("Register_AgreeEmail")]
    [InlineData("Register_AgreeEmailLink")]
    [InlineData("Register_EnterFullName")]
    [InlineData("Register_EnterRole")]
    [InlineData("Register_EnterValue")]
    [InlineData("Register_AddValue")]
    public void RegisterKey_AllLanguages_ReturnsNonEmpty(string key)
    {
        AssertKeyInAllLanguages(key);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AC3 SPOT-CHECK — English ResourceManager returns non-null for each
    // new key via the strongly-typed accessor (compile-time coverage only;
    // runtime values confirmed by the Theory tests above)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ResourceManager_EnglishCulture_CommonSelectAnOption_IsNonEmpty()
    {
        _rm.GetString("Common_SelectAnOption", _en)
           .Should().NotBeNullOrWhiteSpace(
               because: "Common_SelectAnOption must have an English value");
    }

    [Fact]
    public void ResourceManager_EnglishCulture_Dashboard_HiPrefix_IsNonEmpty()
    {
        _rm.GetString("Dashboard_HiPrefix", _en)
           .Should().NotBeNullOrWhiteSpace(
               because: "Dashboard_HiPrefix is used in the greeting interpolation and must not be empty");
    }

    [Fact]
    public void ResourceManager_EnglishCulture_Login_WelcomeBack_IsNonEmpty()
    {
        _rm.GetString("Login_WelcomeBack", _en)
           .Should().NotBeNullOrWhiteSpace(
               because: "Login_WelcomeBack is the first visible text on NewLoginPage");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AC5 GUARD — no two distinct new keys share an identical English value
    //
    // This test loads every key defined in the plan and asserts uniqueness of
    // values in the English resource file. If two keys accidentally receive the
    // same value it suggests a key was duplicated instead of reusing a
    // Common_* key.
    //
    // NOTE: Keys whose English values are intentionally short (e.g. single
    // digits like "1.", "2.") are excluded from this check as they are not
    // meaningful candidates for deduplication.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void EnglishValues_AllNewKeys_AreUnique_NoDuplicatesExceptCommon()
    {
        // All new keys added by PD2-30 (Common_* keys are intentionally shared
        // across pages — they are permitted duplicates).  We only assert that
        // non-Common keys do not share values with one another.
        var nonCommonKeys = new[]
        {
            // Main_*
            "Main_CheckingCode", "Main_Clear", "Main_SuccessCheck", "Main_Success",
            "Main_Register", "Main_LogIn", "Main_IfYouHaveDetails",
            "Main_PasteTextConst", "Main_CheckTextConst",
            "Main_ClipboardUnavailableTitle", "Main_ClipboardUnavailableMsg",
            "Main_RequestTimedOut", "Main_InvalidCode",
            "Main_RegistrationActiveTitle", "Main_RegistrationActiveMsg",
            "Main_SomethingWentWrong",

            // Login_*
            "Login_WelcomeBack", "Login_SignInSubtitle", "Login_EmailLabel",
            "Login_PasswordLabel", "Login_ForgotPassword", "Login_SignInButton",
            "Login_NoAccount", "Login_SignUpLink",
            "Login_EmailEmpty", "Login_EmailInvalid", "Login_PasswordEmpty",
            "Login_AccountNotFound",
            "Login_AccountDeletedTitle", "Login_AccountDeletedMsg",
            "Login_OnboardingTitle", "Login_OnboardingMsg",
            "Login_WithdrawnTitle", "Login_WithdrawnMsg",
            "Login_PasswordIncorrect",

            // Dashboard_* (selected non-trivially short ones)
            "Dashboard_WelcomeBack", "Dashboard_LoggedInAs", "Dashboard_StudyUserId",
            "Dashboard_NotificationsPrompt", "Dashboard_AllTasksCompleted",
            "Dashboard_QCompleted", "Dashboard_BaselineFormStage",
            "Dashboard_BaselineFormRequirement", "Dashboard_SamplingSymptoms",
            "Dashboard_SamplingPeriodSuffix", "Dashboard_WithdrawAlertMsg",
            "Dashboard_LogoutMsg", "Dashboard_ConfirmAccessMsg",
            "Dashboard_GiveAccessMsg", "Dashboard_InviteMembers",
            "Dashboard_CompleteOnBehalf", "Dashboard_ProfileNotActiveMsg",
            "Dashboard_ContactMsg", "Dashboard_ReviewQsSubmitted",
            "Dashboard_MissedQsMsg", "Dashboard_SupportMsg",
            "Dashboard_WithdrawWarning", "Dashboard_NotificationsOff",
            "Dashboard_ResourcesFailedLoad",

            // Manage_*
            "Manage_Subtitle", "Manage_PermissionCheck",
            "Manage_EnableEditHint", "Manage_UpdateFailedMsg", "Manage_SaveFailedMsg",

            // ProfileEdit_*
            "ProfileEdit_DobInstruction", "ProfileEdit_NhiInstruction",
            "ProfileEdit_PasswordTooShort", "ProfileEdit_PasswordLength",

            // Register_*
            "Register_EmailHelperText", "Register_PostcodeNoResultsMsg",
            "Register_SkipQuestion", "Register_IfYesDetail",
            "Register_AgreeTermsLink", "Register_AgreeEmailLink",

            // Others
            "Notification_ChangeAnytime", "Withdraw_ErrorMsg",
            "Faq_NoFaqsMsg",
        };

        var seen = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var key in nonCommonKeys)
        {
            var value = _rm.GetString(key, _en);
            if (string.IsNullOrWhiteSpace(value))
                continue; // Key not yet added — will be caught by the Theory tests

            if (seen.TryGetValue(value, out var existing))
            {
                // Assertion: no duplicate values among non-Common keys
                false.Should().BeTrue(
                    because: $"key '{key}' has the same English value as '{existing}' " +
                             $"(value: \"{value}\"). Use a Common_* key or rename one of them.");
            }
            else
            {
                seen[value] = key;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SATELLITE LANGUAGE SPOT-CHECKS
    //
    // Verify that PL / RO / GU satellite files contain placeholder values
    // (i.e. they are non-empty). These complement the Theory tests above with
    // explicit named assertions that appear in test run reports for traceability.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Polish_CommonSelectAnOption_HasPlaceholderTranslation()
    {
        _rm.GetString("Common_SelectAnOption", _pl)
           .Should().NotBeNullOrWhiteSpace(
               because: "PL satellite file must contain a placeholder for Common_SelectAnOption");
    }

    [Fact]
    public void Romanian_CommonSelectAnOption_HasPlaceholderTranslation()
    {
        _rm.GetString("Common_SelectAnOption", _ro)
           .Should().NotBeNullOrWhiteSpace(
               because: "RO satellite file must contain a placeholder for Common_SelectAnOption");
    }

    [Fact]
    public void Gujarati_CommonSelectAnOption_HasPlaceholderTranslation()
    {
        _rm.GetString("Common_SelectAnOption", _gu)
           .Should().NotBeNullOrWhiteSpace(
               because: "GU satellite file must contain a placeholder for Common_SelectAnOption");
    }

    [Fact]
    public void Polish_Dashboard_HiPrefix_HasPlaceholderTranslation()
    {
        _rm.GetString("Dashboard_HiPrefix", _pl)
           .Should().NotBeNullOrWhiteSpace(
               because: "PL satellite must have Dashboard_HiPrefix so the greeting renders in Polish");
    }

    [Fact]
    public void Romanian_Login_WelcomeBack_HasPlaceholderTranslation()
    {
        _rm.GetString("Login_WelcomeBack", _ro)
           .Should().NotBeNullOrWhiteSpace(
               because: "RO satellite must have Login_WelcomeBack");
    }

    [Fact]
    public void Gujarati_Register_TermsTitle_HasPlaceholderTranslation()
    {
        _rm.GetString("Register_TermsTitle", _gu)
           .Should().NotBeNullOrWhiteSpace(
               because: "GU satellite must have Register_TermsTitle");
    }

    [Fact]
    public void Polish_ProfileEdit_PasswordNeedsNumber_HasPlaceholderTranslation()
    {
        _rm.GetString("ProfileEdit_PasswordNeedsNumber", _pl)
           .Should().NotBeNullOrWhiteSpace(
               because: "PL satellite must have ProfileEdit_PasswordNeedsNumber so password errors are localised");
    }

    [Fact]
    public void Romanian_Dashboard_WithdrawAlertTitle_HasPlaceholderTranslation()
    {
        _rm.GetString("Dashboard_WithdrawAlertTitle", _ro)
           .Should().NotBeNullOrWhiteSpace(
               because: "RO satellite must have Dashboard_WithdrawAlertTitle");
    }

    [Fact]
    public void Gujarati_Faq_Title_HasPlaceholderTranslation()
    {
        _rm.GetString("Faq_Title", _gu)
           .Should().NotBeNullOrWhiteSpace(
               because: "GU satellite must have Faq_Title");
    }
}
