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
            IConfiguration config,
            ILogger<Program> logger) =>
        {
            try
            {
                var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    context.Request.Headers["Stripe-Signature"],
                    config["Stripe:WebhookSecret"]
                );

                logger.LogInformation("Processing Stripe webhook: {EventType} - {EventId}", 
                    stripeEvent.Type, stripeEvent.Id);

                await mediator.Send(new ConfirmPaymentCommand(
                    stripeEvent.Type,
                    stripeEvent.Data.RawObject.ToString()
                ));

                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Stripe webhook");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Webhook processing failed"
                );
            }
        });
        
        app.MapPost("/api/payments/create-session", async (CreatePaymentSessionCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return Results.Ok(result);
        }).RequireAuthorization();
    }  
}