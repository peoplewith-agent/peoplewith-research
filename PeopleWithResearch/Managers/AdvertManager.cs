using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
//sing Acr.Notifications;
using Microsoft.Datasync.Client;

namespace PeopleWithResearch
{
    /// <summary>
    /// Handles all operations with user/medicine objects
    /// </summary>
    public class AdvertManager
    {
        static AdvertManager defaultInstance = new AdvertManager();
        DatasyncClient client;
        IRemoteTable<advert> advertTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private AdvertManager()
              {
              var options = new DatasyncClientOptions
              {
                  HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
              };

        // Initialize the client.
        client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            advertTable = client.GetRemoteTable<advert>();

        }

        public AdvertManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }
        public static AdvertManager DefaultManager => defaultInstance;


        /// <summary>
        /// Returns an observable collection of Announcements from the Database
        /// </summary>
        /// <returns>The Announcements.</returns>
        /// 
        public async Task<ObservableCollection<advert>> GetAdverts()
        {
            try
            {
                //new 
                var observableUsers = new ObservableCollection<advert>();

                var enumerable = advertTable.ToAsyncEnumerable();
                await foreach (var item in enumerable)
                {
                    // Process each item
                    observableUsers.Add(item);
                }
                // Assuming GetItemAsync returns a List<User> or IEnumerable<User>

                // Check if the users list is properly populated
                if (observableUsers == null)
                {
                    throw new InvalidOperationException("Failed to retrieve users from the userTable.");
                }

                // Convert the result to ObservableCollection
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

        public async Task<advert> GerAdvertAsync(string id)
        {
            try
            {
                //   long count = await userTable.CountItemsAsync();

                return await advertTable.GetItemAsync(id);

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                throw;
            }
        }

        public async Task InsertAdvert(advert item)
        {
            //if ID is null, it is a new record.
            if (item.Id == null)
            {
                await advertTable.InsertItemAsync(item);
            }
            //otherwise update existing record
            else
            {
                await advertTable.ReplaceItemAsync(item);
            }
        }

        public async Task<ObservableCollection<advert>> GetSpecficAd(string advertid)
        {
            try
            {

                IEnumerable<advert> items = await advertTable.Where(Item => Item.AdvertID == advertid)
            .ToListAsync();

                return new ObservableCollection<advert>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }


        /// <summary>
        /// Returns the current app service client instance
        /// </summary>
        /// <value>The current client.</value>
        //public MobileServiceClient CurrentClient
        //{
        //    get { return client; }
        //}

        /// <summary>
        /// Checks if offline app service hs been enabled
        /// </summary>
        /// <value><c>true</c> if is offline enabled; otherwise, <c>false</c>.</value>
        //public bool IsOfflineEnabled
        //{
        //    get { return advertTable is Microsoft.WindowsAzure.MobileServices.Sync.IMobileServiceSyncTable<advert>; }
        //}


        /// <summary>
        /// Adds or Updates Announcement in the database
        /// </summary>
        /// <returns>The user.</returns>
        /// <param name="item">Item.</param>
        //public async Task AddAdvert(advert item)
        //{
        //    try
        //    {
        //        if (item.Id == null)

        //        {
        //            await advertTable.InsertAsync(item);
        //        }
        //        else
        //        {
        //            await advertTable.UpdateAsync(item);


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //    }




    }
    }

