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
    public class UserQuestionAnswerManager
    {

        static UserQuestionAnswerManager defaultInstance = new UserQuestionAnswerManager();
        DatasyncClient client;
        IRemoteTable<UserQuestionAnswer> UserQuestionAnswertable;
        public Func<Task<AuthenticationToken>> TokenRequestor;


        private UserQuestionAnswerManager()
        {
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            UserQuestionAnswertable = client.GetRemoteTable<UserQuestionAnswer>();
        }
        public UserQuestionAnswerManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static UserQuestionAnswerManager DefaultManager => defaultInstance;

        public async Task<ObservableCollection<UserQuestionAnswer>> getUserAnswers(string userquestionnaireid)
        {
            try
            {

                IEnumerable<UserQuestionAnswer> items = await UserQuestionAnswertable.Where(Item => Item.Userquestionnaireid == userquestionnaireid)
                    .ToListAsync();

                return new ObservableCollection<UserQuestionAnswer>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }



        public async Task<ObservableCollection<UserQuestionAnswer>> getUserAnswersByUserID()
        {
            try
            {

                IEnumerable<UserQuestionAnswer> items = await UserQuestionAnswertable.Where(Item => Item.Userid == Helpers.Settings.UserKey)
                    .ToListAsync();

                return new ObservableCollection<UserQuestionAnswer>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task AddUserQuestionAnswer(UserQuestionAnswer item)
        {

            try
            {
                //if ID is null, it is a new record.
                if (item.Id == null)
                {
                    await UserQuestionAnswertable.InsertItemAsync(item);

                }

                //otherwise update existing record
                else
                {
                    await UserQuestionAnswertable.ReplaceItemAsync(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        public async Task DeleteReason(UserQuestionAnswer item)
        {

            try
            {
                await UserQuestionAnswertable.DeleteItemAsync(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


        }
    }
}