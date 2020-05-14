using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class ServiceOrder
    {
        public DateTime OrderDate { get; set; }
        public int IdTenant { get; set; }
        public int IdService { get; set; }
        public int IdPayment { get; set; }

        public virtual Payment IdPaymentNavigation { get; set; }
        public virtual Service IdServiceNavigation { get; set; }
        public virtual Tenant IdTenantNavigation { get; set; }
    }
}
