using InsightApp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ObjectPool;

namespace InsightApp.Components
{
    public class MemberPasswordViewComponent : ViewComponent
    {

        private readonly UserManager<Entities.Account> _userManager;
        private readonly SignInManager<Entities.Account> _signInManager;
        private readonly ILogger<MemberPasswordViewComponent> _logger;
        private InsightUpdateCvgs2Context _SVGSDbContext;

        public MemberPasswordViewComponent(
            UserManager<Entities.Account> userManager,
            SignInManager<Entities.Account> signInManager,
            ILogger<MemberPasswordViewComponent> logger,
            InsightUpdateCvgs2Context sVGSDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _SVGSDbContext = sVGSDbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(int memberId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                HttpContext.Response.Redirect("/Identity/Account/Login");
                return Content("");
            }
            var passwordModelErrors = TempData["PasswordModelErrors"];
            var contextUser = HttpContext.User;
            Account account = await _userManager.GetUserAsync(contextUser);
            
            MemberPasswordViewModel memberPasswordViewModel = new MemberPasswordViewModel()
            {
                AccountId = account.Id,
                MemberId = memberId,
            };
            
            if (passwordModelErrors != null && (TempData["PasswordModelErrors"] is string[] errors))
            {
                
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            return View(memberPasswordViewModel);
        }
    }
}
