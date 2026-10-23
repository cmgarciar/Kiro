using abcclientes.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace abcclientes.API.Middleware;

/// <summary>
/// Global exception handler middleware that maps domain exceptions to HTTP responses.
/// </summary>
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
            _logger.LogWarning(ex, "NotFoundException: {Message}", ex.Message);
            await WriteResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "ValidationException: {Message}", ex.Message);
            await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "DatabaseException: {Message}", ex.Message);
            await WriteResponse(context, HttpStatusCode.InternalServerError, "Internal server error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteResponse(context, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new { message });
        await context.Response.WriteAsync(payload);
    }
}
