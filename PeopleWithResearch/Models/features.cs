using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
	public class features : DatasyncClientData
	{

        string id;
        bool deleted;
        string? advertid;
        string? featurename;
        string? order;
        bool isrequired;
        string? featurestackname;

        
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

        [JsonProperty(PropertyName = "featurename")]
        public string? Featurename
        {
            get { return featurename; }
            set { featurename = value; }
        }

        [JsonProperty(PropertyName = "order")]
        public string? Order
        {
            get { return order; }
            set { order = value; }
        }

        [JsonProperty(PropertyName = "isrequired")]
        public bool Isrequired
        {
            get { return isrequired; }
            set { isrequired = value; }
        }

        [JsonProperty(PropertyName = "featurestackname")]
        public string? Featurestackname
        {
            get { return featurestackname; }
            set { featurestackname = value; }
        }


    }
}

