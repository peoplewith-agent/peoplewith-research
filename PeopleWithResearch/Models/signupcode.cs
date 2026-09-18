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


    public class InformationTranslation
    {
        public string? title { get; set; }
        public string? description { get; set; }
        public string? link { get; set; }
    }

    public class InformationDetails
    {
        public string? title { get; set; }
        public string? description { get; set; }
        public string? link { get; set; }

        public string? type { get; set; }

        public string? img { get; set; }

        [Newtonsoft.Json.JsonProperty("translations")]
        public Dictionary<string, InformationTranslation>? translations { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string ColorTheme { get; set; }

        /// <summary>
        /// Returns the localised title, falling back to the base title.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string? LocalisedTitle => GetLocalised(t => t.title) ?? title;

        /// <summary>
        /// Returns the localised description, falling back to the base description.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string? LocalisedDescription => GetLocalised(t => t.description) ?? description;

        /// <summary>
        /// Returns the localised link, falling back to the base link.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string? LocalisedLink => GetLocalised(t => t.link) ?? link;

        private string? GetLocalised(Func<InformationTranslation, string?> selector)
        {
            var lang = Helpers.Settings.SelectedLanguage;
            if (string.IsNullOrEmpty(lang) || translations == null) return null;
            if (translations.TryGetValue(lang, out var t)) return selector(t);
            // Try two-letter prefix (e.g. "en-GB" → "en")
            var prefix = lang.Length >= 2 ? lang[..2] : lang;
            if (translations.TryGetValue(prefix, out var t2)) return selector(t2);
            return null;
        }
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

        /// <summary>
        /// Tri-state consent selection: null = unanswered, true = "I consent", false = "I do not consent".
        /// </summary>
        private bool? _consentGiven;
        public bool? ConsentGiven
        {
            get => _consentGiven;
            set
            {
                if (_consentGiven != value)
                {
                    _consentGiven = value;
                    // Keep legacy ChckedState in sync so submission logic still works
                    _isChecked = value == true;
                    OnPropertyChanged(nameof(ConsentGiven));
                    OnPropertyChanged(nameof(ChckedState));
                    OnPropertyChanged(nameof(IsConsentGiven));
                    OnPropertyChanged(nameof(IsConsentNotGiven));
                    OnPropertyChanged(nameof(HasError));
                    OnPropertyChanged(nameof(IsBlockingError));
                }
            }
        }

        /// <summary>True when the participant explicitly chose "I consent" — used for button highlight binding.</summary>
        public bool IsConsentGiven => ConsentGiven == true;
        /// <summary>True when the participant explicitly chose "I do not consent" — used for button highlight binding.</summary>
        public bool IsConsentNotGiven => ConsentGiven == false;

        private bool _showValidation;
        public bool ShowValidation
        {
            get => _showValidation;
            set
            {
                _showValidation = value;
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(IsBlockingError));
            }
        }

        /// <summary>True when validation has run and this required item is unanswered (null) — drives border/label error styling.</summary>
        public bool HasError => ShowValidation && required && ConsentGiven is null;

        /// <summary>True when a required item has been actively declined — drives the blocking warning message.</summary>
        public bool IsBlockingError => required && ConsentGiven == false;

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
