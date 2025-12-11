using CampusEats.Api.Infrastructure.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusEats.Api.Features.Coupons.GetAvailableCoupons;

public static class GetAvailableCouponsEndpoint
{
    public static void MapGetAvailableCoupons(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coupons/available", async (
            ClaimsPrincipal user,
            [FromServices] IMediator mediator) =>
        {
            Guid? userId = null;
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }
            
            var result = await mediator.Send(new GetAvailableCouponsQuery(userId));
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Coupons");
    }
}
