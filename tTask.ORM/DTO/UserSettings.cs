using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class UserSettings
    {
        public string Notifications { get; set; }
        public string Coloring { get; set; }
        public string CustomizeView { get; set; }
        public int IdUser { get; set; }

        public virtual User IdUserNavigation { get; set; }
    }
}
