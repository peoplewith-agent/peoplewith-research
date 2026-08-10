using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Datasync.Client;

//sing Acr.Notifications;
using PeopleWithResearch;


namespace PeopleWithResearch
{
    /// <summary>
    /// Handles all operations with user/medicine objects
    /// </summary>
    public class IncidentHistoryManager
    {

        static IncidentHistoryManager defaultInstance = new IncidentHistoryManager();
        DatasyncClient client;
        IRemoteTable<incidentHistory> incidentHistoryTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;



        private IncidentHistoryManager()
        {
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            incidentHistoryTable = client.GetRemoteTable<incidentHistory>();
        }

        public IncidentHistoryManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static IncidentHistoryManager DefaultManager => defaultInstance;


        /// <summary>
        /// Returns an observable collection of Announcements from the Database
        /// </summary>
        /// <returns>The Announcements.</returns>
        public async Task<ObservableCollection<incidentHistory>> GetincidentHistorys()
        {
            try
            {

                IEnumerable<incidentHistory> items = await incidentHistoryTable
            .ToListAsync();

                return new ObservableCollection<incidentHistory>(items);
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
        public async Task AddincidentHistory(incidentHistory item)
        {
            try
            {
                if (item.Id == null)

                {
                    await incidentHistoryTable.InsertItemAsync(item);
                }
                else
                {
                    await incidentHistoryTable.ReplaceItemAsync(item);


                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }
    }
}
