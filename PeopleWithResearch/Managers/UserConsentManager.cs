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
    public class UserConsentManager
    {

        static UserConsentManager defaultInstance = new UserConsentManager();
        DatasyncClient client;
        IRemoteTable<userconsent> userconsentTable;
        public Func<Task<AuthenticationToken>> TokenRequestor;

        string userconsentid;

        private UserConsentManager()
        {

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
            };

            // Initialize the client.
            client = TokenRequestor == null
                ? new DatasyncClient(Constants.ApplicationURL, options)
                : new DatasyncClient(Constants.ApplicationURL, new GenericAuthenticationProvider(TokenRequestor), options);
            userconsentTable = client.GetRemoteTable<userconsent>();

        }


        public UserConsentManager(Func<Task<AuthenticationToken>> tokenRequestor)
        {
            TokenRequestor = tokenRequestor;
        }

        public static UserConsentManager DefaultManager => defaultInstance;

        public async Task adduserconsent(userconsent item)
        {

            try
            {

                await userconsentTable.InsertItemAsync(item);

               // userconsentid = item.Id;
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);
            }
        }

        //public async Task<ObservableCollection<userconsent>> checklatestid()
        //{
        //    try
        //    {
        //        IEnumerable<userconsent> items = await userconsentTable.Where(x => x.Id == userconsentid && x.Consented == true).ToListAsync();

        //        return new ObservableCollection<userconsent>(items);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(@"Sync error: {0}", e.Message);

        //    }
        //    return null;
        //}

        //public async Task<ObservableCollection<userconsent>> profileedit(string signup, string area)
        //{
        //    try
        //    {
        //        IEnumerable<userconsent> items = await userconsentTable.Where(x => x.Advertid == signup && x.Userid == Helpers.Settings.UserKey
        //        && x.Area == area && x.Consented == true).ToListAsync();

        //        return new ObservableCollection<userconsent>(items);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(@"Sync error: {0}", e.Message);

        //    }
        //    return null;
        //}

        //public async Task<ObservableCollection<userconsent>> checkuserconsent(string signup, string area)
        //{
        //    try
        //    {
        //        IEnumerable<userconsent> items = await userconsentTable.Where(x => x.Advertid == signup && x.Userid == Helpers.Settings.UserKey
        //        && x.Area == area && x.Consented == true).ToListAsync();

        //        return new ObservableCollection<userconsent>(items);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(@"Sync error: {0}", e.Message);

        //    }
        //    return null;
        //}

    }

}