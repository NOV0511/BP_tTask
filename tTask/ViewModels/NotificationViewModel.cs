using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.Models.Forms;
using tTask.ORM.DTO;

namespace tTask.ViewModels
{
    public class NotificationViewModel : BasePageViewModel
    {
        public IEnumerable<UserProject> ProjectRequests { get; set; }
        public IEnumerable<UserNotification> UnreadNotifications { get; set; }
        public IEnumerable<UserNotification> ReadNotifications { get; set; }
        public bool NotificationSetting { get; set; }

        public NotificationViewModel()
        {
            PageTitle = "Notification";
            PageDescription = "Keep all notifications together. At the same time, you can accept / decline the invitation to the project.";
        }

    }
}
