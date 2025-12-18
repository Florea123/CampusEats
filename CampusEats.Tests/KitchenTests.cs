using CampusEats.Api.Enums;
using CampusEats.Api.Features.Kitchen.UpdateKitchenTask;
using CampusEats.Api.Domain;
using CampusEats.Api.Infrastructure.Loyalty;

namespace CampusEats.Tests;

public class KitchenTests
{
    private sealed class FakeLoyaltyService : ILoyaltyService
    {
        public Task AwardPointsForOrder(Guid userId, Guid orderId, decimal total) => Task.CompletedTask;
    }

    [Fact]
    public async Task UpdateKitchenTask_Should_Sync_OrderStatus()
    {
        using var db = TestDbHelper.GetInMemoryDbContext();

        var orderId = new Guid("3f3d9c77-8b8d-4e3e-a5b4-2f4d9e2f4c11");
        var taskId = new Guid("9c1a5fbb-2f3a-4c9a-9c0c-1a2b3c4d5e6f");

        db.Orders.Add(new Order { Id = orderId, Status = OrderStatus.Pending });
        db.KitchenTasks.Add(new KitchenTask
        {
            Id = taskId,
            OrderId = orderId,
            Status = KitchenTaskStatus.Preparing,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var loyalty = new FakeLoyaltyService();
        var handler = new UpdateKitchenTaskHandler(db, loyalty);

        var command = new UpdateKitchenTaskCommand(taskId, null, "Preparing", null);
        await handler.Handle(command, CancellationToken.None);

        var order = await db.Orders.FindAsync(orderId);

        Assert.Equal(OrderStatus.Preparing, order.Status);
    }
}