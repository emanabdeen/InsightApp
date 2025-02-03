using InsightApp.Entities;

namespace InsightApp.Components
{
    public class MemberAddressesViewModel
    {
        public int MemberId { get; set; }
        public bool IsAdressesSame { get; set; }
        public AddressTable MemberAddress { get; set; }
        public AddressTable ShippingAddress { get; set; }
        public List<Country>? Countries { get; set; }
        public List<Province>? Provinces { get; set; }
    }
}
