using System.Net;
using System.Text.Json;
using Clientes.Domain.Exceptions;

namespace Clientes.API.Middleware;

/// <summary>
/// Middleware que traduce las excepciones de dominio a respuestas HTTP coherentes.
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
            await EscribirRespuesta(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ValidationException ex)
        {
            await EscribirRespuesta(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Error de base de datos");
            await EscribirRespuesta(context, HttpStatusCode.InternalServerError, "Error interno del servidor.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await EscribirRespuesta(context, HttpStatusCode.InternalServerError, "Error interno del servidor.");
        }
    }

    private static Task EscribirRespuesta(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        var payload = JsonSerializer.Serialize(new { message });
        return context.Response.WriteAsync(payload);
    }
}
