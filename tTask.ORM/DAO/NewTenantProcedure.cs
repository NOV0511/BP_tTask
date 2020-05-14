using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace tTask.ORM.DAO
{
    public class NewTenantProcedure
    {
        /*        private NewTenantDbContext db;
                public NewTenantProcedure(NewTenantDbContext db)
                {
                    this.db = db;
                }*/

        private readonly SharedDbContext _db;
        public NewTenantProcedure(Func<bool, SharedDbContext> dbContextFunction) {
            _db = dbContextFunction(true);
        }

        public void NewTenant(string name, string domain)
        {
            _db.Database.ExecuteSqlRaw("PNewTenant @p0, @p1, @p2", parameters: new[] { name, Domain.GetDomainHash(domain), domain });

        }
    }
}
