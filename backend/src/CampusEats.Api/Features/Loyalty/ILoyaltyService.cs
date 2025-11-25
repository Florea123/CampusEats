namespace CampusEats.Api.Infrastructure.Loyalty;

public interface ILoyaltyService
{
    Task AwardPointsForOrder(Guid userId, Guid orderId, decimal orderTotal);
}