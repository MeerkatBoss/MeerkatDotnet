using FluentValidation;
using MeerkatDotnet.Models;

namespace MeerkatDotnet.Validators;

public class UserInputModelValidator : AbstractValidator<UserInputModel>
{
    public UserInputModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Username)
            .NotNull().NotEmpty()
            .IsValidUsername();

        RuleFor(x => x.Password)
            .NotNull().NotEmpty()
            .IsValidPassword();

        RuleFor(x => x.Email)
            .IsValidEmailAddress().When(x => x.Email is not null);

        RuleFor(x => x.Phone)
            .IsValidPhoneNumber().When(x => x.Phone is not null);

    }
}

