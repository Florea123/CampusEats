using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using CampusEats.Api.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Loyalty.RedeemPoints;

public class RedeemPointsHandler(AppDbContext context) : IRequestHandler<RedeemPointsCommand, RedeemPointsResult>
{
    public async Task<RedeemPointsResult> Handle(RedeemPointsCommand request, CancellationToken cancellationToken)
    {
        var account = await context.LoyaltyAccounts
            .FirstOrDefaultAsync(la => la.UserId == request.UserId, cancellationToken);

        if (account is null)
            return new RedeemPointsResult(false, "Loyalty account not found");

        if (account.Points < request.Points)
            return new RedeemPointsResult(false, "Insufficient points");

        account.Points -= request.Points;
        account.UpdatedAtUtc = DateTime.UtcNow;

        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            PointsChange = -request.Points,
            Type = LoyaltyTransactionType.Redeemed,
            Description = request.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        context.LoyaltyTransactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);

        return new RedeemPointsResult(true, "Points redeemed successfully", account.Points);
    }
}