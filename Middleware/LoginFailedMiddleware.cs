using MeerkatDotnet.Services;

namespace MeerkatDotnet.Middleware;

public class LoginFailedMiddleware
{
    private readonly RequestDelegate _next;

    public LoginFailedMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (LoginFailedException)
        {
            await Results.Unauthorized().ExecuteAsync(context);
        }
    }
}
