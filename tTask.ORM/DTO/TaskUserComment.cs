using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace tTask.ORM.DTO
{
    public partial class TaskUserComment
    {
        public int IdComment { get; set; }
        public DateTime Created { get; set; }
        public string Text { get; set; }
        public int IdUser { get; set; }
        public int IdTask { get; set; }

        public virtual Task IdTaskNavigation { get; set; }
        public virtual User IdUserNavigation { get; set; }
    }
}
