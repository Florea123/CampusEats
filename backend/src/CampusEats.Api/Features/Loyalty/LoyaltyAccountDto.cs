namespace CampusEats.Api.Features.Loyalty;

public record LoyaltyAccountDto(
    Guid Id,
    Guid UserId,
    int Points,
    DateTime UpdatedAtUtc
);