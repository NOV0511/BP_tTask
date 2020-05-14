using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using tTask.Models.Forms;
using tTask.ORM.DTO;

namespace tTask.ViewModels
{
    public class TasksViewModel : BasePageViewModel
    {
        public ICollection<Task> TasksExpired { get; set; }
        public ICollection<Task> TasksToday { get; set; }
        public ICollection<Task> TasksWeek { get; set; }
        public ICollection<Task> TasksMonth { get; set; }
        public ICollection<Task> TasksOther { get; set; }
        public List<SelectListItem> UsersProjects { get; set; }
        public List<SelectListItem> Priority { get; set; }
        public int IdService { get; set; }
        public bool SettingsColoring { get; set; }
        public TaskForm TaskForm { get; set; }

        public IEnumerable<Task> UpcommingTasks { get; set; }
        public IEnumerable<Task> ExpiredTasks { get; set; }
        public IEnumerable<Task> CompletedTasks { get; set; }

        public TasksViewModel()
        {
            PageTitle = "Manage your all tasks!";
            PageDescription = "Add new tasks, manage your assigned tasks. View your planned tasks or completed tasks.";
        }

    }
}
