using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class QuestionModel
    {
        public string Id { get; set; }                // Group UUID
        public string Label { get; set; }           // Group title
        public string Type { get; set; }            // Should be "group"
        public bool Required { get; set; }
        public string Default { get; set; }
        public string Placeholder { get; set; }
        public string Order { get; set; }
        public bool Active { get; set; }
        public string XamlNameArea { get; set; }
        public List<QuestionField> Fields { get; set; } = new List<QuestionField>();
    }
    public class QuestionField
    {
        public string QuestionId { get; set; }        // Question UUID
        public string Label { get; set; }           // Question text
        public string Type { get; set; }            // singleselection, freetext
        public bool Required { get; set; }
        public string Default { get; set; }
        public string Placeholder { get; set; }
        public string Order { get; set; }
        public bool Active { get; set; }
        public string XamlNameArea { get; set; }
        public string Directions { get; set; }      // Optional extra info
        public string Why { get; set; }             // Optional "why we're asking"
        public List<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    }
    public class QuestionAnswer
    {
        public string AnswerId { get; set; }          // Answer UUID
        public string Label { get; set; }           // Answer text
        public string Value { get; set; }           // Optional value (can bind to selected value)
        public int Order { get; set; }
    }
}
