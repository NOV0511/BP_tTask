using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using tTask.Models.Forms;
using tTask.ORM.DTO;

namespace tTask.ViewModels
{
    public class ProjectViewModel : BasePageViewModel
    {
        public Project Project { get; set; }
        public ICollection<UserProject> UserProjects { get; set; }
        public IEnumerable<Task> Tasks { get; set; }
        public int IdSignedUser { get; set; }
        public int IdRoleSignedUserProject { get; set; }
        public List<SelectListItem> UsersOutOfProject { get; set; }
        public List<SelectListItem> UserInProject { get; set; }
        public List<SelectListItem> Priority { get; set; }
        public int IdService { get; set; }
        public bool SettingsColoring { get; set; }
        public TaskForm TaskForm { get; set; }
    }
}
