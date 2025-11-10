using FluentValidation;

namespace CampusEats.Api.Features.Orders.PlaceOrder;

public class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderValidator()
    {
        RuleFor(x => x.Order).NotNull();
        RuleFor(x => x.Order.Items).NotEmpty().WithMessage("Order must contain at least one item.");
        RuleForEach(x => x.Order.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.MenuItemId).NotEmpty();
            items.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        });
    }
    
}