namespace InsightApp.Entities
{
    public class WishListReport
    {
        public int? GameId { get; set; }
        public string? GameName { get; set; }
        public string? Details { get; set; }
        public double? Price { get; set; }
        public string? Categories { get; set; }
        public string? Platforms { get; set; }
        public int? FrequencyInWishList { get; set; }

    }
}
