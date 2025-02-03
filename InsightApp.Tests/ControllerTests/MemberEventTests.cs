using InsightApp.Controllers;
using InsightApp.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace InsightApp.Tests.ControllerTests
{
    public class MemberEventTests : IDisposable
    {

        private readonly SqliteConnection _connection;
        private readonly InsightUpdateCvgs2Context _context;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;
        private readonly ServiceProvider _serviceProvider;


        public MemberEventTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            // Set the options
            _contextOptions = new DbContextOptionsBuilder<InsightUpdateCvgs2Context>()
                .UseSqlite(_connection)
                .Options;

            var services = new ServiceCollection();
            services.AddDbContext<InsightUpdateCvgs2Context>(options => options.UseSqlite(_connection));
            services.AddLogging();
            services.AddIdentity<Account, AccountRole>(options =>
            {
                options.Tokens.ProviderMap.Add(TokenOptions.DefaultEmailProvider, new TokenProviderDescriptor(typeof(IUserTwoFactorTokenProvider<Account>)));
                options.Tokens.EmailConfirmationTokenProvider = "Default";
            })
                .AddEntityFrameworkStores<InsightUpdateCvgs2Context>();
            _serviceProvider = services.BuildServiceProvider();

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InsightUpdateCvgs2Context>();

            if (context.Database.EnsureCreated())
            {
                using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = """
                    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,AccessFailedCount) 
                    VALUES ('76C5E2D4-63D2-490E-9D7C-96BC2F4E4D86', 'SalmaEssam', 'SALMAESSAM','salma@mailinator.com', 'SALMA@MAILINATOR.COM', 1,'123-123-1234', 1, 0,1,0);

                    INSERT INTO Member (MemberId, FirstName, LastName, DisplayName, Gender, AccountId) VALUES 
                    (1, 'Salma', 'Essam','Salma Essam' ,'Female', '76C5E2D4-63D2-490E-9D7C-96BC2F4E4D86');

                    INSERT INTO EventType (EvTypeId, EvTypeName) VALUES
                    (1, "Virtual");
                    INSERT INTO EventType (EvTypeId, EvTypeName) VALUES
                    (2, "On-Site");

                    INSERT INTO EventAddressTable (AddressId, StreetName, StreetNumber, PostalCode, City, Province, Country) VALUES
                    (1, "Westmount Street", 123, "n6h 7f7", "Kitchener", "Ontario", "Canada");
                    INSERT INTO EventAddressTable (AddressId, StreetName, StreetNumber, PostalCode, City, Province, Country) VALUES
                    (2, "Waterloo Ave", 33, "N2H 7N4", "Guelph", "Ontario", "Canada");
                    INSERT INTO EventAddressTable (AddressId, StreetName, StreetNumber, PostalCode, City, Province, Country) VALUES
                    (3, "Weber Street", 12345, "N6R 2J5", "Waterloo", "Ontario", "Canada");
                    
                    INSERT INTO GameEvent (EventId, EventName, Details, StartDate, StartTime, EndTime, EvTypeId, IsDeleted, AddressId) VALUES
                    (1, "KW GamesCom", "Kw GamesCom details", '2025-02-15', '13:00', '14:00', 2, 0, 3);
                    INSERT INTO GameEvent (EventId, EventName, Details, StartDate, StartTime, EndTime, EvTypeId, IsDeleted, AddressId) VALUES
                    (2, "Tokyo Game Show", "Tokyo Game Show details", '2024-02-15', '15:00', '20:00', 2, 0, 1);
                    """;
                command.ExecuteNonQuery();
                context.SaveChanges();
            }
        }

        InsightUpdateCvgs2Context CreateContext() => new InsightUpdateCvgs2Context(_contextOptions, true);

        public void Dispose()
        {
            _connection.Dispose();
        }

        [Fact]
        public async Task MemberRegisterToEvent_FutureEvent_Successful()
        {
            var context = CreateContext();
            string expectedTempData = "Successfully registered!";
            int validFutureEventId = 1;
            OptionsWrapper<IdentityOptions> optionsWrapper = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
            ProgramConstants.GetIdentityOptions()(optionsWrapper.Value);
            var userValidators = new List<IUserValidator<Account>> { new UserValidator<Account>() };
            var passwordValidator = new List<IPasswordValidator<Account>> { new PasswordValidator<Account>() };
            var userStore = new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(context);
            var mockedLogger = new Mock<ILogger<UserManager<Account>>>();
            var _userManager = new UserManager<Account>(
                userStore,
                optionsWrapper,
                new PasswordHasher<Account>(),
                userValidators,
                passwordValidator,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                _serviceProvider,
                mockedLogger.Object
                );
            Account expectedUser = new Account
            {
                UserName = "EventGoer",
                Email = "eventgoer@iloveevents.com",
                EmailConfirmed = true
            };
            string validPassword = "Abc123!@#";
            var result = await _userManager.CreateAsync(expectedUser, validPassword);
            Assert.True(result.Succeeded);

            Member newMember = new Member
            {
                AccountId = expectedUser.Id
            };
            var memberResult = context.Members.Add(newMember);
            await context.SaveChangesAsync();

            //Because the RegisterToEvent action uses the "User" as input and that comes from weird Identity ClaimsPrincipal stuff...
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, expectedUser.Id.ToString())
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            ClaimsPrincipal userClaimsPrincipal = new ClaimsPrincipal(identity);

            //Then have to put it into an HttpContext to inject into the controller
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.User = userClaimsPrincipal;

            PortalEventsController controller = new PortalEventsController(context, _userManager);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new TempDataDictionary(
                new Mock<HttpContext>().Object,
                new Mock<ITempDataProvider>().Object
                );
            
            //Action
            var registerActionResult = await controller.RegisterToEvent(validFutureEventId) as RedirectToActionResult;
            bool isRegisteredForEvent = await context.MemberEventRegists.AnyAsync(er => er.EventId == validFutureEventId && er.MemberId == newMember.MemberId);

            Assert.True(registerActionResult.ActionName == "GetEvents");
            Assert.True(registerActionResult.ControllerName == "PortalEvents");
            Assert.Equal(expectedTempData, controller.TempData["LastActionMessage"]);
            Assert.True(isRegisteredForEvent);
        }

        [Fact]
        public async Task MemberRegisterToEvent_AlreadyRegistered_Fail()
        {
            var context = CreateContext();
            string expectedTempData = "You are already registered for this event.";
            int validFutureEventId = 1;
            OptionsWrapper<IdentityOptions> optionsWrapper = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
            ProgramConstants.GetIdentityOptions()(optionsWrapper.Value);
            var userValidators = new List<IUserValidator<Account>> { new UserValidator<Account>() };
            var passwordValidator = new List<IPasswordValidator<Account>> { new PasswordValidator<Account>() };
            var userStore = new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(context);
            var mockedLogger = new Mock<ILogger<UserManager<Account>>>();
            var _userManager = new UserManager<Account>(
                userStore,
                optionsWrapper,
                new PasswordHasher<Account>(),
                userValidators,
                passwordValidator,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                _serviceProvider,
                mockedLogger.Object
                );
            Account expectedUser = new Account
            {
                UserName = "EventGoer",
                Email = "eventgoer@iloveevents.com",
                EmailConfirmed = true
            };
            string validPassword = "Abc123!@#";
            var result = await _userManager.CreateAsync(expectedUser, validPassword);
            Assert.True(result.Succeeded);

            Member newMember = new Member
            {
                AccountId = expectedUser.Id
            };
            var memberResult = context.Members.Add(newMember);
            await context.SaveChangesAsync();

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, expectedUser.Id.ToString())
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            ClaimsPrincipal userClaimsPrincipal = new ClaimsPrincipal(identity);

            HttpContext httpContext = new DefaultHttpContext();
            httpContext.User = userClaimsPrincipal;

            PortalEventsController controller = new PortalEventsController(context, _userManager);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new TempDataDictionary(
                new Mock<HttpContext>().Object,
                new Mock<ITempDataProvider>().Object
                );

            //Action
            var registerActionResult = await controller.RegisterToEvent(validFutureEventId) as RedirectToActionResult;
            bool isRegisteredForEvent = await context.MemberEventRegists.AnyAsync(er => er.EventId == validFutureEventId && er.MemberId == newMember.MemberId);
            var registerAgainActionResult = await controller.RegisterToEvent(validFutureEventId) as RedirectToActionResult;

            Assert.True(registerActionResult.ActionName == "GetEvents");
            Assert.True(isRegisteredForEvent);
            Assert.True(registerAgainActionResult.ActionName == "GetEventById");
            Assert.True(registerAgainActionResult.ControllerName == "PortalEvents");
            Assert.Equal(expectedTempData, controller.TempData["LastActionMessage"]);
        }
    }
}
