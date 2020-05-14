using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace tTask.Models.Forms
{
    public class TaskForm
    {
        [Display(Name = "Name")]
        [StringLength(100)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        public string Name { get; set; }
        [Display(Name = "Description")]
        [StringLength(1000)]
        public string Description { get; set; }
        [Display(Name = "Priority")]
        public string Priority { get; set; }
        [Display(Name = "Deadline")]
        [DataType(DataType.DateTime)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        public DateTime Deadline { get; set; }
        [Display(Name = "Assigned to")]
        [Required(ErrorMessage = "You have to choose at least one person!")]
        public int[] AssignedTo { get; set; }
        public int? IdProject { get; set; }
        public int? IdTask { get; set; }
        public string From { get; set; }
    }
}
