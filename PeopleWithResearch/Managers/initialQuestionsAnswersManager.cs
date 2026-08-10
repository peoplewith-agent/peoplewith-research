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
    public class initialQuestionsAnswersManager
    {

        static initialQuestionsAnswersManager defaultInstance = new initialQuestionsAnswersManager();
        DatasyncClient client;
        IRemoteTable<initialQuestionsAnswers> initialQuestionsAnswersTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private initialQuestionsAnswersManager()
        {

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            initialQuestionsAnswersTable = client.GetRemoteTable<initialQuestionsAnswers>();

        }

        public initialQuestionsAnswersManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static initialQuestionsAnswersManager DefaultManager => defaultInstance;



        /// <summary>
        /// Returns an observable collection of the initialQuestions from the database
        /// </summary>
        /// <returns>The initialQuestions</returns>
        public async Task<ObservableCollection<initialQuestionsAnswers>> getinitialAnswers(string questionid)
        {
            try
            {

                IEnumerable<initialQuestionsAnswers> items = await initialQuestionsAnswersTable.Where(x => x.Questionid == questionid)
                    .ToListAsync();

                return new ObservableCollection<initialQuestionsAnswers>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

    }

}