using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyTransactions;

public class GetLoyaltyTransactionsHandler(AppDbContext context)
    : IRequestHandler<GetLoyaltyTransactionsQuery, List<LoyaltyTransactionDto>>
{
    public async Task<List<LoyaltyTransactionDto>> Handle(GetLoyaltyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await context.LoyaltyTransactions
            .Include(lt => lt.LoyaltyAccount)
            .Where(lt => lt.LoyaltyAccount.UserId == request.UserId)
            .OrderByDescending(lt => lt.CreatedAtUtc)
            .Select(lt => new LoyaltyTransactionDto(
                lt.Id,
                lt.PointsChange,
                lt.Type,
                lt.Description,
                lt.RelatedOrderId,
                lt.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return transactions;
    }
}