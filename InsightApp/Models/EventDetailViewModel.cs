using InsightApp.Entities;

namespace InsightApp.Models
{
    public class EventDetailViewModel
    {
        public GameEvent ActiveEvent { get; set; }

        public List<EventType>? EventTypes { get; set; }

        public int? Registrations { get; set; }

        public string PastEvent { get; set; } = "false";
    }
}
