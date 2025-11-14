using MediatR;
using Stripe;

namespace CampusEats.Api.Features.Payments.ConfirmPayment;

public static class ConfirmPaymentEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/payments/webhook", async (
            HttpContext context,
            IMediator mediator,
            IConfiguration config) =>
        {
            var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                context.Request.Headers["Stripe-Signature"],
                config["Stripe:WebhookSecret"]
            );

            await mediator.Send(new ConfirmPaymentCommand(
                stripeEvent.Type,
                stripeEvent.Data.RawObject.ToString()
            ));

            return Results.Ok();
        });
    }
}