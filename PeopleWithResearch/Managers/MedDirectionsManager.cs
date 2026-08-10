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
    /// <summary>
    /// Handles all operations with user/medicine objects
    /// </summary>
    public class MedDirectionsManager
    {

        static MedDirectionsManager defaultInstance = new MedDirectionsManager();
        DatasyncClient client;
        IRemoteTable<meddirections> meddirectionssTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        private MedDirectionsManager()
        {

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            meddirectionssTable = client.GetRemoteTable<meddirections>();

        }

        public MedDirectionsManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static MedDirectionsManager DefaultManager => defaultInstance;
        public async Task<ObservableCollection<meddirections>> getmeddirectionslist()
        {
            try
            {

                IEnumerable<meddirections> items = await meddirectionssTable
            .ToListAsync();

                return new ObservableCollection<meddirections>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<meddirections>> getmeddirectionslistmed(string medid)
        {
            try
            {

                IEnumerable<meddirections> items = await meddirectionssTable.Where(x => x.Medicationid == medid)
            .ToListAsync();

                return new ObservableCollection<meddirections>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<meddirections>> getmeddirectionslistsupp(string suppid)
        {
            try
            {

                IEnumerable<meddirections> items = await meddirectionssTable.Where(x => x.Supplementid == suppid)
            .ToListAsync();

                return new ObservableCollection<meddirections>(items);
            }

            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<meddirections>> getmeddirectionslistmeasurement(string measurementid)
        {
            try
            {

                IEnumerable<meddirections> items = await meddirectionssTable.Where(x => x.Measurementid == measurementid)
            .ToListAsync();

                return new ObservableCollection<meddirections>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }

        public async Task<ObservableCollection<meddirections>> getmeddirectionslistdiag(string diagid)
        {
            try
            {

                IEnumerable<meddirections> items = await meddirectionssTable.Where(x => x.Diagnosisid == diagid)
            .ToListAsync();

                return new ObservableCollection<meddirections>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);

            }
            return null;
        }


        //added for symptom information
        public async Task<ObservableCollection<meddirections>> getmeddirectionslistsymptom(string symptomid)
        {
            try
            {
                IEnumerable<meddirections> items = await meddirectionssTable.Where(x => x.Symptomid == symptomid)
            .ToListAsync();

                return new ObservableCollection<meddirections>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);
            }
            return null;
        }

        public async Task<ObservableCollection<meddirections>> getinfobyreferralcode(string signupid)
        {
            try
            {
                IEnumerable<meddirections> items = await meddirectionssTable.Where(x => x.Signupcode == Helpers.Settings.SignUp)
            .ToListAsync();

                return new ObservableCollection<meddirections>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);
            }
            return null;
        }

        public async Task<ObservableCollection<meddirections>> getinfobyreferralcodeonreg(string signupid)
        {
            try
            {
                IEnumerable<meddirections> items = await meddirectionssTable.Where(x => x.Signupcode == signupid)
            .ToListAsync();

                return new ObservableCollection<meddirections>(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);
            }
            return null;
        }
    }
}

