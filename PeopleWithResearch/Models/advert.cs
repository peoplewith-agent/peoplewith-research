using System;
using System.Drawing;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;
namespace PeopleWithResearch
{
    public class advert : DatasyncClientData
    {


        string id;
        string? advertID;
        string? advertTitle;
        string? advertGender;
        string? advertAge;
        string? advertCondition;
        string? advertMedications;
        string? advertSymptoms;
        string? advertEthnicity;
        string? advertReferral;
        bool consent;
        bool additionalConsent;
        bool consentReminder;
        string siteTitle;
        bool studyOpen;


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
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "advertID")]
        public string? AdvertID
        {
            get { return advertID; }
            set { advertID = value; }
        }

        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "advertTitle")]
        public string? AdvertTitle
        {
            get { return advertTitle; }
            set { advertTitle = value; }
        }

        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "advertGender")]
        public string? AdvertGender
        {
            get { return advertGender; }
            set { advertGender = value; }
        }

        /// <summary>
        /// ID getter/setter
        /// </summary>
        [JsonProperty(PropertyName = "advertAge")]
        public string? AdvertAge
        {
            get { return advertAge; }
            set { advertAge = value; }
        }

        [JsonProperty(PropertyName = "advertCondition")]
        public string? AdvertCondition
        {
            get { return advertCondition; }
            set { advertCondition = value; }
        }

        [JsonProperty(PropertyName = "advertMedications")]
        public string? AdvertMedications
        {
            get { return advertMedications; }
            set { advertMedications = value; }
        }

        [JsonProperty(PropertyName = "advertSymptoms")]
        public string? AdvertSymptoms
        {
            get { return advertSymptoms; }
            set { advertSymptoms = value; }
        }

        [JsonProperty(PropertyName = "advertEthnicity")]
        public string? AdvertEthnicity
        {
            get { return advertEthnicity; }
            set { advertEthnicity = value; }
        }

        [JsonProperty(PropertyName = "advertReferral")]
        public string? AdvertReferral
        {
            get { return advertReferral; }
            set { advertReferral = value; }
        }

        [JsonProperty(PropertyName = "consent")]
        public bool Consent
        {
            get { return consent; }
            set { consent = value; }
        }

        [JsonProperty(PropertyName = "additionalConsent")]
        public bool AdditionalConsent
        {
            get { return additionalConsent; }
            set { additionalConsent = value; }
        }


        [JsonProperty(PropertyName = "consentReminder")]
        public bool ConsentReminder
        {
            get { return consentReminder; }
            set { consentReminder = value; }
        }

        [JsonProperty(PropertyName = "siteTitle")]
        public string SiteTitle
        {
            get { return siteTitle; }
            set { siteTitle = value; }
        }


        [JsonProperty(PropertyName = "studyOpen")]
        public bool StudyOpen
        {
            get { return studyOpen; }
            set { studyOpen = value; }
        }


    }
}
