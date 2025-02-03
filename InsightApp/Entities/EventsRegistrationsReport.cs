namespace InsightApp.Entities
{
    public class EventsRegistrationsReport
    {
        public string? EventName { get; set; }
        public DateOnly? StartDate { get; set; }
        public string? EventType { get; set; }
        public int? Registrations { get; set; }
    }
}
