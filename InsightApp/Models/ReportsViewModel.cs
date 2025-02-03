using InsightApp.Entities;

namespace InsightApp.Models
{
    public class ReportsViewModel
    {
        public List<GameCategory> GameCategories { get; set; }
        public List<GamePlatform> GamePlatforms { get; set; }
        public List<EventType> EventTypes { get; set; }

        public string? Category { get; set; }
        public string? Platform { get; set; }
        public string? EventType { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

    }
}
