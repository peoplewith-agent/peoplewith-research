using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class newuserquestionnaire : INotifyPropertyChanged
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string userquestionnaireid { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime createdAt{ get; set; }
        public bool deleted { get; set; }
        public string userid { get; set; }
        public string questionnaireid { get; set; }
        public string feedback { get; set; }

        [JsonIgnore]
        public string BorderColour { get; set; }

        [JsonIgnore]
        public string FormattedDateTime { get; set; }

        private string _title;
        [JsonIgnore]
        public string title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(); 
                }
            }
        }

        [JsonIgnore]
        public string DisplayTitle { get; set; }

        [JsonIgnore]
        public DateTime DateTimeAdded { get; set; }


        [JsonIgnore]
        public string CompletedBy { get; set; }

        [JsonIgnore]
        public bool ShowCompletedBy { get; set; }

        [JsonIgnore]
        public ObservableCollection<Feedback> FeedbackList { get; set; }
        public string imagefilename { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Feedback
    {
        public string questionid { get; set; }
        public ObservableCollection<Answer> answer { get; set; }
    }

    public class Answer
    {
        public string answerid { get; set; }
        public object answervalue { get; set; }
        public object answeroptions { get; set; }
        public string text { get; set; }
    }

    public class ApiUserResponseQuestionnaire
    {
        [JsonPropertyName("value")]
        public List<newuserquestionnaire> Value { get; set; }
    }

}
