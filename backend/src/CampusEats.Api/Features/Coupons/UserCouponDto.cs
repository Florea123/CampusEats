using CampusEats.Api.Enums;

namespace CampusEats.Api.Features.Coupons;

public record UserCouponDto(
    Guid Id,
    Guid CouponId,
    string CouponName,
    string CouponDescription,
    CouponType CouponType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    Guid? SpecificMenuItemId,
    string? SpecificMenuItemName,
    DateTime AcquiredAtUtc,
    DateTime? ExpiresAtUtc,
    bool IsUsed,
    DateTime? UsedAtUtc
);
