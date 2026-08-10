using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class UpdateProfile : ValueChangedMessage<string>
    {
        public UpdateProfile(string value) : base(value)
        {
        }
    }

    public class UpdateHouseHoldGroup : ValueChangedMessage<ObservableCollection<householdgroupjsondetails>>
    {
        public UpdateHouseHoldGroup(ObservableCollection<householdgroupjsondetails> value) : base(value)
        {
        }
    }
}
