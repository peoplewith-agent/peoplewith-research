using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Datasync.Client;
using PeopleWithResearch;


namespace PeopleWithResearch
{
    /// <summary>
    /// Handles all operations with user/medicine objects
    /// </summary>
    public class FeatureManager
    {
        static FeatureManager defaultInstance = new FeatureManager();
        DatasyncClient client;
        IRemoteTable<features> featuresTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private FeatureManager()
        {

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            featuresTable = client.GetRemoteTable<features>();
      
        }

        public FeatureManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static FeatureManager DefaultManager => defaultInstance;
        /// <summary>
        /// Returns an observable collection of Announcements from the Database
        /// </summary>
        /// <returns>The Announcements.</returns>
        public async Task<ObservableCollection<features>> Getfeaturess()
        {
            try
            {

                IEnumerable<features> items = await featuresTable
            .ToListAsync();

                return new ObservableCollection<features>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<features>> GetSpecficAd(string advertid)
        {
            try
            {

                IEnumerable<features> items = await featuresTable.Where(Item => Item.Advertid == advertid).Where(item => item.Deleted == false)
            .ToListAsync();

                return new ObservableCollection<features>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }




        /// <summary>
        /// Adds or Updates Announcement in the database
        /// </summary>
        /// <returns>The user.</returns>
        /// <param name="item">Item.</param>
        public async Task Addfeatures(features item)
        {
            try
            {
                if (item.Id == null)

                {
                    await featuresTable.InsertItemAsync(item);
                }
                else
                {
                    await featuresTable.ReplaceItemAsync(item);


                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }
    }
}
