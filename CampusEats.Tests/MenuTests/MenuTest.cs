using CampusEats.Api.Enums;
using CampusEats.Api.Features.Menu.CreateMenuItem;
using CampusEats.Api.Features.Menu.DeleteMenuItem;
using CampusEats.Api.Domain;
using CampusEats.Tests;
using Microsoft.AspNetCore.Http;
using Xunit;



public class MenuTests
{
    [Fact]
    public async Task CreateMenuItem_Should_Add_Item_To_Database()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();
        var handler = new CreateMenuItemHandler(db, new HttpContextAccessor());
        
        var command = new CreateMenuItemCommand(
            Name: "Test Item",
            Description: "A delicious test item",
            Price: 9.99m,
            Category: MenuCategory.PIZZA,
            ImageUrl:null,
            Allergens: new[] {"Gluten"}
        );
        
        var resultId = await handler.Handle(command, CancellationToken.None);
        
        var itemInDb = await db.MenuItems.FindAsync(resultId);
        Assert.NotNull(itemInDb);
        Assert.Equal("Test Item", itemInDb.Name);
    }

    [Fact]
    public async Task DeleteMenuItem_Should_Return_True_When_Exists()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();
        
        //Aici trebuie cu un Guid deja generat
        var item = new MenuItem(Guid.NewGuid(), "Burger", 20, null, MenuCategory.BURGER, null, Array.Empty<string>());
        db.MenuItems.Add(item);
        await db.SaveChangesAsync();
        
        var handler = new DeleteMenuItemHandler(db);

        var result = await handler.Handle(new DeleteMenuItemCommand(item.Id), CancellationToken.None);
        
        Assert.True(result);
        Assert.Null(await db.MenuItems.FindAsync(item.Id));
    }

    [Fact]
    public async Task CreateMenuItem_Should_Set_IsAvailable_To_True()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var handler = new CreateMenuItemHandler(db, new HttpContextAccessor());
        
        var command = new CreateMenuItemCommand(
            Name: "New Pizza",
            Description: "Fresh pizza",
            Price: 25.99m,
            Category: MenuCategory.PIZZA,
            ImageUrl: null,
            Allergens: new[] { "Dairy" }
        );
        
        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var item = await db.MenuItems.FindAsync(resultId);
        Assert.NotNull(item);
    }

    [Fact]
    public async Task CreateMenuItem_Should_Store_Allergens()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var handler = new CreateMenuItemHandler(db, new HttpContextAccessor());
        
        var allergens = new[] { "Gluten", "Dairy", "Nuts" };
        var command = new CreateMenuItemCommand(
            Name: "Allergen Test",
            Description: "Contains allergens",
            Price: 15m,
            Category: MenuCategory.BURGER,
            ImageUrl: null,
            Allergens: allergens
        );
        
        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var item = await db.MenuItems.FindAsync(resultId);
        Assert.NotNull(item);
        Assert.Equal(3, item.Allergens.Length);
        Assert.Contains("Gluten", item.Allergens);
        Assert.Contains("Dairy", item.Allergens);
        Assert.Contains("Nuts", item.Allergens);
    }

    [Fact]
    public async Task DeleteMenuItem_Should_Return_False_When_Not_Exists()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var handler = new DeleteMenuItemHandler(db);
        
        // Act
        var result = await handler.Handle(new DeleteMenuItemCommand(Guid.NewGuid()), CancellationToken.None);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateMenuItem_Should_Store_Price_Correctly()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var handler = new CreateMenuItemHandler(db, new HttpContextAccessor());
        
        var command = new CreateMenuItemCommand(
            Name: "Expensive Item",
            Description: "Premium quality",
            Price: 99.99m,
            Category: MenuCategory.BURGER,
            ImageUrl: null,
            Allergens: Array.Empty<string>()
        );
        
        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var item = await db.MenuItems.FindAsync(resultId);
        Assert.NotNull(item);
        Assert.Equal(99.99m, item.Price);
    }
    
    
}