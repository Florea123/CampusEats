using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusEats.Api.Features.Coupons.GetUserCoupons;

public static class GetUserCouponsEndpoint
{
    public static void MapGetUserCoupons(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/coupons/my-coupons", async (
            [FromServices] IMediator mediator,
            ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var result = await mediator.Send(new GetUserCouponsQuery(userId));
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Coupons");
    }
}
