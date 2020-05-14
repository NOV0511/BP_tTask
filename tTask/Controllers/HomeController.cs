using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using tTask.Models;
using tTask.Models.Forms;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;
using tTask.ViewModels;

namespace tTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Helper _helper;
        private readonly UserTable _ut;
        private readonly TenantTable _tt;
        private readonly NewTenantProcedure _newTenant;
        private readonly ServiceOrderTable _serviceOrderTable;
        private readonly NotificationTable _notificationTable;
        private readonly TaskTable _taskTable;

        private UserManager<User> UserMgr { get; }
        private SignInManager<User> SignInMgr { get; }
        private UserManager<GlobalUser> SharedUserMgr { get; }
        private SignInManager<GlobalUser> SharedSignInMgr { get; }

        private readonly IAuthorizationService _authorization;

        public HomeController(ILogger<HomeController> logger, Helper helper, UserTable ut, TenantTable tt,
                    UserManager<User> userManager, SignInManager<User> signInManager,
                    UserManager<GlobalUser> sharedUserManager, SignInManager<GlobalUser> sharedSignInManager,
                    NewTenantProcedure newTenant, IAuthorizationService auth, ServiceOrderTable serviceOrderTable, NotificationTable notificationTable, TaskTable taskTable)
        {
            _logger = logger;
            _helper = helper;

            _ut = ut;
            _tt = tt;
            _serviceOrderTable = serviceOrderTable;

            _newTenant = newTenant;

            UserMgr = userManager;
            SignInMgr = signInManager;

            SharedUserMgr = sharedUserManager;
            SharedSignInMgr = sharedSignInManager;

            _authorization = auth;

            _notificationTable = notificationTable;
            _taskTable = taskTable;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (_helper.CorrectDomain())
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    if (HttpContext.Request.Cookies["Identity.Domain"] != HttpContext.Items["domain"] as string) return RedirectToAction("LogOut");
                    if (HttpContext.Items["domain"] as string == "default")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        var idUser = _ut.GetUserId(HttpContext.User.Identity.Name);
                        _notificationTable.NotifyCloseDeadlineTasks(idUser);
                        if (idUser == 1)
                        {
                            _notificationTable.NotifyClosePaymentExpirationDate(_tt.GetTenantId(HttpContext.Items["domain"] as string));
                        }
                        var TenantPolicyResult = await _authorization.AuthorizeAsync(User, "TenantPolicy");
                        if (TenantPolicyResult.Succeeded)
                            return RedirectToAction("Index", "HomePage");
                        else
                            await SignInMgr.SignOutAsync();
                    }
                }
                var model = new IndexViewModel
                {
                    Domain = HttpContext.Items["domain"] as string,
                    SignUpSelected = false,
                    SignIn = new SignInForm(),
                    SignUp = new SignUpForm()
                };
                return View(model);
            }
            else
            {
                return RedirectToAction("DomainError", "Home");
            }
        }



        [HttpPost]
        public async Task<IActionResult> SignIn(SignInForm signIn)
        {
            if (ModelState.IsValid)
            {
                if(HttpContext.Items["domain"] as string != "default")
                {
                    User user = await UserMgr.FindByEmailAsync(signIn.Email);
                    if (user != null)
                    {
                        var TenantPolicyResult = await _authorization.AuthorizeAsync(User, "TenantPolicy");
                        if (TenantPolicyResult.Succeeded)
                        {
                            var result = await SignInMgr.PasswordSignInAsync(user, signIn.Password, true, false);
                            if (result.Succeeded)
                            {
                                HttpContext.Response.Cookies.Append(
                                    "Identity.Domain", 
                                    HttpContext.Items["domain"] as string, 
                                    new CookieOptions { 
                                        Expires = DateTime.Now.AddDays(30), 
                                        HttpOnly = true, 
                                        Secure = true, 
                                        SameSite= SameSiteMode.Lax 
                                    }
                                );
                               
                                _notificationTable.NotifyCloseDeadlineTasks(user.Id);
                                if (user.Id == 1)
                                {
                                    _notificationTable.NotifyClosePaymentExpirationDate(_tt.GetTenantId(HttpContext.Items["domain"] as string));
                                }
                                return RedirectToAction("Index", "HomePage");
                            }
                            else
                            {
                                ModelState.AddModelError("CustomErr", $"Unable to login user {signIn.Email}.");
                            }
                        }
                        else
                        {
                            if (await UserMgr.IsInRoleAsync(user, nameof(Roles.DomainAdmin)))
                            {
                                var result = await SignInMgr.PasswordSignInAsync(user, signIn.Password, true, false);
                                if (result.Succeeded)
                                {
                                    HttpContext.Response.Cookies.Append(
                                        "Identity.Domain",
                                        HttpContext.Items["domain"] as string,
                                        new CookieOptions
                                        {
                                            Expires = DateTime.Now.AddDays(30),
                                            HttpOnly = true,
                                            Secure = true,
                                            SameSite = SameSiteMode.Lax
                                        }
                                    );
                                    return RedirectToAction("Index", "Profile");
                                }
                                else
                                {
                                    ModelState.AddModelError("CustomErr", $"Unable to login user {signIn.Email}.");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("CustomErr", $"Service is not paid! Please contact your domain admin.");
                            }
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("CustomErr", $"User {signIn.Email} does not exist in application.");
                    }
                }
                else
                {
                    GlobalUser user = await SharedUserMgr.FindByEmailAsync(signIn.Email);
                    if (user != null)
                    {
                        var result = await SharedSignInMgr.PasswordSignInAsync(user, signIn.Password, true, false);
                        if (result.Succeeded)
                        {
                            HttpContext.Response.Cookies.Append(
                                "Identity.Domain",
                                HttpContext.Items["domain"] as string,
                                new CookieOptions
                                {
                                    Expires = DateTime.Now.AddDays(30),
                                    HttpOnly = true,
                                    Secure = true,
                                    SameSite = SameSiteMode.Lax
                                }
                            );
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            ModelState.AddModelError("CustomErr", $"Unable to login user {signIn.Email}.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("CustomErr", $"User {signIn.Email} does not exist in application.");
                    }
                }   
            }
            var model = new IndexViewModel
            {
                Domain = HttpContext.Items["domain"] as string,
                SignUpSelected = false,
                SignIn = signIn,
                SignUp = new SignUpForm()
            };

            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpForm signUp)
        {
            if (HttpContext.Items["domain"] as string == "default")
            {
                if (!string.IsNullOrEmpty(signUp.TenantDomain) && !string.IsNullOrEmpty(signUp.TenantName) && ModelState.IsValid)
                {
                    if (Regex.Matches(signUp.TenantDomain, @"[a-zA-Z]").Count == signUp.TenantDomain.Length)
                    {
                        var blacklist = new string[] { "default", "admin", "NewTenantUser", "sa", "helper", "dbo", "guest", "sys", "ttask" };
                        if (!_tt.TenantAlreadyExists(signUp.TenantDomain) && !blacklist.Contains(signUp.TenantDomain))
                        {
                            _newTenant.NewTenant(signUp.TenantName, signUp.TenantDomain);
                            Directory.CreateDirectory(@"wwwroot/img/profile/" + signUp.TenantDomain);
                            
                            HttpContext.Items["domain"] = signUp.TenantDomain;

                            int tenantId = _tt.GetTenantId(signUp.TenantDomain);


                            var user = new User()
                            {
                                Id = 1,
                                UserName = signUp.Email,
                                Email = signUp.Email,
                                FirstName = signUp.FirstName,
                                Surname = signUp.Surname,
                                PhoneNumber = signUp.PhoneNumber,
                                IdTenant = tenantId
                            };


                            HttpContext.Items["domain"] = signUp.TenantDomain;
                            IdentityResult result = await UserMgr.CreateAsync(user, signUp.Password);

                            if (result.Succeeded)
                            {
                                await UserMgr.AddToRolesAsync(user, new List<string>{ nameof(Roles.NormalUser), nameof(Roles.DomainAdmin), nameof(Roles.Manager)});

                                var settings = new UserSettings()
                                {
                                    Coloring = "0",
                                    CustomizeView = "0",
                                    Notifications = "0",
                                    IdUser = user.Id
                                };
                                _ut.InsertSettings(settings);

                               
                                return Redirect($"https://{HttpContext.Request.Host}/{signUp.TenantDomain}");

                            }

                        }
                        else
                        {
                            ModelState.AddModelError("CustomErr", $"Domain {signUp.TenantDomain} already exists.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("CustomErr", $"Domain has to contain letters only.");
                    }
                }
                else
                {
                    if(string.IsNullOrEmpty(signUp.TenantName))
                    {
                        ModelState.AddModelError("signUp.TenantName", "Company name is required field!");
                    }
                    if (string.IsNullOrEmpty(signUp.TenantDomain))
                    {
                        ModelState.AddModelError("signUp.TenantDomain", "Domain is required field!");
                    }
                }
            }
            else
            {
                var idService = _serviceOrderTable.GetInUseServiceIdByTenantId(_tt.GetTenantId(HttpContext.Items["domain"] as string));
                var noUsers = _ut.GetNOUsers();

                if ((idService == (int)Services.Basic && noUsers < 5) || (idService == (int)Services.Pro && noUsers < 10) || idService == (int)Services.Business)
                {
                    if (ModelState.IsValid)
                    {
                        signUp.TenantDomain = HttpContext.Items["domain"] as string;

                        int tenantId = _tt.GetTenantId(signUp.TenantDomain);
                        int userId = _ut.GetMaxId();


                        User user = await UserMgr.FindByEmailAsync(signUp.Email);
                        if (user == null)
                        {
                            user = new User()
                            {
                                Id = userId,
                                UserName = signUp.Email,
                                Email = signUp.Email,
                                FirstName = signUp.FirstName,
                                Surname = signUp.Surname,
                                PhoneNumber = signUp.PhoneNumber,
                                IdTenant = tenantId
                            };


                            IdentityResult result = await UserMgr.CreateAsync(user, signUp.Password);

                            if (result.Succeeded)
                            {
                                await UserMgr.AddToRoleAsync(user, nameof(Roles.NormalUser));
                                if (userId == 1)
                                {
                                    await UserMgr.AddToRoleAsync(user, nameof(Roles.DomainAdmin));
                                    await UserMgr.AddToRoleAsync(user, nameof(Roles.Manager));
                                }

                                var settings = new UserSettings()
                                {
                                    Coloring = "0",
                                    CustomizeView = "0",
                                    Notifications = "0",
                                    IdUser = user.Id
                                };
                                _ut.InsertSettings(settings);

                                var TenantPolicyResult = await _authorization.AuthorizeAsync(User, "TenantPolicy");
                                if (TenantPolicyResult.Succeeded)
                                {
                                    var msg = "User " + user.FirstName + " " + user.Surname + " has signed up in your application.";
                                    _notificationTable.NotifyUser(1, msg);
                                    var signInResult = await SignInMgr.PasswordSignInAsync(user, signUp.Password, false, false);
                                    if (signInResult.Succeeded)
                                    {
                                        HttpContext.Response.Cookies.Append(
                                            "Identity.Domain", 
                                            HttpContext.Items["domain"] as string, 
                                            new CookieOptions { 
                                                Expires = DateTime.Now.AddDays(30), 
                                                HttpOnly = true, 
                                                Secure = true, 
                                                SameSite= SameSiteMode.Lax 
                                            }
                                        );
                                        return RedirectToAction("Index", "HomePage");
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("CustomErr", $"User was created, but service of your domain is not paid. Sign in is impossible. Please contact your domain admin.");
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("CustomErr", $"User {user.Email} already exists.");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("CustomErr", $"Number of users has been exceeded! If you want to register, contact domain admin to subscribe to higher service.");
                }
            }

            var model = new IndexViewModel
            {
                Domain = HttpContext.Items["domain"] as string,
                SignUpSelected = true,
                SignIn = new SignInForm(),
                SignUp = signUp
            };
            return View("Index", model);
        }

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await SignInMgr.SignOutAsync();
            HttpContext.Response.Cookies.Delete("Identity.Domain");
            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult DomainError()
        {
            return View();
        }
        [HttpPost]
        public IActionResult DomainErrorPost()
        {
            var host = HttpContext.Request.Host.Host.Split('.');
            return Redirect($"https://{host[host.Length - 1]}:{HttpContext.Request.Host.Port}/");
        }

        [HttpGet]
        public IActionResult AccessDenied( string returnUrl = "")
        {
            return View();
        }

        [HttpPost]
        public IActionResult AccessDeniedPost()
        {
            var host = HttpContext.Request.Cookies["Identity.Domain"] == null ? "default" : HttpContext.Request.Cookies["Identity.Domain"];

            return Redirect($"https://{HttpContext.Request.Host}/{host}");
        }

        [HttpGet]
        public IActionResult LoginRedirect(string returnUrl = "")
        {
            var path = returnUrl.Split('/')[1];
            return Redirect($"https://{HttpContext.Request.Host}/{path}");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
