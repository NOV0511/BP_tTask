using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class Task
    {
        public Task()
        {
            TaskUserComment = new HashSet<TaskUserComment>();
            UserTask = new HashSet<UserTask>();
        }

        public int IdTask { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Completed { get; set; }
        public DateTime Deadline { get; set; }
        public string Priority { get; set; }
        public int IdUser { get; set; }
        public int IdProject { get; set; }

        public virtual Project IdProjectNavigation { get; set; }
        public virtual User IdUserNavigation { get; set; }
        public virtual ICollection<TaskUserComment> TaskUserComment { get; set; }
        public virtual ICollection<UserTask> UserTask { get; set; }
    }
}
