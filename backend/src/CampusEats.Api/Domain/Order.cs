using CampusEats.Api.Enums;

namespace CampusEats.Api.Domain;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal Total { get; set;  }
    public string? Notes { get; set; }
    public Guid? AppliedCouponId { get; set; }
    public bool LoyaltyPointsAwarded { get; set; } = false;
    public List<OrderItem> Items { get; set; } = new();
    public UserCoupon? AppliedCoupon { get; set; }
}