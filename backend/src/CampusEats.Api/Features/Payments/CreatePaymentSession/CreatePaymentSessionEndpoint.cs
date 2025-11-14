using MediatR;

namespace CampusEats.Api.Features.Payments.CreatePaymentSession;

public static class CreatePaymentSessionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/payments/create-session", async (CreatePaymentSessionCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}   