using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
	public class incidentHistory : DatasyncClientData
	{


        string id;
        string? incidentid;
        string? textnotes;
        string? userid;
        string? username;


        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "incidentid")]
        public string? Incidentid
        {
            get { return incidentid; }
            set { incidentid = value; }
        }

        [JsonProperty(PropertyName = "textnotes")]
        public string? Textnotes
        {
            get { return textnotes; }
            set { textnotes = value; }
        }


        [JsonProperty(PropertyName = "userid")]
        public string? Userid
        {
            get { return userid; }
            set { userid = value; }
        }

      

        [JsonProperty(PropertyName = "username")]
        public string? Username
        {
            get { return username; }
            set { username = value; }
        }



    }
}

