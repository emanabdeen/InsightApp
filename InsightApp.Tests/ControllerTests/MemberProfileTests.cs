using InsightApp.Components;
using InsightApp.Controllers;
using InsightApp.Entities;
using InsightApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace InsightApp.Tests.ControllerTests
{
    public class MemberProfileTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly InsightUpdateCvgs2Context _context;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;
        private readonly ServiceProvider _serviceProvider;


        public MemberProfileTests()
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
            context.Database.EnsureCreated();
        }

        InsightUpdateCvgs2Context CreateContext() => new InsightUpdateCvgs2Context(_contextOptions, true);

        public void Dispose()
        {
            _connection.Dispose();
        }

        [Fact]
        public async Task GetMemberProfile_ValidId_ReturnMemberProfileDetails()
        {
            //Arrange
            var _context = CreateContext();
            Mock<IOptions<IdentityOptions>> mockIdentityOptions = new Mock<IOptions<IdentityOptions>>();
            var _userManager = new UserManager<Account>(new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(_context),
                mockIdentityOptions.Object,
                new PasswordHasher<Account>(),
                new IUserValidator<Account>[0],
                new IPasswordValidator<Account>[0],
                null,
                null,
                _serviceProvider,
                null
                );

            Account newUser = new Account
            {
                UserName = "Gamer123",
                Email = "gamer123@testaccount.com",
                EmailConfirmed = true
            };
            string password = "Abc123!";

            var result = await _userManager.CreateAsync(newUser, password);
            Assert.True(result.Succeeded);
            string userId = await _userManager.GetUserIdAsync(newUser);
            Member newMember = new Member
            {
                AccountId = new Guid(userId),
                DisplayName = newUser.UserName,
                FirstName = "Tim",
                LastName = "Horton",
                Gender = "Male",
                Dob = DateOnly.Parse("2000-10-02"),
                RecievesEmails = true,
                PhoneNumber = "226-555-5555",
                
            };
            Assert.True(Utilities.UtilityMethods.ValidateModel(newMember).Count == 0);
            var memberResult = _context.Members.Add(newMember);
            await _context.SaveChangesAsync();

            //Act
            var memberProfileViewComponent = new MemberProfileViewComponent(_context);
            var memberProfileViewResult = await memberProfileViewComponent.InvokeAsync(memberResult.Entity.MemberId) as ViewViewComponentResult;
            Assert.NotNull(memberProfileViewResult);
            var memberProfileViewModel = memberProfileViewResult?.ViewData.ModelExplorer.Model as MemberProfileViewModel;

            //Assert
            Assert.Equal(newUser.Id, memberProfileViewModel.ActiveMember.AccountId);
            Assert.Equal(newMember.MemberId, memberProfileViewModel.ActiveMember.MemberId);
            Assert.Equal(newMember.DisplayName, memberProfileViewModel.ActiveMember.DisplayName);
            Assert.Equal(newMember.FirstName, memberProfileViewModel.ActiveMember.FirstName);
            Assert.Equal(newMember.LastName, memberProfileViewModel.ActiveMember.LastName);
            Assert.Equal(newMember.Dob, memberProfileViewModel.ActiveMember.Dob);
            Assert.Equal(newMember.RecievesEmails, memberProfileViewModel.ActiveMember.RecievesEmails);
            Assert.Equal(newMember.PhoneNumber, memberProfileViewModel.ActiveMember.PhoneNumber);
        }

        [Fact]
        public async Task AddUserProfileDetails_ValidInputModel_SuccessfulAdd()
        {
            //Arrange
            var _context = CreateContext();
            Mock<IOptions<IdentityOptions>> mockIdentityOptions = new Mock<IOptions<IdentityOptions>>();
            var _userManager = new UserManager<Account>(new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(_context),
                mockIdentityOptions.Object,
                new PasswordHasher<Account>(),
                new IUserValidator<Account>[0],
                new IPasswordValidator<Account>[0],
                null,
                null,
                _serviceProvider,
                null
                );
            var _signInManager = new SignInManager<Account>(_userManager,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<Account>>().Object,
                null,
                null,
                null,
                null
                );
            var controller = new MemberController(_context, _userManager, _signInManager);
            controller.TempData = new TempDataDictionary(
                new Mock<HttpContext>().Object,
                new Mock<ITempDataProvider>().Object
                );

            Account newUser = new Account
            {
                UserName = "Gamer123",
                Email = "gamer123@testaccount.com",
                EmailConfirmed = true
            };
            
            string password = "Abc123!";

            var result = await _userManager.CreateAsync(newUser, password);
            Assert.True(result.Succeeded);
            string userId = await _userManager.GetUserIdAsync(newUser);
            Member newMember = new Member
            {
                AccountId = new Guid(userId),
            };
            var memberResult = _context.Members.Add(newMember);
            await _context.SaveChangesAsync();

            MemberProfileViewModel inputProfileViewModel = new MemberProfileViewModel()
            {
                ActiveMember = memberResult.Entity as Member
            };
            inputProfileViewModel.ActiveMember.PhoneNumber = "555-234-4411";
            inputProfileViewModel.ActiveMember.DisplayName = newUser.UserName;
            inputProfileViewModel.ActiveMember.FirstName = "Casey";
            inputProfileViewModel.ActiveMember.LastName = "Snoek";
            inputProfileViewModel.ActiveMember.Dob = DateOnly.Parse("2000-01-01");
            inputProfileViewModel.ActiveMember.Gender = "Male";
            inputProfileViewModel.ActiveMember.RecievesEmails = true;

            //Act
            Assert.True(Utilities.UtilityMethods.ValidateModel(inputProfileViewModel.ActiveMember).Count == 0);
            await controller.EditMemberProfileId(inputProfileViewModel) ;
            var memberProfileViewComponent = new MemberProfileViewComponent(_context);
            var memberProfileViewResult = await memberProfileViewComponent.InvokeAsync(inputProfileViewModel.ActiveMember.MemberId) as ViewViewComponentResult;
            Assert.NotNull(memberProfileViewResult);
            var memberProfileViewModel = memberProfileViewResult?.ViewData.ModelExplorer.Model as MemberProfileViewModel;

            //Assert
            Assert.Equal(inputProfileViewModel.ActiveMember.AccountId, memberProfileViewModel.ActiveMember.AccountId);
            Assert.Equal(inputProfileViewModel.ActiveMember.MemberId, memberProfileViewModel.ActiveMember.MemberId);
            Assert.Equal(inputProfileViewModel.ActiveMember.DisplayName, memberProfileViewModel.ActiveMember.DisplayName);
            Assert.Equal(inputProfileViewModel.ActiveMember.FirstName, memberProfileViewModel.ActiveMember.FirstName);
            Assert.Equal(inputProfileViewModel.ActiveMember.LastName, memberProfileViewModel.ActiveMember.LastName);
            Assert.Equal(inputProfileViewModel.ActiveMember.Dob, memberProfileViewModel.ActiveMember.Dob);
            Assert.Equal(inputProfileViewModel.ActiveMember.RecievesEmails, memberProfileViewModel.ActiveMember.RecievesEmails);
            Assert.Equal(inputProfileViewModel.ActiveMember.PhoneNumber, memberProfileViewModel.ActiveMember.PhoneNumber);
        }
    }
}
