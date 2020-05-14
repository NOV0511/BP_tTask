using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.ORM.DTO;

namespace tTask.ViewModels
{
    public class ServiceViewModel
    {
        public ServiceOrder ServiceTenantOrder { get; set; }
        public Service Basic { get; set; }
        public Service Pro { get; set; }
        public Service Business { get; set; }
    }
}
