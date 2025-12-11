using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusEats.Api.Features.Coupons.PurchaseCoupon;

public static class PurchaseCouponEndpoint
{
    public static void MapPurchaseCoupon(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/coupons/purchase", async (
            [FromBody] PurchaseCouponRequest request,
            [FromServices] IMediator mediator,
            ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var command = new PurchaseCouponCommand(userId, request.CouponId);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Coupons");
    }
}
