using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using tTask.Models.Forms;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;
using tTask.ViewModels;

namespace tTask.Controllers
{
    [Authorize(Roles = nameof(Roles.NormalUser))]
    [Authorize(Policy = "TenantPolicy")]
    public class ProfileController : Controller
    {
        private readonly UserTable _userTable;
        private readonly UserManager<User> _userMgr;
        private readonly TenantTable _tenantTable;
        private readonly ServiceOrderTable _serviceOrderTable;

        public ProfileController(UserTable userTable, UserManager<User> userMgr, TenantTable tenantTable, ServiceOrderTable serviceOrderTable)
        {
            _userTable = userTable;
            _userMgr = userMgr;
            _tenantTable = tenantTable;
            _serviceOrderTable = serviceOrderTable;
        }

        public IActionResult Index()
        {
            var model = new ProfileViewModel
            {
                User = _userTable.GetUserWithSettingsById(_userTable.GetUserId(HttpContext.User.Identity.Name)),
                ChangePassowrdForm = new ChangePasswordForm(),
                IdService = _serviceOrderTable.GetInUseServiceIdByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string)),
                ServiceTenantOrder = _serviceOrderTable.GetNewestServiceOrderToTenantByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string)),
                Basic = _serviceOrderTable.GetServiceById((int)Services.Basic),
                Pro = _serviceOrderTable.GetServiceById((int)Services.Pro),
                Business = _serviceOrderTable.GetServiceById((int)Services.Business)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoto(IFormFile img)
        {
            var domain = HttpContext.Items["domain"];
            var idUser = _userTable.GetUserId(HttpContext.User.Identity.Name);
            var path = "/img/profile/" + domain + "/" + idUser + ".jpg";

            if (!Directory.Exists($"wwwroot/img/profile/{domain}"))
            {
                Directory.CreateDirectory($"wwwroot/img/profile/{domain}");
            }

            if (img.Length > 0)
            {
                var filePath = "wwwroot" + path;

                using (var stream = System.IO.File.Create(filePath))
                {
                    await img.CopyToAsync(stream);
                }
            }

            var user = _userTable.GetUserById(idUser);
            user.Photopath = path;

            _userTable.UpdateUser(user);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ProfileViewModel model)
        {
            var user = _userTable.GetUserById(_userTable.GetUserId(HttpContext.User.Identity.Name));
            if (ModelState.IsValid)
            {
                var result = await _userMgr.ChangePasswordAsync(user, model.ChangePassowrdForm.CurrentPassword, model.ChangePassowrdForm.NewPassword);
                if(result.Succeeded)
                {
                    return RedirectToAction("index");
                }
                else
                {
                    ModelState.AddModelError("CustomErr", $"Current password is wrong!");
                }
            }
            model.User = user;


            model.User = _userTable.GetUserWithSettingsById(_userTable.GetUserId(HttpContext.User.Identity.Name));
            model.IdService = _serviceOrderTable.GetInUseServiceIdByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string));
            model.ServiceTenantOrder = _serviceOrderTable.GetNewestServiceOrderToTenantByTenantId(_tenantTable.GetTenantId(HttpContext.Items["domain"] as string));
            model.Basic = _serviceOrderTable.GetServiceById((int)Services.Basic);
            model.Pro = _serviceOrderTable.GetServiceById((int)Services.Pro);
            model.Business = _serviceOrderTable.GetServiceById((int)Services.Business);
            

            return View("Index", model);
        }

        [HttpPost]
        public void ChangeSettings(string name, bool checkValue)
        {
            var settings = _userTable.GetSettingsbyUserId(_userTable.GetUserId(HttpContext.User.Identity.Name));
            if (name == "notifications")
                settings.Notifications = checkValue ? "1" : "0";
            else if (name == "coloring")
                settings.Coloring = checkValue ? "1" : "0";
            else if (name == "custom")
                settings.CustomizeView = checkValue ? "1" : "0";

            _userTable.UpdateSettings(settings);
        }
    }
}