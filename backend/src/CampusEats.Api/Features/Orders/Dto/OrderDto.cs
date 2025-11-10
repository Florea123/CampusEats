using CampusEats.Api.Enums;

namespace CampusEats.Api.Features.Orders;

public record OrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; init; }
    public OrderStatus Status { get; init; }
    public decimal Total { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
    public string? Notes { get; init; }
}