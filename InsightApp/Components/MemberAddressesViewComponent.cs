using InsightApp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace InsightApp.Components
{
    public class MemberAddressesViewComponent : ViewComponent
    {
        private InsightUpdateCvgs2Context _SVGSDbContext;
        public MemberAddressesViewComponent(InsightUpdateCvgs2Context sVGSDbContext)
        {
            _SVGSDbContext = sVGSDbContext;
        }

        public async Task <IViewComponentResult> InvokeAsync(int memberId)
        {
            var countries = await _SVGSDbContext.Country.ToListAsync();
            var provinces = await _SVGSDbContext.Province.ToListAsync();
            MemberAddressesViewModel memberAddressesViewModel = new MemberAddressesViewModel()
            {
                MemberId = memberId,
                Countries = countries,
                Provinces = provinces
            };
            
            var memberAddress = await _SVGSDbContext.AddressTables
               .Where(e => e.MemberId == memberId && e.IsShipping == false).FirstOrDefaultAsync();
            
            var shippingAddress = await _SVGSDbContext.AddressTables
               .Where(e => e.MemberId == memberId && e.IsShipping == true).FirstOrDefaultAsync();

            if(memberAddress!=null && shippingAddress!=null)
            {
                memberAddressesViewModel.MemberAddress = memberAddress;
                memberAddressesViewModel.ShippingAddress = shippingAddress;
                memberAddressesViewModel.IsAdressesSame = AreAddressEqual(memberAddress, shippingAddress);// check if both addresses are equals

            }
            else if (memberAddress == null || shippingAddress == null)
            {
				memberAddressesViewModel.MemberAddress = memberAddress;
				memberAddressesViewModel.ShippingAddress = shippingAddress;
				memberAddressesViewModel.IsAdressesSame = false;
			}
            else
            {
                memberAddressesViewModel.MemberAddress = new AddressTable();
                memberAddressesViewModel.ShippingAddress = new AddressTable();
                memberAddressesViewModel.IsAdressesSame = false;
            }

            return View(memberAddressesViewModel);
        }

        /// <summary>
        /// To compare the two addresses of the member to see of they are the same 
        /// ( "StreetName", "StreetNumber", "Unit","PostalCode","City", "Province","Country")
        /// </summary>
        public static bool AreAddressEqual<T>(T obj1, T obj2)
        {
            // If one object is null and the other is not, they are not equal
            if (obj1 == null || obj2 == null)
                return false;

            // Get all the public properties of the objects
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "StreetName" || property.Name == "StreetNumber"
                    || property.Name == "Unit" || property.Name == "PostalCode"
                    || property.Name == "City" || property.Name == "Province" || property.Name == "Country")
                {
                    object value1 = property.GetValue(obj1);
                    object value2 = property.GetValue(obj2);

                    // Check if values are not equal
                    if (!Equals(value1, value2))
                    {
                        return false;
                    }
                }
            }

            return true; // All properties are equal
        }
    }
}
