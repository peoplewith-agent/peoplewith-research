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
    public class ConsentManager
    {
        static ConsentManager defaultInstance = new ConsentManager();
        DatasyncClient client;
        IRemoteTable<consent> consentTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private ConsentManager()
        {
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            consentTable = client.GetRemoteTable<consent>();


        }


        /// <summary>
        /// Default Trigger Manager Constructor
        /// </summary>
        /// <value>The default manager.</value>
        public ConsentManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static ConsentManager DefaultManager => defaultInstance;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>consent table data based on sign up</returns>
		public async Task<ObservableCollection<consent>> getconsent(string signup)
        {
            try
            {

                IEnumerable<consent> items = await consentTable.Where(x => x.Advertid == signup.ToUpperInvariant()).ToListAsync();

                return new ObservableCollection<consent>(items);
            }

            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<consent>> getconsentreg(string signup)
        {
            try
            {

                IEnumerable<consent> items = await consentTable.Where(x => x.Advertid == signup.ToUpperInvariant()).Where(x => x.AdditionalConsent == false).ToListAsync();

                return new ObservableCollection<consent>(items);
            }

            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<consent>> getconsentregover18(string signup)
        {
            try
            {

                IEnumerable<consent> items = await consentTable.Where(x => x.Advertid == signup.ToUpperInvariant()).Where(x => x.Area == "o18").ToListAsync();

                return new ObservableCollection<consent>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<consent>> getconsentregunder18(string signup)
        {
            try
            {

                IEnumerable<consent> items = await consentTable.Where(x => x.Advertid == signup.ToUpperInvariant()).Where(x => x.Area == "u18").ToListAsync();

                return new ObservableCollection<consent>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }


        public async Task<ObservableCollection<consent>> GetSpecficAd(string advertid)
        {
            try
            {

                IEnumerable<consent> items = await consentTable.Where(Item => Item.Advertid == advertid && Item.AdditionalConsent == true)
            .ToListAsync();

                return new ObservableCollection<consent>(items);
            }

            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }



    }

}