using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.Models.Forms;
using tTask.ORM.DTO;

namespace tTask.ViewModels
{
    public class ProfileViewModel : BasePageViewModel
    {
        public User User { get; set; }
        public ChangePasswordForm ChangePassowrdForm { get; set; }
        public int IdService { get; set; }
        public ServiceOrder ServiceTenantOrder { get; set; }
        public Service Basic { get; set; }
        public Service Pro { get; set; }
        public Service Business { get; set; }

        public ProfileViewModel()
        {
            PageTitle = "User profile";
            PageDescription = "Set your user information. Track the status of your subscription.";
        }
    }
}
