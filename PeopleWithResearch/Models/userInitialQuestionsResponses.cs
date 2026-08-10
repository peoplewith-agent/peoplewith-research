using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    public class userInitialQuestionsResponses : DatasyncClientData
    {
        string id;
        string userid;
        string questionid;
        string answerid;
        string answervalue;
        string dateComplete;
        string timeComplete;
        bool active;
        string notes;





        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "userid")]
        public string Userid
        {
            get { return userid; }
            set { userid = value; }
        }

        [JsonProperty(PropertyName = "questionid")]
        public string Questionid
        {
            get { return questionid; }
            set { questionid = value; }
        }

        [JsonProperty(PropertyName = "answerid")]
        public string Answerid
        {
            get { return answerid; }
            set { answerid = value; }
        }


        [JsonProperty(PropertyName = "answervalue")]
        public string Answervalue
        {
            get { return answervalue; }
            set { answervalue = value; }
        }

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

        [JsonProperty(PropertyName = "active")]
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }


        [JsonProperty(PropertyName = "notes")]
        public string Notes
        {
            get { return notes; }
            set { notes = value; }
        }
    }
}

