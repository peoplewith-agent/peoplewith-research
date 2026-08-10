using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class UpdateDashCompelted : ValueChangedMessage<List<newuserquestionnaire>>
    {
        public UpdateDashCompelted(List<newuserquestionnaire> AllAnswers) : base(AllAnswers)
        {
        }
    }

    public class RefreshNotificationDash : ValueChangedMessage<string>
    {
        public RefreshNotificationDash(string value) : base(value)
        {
        }
    }
}
