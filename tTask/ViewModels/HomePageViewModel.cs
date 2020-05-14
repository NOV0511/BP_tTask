using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.ORM.DTO;

namespace tTask.ViewModels
{
    public class HomePageViewModel : BasePageViewModel
    {
        public User User { get; set; }
        public bool ExistsProject { get; set; }
        public int IdService { get; set; }
        public bool SettingsCustom { get; set; }

        public HomePageViewModel()
        {
            PageTitle = "Add your project!";
            PageDescription =
                "A basic account can add 1 project in maximum. For more unlimited amount of projects upgrade your account plan.";
        }
    }
}
