using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.ORM.DTO;

namespace tTask.Models
{
    public class ProjectPeopleModel
    {
        public string ProjectName { get; set; }
        public ICollection<User> ProjectUsers { get; set; }
    }
}
