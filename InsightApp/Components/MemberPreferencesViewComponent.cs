using InsightApp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Components
{
    public class MemberPreferencesViewComponent : ViewComponent
    {
        private InsightUpdateCvgs2Context _SVGSDbContext;
        public MemberPreferencesViewComponent(InsightUpdateCvgs2Context sVGSDbContext)
        {
            _SVGSDbContext = sVGSDbContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(int memberId)
        {
            var allCategories = await _SVGSDbContext.GameCategories.ToListAsync();
            var allLanguages = await _SVGSDbContext.LanguageTables.ToListAsync();
            var allPlatforms = await _SVGSDbContext.GamePlatforms.ToListAsync();

            var memberPrefCategoriesSelected = await _SVGSDbContext.MemberGameCategoryPrefs
                .Where(g => g.MemberId == memberId)
                .Select(g => g.Category.CategoryId)
                .ToListAsync();

            var memberPrefLanguagesSelected = await _SVGSDbContext.MemberLanguagePrefs
                .Where(g => g.MemberId == memberId)
                .Select(g => g.Language.LanguageId)
                .ToListAsync();

            var memberPrefPlatformsSelected = await _SVGSDbContext.MemberPlatformPrefs
                .Where(g => g.MemberId == memberId)
                .Select(g => g.Platform.PlatformId)
                .ToListAsync();


            MemberPreferencesViewModel memberPreferencesViewModel = new MemberPreferencesViewModel()
            {
                MemberId = memberId,
                AllCategories = allCategories,
                AllLanguages = allLanguages,
                AllPlatforms = allPlatforms,
                SelectedCategoryIds = memberPrefCategoriesSelected,
                SelectedLanguageIds = memberPrefLanguagesSelected,
                SelectedPlatformIds = memberPrefPlatformsSelected
            };

            return View(memberPreferencesViewModel);
        }

    }
}
