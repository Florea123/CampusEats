using MediatR;

namespace CampusEats.Api.Features.Coupons.GetUserCoupons;

public record GetUserCouponsQuery(Guid UserId) : IRequest<List<UserCouponDto>>;
