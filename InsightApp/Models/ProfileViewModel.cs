using InsightApp.Entities;

namespace InsightApp.Models
{
    public class ProfileViewModel
    {
        public Member ActiveMember { get; set; }
        public AddressTable MemberAddress { get; set; }
        public AddressTable ShippingAddress { get; set; }

        public Account? ActiveAccount { get; set; }
    }
}
