using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;
namespace PeopleWithResearch
{
    public class consent : DatasyncClientData
    {

        string id;
        bool deleted;
        string? advertid;
        string? consentTitle;
        string? consentSubTitle;
        string? consentContent;
        string? consentQuestion1;
        string? consentAnswer1;
        string? consentQuestion2;
        string? consentAnswer2;
        string? consentQuestion3;
        string? consentAnswer3;
        string? consentOption1;
        string? consentOption2;
        string? area;
        DateTime createdAt;
        bool additionalConsent;

        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "deleted")]
        public bool Deleted
        {
            get { return deleted; }
            set { deleted = value; }
        }

        [JsonProperty(PropertyName = "advertid")]
        public string? Advertid
        {
            get { return advertid; }
            set { advertid = value; }
        }

        [JsonProperty(PropertyName = "consentTitle")]
        public string? ConsentTitle
        {
            get { return consentTitle; }
            set { consentTitle = value; }
        }

        [JsonProperty(PropertyName = "consentSubTitle")]
        public string? ConsentSubTitle
        {
            get { return consentSubTitle; }
            set { consentSubTitle = value; }
        }

        [JsonProperty(PropertyName = "consentContent")]
        public string? ConsentContent
        {
            get { return consentContent; }
            set { consentContent = value; }
        }

        [JsonProperty(PropertyName = "consentQuestion1")]
        public string? ConsentQuestion1
        {
            get
            {
                return consentQuestion1;
            }
            set { consentQuestion1 = value; }
        }

        [JsonProperty(PropertyName = "consentAnswer1")]
        public string? ConsentAnswer1
        {
            get
            {
                return consentAnswer1;
            }
            set { consentAnswer1 = value; }
        }

        [JsonProperty(PropertyName = "consentQuestion2")]
        public string? ConsentQuestion2
        {
            get
            {
                return consentQuestion2;
            }
            set { consentQuestion2 = value; }
        }

        [JsonProperty(PropertyName = "consentAnswer2")]
        public string? ConsentAnswer2
        {
            get
            {
                return consentAnswer2;
            }
            set { consentAnswer2 = value; }
        }

        [JsonProperty(PropertyName = "consentQuestion3")]
        public string? ConsentQuestion3
        {
            get
            {
                return consentQuestion3;
            }
            set { consentQuestion3 = value; }
        }



        [JsonProperty(PropertyName = "consentAnswer3")]
        public string? ConsentAnswer3
        {
            get
            {
                return consentAnswer3;
            }
            set { consentAnswer3 = value; }
        }


        [JsonProperty(PropertyName = "consentOption1")]
        public string? ConsentOption1
        {
            get
            {
                return consentOption1;
            }
            set { consentOption1 = value; }
        }

        [JsonProperty(PropertyName = "consentOption2")]
        public string? ConsentOption2
        {
            get
            {
                return consentOption2;
            }
            set { consentOption2 = value; }
        }

        [JsonProperty(PropertyName = "area")]
        public string? Area
        {
            get
            {
                return area;
            }
            set { area = value; }
        }

        [JsonProperty(PropertyName = "additionalConsent")]
        public bool AdditionalConsent
        {
            get { return additionalConsent; }
            set { additionalConsent = value; }
        }






    }
}
