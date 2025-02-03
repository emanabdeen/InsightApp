//using InsightApp.Areas.Identity.Data;
using InsightApp.Entities;
using InsightApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace InsightApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventController : Controller
    {
        private InsightUpdateCvgs2Context _SVGSDbContext;
        public EventController( InsightUpdateCvgs2Context sVGSDbContext)
        {
            _SVGSDbContext = sVGSDbContext;
        }


        [HttpGet("AdminPanel/events")]
        public async Task<IActionResult> GetAllEvents(EventListModel eventListModel)
        {

            if (eventListModel.SearchText==null)
            {
                //will return only the events that (isDeleted=false)
                var allEvents = await _SVGSDbContext.GameEvents
                    .Include(e => e.EvType)
                    .Include(e => e.Address)
                    .Where(e => e.IsDeleted == false)
                    .OrderBy(e => e.StartDate).ToListAsync();

                eventListModel.EventList = allEvents;

            }
            else
            {
                //will return only the events that (isDeleted=false) && start with SearchText
                var allEvents = await _SVGSDbContext.GameEvents
                    .Include(e => e.EvType)
                    .Include(e => e.Address)
                    .Where(e => e.IsDeleted == false && (e.EventName.Contains(eventListModel.SearchText) || e.EvType.EvTypeName.Contains(eventListModel.SearchText)))
                    .OrderBy(e => e.EventName).ToListAsync();

                eventListModel.EventList = allEvents;
            }

            return View("List", eventListModel);
        }

        
        [HttpPost("AdminPanel/events/searchResult")]
        public async Task<IActionResult> GetsearchResultEvents(EventListModel eventListModel)
        {
            return RedirectToAction("GetAllEvents", "Event", new { EventList = eventListModel.EventList, SearchText = eventListModel.SearchText });
        }

        
        [HttpGet("AdminPanel/events/add-request")]
        public async Task<IActionResult> GetAddEventRequest()
        {
            EventViewModel eventViewModel = new EventViewModel()
            {
                EventTypes = await _SVGSDbContext.EventTypes.OrderBy(t => t.EvTypeId).ToListAsync(),
                ActiveEvent = new GameEvent() { StartDate= DateOnly.FromDateTime(DateTime.Today.AddDays(1)) } //to put the default date as tomorrow 
            };

            return View("AddEvent", eventViewModel);
        }

        
        [HttpPost("AdminPanel/events")]
        public async Task<IActionResult> AddNewEvent(EventViewModel eventViewModel)
        {            
            if (ModelState.IsValid)
            {
                
                // it's valid so we want to add the new event to the DB:
                await _SVGSDbContext.GameEvents.AddAsync(eventViewModel.ActiveEvent);
                await _SVGSDbContext.SaveChangesAsync();
                TempData["LastActionMessage"] = $"The event \"{eventViewModel.ActiveEvent.EventName}\" is successfully add.";
                return RedirectToAction("GetAllEvents", "Event");
            }
            else
            {
                // it's invalid so we simply return the event object
                // to the Edit view again:
                eventViewModel.EventTypes = _SVGSDbContext.EventTypes.OrderBy(t => t.EvTypeName).ToList();
                return View("AddEvent", eventViewModel);
            }
        }

        
        [HttpGet("AdminPanel/events/{id}")]
        public async Task<IActionResult> GetEventById(int id)
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

            return View("Details", eventDetailViewModel);
        }

        
        [HttpGet("AdminPanel/events/{id}/edit-request")]
        public async Task<IActionResult> GetEditRequestById(int id)
        {
            var gameEvent = await _SVGSDbContext.GameEvents
                .Include(e => e.EvType)
                .Include(e => e.Address)
                .Where(e => e.EventId == id).FirstOrDefaultAsync();

            EventViewModel eventViewModel = new EventViewModel()
            {
                EventTypes = await _SVGSDbContext.EventTypes.OrderBy(t => t.EvTypeId).ToListAsync(),
                ActiveEvent = gameEvent
            };

            return View("EditEvent", eventViewModel);
        }

        
        [HttpPost("AdminPanel/events/edit-requests")]
        public async Task<IActionResult> ProcessEditRequest(EventViewModel eventViewModel)
        {

            if (ModelState.IsValid)
            {
                var eventType = await _SVGSDbContext.EventTypes
                .Where(e => e.EvTypeId == eventViewModel.ActiveEvent.EvTypeId).FirstOrDefaultAsync();

                //the event is virtual and there is a value for Address id,
                //will remove the address record from Address table then change the addressId to null in the GameEvent table

                
                if (eventType.EvTypeName.ToLower()=="virtual" && eventViewModel.ActiveEvent.AddressId !=null)
                {

                    // Find the address in EventAddressTable to delete:
                    var eventAddress = _SVGSDbContext.EventAddressTables.Find(eventViewModel.ActiveEvent.AddressId);

                    //update the gameEvent
                    eventViewModel.ActiveEvent.AddressId = null;
                    _SVGSDbContext.GameEvents.Update(eventViewModel.ActiveEvent);
                    await _SVGSDbContext.SaveChangesAsync();

                    //Delete the address from EventAddressTable
                    if (eventAddress != null)
                    {
                        _SVGSDbContext.EventAddressTables.Remove(eventAddress);
                        await _SVGSDbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    _SVGSDbContext.GameEvents.Update(eventViewModel.ActiveEvent);
                    await _SVGSDbContext.SaveChangesAsync();
                }

                TempData["LastActionMessage"] = $"The event \"{eventViewModel.ActiveEvent.EventName}\" was updated.";

                return RedirectToAction("GetAllEvents", "Event");
            }
            else
            {
                eventViewModel.EventTypes = await _SVGSDbContext.EventTypes.OrderBy(t => t.EvTypeId).ToListAsync();
                return View("EditEvent", eventViewModel);
            }
        }

        
        [HttpGet("AdminPanel/events/{id}/delete-requests")]
        public async Task<IActionResult> ProcessDeleteRequest(int id)
        {
            //actually we are not deleting the event record, we only update "isDeleted" to true without saving any changes that happened in the form
            //here we get the original event by id
            var gameEvent = await _SVGSDbContext.GameEvents
            .Include(e => e.EvType)
            .Include(e => e.Address)
            .Where(e => e.EventId == id).FirstOrDefaultAsync();

            // do soft deletion
            gameEvent.IsDeleted = true;

            // save the changes to DB:
            _SVGSDbContext.GameEvents.Update(gameEvent);
            await _SVGSDbContext.SaveChangesAsync();

            return RedirectToAction("GetAllEvents", "Event");
            
        }

        
        public IActionResult Index()
        {
            return View();
        }
    }
}
