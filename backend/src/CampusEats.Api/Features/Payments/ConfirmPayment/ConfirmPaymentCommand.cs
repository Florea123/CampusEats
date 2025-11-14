using MediatR;

namespace CampusEats.Api.Features.Payments.ConfirmPayment;

public record ConfirmPaymentCommand(string EventType, string PayloadJson) : IRequest;