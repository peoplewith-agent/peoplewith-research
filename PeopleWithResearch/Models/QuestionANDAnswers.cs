using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    public class QuestionANDAnswers : DatasyncClientData
    {
        string answerid;
        string answervalue;
        string label;
        string questionid;
        string order;
        bool isactive;
        string answernotes;
        string question;
        string directions;
        string type;
        string questionnaireid;
        string category;
        string questionnotes;
        int questionorder;


        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "answerid")]
        public string Answerid
        {
            get { return answerid; }
            set { answerid = value; }
        }


        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "answervalue")]
        public string Answervalue
        {
            get { return answervalue; }
            set { answervalue = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "label")]
        public string Label
        {
            get { return label; }
            set { label = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "questionid")]
        public string Questionid
        {
            get { return questionid; }
            set { questionid = value; }
        }


        [JsonProperty(PropertyName = "order")]
        public string Order
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


        [JsonProperty(PropertyName = "answernotes")]
        public string Answernotes
        {
            get { return answernotes; }
            set { answernotes = value; }

        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "question")]
        public string QuestionName
        {
            get { return question; }
            set { question = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "directions")]
        public string Directions
        {
            get { return directions; }
            set { directions = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get { return type; }
            set { type = value; }
        }


        [JsonProperty(PropertyName = "questionnaireid")]
        public string Questionnaireid
        {
            get { return questionnaireid; }
            set { questionnaireid = value; }
        }



        [JsonProperty(PropertyName = "category")]
        public string Category
        {
            get { return category; }
            set { category = value; }
        }


        [JsonProperty(PropertyName = "questionnotes")]
        public string Questionnotes
        {
            get { return questionnotes; }
            set { questionnotes = value; }
        }

        [JsonProperty(PropertyName = "questionorder")]
        public int Questionorder
        {
            get { return questionorder; }
            set { questionorder = value; }
        }
    }
}
