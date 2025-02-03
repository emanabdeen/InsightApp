using InsightApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightApp.Tests.IdentityTests
{
    public class RegisterTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly InsightUpdateCvgs2Context _context;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;
        private readonly IServiceProvider _serviceProvider;

        public RegisterTests()
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
        public async Task RegisterNewUser_ValidInputModel_AccountCreatedSuccessfully()
        {
            //Arrange
            var context = CreateContext();
            OptionsWrapper<IdentityOptions> optionsWrapper = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
            ProgramConstants.GetIdentityOptions()(optionsWrapper.Value);
            List<IPasswordValidator<Account>> passwordValidators = new List<IPasswordValidator<Account>> { new PasswordValidator<Account>() };
            List<IUserValidator<Account>> userValidators = new List<IUserValidator<Account>> { new UserValidator<Account>() };
            var mockedLogger = new Mock<ILogger<UserManager<Account>>>();
            var userManager = new UserManager<Account>(
                new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(context),
                optionsWrapper,
                new PasswordHasher<Account>(),
                userValidators,
                passwordValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                _serviceProvider,
                mockedLogger.Object
                );
            Account expectedNewUser = new Account()
            {
                UserName = "Gamer123",
                Email = "gamer123test@testaccount.com",
            };
            string validPassword = "AabbCCdd99&22&33";

            //Act
            var result = await userManager.CreateAsync(expectedNewUser, validPassword);
            Account actualUserResult = await userManager.FindByIdAsync(expectedNewUser.Id.ToString());

            //Assert
            Assert.True(result.Succeeded);
            Assert.True(result.Errors.Count() == 0);
            Assert.Equal(expectedNewUser.Email, actualUserResult.Email);
            Assert.Equal(expectedNewUser.UserName, actualUserResult.UserName);
        }

        [Fact]
        public async Task RegisterNewUser_InvalidPassword_FailWithErrorsReturned()
        {
            //Arrange
            var context = CreateContext();
            OptionsWrapper<IdentityOptions> optionsWrapper = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
            ProgramConstants.GetIdentityOptions()(optionsWrapper.Value);
            List<IPasswordValidator<Account>> passwordValidators = new List<IPasswordValidator<Account>> { new PasswordValidator<Account>() };
            List<IUserValidator<Account>> userValidators = new List<IUserValidator<Account>> { new  UserValidator<Account>() };
            var mockedLogger = new Mock<ILogger<UserManager<Account>>>();
            var userManager = new UserManager<Account>(
                new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(context),
                optionsWrapper,
                new PasswordHasher<Account>(),
                userValidators,
                passwordValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                _serviceProvider,
                mockedLogger.Object
                );
            Account expectedNewUser = new Account()
            {
                UserName = "Gamer123",
                Email = "gamer123test@testaccount.com",
            };
            string validPassword = "abc";

            //Act
            var result = await userManager.CreateAsync(expectedNewUser, validPassword);

            //Assert
            Assert.False(result.Succeeded);
            Assert.True(result.Errors.Count() == 4);
            Assert.Contains(result.Errors, error => error.Code == "PasswordTooShort");
            Assert.Contains(result.Errors, error => error.Code == "PasswordRequiresNonAlphanumeric");
            Assert.Contains(result.Errors, error => error.Code == "PasswordRequiresDigit");
            Assert.Contains(result.Errors, error => error.Code == "PasswordRequiresUpper");
        }

        [Fact]
        public async Task RegisterNewUser_AlreadyUsedEmail_FailWithErrorsReturned()
        {
            //Arrange
            var context = CreateContext();
            OptionsWrapper<IdentityOptions> optionsWrapper = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
            ProgramConstants.GetIdentityOptions()(optionsWrapper.Value);
            List<IPasswordValidator<Account>> passwordValidators = new List<IPasswordValidator<Account>> { new PasswordValidator<Account>() };
            List<IUserValidator<Account>> userValidators = new List<IUserValidator<Account>> { new UserValidator<Account>() };
            var mockedLogger = new Mock<ILogger<UserManager<Account>>>();
            var userManager = new UserManager<Account>(
                new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(context),
                optionsWrapper,
                new PasswordHasher<Account>(),
                userValidators,
                passwordValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                _serviceProvider,
                mockedLogger.Object
                );
            Account firstNewUser = new Account()
            {
                UserName = "Gamer123",
                Email = "gamer123test@testaccount.com",
            };
            string validPassword = "AabbCCdd99&22&33";
            Account secondNewUser = new Account()
            {
                UserName = "NintendoFan404",
                Email = "gamer123test@testaccount.com"
            };


            //Act
            var result = await userManager.CreateAsync(firstNewUser, validPassword);
            var failResult = await userManager.CreateAsync(secondNewUser, validPassword);

            //Assert
            Assert.True(result.Succeeded);
            Assert.True(result.Errors.Count() == 0);
            Assert.False(failResult.Succeeded);
            Assert.True(failResult.Errors.Count() == 1);
            Assert.Contains(failResult.Errors, error => error.Code == "DuplicateEmail");
        }
    }
}
