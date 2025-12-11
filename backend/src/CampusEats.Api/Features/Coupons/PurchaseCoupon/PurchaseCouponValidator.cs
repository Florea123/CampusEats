using FluentValidation;

namespace CampusEats.Api.Features.Coupons.PurchaseCoupon;

public class PurchaseCouponValidator : AbstractValidator<PurchaseCouponCommand>
{
    public PurchaseCouponValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.CouponId)
            .NotEmpty().WithMessage("Coupon ID is required");
    }
}
