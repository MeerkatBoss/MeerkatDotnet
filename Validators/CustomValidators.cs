using FluentValidation;

namespace MeerkatDotnet.Validators;

public static class CustomValidators
{

    public static IRuleBuilderOptions<T, string?> IsValidUsername<T>(
            this IRuleBuilder<T, string?> builder
        ) => builder
            .Matches("\\w+").WithMessage("Invalid characters in username")
            .MinimumLength(2).WithMessage("Username too short (minimum length is {MinLength})")
            .MaximumLength(32).WithMessage("Username too long (maximum length is {MaxLength})");

    public static IRuleBuilderOptions<T, string?> IsValidPassword<T>(
            this IRuleBuilder<T, string?> builder
        ) => builder
            .Matches("[\\w!@#$%^&]+").WithMessage("Invalid characters in password")
            .MinimumLength(8).WithMessage("Password too short (minimum length is {MinLength})")
            .MaximumLength(128).WithMessage("Password too long (maximum length is {MaxLength})");

    public static IRuleBuilderOptions<T, string?> IsValidEmailAddress<T>(
            this IRuleBuilder<T, string?> builder
        ) => builder
            .Matches("\\w+@\\w+\\.\\w+") .WithMessage("Invalid email provided");

    public static IRuleBuilderOptions<T, string?> IsValidPhoneNumber<T>(
            this IRuleBuilder<T, string?> builder
        ) => builder
            .Matches(@"\+?\d*(\(\d+\)| \(\d+\) )?\d+(-\d+)*")
            .WithMessage("Invalid phone number provided");

}
