using InsightApp.Entities;

namespace InsightApp.Models
{
    public class ReportsGenerationViewModel
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public DateTime DateTimeStamp { get; set; } = DateTime.Now;

        public string? Category { get; set; }
        public string? Platform { get; set; }
        public string? EventType { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public List<WishListReport>? WishListReport { get; set; }
        public List<GameRatingReport>? GameRatingReport { get; set; }
        public List<EventsRegistrationsReport>? EventsRegistrationsReport { get; set; }
        public List<MemberOrderDetailsReport>? MemberOrderDetailsReport { get; set; }
        public List<SalesReport>? SalesReport { get; set; }
        public List<MemberListReport>? MemberListReport { get; set; }

    }
}
