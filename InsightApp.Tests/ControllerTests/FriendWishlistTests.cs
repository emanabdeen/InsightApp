using InsightApp.Controllers;
using InsightApp.Entities;
using InsightApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using NuGet.ContentModel;

namespace InsightApp.Tests.ControllerTests
{
    public class FriendWishlistTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly InsightUpdateCvgs2Context _context;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;
        private readonly ServiceProvider _serviceProvider;


        public FriendWishlistTests()
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

                    INSERT INTO Game (GameId, GameName, Details, Price, GameImageLink) VALUES 
                    (1, 'Super Mario', 'game details', 10, '~/Imgs/Games/Super Mario.jpg'),
                    (2, 'Sonic Mania', 'game details', 20, '~/Imgs/Games/Sonic Mania.jpg'),
                    (3, 'Life is Strange', 'game details',30, '~/Imgs/Games/Life is Strange.jpg'),
                    (4, 'Minecraft', 'game details', 40, '~/Imgs/Games/Minecraft.jpg'),
                    (5, 'Monopoly', 'game details', 50, '~/Imgs/Games/Monopoly.jpg');
                    
                    INSERT INTO GameDetailsCategory (GameId, CategoryId)
                    VALUES 
                    (1, 7), -- Super Mario is in Platformer category
                    (2, 7), -- Sonic Mania is in Platformer category
                    (3, 2), -- Life is Strange is in Adventure category
                    (4, 4), -- Minecraft is in Survival category
                    (5, 1); -- Monopoly is in Board category
                    
                    INSERT INTO GameDetailsLanguage (GameId, LanguageId)
                    VALUES 
                    (1, 1), -- Super Mario is in English
                    (2, 2), -- Sonic Mania is in French
                    (2, 1), -- Sonic Mania is in English
                    (3, 1), -- Life is Strange is in English
                    (4, 1), -- Minecraft is in English
                    (5, 2); -- Monopoly is in French
                    
                    INSERT INTO GameDetailsPlatform (GameId, PlatformId)
                    VALUES
                    (1,1),
                    (2,7),
                    (3,3),
                    (4,2),
                    (5,8);

                    INSERT INTO WishList (MemberId, GameId)
                    VALUES
                    (1, 1),
                    (1, 3),
                    (1, 5);
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
        public async Task AddFriendAndViewFriendWishlist_ReturnsFriendWishlist_Success()
        {
            var context = CreateContext();
            int expectedFriendId = 1;
            int expectedFriendWishlistGameCount = 3;
            List<String> expectedGames = new List<String>() { "Super Mario", "Life is Strange", "Monopoly" };
            string expectedFriendName = "SalmaEssam";
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
            var _signInManager = new SignInManager<Account>(_userManager,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<Account>>().Object,
                null,
                null,
                null,
                null
                );
            Account expectedUser = new Account
            {
                UserName = "FriendHaver",
                Email = "befriendme@friendly.com",
                EmailConfirmed = true
            };
            string validPassword = "Abc123!@#";
            var result = await _userManager.CreateAsync(expectedUser, validPassword);
            Assert.True(result.Succeeded);

            Member expectedMember = new Member
            {
                AccountId = expectedUser.Id
            };
            var memberResult = context.Members.Add(expectedMember);
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

            MemberController controller = new MemberController(context, _userManager, _signInManager);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new TempDataDictionary(
                new Mock<HttpContext>().Object,
                new Mock<ITempDataProvider>().Object
            );

            //Act
            var addFriendResult = await controller.AddFriend(expectedFriendId) as RedirectToActionResult;
            Assert.True(addFriendResult.ActionName == "GetFriends");
            var getFriendsListResult = await controller.GetFriends() as ViewResult;
            var model = getFriendsListResult.Model as List<FriendViewModel>;
            Assert.NotNull(model);
            Assert.True(model[0].FriendId == expectedFriendId);

            var friendWishlistResult = await controller.GetFriendWishList(expectedFriendId) as ViewResult;
            var friendWishlistModel = friendWishlistResult.Model as List<WishlistViewModel>;
            Assert.NotNull(friendWishlistModel);

            //Assert
            Assert.True(friendWishlistModel.Count == expectedFriendWishlistGameCount);
            Assert.True(controller.ViewBag.Friend == expectedFriendName);
            foreach (var viewmodel in friendWishlistModel)
            {
                Assert.True(expectedGames.Contains(viewmodel.GameName));
            }
        }

        [Fact]
        public async Task ViewWishlistOfNonFriend_ReturnsError_Success()
        {
            var context = CreateContext();
            int expectedFriendId = 1;
            int expectedFriendWishlistGameCount = 3;
            List<String> expectedGames = new List<String>() { "Super Mario", "Life is Strange", "Monopoly" };
            string expectedViewbagContent = "Non-Friend";
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
            var _signInManager = new SignInManager<Account>(_userManager,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<Account>>().Object,
                null,
                null,
                null,
                null
                );
            Account expectedUser = new Account
            {
                UserName = "FriendHaver",
                Email = "befriendme@friendly.com",
                EmailConfirmed = true
            };
            string validPassword = "Abc123!@#";
            var result = await _userManager.CreateAsync(expectedUser, validPassword);
            Assert.True(result.Succeeded);

            Member expectedMember = new Member
            {
                AccountId = expectedUser.Id
            };
            var memberResult = context.Members.Add(expectedMember);
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

            MemberController controller = new MemberController(context, _userManager, _signInManager);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new TempDataDictionary(
                new Mock<HttpContext>().Object,
                new Mock<ITempDataProvider>().Object
            );

            //Act
            var friendWishlistResult = await controller.GetFriendWishList(expectedFriendId) as ViewResult;
            var friendWishlistModel = friendWishlistResult.Model as List<WishlistViewModel>;

            //Assert
            Assert.Null(friendWishlistModel);
            Assert.True(controller.ViewBag.Friend == expectedViewbagContent);
        }
    }
}
