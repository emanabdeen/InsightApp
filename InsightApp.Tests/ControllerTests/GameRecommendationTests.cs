using InsightApp.Controllers;
using InsightApp.Entities;
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
using InsightApp.Models;
using InsightApp.Tests.TestData;

namespace InsightApp.Tests.ControllerTests
{
    public class GameRecommendationTests : IDisposable
    {

        private readonly SqliteConnection _connection;
        private readonly InsightUpdateCvgs2Context _context;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;
        private readonly ServiceProvider _serviceProvider;


        public GameRecommendationTests()
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

                    INSERT INTO Game (GameId, GameName, Details, Price, GameImageLink, isDeleted) VALUES 
                    (1, 'Super Mario', 'game details', 10, '~/Imgs/Games/Super Mario.jpg', 0),
                    (2, 'Sonic Mania', 'game details', 20, '~/Imgs/Games/Sonic Mania.jpg', 0),
                    (3, 'Life is Strange', 'game details',30, '~/Imgs/Games/Life is Strange.jpg', 0),
                    (4, 'Minecraft', 'game details', 40, '~/Imgs/Games/Minecraft.jpg', 0),
                    (5, 'Monopoly', 'game details', 50, '~/Imgs/Games/Monopoly.jpg', 0);
                    
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
                    """;
                command.ExecuteNonQuery();
                context.Database.ExecuteSqlRaw("CREATE VIEW GameAverageRating AS SELECT GameId, ROUND(AVG(RateValue), 1) AS AverageRating FROM  GameRating GROUP BY GameId;");
                context.SaveChanges();
            }
        }

        InsightUpdateCvgs2Context CreateContext() => new InsightUpdateCvgs2Context(_contextOptions, true);

        public void Dispose()
        {
            _connection.Dispose();
        }

        [Fact]
        public async Task GetGameRecommendationsFromCategoryPref_ExpectReturnOnlyLifeIsStrange_Success()
        {
            var context = CreateContext();
            int expectedGameRecommendationCount = 1;
            string expectedGamePreferenceMatch = "Category";
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
                UserName = "GamePlayer",
                Email = "iplaythegames@fun.com",
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

            var memberCategoryPrefResult = context.MemberGameCategoryPrefs.Add(new MemberGameCategoryPref() { MemberId = expectedMember.MemberId, CategoryId = 2 });
            await context.SaveChangesAsync();

            //Create a claim to set this user as the logged in user.
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, expectedUser.Id.ToString())
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            ClaimsPrincipal userClaimsPrincipal = new ClaimsPrincipal(identity);

            //Then have to put it into an HttpContext to inject into the controller
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.User = userClaimsPrincipal;

            ProductController controller = new ProductController(context, _userManager);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new TempDataDictionary(
                new Mock<HttpContext>().Object,
                new Mock<ITempDataProvider>().Object
            );

            //Act
            var gameDetailsResult = await controller.details(Constants.Mario.GameId) as ViewResult;
            Assert.NotNull(gameDetailsResult);
            var model = gameDetailsResult.Model as ProductDetailsViewModel;
            var viewDataPreferencesMap = gameDetailsResult.ViewData["PreferencesMap"] as Dictionary<int, List<string>>;
            Assert.True(viewDataPreferencesMap.ContainsKey(Constants.LifeIsStrange.GameId));
            var gamePreferenceMatch = viewDataPreferencesMap[Constants.LifeIsStrange.GameId];


            //Assert
            Assert.Equal(expectedGameRecommendationCount, model.GamesFromPreferences.Count);
            Assert.Equal(Constants.LifeIsStrange.GameId, model.GamesFromPreferences[0].GameId);
            Assert.Equal(Constants.LifeIsStrange.GameName, model.GamesFromPreferences[0].GameName);
            Assert.Equal(expectedGamePreferenceMatch, gamePreferenceMatch[0]);
        }

        [Fact]
        public async Task GetRelatedGameRecommendationsForMarioGame_ExpectReturnSonicMania_Success()
        {
            var context = CreateContext();
            int expectedRelatedGameCount = 1;
            string expectedGameCategoryMatch = "Platformer";
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
                UserName = "GamePlayer",
                Email = "iplaythegames@fun.com",
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

            //Create a claim to set this user as the logged in user
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, expectedUser.Id.ToString())
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            ClaimsPrincipal userClaimsPrincipal = new ClaimsPrincipal(identity);

            //Then have to put it into an HttpContext to inject into the controller
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.User = userClaimsPrincipal;

            ProductController controller = new ProductController(context, _userManager);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new TempDataDictionary(
                new Mock<HttpContext>().Object,
                new Mock<ITempDataProvider>().Object
            );

            //Act
            var gameDetailsResult = await controller.details(Constants.Mario.GameId) as ViewResult;
            Assert.NotNull(gameDetailsResult);
            var model = gameDetailsResult.Model as ProductDetailsViewModel;

            //Assert
            Assert.Equal(expectedRelatedGameCount, model.RelatedGames.Count);
            Assert.Equal(Constants.Sonic.GameId, model.RelatedGames[0].GameId);
            Assert.Equal(Constants.Sonic.GameName, model.RelatedGames[0].GameName);
            Assert.Equal(expectedGameCategoryMatch, model.SharedGameCategories[Constants.Sonic.GameId]);
        }
    }
}
