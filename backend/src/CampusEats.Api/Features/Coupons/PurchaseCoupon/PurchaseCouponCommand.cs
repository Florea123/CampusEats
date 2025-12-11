using MediatR;

namespace CampusEats.Api.Features.Coupons.PurchaseCoupon;

public record PurchaseCouponCommand(Guid UserId, Guid CouponId) : IRequest<PurchaseCouponResult>;
