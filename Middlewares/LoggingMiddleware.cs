using System.Text;
using Microsoft.Extensions.Logging;

namespace ProductManagementSystem.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log request
            var requestBody = await ReadRequestBody(context.Request);
            _logger.LogInformation("Request: {Method} {Path} {QueryString} {Body}", 
                context.Request.Method, 
                context.Request.Path, 
                context.Request.QueryString,
                requestBody);

            // Capture response
            var originalBodyStream = context.Response.Body;
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Log response
                var responseBodyText = await ReadResponseBody(context.Response);
                _logger.LogInformation("Response: {StatusCode} {Body}", 
                    context.Response.StatusCode, 
                    responseBodyText);

                // Copy response back to original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                // Restore original stream and dispose the memory stream
                context.Response.Body = originalBodyStream;
                responseBody.Dispose();
            }
        }

        private static async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private static async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(response.Body, leaveOpen: true);
            var text = await reader.ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return text;
        }
    }
}
