using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tTask.Models;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;
using tTask.ViewModels;

namespace tTask.Controllers
{
    [Authorize(Roles = nameof(Roles.NormalUser))]
    [Authorize(Policy = "TenantPolicy")]
    public class NotificationController : Controller
    {
        private readonly ServiceOrderTable _serviceOrderTable;
        private readonly TenantTable _tenantTable;
        private readonly UserTable _userTable;
        private readonly NotificationTable _notificationTable;
        private readonly ProjectTable _projectTable;

        public NotificationController(ServiceOrderTable serviceOrderTable, TenantTable tenantTable, UserTable userTable, NotificationTable notificationTable, ProjectTable projectTable)
        {
            _serviceOrderTable = serviceOrderTable;
            _tenantTable = tenantTable;
            _userTable = userTable;
            _notificationTable = notificationTable;
            _projectTable = projectTable;
        }

        public IActionResult Index()
        {
            var idUser = _userTable.GetUserId(HttpContext.User.Identity.Name);
            var model = new NotificationViewModel()
            {
                ProjectRequests = _projectTable.GetUserProjectRequestsByUserId(idUser),
                ReadNotifications = _notificationTable.GetReadNotifiactionsByUserId(idUser).OrderByDescending(n => n.IdNotification),
                UnreadNotifications = _notificationTable.GetUnreadNotifiactionsByUserId(idUser).OrderByDescending(n => n.IdNotification),
                NotificationSetting = _userTable.GetSettingsbyUserId(_userTable.GetUserId(HttpContext.User.Identity.Name)).Notifications == "1"
            };
            return View(model);
        }
 
        [HttpGet]
        public IActionResult UserUseNotifications()
        {
            int idService = _serviceOrderTable.GetInUseServiceIdByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string));
            bool notificationSetting = _userTable.GetSettingsbyUserId(_userTable.GetUserId(HttpContext.User.Identity.Name)).Notifications == "1";

            if (idService != (int)Services.Basic && notificationSetting)
                return Json(true);

            return Json(false);
        }

        [HttpGet]
        public IActionResult GetUnreadNotifications()
        {
            var notifications = _notificationTable.GetUnreadNotifiactionsByUserId(_userTable.GetUserId(HttpContext.User.Identity.Name)).OrderByDescending(n => n.IdNotification);
            return Json(notifications);
        }

        [HttpGet]
        public IActionResult GetCountUnread()
        {
            int count = _notificationTable.GetCountUnreadByUserId(_userTable.GetUserId(HttpContext.User.Identity.Name));
            return Json(count);
        }

        [HttpPost]
        public void ReadNotification(int idNotification)
        {
            _notificationTable.ReadNotification(idNotification);
        }

        [HttpPost]
        public void ReadAllNotifications()
        {
            var idUser = _userTable.GetUserId(HttpContext.User.Identity.Name);
            _notificationTable.ReadAllNotifications(idUser);
        }


        public bool HelperUserUseNotifications()
        {
            int idService = _serviceOrderTable.GetInUseServiceIdByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string));
            bool notificationSetting = _userTable.GetSettingsbyUserId(_userTable.GetUserId(HttpContext.User.Identity.Name)).Notifications == "1";

            return (idService != (int)Services.Basic && notificationSetting);
        }

        [HttpGet]
        public IActionResult GetNOProjectRequests()
        {
            var idUser = _userTable.GetUserId(HttpContext.User.Identity.Name);
            return Json(_projectTable.GetNOProjectRequest(idUser));
        }
    }
}