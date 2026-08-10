using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch    
{
    public class Answers : DatasyncClientData
    {
        string id;
        string? answervalue;
        string? label;
        string? questionid;
        int order;
        bool isactive;
        string? notes;


        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string? Id
        {
            get { return id; }
            set { id = value; }
        }


        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "answervalue")]
        public string? Answervalue
        {
            get { return answervalue; }
            set { answervalue = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "label")]
        public string? Label
        {
            get { return label; }
            set { label = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "questionid")]
        public string? Questionid
        {
            get { return questionid; }
            set { questionid = value; }
        }


        [JsonProperty(PropertyName = "order")]
        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        [JsonProperty(PropertyName = "isactive")]
        public bool Isactive
        {
            get { return isactive; }
            set { isactive = value; }
        }


        [JsonProperty(PropertyName = "notes")]
        public string? Notes
        {
            get { return notes; }
            set { notes = value; }
        }




    }
}
