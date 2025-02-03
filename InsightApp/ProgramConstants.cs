using Microsoft.AspNetCore.Identity;
using System.Security.Policy;

namespace InsightApp
{
    public static class ProgramConstants
    {
        public static Action<IdentityOptions> GetIdentityOptions() => options =>
        {
            options.SignIn.RequireConfirmedEmail = true;

			options.User.RequireUniqueEmail = true;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
            options.Lockout.MaxFailedAccessAttempts = 3;

            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
        };
    }
}
