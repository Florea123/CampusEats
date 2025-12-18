using CampusEats.Api.Enums;
using MediatR;
using System.Security.Claims;

namespace CampusEats.Api.Features.Coupons.DeleteCoupon;

public static class DeleteCouponEndpoint
{
    public static void MapDeleteCoupon(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/coupons/{couponId:guid}", async (
            Guid couponId,
            IMediator mediator,
            HttpContext httpContext) =>
        {
            var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (!Enum.TryParse<UserRole>(roleClaim, out var role) || role != UserRole.MANAGER)
                return Results.Forbid();

            var command = new DeleteCouponCommand(couponId);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.NotFound(result);

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Coupons");
    }
}
