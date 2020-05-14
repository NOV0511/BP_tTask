using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class UserTask
    {
        public DateTime? Completed { get; set; }
        public int IdUser { get; set; }
        public int IdTask { get; set; }

        public virtual Task IdTaskNavigation { get; set; }
        public virtual User IdUserNavigation { get; set; }
    }
}
