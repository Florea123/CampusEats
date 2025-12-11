using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using CampusEats.Api.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Coupons.PurchaseCoupon;

public class PurchaseCouponHandler(AppDbContext context) 
    : IRequestHandler<PurchaseCouponCommand, PurchaseCouponResult>
{
    public async Task<PurchaseCouponResult> Handle(PurchaseCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await context.Coupons
            .FirstOrDefaultAsync(c => c.Id == request.CouponId, cancellationToken);

        if (coupon is null)
            return new PurchaseCouponResult(false, "Coupon not found");

        if (!coupon.IsActive)
            return new PurchaseCouponResult(false, "Coupon is not available");

        if (coupon.ExpiresAtUtc.HasValue && coupon.ExpiresAtUtc.Value <= DateTime.UtcNow)
            return new PurchaseCouponResult(false, "Coupon has expired");

        var loyaltyAccount = await context.LoyaltyAccounts
            .FirstOrDefaultAsync(la => la.UserId == request.UserId, cancellationToken);

        if (loyaltyAccount is null)
            return new PurchaseCouponResult(false, "Loyalty account not found");

        if (loyaltyAccount.Points < coupon.PointsCost)
            return new PurchaseCouponResult(false, $"Insufficient points. Need {coupon.PointsCost}, have {loyaltyAccount.Points}");

        loyaltyAccount.Points -= coupon.PointsCost;
        loyaltyAccount.UpdatedAtUtc = DateTime.UtcNow;

        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = loyaltyAccount.Id,
            PointsChange = -coupon.PointsCost,
            Type = LoyaltyTransactionType.Redeemed,
            Description = $"Purchased coupon: {coupon.Name}",
            CreatedAtUtc = DateTime.UtcNow
        };

        var userCoupon = new UserCoupon
        {
            UserId = request.UserId,
            CouponId = request.CouponId,
            AcquiredAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = coupon.ExpiresAtUtc,
            IsUsed = false
        };

        context.LoyaltyTransactions.Add(transaction);
        context.UserCoupons.Add(userCoupon);
        await context.SaveChangesAsync(cancellationToken);

        return new PurchaseCouponResult(true, "Coupon purchased successfully", userCoupon.Id, loyaltyAccount.Points);
    }
}
