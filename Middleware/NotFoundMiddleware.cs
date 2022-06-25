using MeerkatDotnet.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeerkatDotnet.Middleware;

public class NotFoundMiddleware
{
    private readonly RequestDelegate _next;

    public NotFoundMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (EntityNotFoundException e)
        {
            ProblemDetails details = new()
            {
                Title = "Requested entity could not be found",
                Detail = e.Message,
                Status = 404
            };
            await Results.NotFound(details).ExecuteAsync(context);
        }
    }
}
