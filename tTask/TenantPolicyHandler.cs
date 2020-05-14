using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.ORM;
using tTask.ORM.DAO;

namespace tTask
{
    public class TenantPolicyHandler : AuthorizationHandler<TenantPolicyRequirement>
    {
        private readonly ServiceOrderTable _serviceOrderTable;
        private readonly TenantTable _tenantTable;
        private readonly HttpContext _httpContext;
        private readonly UserManager<ORM.DTO.User> _userMgr;

        public TenantPolicyHandler(ServiceOrderTable so, TenantTable tt, IHttpContextAccessor httpContextAccessor, UserManager<ORM.DTO.User> userManager)
        {
            _serviceOrderTable = so;
            _tenantTable = tt;
            _httpContext = httpContextAccessor.HttpContext;
            _userMgr = userManager;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantPolicyRequirement requirement)
        {
            if (CorrectDomain())
            {
                if (CorrectUser() || _httpContext.Request.Path.Value.Contains("SignIn") || _httpContext.Request.Path.Value.Contains("SignUp"))
                {
                    var serviceOrder = _serviceOrderTable.GetNewestServiceOrderToTenantByTenantId(_tenantTable.GetTenantId(_httpContext.Items["domain"] as string));
                    if (serviceOrder.IdService == (int)Services.Basic)
                        context.Succeed(requirement);
                    else if (serviceOrder.IdPaymentNavigation.Paid.HasValue)
                    {
                        if ((serviceOrder.IdPaymentNavigation.Price == serviceOrder.IdServiceNavigation.Price &&
                            serviceOrder.IdPaymentNavigation.Paid.Value.AddDays(30) > DateTime.Now) || _httpContext.Request.Path.Value.Contains("Profile") || _httpContext.Request.Path.Value.Contains("Service"))
                            context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public bool CorrectDomain()
        {
            var tmp = _httpContext.Items["domain"] as string;
            var domainList = _tenantTable.GetAllDomains();
            if (domainList.Contains(tmp) && tmp != "default")
            {
                return true;
            }
            return false;
        }

        public  bool CorrectUser()
        {
            if (_httpContext.Request.Cookies["Identity.Domain"] == _httpContext.Items["domain"] as string) return true;
            return false;
        }
    }
}
