using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class Tenant
    {
        public Tenant()
        {
            ServiceOrder = new HashSet<ServiceOrder>();
            User = new HashSet<User>();
        }

        public int IdTenant { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }

        public virtual ICollection<ServiceOrder> ServiceOrder { get; set; }
        public virtual ICollection<User> User { get; set; }
    }
}
