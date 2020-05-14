using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tTask.ORM.DTO;

namespace tTask.ORM.DAO
{
    public class PaymentTable
    {
        private readonly SharedDbContext _db;

        public PaymentTable(SharedDbContext db)
        {
            _db = db;
        }

        public int GetMaxId()
        {
            var max = 0;
            int? val = _db.Payment.Max(p => (int?)p.IdPayment);
            if (val != null)
            {
                max = (int)val;
            }

            return max + 1;
        }

        public void NewPayment(Payment p)
        {
            _db.Payment.Add(p);
            _db.SaveChanges();
        }

        public void UpdatePayment(Payment p)
        {
            _db.Update<Payment>(p);
            _db.SaveChanges();
        }
    }
}
