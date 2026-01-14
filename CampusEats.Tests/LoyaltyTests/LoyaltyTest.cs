using Xunit;
using CampusEats.Api.Enums;
using CampusEats.Api.Features.Loyalty.RedeemPoints;
using CampusEats.Api.Infrastructure.Loyalty;
using CampusEats.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests;

public class LoyaltyTest
{
    [Fact]
    public async Task AwardPointsForOrder_Should_Create_LoyaltyAccount_And_Add_Points()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var loyaltyService = new LoyaltyService(db);
        
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        decimal orderTotal = 100m; // Should award 10 points (100 / 10)
        
        // Act
        await loyaltyService.AwardPointsForOrder(userId, orderId, orderTotal);
        
        // Assert
        var account = await db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId);
        Assert.NotNull(account);
        Assert.Equal(10, account.Points);
        
        var transaction = await db.LoyaltyTransactions.FirstOrDefaultAsync(lt => lt.LoyaltyAccountId == account.Id);
        Assert.NotNull(transaction);
        Assert.Equal(10, transaction.PointsChange);
        Assert.Equal(LoyaltyTransactionType.Earned, transaction.Type);
        Assert.Equal(orderId, transaction.RelatedOrderId);
    }

    [Fact]
    public async Task AwardPointsForOrder_Should_Add_Points_To_Existing_Account()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var loyaltyService = new LoyaltyService(db);
        
        var userId = Guid.NewGuid();
        var existingAccount = new LoyaltyAccount
        {
            UserId = userId,
            Points = 50,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.LoyaltyAccounts.Add(existingAccount);
        await db.SaveChangesAsync();
        
        var orderId = Guid.NewGuid();
        decimal orderTotal = 75m; // Should award 7 points
        
        // Act
        await loyaltyService.AwardPointsForOrder(userId, orderId, orderTotal);
        
        // Assert
        var account = await db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId);
        Assert.NotNull(account);
        Assert.Equal(57, account.Points); // 50 + 7
    }

    [Fact]
    public async Task AwardPointsForOrder_Should_Not_Award_Points_For_Small_Orders()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var loyaltyService = new LoyaltyService(db);
        
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        decimal orderTotal = 5m; // Less than 10, should award 0 points
        
        // Act
        await loyaltyService.AwardPointsForOrder(userId, orderId, orderTotal);
        
        // Assert
        var account = await db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId);
        Assert.NotNull(account); // Account created
        Assert.Equal(0, account.Points); // But no points awarded
        
        var transactions = await db.LoyaltyTransactions.ToListAsync();
        Assert.Empty(transactions); // No transaction created
    }

    [Fact]
    public async Task AwardPointsForOrder_Should_Floor_Points_Calculation()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var loyaltyService = new LoyaltyService(db);
        
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        decimal orderTotal = 99m; // Should award 9 points (floor of 9.9)
        
        // Act
        await loyaltyService.AwardPointsForOrder(userId, orderId, orderTotal);
        
        // Assert
        var account = await db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId);
        Assert.NotNull(account);
        Assert.Equal(9, account.Points);
    }

    [Fact]
    public async Task RedeemPoints_Should_Deduct_Points_And_Create_Transaction()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var handler = new RedeemPointsHandler(db);
        
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            UserId = userId,
            Points = 100,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.LoyaltyAccounts.Add(account);
        await db.SaveChangesAsync();
        
        var command = new RedeemPointsCommand(
            UserId: userId,
            Points: 50,
            Description: "Redeemed for discount"
        );
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal(50, result.RemainingPoints);
        
        var updatedAccount = await db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId);
        Assert.NotNull(updatedAccount);
        Assert.Equal(50, updatedAccount.Points);
        
        var transaction = await db.LoyaltyTransactions.FirstOrDefaultAsync(lt => lt.LoyaltyAccountId == account.Id);
        Assert.NotNull(transaction);
        Assert.Equal(-50, transaction.PointsChange);
        Assert.Equal(LoyaltyTransactionType.Redeemed, transaction.Type);
        Assert.Equal("Redeemed for discount", transaction.Description);
    }

    [Fact]
    public async Task RedeemPoints_Should_Fail_With_Insufficient_Points()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var handler = new RedeemPointsHandler(db);
        
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            UserId = userId,
            Points = 30,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.LoyaltyAccounts.Add(account);
        await db.SaveChangesAsync();
        
        var command = new RedeemPointsCommand(
            UserId: userId,
            Points: 50,
            Description: "Trying to redeem too many points"
        );
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.False(result.Success);
        Assert.Equal("Insufficient points", result.Message);
        
        var updatedAccount = await db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId);
        Assert.NotNull(updatedAccount);
        Assert.Equal(30, updatedAccount.Points); // Points unchanged
    }

    [Fact]
    public async Task RedeemPoints_Should_Fail_When_Account_Not_Found()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var handler = new RedeemPointsHandler(db);
        
        var userId = Guid.NewGuid();
        var command = new RedeemPointsCommand(
            UserId: userId,
            Points: 50,
            Description: "Trying to redeem without account"
        );
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.False(result.Success);
        Assert.Equal("Loyalty account not found", result.Message);
    }

    [Fact]
    public async Task AwardPointsForOrder_Should_Create_Multiple_Transactions()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var loyaltyService = new LoyaltyService(db);
        
        var userId = Guid.NewGuid();
        var order1Id = Guid.NewGuid();
        var order2Id = Guid.NewGuid();
        
        // Act
        await loyaltyService.AwardPointsForOrder(userId, order1Id, 50m); // 5 points
        await loyaltyService.AwardPointsForOrder(userId, order2Id, 30m); // 3 points
        
        // Assert
        var account = await db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId);
        Assert.NotNull(account);
        Assert.Equal(8, account.Points); // 5 + 3
        
        var transactions = await db.LoyaltyTransactions
            .Where(lt => lt.LoyaltyAccountId == account.Id)
            .OrderBy(lt => lt.CreatedAtUtc)
            .ToListAsync();
        Assert.Equal(2, transactions.Count);
        Assert.Equal(5, transactions[0].PointsChange);
        Assert.Equal(3, transactions[1].PointsChange);
    }

    [Fact]
    public async Task RedeemPoints_Should_Allow_Redeeming_All_Points()
    {
        // Arrange
        using var db = TestDbHelper.GetInMemoryDbContext();
        var handler = new RedeemPointsHandler(db);
        
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            UserId = userId,
            Points = 75,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.LoyaltyAccounts.Add(account);
        await db.SaveChangesAsync();
        
        var command = new RedeemPointsCommand(
            UserId: userId,
            Points: 75,
            Description: "Redeem all points"
        );
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.RemainingPoints);
        
        var updatedAccount = await db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId);
        Assert.NotNull(updatedAccount);
        Assert.Equal(0, updatedAccount.Points);
    }
}
