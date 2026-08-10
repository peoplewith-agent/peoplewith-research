using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using PeopleWithResearch;

using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Datasync.Client;

namespace PeopleWithResearch
{
    /// <summary>
    /// Handles all operations on the User Object
    /// </summary>
    public class UserManager
    {

        static UserManager defaultInstance = new UserManager();
          DatasyncClient client;
          IRemoteTable<user> userTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        /// <summary>
        /// Main constructor for the user manager class
        /// </summary>
        // Main constructor for the user manager class
        UserManager()
        {
            // 1. Setup your specific header values
            //var headerHandler = new ZumoApiVersionHandler(Constants.ClientPrincipal, Constants.ApiRole);

            // 2. Add it to the pipeline (keep your LoggingHandler if you have one)
            var options = new DatasyncClientOptions
            {
                // Pass the principal and role here
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler(Constants.ClientPrincipal, Constants.ApiRole) }
            };

            // 3. Initialize the client
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);

            // 4. Get your table as usual
            userTable = client.GetRemoteTable<user>();

            //var options = new DatasyncClientOptions
            //{
            //    HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            //};

            //// Initialize the client.
            //client = TokenRequestor == null
            //    ? new DatasyncClient(Constants.ApplicationURL, options)
            //    : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            //userTable = client.GetRemoteTable<user>();


            // var handler = new zumoapiversionhandler
            // {
            //     innerhandler = new httpclienthandler()
            // };

            // // Ensure the base address is an HTTPS URL
            // var httpClient = new HttpClient(handler)
            // {
            //     BaseAddress = new Uri(Constants.ApplicationURL)
            // };


            // var options = new DatasyncClientOptions
            // {
            //     HttpPipeline = new DelegatingHandler[] { handler }
            // };

