using MediatR;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyAccount;

public record GetLoyaltyAccountQuery(Guid UserId) : IRequest<LoyaltyAccountDto?>;