using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using CampusEats.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Loyalty;

public class LoyaltyService(AppDbContext context) : ILoyaltyService
{
    public async Task AwardPointsForOrder(Guid userId, Guid orderId, decimal orderTotal)
    {
        var account = await context.LoyaltyAccounts
            .FirstOrDefaultAsync(la => la.UserId == userId);

        if (account is null)
        {
            // Create loyalty account automatically if it doesn't exist
            account = new LoyaltyAccount
            {
                UserId = userId,
                Points = 0,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };
            context.LoyaltyAccounts.Add(account);
            await context.SaveChangesAsync();
        }

        var pointsToAward = (int)Math.Floor(orderTotal / 10);
        if (pointsToAward <= 0) return;

        account.Points += pointsToAward;
        account.UpdatedAtUtc = DateTime.UtcNow;

        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            PointsChange = pointsToAward,
            Type = LoyaltyTransactionType.Earned,
            Description = $"Earned from order #{orderId.ToString()[..8]}",
            RelatedOrderId = orderId,
            CreatedAtUtc = DateTime.UtcNow
        };

        context.LoyaltyTransactions.Add(transaction);
        await context.SaveChangesAsync();
    }
}