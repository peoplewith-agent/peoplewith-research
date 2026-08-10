using System;
using System.Collections.ObjectModel;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    public class Questionnaire : DatasyncClientData
    {
        string id;
        string title;
        string description;
        bool isactive;
        string notes;
        string author;
        string diagnosisid;
        string medicationid;
        string symptomid;
        string measurementid;
        string supplementid;
        string advertid;
        string gender;
        string ethnicity;
        string age;

        bool inprogress;
        string progressnum;
        bool start;
        ObservableCollection<Questionnaire> detaillist;
        int questioncount;
        int responsecount;
        string info;
        string consent1;
        string consent2;
        string consent3;
        string consent4;

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
        [JsonProperty(PropertyName = "title")]
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Reason description getter/setter
        /// </summary>
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

        [JsonProperty(PropertyName = "author")]
        public string Author
        {
            get { return author; }
            set { author = value; }
        }


        [JsonProperty(PropertyName = "diagnosisid")]
        public string Diagnosisid
        {
            get { return diagnosisid; }
            set { diagnosisid = value; }
        }


        [JsonProperty(PropertyName = "medicationid")]
        public string Medicationid
        {
            get { return medicationid; }
            set { medicationid = value; }
        }


        [JsonProperty(PropertyName = "symptomid")]
        public string Symptomid
        {
            get { return symptomid; }
            set { symptomid = value; }
        }

        [JsonProperty(PropertyName = "measurementid")]
        public string Measurementid
        {
            get { return measurementid; }
            set { measurementid = value; }
        }

        [JsonProperty(PropertyName = "supplementid")]
        public string Supplementid
        {
            get { return supplementid; }
            set { supplementid = value; }
        }

        [JsonProperty(PropertyName = "advertid")]
        public string Advertid
        {
            get { return advertid; }
            set { advertid = value; }
        }

        [JsonProperty(PropertyName = "gender")]
        public string Gender
        {
            get { return gender; }
            set { gender = value; }
        }

        [JsonProperty(PropertyName = "ethnicity")]
        public string Ethnicity
        {
            get { return ethnicity; }
            set { ethnicity = value; }
        }

        [JsonProperty(PropertyName = "age")]
        public string Age
        {
            get { return age; }
            set { age = value; }
        }

        [JsonIgnore]
        public bool Inprogress
        {
            get { return inprogress; }
            set { inprogress = value; }
        }

        [JsonIgnore]
        public string Progressnum
        {
            get { return progressnum; }
            set { progressnum = value; }
        }


        [JsonIgnore]
        public bool Start
        {
            get { return start; }
            set { start = value; }
        }

        public ObservableCollection<Questionnaire> Detaillist
        {
            get { return detaillist; }
            set
            {
                detaillist = value;

            }
        }

        [JsonIgnore]
        public int Questioncount
        {
            get { return questioncount; }
            set { questioncount = value; }
        }

        [JsonIgnore]
        public int Responsecount
        {
            get { return responsecount; }
            set { responsecount = value; }
        }

        [JsonProperty(PropertyName = "info")]
        public string Info
        {
            get { return info; }
            set { info = value; }
        }

        [JsonProperty(PropertyName = "consent1")]
        public string Consent1
        {
            get { return consent1; }
            set { consent1 = value; }
        }


        [JsonProperty(PropertyName = "consent2")]
        public string Consent2
        {
            get { return consent2; }
            set { consent2 = value; }
        }


        [JsonProperty(PropertyName = "consent3")]
        public string Consent3
        {
            get { return consent3; }
            set { consent3 = value; }
        }


        [JsonProperty(PropertyName = "consent4")]
        public string Consent4
        {
            get { return consent4; }
            set { consent4 = value; }
        }


    }
}
