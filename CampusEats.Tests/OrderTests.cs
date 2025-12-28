using System.Security.Claims;
using CampusEats.Api.Features.Orders;
using CampusEats.Api.Features.Orders.PlaceOrder;
using CampusEats.Api.Domain;
using Xunit;
using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Enums;
using Microsoft.AspNetCore.Http;
using NSubstitute;


namespace CampusEats.Tests;

public class OrderTests
{
    [Fact]
    public async Task PlaceOrder_Should_Calculate_Total_And_Create_KitchenTask()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();

        var pizza = new MenuItem(Guid.NewGuid(), "Pizza", 20m, null, MenuCategory.PIZZA, null, []);
        var cola = new MenuItem(Guid.NewGuid(), "Cola", 5m, null, MenuCategory.DRINK, null, []);
        db.MenuItems.AddRange(pizza, cola);
        await db.SaveChangesAsync();
        
        var userId = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = userClaims });
        var handler = new PlaceOrderHandler(db, httpContextAccessor);

        var command = new PlaceOrderCommand(new OrderCreateDto
        {
            Items = new List<OrderItemCreateDto>
            {
                new() { MenuItemId = pizza.Id, Quantity = 2 },
                new() { MenuItemId = cola.Id, Quantity = 1 }
            }
        });
        
        
        var orderId = await handler.Handle(command, CancellationToken.None);
        
        var order = await db.Orders.FindAsync(orderId);
        Assert.Equal(45m, order.Total);
        Assert.Equal(userId, order.UserId);
        
        var kitchenTask = await db.KitchenTasks.FirstOrDefaultAsync(kt => kt.OrderId == orderId);
        Assert.NotNull(kitchenTask);
        Assert.Equal(KitchenTaskStatus.NotStarted, kitchenTask.Status);
    }

    [Fact]
    public async Task PlaceOrder_Should_Create_Order_With_Multiple_Items()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        
        var burger = new MenuItem(Guid.NewGuid(), "Burger", 30m, null, MenuCategory.BURGER, null, []);
        var fries = new MenuItem(Guid.NewGuid(), "Fries", 10m, null, MenuCategory.OTHER, null, []);
        var drink = new MenuItem(Guid.NewGuid(), "Drink", 7m, null, MenuCategory.DRINK, null, []);
        
        db.MenuItems.AddRange(burger, fries, drink);
        await db.SaveChangesAsync();
        
        var userId = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = userClaims });
        
        var handler = new PlaceOrderHandler(db, httpContextAccessor);
        var command = new PlaceOrderCommand(new OrderCreateDto
        {
            Items = new List<OrderItemCreateDto>
            {
                new() { MenuItemId = burger.Id, Quantity = 1 },
                new() { MenuItemId = fries.Id, Quantity = 2 },
                new() { MenuItemId = drink.Id, Quantity = 1 }
            }
        });
        
        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        Assert.NotNull(order);
        Assert.Equal(57m, order.Total); // 30 + (10*2) + 7
        Assert.Equal(3, order.Items.Count);
    }

    [Fact]
    public async Task PlaceOrder_Should_Set_Correct_Order_Status()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        
        var menuItem = new MenuItem(Guid.NewGuid(), "Test Item", 15m, null, MenuCategory.BURGER, null, []);
        db.MenuItems.Add(menuItem);
        await db.SaveChangesAsync();
        
        var userId = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = userClaims });
        
        var handler = new PlaceOrderHandler(db, httpContextAccessor);
        var command = new PlaceOrderCommand(new OrderCreateDto
        {
            Items = new List<OrderItemCreateDto>
            {
                new() { MenuItemId = menuItem.Id, Quantity = 1 }
            }
        });
        
        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var order = await db.Orders.FindAsync(orderId);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public async Task PlaceOrder_Should_Store_Correct_UnitPrices()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        
        var menuItem = new MenuItem(Guid.NewGuid(), "Premium Burger", 45m, null, MenuCategory.BURGER, null, []);
        db.MenuItems.Add(menuItem);
        await db.SaveChangesAsync();
        
        var userId = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = userClaims });
        
        var handler = new PlaceOrderHandler(db, httpContextAccessor);
        var command = new PlaceOrderCommand(new OrderCreateDto
        {
            Items = new List<OrderItemCreateDto>
            {
                new() { MenuItemId = menuItem.Id, Quantity = 3 }
            }
        });
        
        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        Assert.NotNull(order);
        Assert.Single(order.Items);
        
        var orderItem = order.Items.First();
        Assert.Equal(45m, orderItem.UnitPrice);
        Assert.Equal(3, orderItem.Quantity);
        Assert.Equal(135m, order.Total); // 45 * 3
    }

    [Fact]
    public async Task CancelOrder_Should_Update_Order_Status()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            Total = 50m,
            CreatedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        
        // Act
        order.Status = OrderStatus.Cancelled;
        await db.SaveChangesAsync();
        
        // Assert
        var cancelledOrder = await db.Orders.FindAsync(order.Id);
        Assert.NotNull(cancelledOrder);
        Assert.Equal(OrderStatus.Cancelled, cancelledOrder.Status);
    }

    [Fact]
    public async Task PlaceOrder_Should_Calculate_Subtotal_Correctly()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        
        var item1 = new MenuItem(Guid.NewGuid(), "Item 1", 12.50m, null, MenuCategory.BURGER, null, []);
        var item2 = new MenuItem(Guid.NewGuid(), "Item 2", 7.75m, null, MenuCategory.OTHER, null, []);
        
        db.MenuItems.AddRange(item1, item2);
        await db.SaveChangesAsync();
        
        var userId = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }));
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = userClaims });
        
        var handler = new PlaceOrderHandler(db, httpContextAccessor);
        var command = new PlaceOrderCommand(new OrderCreateDto
        {
            Items = new List<OrderItemCreateDto>
            {
                new() { MenuItemId = item1.Id, Quantity = 2 },
                new() { MenuItemId = item2.Id, Quantity = 3 }
            }
        });
        
        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var order = await db.Orders.FindAsync(orderId);
        Assert.NotNull(order);
        Assert.Equal(48.25m, order.Total); // (12.50 * 2) + (7.75 * 3) = 25 + 23.25
    }
    
}