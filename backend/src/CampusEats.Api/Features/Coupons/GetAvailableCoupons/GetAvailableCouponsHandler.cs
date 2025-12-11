using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Coupons.GetAvailableCoupons;

public class GetAvailableCouponsHandler(AppDbContext context) 
    : IRequestHandler<GetAvailableCouponsQuery, List<CouponDto>>
{
    public async Task<List<CouponDto>> Handle(GetAvailableCouponsQuery request, CancellationToken cancellationToken)
    {
        // Get all active coupons
        var coupons = await context.Coupons
            .Include(c => c.SpecificMenuItem)
            .Where(c => c.IsActive && (c.ExpiresAtUtc == null || c.ExpiresAtUtc > DateTime.UtcNow))
            .OrderBy(c => c.PointsCost)
            .ToListAsync(cancellationToken);

        // Get coupon IDs that the user has already purchased
        var userCouponIds = new HashSet<Guid>();
        if (request.UserId.HasValue)
        {
            userCouponIds = (await context.UserCoupons
                .Where(uc => uc.UserId == request.UserId.Value)
                .Select(uc => uc.CouponId)
                .ToListAsync(cancellationToken))
                .ToHashSet();
        }

        // Filter out coupons the user already owns
        var availableCoupons = coupons.Where(c => !userCouponIds.Contains(c.Id)).ToList();

        return availableCoupons.Select(c => new CouponDto(
            c.Id,
            c.Name,
            c.Description,
            c.Type,
            c.DiscountValue,
            c.PointsCost,
            c.SpecificMenuItemId,
            c.SpecificMenuItem?.Name,
            c.MinimumOrderAmount,
            c.IsActive,
            c.ExpiresAtUtc
        )).ToList();
    }
}
