using InsightApp.Controllers;
using InsightApp.Entities;
using InsightApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightApp.Tests.ControllerTests
{
    public class ReviewManagementTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly InsightUpdateCvgs2Context _context;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;
        private readonly ServiceProvider _serviceProvider;

        public ReviewManagementTests()
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

                    INSERT INTO ReviewStatus (StatusId, Statusname)
                    VALUES
                    (1, 'Approved'),
                    (2, 'Pending'),
                    (3, 'Rejected');

                    INSERT INTO Review (MemberId, GameId, StatusId, ReviewBody)
                    VALUES
                    (1, 1, 2, 'Member 1 review on Game 1 - pending'),
                    (1, 2, 1, 'Member 1 review on Game 2 - approved'),
                    (1, 3, 3, 'Member 1 review on Game 3 - declined');
                    """;
                command.ExecuteNonQuery();
                //context.Database.ExecuteSqlRaw("CREATE VIEW GameAverageRating AS SELECT GameId, ROUND(AVG(RateValue), 1) AS AverageRating FROM  GameRating GROUP BY GameId;");
                context.SaveChanges();
            }
        }

        InsightUpdateCvgs2Context CreateContext() => new InsightUpdateCvgs2Context(_contextOptions, true);

        public void Dispose()
        {
            _connection.Dispose();
        }

        [Fact]
        public async Task GetPendingReviews_ReturnOneReview_Success()
        {
            //Arrange
            var context = CreateContext();
            var controller = new GamesController(context);
            int expectedReturnedPendingReviews = 1;
            string expectedReviewOwner = "Salma Essam";
            

            //Act
            var result = await controller.Reviews() as ViewResult;
            Assert.NotNull(result);
            var model = result.Model as List<ReviewViewModel>;

            //Assert
            Assert.Equal(result.ViewName, "Reviews");
            Assert.Equal(expectedReturnedPendingReviews, model.Count);
            Assert.Equal(expectedReviewOwner, model[0].ReviewedBy);
        }

        [Fact]
        public async Task ApprovePendingReview_ReturnZeroReviews_Success()
        {
            //Arrange
            var context = CreateContext();
            var controller = new GamesController(context);
            controller.TempData = new TempDataDictionary(
                new Mock<HttpContext>().Object,
                new Mock<ITempDataProvider>().Object
            );
            int expectedReturnedPendingReviews = 1;
            string expectedReviewOwner = "Salma Essam";
            string expectedTempDataKey = "SuccessMessage";
            string expectedTempDataValue = "Review approved successfully.";


            //Act
            var result = await controller.Reviews() as ViewResult;
            Assert.NotNull(result);
            var model = result.Model as List<ReviewViewModel>;
            var approveReviewResult = controller.ApproveReview(model[0].ReviewId) as RedirectToActionResult;
            Assert.Equal("Reviews", approveReviewResult.ActionName);
            var reviewsResultAfterApproval = await controller.Reviews() as ViewResult;
            var pendingReviewsModelAfterApproval = reviewsResultAfterApproval.Model as List<ReviewViewModel>;


            //Assert
            Assert.Equal(result.ViewName, "Reviews");
            Assert.Equal(expectedReturnedPendingReviews, model.Count);
            Assert.Equal(expectedReviewOwner, model[0].ReviewedBy);
            //After approving the review, returned model should be empty.
            Assert.Equal(expectedTempDataValue, reviewsResultAfterApproval.TempData[expectedTempDataKey]);
            Assert.Equal(0, pendingReviewsModelAfterApproval.Count);
        }

    }
}
