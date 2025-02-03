using InsightApp.Entities;

namespace InsightApp.Models
{
	public class CheckoutViewModel
	{
		public AddressTable? ShippingAddress { get; set; }
		public List<Country>? Countries { get; set; }
		public List<Province>? Provinces { get; set; }
		public double? TotalPrice { get; set; }
		public int? HasPhysicalItems { get; set; }

	}
}
