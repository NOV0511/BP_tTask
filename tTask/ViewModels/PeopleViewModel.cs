using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.Models;
using tTask.ORM.DTO;

namespace tTask.ViewModels
{
    public class PeopleViewModel : BasePageViewModel
    {
        public ICollection<ProjectPeopleModel> Projects { get; set; }

        public PeopleViewModel()
        {
            PageTitle = "People";
            PageDescription = "Browse the contacts of all the people you met on the projects. For contact details, click on the user profile icon.";
        }

    }
}
