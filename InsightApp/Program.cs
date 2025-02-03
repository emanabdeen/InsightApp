using InsightApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using AspNetCore.ReCaptcha;
using InsightApp.Services;
using InsightApp.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Identity;
using InsightApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReCaptcha(options =>
{
    options.SiteKey = builder.Configuration["GoogleReCAPTCHA:SiteKey"];
    options.SecretKey = builder.Configuration["GoogleReCAPTCHA:SecretKey"];
});
// Add services to the container.
builder.Services.AddControllersWithViews();

var connStr = builder.Configuration.GetConnectionString("SVGSContext");
string appPassword = builder.Configuration["EmailServiceConfig:AppPassword"];

builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddDbContext<InsightUpdateCvgs2Context>(options => options.UseSqlServer(connStr));
builder.Services.AddIdentity<Account, AccountRole>(ProgramConstants.GetIdentityOptions()
).AddEntityFrameworkStores<InsightUpdateCvgs2Context>().AddDefaultTokenProviders();
builder.Services.AddTransient<RoleDataSeeder>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
});

builder.Services.AddRazorPages();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<InsightUpdateCvgs2Context>();

    if (dbContext.Database.CanConnect())
    {
        try
        {
            var roleManager = services.GetRequiredService<RoleManager<AccountRole>>();
            var userManager = services.GetRequiredService<UserManager<Account>>();
            var seeder = services.GetRequiredService<RoleDataSeeder>();
            await seeder.SeedRoleDataAsync(roleManager);
            await seeder.SeedAdminAccount(roleManager, userManager);
        }
        catch (Exception)
        {
            throw;
        }
    }
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Redirect from root to /Home/FirstPage
app.MapGet("/", async context =>
{
    context.Response.Redirect("/Home/FirstPage");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=FirstPage}/{id?}");
app.MapRazorPages();

app.Run();
