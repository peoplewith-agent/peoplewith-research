
using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    public class meddirections : DatasyncClientData
    {
        string id;
        string? medicationid;
        bool active;
        string? type;
        string? supplementid;
        string? diagnosisid;
        string? weblink;
        string? filename;
        string? title;
        string? img;
        string? measurementid;
        string? symptomid;
        string? signup;

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
        /// MedicationID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "medicationid")]
        public string? Medicationid
        {
            get { return medicationid; }
            set { medicationid = value; }
        }

        /// <summary>
        /// MedicationID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "symptomid")]
        public string? Symptomid
        {
            get { return symptomid; }
            set { symptomid = value; }
        }

        /// <summary>
        /// UserID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "active")]
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }


        /// <summary>
        /// MedsTaken getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string? Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// TakenWithFood getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "supplementid")]
        public string? Supplementid
        {
            get { return supplementid; }
            set { supplementid = value; }
        }
        /// <summary>
        /// FoodID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "diagnosisid")]
        public string? Diagnosisid
        {
            get { return diagnosisid; }
            set { diagnosisid = value; }
        }

        /// <summary>
        /// FeedbackReminder getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "weblink")]
        public string? Weblink
        {
            get { return weblink; }
            set { weblink = value; }
        }

        /// <summary>
        /// ReminderInterval description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "filename")]
        public string? Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        /// <summary>
        /// DateTime description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string? Title
        {
            get { return title; }
            set { title = value; }
        }

        [JsonIgnore]
        public string? Img
        {
            get { return img; }
            set { img = value; }
        }

        [JsonProperty(PropertyName = "measurementid")]
        public string? Measurementid
        {
            get { return measurementid; }
            set { measurementid = value; }
        }

        [JsonProperty(PropertyName = "referralcode")]
        public string? Signupcode
        {
            get { return signup; }
            set { signup = value; }
        }




    }
}
