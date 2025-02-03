using InsightApp.Entities;

namespace InsightApp.Models
{
    public class ProductDetailsViewModel
    {
        public Game ActiveGame { get; set; }
        public double AverageRating { get; set; } = 0;
        public List<string>? Reviews { get; set; }
        public List<Game>? RelatedGames { get; set; }
        public List<Game>? GamesFromPreferences { get; set; }
        public Dictionary<int, string>? SharedGameCategories { get; set; }
    }
}
