using InsightApp.Entities;

namespace InsightApp.Models
{
	public class CartItemsViewModel
	{
		public int CartItemId { get; set; }
		public string GameName { get; set; }
        public string? GameImageLink { get; set; } = "";
        public double Price { get; set; }
		public int IsPhysical { get; set; }
	}
}
