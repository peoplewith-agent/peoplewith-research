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
    public class QuestionnaireManager
    {

        static QuestionnaireManager defaultInstance = new QuestionnaireManager();
        DatasyncClient client;
        IRemoteTable<Questionnaire> Questionnairetable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private QuestionnaireManager()
        {
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            Questionnairetable = client.GetRemoteTable<Questionnaire>();
        }

        public QuestionnaireManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static QuestionnaireManager DefaultManager => defaultInstance;


        public async Task<ObservableCollection<Questionnaire>> getQuestionnaire()
        {
            try
            {

                IEnumerable<Questionnaire> items = await Questionnairetable.ToListAsync();
                return new ObservableCollection<Questionnaire>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }


        public async Task<ObservableCollection<Questionnaire>> getQuestionnaireSingle(string questionnaireid)
        {
            try
            {

                IEnumerable<Questionnaire> items = await Questionnairetable.Where(Item => Item.Id == questionnaireid).ToListAsync();
                return new ObservableCollection<Questionnaire>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<Questionnaire>> getQuestionnairebySignupcode(string signupcode)
        {
            try
            {

                IEnumerable<Questionnaire> items = await Questionnairetable.Where(Item => Item.Advertid == signupcode).ToListAsync();
                return new ObservableCollection<Questionnaire>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task AddReason(Questionnaire item)
        {

            try
            {
                //if ID is null, it is a new record.
                if (item.Id == null)
                {
                    await Questionnairetable.InsertItemAsync(item);

                }

                //otherwise update existing record
                else
                {
                    await Questionnairetable.ReplaceItemAsync(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

 
        public async Task DeleteReason(Questionnaire item)
        {

            try
            {
                await Questionnairetable.DeleteItemAsync(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


        }
    }
}