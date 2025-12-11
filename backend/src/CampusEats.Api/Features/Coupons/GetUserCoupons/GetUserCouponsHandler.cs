using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Coupons.GetUserCoupons;

public class GetUserCouponsHandler(AppDbContext context) 
    : IRequestHandler<GetUserCouponsQuery, List<UserCouponDto>>
{
    public async Task<List<UserCouponDto>> Handle(GetUserCouponsQuery request, CancellationToken cancellationToken)
    {
        var userCoupons = await context.UserCoupons
            .Include(uc => uc.Coupon)
            .ThenInclude(c => c.SpecificMenuItem)
            .Where(uc => uc.UserId == request.UserId && 
                         !uc.IsUsed && 
                         (uc.ExpiresAtUtc == null || uc.ExpiresAtUtc > DateTime.UtcNow))
            .OrderByDescending(uc => uc.AcquiredAtUtc)
            .ToListAsync(cancellationToken);

        return userCoupons.Select(uc => new UserCouponDto(
            uc.Id,
            uc.CouponId,
            uc.Coupon.Name,
            uc.Coupon.Description,
            uc.Coupon.Type,
            uc.Coupon.DiscountValue,
            uc.Coupon.MinimumOrderAmount,
            uc.Coupon.SpecificMenuItemId,
            uc.Coupon.SpecificMenuItem?.Name,
            uc.AcquiredAtUtc,
            uc.ExpiresAtUtc,
            uc.IsUsed,
            uc.UsedAtUtc
        )).ToList();
    }
}
