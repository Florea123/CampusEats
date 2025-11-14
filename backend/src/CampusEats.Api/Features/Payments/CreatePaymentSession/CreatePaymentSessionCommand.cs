using MediatR;

namespace CampusEats.Api.Features.Payments.CreatePaymentSession;

public record CreatePaymentSessionCommand(List<OrderItemDto> Items) : IRequest<CreatePaymentSessionResult>;

public record OrderItemDto(string MenuItemId, int Quantity);

public record CreatePaymentSessionResult(string SessionId, string CheckoutUrl);