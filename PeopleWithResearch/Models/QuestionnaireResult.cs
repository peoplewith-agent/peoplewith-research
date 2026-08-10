using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class QuestionnaireResult
    {
        public string QuestionId { get; set; }
        public string AnswerId { get; set; }

        public string AnswerValue { get; set; }
        public string InternalName { get; set; }
    }
}
