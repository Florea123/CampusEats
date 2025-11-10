namespace CampusEats.Api.Features.Orders;

public class OrderItemDto
{
    public Guid Id { get; init; }
    public Guid MenuItemId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? MenuItemName { get; init; }
}