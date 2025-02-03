using InsightApp.Entities;

namespace InsightApp.Tests.TestData
{
    public class Constants
    {
        //Languages
        public static readonly LanguageTable English = new LanguageTable() { LanguageId = 1, LanguageName = "English" };

        public static readonly LanguageTable French = new LanguageTable() { LanguageId = 2, LanguageName = "French" };

        //Games
        public static readonly Game Mario = new Game()
        {
            GameId = 1,
            GameName = "Super Mario",
            Details = "game details",
            Price = 10,
            GameImageLink = "~/Imgs/Games/Super Mario.jpg",
            IsDeleted = false,
            GameDetailsLanguages = new List<GameDetailsLanguage>() { new GameDetailsLanguage() { LanguageId = 1 } }
        };

        public static readonly Game Sonic = new Game()
        {
            GameId = 2,
            GameName = "Sonic Mania",
            Details = "game details",
            Price = 20,
            GameImageLink = "~/Imgs/Games/Sonic Mania.jpg",
            IsDeleted = false,
            GameDetailsLanguages = new List<GameDetailsLanguage>() { new GameDetailsLanguage() { LanguageId = 2 }, new GameDetailsLanguage() { LanguageId = 1 } }
        };

        public static readonly Game LifeIsStrange = new Game()
        {
            GameId = 3,
            GameName = "Life is Strange",
            Details = "game details",
            Price = 30,
            GameImageLink = "~/Imgs/Games/Life is Strange.jpg",
            IsDeleted = false,
            GameDetailsLanguages = new List<GameDetailsLanguage>() { new GameDetailsLanguage() { LanguageId = 1 } }
        };

        public static readonly Game Minecraft = new Game()
        {
            GameId = 4,
            GameName = "Minecraft",
            Details = "game details",
            Price = 40,
            GameImageLink = "~/Imgs/Games/Minecraft.jpg",
            IsDeleted = false,
            GameDetailsLanguages = new List<GameDetailsLanguage>() { new GameDetailsLanguage() { LanguageId = 1 } }
        };

        public static readonly Game Monopoly = new Game()
        {
            GameId = 5,
            GameName = "Monopoly",
            Details = "game details",
            Price = 50,
            GameImageLink = "~/Imgs/Games/Monopoly.jpg",
            IsDeleted = false,
            GameDetailsLanguages = new List<GameDetailsLanguage>() { new GameDetailsLanguage() { LanguageId = 2 } }
        };

        //Events
        public static readonly GameEvent KWGamesCom = new GameEvent
        {
            EventId = 1,
            EventName = "KW Games Com",
            Details = "Game convention of Kitchener-Waterloo region",
            StartDate = new DateOnly(2025, 2, 15),
            StartTime = new TimeOnly(10, 0),
            Duration = 1d,
            EvTypeId = 6,
            EventLink = "https://www.example.com/events/kwgamescom"
        };

        public static readonly GameEvent Fortnite = new GameEvent
        {
            EventId = 2,
            EventName = "Fornite Remix",
            Details = "Fornite New Season event",
            StartDate = new DateOnly(2024, 10, 2),
            StartTime = new TimeOnly(13, 0),
            Duration = 1d,
            EvTypeId = 1,
            EventLink = "https://www.example.com/events/forniteRemix"
        };

        public static readonly GameEvent MagicTc = new GameEvent
        {
            EventId = 3,
            EventName = "Magic Trading Cards Tournament",
            Details = "Local Magic Trading Cards Tournament",
            StartDate = new DateOnly(2024, 12, 2),
            StartTime = new TimeOnly(13, 0),
            Duration = 1d,
            EvTypeId = 4,
            AddressId = 1,
        };

        //Orders
        public static readonly OrderTable OrderWithMixedProductTypes = new OrderTable
        {
            OrderId = 1,
            OrderDate = new DateOnly(2024, 10, 20),
            OrderFulfilled = false,
            OrderTime = new TimeOnly(10, 20),
            TotalPayment = 90,
            MemberId = 1,
            OrderItems = new List<OrderItem>{
                 new OrderItem()
                 {
                    GameId = 4,
                    IsPhysical = 0,
                    Quantity = 1,
                    OrderId = 1
                },
                new OrderItem()
                {
                    GameId = 5,
                    IsPhysical = 1,
                    Quantity = 1,
                    OrderId = 1
                }
            }
        };
    }
}