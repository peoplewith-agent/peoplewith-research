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
    public class IncidentsManager
    {
        static IncidentsManager defaultInstance = new IncidentsManager();
        DatasyncClient client;
        IRemoteTable<incidents> incidentsTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;
    
        private IncidentsManager()
        {

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            incidentsTable = client.GetRemoteTable<incidents>();

        }

        public IncidentsManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static IncidentsManager DefaultManager => defaultInstance;


        /// <summary>
        /// Returns an observable collection of Announcements from the Database
        /// </summary>
        /// <returns>The Announcements.</returns>
        public async Task<ObservableCollection<incidents>> Getincidentss()
        {
            try
            {

                IEnumerable<incidents> items = await incidentsTable
            .ToListAsync();

                return new ObservableCollection<incidents>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<incidents>> Getincidentsforuser()
        {
            try
            {

                IEnumerable<incidents> items = await incidentsTable.Where(x => x.Userid == Helpers.Settings.UserKey)
            .ToListAsync();

                return new ObservableCollection<incidents>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<incidents>> GetSpecficIncident(string weeknumber)
        {
            try
            {

                IEnumerable<incidents> items = await incidentsTable.Where(Item => Item.Userid == Helpers.Settings.UserKey).Where(x => x.Week == weeknumber)
            .ToListAsync();

                return new ObservableCollection<incidents>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<incidents>> GetSpecficIncidentbyuserquestionnaireid(string userquestionnaireid)
        {
            try
            {

                IEnumerable<incidents> items = await incidentsTable.Where(Item => Item.Userid == Helpers.Settings.UserKey).Where(x => x.Userquestionnaireid == userquestionnaireid)
            .ToListAsync();

                return new ObservableCollection<incidents>(items);
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
        public async Task Addincidents(incidents item)
        {
            try
            {
                if (item.Id == null)

                {
                    await incidentsTable.InsertItemAsync(item);
                }
                else
                {
                    await incidentsTable.ReplaceItemAsync(item);


                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        public async Task DeleteIncident(incidents item)
        {
            try
            {
                await incidentsTable.DeleteItemAsync(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
