using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class Project
    {
        public Project()
        {
            Task = new HashSet<Task>();
            UserProject = new HashSet<UserProject>();
        }

        public int IdProject { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Task> Task { get; set; }
        public virtual ICollection<UserProject> UserProject { get; set; }
    }
}
