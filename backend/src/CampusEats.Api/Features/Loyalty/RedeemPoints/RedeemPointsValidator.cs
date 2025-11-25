using FluentValidation;

namespace CampusEats.Api.Features.Loyalty.RedeemPoints;

public class RedeemPointsValidator : AbstractValidator<RedeemPointsCommand>
{
    public RedeemPointsValidator()
    {
        RuleFor(x => x.Points).GreaterThan(0).WithMessage("Points must be greater than 0");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
    }
}