namespace InsightApp.Models
{
    public class ReviewViewModel
    {
        public int ReviewId { get; set; }
        public string GameName { get; set; }
        public string? GameImageLink { get; set; }
        public string ReviewedBy { get; set; }
        public string ReviewBody { get; set; }
        public string StatusName { get; set; }
        public double? UserRating { get; set; }
    }
}
