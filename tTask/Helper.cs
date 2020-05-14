using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.ORM.DAO;

namespace tTask
{
    public class Helper
    {
        private readonly HttpContext _httpContext;
        private readonly TenantTable _tenantTable;


        public Helper(IHttpContextAccessor httpContextAccessor, TenantTable tenantTable)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _tenantTable = tenantTable;
        }
        public bool CorrectDomain()
        {
            var tmp = _httpContext.Items["domain"] as string;
            var domainList = _tenantTable.GetAllDomains();
            if (domainList.Contains(tmp))
            {
                return true;
            }
            return false;
        }


    }
}
