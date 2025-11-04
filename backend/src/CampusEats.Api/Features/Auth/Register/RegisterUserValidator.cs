using FluentValidation;

namespace CampusEats.Api.Features.Auth.Register;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Must contain a non-alphanumeric.");
        RuleFor(x => x.Role).IsInEnum();
    }
}