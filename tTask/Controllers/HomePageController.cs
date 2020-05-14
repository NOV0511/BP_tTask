using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;
using tTask.ViewModels;

namespace tTask.Controllers
{
    [Authorize(Roles = nameof(Roles.NormalUser))]
    [Authorize(Policy = "TenantPolicy")]
    public class HomePageController : Controller
    {
        private readonly UserTable _userTable;
        private readonly ProjectTable _projectTable;
        private readonly TenantTable _tenantTable;
        private readonly ServiceOrderTable _serviceOrderTable;


        public HomePageController(UserTable userTable, ProjectTable projectTable, TenantTable tenantTable, ServiceOrderTable serviceOrderTable)
        {
            _userTable = userTable;
            _projectTable = projectTable;
            _tenantTable = tenantTable;
            _serviceOrderTable = serviceOrderTable;
        }

        
        public IActionResult Index()
        {
            if(HttpContext.Request.Cookies["Identity.Domain"] != HttpContext.Items["domain"] as string)
            {
                return RedirectToAction("LogOut", "Home");
            }
            var model = new HomePageViewModel
            {
                User = _userTable.GetUserWithProjects(HttpContext.User.Identity.Name),
                ExistsProject = false,
                IdService = _serviceOrderTable.GetInUseServiceIdByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string)),
                SettingsCustom = _userTable.GetSettingsbyUserId(_userTable.GetUserId(HttpContext.User.Identity.Name)).CustomizeView == "1"
            };
            foreach (var p in model.User.UserProject)
            {
                if (p.IdRole == (int)Roles.ProjectUser || p.IdRole == (int)Roles.ProjectLeader)
                {
                    model.ExistsProject = true;
                    break;
                }
            }
            return View(model);
            
        }

        [HttpPost]
        public void ProjectRequest (int idProject, string action)
        {
            int idUser = _userTable.GetUserId(HttpContext.User.Identity.Name);
            var up = _projectTable.GetUserProjectByIds(idUser, idProject);
            if (action == "confirm")
            {
                up.IdRole = (int)Roles.ProjectUser;
                _projectTable.UpdateUserProject(up);
            }
            else
            {
                _projectTable.DeleteUserProject(up);
            }

        }

        [HttpPost]
        public void ChangeProjectState(int idProject, int idUser, int idRole, string checkedValue)
        {
            var userProject = new UserProject()
            {
                IdProject = idProject,
                IdUser = idUser,
                IdRole = idRole,
                Active = checkedValue
            };
            _projectTable.UpdateUserProject(userProject);
        }
    }
}