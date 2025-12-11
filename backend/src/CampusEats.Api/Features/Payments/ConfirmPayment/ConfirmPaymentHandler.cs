using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using CampusEats.Api.Enums;
using CampusEats.Api.Features.Payments.CreatePaymentSession;
using CampusEats.Api.Infrastructure.Loyalty;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Payments.ConfirmPayment;

public class ConfirmPaymentHandler(AppDbContext db, ILoyaltyService loyaltyService) : IRequestHandler<ConfirmPaymentCommand>
{
    public async Task Handle(ConfirmPaymentCommand request, CancellationToken ct)
    {
        if (request.EventType != "checkout.session.completed")
            return;

        using var doc = JsonDocument.Parse(request.PayloadJson);
        var root = doc.RootElement;

        JsonElement metadataEl;
        if (root.TryGetProperty("metadata", out metadataEl) == false)
        {
            if (root.TryGetProperty("data", out var dataEl)
                && dataEl.ValueKind == JsonValueKind.Object
                && dataEl.TryGetProperty("object", out var objEl)
                && objEl.TryGetProperty("metadata", out metadataEl))
            {
              
            }
            else
            {
                throw new InvalidDataException("Missing `metadata` in payload or in `data.object`.");
            }
        }

        if (!metadataEl.TryGetProperty("payment_id", out var paymentIdEl) ||
            !metadataEl.TryGetProperty("user_id", out var userIdEl) ||
            !metadataEl.TryGetProperty("order_items", out var orderItemsEl))
        {
            throw new InvalidDataException("Required metadata keys (payment_id, user_id, order_items) are missing.");
        }

        var paymentIdRaw = paymentIdEl.GetString();
        var userIdRaw = userIdEl.GetString();
        var orderItemsJson = orderItemsEl.GetString();

        string? userCouponIdRaw = null;
        if (metadataEl.TryGetProperty("user_coupon_id", out var userCouponIdEl))
        {
            userCouponIdRaw = userCouponIdEl.GetString();
        }

        if (string.IsNullOrWhiteSpace(paymentIdRaw) || string.IsNullOrWhiteSpace(userIdRaw) || string.IsNullOrWhiteSpace(orderItemsJson))
            throw new InvalidDataException("One or more required metadata values are empty.");

        await HandleCheckoutCompleted(paymentIdRaw, userIdRaw, orderItemsJson, userCouponIdRaw, ct);
    }

    private async Task HandleCheckoutCompleted(string paymentIdRaw, string userIdRaw, string orderItemsJson, string? userCouponIdRaw, CancellationToken ct)
    {
        var paymentId = Guid.Parse(paymentIdRaw);
        var userId = Guid.Parse(userIdRaw);
        var orderItems = JsonSerializer.Deserialize<List<OrderItemDto>>(orderItemsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                         ?? new List<OrderItemDto>();

        var payment = await db.Payments.FindAsync(new object[] { paymentId }, ct);
        if (payment == null) return;

        var menuItemIds = orderItems.Select(x => Guid.Parse(x.MenuItemId)).ToList();
        var menuItems = await db.MenuItems
            .Where(m => menuItemIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in orderItems)
        {
            if (!Guid.TryParse(item.MenuItemId, out var menuItemId)) continue;
            if (!menuItems.TryGetValue(menuItemId, out var menuItem)) continue;

            order.Items.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = item.Quantity,
                UnitPrice = menuItem.Price
            });
        }

        // Calculate subtotal
        order.Subtotal = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        order.Total = order.Subtotal;

        // Apply coupon if provided
        if (!string.IsNullOrWhiteSpace(userCouponIdRaw) && Guid.TryParse(userCouponIdRaw, out var userCouponId))
        {
            var userCoupon = await db.UserCoupons
                .Include(uc => uc.Coupon)
                .ThenInclude(c => c.SpecificMenuItem)
                .FirstOrDefaultAsync(uc => uc.Id == userCouponId && 
                                           uc.UserId == userId && 
                                           !uc.IsUsed &&
                                           (uc.ExpiresAtUtc == null || uc.ExpiresAtUtc > DateTime.UtcNow), ct);

            if (userCoupon != null && userCoupon.Coupon.IsActive)
            {
                var coupon = userCoupon.Coupon;
                decimal discountAmount = 0;

                // Check minimum order amount
                if (!coupon.MinimumOrderAmount.HasValue || order.Subtotal >= coupon.MinimumOrderAmount.Value)
                {
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
        }

        var kitchenTasks = new KitchenTask
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            AssignedTo = userId,
            Status = KitchenTaskStatus.NotStarted,
            Notes = "",
            UpdatedAt = DateTime.UtcNow
        };
        db.KitchenTasks.Add(kitchenTasks);

        db.Orders.Add(order);

        payment.OrderId = order.Id;
        payment.Status = PaymentStatus.SUCCEDED;
        payment.CompletedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        
        await loyaltyService.AwardPointsForOrder(userId, order.Id, order.Total);
    }
}