using CampusEats.Api.Enums;

namespace CampusEats.Api.Domain;

public class LoyaltyTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoyaltyAccountId { get; set; }
    public int PointsChange { get; set; }
    public LoyaltyTransactionType Type { get; set; }
    public string Description { get; set; } = default!;
    public Guid? RelatedOrderId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public LoyaltyAccount LoyaltyAccount { get; set; } = null!;
    public Order? RelatedOrder { get; set; }
}