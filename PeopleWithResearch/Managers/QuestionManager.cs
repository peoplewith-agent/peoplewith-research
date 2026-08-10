using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Datasync.Client;

namespace PeopleWithResearch
{
    public class QuestionManager
    {
        static QuestionManager defaultInstance = new QuestionManager();
        DatasyncClient client;
        IRemoteTable<Question> Questionstable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private QuestionManager()
        {

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            Questionstable = client.GetRemoteTable<Question>();

        }

        public QuestionManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static QuestionManager DefaultManager => defaultInstance;
        public async Task<ObservableCollection<Question>> getQuestions(string questionnaireid)
        {
            try
            {

                IEnumerable<Question> items = await Questionstable.Where(Item => Item.Questionnaireid == questionnaireid).Where(item => item.Isactive == true)
                    .ToListAsync();

                return new ObservableCollection<Question>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<Question>> getQuestionbysignupcode(string signupcode)
        {
            try
            {

                IEnumerable<Question> items = await Questionstable.Where(Item => Item.Signupcode == signupcode).Where(x => x.Area == "Notification")
                    .ToListAsync();

                return new ObservableCollection<Question>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<Question>> getQuestionsbyarea(string area)
        {
            try
            {

                IEnumerable<Question> items = await Questionstable.Where(Item => Item.Area == area)
                    .ToListAsync();

                return new ObservableCollection<Question>(items);
            }

            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }


        public async Task AddReason(Question item)
        {

            try
            {
                //if ID is null, it is a new record.
                if (item.Id == null)
                {
                    await Questionstable.InsertItemAsync(item);

                }

                //otherwise update existing record
                else
                {
                    await Questionstable.ReplaceItemAsync(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        /// <summary>
        /// Deletes a Reason from the Database
        /// </summary>
        /// <returns>The reason.</returns>
        /// <param name="item">Item.</param>
        public async Task DeleteReason(Question item)
        {

            try
            {
                await Questionstable.DeleteItemAsync(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


        }
    }
}