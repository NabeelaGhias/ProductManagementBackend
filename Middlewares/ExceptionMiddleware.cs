using ProductManagementSystem.Configs;
using ProductManagementSystem.Helpers;
using System.Net;
using System.Text.Json;

namespace ProductManagementSystem.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = exception switch
            {
                ApiException apiEx => new ResponseEntity(apiEx.StatusCode, apiEx.Message, apiEx.Details),
                _ => new ResponseEntity(500, "An internal server error occurred")
            };

            context.Response.StatusCode = response.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
