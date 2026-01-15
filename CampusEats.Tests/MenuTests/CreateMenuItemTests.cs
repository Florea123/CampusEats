using System.Net;
using System.Net.Http.Json;
using CampusEats.Api.Enums;
using CampusEats.Api.Features.Menu.CreateMenuItem;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.MenuTests;

public class CreateMenuItemTests
{
    [Fact]
    public async Task Handle_Should_Create_MenuItem_In_Db()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();
        var httpContextAccessor = new HttpContextAccessor();
        var handler = new CreateMenuItemHandler(db, httpContextAccessor);

        var cmd = new CreateMenuItemCommand(
            "Pizza",
            20m,
            "Desc",
            MenuCategory.PIZZA,
            ImageUrl: null,
            Allergens: []
        );

        var id = await handler.Handle(cmd, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);

        var entity = await db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
        Assert.NotNull(entity);
        Assert.Equal("Pizza", entity!.Name);
        Assert.Equal(20m, entity.Price);
        Assert.Equal(MenuCategory.PIZZA, entity.Category);
    }

    [Fact]
    public async Task Handle_Should_Use_Default_ImageUrl_When_None_Provided()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();
        var httpContextAccessor = new HttpContextAccessor();
        var handler = new CreateMenuItemHandler(db, httpContextAccessor);

        var cmd = new CreateMenuItemCommand(
            "Burger",
            15m,
            "Desc",
            MenuCategory.BURGER,
            ImageUrl: null,
            Allergens: []
        );

        var id = await handler.Handle(cmd, CancellationToken.None);

        var entity = await db.MenuItems.FirstAsync(m => m.Id == id);
        Assert.NotNull(entity.ImageUrl);
        Assert.Contains("burger", entity.ImageUrl!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_With_ImageUrl_Should_Keep_Provided_Url()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();
        var httpContextAccessor = new HttpContextAccessor();
        var handler = new CreateMenuItemHandler(db, httpContextAccessor);

        var url = "https://cdn.example.com/custom/pizza.png";

        var cmd = new CreateMenuItemCommand(
            "Pizza",
            20m,
            "Desc",
            MenuCategory.PIZZA,
            ImageUrl: url,
            Allergens: []
        );

        var id = await handler.Handle(cmd, CancellationToken.None);

        var entity = await db.MenuItems.FirstAsync(m => m.Id == id);
        Assert.Equal(url, entity.ImageUrl);
    }
}

public class CreateMenuItemEndpointTests(WebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    private sealed class CreateMenuItemRequest
    {
        public string Name { get; set; } = "Created From Endpoint";
        public decimal Price { get; set; } = 12m;
        public string? Description { get; set; } = "Desc";
        public MenuCategory Category { get; set; } = MenuCategory.PIZZA;
        public string? ImageUrl { get; set; }
        public string[] Allergens { get; set; } = [];
    }

    [Fact]
    public async Task Post_Menu_Should_Return_Created_And_Id()
    {
        var body = new CreateMenuItemRequest();

        var response = await Client.PostAsJsonAsync("/api/menu", body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(payload);
        Assert.True(payload!.TryGetValue("id", out var idObj));
        Assert.True(Guid.TryParse(idObj?.ToString(), out _));
    }

    [Fact]
    public async Task Post_Menu_With_Invalid_Body_Should_Return_BadRequest()
    {
        var body = new CreateMenuItemRequest
        {
            Name = "",  
            Price = 0m
        };

        var response = await Client.PostAsJsonAsync("/api/menu", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}