using InsightApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading.Tasks;

namespace InsightApp.Services
{
    public class RoleDataSeeder
    {
        private const string ROLENAME_ADMIN = "Admin";
        private const string ROLENAME_MEMBER = "Member";
        private const string ADMIN_ID = "A80643A1-5F2D-4177-B8DE-A5737CE5022D";
        private const string ADMIN_BASE_PASSWORD = "InsightAdminAccount123!@#";  //adamjohn@insight.com//
        private static readonly string[] _roleNameData = { ROLENAME_ADMIN, ROLENAME_MEMBER };



        public async Task SeedRoleDataAsync(RoleManager<AccountRole> _roleManager)
        {

            foreach (string roleName in _roleNameData)
            {
                
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    AccountRole identityRole = new AccountRole(roleName);
                    await _roleManager.CreateAsync(identityRole);
                }
            }
        }

        public async Task SeedAdminAccount(RoleManager<AccountRole> _roleManager, UserManager<Account> _userManager)
        {
            if (await _roleManager.RoleExistsAsync(ROLENAME_ADMIN))
            {
                Account adminUser = await _userManager.FindByIdAsync(ADMIN_ID);
                await _userManager.AddPasswordAsync(adminUser, ADMIN_BASE_PASSWORD);
                await _userManager.AddToRoleAsync(adminUser, ROLENAME_ADMIN);
            }
        }
    }
}
