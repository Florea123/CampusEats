// backend/src/CampusEats.Api/Domain/Payment.cs

using CampusEats.Api.Enums;

namespace CampusEats.Api.Domain;

public class Payment
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }  // ← Make nullable
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public PaymentStatus Status { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeSessionId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    
    public Order? Order { get; set; }  // ← Make nullable
    public User User { get; set; } = null!;
}
