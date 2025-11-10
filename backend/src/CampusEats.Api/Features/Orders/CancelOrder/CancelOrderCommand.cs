
using MediatR;

namespace CampusEats.Api.Features.Orders;

public record CancelOrderCommand(Guid Id) : IRequest<bool>
{
    
}