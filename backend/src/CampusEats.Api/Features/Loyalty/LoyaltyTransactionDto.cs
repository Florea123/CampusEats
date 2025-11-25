using CampusEats.Api.Enums;

namespace CampusEats.Api.Features.Loyalty;

public record LoyaltyTransactionDto(
    Guid Id,
    int PointsChange,
    LoyaltyTransactionType Type,
    string Description,
    Guid? RelatedOrderId,
    DateTime CreatedAtUtc
);