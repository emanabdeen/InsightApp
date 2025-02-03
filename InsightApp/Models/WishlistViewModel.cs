using InsightApp.Entities;

namespace InsightApp.Models
{
    public class WishlistViewModel
    {
        public int WishlistItemId { get; set; }
        public int? GameId { get; set; }
        public string? GameName { get; set; }
        public string? GameImageLink { get; set; } = "";
        public double? Price { get; set; }
    }
}
