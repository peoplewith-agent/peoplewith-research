using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace PeopleWithResearch
{

    public class questionnaires : INotifyPropertyChanged
    {
        public string questionnaireid { get; set; }
        public bool deleted { get; set; }
        public string title { get; set; }
        public string description { get; set; }

        public string signupcodeid { get; set; }

        [JsonPropertyName("questionanswerjson")]
        public string QuestionAnswerJsonRaw { get; set; }

        [JsonPropertyName("confirmationmessage")]
        public string ConfirmationMessageRaw { get; set; }

        [JsonIgnore]
        public ObservableCollection<QuestionAnswerJson> QuestionAnswerJson { get; set; }

        [JsonIgnore]
        public ObservableCollection<confirmationmessage> Confirmationmessage { get; set; }

        public string redcapid { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class QuestionAnswerJson : INotifyPropertyChanged
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public bool required { get; set; }

        public Option[] options { get; set; }

        //[JsonIgnore]
        public SymptomGroup[] symptom_groups { get; set; }

        //[JsonIgnore]
        public daytabs[] day_tabs { get; set; }

        //[JsonIgnore]
        public severityoptions[] severity_options { get; set; }

        public rashfollowup rash_followup { get; set; }

        public rashfollowup daily_impact { get; set; }

        [JsonPropertyName("default")]
        public string DefaultValue { get; set; }
        public string placeholder { get; set; }
        public int order { get; set; }
        public string section_label { get; set; }
        public string questionid { get; set; }
        public string image { get; set; }
        public string branchinglogic { get; set; }
        public string usertype { get; set; }
        public string sublabel { get; set; }

        [JsonIgnore]
        public bool notcomplete { get; set; } = true;

        [JsonIgnore]
        public bool showimage { get; set; } = false;

        //[JsonIgnore] public bool HasImage => !string.IsNullOrEmpty(LocalImagePath);

        private bool haserror;
        [JsonIgnore]
        public bool Haserror
        {
            get => haserror;
            set { haserror = value; OnPropertyChanged(); }
        }


        [JsonIgnore]
        public Uri imageURI => !string.IsNullOrEmpty(image)
         ? new Uri($"{APICalls.Imperial}{image}")
         : null;

        [JsonIgnore]
        private string _questionnum;
        [JsonIgnore]
        public string questionnum
        {
            get => _questionnum;
            set { _questionnum = value; OnPropertyChanged(); }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasAnswered;

        [System.Text.Json.Serialization.JsonIgnore]
        private Color _borderColor = Colors.White;
        [System.Text.Json.Serialization.JsonIgnore]

        public Color ColourBorder
        {
            get => _borderColor;
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    OnPropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        private bool showrequired;
        [System.Text.Json.Serialization.JsonIgnore]
        public bool ShowRequired
        {
            get => showrequired;
            set
            {
                if (showrequired != value)
                {
                    showrequired = value;
                    OnPropertyChanged();
                }
            }
        }


        [System.Text.Json.Serialization.JsonIgnore]
        private bool shownext;
        [System.Text.Json.Serialization.JsonIgnore]
        public bool Shownext
        {
            get => shownext;
            set
            {
                if (shownext != value)
                {
                    shownext = value;
                    OnPropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        private bool showback;
        [System.Text.Json.Serialization.JsonIgnore]
        public bool Showback
        {
            get => showback;
            set
            {
                if (showback != value)
                {
                    showback = value;
                    OnPropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        private bool showsubmit;
        [System.Text.Json.Serialization.JsonIgnore]
        public bool Showsubmit
        {
            get => showsubmit;
            set
            {
                if (showsubmit != value)
                {
                    showsubmit = value;
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

    public class Option : INotifyPropertyChanged
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public string title { get; set; }
        public string answerid { get; set; }
        public string value { get; set; }
        public string text { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        private int _slidervalue { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public int SliderValue
        {
            get => _slidervalue;
            set
            {
                if (_slidervalue != value)
                {
                    _slidervalue = value;
                    OnPropertyChanged();
                    this.value = value.ToString();
                }
            }
        }


        private bool _selected;
        [JsonIgnore]
        public bool selected
        {
            get => _selected;
            set { _selected = value; OnPropertyChanged(); }
        }

        private bool haserror;
        [JsonIgnore]
        public bool Haserror
        {
            get => haserror;
            set { haserror = value; OnPropertyChanged(); }
        }
        private string errortext { get; set; }

        [JsonIgnore]
        public string Errortext
        {
            get => errortext;
            set { errortext = value; OnPropertyChanged(); }
        }
 

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Symptom : INotifyPropertyChanged
    {
        public string id { get; set; }
        public string label { get; set; }


        private ObservableCollection<symptomdata> _symptomData;
        public ObservableCollection<symptomdata> SymptomData
        {
            get => _symptomData;
            set
            {
                _symptomData = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSymptomData));
            }
        }

        [JsonIgnore]
        public bool HasSymptomData =>
        SymptomData != null && SymptomData.Any();


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }

    public class symptomdata
    {

        public string id { get; set; }
        //label = date of symptom 
        public string label { get; set; }

        //Denotes index of that label
        public object value { get; set; }

        //Text = Symptom intensity selected
        public string text { get; set; }

        public string colour { get; set; }

        public string textcolour { get; set; }
    }

    public class severityoptions
    {
        public string value { get; set; }
        public string text { get; set; }
    }

    public class rashfollowup
    {
        public string note { get; set; }
        public question[] questions { get; set; }
    }

    public class question : INotifyPropertyChanged
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public bool required { get; set; }
        public Option[] options { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string show_if { get; set; }

        [JsonIgnore]
        public bool HasOptions =>
    options != null && options.Any();

        [JsonIgnore]
        public bool HasGroupedOptions =>
            groupedOptions != null &&
            groupedOptions.Any(g => g.options != null && g.options.Any());

        [JsonIgnore]
        public ObservableCollection<OptionGroup> groupedOptions { get; set; }

        [JsonIgnore]
        private string _selectedtext;

        [JsonIgnore]
        public string selectedtext
        {
            get => _selectedtext;
            set
            {
                if (_selectedtext != value)
                {
                    _selectedtext = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OptionGroup
    {
        public string title { get; set; }
        public ObservableCollection<Option> options { get; set; }
    }

    public class SymptomGroup : INotifyPropertyChanged
    {
        public string group { get; set; }
        public ObservableCollection<Symptom> symptoms { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class daytabs
    {
        public string id {get;set; }
        public string label { get;set; }
        public string show_if { get; set; }
    }

    public class confirmationmessage : INotifyPropertyChanged
    {
        public string confirmationmessageid { get; set; }
        public string confirmationmessagetitle { get; set; }
        public string action { get; set; }
        public string answerid { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string title { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string image { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public bool Hasimage { get; set; } = false;

        [System.Text.Json.Serialization.JsonIgnore]
        public bool Hideimage { get; set; } = true;

        [System.Text.Json.Serialization.JsonIgnore]
        private ImageSource _imageSource;

        [System.Text.Json.Serialization.JsonIgnore]
        public ImageSource ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; OnPropertyChanged(); }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        private string _picturebtn = "Take Photo";

        [System.Text.Json.Serialization.JsonIgnore]
        public string Picturebtn
        {
            get => _picturebtn;
            set { _picturebtn = value; OnPropertyChanged(); }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        private string _details = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        public string Details
        {
            get => _details;
            set { _details = value; OnPropertyChanged(); }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        private string _completebtn = "Upload from Gallery";

        [System.Text.Json.Serialization.JsonIgnore]
        public string Completebtn
        {
            get => _completebtn;
            set { _completebtn = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ApiResponseQuestionnaire
    {
        [JsonPropertyName("value")]
        public List<questionnaires> Value { get; set; }
    }
}