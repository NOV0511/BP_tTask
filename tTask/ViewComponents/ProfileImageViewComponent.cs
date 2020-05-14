using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using tTask.ORM.DTO;

namespace tTask.ViewComponents
{
    public class ProfileImageViewComponent : ViewComponent
    {
        private readonly UserManager<User> _userManager;
        private readonly HttpContext _httpContext;

        public ProfileImageViewComponent(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = (httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor))).HttpContext;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("Default", await _userManager.GetUserAsync(_httpContext.User));
        }
    }
}
