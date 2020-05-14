using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class UserNotification
    {
        public int IdNotification { get; set; }
        public string Text { get; set; }
        public string Read { get; set; }
        public DateTime Created { get; set; }
        public int IdUser { get; set; }

        public virtual User IdUserNavigation { get; set; }
    }
}
