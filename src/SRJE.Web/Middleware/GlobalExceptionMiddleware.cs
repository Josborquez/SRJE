using System.Net;
using System.Text.Json;
using SRJE.Web.Models.Exceptions;

namespace SRJE.Web.Middleware;

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
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Solicitud invalida: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Recurso no encontrado: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (BusinessConflictException ex)
        {
            _logger.LogWarning(ex, "Conflicto de negocio: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Response already started, cannot write error response for {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                throw;
            }

            _logger.LogError(ex, "Error no controlado procesando {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError,
                "Error interno del servidor. Contacte al administrador.");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = message,
            status = (int)statusCode,
            traceId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
