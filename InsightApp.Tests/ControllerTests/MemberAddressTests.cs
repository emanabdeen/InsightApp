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
    public class MemberAddressTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly InsightUpdateCvgs2Context _context;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;
        private readonly ServiceProvider _serviceProvider;


        public MemberAddressTests()
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
                    INSERT INTO Country (id, CountryName) VALUES (1, 'Canada');

                    INSERT INTO Province VALUES (1, 'Alberta');
                    INSERT INTO Province VALUES (2, 'British Columbia');
                    INSERT INTO Province VALUES (3, 'Manitoba');
                    INSERT INTO Province VALUES (4, 'New Brunswick');
                    INSERT INTO Province VALUES (5, 'Newfoundland and Labrador');
                    INSERT INTO Province VALUES (6, 'Nova Scotia');
                    INSERT INTO Province VALUES (7, 'Ontario');
                    INSERT INTO Province VALUES (8, 'Prince Edward Island');
                    INSERT INTO Province VALUES (9, 'Quebec');
                    INSERT INTO Province VALUES (10, 'Saskatchewan');
                    INSERT INTO Province VALUES (11, 'Northwest Territories');
                    INSERT INTO Province VALUES (12, 'Nunavut');
                    INSERT INTO Province VALUES (13, 'Yukon');
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
        public async Task AddMemberAddress_ValidInput_SuccessfulAdd()
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
            AddressTable expectedAddress = new AddressTable()
            {
                MemberId = memberResult.Entity.MemberId,
                StreetName = "Fake Street",
                StreetNumber = "123",
                Unit = "1",
                PostalCode = "N1E6R1",
                City = "Waterloo",
                Province = "Ontario",
                Country = "Canada",
                IsShipping = false,
                DelivaryInstructions = "Do not knock or ring doorbell"
            };
            AddressTable shippingAddress = new AddressTable()
            {
                IsShipping = true
            };
            MemberAddressesViewModel addressViewModel = new MemberAddressesViewModel()
            {
                MemberId = (int)expectedAddress.MemberId,
                IsAdressesSame = true,
                MemberAddress = expectedAddress,
                ShippingAddress = shippingAddress,
            };


            //Act
            Assert.True(Utilities.UtilityMethods.ValidateModel(expectedAddress).Count == 0);
            await controller.AddAddressesById(addressViewModel);
            var memberAddressesViewComponent = new MemberAddressesViewComponent(_context);
            var memberAddressesViewResult = await memberAddressesViewComponent.InvokeAsync(memberResult.Entity.MemberId) as ViewViewComponentResult;
            Assert.NotNull(memberAddressesViewResult);
            var memberAddressesViewModel = memberAddressesViewResult?.ViewData.ModelExplorer.Model as MemberAddressesViewModel;


            //Assert
            Assert.Equal(expectedAddress.MemberId, memberAddressesViewModel.MemberAddress.MemberId);
            Assert.Equal(expectedAddress.StreetName, memberAddressesViewModel.MemberAddress.StreetName);
            Assert.Equal(expectedAddress.StreetNumber, memberAddressesViewModel.MemberAddress.StreetNumber);
            Assert.Equal(expectedAddress.Unit, memberAddressesViewModel.MemberAddress.Unit);
            Assert.Equal(expectedAddress.City, memberAddressesViewModel.MemberAddress.City);
            Assert.Equal(expectedAddress.PostalCode, memberAddressesViewModel.MemberAddress.PostalCode);
            Assert.Equal(expectedAddress.Country, memberAddressesViewModel.MemberAddress.Country);
            Assert.Equal(expectedAddress.Province, memberAddressesViewModel.MemberAddress.Province);
            Assert.Equal(shippingAddress.DelivaryInstructions, memberAddressesViewModel.ShippingAddress.DelivaryInstructions);
        }

        [Fact]
        public async Task AddMemberAddress_MissingField_FailValidation()
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
            AddressTable expectedAddress = new AddressTable()
            {
                MemberId = memberResult.Entity.MemberId,
                StreetName = "Fake Street",
                StreetNumber = "123",
                Unit = "1",
                PostalCode = "N1E 6R1",
                City = "Waterloo",
                Province = "Ontario",
                Country = "Canada",
                IsShipping = false,
                DelivaryInstructions = "Do not knock or ring doorbell"
            };
            AddressTable shippingAddress = new AddressTable()
            {
                IsShipping = true
            };
            MemberAddressesViewModel addressViewModel = new MemberAddressesViewModel()
            {
                MemberId = (int)expectedAddress.MemberId,
                IsAdressesSame = true,
                MemberAddress = expectedAddress,
                ShippingAddress = shippingAddress,
            };


            //Act
            await controller.AddAddressesById(addressViewModel);
            //Set up an invalid address model to validate and pass into action to update with
            AddressTable invalidUpdateAddress = expectedAddress;
            invalidUpdateAddress.StreetName = null;
            //validate and then update modelstate in controller
            var validationResults = Utilities.UtilityMethods.ValidateModel(expectedAddress);
            Assert.True(validationResults.Count == 1);
            controller.ModelState.AddModelError("StreetName",validationResults[0].ErrorMessage);
            //Attempt to update address, it won't work due to invalid modelstate
            await controller.AddAddressesById(addressViewModel);
            //Retrieve the address from the database to check whether it was updated
            var memberAddressesViewComponent = new MemberAddressesViewComponent(_context);
            var memberAddressesViewResult = await memberAddressesViewComponent.InvokeAsync(memberResult.Entity.MemberId) as ViewViewComponentResult;
            Assert.NotNull(memberAddressesViewResult);
            var memberAddressesViewModel = memberAddressesViewResult?.ViewData.ModelExplorer.Model as MemberAddressesViewModel;
            AddressTable retrievedAddress = memberAddressesViewModel.MemberAddress;



            //Assert
            Assert.True(controller.ModelState.IsValid == false);
            Assert.Equal(validationResults[0].ErrorMessage, controller.ModelState["StreetName"].Errors[0].ErrorMessage);
            Assert.Equivalent(expectedAddress, retrievedAddress);
        }
    }
}
