using Newtonsoft.Json;
using PeopleWithResearch;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class Genericlist
    {

        private static readonly List<string> GenericList = new List<string>
        {
            "White", "White English", "White Welsh", "White Scottish", "White Northern Irish",
            "White Irish", "White Gypsy or Irish Traveller", "White Other",
            "Mixed White and Black Caribbean", "Mixed White and Black African", "Mixed White Other",
            "Asian Indian", "Asian Pakistani", "Asian Bangladeshi", "Asian Chinese", "Asian British", "Asian Other",
            "Black African", "Black African American", "Black Caribbean", "Black Other",
            "Arab", "Hispanic", "Latino", "Native American", "Pacific Islander", "Other", "Prefer not to disclose"
        };

        private static readonly List<string> HopperCtpeList = new List<string>
        {
            "Asian: Asian or Asian British", "Asian: Indian", "Asian: Pakistani", "Asian: Bangladeshi", "Asian: Chinese", "Asian: Any other Asian background",
            "Black: Caribbean", "Black: African", "Black: Any other Black, Black British, or Caribbean background",
            "Mixed: White and Black Caribbean", "Mixed: White and Black African", "Mixed: White and Asian", "Mixed: Any other Mixed or multiple ethnic background",
            "White: English, Welsh, Scottish, Northern Irish or British", "White: Irish", "White: Gypsy or Irish Traveller", "White: Roma", "White: Any other White background",
            "Arab", "Other: Any other ethnic group", "Prefer not to answer"
        };

        private static readonly List<string> HOPPERCRList = new List<string>
        {
            "Asian: Asian or Asian British", "Asian: Indian", "Asian: Pakistani", "Asian: Bangladeshi", "Asian: Chinese", "Asian: Any other Asian background",
            "Black: Caribbean", "Black: African", "Black: Any other Black, Black British, or Caribbean background", "Mixed: White and Black Caribbean",
            "Mixed: White and Black African", "Mixed: White and Asian", "Mixed: Any other Mixed or multiple ethnic background",
            "White: English, Welsh, Scottish, Northern Irish or British", "White: Irish", "White: Gypsy or Irish Traveller", "White: Roma",
            "White: Any other White background", "Arab", "Other: Any other ethnic group", "Prefer not to answer"
        };

        public static readonly List<string> ProfileColors = new List<string>
        {
            "#CCEBF9", // light blue (original)
            "#D8F9CC", // soft green
            "#F9ECCC", // pale yellow
            "#F9D6CC", // light peach
            "#E1CCF9", // lavender
            "#CCE9F9", // slightly different blue
            "#F9CCE3", // light pink
            "#CCF9F2", // mint aqua
            "#F4F9CC", // pale lime
            "#CCD7F9", // periwinkle
            "#F9CCCC", // soft rose
        };

        public static readonly List<string> GenderOptions = new List<string>
        {
            "Male",
            "Female",
            "Prefer not to say",
            //"Other (Specify)",
            "Prefer to self-describe"
        };

        public static readonly List<string> AgeOptions = new List<string>
        {
            "0 - 5",
            "5 - 10",
            "11 - 15",
            "16+",
        };

        public static readonly List<string> RelationOptions = new List<string>
        {
            "I am their parent/guardian",
            "I am their partner/spouse",
            "I am their child",
            "Other"
        };


        public static List<string> EthnicityOptions()
        {
            var signup = Helpers.Settings.SignUp;

            if (string.IsNullOrEmpty(signup))
            {
                return GenericList;
            }

            switch (signup.ToUpper())
            {
                case "HOPPERCTPE":
                    return HopperCtpeList;

                case "HOPPERCR":
                    return HOPPERCRList;

                default:
                    return GenericList;
            }
        }



        public static int CalculateUserAge(DateTime Dob)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - Dob.Year;
            if (Dob.Date > today.AddYears(-age))
            {
                age--;
            }
            return age;

        }


        public static ObservableCollection<user> GetProfileItems()
        {
            var items = new ObservableCollection<user>
            {
                new user
                {
                    Title = "Name",
                    Role = string.Join(" ", new[] { Helpers.Settings.FirstName, Helpers.Settings.Surname }
                           .Where(s => !string.IsNullOrWhiteSpace(s)))
                           is string fullName && !string.IsNullOrEmpty(fullName)
                           ? fullName
                           : "--" , Image = "nameicon.png"
                },
                new user { Title = "Email", Role = !string.IsNullOrEmpty(Helpers.Settings.Email) ? Helpers.Settings.Email : "--", Image = "emailicon.png" },
                new user { Title = "Date of Birth", Role = !string.IsNullOrEmpty(Helpers.Settings.Age) ? Helpers.Settings.Age : "--", Image = "birthdateicon.png"  },
                new user { Title = "Gender", Role = !string.IsNullOrEmpty(Helpers.Settings.Gender) ? Helpers.Settings.Gender : "--", Image = "newgendericon.png"  },
                new user { Title = "Ethnicity", Role = !string.IsNullOrEmpty(Helpers.Settings.Ethnicity) ? Helpers.Settings.Ethnicity : "--" , Image = "ethnicityicon.png" },
                new user { Title = "Phone Number", Role = !string.IsNullOrEmpty(Helpers.Settings.PhoneNumber) ? Helpers.Settings.PhoneNumber : "--" , Image = "numbericon.png"},
                new user { Title = "Town/City", Role = !string.IsNullOrEmpty(Helpers.Settings.Town) ? Helpers.Settings.Town : "--" , Image = "townicon.png"}
            };

            if (Helpers.Settings.Validityconfirmed == "false")
            {
                items.Add(new user
                {
                    Title = "National Health Identifier",
                    Role = !string.IsNullOrEmpty(Helpers.Settings.Userepid) ? Helpers.Settings.Userepid : "--"
                });
            }

            return items;
        }



        public static async Task<ObservableCollection<user>> GetSettingItems(bool showhide)
        {

            bool isEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();

            var items = new ObservableCollection<user>
            {
                new user { Title = "Reset Password", Role = "**********", Image = "passwordicon.png"  },
                new user { Title = "Notifications", Role = isEnabled ? "Enabled" : "Disabled" , Image = "bellicon.png" },
                new user { Title = "Sign-up Code", Role = !string.IsNullOrEmpty(Helpers.Settings.SignUp) ? Helpers.Settings.SignUp : "--" , Image = "keyicon.png"},
            
            };
            //var notificationTime = Preferences.Get("notificationtime", string.Empty);
            //if (!string.IsNullOrEmpty(notificationTime))
            //{
            if (showhide) 
            {
                items.Add(new user
                {
                    Title = "Notification Schedule",
                    Role = "Change Time",
                    Image = "time.png"
                });
            }

             
            //}
           
            return items;
        }

    }
}
