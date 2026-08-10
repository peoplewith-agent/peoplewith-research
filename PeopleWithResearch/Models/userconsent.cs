using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;
namespace PeopleWithResearch
{
    public class userconsent 
    {

        [System.Text.Json.Serialization.JsonIgnore]
        public string userconsentid { get; set; }
 
        public string userid { get; set; }
        public string consentid { get; set; }
        public string signaturefilename { get; set; }

        public string consentinput { get; set; }

        public string consentselection { get; set; }

        public string additionaldetails { get; set; }

    }
}
