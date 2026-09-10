using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using STJIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace PeopleWithResearch
{
    public class householdgroupjsondetails : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

       
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string household_group_id { get; set; }

        private string _name;
        public string household_individual_name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _email;
        public string household_individual_email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }
        public string household_individual_userid { get; set; }

        private string _status;
        public string household_individual_status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private string _relation;
        public string household_individual_relationship
        {
            get => _relation;
            set { _relation = value; OnPropertyChanged(); }
        }

        private string _age;
        public string household_individual_age
        {
            get => _age;
            set { _age = value; OnPropertyChanged(); }
        }

        private string _repaccess;
        public string household_rep_access
        {
            get => _repaccess;
            set { _repaccess = value; OnPropertyChanged(); }
        }
        public string household_individual_baseline_samples { get; set; }

        // --- Computed UI properties ---

        private string _studyinfo;
        [JsonIgnore]
        public string Studyinfo
        {
            get => _studyinfo;
            set { _studyinfo = value; OnPropertyChanged(); }
        }

        private string _studyactiveimage;
        [JsonIgnore]
        public string Studyactiveimage
        {
            get => _studyactiveimage;
            set { _studyactiveimage = value; OnPropertyChanged(); }
        }

        private double _listOpacity = 1;
        [JsonIgnore]
        public double ListOpacity
        {
            get => _listOpacity;
            set { _listOpacity = value; OnPropertyChanged(); }
        }

        // Shows the full actions + baseline section (hidden for Household Rep and Withdrawn)
        private bool _showActions;
        [JsonIgnore]
        public bool ShowActions
        {
            get => _showActions;
            set { _showActions = value; OnPropertyChanged(); }
        }

        private bool _showDetails;
        [JsonIgnore]
        public bool ShowDetails
        {
            get => _showDetails;
            set { _showDetails = value; OnPropertyChanged(); }
        }

        private bool _showawaitingbaseline;
        [JsonIgnore]
        public bool ShowAwaitingBaseline
        {
            get => _showawaitingbaseline;
            set { _showawaitingbaseline = value; OnPropertyChanged(); }
        }


        // Shows the baseline warning text block
        private bool _showBaselineText;
        [JsonIgnore]
        public bool ShowBaselineText
        {
            get => _showBaselineText;
            set { _showBaselineText = value; OnPropertyChanged(); }
        }

        // Baseline button opacity — 1.0 if actionable, 0.2 if not
        private double _baselineButtonOpacity = 1;
        [JsonIgnore]
        public double BaselineButtonOpacity
        {
            get => _baselineButtonOpacity;
            set { _baselineButtonOpacity = value; OnPropertyChanged(); }
        }

        // Whether the baseline button is tappable
        private bool _baselineButtonEnabled = true;
        [JsonIgnore]
        public bool BaselineButtonEnabled
        {
            get => _baselineButtonEnabled;
            set { _baselineButtonEnabled = value; OnPropertyChanged(); }
        }

        // Manage profile button opacity — dimmed during onboarding
        private double _manageButtonOpacity = 1;
        [JsonIgnore]
        public double ManageButtonOpacity
        {
            get => _manageButtonOpacity;
            set { _manageButtonOpacity = value; OnPropertyChanged(); }
        }

        private bool _manageButtonEnabled = true;
        [JsonIgnore]
        public bool ManageButtonEnabled
        {
            get => _manageButtonEnabled;
            set { _manageButtonEnabled = value; OnPropertyChanged(); }
        }

        private string _sendReminderText = "Send reminder";
        [JsonIgnore]
        public string SendReminderText
        {
            get => _sendReminderText;
            set { _sendReminderText = value; OnPropertyChanged(); }
        }

        private string _lastActiveDate;
        [JsonIgnore]
        public string LastActiveDate
        {
            get => _lastActiveDate;
            set { _lastActiveDate = value; OnPropertyChanged(); }
        }

        private string _questionnairesCompleted;
        [JsonIgnore]
        public string QuestionnairesCompleted
        {
            get => _questionnairesCompleted;
            set { _questionnairesCompleted = value; OnPropertyChanged(); }
        }

        // --- Baseline Form status tile ---

        private string _baselineFormStatusText;
        [JsonIgnore]
        public string BaselineFormStatusText
        {
            get => _baselineFormStatusText;
            set { _baselineFormStatusText = value; OnPropertyChanged(); }
        }

        private string _baselineFormStatusColor;
        [JsonIgnore]
        [STJIgnore]
        public string BaselineFormStatusColor
        {
            get => _baselineFormStatusColor;
            set { _baselineFormStatusColor = value; OnPropertyChanged(); }
        }

        private string _baselineFormStatusIcon;
        [JsonIgnore]
        [STJIgnore]
        public string BaselineFormStatusIcon
        {
            get => _baselineFormStatusIcon;
            set { _baselineFormStatusIcon = value; OnPropertyChanged(); }
        }

   

        private string _baselineFormTextColor;
        [JsonIgnore]
        [STJIgnore]
        public string BaselineFormTextColor
        {
            get => _baselineFormTextColor;
            set { _baselineFormTextColor = value; OnPropertyChanged(); }
        }

        // --- Baseline Samples status tile ---

        private string _baselineSamplesStatusText;
        [JsonIgnore]
        [STJIgnore]
        public string BaselineSamplesStatusText
        {
            get => _baselineSamplesStatusText;
            set { _baselineSamplesStatusText = value; OnPropertyChanged(); }
        }

        private string _baselineSamplesStatusColor;
        [JsonIgnore]
        [STJIgnore]
        public string BaselineSamplesStatusColor
        {
            get => _baselineSamplesStatusColor;
            set { _baselineSamplesStatusColor = value; OnPropertyChanged(); }
        }

        private string _baselineSamplesStatusIcon;
        [JsonIgnore]
        [STJIgnore]
        public string BaselineSamplesStatusIcon
        {
            get => _baselineSamplesStatusIcon;
            set { _baselineSamplesStatusIcon = value; OnPropertyChanged(); }
        }



        private string _baselineSamplesTextColor;
        [JsonIgnore]
        [STJIgnore]
        public string BaselineSamplesTextColor
        {
            get => _baselineSamplesTextColor;
            set { _baselineSamplesTextColor = value; OnPropertyChanged(); }
        }

        private Brush _baselineFormBorderColor;
        [JsonIgnore]
        [STJIgnore]
        public Brush BaselineFormBorderColor
        {
            get => _baselineFormBorderColor;
            set { _baselineFormBorderColor = value; OnPropertyChanged(); }
        }

        private Brush _baselineSamplesBorderColor;
        [JsonIgnore]
        [STJIgnore]
        public Brush BaselineSamplesBorderColor
        {
            get => _baselineSamplesBorderColor;
            set { _baselineSamplesBorderColor = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public bool mainuser { get; set; } = false;


        private bool _ShowSwitchProfile;
        [JsonIgnore]
        public bool ShowSwitchProfile
        {
            get => _ShowSwitchProfile;
            set { _ShowSwitchProfile = value; OnPropertyChanged(); }
        }

        private bool _ShowActiveProfile;
        [JsonIgnore]
        public bool ShowActiveProfile
        {
            get => _ShowActiveProfile;
            set { _ShowActiveProfile = value; OnPropertyChanged(); }
        }

        private bool _showContactStudyTeam;
        [JsonIgnore]
        public bool ShowContactStudyTeam
        {
            get => _showContactStudyTeam;
            set { _showContactStudyTeam = value; OnPropertyChanged(); }
        }

    }
}
