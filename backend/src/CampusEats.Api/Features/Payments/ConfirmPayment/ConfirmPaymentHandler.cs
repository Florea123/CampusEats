using CampusEats.Api.Data;
using CampusEats.Api.Domain;
using CampusEats.Api.Enums;
using CampusEats.Api.Features.Payments.CreatePaymentSession;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Text.Json;

namespace CampusEats.Api.Features.Payments.ConfirmPayment;

public class ConfirmPaymentHandler(AppDbContext db) : IRequestHandler<ConfirmPaymentCommand>
{
    public async Task Handle(ConfirmPaymentCommand request, CancellationToken ct)
    {
        if (request.EventType == "checkout.session.completed")
        {
            var session = JsonSerializer.Deserialize<Session>(request.PayloadJson);
            await HandleCheckoutCompleted(session!, ct);
        }
    }

    private async Task HandleCheckoutCompleted(Session session, CancellationToken ct)
    {
        var paymentId = Guid.Parse(session.Metadata["payment_id"]);
        var userId = Guid.Parse(session.Metadata["user_id"]);
        var orderItemsJson = session.Metadata["order_items"];
        var orderItems = JsonSerializer.Deserialize<List<OrderItemDto>>(orderItemsJson);

        var payment = await db.Payments.FindAsync(new object[] { paymentId }, ct);
        if (payment == null) return;

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            Total = payment.Amount,
            CreatedAt = DateTime.UtcNow
        };

        var menuItemIds = orderItems!.Select(x => Guid.Parse(x.MenuItemId)).ToList();
        var menuItems = await db.MenuItems
            .Where(m => menuItemIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);

        foreach (var item in orderItems)
        {
            var menuItem = menuItems[Guid.Parse(item.MenuItemId)];
            order.Items.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = item.Quantity,
                UnitPrice = menuItem.Price
            });
        }

        db.Orders.Add(order);

        payment.OrderId = order.Id;
        payment.Status = PaymentStatus.SUCCEDED;
        payment.CompletedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
