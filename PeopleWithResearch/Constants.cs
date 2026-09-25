using System;
using System.Net;

namespace PeopleWithResearch
{
    public class Constants
    {

        // public static string ApplicationURL = @"http://peoplewithmob.azurewebsites.net/"; //App Service URL
        //    public static string ApplicationURL = @"https://peoplewithdevelopment.azurewebsites.net/"; //App Service URL "; //App Service URL
        //public static string ApplicationURL = @"https://pwdev2024.azurewebsites.net";

        public static string ClientPrincipal = "eyAgCiAgImlkZW50aXR5UHJvdmlkZXIiOiAidGVzdCIsCiAgInVzZXJJZCI6ICIxMjM0NSIsCiAgInVzZXJEZXRhaWxzIjogImpvaG5AY29udG9zby5jb20iLAogICJ1c2VyUm9sZXMiOiBbIjFFMzNDMEFDLTMzOTMtNEMzNC04MzRBLURFNUZEQkNCQjNDQyJdCn0=";
        public static string ApiRole = "1E33C0AC-3393-4C34-834A-DE5FDBCBB3CC";

       // public static string ApplicationURL = @"https://peoplewith-research-maui-2024.azurewebsites.net";
        public static string ApplicationURL = @"https://peoplewithresearch-sqlserver.database.windows.net";

        //Dev
        // public const string ListenConnectionString = "Endpoint=sb://PeopleWithResearch.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=2F2G+P+4U3y0kA9mm32VitVkjlc6w5yP6VI69p2C+z4=";
        // public const string NotificationHubName = "PWRessearchDev";

        //Production 
        public const string ListenConnectionString = "Endpoint=sb://PeopleWithResearchNotifcations.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=nXmDTTvexWnioeLhjXNP5BG/wAWblKdInnrpIVFNtZA=";
        public const string NotificationHubName = "PWResearch";
         
        //public const string ListenConnectionString = "Endpoint=sb://PeopleWithResearch.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=2F2G+P+4U3y0kA9mm32VitVkjlc6w5yP6VI69p2C+z4=";
        //public const string NotificationHubName = "PWRessearchDev";
        //  public const string FullAccessConnectionString = "Endpoint=sb://PeopleWithResearch.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=4BwTTPYq2xCjgJrsKoBpl8HkZ+QlM2ptazc/LdZiYXE=";

    }

}

