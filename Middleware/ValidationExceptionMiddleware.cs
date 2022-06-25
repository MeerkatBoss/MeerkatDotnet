using FluentValidation;

namespace MeerkatDotnet.Middleware;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException e)
        {
            Dictionary<string, string[]> errorsDict = e.Errors.ToDictionary(
                    err => err.PropertyName,
                    err => new[] {err.ErrorMessage});
            HttpValidationProblemDetails details = new(errorsDict);
            await Results.BadRequest(details).ExecuteAsync(context);
        }
    }
}
