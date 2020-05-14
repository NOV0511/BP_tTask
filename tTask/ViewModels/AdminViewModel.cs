﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.ORM.DTO;

namespace tTask.ViewModels
{
    public class AdminViewModel
    {
        public ICollection<Tenant> Tenants { get; set; }
    }
}
