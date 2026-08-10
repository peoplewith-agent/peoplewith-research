using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    public class privacyPolicy : DatasyncClientData
    {

        string id;
        string? title;
        string? content;
        string? advertID;

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "title")]
        public string? Title
        {
            get { return title; }
            set { title = value; }
        }

        [JsonProperty(PropertyName = "content")]
        public string? Content
        {
            get { return content; }
            set { content = value; }
        }

        [JsonProperty(PropertyName = "advertID")]
        public string? AdvertID
        {
            get { return advertID; }
            set { advertID = value; }
        }



    }
}
