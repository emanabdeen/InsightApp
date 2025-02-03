using InsightApp.Controllers;
using InsightApp.Entities;
using InsightApp.Models;
using InsightApp.Tests.TestData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Tests.ControllerTests
{
        public class ProductControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;

        public static IEnumerable<Object[]> Games
        {
            get
            {
                yield return new object[] { Constants.Mario };
                yield return new object[] { Constants.Sonic };
                yield return new object[] { Constants.LifeIsStrange };
                yield return new object[] { Constants.Minecraft };
                yield return new object[] { Constants.Monopoly };
            }

        }


        public ProductControllerTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            // Set the options
            _contextOptions = new DbContextOptionsBuilder<InsightUpdateCvgs2Context>()
                .UseSqlite(_connection)
                .Options;

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
                context.Database.ExecuteSqlRaw("CREATE VIEW GameAverageRating AS SELECT GameId, ROUND(AVG(RateValue), 1) AS AverageRating FROM  GameRating GROUP BY GameId;");
                context.SaveChanges();
            }
        }

        InsightUpdateCvgs2Context CreateContext() => new InsightUpdateCvgs2Context(_contextOptions, true);

        public void Dispose()
        {
            _connection.Dispose();
        }



        [Theory] //currently this validates that the game details returned match the game id, name, price, imageLink and languages.
        [MemberData(nameof(Games))]
            public async Task GetProductDetails_ValidGameId_ReturnCorrectGame(Game expectedGame)
            {   
                //Arrange
                using var context = CreateContext();
                var controller = new GamesController(context);

                //Act
                var gameDetailsViewResult = await controller.Details(expectedGame.GameId) as ViewResult;
                Assert.NotNull(gameDetailsViewResult);
                var editGameViewModel = gameDetailsViewResult?.Model as ProductDetailsViewModel;

                //Assert
                Assert.Equal(gameDetailsViewResult.ViewData["Title"], "Game Details");
                Assert.Equal(gameDetailsViewResult.ViewName, "Details");
                Assert.Equal(expectedGame.GameId, editGameViewModel.ActiveGame.GameId);
                Assert.Equal(expectedGame.GameName, editGameViewModel.ActiveGame.GameName);
                Assert.Equal(expectedGame.Price, editGameViewModel.ActiveGame.Price);
                Assert.Equal(expectedGame.GameImageLink, editGameViewModel.ActiveGame.GameImageLink);
                foreach(var language in expectedGame.GameDetailsLanguages)
                {
                    Assert.Contains(editGameViewModel.ActiveGame.GameDetailsLanguages, lang => lang.LanguageId == language.LanguageId);
                }
            }



        [Theory] //GA001
        [InlineData("Mario", 1, new string[]{"Super Mario"})]
        [InlineData("Platformer", 2, new string[]{"Super Mario", "Sonic Mania"})]
        public async Task GetAllGames_SearchTextUsed_ExpectedGamesReturned(string searchText, int expectedGameCount, string[] expectedGameNames)
        {
            //Arrange
            using var context = CreateContext();
            var controller = new GamesController(context);

            //Act
            var gamesViewResult = await controller.List(new GamesListModel() { SearchText = searchText }) as ViewResult;
            Assert.NotNull(gamesViewResult);
            var gamesListModel = gamesViewResult?.Model as GamesListModel;

            //Assert
            Assert.Equal(expectedGameCount, gamesListModel.GamesList.Count);
            foreach (string expectedGameName in expectedGameNames)
            {
                Assert.Contains(gamesListModel.GamesList, game => game.GameName == expectedGameName);
            }
        }


    }
}