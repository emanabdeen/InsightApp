using Humanizer.Localisation;
using InsightApp.Entities;

namespace InsightApp.Models
{
    public class EventViewModel
    {
        public List<EventType>? EventTypes { get; set; }

        public GameEvent ActiveEvent { get; set; }
    }
}
