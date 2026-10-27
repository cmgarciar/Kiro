using System.Net;
using System.Text.Json;
using OnPremise.Domain.Exceptions;

namespace OnPremise.API.Middleware;

/// <summary>
/// Middleware global que traduce excepciones de dominio a respuestas HTTP consistentes.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Error de base de datos");
            await WriteAsync(context, HttpStatusCode.InternalServerError, "Error interno del servidor.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await WriteAsync(context, HttpStatusCode.InternalServerError, "Error interno del servidor.");
        }
    }

    private static async Task WriteAsync(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        var payload = JsonSerializer.Serialize(new { message });
        await context.Response.WriteAsync(payload);
    }
}