            //// MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
            // // this.client = new DatasyncClient(new Uri(Constants.ApplicationURL), new HttpClientHandler());
            // this.client = new DatasyncClient(new Uri(Constants.ApplicationURL), options);
            // this.userTable = client.GetRemoteTable<Users>("Users"); // "Users" should be the name of your table
        }

        public UserManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static UserManager DefaultManager => defaultInstance;

        public async Task<ObservableCollection<user>> GetUsersAsync()
        {
            try
            {
                var observableUsers = new ObservableCollection<user>();

                var enumerable = userTable.ToAsyncEnumerable();
                await foreach (var item in enumerable)
                {
                    // Process each item
                    observableUsers.Add(item);
                }
                // Assuming GetItemAsync returns a List<User> or IEnumerable<User>
               // var userss = await userTable.ToListAsync();

                // Check if the users list is properly populated
                if (observableUsers == null)
                {
                    throw new InvalidOperationException("Failed to retrieve users from the userTable.");
                }

                // Convert the result to ObservableCollection
                //var observableUsers = new ObservableCollection<user>(userss);

                return observableUsers;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Optionally, log the stack trace or other details
                Console.WriteLine(ex.StackTrace);

                // Handle exceptions (e.g., log them)
                throw;
            }

        }

        public async Task<user> GetUserAsync(string id)
        {
            try
            {
             //   long count = await userTable.CountItemsAsync();

                return await userTable.GetItemAsync(id);
               
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                throw;
            }
        }

        public async Task InsertUser(user item)
        {
            //if ID is null, it is a new record.
            if (item.Id == null)
            {
                await userTable.InsertItemAsync(item);
            }
            //otherwise update existing record
            else
            {
                await userTable.ReplaceItemAsync(item);
            }
        }

        public DatasyncClient CurrentClient
        {
            get { return client; }
        }

        public int CalculateUserAge(DateTime Dob)
        {


            DateTime Now = DateTime.Now;
            int Years = new DateTime(DateTime.Now.Subtract(Dob).Ticks).Year - 1;
            DateTime PastYearDate = Dob.AddYears(Years);
            int Months = 0;
            for (int i = 1; i <= 12; i++)
            {
                if (PastYearDate.AddMonths(i) == Now)
                {
                    Months = i;
                    break;
                }
                else if (PastYearDate.AddMonths(i) >= Now)
                {
                    Months = i - 1;
                    break;
                }
            }
            int Days = Now.Subtract(PastYearDate.AddMonths(Months)).Days;
            int Hours = Now.Subtract(PastYearDate).Hours;
            int Minutes = Now.Subtract(PastYearDate).Minutes;
            int Seconds = Now.Subtract(PastYearDate).Seconds;

            //return new DateTime(Years, Months, Days, Hours, Minutes, Seconds);

            return Years;



        }


        /// <summary>
        /// Checks if offline app service hs been enabled
        /// </summary>
        /// <value><c>true</c> if is offline enabled; otherwise, <c>false</c>.</value>
        //      public bool IsOfflineEnabled
        //      {
        //          get { return userTable is Microsoft.WindowsAzure.MobileServices.Sync.IMobileServiceSyncTable<User>; }
        //      }

        /// <summary>
        /// Returns an observable collection of users from the database
        /// </summary>
        /// <returns>The users.</returns>
        public async Task<ObservableCollection<user>> getUsers()
        {
            try
            {

                IEnumerable<user> items = await userTable
                    .ToListAsync();

                return new ObservableCollection<user>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        /// <summary>
        /// Checks the user.
        /// </summary>
        /// <returns>The user.</returns>
        /// <param name="email">Email.</param>
		public async Task<ObservableCollection<user>> checkUser(string email)
        {
            try
            {

                IEnumerable<user> items = await userTable.Where(item => item.Email == email)
                    .ToListAsync();

                return new ObservableCollection<user>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        /// <summary>
        /// Checks the user.
        /// </summary>
        /// <returns>The user.</returns>
        /// <param name="id">Email.</param>
        public async Task<ObservableCollection<user>> getUserInfo(string id)
        {
            try
            {

                IEnumerable<user> items = await userTable.Where(item => item.Id == id)
                    .ToListAsync();

                return new ObservableCollection<user>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<user>> getUserInfoEPIDandGPID(string epid, string gpid)
        {
            try
            {

                IEnumerable<user> items = await userTable.Where(item => item.Epid == epid && item.Gpid == gpid)
                    .ToListAsync();

                return new ObservableCollection<user>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<user>> getUserInfoEPIDandGPIDandSignupcode(string epid, string gpid, string signupcode)
        {
            try
            {

                IEnumerable<user> items = await userTable.Where(item => item.Epid == epid && item.Gpid == gpid && item.Signupid == signupcode)
                    .ToListAsync();

                return new ObservableCollection<user>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }


        public async Task<ObservableCollection<user>> getUserInfoGPID(string gpid)
        {
            try
            {

                IEnumerable<user> items = await userTable.Where(item => item.Gpid == gpid)
                    .ToListAsync();

                return new ObservableCollection<user>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        /// <summary>
        /// Checks the user.
        /// </summary>
        /// <returns>The user.</returns>
        /// <param name="id">Email.</param>
        public async Task<user> LookupUser(string id)
        {
            try
            {

                user user = await userTable.GetItemAsync(id);


                return user;
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }


        /// <summary>
        /// Adds or Updates user in system
        /// </summary>
        /// <returns>The user.</returns>
        /// <param name="item">Item.</param>
        public async Task AddUser(user item)
        {
            //if ID is null, it is a new record.
            if (item.Id == null)
            {
                await userTable.InsertItemAsync(item);
            }

            //otherwise update existing record
            else
            {
                await userTable.ReplaceItemAsync(item);
            }
        }

        /// <summary>
        /// Delete User Method
        /// </summary>
        /// <returns>The user.</returns>
        /// <param name="item">Item.</param>
        public async Task DeleteUser(user item)
        {
            try
            {
                await userTable.DeleteItemAsync(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Convert to Hex
        /// </summary>
        /// <returns>The string to hex.</returns>
        /// <param name="asciiString">ASCII string.</param>
        public string ConvertStringToHex(string asciiString)
        {


            string hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += String.Format("{0:x2}", System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }

        /// <summary>
        /// Convert to String
        /// </summary>
        /// <returns>The hex to string.</returns>
        /// <param name="HexValue">Hex value.</param>
        public string ConvertHexToString(string HexValue)
        {
            string StrValue = "";
            while (HexValue.Length > 0)
            {
                StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString();
                HexValue = HexValue.Substring(2, HexValue.Length - 2);
            }
            return StrValue;
        }


        /// <summary>
        /// Extracts the token from the Facebook URL
        /// </summary>
        /// <returns>The token.</returns>
        /// <param name="url">URL.</param>
		public string ExtractToken(string url)

        {

            if (url.Contains("access_token") || url.Contains("&expires_in"))

            {
                var replacement = url.Replace("https://www.facebook.com/connect/login_success.html#access_token=", "");


                var accessToken = replacement.Remove(replacement.IndexOf("&expires_in="));

                return accessToken;
            }

            return string.Empty;
        }

        public byte[] GetHash(string data)
        {
            //var hash = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Md5);
            //byte[] dataTB = Encoding.UTF8.GetBytes(data);
            //return hash.HashData(dataTB);

            using (var md5 = MD5.Create())
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                return md5.ComputeHash(dataBytes);
            }
        }

        /// <summary>
        /// Converts the generated array to hex
        /// </summary>
        /// <returns>The array to hex.</returns>
        /// <param name="hash">Hash.</param>
        public string ByteArrayToHex(byte[] hash)
        {
            var hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                hex.AppendFormat("{0:x2}", b);

            return hex.ToString();
        }

        /// <summary>
        /// Gets the facebook profile information
        /// </summary>
        /// <returns>The facebook info async.</returns>
        /// <param name="accessToken">Access token.</param>
        //public async Task<FacebookProfile> GetFacebookInfoAsync(string accessToken)
        //      {
        //          var requestUrl =
        //              "https://graph.facebook.com/v2.7/me/?fields=name,picture,work,website,religion,location,locale,link,cover,age_range,birthday,devices,email,first_name,last_name,gender,hometown,is_verified,languages&access_token="
        //              + accessToken;

        //          var httpClient = new HttpClient();

        //          var userJson = await httpClient.GetStringAsync(requestUrl);

        //          var facebookProfile = JsonConvert.DeserializeObject<FacebookProfile>(userJson);

        //          return facebookProfile;
        //      }

        //      /// <summary>
        //      /// Send Email to User
        //      /// </summary>
        //      /// <param name="email">Email.</param>
        //      /// <param name="name">Name.</param>
        //      public async Task SendSuccessEmail(string email, string name)
        //      {
        //          HttpClient httpClient = new HttpClient();

        //          Uri emailUri = new Uri(Constants.EmailURL + email + "/" + name + "/");
        //          string emailuri = emailUri.ToString();

        //          var response = await httpClient.GetAsync(emailUri);

        //          if (response.IsSuccessStatusCode)
        //          {
        //              string content = await response.Content.ReadAsStringAsync();
        //              //Debug.WriteLine(content);
        //          }

        //          await Task.Delay(1);


        //      }

        //      /// <summary>
        //      /// Geneartes an MD5 Hash from the Database
        //      /// </summary>
        //      /// <returns>The hash.</returns>
        //      /// <param name="data">Data.</param>
        //      public byte[] GetHash(string data)
        //      {
        //          var hash = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Md5);
        //          byte[] dataTB = Encoding.UTF8.GetBytes(data);
        //          return hash.HashData(dataTB);
        //      }



    }
}
