using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Coupons.CreateCoupon;

public class CreateCouponHandler(AppDbContext context) 
    : IRequestHandler<CreateCouponCommand, CreateCouponResult>
{
    public async Task<CreateCouponResult> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        if (request.SpecificMenuItemId.HasValue)
        {
            var menuItemExists = await context.MenuItems
                .AnyAsync(m => m.Id == request.SpecificMenuItemId.Value, cancellationToken);
            
            if (!menuItemExists)
                return new CreateCouponResult(false, "Menu item not found");
        }

        var coupon = new Coupon
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            DiscountValue = request.DiscountValue,
            PointsCost = request.PointsCost,
            SpecificMenuItemId = request.SpecificMenuItemId,
            MinimumOrderAmount = request.MinimumOrderAmount,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = request.ExpiresAtUtc
        };

        context.Coupons.Add(coupon);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateCouponResult(true, "Coupon created successfully", coupon.Id);
    }
}
