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

        // English parallel lists used for DB save (same order as localized lists)
        private static readonly List<string> _genericEnglish = new List<string>
        {
            "White", "White English", "White Welsh", "White Scottish", "White Northern Irish",
            "White Irish", "White Gypsy or Irish Traveller", "White Other",
            "Mixed White and Black Caribbean", "Mixed White and Black African", "Mixed White Other",
            "Asian Indian", "Asian Pakistani", "Asian Bangladeshi", "Asian Chinese", "Asian British", "Asian Other",
            "Black African", "Black African American", "Black Caribbean", "Black Other",
            "Arab", "Hispanic", "Latino", "Native American", "Pacific Islander", "Other", "Prefer not to disclose",
        };

        private static readonly List<string> _hopperEnglish = new List<string>
        {
            "Asian: Asian or Asian British", "Asian: Indian", "Asian: Pakistani", "Asian: Bangladeshi", "Asian: Chinese", "Asian: Any other Asian background",
            "Black: Caribbean", "Black: African", "Black: Any other Black, Black British, or Caribbean background",
            "Mixed: White and Black Caribbean", "Mixed: White and Black African", "Mixed: White and Asian", "Mixed: Any other Mixed or multiple ethnic background",
            "White: English, Welsh, Scottish, Northern Irish or British", "White: Irish", "White: Gypsy or Irish Traveller", "White: Roma", "White: Any other White background",
            "Arab", "Other: Any other ethnic group", "Prefer not to answer",
        };

        private static List<string> LocalizedGenericList() => new List<string>
        {
            LocalizationManager.Get("Eth_White"),
            LocalizationManager.Get("Eth_WhiteEnglish"),
            LocalizationManager.Get("Eth_WhiteWelsh"),
            LocalizationManager.Get("Eth_WhiteScottish"),
            LocalizationManager.Get("Eth_WhiteNorthernIrish"),
            LocalizationManager.Get("Eth_WhiteIrish"),
            LocalizationManager.Get("Eth_WhiteGypsy"),
            LocalizationManager.Get("Eth_WhiteOther"),
            LocalizationManager.Get("Eth_MixedWhiteBlackCarib"),
            LocalizationManager.Get("Eth_MixedWhiteBlackAf"),
            LocalizationManager.Get("Eth_MixedWhiteOther"),
            LocalizationManager.Get("Eth_AsianIndian"),
            LocalizationManager.Get("Eth_AsianPakistani"),
            LocalizationManager.Get("Eth_AsianBangladeshi"),
            LocalizationManager.Get("Eth_AsianChinese"),
            LocalizationManager.Get("Eth_AsianBritish"),
            LocalizationManager.Get("Eth_AsianOther"),
            LocalizationManager.Get("Eth_BlackAfrican"),
            LocalizationManager.Get("Eth_BlackAfricanAmerican"),
            LocalizationManager.Get("Eth_BlackCaribbean"),
            LocalizationManager.Get("Eth_BlackOther"),
            LocalizationManager.Get("Eth_Arab"),
            LocalizationManager.Get("Eth_Hispanic"),
            LocalizationManager.Get("Eth_Latino"),
            LocalizationManager.Get("Eth_NativeAmerican"),
            LocalizationManager.Get("Eth_PacificIslander"),
            LocalizationManager.Get("Eth_Other"),
            LocalizationManager.Get("Eth_PreferNotDisclose"),
        };

        private static List<string> LocalizedHopperList() => new List<string>
        {
            LocalizationManager.Get("Eth_HAsianOrAsianBritish"),
            LocalizationManager.Get("Eth_HAsianIndian"),
            LocalizationManager.Get("Eth_HAsianPakistani"),
            LocalizationManager.Get("Eth_HAsianBangladeshi"),
            LocalizationManager.Get("Eth_HAsianChinese"),
            LocalizationManager.Get("Eth_HAsianOtherBackground"),
            LocalizationManager.Get("Eth_HBlackCaribbean"),
            LocalizationManager.Get("Eth_HBlackAfrican"),
            LocalizationManager.Get("Eth_HBlackOtherBackground"),
            LocalizationManager.Get("Eth_HMixedWhiteBlackCarib"),
            LocalizationManager.Get("Eth_HMixedWhiteBlackAf"),
            LocalizationManager.Get("Eth_HMixedWhiteAsian"),
            LocalizationManager.Get("Eth_HMixedOther"),
            LocalizationManager.Get("Eth_HWhiteBritish"),
            LocalizationManager.Get("Eth_HWhiteIrish"),
            LocalizationManager.Get("Eth_HWhiteGypsy"),
            LocalizationManager.Get("Eth_HWhiteRoma"),
            LocalizationManager.Get("Eth_HWhiteOther"),
            LocalizationManager.Get("Eth_HArab"),
            LocalizationManager.Get("Eth_HOtherGroup"),
            LocalizationManager.Get("Eth_HPreferNotAnswer"),
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

        // Returns gender options translated into the current language for display purposes.
        // The parallel GenderOptions list (English) must be used when saving to the database.
        public static List<string> GenderOptionsLocalized()
        {
            return new List<string>
            {
                LocalizationManager.Get("Gender_Male"),
                LocalizationManager.Get("Gender_Female"),
                LocalizationManager.Get("Gender_PreferNotToSay"),
                LocalizationManager.Get("Gender_SelfDescribe")
            };
        }

        // Returns the English DB value for a given index into GenderOptions / GenderOptionsLocalized.
        public static string GenderEnglishValue(int index)
        {
            return index >= 0 && index < GenderOptions.Count ? GenderOptions[index] : string.Empty;
        }

        // Returns the index in GenderOptions for the given English DB value.
        public static int GenderIndexForEnglishValue(string englishValue)
        {
            return GenderOptions.IndexOf(englishValue ?? string.Empty);
        }

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

        // Show the language name (e.g. "English", "Español") rather than the raw code
        private static readonly Dictionary<string, string> _languageDisplayNames = new()
        {
            { "en", "English" }, { "pl", "Polski" }, { "ro", "Română" }, { "gu", "ગુજરાતી" }, { "es", "Español" }
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

        // Returns ethnicity list translated for the current language.
        // Use EthnicityEnglishValue(index) to get the English value for DB save.
        public static List<string> EthnicityOptionsLocalized()
        {
            var signup = Helpers.Settings.SignUp;
            if (string.IsNullOrEmpty(signup))
                return LocalizedGenericList();

            switch (signup.ToUpper())
            {
                case "HOPPERCTPE":
                case "HOPPERCR":
                    return LocalizedHopperList();
                default:
                    return LocalizedGenericList();
            }
        }

        // Returns the English DB value for a given index in the localized ethnicity list.
        public static string EthnicityEnglishValue(int index)
        {
            var signup = Helpers.Settings.SignUp?.ToUpper() ?? "";
            var english = (signup == "HOPPERCTPE" || signup == "HOPPERCR") ? _hopperEnglish : _genericEnglish;
            return index >= 0 && index < english.Count ? english[index] : string.Empty;
        }

        // Returns the index of the stored English DB value in the English list.
        public static int EthnicityIndexForEnglishValue(string englishValue)
        {
            var signup = Helpers.Settings.SignUp?.ToUpper() ?? "";
            var english = (signup == "HOPPERCTPE" || signup == "HOPPERCR") ? _hopperEnglish : _genericEnglish;
            return english.IndexOf(englishValue ?? string.Empty);
        }

        // Translates a stored English ethnicity value into the current language for display.
        public static string EthnicityLocalizedForEnglishValue(string englishValue)
        {
            int idx = EthnicityIndexForEnglishValue(englishValue);
            if (idx < 0) return englishValue; // unknown value — show as-is
            var localized = EthnicityOptionsLocalized();
            return idx < localized.Count ? localized[idx] : englishValue;
        }

        // Translates a stored English gender value into the current language for display.
        public static string GenderLocalizedForEnglishValue(string englishValue)
        {
            int idx = GenderIndexForEnglishValue(englishValue);
            if (idx < 0) return englishValue; // custom self-describe value — show as-is
            var localized = GenderOptionsLocalized();
            return idx < localized.Count ? localized[idx] : englishValue;
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
                    Id    = "Name",
                    Title = LocalizationManager.Get("Profile_Name"),
                    Role  = string.Join(" ", new[] { Helpers.Settings.FirstName, Helpers.Settings.Surname }
                           .Where(s => !string.IsNullOrWhiteSpace(s)))
                           is string fullName && !string.IsNullOrEmpty(fullName)
                           ? fullName
                           : "--", Image = "nameicon.png"
                },
                new user { Id = "Email",              Title = LocalizationManager.Get("Profile_Email"),       Role = !string.IsNullOrEmpty(Helpers.Settings.Email)       ? Helpers.Settings.Email       : "--", Image = "emailicon.png" },
                new user { Id = "Date of Birth",      Title = LocalizationManager.Get("Profile_DateOfBirth"), Role = !string.IsNullOrEmpty(Helpers.Settings.Age)         ? Helpers.Settings.Age         : "--", Image = "birthdateicon.png"  },
                new user { Id = "Gender",             Title = LocalizationManager.Get("Profile_Gender"),      Role = !string.IsNullOrEmpty(Helpers.Settings.Gender)      ? GenderLocalizedForEnglishValue(Helpers.Settings.Gender)           : "--", Image = "newgendericon.png"  },
                new user { Id = "Ethnicity",          Title = LocalizationManager.Get("Profile_Ethnicity"),   Role = !string.IsNullOrEmpty(Helpers.Settings.Ethnicity)   ? EthnicityLocalizedForEnglishValue(Helpers.Settings.Ethnicity)     : "--", Image = "ethnicityicon.png" },
                new user { Id = "Phone Number",       Title = LocalizationManager.Get("Profile_PhoneNumber"), Role = !string.IsNullOrEmpty(Helpers.Settings.PhoneNumber) ? Helpers.Settings.PhoneNumber : "--", Image = "numbericon.png" },
                new user { Id = "Town/City",          Title = LocalizationManager.Get("Profile_TownCity"),    Role = !string.IsNullOrEmpty(Helpers.Settings.Town)        ? Helpers.Settings.Town        : "--", Image = "townicon.png" }
            };

            if (Helpers.Settings.Validityconfirmed == "false")
            {
                items.Add(new user
                {
                    Id    = "National Health Identifier",
                    Title = LocalizationManager.Get("Profile_Nhi"),
                    Role  = !string.IsNullOrEmpty(Helpers.Settings.Userepid) ? Helpers.Settings.Userepid : "--"
                });
            }

            return items;
        }



        public static async Task<ObservableCollection<user>> GetSettingItems(bool showhide)
        {

            bool isEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();


            var langCode = Helpers.Settings.SelectedLanguage;
            var langDisplayName = _languageDisplayNames.TryGetValue(langCode ?? "", out var dn) ? dn : langCode ?? "--";

            var newuser = new user
            {
                Title = langDisplayName,
                Role = LocalizationManager.Get("Settings_ChangeLanguage"),
                Image = "world.png",
                Id = "Select Language"
            };

            var items = new ObservableCollection<user>
            {
                new user { Title = LocalizationManager.Get("Settings_ResetPassword"),        Role = "**********",                                                        Image = "passwordicon.png"  },
                new user { Title = LocalizationManager.Get("Settings_Notifications"),        Role = isEnabled ? LocalizationManager.Get("Settings_Enabled") : LocalizationManager.Get("Settings_Disabled"), Image = "bellicon.png" },
                new user { Title = LocalizationManager.Get("Settings_SignupCode"),           Role = !string.IsNullOrEmpty(Helpers.Settings.SignUp) ? Helpers.Settings.SignUp : "--", Image = "keyicon.png"},
                newuser,        
            };
            //var notificationTime = Preferences.Get("notificationtime", string.Empty);
            //if (!string.IsNullOrEmpty(notificationTime))
            //{
            if (showhide) 
            {
                items.Add(new user
                {
                    Title = LocalizationManager.Get("Settings_NotificationSchedule"),
                    Role  = LocalizationManager.Get("Settings_ChangeTime"),
                    Image = "time.png"
                });
            }

             
            //}
           
            return items;
        }

    }
}
