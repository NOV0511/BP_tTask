using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tTask.Models;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ViewModels;

namespace tTask.Controllers
{
    [Authorize(Roles = nameof(Roles.NormalUser))]
    [Authorize(Policy = "TenantPolicy")]
    public class PeopleController : Controller
    {
        private readonly ProjectTable _projectTable;
        private readonly UserTable _userTable;

        public PeopleController(ProjectTable projectTable, UserTable userTable)
        {
            _projectTable = projectTable;
            _userTable = userTable;
        }
        public IActionResult Index()
        {
            var idUser = _userTable.GetUserId(HttpContext.User.Identity.Name);
            var model = new PeopleViewModel()
            {
                Projects = new List<ProjectPeopleModel>()
            };
            var projects = _projectTable.GetProjectsByUserId(idUser);
            foreach (var project in projects)
            {
                var pp = new ProjectPeopleModel()
                {
                    ProjectName = project.Name,
                    ProjectUsers = _userTable.GetUsersInProjectByProjectId(project.IdProject)
                };
                model.Projects.Add(pp);
            }
            return View(model);
        }
    }
}