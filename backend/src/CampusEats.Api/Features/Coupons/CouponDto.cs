using CampusEats.Api.Enums;

namespace CampusEats.Api.Features.Coupons;

public record CouponDto(
    Guid Id,
    string Name,
    string Description,
    CouponType Type,
    decimal DiscountValue,
    int PointsCost,
    Guid? SpecificMenuItemId,
    string? SpecificMenuItemName,
    decimal? MinimumOrderAmount,
    bool IsActive,
    DateTime? ExpiresAtUtc
);
