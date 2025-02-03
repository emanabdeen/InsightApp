using InsightApp.Entities;
using InsightApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InsightApp.Controllers
{
    [Authorize(Roles = "Member")]
    public class PortalEventsController : Controller
    {
        private InsightUpdateCvgs2Context _SVGSDbContext;
        private readonly SignInManager<Account> _signInManager;
        private readonly UserManager<Account> _userManager;

        public PortalEventsController(InsightUpdateCvgs2Context sVGSDbContext, UserManager<Account> userManager)
        {
            _SVGSDbContext = sVGSDbContext;
            _userManager = userManager;
        }

        [HttpGet("Portal/events")]
        [HttpPost("Portal/events")]
        public async Task<IActionResult> GetEvents(EventListModel eventListModel)
        {

            if (eventListModel.SearchText == null)
            {
                //will return only the events that (isDeleted=false)
                var allEvents = await _SVGSDbContext.GameEvents
                    .Include(e => e.EvType)
                    .Include(e => e.Address)
                    .Where(e => e.IsDeleted == false && e.StartDate> DateOnly.FromDateTime(DateTime.Today))  //to show only the future events 
                    .OrderBy(e => e.StartDate).ToListAsync();

                eventListModel.EventList = allEvents;

            }
            else
            {
                //will return only the events that (isDeleted=false) && start with SearchText
                var allEvents = await _SVGSDbContext.GameEvents
                    .Include(e => e.EvType)
                    .Include(e => e.Address)
                    .Where(e => e.IsDeleted == false && e.StartDate > DateOnly.FromDateTime(DateTime.Today) && (e.EventName.Contains(eventListModel.SearchText) || e.EvType.EvTypeName.Contains(eventListModel.SearchText))) //to show only the future events 
                    .OrderBy(e => e.EventName).ToListAsync();

                eventListModel.EventList = allEvents;
            }
            return View("EventsList", eventListModel);
        }

        [HttpGet("Portal/events/{id}")]
        public async Task<IActionResult> GetEventById(int id, string? myEvent)
        {
            var gameEvent = await _SVGSDbContext.GameEvents
                .Include(e => e.EvType)
                .Include(e => e.Address)
                .Include(e => e.MemberEventRegists)
                .Where(e => e.EventId == id).FirstOrDefaultAsync();
            
            EventDetailViewModel eventDetailViewModel = new EventDetailViewModel()
            {
                ActiveEvent = gameEvent,
                Registrations = gameEvent.MemberEventRegists.Count()
            };
            if (gameEvent.StartDate < DateOnly.FromDateTime(DateTime.Now))
            {
                eventDetailViewModel.PastEvent = "true";
            }

            if (!string.IsNullOrEmpty(myEvent))
            {
                ViewBag.MyEvent = "true";
            }

            return View("Details", eventDetailViewModel);
        }

        [HttpPost("Portal/events/register-request")]
        public async Task<IActionResult> RegisterToEvent(int id)
        {
            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            bool registeredMember = await _SVGSDbContext.MemberEventRegists
                .AnyAsync(r => r.EventId == id && r.MemberId == memberId);
            if (registeredMember == false)
            {
                MemberEventRegist memberEventRegist = new MemberEventRegist() { EventId = id, MemberId = memberId};
                await _SVGSDbContext.MemberEventRegists.AddAsync(memberEventRegist);
                await _SVGSDbContext.SaveChangesAsync();
                TempData["LastActionMessage"] = $"Successfully registered!";
                return RedirectToAction("GetEvents", "PortalEvents"); //return to the product list page
            }
            else
            {
                TempData["LastActionMessage"] = $"You are already registered for this event.";
                return RedirectToAction("GetEventById", "PortalEvents", new { id = id }); // keep in the same page
            }
        }

    }
}
