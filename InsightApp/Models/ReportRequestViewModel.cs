namespace InsightApp.Models
{
    public class ReportRequestViewModel
    {
        public string? category { get; set; }
        public string? Platform { get; set; }
        public string? EventType { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
