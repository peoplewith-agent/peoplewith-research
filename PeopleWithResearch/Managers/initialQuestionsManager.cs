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
    public class initialQuestionsManager
    {

        static initialQuestionsManager defaultInstance = new initialQuestionsManager();
        DatasyncClient client;
        IRemoteTable<initialQuestions> initialQuestionsTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private initialQuestionsManager()
        {

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            initialQuestionsTable = client.GetRemoteTable<initialQuestions>();
        }

        public initialQuestionsManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static initialQuestionsManager DefaultManager => defaultInstance;


        /// <summary>
        /// Returns an observable collection of the initialQuestions from the database
        /// </summary>
        /// <returns>The initialQuestions</returns>
        public async Task<ObservableCollection<initialQuestions>> getinitialQuestions(string advertid)
        {
            try
            {

                IEnumerable<initialQuestions> items = await initialQuestionsTable.Where(x => x.Advertid == advertid)
                    .ToListAsync();

                return new ObservableCollection<initialQuestions>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

    }

}