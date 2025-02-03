using InsightApp.Components;
using InsightApp.Entities;
using InsightApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;



namespace InsightApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private InsightUpdateCvgs2Context _SVGSDbContext;
        private readonly UserManager<Entities.Account> _userManager;
        public ReportController(InsightUpdateCvgs2Context sVGSDbContext, UserManager<Entities.Account> userManager)
        {
            _SVGSDbContext = sVGSDbContext;
            _userManager = userManager;
        }

        [HttpGet("AdminPanel/reports")]
        public async Task<IActionResult> GetGenerateReports()
        {
            try
            {
                ReportsViewModel reportsViewModel = new ReportsViewModel();
                var categories = await _SVGSDbContext.GameCategories.ToListAsync();
                var platforms = await _SVGSDbContext.GamePlatforms.ToListAsync();
                var eventTyps = await _SVGSDbContext.EventTypes.ToListAsync();
                if (!categories.Any() || !platforms.Any()|| !eventTyps.Any())
                {
                    throw new Exception();
                }
                else
                {
                    reportsViewModel.GameCategories = categories;
                    reportsViewModel.GamePlatforms = platforms;
                    reportsViewModel.EventTypes = eventTyps;
                }
                

                return View("Reports", reportsViewModel);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home");

            }
        }


        [HttpPost("AdminPanel/reports/WishListReports")]
        public async Task<IActionResult> GenerateWishListReport(ReportsViewModel reportsViewModel)
        {
            try
            {
                string category=reportsViewModel.Category;
                string platform =reportsViewModel.Platform;

                //retrieve the userId from Accounts Table according to the signing-in user (AccountId)
                string? accountId = _userManager.GetUserId(User);

                var userName = await _SVGSDbContext.Accounts
                    .Where(u => u.Id.ToString() == accountId).Select(u => u.UserName).FirstOrDefaultAsync();

                List<WishListReport> wishListReport = new List<WishListReport>();

                if (category == "All" && platform == "All")
                {
                    wishListReport = await _SVGSDbContext.WishListReports
                   .ToListAsync();
                }
                else if (category == "All" && platform != "All")
                {
                    wishListReport = await _SVGSDbContext.WishListReports
                   .Where(r => r.Platforms.Contains(platform))
                   .ToListAsync();
                }
                else if (category != "All" && platform == "All")
                {
                    wishListReport = await _SVGSDbContext.WishListReports
                   .Where(e => e.Categories.Contains(category))
                   .ToListAsync();
                }
                else if (category != "All" && platform != "All")
                {
                    wishListReport = await _SVGSDbContext.WishListReports
                   .Where(r => r.Platforms.Contains(platform) && r.Categories.Contains(category))
                   .ToListAsync();
                }


                ReportsGenerationViewModel reportsGenerationViewModel = new ReportsGenerationViewModel()
                {
                    userId = accountId,
                    userName = userName,
                    WishListReport = wishListReport,
                    Category = category,
                    Platform = platform
                };

                return View("WishListReport", reportsGenerationViewModel);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home");

            }
        }

        [HttpPost("AdminPanel/reports/GameRatingReport")]
        public async Task<IActionResult> GenerateGameRatingReport(ReportsViewModel reportsViewModel)
        {
            try
            {
                string category = reportsViewModel.Category;
                string platform = reportsViewModel.Platform; 

                //retrieve the userId from Accounts Table according to the signing-in user (AccountId)
                string? accountId = _userManager.GetUserId(User);

                var userName = await _SVGSDbContext.Accounts
                    .Where(u => u.Id.ToString() == accountId).Select(u => u.UserName).FirstOrDefaultAsync();

                List<GameRatingReport> gameRatingReport = new List<GameRatingReport>();

                if (category == "All" && platform == "All")
                {
                    gameRatingReport = await _SVGSDbContext.GameRatingReports
                   .ToListAsync();
                }
                else if (category == "All" && platform != "All")
                {
                    gameRatingReport = await _SVGSDbContext.GameRatingReports
                   .Where(r => r.Platforms.Contains(platform))
                   .ToListAsync();
                }
                else if (category != "All" && platform == "All")
                {
                    gameRatingReport = await _SVGSDbContext.GameRatingReports
                   .Where(e => e.Categories.Contains(category))
                   .ToListAsync();
                }
                else if (category != "All" && platform != "All")
                {
                    gameRatingReport = await _SVGSDbContext.GameRatingReports
                   .Where(r => r.Platforms.Contains(platform) && r.Categories.Contains(category))
                   .ToListAsync();
                }

                ReportsGenerationViewModel reportsGenerationViewModel = new ReportsGenerationViewModel()
                {
                    userId = accountId,
                    userName = userName,
                    GameRatingReport = gameRatingReport,
                    Category = category,
                    Platform = platform
                };

                return View("GameRatingReport", reportsGenerationViewModel);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home");

            }
        }

        [HttpPost("AdminPanel/reports/EventsRegistrationsReport")]
        public async Task<IActionResult> GenerateEventsRegistrationsReport(ReportsViewModel reportsViewModel)
        {
            try
            {
                string eventType = reportsViewModel.EventType;


                //retrieve the userId from Accounts Table according to the signing-in user (AccountId)
                string? accountId = _userManager.GetUserId(User);

                var userName = await _SVGSDbContext.Accounts
                    .Where(u => u.Id.ToString() == accountId).Select(u => u.UserName).FirstOrDefaultAsync();

                List<EventsRegistrationsReport> eventsRegistrationsReport = new List<EventsRegistrationsReport>();

                if (eventType == "All" )
                {
                    eventsRegistrationsReport = await _SVGSDbContext.EventsRegistrationsReports
                   .ToListAsync();
                }
                else 
                {
                    eventsRegistrationsReport = await _SVGSDbContext.EventsRegistrationsReports
                   .Where(e => e.EventType.Contains(eventType))
                   .ToListAsync();
                }
                

                ReportsGenerationViewModel reportsGenerationViewModel = new ReportsGenerationViewModel()
                {
                    userId = accountId,
                    userName = userName,
                    EventsRegistrationsReport = eventsRegistrationsReport,
                    EventType = eventType
                };

                return View("EventsRegistrationsReport", reportsGenerationViewModel);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home");

            }
        }

        [HttpPost("AdminPanel/reports/MemberOrderDetailsReport")]
        public async Task<IActionResult> GenerateMemberOrderDetailsReport(ReportsViewModel reportsViewModel)
        {
            try
            {

                //retrieve the userId from Accounts Table according to the signing-in user (AccountId)
                string? accountId = _userManager.GetUserId(User);

                var userName = await _SVGSDbContext.Accounts
                    .Where(u => u.Id.ToString() == accountId).Select(u => u.UserName).FirstOrDefaultAsync();

                List<MemberOrderDetailsReport> memberOrderDetailsReport = new List<MemberOrderDetailsReport>();

                memberOrderDetailsReport = await _SVGSDbContext.MemberOrderDetailsReports
                   .OrderByDescending(o => o.OrderCount)
                   .ToListAsync();



                ReportsGenerationViewModel reportsGenerationViewModel = new ReportsGenerationViewModel()
                {
                    userId = accountId,
                    userName = userName,
                    MemberOrderDetailsReport = memberOrderDetailsReport
                };

                return View("MemberOrderDetailsReport", reportsGenerationViewModel);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home");

            }
        }

        [HttpPost("AdminPanel/reports/MemberListReport")]
        public async Task<IActionResult> GenerateMemberListReport(ReportsViewModel reportsViewModel)
        {
            try
            {

                //retrieve the userId from Accounts Table according to the signing-in user (AccountId)
                string? accountId = _userManager.GetUserId(User);

                var userName = await _SVGSDbContext.Accounts
                    .Where(u => u.Id.ToString() == accountId).Select(u => u.UserName).FirstOrDefaultAsync();

                List<MemberListReport> memberListReport = new List<MemberListReport>();

                memberListReport = await _SVGSDbContext.MemberListReports
                   .ToListAsync();

                ReportsGenerationViewModel reportsGenerationViewModel = new ReportsGenerationViewModel()
                {
                    userId = accountId,
                    userName = userName,
                    MemberListReport = memberListReport
                };

                return View("MemberListReport", reportsGenerationViewModel);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home");

            }
        }

        [HttpPost("AdminPanel/reports/SalesReport")]
        public async Task<IActionResult> GenerateSalesReport(ReportsViewModel reportsViewModel)
        {
            try
            {
                DateOnly? startDate = reportsViewModel.StartDate;
                DateOnly? endDate = reportsViewModel.EndDate;

                //retrieve the userId from Accounts Table according to the signing-in user (AccountId)
                string? accountId = _userManager.GetUserId(User);

                var userName = await _SVGSDbContext.Accounts
                    .Where(u => u.Id.ToString() == accountId).Select(u => u.UserName).FirstOrDefaultAsync();

                List<SalesReport> salesReport = new List<SalesReport>();


                if (startDate == null && endDate==null)
                {
                    salesReport = await _SVGSDbContext.SalesReports
                   .ToListAsync();
                }
                else if(startDate != null && endDate == null)
                {
                    salesReport = await _SVGSDbContext.SalesReports
                   .Where(e => e.OrderDate>=startDate)
                   .ToListAsync();
                }
                else if (startDate == null && endDate != null)
                {
                    salesReport = await _SVGSDbContext.SalesReports
                    .Where(e => e.OrderDate <= endDate)
                    .ToListAsync();
                }
                else if (startDate != null && endDate != null)
                {
                    salesReport = await _SVGSDbContext.SalesReports
                    .Where(e => e.OrderDate <= endDate && e.OrderDate >= startDate)
                    .ToListAsync();
                }

                ReportsGenerationViewModel reportsGenerationViewModel = new ReportsGenerationViewModel()
                {
                    userId = accountId,
                    userName = userName,
                    SalesReport = salesReport,
                    StartDate = startDate,
                    EndDate = endDate,

                };

                return View("SalesReport", reportsGenerationViewModel);
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home");

            }
        }

    }
}
