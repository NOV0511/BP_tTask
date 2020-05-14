using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;
using tTask.ViewModels;
using tTask.Models.Forms;

namespace tTask.Controllers
{
    [Authorize(Roles = nameof(Roles.NormalUser))]
    [Authorize(Policy = "TenantPolicy")]
    public class ProjectController : Controller
    {
        private readonly TaskTable _taskTable;
        private readonly ProjectTable _projectTable;
        private readonly UserTable _userTable;
        private readonly TenantTable _tenantTable;
        private readonly ServiceOrderTable _serviceOrderTable;
        private readonly NotificationTable _notificationTable;

        public ProjectController(TaskTable taskTable, ProjectTable projectTable, UserTable userTable,
            TenantTable tenantTable, ServiceOrderTable serviceOrderTable, NotificationTable notificationTable)
        {
            _taskTable = taskTable;
            _projectTable = projectTable;
            _userTable = userTable;
            _tenantTable = tenantTable;
            _serviceOrderTable = serviceOrderTable;
            _notificationTable = notificationTable;
        }

        public IActionResult Index(int idProject)
        {
            var idUser = _userTable.GetUserId(HttpContext.User.Identity.Name);

            if (_projectTable.UserCanViewProject(idUser, idProject))
            {
                var project = _projectTable.GetProjectById(idProject);
                var model = new ProjectViewModel()
                {
                    Project = project,
                    UserProjects = _projectTable.GetUserProjectsByProjectId(idProject),
                    Tasks = _taskTable.GetTasksByProjectId(idProject).OrderByDescending(t => t.Created),
                    IdSignedUser = idUser,
                    IdRoleSignedUserProject = _projectTable.GetRoleOfUser(idUser, idProject),
                    UsersOutOfProject = new List<SelectListItem>(),
                    UserInProject = new List<SelectListItem>(),
                    Priority = new List<SelectListItem>(),
                    IdService = _serviceOrderTable.GetInUseServiceIdByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string)),
                    SettingsColoring = _userTable.GetSettingsbyUserId(idUser).Coloring == "1" ? true : false,
                    PageTitle = project.Name,
                    PageDescription = project.Description,
                    TaskForm = new TaskForm()
                };
                foreach (var user in _userTable.GetUsersOutOfProjectByProjectId(idProject))
                {
                    var text = user.FirstName + " " + user.Surname + " - " + user.Email;
                    model.UsersOutOfProject.Add(new SelectListItem { Text = text, Value = user.Id.ToString() });
                }
                foreach (var user in _userTable.GetUsersInProjectByProjectId(idProject))
                {
                    var text = user.FirstName + " " + user.Surname + " - " + user.Email;
                    model.UserInProject.Add(new SelectListItem { Text = text, Value = user.Id.ToString() });
                }
                foreach (string name in Enum.GetNames(typeof(Priority)))
                {
                    model.Priority.Add(new SelectListItem { Text = name, Value = name });
                }
                return View(model);
            }
            return RedirectToAction("Index", "HomePage");
        }

      [Authorize(Roles = nameof(Roles.Manager))]
        [HttpPost]
        public IActionResult AddNewProject(string projectName, string projectDescription)
        {
            var project = new Project()
            {
                IdProject = _projectTable.GetMaxId(),
                Name = projectName,
                Description = projectDescription
            };
            _projectTable.NewProject(project, _userTable.GetUserId(HttpContext.User.Identity.Name));
            return RedirectToAction("index", new { idProject = project.IdProject });
        }

        [HttpPost]
        public IActionResult UpdateProject(int idProject, string projectName, string projectDescription)
        {
            var project = new Project()
            {
                IdProject = idProject,
                Name = projectName,
                Description = projectDescription
            };
            _projectTable.UpdateProject(project);
            return RedirectToAction("index", new { idProject = project.IdProject });
        }

        [HttpPost]
        public IActionResult DeleteProject(int idProject)
        {
           
            _projectTable.DeleteProject(idProject);
            return RedirectToAction("index", "HomePage");
        }

        [HttpPost]
        public void ChangeRole(int idUser, int idProject)
        {
            var idSignedUser = _userTable.GetUserId(HttpContext.User.Identity.Name);
            if (_projectTable.UserCanControlProject(idSignedUser, idProject))
            {
                var up = _projectTable.GetUserProjectByIds(idUser, idProject);
                if (up.IdRole == (int)Roles.ProjectLeader)
                {
                    up.IdRole = (int)Roles.ProjectUser;
                    var msg = "You have been demoted in project " + _projectTable.GetProjectById(idProject).Name;
                    _notificationTable.NotifyUser(idUser, msg);
                }
                else if (up.IdRole == (int)Roles.ProjectUser) { 
                    up.IdRole = (int)Roles.ProjectLeader;
                    var msg = "You have been promoted in project " + _projectTable.GetProjectById(idProject).Name;
                    _notificationTable.NotifyUser(idUser, msg);

                }
                _projectTable.UpdateUserProject(up);
            }
        }
        [HttpPost]
        public IActionResult AddUser(int idUser, int idProject)
        {
            var idSignedUser = _userTable.GetUserId(HttpContext.User.Identity.Name);
            if (_projectTable.UserCanControlProject(idSignedUser, idProject))
            {
                var up = new UserProject()
                {
                    IdUser = idUser,
                    IdProject = idProject,
                    IdRole = (int)Roles.ProjectRequest,
                    Active = "1"
                };
                _projectTable.InsertUserProject(up);
                var msg = "You have been invited to project " + _projectTable.GetProjectById(idProject).Name;
                _notificationTable.NotifyUser(idUser, msg);
            }

            return RedirectToAction("Index", new { idProject });
        }

        [HttpGet]
        public IActionResult CanAdd()
        {
            var idService = _serviceOrderTable.GetInUseServiceIdByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string));
            var noProjects = _projectTable.GetNOProject();

            if ((idService == (int)Services.Basic && noProjects >= 1) || (idService == (int)Services.Pro && noProjects >= 5))
                return Json(false);

            return Json(true);
        }
    }
}