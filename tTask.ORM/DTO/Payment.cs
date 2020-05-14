using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class Payment
    {
        public int IdPayment { get; set; }
        public int Price { get; set; }
        public DateTime? Paid { get; set; }

        public virtual ServiceOrder ServiceOrder { get; set; }
    }
}
