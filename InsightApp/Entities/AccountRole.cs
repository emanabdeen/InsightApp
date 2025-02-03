using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsightApp.Entities
{
    [Table("AspNetRole")]
    public class AccountRole : IdentityRole<Guid>
    {
        public AccountRole(string name)
        {
            this.Name = name;
            this.NormalizedName = name.ToUpperInvariant();
        }
    }
}
