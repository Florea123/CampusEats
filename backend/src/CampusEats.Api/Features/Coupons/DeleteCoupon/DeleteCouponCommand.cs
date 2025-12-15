using MediatR;

namespace CampusEats.Api.Features.Coupons.DeleteCoupon;

public record DeleteCouponCommand(Guid CouponId) : IRequest<DeleteCouponResult>;
