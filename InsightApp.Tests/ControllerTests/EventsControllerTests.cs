using InsightApp.Controllers;
using InsightApp.Entities;
using InsightApp.Models;
using InsightApp.Tests.TestData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace InsightApp.Tests.ControllerTests
{
        public class EventControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;


    public static IEnumerable<Object[]> Events
        {
            get
            {
                yield return new object[] { Constants.KWGamesCom};
                yield return new object[] { Constants.Fortnite };
                yield return new object[] { Constants.MagicTc};

            }

        }


        public EventControllerTests()
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
                // check if already exists? not sure but there's an error
                //command.CommandText = File.ReadAllText(@"..\..\..\..\..\DB_WithLoginTables.sql");
                //command.ExecuteNonQuery();
                command.CommandText = """
                    INSERT INTO EventType VALUES 
                    (1, 'Virtual'),
                    (2, 'On-Site');
                   
                   
                    /*Virtual Events*/
                    INSERT INTO GameEvent 
                        (EventId,EventName, Details, StartDate, StartTime, Duration, 
                        EvTypeId, EventLink) 
                        VALUES (1,'KW GamesCom', 'Game convention of Kitchener-Waterloo region',
                         '2025-02-15', '13:00', 
                        '1', 1,'https://www.example.com/events/kwgamescom'), 
                        
                        (2,'Fornite Remix', 'Fornite New Season event','2024-10-20', '13:00', 
                        '1', 1,'https://www.example.com/events/fortniteRemix');
 

                        INSERT INTO EventAddressTable 
                        (AddressId, StreetName, StreetNumber, PostalCode,City,Province,Country ) 
                        VALUES (1,'King Street', '1234', 'A1A1A1','Waterloo','ON','Canada');  


                        /*OnSite Event*/
                        INSERT INTO GameEvent 
                        (EventId, EventName, Details, StartDate, StartTime, Duration, 
                        EvTypeId, AddressId) 
                        VALUES (3,'Magic Tournament', 'Magic Trading Card Game local tournament',
                         '2024-06-20', '10:00', 
                        '8', 2,1);  
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
        public async Task GetAllEvents_NoSearchText_ThreeEventsReturned()
        {   
            //Arrange
            using var context = CreateContext();
            var controller = new EventController(context);

            //Act
            var eventListViewResult = await controller.GetAllEvents(new EventListModel()) as ViewResult;
            Assert.NotNull(eventListViewResult);
            var eventListModel = eventListViewResult?.Model as EventListModel;

            int expectedEventsCount = 3;
        //Assert
        Assert.Equal(expectedEventsCount, eventListModel.EventList.Count);
        }



        [Fact]
        public async Task DeleteEvent_ValidId_Success(){
            var _context = CreateContext();

            Mock<IOptions<GameEvent>> mockGameEvent = new Mock<IOptions<GameEvent>>();

            var controller = new EventController(_context);

            await controller.ProcessDeleteRequest(Constants.Fortnite.EventId);

            var eventListViewResult = await controller.GetAllEvents(new EventListModel()) as ViewResult;
            Assert.NotNull(eventListViewResult);
            var eventListModel = eventListViewResult?.Model as EventListModel;


            GameEvent expectedSoftDeletedEvent = Constants.Fortnite;
            expectedSoftDeletedEvent.IsDeleted = true;

            var actualSoftDeletedEvent = _context.GameEvents.FirstOrDefault(g => g.EventId == Constants.Fortnite.EventId);
            int expectedGamesCount = 2;
  
             //Assert
            Assert.NotNull(eventListModel);
            Assert.Equal(expectedSoftDeletedEvent.IsDeleted, actualSoftDeletedEvent.IsDeleted);
            Assert.Equal(expectedGamesCount, eventListModel.EventList.Count);
             

        }


        


    }
}