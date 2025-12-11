using CampusEats.Api.Enums;

namespace CampusEats.Api.Domain;

public class Coupon
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public CouponType Type { get; set; }
    public decimal DiscountValue { get; set; }
    public int PointsCost { get; set; }
    public Guid? SpecificMenuItemId { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }

    public MenuItem? SpecificMenuItem { get; set; }
}
