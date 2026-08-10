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
    public class PrimaryCareManager
    {

        string newestid;

        static PrimaryCareManager defaultInstance = new PrimaryCareManager();
        DatasyncClient client;
        IRemoteTable<primarycare> primarycaretable;
        public Func<Task<AuthenticationToken>> TokenRequestor;


        private PrimaryCareManager()
        {
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            primarycaretable = client.GetRemoteTable<primarycare>();
        }

        public PrimaryCareManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static PrimaryCareManager DefaultManager => defaultInstance;
        public async Task<ObservableCollection<primarycare>> GetDoctorListEMIS(string advertpassed)
        {
            try
            {

                IEnumerable<primarycare> items = await primarycaretable.Where(x => x.Advertid == advertpassed)
                    .ToListAsync();

                return new ObservableCollection<primarycare>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }


        public async Task<ObservableCollection<primarycare>> GetDoctorList()
        {
            try
            {

                IEnumerable<primarycare> items = await primarycaretable
                    .ToListAsync();

                return new ObservableCollection<primarycare>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<primarycare>> GetDoctorListbasedonGPID()
        {
            try
            {

                IEnumerable<primarycare> items = await primarycaretable.Where(x => x.Pracno == Helpers.Settings.Usergpid)
                    .ToListAsync();

                return new ObservableCollection<primarycare>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }



        public async Task updaterequest(primarycare item)
        {

            try
            {
                await primarycaretable.ReplaceItemAsync(item);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

    }

}