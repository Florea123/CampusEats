using MediatR;

namespace CampusEats.Api.Features.Coupons.GetAvailableCoupons;

public record GetAvailableCouponsQuery(Guid? UserId) : IRequest<List<CouponDto>>;
