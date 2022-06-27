using FluentValidation;
using MeerkatDotnet.Models;

namespace MeerkatDotnet.Validators;

public class UserDeleteModelValidator : AbstractValidator<UserDeleteModel>
{

    public UserDeleteModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Password)
            .IsValidPassword();
    }

}
