using System.Net;
using System.Text.Json;
using ProductManagementSystem.Configs;
using ProductManagementSystem.Domain.Models;
using Serilog;

namespace ProductManagementSystem.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Serilog.ILogger _logger;

    public ExceptionMiddleware(RequestDelegate next, Serilog.ILogger logger)
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
        catch (Exception ex)
        {
            _logger.Error(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ResponseEntity
        {
            Success = false,
            Message = "An error occurred while processing your request."
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Unauthorized access";
                break;

            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "The requested resource was not found";
                break;

            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                break;

            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        response.StatusCode = context.Response.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsJsonAsync(response, options);
    }
}
