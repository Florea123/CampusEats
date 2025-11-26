using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusEats.Api.Features.Loyalty.GetLoyaltyAccount;

public static class GetLoyaltyAccountEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/loyalty/account", async (
                IMediator mediator,
                ClaimsPrincipal user) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                var account = await mediator.Send(new GetLoyaltyAccountQuery(userId));
                return account is not null ? Results.Ok(account) : Results.NotFound();
            })
            .WithTags("Loyalty")
            .RequireAuthorization();
    }
}