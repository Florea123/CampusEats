using CampusEats.Api.Enums;
using MediatR;

namespace CampusEats.Api.Features.Coupons.CreateCoupon;

public record CreateCouponCommand(
    string Name,
    string Description,
    CouponType Type,
    decimal DiscountValue,
    int PointsCost,
    Guid? SpecificMenuItemId,
    decimal? MinimumOrderAmount,
    DateTime? ExpiresAtUtc
) : IRequest<CreateCouponResult>;
