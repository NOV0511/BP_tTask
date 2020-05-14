using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using tTask.ORM.DTO;
using tTask.ViewModels;

namespace tTask.ViewComponents
{
    public class PageHeaderViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<User> _userManager;

        public PageHeaderViewComponent(IHttpContextAccessor httpContext, UserManager<User> userManager)
        {
            _contextAccessor =
                httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<IViewComponentResult> InvokeAsync(BasePageViewModel model)
        {
            var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User) ?? throw new Exception("User was not found");

            return View("Default",
                new PageHeaderViewModel { FirstName = user.FirstName, Surname = user.Surname, PageModel = model });
        }
    }
}
