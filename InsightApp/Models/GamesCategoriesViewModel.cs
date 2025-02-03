namespace InsightApp.Models
{
    public class GamesCategoriesViewModel
    {
        public int GameId { get; set; }
        public string GameName { get; set; }
        public double? GamePrice { get; set; }
        public string? GameImageLink { get; set; } = "";
        public List<string> Categories { get; set; } = new List<string>();
    }
}
