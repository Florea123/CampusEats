using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Routing;
using CampusEats.Api.Features.Orders.GetOrders;
using CampusEats.Api.Features.Orders.PlaceOrder;

namespace CampusEats.Api.Features.Orders;

public static class OrdersEndpoint
{
    public static void MapOrders(this IEndpointRouteBuilder app)
    {
        // Place a new order
        app.MapPost("/api/orders",
            async (PlaceOrderCommand cmd, IMediator mediator, CancellationToken ct) =>
            {
                var id = await mediator.Send(cmd, ct);
                return Results.Created($"/api/orders/{id}", new { id });
            })
            .WithTags("Orders")
            .RequireAuthorization();

        // Get all orders (admins can request all using ?all=true; regular users get only their orders)
        app.MapGet("/api/orders",
            async (bool? all, IMediator mediator, CancellationToken ct) =>
            {
                var query = new GetAllOrdersQuery(all ?? false);
                var list = await mediator.Send(query, ct);
                return Results.Ok(list);
            })
            .WithTags("Orders")
            .RequireAuthorization();

        // Get single order by id
        app.MapGet("/api/orders/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                var order = await mediator.Send(new GetOrdersQuerry(id), ct);
                return order is null ? Results.NotFound() : Results.Ok(order);
            })
            .WithTags("Orders")
            .RequireAuthorization();

        // Cancel an order
        app.MapPost("/api/orders/{id:guid}/cancel",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                var success = await mediator.Send(new CancelOrderCommand(id), ct);
                return success ? Results.NoContent() : Results.NotFound();
            })
            .WithTags("Orders")
            .RequireAuthorization();
    }
}