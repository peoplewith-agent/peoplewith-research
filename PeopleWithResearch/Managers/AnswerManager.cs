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
    public class AnswerManager
    {

        static AnswerManager defaultInstance = new AnswerManager();
        DatasyncClient client;
        IRemoteTable<Answers> Answerstable;
        public Func<Task<AuthenticationToken>> TokenRequestor;


        private AnswerManager()
        {

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            Answerstable = client.GetRemoteTable<Answers>();
        }
    
        public AnswerManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        /// <summary>
        /// Defaul ReasonManager Constructor
        /// </summary>
        /// <value>The default manager.</value>


        public static AnswerManager DefaultManager => defaultInstance;


        /// <summary>
        /// Checks if offline app service hs been enabled
        /// </summary>
        /// <value><c>true</c> if is offline enabled; otherwise, <c>false</c>.</value>
 

        /// <summary>
        /// Returns an observable collection of medicines from the database
        /// </summary>
        /// <returns>The users.</returns>
        public async Task<ObservableCollection<Answers>> getAnswerss(string Questionid)
        {
            try
            {

                IEnumerable<Answers> items = await Answerstable.Where(Item => Item.Questionid == Questionid).ToListAsync();

                return new ObservableCollection<Answers>(items);
            }
            //catch (MobileServiceInvalidOperationException msioe)
            //{
            //    Debug.WriteLine(@"Invalid sync operation: {0}", msioe.Message);
            //}
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<Answers>> getAnswerforincident(string answerid)
        {
            try
            {

                IEnumerable<Answers> items = await Answerstable.Where(Item => Item.Id == answerid)
                    .ToListAsync();

                return new ObservableCollection<Answers>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<Answers>> getAllAnswerss()
        {
            try
            {

                IEnumerable<Answers> items = await Answerstable
                    .ToListAsync();

                return new ObservableCollection<Answers>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }


        /// <summary>
        /// Adds or Updates Answerss in system
        /// </summary>
        /// <returns>The user.</returns>
        /// <param name="item">Item.</param>
        public async Task AddReason(Answers item)
        {

            try
            {
                //if ID is null, it is a new record.
                if (item.Id == null)
                {
                    await Answerstable.InsertItemAsync(item);

                }

                //otherwise update existing record
                else
                {
                    await Answerstable.ReplaceItemAsync(item);
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
        public async Task DeleteReason(Answers item)
        {

            try
            {
                await Answerstable.DeleteItemAsync(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


        }
    }
}