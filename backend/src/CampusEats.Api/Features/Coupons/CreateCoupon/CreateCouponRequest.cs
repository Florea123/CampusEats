using CampusEats.Api.Enums;

namespace CampusEats.Api.Features.Coupons.CreateCoupon;

public record CreateCouponRequest(
    string Name,
    string Description,
    CouponType Type,
    decimal DiscountValue,
    int PointsCost,
    Guid? SpecificMenuItemId,
    decimal? MinimumOrderAmount,
    DateTime? ExpiresAtUtc
);
