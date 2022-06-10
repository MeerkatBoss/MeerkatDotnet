using FluentValidation;
using MeerkatDotnet.Models.Requests;

namespace MeerkatDotnet.Validators;

public class LogInRequestValidator : AbstractValidator<LogInRequest>
{
    public LogInRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Login)
            .NotNull().NotEmpty()
            .IsValidUsername();

        RuleFor(x => x.Password)
            .NotNull().NotEmpty()
            .IsValidPassword();
    }
}
