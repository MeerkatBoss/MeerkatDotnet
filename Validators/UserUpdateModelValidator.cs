using FluentValidation;
using MeerkatDotnet.Models;

namespace MeerkatDotnet.Validators;

public class UserUpdateModelValidator : AbstractValidator<UserUpdateModel>
{
    public UserUpdateModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Username)
            .IsValidUsername()
            .When(x => x.Username is not null);

        RuleFor(x => x.Password)
            .IsValidPassword()
            .When(x => x.Password is not null);

        RuleFor(x => x.Email)
            .IsValidEmailAddress()
            .When(x => !String.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .IsValidPhoneNumber()
            .When(x => !String.IsNullOrEmpty(x.Phone));
    }
}
