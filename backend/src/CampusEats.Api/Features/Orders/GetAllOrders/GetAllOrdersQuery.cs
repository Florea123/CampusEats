using MediatR;

namespace CampusEats.Api.Features.Orders.GetOrders;

public record GetAllOrdersQuery(bool All = false) : IRequest<List<CampusEats.Api.Features.Orders.OrderDto>>;
