using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusEats.Api.Features.Loyalty.RedeemPoints;

public static class RedeemPointsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/loyalty/redeem", async (
                [FromBody] RedeemPointsRequest request,
                [FromServices] IMediator mediator,
                ClaimsPrincipal user) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var result = await mediator.Send(new RedeemPointsCommand(userId, request.Points, request.Description));
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .RequireAuthorization()
            .WithTags("Loyalty")
            .WithOpenApi();
    }
}