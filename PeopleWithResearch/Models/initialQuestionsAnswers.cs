using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    public class initialQuestionsAnswers : DatasyncClientData
    {
        string id;
        string? answervalue;
        string? label;
        string? questionid;
        int order;
        bool active;
        string? notes;

        string? splitlabel1;
        string? splitlabel2;



        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "answervalue")]
        public string? Answervalue
        {
            get { return answervalue; }
            set { answervalue = value; }
        }

        [JsonProperty(PropertyName = "label")]
        public string? Label
        {
            get { return label; }
            set { label = value; }
        }

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

        [JsonProperty(PropertyName = "active")]
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }


        [JsonProperty(PropertyName = "notes")]
        public string? Notes
        {
            get { return notes; }
            set { notes = value; }
        }

        [JsonIgnore]
        public string? Splitlabel1
        {
            get { return splitlabel1; }
            set { splitlabel1 = value; }
        }

        [JsonIgnore]
        public string? Splitlabel2
        {
            get { return splitlabel2; }
            set { splitlabel2 = value; }
        }
    }
}

