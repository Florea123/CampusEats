using MediatR;
using System.Security.Claims;
using CampusEats.Api.Features.Orders;
using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Enums;
namespace CampusEats.Api.Features.Orders.PlaceOrder;

public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public PlaceOrderHandler(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken ct)
    {
        if (request?.Order == null) throw new ArgumentNullException(nameof(request.Order));
        IEnumerable<OrderItemCreateDto> items = request.Order.Items ?? Enumerable.Empty<OrderItemCreateDto>();
        if (!items.Any()) throw new InvalidOperationException("Order must contain at least one item.");

        // merge duplicates by menu item id
        var grouped = items
            .GroupBy(i => i.MenuItemId)
            .Select(g => new { MenuItemId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        if (grouped.Any(g => g.Quantity <= 0))
            throw new InvalidOperationException("All quantities must be greater than zero.");

        var menuIds = grouped.Select(g => g.MenuItemId).Distinct().ToList();

        var menuItems = await _db.MenuItems
            .Where(m => menuIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);

        var missing = menuIds.Except(menuItems.Keys).ToList();
        if (missing.Any())
            throw new InvalidOperationException($"Menu items not found: {string.Join(',', missing)}");

        var user = _http.HttpContext?.User ?? throw new InvalidOperationException("No authenticated user.");
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(idClaim, out var userId)) throw new InvalidOperationException("Invalid user id claim.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Notes = request.Order.Notes?.Trim(),
            Items = new System.Collections.Generic.List<OrderItem>()
        };

        foreach (var g in grouped)
        {
            var menu = menuItems[g.MenuItemId];
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                MenuItemId = menu.Id,
                Quantity = g.Quantity,
                UnitPrice = menu.Price
            };
            order.Items.Add(orderItem);
        }

        order.Subtotal = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        order.Total = order.Subtotal;

        // Apply coupon if provided
        if (request.Order.UserCouponId.HasValue)
        {
            var userCoupon = await _db.UserCoupons
                .Include(uc => uc.Coupon)
                .ThenInclude(c => c.SpecificMenuItem)
                .FirstOrDefaultAsync(uc => uc.Id == request.Order.UserCouponId.Value && 
                                           uc.UserId == userId && 
                                           !uc.IsUsed &&
                                           (uc.ExpiresAtUtc == null || uc.ExpiresAtUtc > DateTime.UtcNow), ct);

            if (userCoupon != null && userCoupon.Coupon.IsActive)
            {
                var coupon = userCoupon.Coupon;
                
                // Check minimum order amount
                if (coupon.MinimumOrderAmount.HasValue && order.Subtotal < coupon.MinimumOrderAmount.Value)
                    throw new InvalidOperationException($"Minimum order amount for this coupon is {coupon.MinimumOrderAmount.Value}");

                decimal discountAmount = 0;

                switch (coupon.Type)
                {
                    case CampusEats.Api.Enums.CouponType.PercentageDiscount:
                        discountAmount = order.Subtotal * (coupon.DiscountValue / 100m);
                        break;
                    
                    case CampusEats.Api.Enums.CouponType.FixedAmountDiscount:
                        discountAmount = Math.Min(coupon.DiscountValue, order.Subtotal);
                        break;
                    
                    case CampusEats.Api.Enums.CouponType.FreeItem:
                        if (coupon.SpecificMenuItemId.HasValue)
                        {
                            var freeItem = order.Items.FirstOrDefault(i => i.MenuItemId == coupon.SpecificMenuItemId.Value);
                            if (freeItem != null)
                                discountAmount = freeItem.UnitPrice;
                        }
                        break;
                }

                order.DiscountAmount = discountAmount;
                order.Total = Math.Max(0, order.Subtotal - discountAmount);
                order.AppliedCouponId = userCoupon.Id;

                // Mark coupon as used
                userCoupon.IsUsed = true;
                userCoupon.UsedAtUtc = DateTime.UtcNow;
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        var kitchenTask = new KitchenTask()
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = KitchenTaskStatus.NotStarted,
            UpdatedAt = DateTime.UtcNow,
            AssignedTo = userId,
            Notes = order.Notes?.Trim()
        };
        _db.KitchenTasks.Add(kitchenTask);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return order.Id;
    }
}