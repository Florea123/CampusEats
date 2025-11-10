namespace CampusEats.Api.Features.Orders;

public class OrderItemCreateDto
{
    public Guid MenuItemId { get; init; }
    public int Quantity { get; init; }
}