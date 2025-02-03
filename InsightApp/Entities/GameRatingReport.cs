namespace InsightApp.Entities
{
    public class GameRatingReport
    {
        public int? GameId { get; set; }
        public string? GameName { get; set; }
        public string? Categories { get; set; }
        public string? Platforms { get; set; }
        public double? AverageRating { get; set; }
    }
}
