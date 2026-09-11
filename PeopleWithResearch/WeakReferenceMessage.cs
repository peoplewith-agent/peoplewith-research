using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeopleWithResearch;

namespace PeoplewithResearch
{
    public class refreshdashafterquestionnaire : ValueChangedMessage<ObservableCollection<UserQuestionnaire>>
    {
        public refreshdashafterquestionnaire(ObservableCollection<UserQuestionnaire>? Qpassed) : base(Qpassed)
        {
        }
    }

    public class updateuserdetails : ValueChangedMessage<ObservableCollection<user>>
    {
        public updateuserdetails(ObservableCollection<user>? user) : base(user)
        {
        }
    }
   public class VideoPageLeft : ValueChangedMessage<string>
    {
        public VideoPageLeft(string value) : base(value)
        {
        }
    }

      public class refreshname : ValueChangedMessage<string>
    {
        public refreshname(string value) : base(value)
        {
        }
    }

    public class ReloadProfileMessage : ValueChangedMessage<string>
    {
        public ReloadProfileMessage(string? value) : base(value)
        {
        }
    }

}
