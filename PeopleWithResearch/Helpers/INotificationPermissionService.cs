using System;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public interface INotificationPermissionService
    {
        Task<bool> RequestNotificationPermissions();
    }
}

