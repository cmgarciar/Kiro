using abcclientes.Domain.Exceptions;
using System.Text.Json;

namespace abcclientes.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Not found: {Message}", ex.Message);
            await WriteResponse(context, 404, ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            await WriteResponse(context, 400, ex.Message);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Database error");
            await WriteResponse(context, 500, "Internal server error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            await WriteResponse(context, 500, "Internal server error");
        }
    }

    private static async Task WriteResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new { message });
        await context.Response.WriteAsync(payload);
    }
}
