using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class UserProject
    {
        public string Active { get; set; }
        public int IdUser { get; set; }
        public int IdRole { get; set; }
        public int IdProject { get; set; }

        public virtual Project IdProjectNavigation { get; set; }
        public virtual Role IdRoleNavigation { get; set; }
        public virtual User IdUserNavigation { get; set; }
    }
}
