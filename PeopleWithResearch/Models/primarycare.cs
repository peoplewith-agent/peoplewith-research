using System;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    public class primarycare : DatasyncClientData
    {

        string id;
        string? practicename;
        string? address1;
        string? address2;
        string? address3;
        string? address4;
        string? postcode;
        string? telephonenumber;
        string? LCG;
        string? partnershipname;
        string? partnershipno;
        string? pracno;
        string? email;
        string? prescriptionemail;
        string? advertid;


        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "practicename")]
        public string? Practicename
        {
            get { return practicename; }
            set { practicename = value; }
        }

        [JsonProperty(PropertyName = "address1")]
        public string? Address1
        {
            get { return address1; }
            set { address1 = value; }
        }

        [JsonProperty(PropertyName = "address2")]
        public string? Address2
        {
            get { return address2; }
            set { address2 = value; }
        }

        [JsonProperty(PropertyName = "address3")]
        public string? Address3
        {
            get { return address3; }
            set { address3 = value; }
        }

        [JsonProperty(PropertyName = "address4")]
        public string? Address4
        {
            get { return address4; }
            set { address4 = value; }
        }

        [JsonProperty(PropertyName = "postcode")]
        public string? Postcode
        {
            get { return postcode; }
            set { postcode = value; }
        }

        [JsonProperty(PropertyName = "telephonenumber")]
        public string? Telephonenumber
        {
            get { return telephonenumber; }
            set { telephonenumber = value; }
        }

        [JsonProperty(PropertyName = "LCG")]
        public string? lcg
        {
            get { return LCG; }
            set { LCG = value; }
        }

        [JsonProperty(PropertyName = "partnershipname")]
        public string? Partnershipname
        {
            get { return partnershipname; }
            set { partnershipname = value; }
        }

        [JsonProperty(PropertyName = "partnershipno")]
        public string? Partnershipno
        {
            get { return partnershipno; }
            set { partnershipno = value; }
        }

        [JsonProperty(PropertyName = "pracno")]
        public string? Pracno
        {
            get { return pracno; }
            set { pracno = value; }
        }

        [JsonProperty(PropertyName = "email")]
        public string? Email
        {
            get { return email; }
            set { email = value; }
        }

        [JsonProperty(PropertyName = "prescriptionemail")]
        public string? Prescriptionemail
        {
            get { return prescriptionemail; }
            set { prescriptionemail = value; }
        }

        [JsonProperty(PropertyName = "advertid")]
        public string? Advertid
        {
            get { return advertid; }
            set { advertid = value; }
        }

    }
}
