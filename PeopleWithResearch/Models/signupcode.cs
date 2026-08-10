using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class signupcode
    {

        [System.Text.Json.Serialization.JsonIgnore]

        //public string createdAt { get; set; }
        //public string updatedAt { get; set; }
        //public string version { get; set; }
        //public bool deleted { get; set; }
        public string signupcodeid { get; set; }
        public string signupcodegrouping { get; set; }
        public bool studyonboarding { get; set; }
        public string appdetails { get; set; }

        public string information { get; set; }

        public string consent { get; set; }

        public string faqs { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public ObservableCollection<InformationDetails> informationlist { get; set; } = new();

        [System.Text.Json.Serialization.JsonIgnore]
        public ObservableCollection<FAQItem> FAQList { get; set; } = new();


    }

    public class ApiResponseSignUpCode
    {
        public ObservableCollection<signupcode> Value { get; set; }
    }

    public class ConditionDetails
    {
        public string? conditionid { get; set; }
        public string? conditionlabel { get; set; }
        public string? conditiondescription { get; set; }
    }


    public class InformationDetails
    {
        public string? title { get; set; }
        public string? description { get; set; }
        public string? link { get; set; }

        public string? type { get; set; }

        public string? img { get; set; }


        [System.Text.Json.Serialization.JsonIgnore]
        public string ColorTheme { get; set; }
    }

    public class ConsentDetails
    {
        public string? age { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }

        public string? author { get; set; }

        public string? supportingdocumentfilename { get; set; }

        public List<ConsentSection> consentcontent { get; set; }

        public string? area { get; set; }

        public string? siganturepad { get; set; }

        public string? consentid { get; set; }

        public List<Signoffparameters> signoffparameters { get; set; }
    }
    public class ConsentSection
    {
        public string section { get; set; }
        public ObservableCollection<ConsentItem> sectioncontent { get; set; }
    }

    public class SectionConsent : ObservableCollection<ConsentItem>
    {
        public string SectionTitle { get; set; }
        public SectionConsent(string title, IEnumerable<ConsentItem> items) : base(items)
        {
            SectionTitle = title;
        }
    }

    public class Signoffparameters
    {
        public string label { get; set; }

        public string type { get; set; }

        public List<SignoffOption>? options { get; set; }

    }

    public class SignoffOption
    {
        public string optionid { get; set; }
        public string label { get; set; }
    }

    public class ConsentItem : INotifyPropertyChanged
    {
        public bool required { get; set; }
        public string consentitem { get; set; }

        public string consentitemid { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string? requiredlbl { get; set; }
        public bool Required { get; set; }
        private bool _isChecked;
        public bool ChckedState
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(ChckedState));
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        private bool _showValidation;
        public bool ShowValidation
        {
            get => _showValidation;
            set
            {
                _showValidation = value;
               // OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => ShowValidation && required && !ChckedState;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class FAQItem : INotifyPropertyChanged
    {
        public string Group { get; set; }
        public bool Active { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
