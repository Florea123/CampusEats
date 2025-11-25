using MediatR;

namespace CampusEats.Api.Features.Loyalty.RedeemPoints;

public record RedeemPointsCommand(Guid UserId, int Points, string Description) : IRequest<RedeemPointsResult>;