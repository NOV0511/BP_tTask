using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using tTask.ORM.DTO;

namespace tTask.ORM.DAO
{
    public class TenantTable
    {
        private SharedDbContext db;
        private readonly ServiceOrderTable _serviceOrderTable;

        public TenantTable(SharedDbContext db, ServiceOrderTable serviceOrderTable)
        {
            this.db = db;
            _serviceOrderTable = serviceOrderTable;
        }
        public ICollection<string> GetAllDomains()
        {
            var output = new Collection<string>();
            var tenants = db.Tenant;
            foreach (var t in tenants)
            {
                output.Add(t.Domain.ToLower());
            }
            output.Add("default");
            return output;
        }

        public int GetTenantId(string domain)
        {
            var tenant = db.Tenant.Where(t => t.Domain == domain).FirstOrDefault();
            return tenant.IdTenant;
        }

        public bool TenantAlreadyExists(string domain)
        {
            var tenant = db.Tenant.Where(t => t.Domain == domain).FirstOrDefault();
            if (tenant == null) return false;
            return true;
        }

        public ICollection<Tenant> GetAllTenants()
        {
            var tenants = db.Tenant.ToList();

            foreach (var t in tenants)
            {
                t.ServiceOrder.Add(_serviceOrderTable.GetNewestServiceOrderToTenantByTenantId(t.IdTenant));
            }

            return tenants;
        }
    }
}
