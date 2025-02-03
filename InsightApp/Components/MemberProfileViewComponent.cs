using InsightApp.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace InsightApp.Components
{
    public class MemberProfileViewComponent : ViewComponent
    {
        private InsightUpdateCvgs2Context _SVGSDbContext;
		public MemberProfileViewComponent(InsightUpdateCvgs2Context sVGSDbContext)
        {
            _SVGSDbContext = sVGSDbContext;
		}

        public async Task<IViewComponentResult> InvokeAsync(int memberId)
        {
            var member = await _SVGSDbContext.Members
                .Where(e => e.MemberId == memberId).FirstOrDefaultAsync();

            MemberProfileViewModel memberProfileViewModel = new MemberProfileViewModel()
            {
                ActiveMember = member,
            };
            var user = await _SVGSDbContext.Accounts.Where(a => a.Member == member).FirstOrDefaultAsync();
            if (user != null)
            {
                ViewBag.Email = user.Email;
            }

			return View(memberProfileViewModel);
        }
    }
}
