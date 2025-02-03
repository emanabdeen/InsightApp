using InsightApp.Controllers;
using InsightApp.Entities;
using InsightApp.Models;
using InsightApp.Tests.TestData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NuGet.ContentModel;
using SQLitePCL;
using System.Net.Http;
using Xunit.Sdk;

namespace InsightApp.Tests.ControllerTests;

public class CartControllerTests : IDisposable
{

    private readonly ServiceProvider _serviceProvider;

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;

    public static IEnumerable<Object[]> Games
    {
        get
        {
            yield return new object[] { Constants.OrderWithMixedProductTypes };

        }

    }



    public CartControllerTests()
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
        using var context = new InsightUpdateCvgs2Context(_contextOptions, true);

        if (context.Database.EnsureCreated())
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = """
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
    public async Task GetorderToFullfill_OnePendingOrder__Success()
    {
        //Arrange
        using var _context = CreateContext();
        OptionsWrapper<IdentityOptions> optionsWrapper = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
        ProgramConstants.GetIdentityOptions()(optionsWrapper.Value);
        var userValidators = new List<IUserValidator<Account>> { new UserValidator<Account>() };
        var passwordValidator = new List<IPasswordValidator<Account>> { new PasswordValidator<Account>() };
        var userStore = new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(_context);
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
            UserName = "GameDownloader",
            Email = "downloader@gameDownloader.com",
            EmailConfirmed = true
        };
        string validPassword = "Abc123!@#";
        var result = await _userManager.CreateAsync(expectedUser, validPassword);
        Assert.True(result.Succeeded);

        Member newMember = new Member
        {
            AccountId = expectedUser.Id
        };
        var memberResult = _context.Members.Add(newMember);
        await _context.SaveChangesAsync();

        OrderTable newOrder = Constants.OrderWithMixedProductTypes;
        newOrder.MemberId = newMember.MemberId;

        await _context.OrderTables.AddAsync(newOrder);
        await _context.SaveChangesAsync();

        var gameController = new GamesController(_context);
        gameController.TempData = new TempDataDictionary(
            new Mock<HttpContext>().Object,
            new Mock<ITempDataProvider>().Object
            );
        int referenceOrderId = Constants.OrderWithMixedProductTypes.OrderId;
        var orderList = _context.OrderTables.ToList();
        Assert.False(Constants.OrderWithMixedProductTypes.OrderFulfilled);
        int expectedOrderCountNeedingFulfill = 1;

        //Act
        var ordersToFulfillResult = await gameController.OrdersToFulfill() as ViewResult;
        List<OrderTable> actualOrdersToFulfill = ordersToFulfillResult.Model as List<OrderTable>;

        Assert.Equal(expectedOrderCountNeedingFulfill, actualOrdersToFulfill.Count());
        Assert.Equivalent(actualOrdersToFulfill[0], newOrder);
    }

    [Fact]
    public async Task UpdateOrder_FulfillPendingOrder_Success()
    {
        //Arrange
        int expectedOrderCountBeforeFulfill = 1;
        int expectedOrderCountAfterFulfill = 0;
        string expectedTempDataMessage = "The order is fulfilled";
        using var _context = CreateContext();
        OptionsWrapper<IdentityOptions> optionsWrapper = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
        ProgramConstants.GetIdentityOptions()(optionsWrapper.Value);
        var userValidators = new List<IUserValidator<Account>> { new UserValidator<Account>() };
        var passwordValidator = new List<IPasswordValidator<Account>> { new PasswordValidator<Account>() };
        var userStore = new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(_context);
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
            UserName = "GameDownloader",
            Email = "downloader@gameDownloader.com",
            EmailConfirmed = true
        };
        string validPassword = "Abc123!@#";
        var result = await _userManager.CreateAsync(expectedUser, validPassword);
        Assert.True(result.Succeeded);

        Member newMember = new Member
        {
            AccountId = expectedUser.Id
        };
        var memberResult = _context.Members.Add(newMember);
        await _context.SaveChangesAsync();

        OrderTable newOrder = new OrderTable
        {
            OrderId = 1,
            OrderDate = new DateOnly(2024, 3, 21),
            OrderTime = new TimeOnly(10, 0),
            TotalPayment = 40,
            OrderFulfilled = true,
            MemberId = newMember.MemberId
        };

        OrderItem firstOrderItem = new OrderItem
        {
            OrderId = 1,
            GameId = 1,
            MemberId = newMember.MemberId,
            Quantity = 1,
        };
        await _context.OrderTables.AddAsync(newOrder);
        await _context.OrderItems.AddAsync(firstOrderItem);
        await _context.SaveChangesAsync();

        var gameController = new GamesController(_context);
        gameController.TempData = new TempDataDictionary(
            new Mock<HttpContext>().Object,
            new Mock<ITempDataProvider>().Object
            );
        int referenceOrderId = Constants.OrderWithMixedProductTypes.OrderId;
        var orderList = _context.OrderTables.ToList();
        Assert.False(Constants.OrderWithMixedProductTypes.OrderFulfilled);

        //Act
        var orderViewResult = await gameController.FulfillOrder(Constants.OrderWithMixedProductTypes.OrderId) as RedirectToActionResult;
        var ordersAfterFulfillingViewResult = await gameController.OrdersToFulfill() as ViewResult;
        List<OrderTable> actualOrdersToFulfill = ordersAfterFulfillingViewResult.Model as List<OrderTable>;
        Assert.NotNull(ordersAfterFulfillingViewResult);

        //Assert
        Assert.Equal(expectedOrderCountBeforeFulfill, orderList.Count());
        Assert.Equal(expectedOrderCountAfterFulfill, actualOrdersToFulfill.Count());
        Assert.Equal(expectedTempDataMessage, ordersAfterFulfillingViewResult.TempData["LastActionMessage"]);
    }
}