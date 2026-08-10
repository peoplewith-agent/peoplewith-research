using System;
using Newtonsoft.Json;
namespace PeopleWithResearch
{
	public class Customheightandweight
	{
        string id;
        string valuenumber;
        bool mainnumber;
        bool grayvisible;

        [JsonIgnore]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonIgnore]
        public string Valuenumber
        {
            get { return valuenumber; }
            set { valuenumber = value; }
        }


        [JsonIgnore]
        public bool Mainnumber
        {
            get { return mainnumber; }
            set { mainnumber = value; }
        }

        [JsonIgnore]
        public bool Grayvisible
        {
            get { return grayvisible; }
            set { grayvisible = value; }
        }
    }
}

