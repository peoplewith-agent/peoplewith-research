using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class householdstudyrecord
    {
        public string household_id { get; set; }
        public string current_phase { get; set; }
        public List<TEvent> t_events { get; set; } = new();
    }

    public class TEvent
    {
        public string t_event_id { get; set; }
        public string t_event_status { get; set; }
        public string scenario { get; set; }

        public string trigger_type { get; set; }  // "reactive" or "preemptive"
        public string t1_start_date { get; set; }
        public List<TEventMember> members { get; set; } = new();
    }

    public class TEventMember
    {
        public string user_id { get; set; }
        public bool daily_forms_complete { get; set; }
        public string daily_forms_stopped_at { get; set; }
        public List<TQuestionnaire> questionnaires { get; set; } = new();
    }

    public class TQuestionnaire
    {
        public string questionnaire_type { get; set; }
        public string date_completed { get; set; }
        public bool has_symptoms { get; set; }
        public bool lfd_result { get; set; }

        public bool latesubmission { get; set; }
    }
}
