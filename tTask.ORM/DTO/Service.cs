using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class Service
    {
        public Service()
        {
            ServiceOrder = new HashSet<ServiceOrder>();
        }

        public int IdService { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<ServiceOrder> ServiceOrder { get; set; }
    }
}
