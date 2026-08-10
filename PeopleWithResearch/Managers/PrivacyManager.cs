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
    public class PrivacyManager
    {
        static PrivacyManager defaultInstance = new PrivacyManager();
        DatasyncClient client;
        IRemoteTable<privacyPolicy> privacyPolicyTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private PrivacyManager()
        {

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            privacyPolicyTable = client.GetRemoteTable<privacyPolicy>();
        }

        public PrivacyManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static PrivacyManager DefaultManager => defaultInstance;
        public async Task<ObservableCollection<privacyPolicy>> GetprivacyPolicy()
        {
            try
            {

                IEnumerable<privacyPolicy> items = await privacyPolicyTable.ToListAsync();

                return new ObservableCollection<privacyPolicy>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<privacyPolicy>> GetprivacyPolicySignupID()
        {
            try
            {

                IEnumerable<privacyPolicy> items = await privacyPolicyTable.Where(x => x.AdvertID == "IID3").ToListAsync();

                return new ObservableCollection<privacyPolicy>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task AddprivacyPolicy(privacyPolicy item)
        {
            try
            {
                if (item.Id == null)

                {
                    await privacyPolicyTable.InsertItemAsync(item);
                }
                else
                {
                    await privacyPolicyTable.ReplaceItemAsync(item);


                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
