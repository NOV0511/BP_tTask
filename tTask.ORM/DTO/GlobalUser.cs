using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace tTask.ORM.DTO
{
    public partial class GlobalUser : IdentityUser<int>
    {
        public override int Id { get; set; }
        public override string Email { get; set; }
    }
}
