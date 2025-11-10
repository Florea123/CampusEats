using CampusEats.Api.Domain;
using Microsoft.EntityFrameworkCore;
using MediatR;
namespace CampusEats.Api.Features.Orders.GetOrders;

public record GetOrdersQuerry(Guid Id) : IRequest<OrderDto?>;