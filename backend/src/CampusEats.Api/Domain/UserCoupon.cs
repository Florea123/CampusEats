namespace CampusEats.Api.Domain;

public class UserCoupon
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid CouponId { get; set; }
    public DateTime AcquiredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAtUtc { get; set; }
    public Guid? UsedInOrderId { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? ExpiresAtUtc { get; set; }

    public User User { get; set; } = null!;
    public Coupon Coupon { get; set; } = null!;
    public Order? UsedInOrder { get; set; }
}
