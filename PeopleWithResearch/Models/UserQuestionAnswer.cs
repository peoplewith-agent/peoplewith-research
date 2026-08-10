using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    public class UserQuestionAnswer : DatasyncClientData
    {
        string id;
        string userid;
        string questionid;
        string answerid;
        bool isactive;
        string notes;
        string dateComplete;
        string timeComplete;
        string userquestionnaireid;
        string score;
        string responseDate;



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

        [JsonProperty(PropertyName = "isactive")]
        public bool Isactive
        {
            get { return isactive; }
            set { isactive = value; }
        }


        [JsonProperty(PropertyName = "notes")]
        public string Notes
        {
            get { return notes; }
            set { notes = value; }
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



        [JsonProperty(PropertyName = "userquestionnaireid")]
        public string Userquestionnaireid
        {
            get { return userquestionnaireid; }
            set { userquestionnaireid = value; }
        }


        [JsonProperty(PropertyName = "score")]
        public string Score
        {
            get { return score; }
            set { score = value; }
        }

        [JsonProperty(PropertyName = "responseDate")]
        public string ResponseDate
        {
            get { return responseDate; }
            set { responseDate = value; }
        }
    }
}
