using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    public class UserQuestionnaire : DatasyncClientData
    {
        string id;
        string userid;
        string questionnaireid;
        string complete;
        bool isactive;
        string dateComplete;
        string timeComplete;
        string score;
        string dateandtime;
        string weeknumber;
        string imagename;
        string completestring;
        bool imagevis;
        string feedback;
        // string responseDate;



        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }


        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "userid")]
        public string Userid
        {
            get { return userid; }
            set { userid = value; }
        }

        [JsonProperty(PropertyName = "questionnaireid")]
        public string Questionnaireid
        {
            get { return questionnaireid; }
            set { questionnaireid = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "complete")]
        public string Complete
        {
            get { return complete; }
            set { complete = value; }
        }

        [JsonProperty(PropertyName = "isactive")]
        public bool Isactive
        {
            get { return isactive; }
            set { isactive = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "dateComplete")]
        public string DateComplete
        {
            get { return dateComplete; }
            set { dateComplete = value; }
        }



        [JsonProperty(PropertyName = "timeComplete")]
        public string TimeComplete
        {
            get { return timeComplete; }
            set { timeComplete = value; }
        }


        [JsonProperty(PropertyName = "score")]
        public string Score
        {
            get { return score; }
            set { score = value; }
        }

        [JsonIgnore]
        public string Dateandtime
        {
            get { return dateandtime; }
            set { dateandtime = value; }
        }

        [JsonIgnore]
        public string Weeknumber
        {
            get { return weeknumber; }
            set { weeknumber = value; }
        }

        [JsonIgnore]
        public string Imagename
        {
            get { return imagename; }
            set { imagename = value; }
        }

        [JsonIgnore]
        public string Completestring
        {
            get { return completestring; }
            set { completestring = value; }
        }

        [JsonIgnore]
        public bool Imagevis
        {
            get { return imagevis; }
            set { imagevis = value; }
        }

        [JsonProperty(PropertyName = "feedback")]
        public string Feedback
        {
            get { return feedback; }
            set { feedback = value; }
        }

        //[JsonProperty(PropertyName = "responseDate")]
        //public string ResponseDate
        //{
        //    get { return responseDate; }
        //    set { responseDate = value; }
        //}
    }
}
