using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using tTask.ORM.DTO;

namespace tTask.ViewModels
{
    public class TenantViewModel : BasePageViewModel
    {
        public ICollection<User> AllUsers { get; set; }
        public ICollection<User> Managers { get; set; }
        public ICollection<Project> AllProject { get; set; }
        public List<SelectListItem> UsersNotManager { get; set; }

        public TenantViewModel()
        {
            PageTitle = "People & Projects";
            PageDescription = "Manage all your users on the team. You can promote your users to the level of managers. View and manage all projects.";
        }

    }
}
