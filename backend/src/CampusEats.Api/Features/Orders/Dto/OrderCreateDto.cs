namespace CampusEats.Api.Features.Orders;

public class OrderCreateDto
{
    public List<OrderItemCreateDto>? Items { get; init; } = new();
    public string? Notes { get; init; }
    public Guid? UserCouponId { get; init; }
}