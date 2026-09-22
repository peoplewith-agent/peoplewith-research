using FreakyKit.Utils;
using Microsoft.Maui.ApplicationModel.Communication;
using Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace PeopleWithResearch
{
    public class APICalls
    {
        public const string ApplicationURL = "https://pwresearchapi.peoplewith.com/api/";
        public const string AzureUrl = "https://peoplewithappiamges.blob.core.windows.net/appimages/";
        public const string Imperial = " https://peoplewithappiamges.blob.core.windows.net/imperial/";
        public const string PasswordRestLink = "https://portal.peoplewith.com/hopper-email/process-password-request-research.php?email=";
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=peoplewithappiamges;AccountKey=9maBMGnjWp6KfOnOuXWHqveV4LPKyOnlCgtkiKQOeA+d+cr/trKApvPTdQ+piyQJlicOE6dpeAWA56uD39YJhg==;EndpointSuffix=core.windows.net";
        public const string SendNudgeNotification = "https://hpru.peoplewith.com/hub/user-notification.php?ak=1E33C0AC-3393-4C34-834A-DE5FDBCBB3CC&uid=";

        // Use a single static client
        public static string CheckSignUpCode => $"{ApplicationURL}signupcode?$filter=signupcodeid%20eq%20";
        public static string Checkuseremail => $"{ApplicationURL}user?$filter=email%20eq%20";

        public static string CheckPostcode => $"{ApplicationURL}user?$filter=postcode%20eq%20";
        public static string UserHousehold => $"{ApplicationURL}householdgroup";

        public static string UserConsent => $"{ApplicationURL}userconsent";

        //public static HttpClient GetClient()
        //{
        //    // Lock ensures only one thread configures the headers
        private static readonly APICalls _instance = new APICalls();
        public static APICalls Instance => _instance;

        private static readonly HttpClient Client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        private APICalls() { }

        public HttpClient GetClient()
        {
            // Always ensure these headers are present before returning
            if (!Client.DefaultRequestHeaders.Contains("X-MS-CLIENT-PRINCIPAL"))
            {
                Client.DefaultRequestHeaders.TryAddWithoutValidation("X-MS-CLIENT-PRINCIPAL", Constants.ClientPrincipal);
            }

            if (!Client.DefaultRequestHeaders.Contains("X-MS-API-ROLE"))
            {
                Client.DefaultRequestHeaders.TryAddWithoutValidation("X-MS-API-ROLE", Constants.ApiRole);
            }

            return Client;
        }

        //CrashDetected crashHandler = new CrashDetected();

        //async public Task NotasyncMethod(Exception Ex)
        //{
        //    try
        //    {
        //        await crashHandler.SentryCrashDetected(Ex);
        //        //await Navigation.PushAsync(new ErrorPage("Dashboard", Ex), false);
        //    }
        //    catch (Exception ex)
        //    {
        //        //Dunno 
        //    }
        //}

        public async Task<string> GetCurrentAppVersion()
        {
            try
            {
                var url = $"{ApplicationURL}appversion";
                var configuredClient = GetClient();
                HttpResponseMessage response = await configuredClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var GetItem = JsonConvert.DeserializeObject<ApiAppVersion>(data);
                    if (GetItem != null)
                    {
                        var Version = String.Empty;
                        var Item = GetItem?.Value.FirstOrDefault();
                        if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
                        {
                            Version = Item.iosversion;
                        }
                        else
                        {
                            Version = Item.androidversion;
                        }
                        return Version;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return String.Empty;
                }
            }
            catch (Exception ex) when (
         ex is HttpRequestException ||
         ex is WebException ||
         ex is TaskCanceledException)
            {
                //CrashDetected.LogCrash(ex, Navigation, "GetCurrentAppVersion");
                return string.Empty;

            }

        }

        private async Task<T> ExecuteWithRetry<T>(Func<Task<T>> action, int maxRetries = 3, [CallerMemberName] string callerName = "")
        {
            int delay = 2000;
            Exception lastException = null;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is WebException)
                {
                    lastException = ex;
                    if (i == maxRetries - 1) break;
                    Debug.WriteLine($"API Throttled/Slow. Retry {i + 1} in {delay}ms...");
                    await Task.Delay(delay);
                    delay *= 2;
                }
            }
            if (lastException != null)
            {
                CrashDetected.LogCrash(lastException, $"ExecuteWithRetry_{callerName}_ExhaustedRetries");
            }

            return default;
        }

        //private async Task<T> ExecuteWithRetry<T>(Func<Task<T>> action, int maxRetries = 3)
        //{
        //    int delay = 2000; // Start with 2 seconds
        //    for (int i = 0; i < maxRetries; i++)
        //    {
        //        try
        //        {
        //            return await action();
        //        }
        //        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is WebException)
        //        {
        //            if (i == maxRetries - 1) break;
        //            Debug.WriteLine($"API Throttled/Slow. Retry {i + 1} in {delay}ms...");
        //            await Task.Delay(delay);
        //            delay *= 2; 
        //        }
        //    }
        //    return default;
        //}

        public async Task<ObservableCollection<user>> GetuserDetails(string userId)
        {
            return await ExecuteWithRetry(async () =>
            {
                var url = $"{ApplicationURL}user/userid/{userId}";
                var response = await GetClient().GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var userResponse = JsonConvert.DeserializeObject<APIUserResponse>(content);

                    if (userResponse?.Value != null)
                    {
                        foreach (var o in userResponse.Value)
                        {
                            o.DetailsList = DeserializeNestedJson<Detailslist>(o.Details);
                        }

                        return new ObservableCollection<user>(userResponse.Value);
                    }
                }

                return new ObservableCollection<user>();
            });
        }

        public async Task<ObservableCollection<newuser>> Getuser()
        {
            return await ExecuteWithRetry(async () =>
            {
                var userId = Helpers.Settings.UsersID;
                if (string.IsNullOrEmpty(userId)) return new ObservableCollection<newuser>();
                var url = $"{ApplicationURL}user/userid/{userId}";
                var response = await GetClient().GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var userResponse = JsonConvert.DeserializeObject<APINewUserResponse>(content);

                    if (userResponse?.Value != null)
                    {
                        foreach (var o in userResponse.Value)
                        {
                            o.DetailsList = DeserializeNestedJson<Detailslist>(o.details);
                            o.NotificationDetails = DeserializeNestedJson<NotificationData>(o.notificationtime);
                            o.AccountCreated = o.createdAt.ToLocalTime();
                        }

                        return new ObservableCollection<newuser>(userResponse.Value);
                    }
                }

                return new ObservableCollection<newuser>();
            });
        }

        public async Task<ObservableCollection<newuser>> Getuserspostcodes(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
                return new ObservableCollection<newuser>();

            return await ExecuteWithRetry(async () =>
            {
                var cleanPostcode = Uri.EscapeDataString(postcode.Trim());
                //var url = $"{ApplicationURL}user/postcode/{cleanPostcode}";
                var url = $"{APICalls.CheckPostcode}%27{cleanPostcode}%27";

                var response = await GetClient().GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var userResponse = JsonConvert.DeserializeObject<APINewUserResponse>(content);

                    if (userResponse?.Value != null)
                    {
                        foreach (var o in userResponse.Value)
                        {
                            o.DetailsList = DeserializeNestedJson<Detailslist>(o.details);
                        }

                        return new ObservableCollection<newuser>(userResponse.Value);
                    }
                }

                return new ObservableCollection<newuser>();
            });
        }

        public async Task<ObservableCollection<signupcode>> GetSingupCode()
        {
            var signup = Helpers.Settings.SignUp;

            if (string.IsNullOrEmpty(signup))
            {
                return new ObservableCollection<signupcode>();
            }

            return await ExecuteWithRetry(async () =>
            {
                var url = $"{CheckSignUpCode}%27{signup}%27";
                var response = await GetClient().GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var userResponse = JsonConvert.DeserializeObject<ApiResponseSignUpCode>(content);

                    if (userResponse?.Value != null)
                    {
                        foreach (var o in userResponse.Value)
                        {
                            o.informationlist = DeserializeNestedJson<InformationDetails>(o.information);
                            o.FAQList = DeserializeNestedJson<FAQItem>(o.faqs);

                            if (o.informationlist == null || !o.informationlist.Any()) continue;

                            foreach (var item in o.informationlist)
                            {
                                bool ContainsVideo = item.title.Contains("Video", StringComparison.OrdinalIgnoreCase)
                                 || item.description.Contains("video", StringComparison.OrdinalIgnoreCase);

                                if (ContainsVideo)
                                {
                                    item.type = "video";
                                }

                                switch (item.type)
                                {
                                    case "PDF":
                                        item.img = "pdf.png";
                                        item.ColorTheme = "#EF4444";
                                        break;
                                    case "video":
                                        item.img = "videoicon.png";
                                        item.ColorTheme = "#8B5CF6";
                                        break;
                                    case "phone":
                                        item.img = "call.png";
                                        item.ColorTheme = "#10B981";
                                        break;
                                    case "email":
                                        item.img = "emailicon.png";
                                        item.ColorTheme = "#F59E0B";
                                        break;
                                    default:
                                        item.img = "webicon.png";
                                        item.ColorTheme = "#6366F1";
                                        break;
                                }
                            }
                        }

                        return new ObservableCollection<signupcode>(userResponse.Value);
                    }
                }

                return new ObservableCollection<signupcode>();
            });
        }

        public async Task<bool> PasswordRest(string Email)
        {
            try
            {
                var payload = new { Email = Email.Trim() };
                var content = JsonContent.Create(payload);
                var url = $"{APICalls.PasswordRestLink}{Uri.EscapeDataString(payload.Email)}";
                var response = await APICalls.Instance.GetClient().PostAsync(url, content);
                return (response.IsSuccessStatusCode);
            }
            catch (Exception Ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserData(string userId, Dictionary<string, object> fieldsToUpdate)
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(fieldsToUpdate);

                var url = $"{APICalls.ApplicationURL}user/userid/{userId}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await APICalls.Instance.GetClient().PatchAsync(url, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUser(newuser newuser)
        {
            try
            {
                var url = $"{APICalls.ApplicationURL}user/userid/{newuser.userid}";

                var json = System.Text.Json.JsonSerializer.Serialize(newuser);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await APICalls.Instance.GetClient().PatchAsync(url, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //public async Task<ObservableCollection<user>> GetuserDetails(string userid)
        //{
        //    try
        //    {
        //        var url = $"{ApplicationURL}user/userid/{userid}";

        //        var httpClient = GetClient();

        //        HttpResponseMessage response = await httpClient.GetAsync(url);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            string content = await response.Content.ReadAsStringAsync();
        //            var userResponse = JsonConvert.DeserializeObject<APIUserResponse>(content);

        //            if (userResponse?.Value != null)
        //                return new ObservableCollection<user>(userResponse.Value);
        //        }

        //        // Log the failure for debugging
        //        Debug.WriteLine($"API Error: {response.StatusCode} - {response.ReasonPhrase}");
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Exception in GetuserDetails: {ex.Message}");
        //        return null;
        //    }
        //}

        public async Task<ObservableCollection<newuser>> CheckEmailExists(string email)
        {
            try
            {
                var url = $"{APICalls.Checkuseremail}%27{email}%27";
                var configuredClient = GetClient();
                HttpResponseMessage response = await configuredClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var userResponse = JsonConvert.DeserializeObject<APINewUserResponse>(content);
                    if (userResponse?.Value != null)
                    {
                        var activeUsers = userResponse.Value.Where(u => u.deleted == false).ToList();
                        foreach (var o in activeUsers)
                        {
                            o.DetailsList = DeserializeNestedJson<Detailslist>(o.details);
                            o.NotificationDetails = DeserializeNestedJson<NotificationData>(o.notificationtime);
                            o.AccountCreated = o.createdAt.ToLocalTime();
                        }
                        return new ObservableCollection<newuser>(activeUsers);
                    }
                }

                return new ObservableCollection<newuser>();
            }
            catch (Exception ex)
            {
                return new ObservableCollection<newuser>();
            }
        }


        public async Task<ObservableCollection<householdgroup>> GetUserHouseholdInfo(string HHgroupID)
        {
            try
            {
                var configuredClient = GetClient();
                string urlWithQuery = $"{UserHousehold}?$filter=householdgroupid eq '{HHgroupID}'";
                HttpResponseMessage response = await configuredClient.GetAsync(urlWithQuery);

                var newcollection = new ObservableCollection<householdgroup>();

                if (response.IsSuccessStatusCode)
                {
                    string contentconsent = await response.Content.ReadAsStringAsync();
                    var userResponseconsent = JsonConvert.DeserializeObject<ApiResponseUserHousehold>(contentconsent);

                    // Ensure the response value isn't null before looping
                    var consent = userResponseconsent?.Value;

                    if (consent != null)
                    {
                        foreach (var item in consent)
                        {
                            if (!string.IsNullOrEmpty(item.groupuserdetails) && item.groupuserdetails != "[]")
                            {
                                try
                                {
                                    item.userdetailslist = JsonConvert.DeserializeObject<ObservableCollection<householdgroupjsondetails>>(item.groupuserdetails);
                                }
                                catch (JsonSerializationException)
                                {
                                    // Fallback for single objects instead of arrays
                                    var singleItem = JsonConvert.DeserializeObject<householdgroupjsondetails>(item.groupuserdetails);
                                    item.userdetailslist = new ObservableCollection<householdgroupjsondetails> { singleItem };
                                }
                            }

                            //if (!string.IsNullOrEmpty(item.details) && item.details != "[]")
                            //{
                            //    try
                            //    {
                            //        item.studydetails = JsonConvert.DeserializeObject<ObservableCollection<householdstudyrecord>>(item.details);
                            //    }
                            //    catch (JsonSerializationException)
                            //    {
                            //        // Fallback for single objects instead of arrays 
                            //        var singleItem = JsonConvert.DeserializeObject<householdstudyrecord>(item.details);
                            //        item.studydetails = new ObservableCollection<householdstudyrecord> { singleItem };
                            //    }
                            //}

                            // CRITICAL: Add the processed item to your collection!
                            newcollection.Add(item);
                        }
                    }

                    return newcollection;
                }
                else
                {
                    // Log errorcontent if necessary
                    var content = await response.Content.ReadAsStringAsync();
                    return new ObservableCollection<householdgroup>();
                }
            }
            catch (Exception ex) when (
      ex is HttpRequestException ||
      ex is WebException ||
      ex is TaskCanceledException)
            {
                // await NotasyncMethod(ex);
                return new ObservableCollection<householdgroup>();
            }
            catch (Exception ex)
            {
                //  await NotasyncMethod(ex);
                return new ObservableCollection<householdgroup>();
            }
        }


        public async Task<userconsent> PostUserConsentAsync(userconsent ConsentPassed)
        {
            try
            {
                var configuredClient = GetClient();
                var url = APICalls.UserConsent;
                string jsonns = System.Text.Json.JsonSerializer.Serialize<userconsent>(ConsentPassed);
                StringContent contenttts = new StringContent(jsonns, Encoding.UTF8, "application/json");
                var response = await configuredClient.PostAsync(url, contenttts);
                var errorResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string 
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return null;
                }
                else
                {
                    string errorcontent = await response.Content.ReadAsStringAsync();
                    var s = errorcontent;
                    return null;
                }
            }
            catch (Exception ex) when (
    ex is HttpRequestException ||
    ex is WebException ||
    ex is TaskCanceledException)
            {
                //  await NotasyncMethod(ex);
                return null;
            }
            catch (Exception ex)
            {
                //await NotasyncMethod(ex);
                return null;
            }
        }

        //public async Task<ObservableCollection<questionnaires>> GetSingleQuestionnaire(string questionnaireid)
        //{
        //    try
        //    {
        //        var url = $"{ApplicationURL}questionnaires/";
        //        string urlWithQuery = $"{url}?$filter=questionnaireid eq '{questionnaireid}'";
        //        var configuredClient = GetClient();
        //        HttpResponseMessage responseconsent = await configuredClient.GetAsync(urlWithQuery);

        //        if (responseconsent.IsSuccessStatusCode)
        //        {
        //            if (responseconsent.IsSuccessStatusCode)
        //            {
        //                string contentconsent = await responseconsent.Content.ReadAsStringAsync();
        //                var userResponseconsent = System.Text.Json.JsonSerializer.Deserialize<ApiResponseQuestionnaire>(contentconsent);

        //                if (userResponseconsent?.Value != null)
        //                {
        //                    return new ObservableCollection<questionnaires>(userResponseconsent.Value);
        //                }
        //            }

        //            return new ObservableCollection<questionnaires>();

        //        }
        //        else
        //        {
        //            return new ObservableCollection<questionnaires>();
        //        }
        //    }
        //    catch (Exception ex) when (
        //    ex is HttpRequestException ||
        //    ex is WebException ||
        //    ex is TaskCanceledException)
        //    {
        //        return new ObservableCollection<questionnaires>();
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ObservableCollection<questionnaires>();
        //    }
        //}
        public async Task<ObservableCollection<questionnaires>> GetSingleQuestionnaire(string IDpassed)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(IDpassed)) return new ObservableCollection<questionnaires>();

                var url = $"{ApplicationURL}questionnaires/";
                string urlWithQuery = $"{url}?$filter=questionnaireid eq '{IDpassed}'";
                var configuredClient = GetClient();
                HttpResponseMessage responseconsent = await configuredClient.GetAsync(urlWithQuery);

                if (responseconsent.IsSuccessStatusCode)
                {
                    if (responseconsent.IsSuccessStatusCode)
                    {
                        string contentconsent = await responseconsent.Content.ReadAsStringAsync();
                        var userResponseconsent = System.Text.Json.JsonSerializer.Deserialize<ApiResponseQuestionnaire>(contentconsent);

                        if (userResponseconsent?.Value != null)
                        {
                            foreach (var o in userResponseconsent.Value)
                            {
                                o.QuestionAnswerJson = DeserializeNestedJson<QuestionAnswerJson>(o.QuestionAnswerJsonRaw);
                                o.Confirmationmessage = DeserializeNestedJson<confirmationmessage>(o.ConfirmationMessageRaw);
                            }

                            return new ObservableCollection<questionnaires>(userResponseconsent.Value);
                        }
                    }

                    return new ObservableCollection<questionnaires>();

                }
                else
                {
                    return new ObservableCollection<questionnaires>();
                }
            }
            catch (Exception ex) when (
     ex is HttpRequestException ||
     ex is WebException ||
     ex is TaskCanceledException)
            {
                return new ObservableCollection<questionnaires>();
            }
            catch (Exception ex)
            {
                return new ObservableCollection<questionnaires>();
            }
        }


        public async Task<List<questionnaires>> GetAllQuestionnaire()
        {
            try
            {
                var url = $"{ApplicationURL}questionnaires/";
                var configuredClient = GetClient();
                HttpResponseMessage responseconsent = await configuredClient.GetAsync(url);

                if (responseconsent.IsSuccessStatusCode)
                {
                    string contentconsent = await responseconsent.Content.ReadAsStringAsync();
                    var userResponseconsent = System.Text.Json.JsonSerializer.Deserialize<ApiResponseQuestionnaire>(contentconsent);

                    if (userResponseconsent?.Value != null)
                    {
                        foreach (var o in userResponseconsent.Value)
                        {
                            o.QuestionAnswerJson = DeserializeNestedJson<QuestionAnswerJson>(o.QuestionAnswerJsonRaw);
                            o.Confirmationmessage = DeserializeNestedJson<confirmationmessage>(o.ConfirmationMessageRaw);
                        }

                        return userResponseconsent.Value.ToList();
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex) when (
     ex is HttpRequestException ||
     ex is WebException ||
     ex is TaskCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ObservableCollection<T> DeserializeNestedJson<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return new ObservableCollection<T>();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                if (json.Trim().StartsWith("\""))
                {
                    json = System.Text.Json.JsonSerializer.Deserialize<string>(json, options);
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<T>>(json, options);
                return result ?? new ObservableCollection<T>();
            }
            catch (System.Text.Json.JsonException)
            {
                try
                {
                    // Fallback: maybe it's a single item rather than a collection
                    var singleItem = System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
                    return singleItem != null
                        ? new ObservableCollection<T> { singleItem }
                        : new ObservableCollection<T>();
                }
                catch (System.Text.Json.JsonException singleEx)
                {
                    LogDeserializationFailure(typeof(T), singleEx, json);
                    return new ObservableCollection<T>();
                }
            }
            catch (Exception ex)
            {
                // Catch-all for non-JSON unexpected errors (e.g. bad initial string-unwrap)
                System.Diagnostics.Debug.WriteLine($"Unexpected error deserializing {typeof(T).Name}: {ex.Message}");
                return new ObservableCollection<T>();
            }
        }

        private void LogDeserializationFailure(Type targetType, System.Text.Json.JsonException ex, string json)
        {
            string errorDetails = $"[JSON Deserialization Failed]\n" +
                                  $"Target Type: {targetType.Name}\n" +
                                  $"Failed JSON Path: {ex.Path}\n" +
                                  $"Line Number: {ex.LineNumber}\n" +
                                  $"Position: {ex.BytePositionInLine}\n" +
                                  $"Reason: {ex.Message}";

            System.Diagnostics.Debug.WriteLine(errorDetails);
        }


        //Old
        //public ObservableCollection<T> DeserializeNestedJson<T>(string json)
        //{
        //    if (string.IsNullOrWhiteSpace(json) || json.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return new ObservableCollection<T>();
        //    }

        //    var options = new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    };

        //    try
        //    {
        //        if (json.Trim().StartsWith("\""))
        //        {
        //            json = System.Text.Json.JsonSerializer.Deserialize<string>(json, options);
        //        }

        //        var result = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<T>>(json, options);
        //        return result ?? new ObservableCollection<T>();
        //    }
        //    catch (System.Text.Json.JsonException collectionEx)
        //    {
        //        try
        //        {
        //            // Attempting to fallback to a single item
        //            var singleItem = System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
        //            return singleItem != null
        //                ? new ObservableCollection<T> { singleItem }
        //                : new ObservableCollection<T>();
        //        }
        //        catch (System.Text.Json.JsonException singleEx)
        //        {
        //            // --- DIAGNOSTIC CAPTURE ---
        //            // If both collection and single item deserialization fail, 
        //            // we capture the specific path and reason why the object mapping failed.

        //            string errorDetails = $"[JSON Deserialization Failed]\n" +
        //                                  $"Target Type: {typeof(T).Name}\n" +
        //                                  $"Failed JSON Path: {singleEx.Path}\n" +
        //                                  $"Line Number: {singleEx.LineNumber}\n" +
        //                                  $"Position: {singleEx.BytePositionInLine}\n" +
        //                                  $"Reason: {singleEx.Message}";

        //            // Prints directly to your Visual Studio Output Window
        //            System.Diagnostics.Debug.WriteLine(errorDetails);

        //            // Re-throw with the precise path details included so your app stack trace shows it
        //            throw new System.Text.Json.JsonException(errorDetails, singleEx);
        //            return new ObservableCollection<T>();
        //        }
        //        catch (Exception ex)
        //        {
        //            // Catch-all for any non-JSON unexpected errors
        //            System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}");
        //            return new ObservableCollection<T>();
        //        }
        //    }
        //}

        //public ObservableCollection<T> DeserializeNestedJson<T>(string json)
        //{
        //    if (string.IsNullOrWhiteSpace(json) || json.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return new ObservableCollection<T>();
        //    }

        //    try
        //    {
        //        if (json.Trim().StartsWith("\""))
        //        {
        //            json = System.Text.Json.JsonSerializer.Deserialize<string>(json);
        //        }

        //        var options = new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        };

        //        var result = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<T>>(json, options);
        //        return result ?? new ObservableCollection<T>();
        //    }
        //    catch (System.Text.Json.JsonException)
        //    {
        //        try
        //        {
        //            var singleItem = System.Text.Json.JsonSerializer.Deserialize<T>(json);
        //            return singleItem != null
        //                ? new ObservableCollection<T> { singleItem }
        //                : new ObservableCollection<T>();
        //        }
        //        catch (Exception Ex)
        //        {
        //            return new ObservableCollection<T>();
        //        }
        //    }
        //}

        public async Task<newuserquestionnaire> PostUserQuestionnaire(newuserquestionnaire userquestionnairepassed)
        {
            try
            {
                var configuredClient = GetClient();
                var url = $"{ApplicationURL}userquestionnaire";
                string jsonns = System.Text.Json.JsonSerializer.Serialize<newuserquestionnaire>(userquestionnairepassed);
                StringContent contenttts = new StringContent(jsonns, Encoding.UTF8, "application/json");
                var response = await configuredClient.PostAsync(url, contenttts);
                var errorResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseContent);
                    var firstItem = jsonResponse["value"]?[0];

                    if (firstItem != null)
                    {
                        // Parse ID
                        userquestionnairepassed.userquestionnaireid = firstItem["userquestionnaireid"]?.ToString();

                        //// Parse createdAt
                        //var createdAtRaw = firstItem["createdAt"]?.ToString();

                        //// Use AdjustToUniversal to ensure the Kind is set to Utc
                        //if (DateTime.TryParse(createdAtRaw, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime createdDate))
                        //{
                        //    userquestionnairepassed.createdAt = createdDate;
                        //}
                    }

                    return userquestionnairepassed;

                }
                else
                {
                    string errorcontent = await response.Content.ReadAsStringAsync();
                    var s = errorcontent;
                    return userquestionnairepassed;
                }
            }
            catch (Exception ex) when (
  ex is HttpRequestException ||
  ex is WebException ||
  ex is TaskCanceledException)
            {
                return userquestionnairepassed;
            }
            catch (Exception ex)
            {
                return userquestionnairepassed;
            }
        }

        public async Task<ObservableCollection<newuserquestionnaire>> GetUserQuestionnaires()
        {
            return await ExecuteWithRetry(async () =>
            {
                var userId = Helpers.Settings.UsersID;
                var url = $"{ApplicationURL}userquestionnaire?$filter=userid eq '{userId}'";

                var response = await GetClient().GetAsync(url);

                if (!response.IsSuccessStatusCode) return new ObservableCollection<newuserquestionnaire>();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiUserResponseQuestionnaire>(content);

                if (apiResponse?.Value == null) return new ObservableCollection<newuserquestionnaire>();

                var items = apiResponse.Value.Where(item => !item.deleted && item.questionnaireid != "b1_individual_questionnaire").ToList();
                foreach (var item in items)
                {
                    item.FeedbackList = DeserializeNestedJson<Feedback>(item.feedback);
                    item.DateTimeAdded = item.createdAt.ToLocalTime();
                }

                return new ObservableCollection<newuserquestionnaire>(items);
            });
        }

        public async Task<ObservableCollection<newuserquestionnaire>> GetUserQuestionnairesbyUserid(string userid)
        {
            return await ExecuteWithRetry(async () =>
            {
                // var userId = Helpers.Settings.UsersID;
                var url = $"{ApplicationURL}userquestionnaire?$filter=userid eq '{userid}'";

                var response = await GetClient().GetAsync(url);

                if (!response.IsSuccessStatusCode) return new ObservableCollection<newuserquestionnaire>();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiUserResponseQuestionnaire>(content);

                if (apiResponse?.Value == null) return new ObservableCollection<newuserquestionnaire>();

                var items = apiResponse.Value.Where(item => !item.deleted && item.questionnaireid != "b1_individual_questionnaire").ToList();
                foreach (var item in items)
                {
                    item.FeedbackList = DeserializeNestedJson<Feedback>(item.feedback);
                    item.DateTimeAdded = item.createdAt.ToLocalTime();
                }

                return new ObservableCollection<newuserquestionnaire>(items);
            });
        }
        public async Task<bool> UpdateHouseholdFeedback(
     ObservableCollection<householdgroupjsondetails> updateFeedback)
        {
            try
            {
                if (updateFeedback == null || !updateFeedback.Any())
                    return false;

                string householdGroupId = updateFeedback
                    .FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.household_group_id))
                    ?.household_group_id;

                if (string.IsNullOrWhiteSpace(householdGroupId))
                    return false;

                var url = $"{UserHousehold}/householdgroupid/{householdGroupId}";

                var feedbackWithoutMain = updateFeedback
                    .Where(a => !a.mainuser)
                    .ToList();

                var json = JsonConvert.SerializeObject(
    new { groupuserdetails = feedbackWithoutMain },
    Newtonsoft.Json.Formatting.None);


                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Patch, url)
                {
                    Content = content
                };

                var response = await GetClient().SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return false;
                }

                return true;
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is WebException ||
                ex is TaskCanceledException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }


        //        public async Task<ObservableCollection<newuserquestionnaire>> GetUserQuestionnaires()
        //        {
        //            try
        //            {
        //                var userId = Helpers.Settings.UsersID;
        //                var url = $"{ApplicationURL}userquestionnaire?$filter=userid eq '{userId}'";

        //                var configuredClient = GetClient();
        //                var response = await configuredClient.GetAsync(url);

        //                if (!response.IsSuccessStatusCode)
        //                    return new ObservableCollection<newuserquestionnaire>();

        //                var content = await response.Content.ReadAsStringAsync();
        //                var apiResponse = JsonConvert.DeserializeObject<ApiUserResponseQuestionnaire>(content);

        //                if (apiResponse?.Value == null)
        //                    return new ObservableCollection<newuserquestionnaire>();

        //                var filteredItems = apiResponse.Value
        //                    .Where(item => !item.deleted)
        //                    .Select(item =>
        //                    {
        //                        item.FeedbackList = DeserializeNestedJson<Feedback>(item.feedback);
        //                        return item;
        //                    });

        //                return new ObservableCollection<newuserquestionnaire>(filteredItems);
        //            }
        //            catch (Exception ex) when (
        //ex is HttpRequestException ||
        //ex is WebException ||
        //ex is TaskCanceledException)
        //            {
        //                return new ObservableCollection<newuserquestionnaire>();
        //            }
        //            catch (Exception ex)
        //            {
        //                return new ObservableCollection<newuserquestionnaire>();
        //            }
        //        }
    }
}