using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch.Models
{
    class AppConfig
    {

        public OverallSettings OverallSettings { get; set; }
        public List<RegField> RegFields { get; set; }

    }


    public class OverallSettings
    {
        public bool StudyOpen { get; set; }
        public string StudyLimit { get; set; }
        public string CurrentParticipants { get; set; }
        public string StudyName { get; set; }

        public string StudyTitle { get; set; }
        public string StudyDescription { get; set; }
    }

    public class RegField
    {
        public string Id { get; set; }
        public string Label { get; set; }

        public string SubLabel { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public List<OptionDetails> Options { get; set; }
        public string Default { get; set; }
        public string Placeholder { get; set; }
        public string Order { get; set; }
        public string HelpText { get; set; }

        public string HelpTextInfo { get; set; }

        public string questionid { get; set; }
        public bool Active { get; set; }
        public string XamlNameArea { get; set; }

        public List<RegField> subFields { get; set; }

    }

    public class OptionDetails   
    { 
        public string AnswerId { get; set; } 
        public string Value { get; set; } 
        public string Text { get; set; }

        public List<string> validpostcodesvalues { get; set; }
    }


    public class RegQuestionAnswerJson
    {
        public string questionid { get; set; }
        public string answerid { get; set; }
        public string value { get; set; }
    }
}
