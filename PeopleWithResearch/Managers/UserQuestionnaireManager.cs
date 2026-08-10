using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Datasync.Client;

namespace PeopleWithResearch
{
    public class UserQuestionnaireManager
    {

        static UserQuestionnaireManager defaultInstance = new UserQuestionnaireManager();
        DatasyncClient client;
        IRemoteTable<UserQuestionnaire> UserQuestionnairetable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private UserQuestionnaireManager()
        {


            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            UserQuestionnairetable = client.GetRemoteTable<UserQuestionnaire>();

        }


        public UserQuestionnaireManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static UserQuestionnaireManager DefaultManager => defaultInstance;
        public async Task<ObservableCollection<UserQuestionnaire>> getUserQuestionnaire()
        {
            try
            {

                IEnumerable<UserQuestionnaire> items = await UserQuestionnairetable.Where(Item => Item.Userid == Helpers.Settings.UserKey)
                    .ToListAsync();

                return new ObservableCollection<UserQuestionnaire>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task AddUserQuestionnaire(UserQuestionnaire item)
        {

            try
            {
                //if ID is null, it is a new record.
                if (item.Id == null)
                {
                    await UserQuestionnairetable.InsertItemAsync(item);

                }

                //otherwise update existing record
                else
                {
                    await UserQuestionnairetable.ReplaceItemAsync(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }


        public async Task DeleteReason(UserQuestionnaire item)
        {

            try
            {
                await UserQuestionnairetable.DeleteItemAsync(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


        }
    }
}