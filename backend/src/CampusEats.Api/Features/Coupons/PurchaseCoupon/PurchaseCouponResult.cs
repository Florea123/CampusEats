namespace CampusEats.Api.Features.Coupons.PurchaseCoupon;

public record PurchaseCouponResult(
    bool Success,
    string Message,
    Guid? UserCouponId = null,
    int? RemainingPoints = null
);
