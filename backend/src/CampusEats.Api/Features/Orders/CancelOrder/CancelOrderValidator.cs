using FluentValidation;

namespace CampusEats.Api.Features.Orders;

public class CancelOrderValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderValidator()
    {
        RuleFor(X => X.Id).NotEmpty();
    }
    
}