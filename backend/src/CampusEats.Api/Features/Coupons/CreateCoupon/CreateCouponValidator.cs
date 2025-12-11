using FluentValidation;

namespace CampusEats.Api.Features.Coupons.CreateCoupon;

public class CreateCouponValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.DiscountValue)
            .GreaterThanOrEqualTo(0).WithMessage("Discount value must be greater than or equal to 0")
            .GreaterThan(0).When(x => x.Type != CampusEats.Api.Enums.CouponType.FreeItem)
            .WithMessage("Discount value must be greater than 0 for percentage and fixed discounts");

        RuleFor(x => x.PointsCost)
            .GreaterThan(0).WithMessage("Points cost must be greater than 0");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0).When(x => x.MinimumOrderAmount.HasValue)
            .WithMessage("Minimum order amount must be greater than or equal to 0");
    }
}
