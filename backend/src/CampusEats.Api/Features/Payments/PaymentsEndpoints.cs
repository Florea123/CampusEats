using CampusEats.Api.Features.Payments.ConfirmPayment;
using CampusEats.Api.Features.Payments.CreatePaymentSession;
using MediatR;
using Stripe;

namespace CampusEats.Api.Features.Payments;

public static class PaymentsEndpoints
{
    public static void MapPayments(this IEndpointRouteBuilder app)
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
        
        app.MapPost("/api/payments/create-session", async (CreatePaymentSessionCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return Results.Ok(result);
        }).RequireAuthorization();
    }  
}