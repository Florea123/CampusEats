using MediatR;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyTransactions;

public record GetLoyaltyTransactionsQuery(Guid UserId) : IRequest<List<LoyaltyTransactionDto>>;