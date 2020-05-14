using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tTask.ORM.DTO;

namespace tTask.ORM.DAO
{
    public class ServiceOrderTable
    {
        private readonly SharedDbContext _db;

        public ServiceOrderTable(SharedDbContext db)
        {
            _db = db;
        }

        public ServiceOrder GetNewestServiceOrderToTenantByTenantId(int id)
        {
            var so = _db.ServiceOrder.Where(t => t.IdTenant == id)
                                     .Where(d => d.OrderDate == _db.ServiceOrder.Where(t => t.IdTenant == id).Max(d => d.OrderDate))
                                     .Include(p => p.IdPaymentNavigation)
                                     .Include(s => s.IdServiceNavigation)
                                     .FirstOrDefault();
            return so;
        }

        public int GetInUseServiceIdByTenantId(int id)
        {
            var so = _db.ServiceOrder.Where(t => t.IdTenant == id)
                                     .Where(d => d.OrderDate == _db.ServiceOrder.Where(t => t.IdTenant == id).Max(d => d.OrderDate))
                                     .FirstOrDefault();
            return so.IdService;
        }

        public Service GetServiceById(int id)
        {
            return _db.Service.Find(id);
        }

        public void NewServiceOrder(ServiceOrder so)
        {
            _db.ServiceOrder.Add(so);
            _db.SaveChanges();
        }

        public int GetDaysToExpireByTenantId(int id)
        {
            var so = _db.ServiceOrder.Where(t => t.IdTenant == id)
                                     .Where(d => d.OrderDate == _db.ServiceOrder.Where(t => t.IdTenant == id).Max(d => d.OrderDate))
                                     .Include(p => p.IdPaymentNavigation)
                                     .Include(s => s.IdServiceNavigation)
                                     .FirstOrDefault();
            var noDays = 99;
            if (so.IdService != 1 && so.IdPaymentNavigation.Paid.HasValue)
            {
                noDays = (int)(so.IdPaymentNavigation.Paid.Value.AddDays(30) - DateTime.Now).TotalDays;
            }
            return noDays;
        }
    }
}
