using MediatR;
using CampusEats.Api.Features.Orders;


namespace CampusEats.Api.Features.Orders.PlaceOrder;

public record PlaceOrderCommand(OrderCreateDto Order) : IRequest<Guid>;