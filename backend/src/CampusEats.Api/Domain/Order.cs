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
    public decimal Total { get; set;  }
    public string? Notes { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}