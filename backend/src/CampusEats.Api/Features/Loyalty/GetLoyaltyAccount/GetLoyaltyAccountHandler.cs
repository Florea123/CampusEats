using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyAccount;

public class GetLoyaltyAccountHandler(AppDbContext context)
    : IRequestHandler<GetLoyaltyAccountQuery, LoyaltyAccountDto?>
{
    public async Task<LoyaltyAccountDto?> Handle(GetLoyaltyAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await context.LoyaltyAccounts
            .Where(la => la.UserId == request.UserId)
            .Select(la => new LoyaltyAccountDto(la.Id, la.UserId, la.Points, la.UpdatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        return account;
    }
}