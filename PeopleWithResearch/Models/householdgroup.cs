using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class householdgroup
    {
        public string householdgroupid { get; set; }
        public string primaryuserid { get; set; }
        public string groupuserdetails { get; set; }
        public string details { get; set; }
        public string signupcodeid { get; set; }

        public string signupcodegrouping { get; set; }

        public bool active { get; set; }

        public string primarycareid { get; set; }

        public string status { get; set; }



        [System.Text.Json.Serialization.JsonIgnore]
        public ObservableCollection<householdgroupjsondetails> userdetailslist { get; set; } = new();


        [System.Text.Json.Serialization.JsonIgnore]
        public householdstudyrecord studydetails
        {
            get
            {
                if (string.IsNullOrEmpty(details)) return null;
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return System.Text.Json.JsonSerializer.Deserialize<householdstudyrecord>(details, options);
            }
            set
            {
                details = System.Text.Json.JsonSerializer.Serialize(value);
            }
        }
    }


    public class ApiResponseUserHousehold
    {
        public ObservableCollection<householdgroup> Value { get; set; }
    }
}
