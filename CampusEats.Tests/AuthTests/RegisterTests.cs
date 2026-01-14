using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using CampusEats.Api.Enums;
using CampusEats.Api.Features.Auth.Register;
using CampusEats.Api.Infrastructure.Auth;
using CampusEats.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CampusEats.Tests.AuthTests;

public class RegisterUserTests
{
    // Simple stubs for dependencies to avoid Moq dependency if not installed
    class FakePasswordService : IPasswordService
    {
        public string Hash(User user, string password) => "hashed_" + password;
        public bool Verify(User user, string hashed, string password) => true;
    }

    class FakeJwtService : IJwtTokenService
    {
        public string GenerateAccessToken(User user) => "fake_access_token";
        public (string token, string hash, DateTime expiresAtUtc) GenerateRefreshToken() => ("rt", "hash", DateTime.UtcNow.AddDays(7));
        public string Hash(string value) => "hashed_" + value;
    }

    [Fact]
    public async Task Handle_Should_Register_Student_Successfully()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();
        var passwords = new FakePasswordService();
        var jwt = new FakeJwtService();
        var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() }; // Empty context is fine for Student

        var handler = new RegisterUserHandler(db, passwords, jwt, http);
        var cmd = new RegisterUserCommand("Student", "s@t.com", "Pass123!", UserRole.STUDENT);

        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert Result
        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<AuthResultDto>>(result);
        Assert.Equal("fake_access_token", okResult.Value!.AccessToken);

        // Assert DB side effects
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "s@t.com");
        Assert.NotNull(user);
        Assert.Equal(UserRole.STUDENT, user!.Role);
        Assert.Equal("hashed_Pass123!", user.PasswordHash);
        
        // Assert Loyalty
        var loyalty = await db.LoyaltyAccounts.FirstOrDefaultAsync(l => l.UserId == user.Id);
        Assert.NotNull(loyalty);
        Assert.Equal(0, loyalty!.Points);

        // Assert Refresh Token in DB
        var rt = await db.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == user.Id);
        Assert.NotNull(rt);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Anonymous_Tries_To_Register_Manager()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();
        var passwords = new FakePasswordService();
        var jwt = new FakeJwtService();
        
        // Unauthenticated context
        var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() }; 

        var handler = new RegisterUserHandler(db, passwords, jwt, http);
        var cmd = new RegisterUserCommand("Boss", "m@t.com", "Pass123!", UserRole.MANAGER);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult>(result);
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Student_Tries_To_Register_Manager()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();
        var passwords = new FakePasswordService();
        var jwt = new FakeJwtService();

        // Authenticated as Student
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Role, "Student") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var http = new HttpContextAccessor { HttpContext = context };

        var handler = new RegisterUserHandler(db, passwords, jwt, http);
        var cmd = new RegisterUserCommand("Boss", "m@t.com", "Pass123!", UserRole.MANAGER);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.ForbidHttpResult>(result);
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_Manager_Registers_Worker()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();
        var passwords = new FakePasswordService();
        var jwt = new FakeJwtService();

        // Authenticated as Manager
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Role, "Manager") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var http = new HttpContextAccessor { HttpContext = context };

        var handler = new RegisterUserHandler(db, passwords, jwt, http);
        var cmd = new RegisterUserCommand("Worker", "w@t.com", "Pass123!", UserRole.WORKER);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<AuthResultDto>>(result);
        Assert.Single(db.Users);
        Assert.Equal(UserRole.WORKER, db.Users.First().Role);
    }
}

public class RegisterUserEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public class TestUserContext
    {
        public string Role { get; set; } = "Student";
    }

    public class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TestUserContext userContext)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, userContext.Role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public RegisterUserEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));
                services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("TestDb_Register"));
                
                services.AddScoped(sp => new TestUserContext());
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        });
    }

    private HttpClient CreateClientWithRole(string role)
    {
        var clientFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<TestUserContext>();
                services.AddScoped(sp => new TestUserContext { Role = role });
            });
        });

        var client = clientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        return client;
    }

    [Fact]
    public async Task Register_Student_Should_Return_Ok()
    {
        var client = _factory.CreateClient(); // Anonymous
        var command = new RegisterUserCommand("New Student", "student@test.com", "P@ssword123", UserRole.STUDENT);

        var response = await client.PostAsJsonAsync("/auth/register", command);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        Assert.NotNull(result?.AccessToken);
    }

    [Fact]
    public async Task Register_Worker_With_Manager_Auth_Should_Return_Ok()
    {
        var client = CreateClientWithRole("MANAGER");
        var command = new RegisterUserCommand("New Worker", "worker@test.com", "P@ssword123", UserRole.WORKER);

        var response = await client.PostAsJsonAsync("/auth/register", command);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_Worker_With_Student_Auth_Should_Return_Forbidden()
    {
        var client = CreateClientWithRole("STUDENT");
        var command = new RegisterUserCommand("Hacker", "hacked@test.com", "P@ssword123", UserRole.MANAGER);

        var response = await client.PostAsJsonAsync("/auth/register", command);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Register_With_Weak_Password_Should_Return_BadRequest_Validation()
    {
        var client = _factory.CreateClient();
        var command = new RegisterUserCommand("User", "weak@test.com", "weak", UserRole.STUDENT);

        var response = await client.PostAsJsonAsync("/auth/register", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var details = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.True(details!.ContainsKey("errors"));
    }

    [Fact]
    public async Task Register_Duplicate_Email_Should_Return_BadRequest()
    {
        var client = _factory.CreateClient();
        var command = new RegisterUserCommand("User One", "dup@test.com", "P@ssword123", UserRole.STUDENT);
        
        // First register
        await client.PostAsJsonAsync("/auth/register", command);

        // Second register
        var response = await client.PostAsJsonAsync("/auth/register", command);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Email already registered", content);
    }
}
