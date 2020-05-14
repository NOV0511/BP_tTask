using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tTask.ViewModels
{
    public class PageHeaderViewModel
    {
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public BasePageViewModel PageModel  { get; set; }
    }
}
