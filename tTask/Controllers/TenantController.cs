using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;
using tTask.ViewModels;

namespace tTask.Controllers
{
    [Authorize(Roles = nameof(Roles.DomainAdmin))]
    [Authorize(Policy = "TenantPolicy")]
    public class TenantController : Controller
    {
        private readonly UserTable _userTable;
        private readonly UserManager<User> _userMgr;
        private readonly ProjectTable _projectTable;
        private readonly NotificationTable _notificationTable;

        public TenantController(UserTable userTable, UserManager<User> userMgr, ProjectTable projectTable, NotificationTable notificationTable)
        {
            _userTable = userTable;
            _userMgr = userMgr;
            _projectTable = projectTable;
            _notificationTable = notificationTable;
        }
        public async Task<IActionResult> Index()
        {
            var signedUserId = _userTable.GetUserId(HttpContext.User.Identity.Name);
            var model = new TenantViewModel()
            {
                AllUsers = _userTable.GetAllUsers(),
                AllProject = _projectTable.GetAllProjects(),
                UsersNotManager = new List<SelectListItem>(),
                Managers = new List<User>()
            };
            foreach (var user in model.AllUsers)
            {
                if(!await _userMgr.IsInRoleAsync(user, nameof(Roles.Manager)))
                {
                    var text = user.FirstName + " " + user.Surname + " - " + user.Email;
                    model.UsersNotManager.Add(new SelectListItem { Text = text, Value = user.Id.ToString() });
                }
                else if (user.Id != signedUserId)
                {
                    model.Managers.Add(user);
                }

            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PromoteUser(int idUser)
        {
            var user = _userTable.GetUserById(idUser);
            await _userMgr.AddToRoleAsync(user, nameof(Roles.Manager));

            var msg = "You have been promoted to manager! You can create your own projects now.";
            _notificationTable.NotifyUser(idUser, msg);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DemoteUser(int idUser)
        {
            var user = _userTable.GetUserById(idUser);
            await _userMgr.RemoveFromRoleAsync(user, nameof(Roles.Manager));

            var msg = "You have been demoted to user! You cannot create your own projects now.";
            _notificationTable.NotifyUser(idUser, msg);

            return RedirectToAction("Index");
        }
    }
}