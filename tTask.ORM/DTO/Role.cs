using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class Role : IdentityRole<int>
    {
        public Role()
        {
            UserProject = new HashSet<UserProject>();
        }

        public override int Id { get; set; }
        public override string Name { get; set; }

        public virtual ICollection<UserProject> UserProject { get; set; }
    }
}
