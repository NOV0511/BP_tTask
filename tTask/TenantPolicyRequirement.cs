using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;

namespace tTask
{
    public class TenantPolicyRequirement : IAuthorizationRequirement
    {

    }
}
