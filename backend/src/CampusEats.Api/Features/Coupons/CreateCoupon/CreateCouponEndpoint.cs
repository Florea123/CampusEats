using CampusEats.Api.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusEats.Api.Features.Coupons.CreateCoupon;

public static class CreateCouponEndpoint
{
    public static void MapCreateCoupon(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/coupons", async (
            [FromBody] CreateCouponRequest request,
            [FromServices] IMediator mediator,
            HttpContext httpContext) =>
        {
            var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (!Enum.TryParse<UserRole>(roleClaim, out var role) || role != UserRole.MANAGER)
                return Results.Forbid();

            var command = new CreateCouponCommand(
                request.Name,
                request.Description,
                request.Type,
                request.DiscountValue,
                request.PointsCost,
                request.SpecificMenuItemId,
                request.MinimumOrderAmount,
                request.ExpiresAtUtc
            );

            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Created($"/api/coupons/{result.CouponId}", result);
        })
        .RequireAuthorization()
        .WithTags("Coupons");
    }
}
