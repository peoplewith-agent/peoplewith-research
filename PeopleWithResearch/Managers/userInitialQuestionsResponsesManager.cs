using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Datasync.Client;


namespace PeopleWithResearch
{
    public class userInitialQuestionsResponsesManager
    {

        static userInitialQuestionsResponsesManager defaultInstance = new userInitialQuestionsResponsesManager();
        DatasyncClient client;
        IRemoteTable<userInitialQuestionsResponses> userInitialQuestionsResponsesTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private userInitialQuestionsResponsesManager()
        {
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            userInitialQuestionsResponsesTable = client.GetRemoteTable<userInitialQuestionsResponses>();
        }
        public userInitialQuestionsResponsesManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static userInitialQuestionsResponsesManager DefaultManager => defaultInstance;

        public async Task<ObservableCollection<userInitialQuestionsResponses>> getinitialQuestions()
        {
            try
            {

                IEnumerable<userInitialQuestionsResponses> items = await userInitialQuestionsResponsesTable
                    .ToListAsync();

                return new ObservableCollection<userInitialQuestionsResponses>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task Addresponse(userInitialQuestionsResponses item)
        {

            try
            {
                //if ID is null, it is a new record.
                if (item.Id == null)
                {
                    await userInitialQuestionsResponsesTable.InsertItemAsync(item);

                }

                //otherwise update existing record
                else
                {
                    await userInitialQuestionsResponsesTable.ReplaceItemAsync(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

    }

}