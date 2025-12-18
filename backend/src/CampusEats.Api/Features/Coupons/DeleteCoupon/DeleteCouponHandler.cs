using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using CampusEats.Api.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Coupons.DeleteCoupon;

public class DeleteCouponHandler(AppDbContext context) 
    : IRequestHandler<DeleteCouponCommand, DeleteCouponResult>
{
    public async Task<DeleteCouponResult> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await context.Coupons
            .FirstOrDefaultAsync(c => c.Id == request.CouponId, cancellationToken);

        if (coupon == null)
            return new DeleteCouponResult(false, "Coupon not found");

        // Găsește toți userii care au cumpărat acest cupon
        var userCoupons = await context.UserCoupons
            .Include(uc => uc.User)
            .Where(uc => uc.CouponId == request.CouponId)
            .ToListAsync(cancellationToken);

        // Returnează punctele de loialitate pentru fiecare utilizator
        foreach (var userCoupon in userCoupons)
        {
            var loyaltyAccount = await context.LoyaltyAccounts
                .FirstOrDefaultAsync(la => la.UserId == userCoupon.UserId, cancellationToken);

            if (loyaltyAccount != null)
            {
                // Adaugă punctele înapoi
                loyaltyAccount.Points += coupon.PointsCost;
                loyaltyAccount.UpdatedAtUtc = DateTime.UtcNow;

                // Creează o tranzacție de refund
                var transaction = new LoyaltyTransaction
                {
                    LoyaltyAccountId = loyaltyAccount.Id,
                    PointsChange = coupon.PointsCost,
                    Type = LoyaltyTransactionType.Adjusted,
                    Description = $"Refund pentru cuponul șters: {coupon.Name}",
                    CreatedAtUtc = DateTime.UtcNow
                };

                context.LoyaltyTransactions.Add(transaction);
            }
        }

        // Șterge toate UserCoupon-urile asociate
        context.UserCoupons.RemoveRange(userCoupons);

        // Șterge cuponul
        context.Coupons.Remove(coupon);

        await context.SaveChangesAsync(cancellationToken);

        return new DeleteCouponResult(true, $"Coupon deleted successfully. Refunded {userCoupons.Count} users.");
    }
}
