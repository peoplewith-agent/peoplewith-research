using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PeopleWithResearch
{
    /// <summary>
    /// User Object
    /// </summary>
    /// [Table("User")]
    [Table("user")]
    public class newuser 
    {
        public string userid { get; set; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime createdAt { get; set; }
        public bool deleted { get; set; }   
        public string firstname { get; set; }
        public string surname { get; set; }
        public string gender { get; set; }
        public string status { get; set; }
        public string ethnicity { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string postcode { get; set; }
        public string signupcodeid { get; set; }
        public string signupcodegrouping { get; set; }
        public bool primaryuser { get; set; }
        public string householdgroupid { get; set; }
        public string primarycareid { get; set; } 
        public string dateofbirth { get; set; }
        public string details { get; set; }
        public string notes { get; set; }
        public string telephone { get; set; }
        public string notificationtime { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime AccountCreated { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string studyinfo { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public ObservableCollection<NotificationData> NotificationDetails { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string studyactiveimage { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public bool showbaseline { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ObservableCollection<Detailslist> DetailsList { get; set; }
    }

    public class NotificationData
    { 
        public string DailyTime { get; set; }
        public string DailyId { get; set; }    
        public string WeeklyId { get; set; }
    }


    public class APINewUserResponse
    {
        public ObservableCollection<newuser> Value { get; set; }
    }


}
