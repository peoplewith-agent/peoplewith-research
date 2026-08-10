using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    public class incidents : DatasyncClientData
    {

        string id;
        string? userid;
        string? notificationid;
        string? userquestionnaireid;
        bool reportacknowledged;
        bool invitedtocollectkit;
        bool kitcollectedgp;
        bool kitreturnedgp;
        bool kitcollectedpatient;
        bool kitreturnedpatient;
        string? week;
        string? weeklyfollowupanswerid;
        string? notes;
        DateTimeOffset createdAt;


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
        public string? Userid
        {
            get { return userid; }
            set { userid = value; }
        }

        [JsonProperty(PropertyName = "notificationid")]
        public string? Notificationid
        {
            get { return notificationid; }
            set { notificationid = value; }
        }

        [JsonProperty(PropertyName = "userquestionnaireid")]
        public string? Userquestionnaireid
        {
            get { return userquestionnaireid; }
            set { userquestionnaireid = value; }
        }

        [JsonProperty(PropertyName = "reportacknowledged")]
        public bool Reportacknowledged
        {
            get { return reportacknowledged; }
            set { reportacknowledged = value; }
        }

        [JsonProperty(PropertyName = "invitedtocollectkit")]
        public bool Invitedtocollectkit
        {
            get { return invitedtocollectkit; }
            set { invitedtocollectkit = value; }
        }

        [JsonProperty(PropertyName = "kitcollectedgp")]
        public bool Kitcollectedgp
        {
            get { return kitcollectedgp; }
            set { kitcollectedgp = value; }
        }

        [JsonProperty(PropertyName = "kitreturnedgp")]
        public bool Kitreturnedgp
        {
            get { return kitreturnedgp; }
            set { kitreturnedgp = value; }
        }

        [JsonProperty(PropertyName = "kitcollectedpatient")]
        public bool Kitcollectedpatient
        {
            get { return kitcollectedpatient; }
            set { kitcollectedpatient = value; }
        }

        [JsonProperty(PropertyName = "kitreturnedpatient")]
        public bool Kitreturnedpatient
        {
            get { return kitreturnedpatient; }
            set { kitreturnedpatient = value; }
        }

        [JsonProperty(PropertyName = "week")]
        public string? Week
        {
            get { return week; }
            set { week = value; }
        }

        [JsonProperty(PropertyName = "weeklyfollowupanswerid")]
        public string? Weeklyfollowupanswerid
        {
            get { return weeklyfollowupanswerid; }
            set { weeklyfollowupanswerid = value; }
        }


        [JsonProperty(PropertyName = "notes")]
        public string? Notes
        {
            get { return notes; }
            set { notes = value; }
        }

        [JsonProperty(PropertyName = "createdAt")]
        public DateTimeOffset CreatedAt
        {
            get { return createdAt; }
            set { createdAt = value; }
        }

    }
}

