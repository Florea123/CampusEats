namespace CampusEats.Api.Features.Coupons.CreateCoupon;

public record CreateCouponResult(bool Success, string Message, Guid? CouponId = null);
