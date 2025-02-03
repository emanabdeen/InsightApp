using InsightApp.Entities;

namespace InsightApp.Models
{
    public class EventListModel
    {

        public List<GameEvent>? EventList { get; set; }

        public string SearchText { get; set; }
    }
}
