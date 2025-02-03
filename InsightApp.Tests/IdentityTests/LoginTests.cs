using AspNetCore.ReCaptcha;
using InsightApp.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Identity.UI.Services;
using InsightApp.Entities;
using InsightApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Moq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Components;

namespace InsightApp.Tests.IdentityTests
{
    public class LoginTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly InsightUpdateCvgs2Context _context;
        private readonly DbContextOptions<InsightUpdateCvgs2Context> _contextOptions;
        private readonly IServiceProvider _serviceProvider;

        public LoginTests()
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

            services.AddDataProtection();
            services.AddIdentity<Account, AccountRole>(options =>
            {
                options.Tokens.ProviderMap.Add(TokenOptions.DefaultEmailProvider, new TokenProviderDescriptor(typeof(DataProtectorTokenProvider<Account>)));
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.ProviderMap.Add(TokenOptions.DefaultAuthenticatorProvider, new TokenProviderDescriptor(typeof(DataProtectorTokenProvider<Account>)));
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            }).AddEntityFrameworkStores<InsightUpdateCvgs2Context>()
            .AddDefaultTokenProviders();

            var mockedIHttpContextAccessor = new Mock<IHttpContextAccessor>();
            HttpContext httpContext = new DefaultHttpContext();
            mockedIHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            services.AddSingleton<IHttpContextAccessor>(mockedIHttpContextAccessor.Object);

            services.AddSingleton<IUserClaimsPrincipalFactory<Account>, UserClaimsPrincipalFactory<Account, AccountRole>>();

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
        public async Task Login_CorrectCredentialsUnlockedAccount_Success()
        {
            //Arrange
            var context = CreateContext();
            OptionsWrapper<IdentityOptions> optionsWrapper = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
            ProgramConstants.GetIdentityOptions()(optionsWrapper.Value);
            List<IPasswordValidator<Account>> passwordValidators = new List<IPasswordValidator<Account>> { new PasswordValidator<Account>() };
            List<IUserValidator<Account>> userValidators = new List<IUserValidator<Account>> { new UserValidator<Account>() };
            UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid> userStore = new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(context);
            UserManager<Account> userManager = new UserManager<Account>(
                userStore,
                optionsWrapper,
                new PasswordHasher<Account>(),
                userValidators,
                passwordValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                _serviceProvider,
                new Mock<ILogger<UserManager<Account>>>().Object
                );
            SignInManager<Account> signInManager = new SignInManager<Account>(
                userManager,
                new Mock<IHttpContextAccessor>().Object,
                _serviceProvider.GetRequiredService<IUserClaimsPrincipalFactory<Account>>(),
                optionsWrapper,
                new Mock<ILogger<SignInManager<Account>>>().Object,
                null,
                new Mock<IUserConfirmation<Account>>().Object
                );

            Account expectedNewUser = new Account()
            {
                UserName = "Gamer123",
                Email = "gamer123test@testaccount.com",
                EmailConfirmed = true,
                LockoutEnabled = true
            };
            string validPassword = "AabbCCdd99&22&33";

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProvider;
            signInManager.Context = httpContext;

            var result = await userManager.CreateAsync(expectedNewUser, validPassword);
            Assert.True(result.Succeeded);
            
            //Act
            var loginResult = await signInManager.PasswordSignInAsync(expectedNewUser.UserName, validPassword, false, true);
            
            //Assert
            Assert.True(loginResult.Succeeded);
        }
        
        [Fact]
        public async Task Login_IncorrectPasswordThreeTimes_AccountLockedOut()
        {
            //Arrange
            var context = CreateContext();
            OptionsWrapper<IdentityOptions> optionsWrapper = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
            ProgramConstants.GetIdentityOptions()(optionsWrapper.Value);
            List<IPasswordValidator<Account>> passwordValidators = new List<IPasswordValidator<Account>> { new PasswordValidator<Account>() };
            List<IUserValidator<Account>> userValidators = new List<IUserValidator<Account>> { new UserValidator<Account>() };
            UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid> userStore = new UserStore<Account, AccountRole, InsightUpdateCvgs2Context, Guid>(context);
            UserManager<Account> userManager = new UserManager<Account>(
                userStore,
                optionsWrapper,
                new PasswordHasher<Account>(),
                userValidators,
                passwordValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                _serviceProvider,
                new Mock<ILogger<UserManager<Account>>>().Object
                );
            SignInManager<Account> signInManager = new SignInManager<Account>(
                userManager,
                new Mock<IHttpContextAccessor>().Object,
                _serviceProvider.GetRequiredService<IUserClaimsPrincipalFactory<Account>>(),
                optionsWrapper,
                new Mock<ILogger<SignInManager<Account>>>().Object,
                null,
                new Mock<IUserConfirmation<Account>>().Object
                );

            Account expectedNewUser = new Account()
            {
                UserName = "Gamer123",
                Email = "gamer123test@testaccount.com",
                EmailConfirmed = true,
                LockoutEnabled = true
            };
            string validPassword = "AabbCCdd99&22&33";

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProvider;
            signInManager.Context = httpContext;

            var result = await userManager.CreateAsync(expectedNewUser, validPassword);
            Assert.True( result.Succeeded );

            //Act
            for(int i = 0; i < 3; i++)
            {
                var loginResult = await signInManager.PasswordSignInAsync(expectedNewUser.UserName, "incorrect", false, true);
            }
            Account actualAccount = await userManager.FindByEmailAsync(expectedNewUser.Email);

            //Assert
            Assert.True(await userManager.IsLockedOutAsync(actualAccount));
        }
    }
}
