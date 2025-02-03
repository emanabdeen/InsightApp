using InsightApp.Components;
using InsightApp.Controllers;
using InsightApp.Entities;
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
    public class MemberPreferencesTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly InsightUpdateCvgs2Context _context;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;
        private readonly ServiceProvider _serviceProvider;


        public MemberPreferencesTests()
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

                    INSERT INTO LanguageTable VALUES 
                    (1, 'English'),
                    (2, 'French');
                    
                    INSERT INTO GameCategory VALUES 
                    (1, 'Board'), /*(e.g., Monopoly).*/
                    (2, 'Adventure'), /*(e.g., The Legend of Zelda).*/
                    (3, 'Fighting'), /*(e.g., Street Fighter V).*/
                    (4, 'Survival'), /*(e.g., Minecraft).*/
                    (5, 'Racing'), /*(e.g., Need for Speed).*/
                    (6, 'Horror'), /*(e.g., Resident Evil).*/
                    (7, 'Platformer'); /*(e.g., Super Mario).*/
                    
                    INSERT INTO GamePlatform
                    VALUES
                    (1, 'Nintendo Switch'),
                    (2, 'PC'),
                    (3, 'PS2'),
                    (4, 'PS3'),
                    (5, 'PS4'),
                    (6, 'PS5'),
                    (7, 'Wii U'),
                    (8, 'Xbox 360'),
                    (9, 'Xbox One'),
                    (10, 'Xbox Series X');

                    INSERT INTO MemberGameCategoryPref (Id, MemberId, CategoryId)
                    VALUES
                    (1, 1, 2),
                    (2, 1, 6);

                    INSERT INTO MemberLanguagePref (Id, MemberId, LanguageId)
                    VALUES
                    (1, 1, 1);

                    INSERT INTO MemberPlatformPref (Id, MemberId, PlatformId)
                    VALUES
                    (1, 1, 1),
                    (2, 1, 2);
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
        public async Task GetMemberPrefrences_ExistingPreferences_Successful()
        {
            //Arrange
            var _context = CreateContext();
            int expectedMemberId = 1;
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
            MemberPreferencesViewModel expectedPreferencesViewModel = new MemberPreferencesViewModel()
            {
                SelectedCategoryIds = new List<int>() { 2, 6 },
                SelectedLanguageIds = new List<int>() { 1 },
                SelectedPlatformIds = new List<int>() { 1, 2 },
                MemberId = 1,
            };

            //Act
            var memberPreferencesViewComponent = new MemberPreferencesViewComponent(_context);
            var memberPreferencesViewResult = await memberPreferencesViewComponent.InvokeAsync(expectedMemberId) as ViewViewComponentResult;
            Assert.NotNull(memberPreferencesViewResult);
            var actualResultViewModel = memberPreferencesViewResult?.ViewData.ModelExplorer.Model as MemberPreferencesViewModel;

            //Assert
            Assert.Equal(expectedPreferencesViewModel.SelectedCategoryIds.Count(), actualResultViewModel.SelectedCategoryIds.Count());
            Assert.Equal(expectedPreferencesViewModel.SelectedLanguageIds.Count(), actualResultViewModel.SelectedLanguageIds.Count());
            Assert.Equal(expectedPreferencesViewModel.SelectedPlatformIds.Count(), actualResultViewModel.SelectedPlatformIds.Count());
            Assert.Equal(expectedPreferencesViewModel.MemberId, actualResultViewModel.MemberId);
        }

        [Fact]
        public async Task GetMemberPrefrences_UpdateExistingPreferences_Successful()
        {
            //Arrange
            var _context = CreateContext();
            int expectedMemberId = 1;
            string expectedTempData = "The preferences have been updated.";
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
            MemberPreferencesViewModel expectedPreferencesViewModel = new MemberPreferencesViewModel()
            {
                SelectedCategoryIds = new List<int>() { 2, 6, 7 },
                SelectedLanguageIds = new List<int>() { 1, 2 },
                SelectedPlatformIds = new List<int>() { 1, 2, 7 },
                MemberId = 1,
            };

            //Act
            var result = await controller.SubmitPreferences(expectedPreferencesViewModel);
            var memberPreferencesViewComponent = new MemberPreferencesViewComponent(_context);
            var memberPreferencesViewResult = await memberPreferencesViewComponent.InvokeAsync(expectedMemberId) as ViewViewComponentResult;
            Assert.NotNull(memberPreferencesViewResult);
            var actualResultViewModel = memberPreferencesViewResult?.ViewData.ModelExplorer.Model as MemberPreferencesViewModel;

            //Assert
            Assert.Equal(expectedPreferencesViewModel.SelectedCategoryIds.Count(), actualResultViewModel.SelectedCategoryIds.Count());
            Assert.Equal(expectedPreferencesViewModel.SelectedLanguageIds.Count(), actualResultViewModel.SelectedLanguageIds.Count());
            Assert.Equal(expectedPreferencesViewModel.SelectedPlatformIds.Count(), actualResultViewModel.SelectedPlatformIds.Count());
            Assert.Equal(expectedTempData, controller.TempData["LastActionMessage"]);
            Assert.Equal(expectedPreferencesViewModel.MemberId, actualResultViewModel.MemberId);
        }
    }
}
