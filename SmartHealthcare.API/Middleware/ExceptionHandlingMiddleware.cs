using System.Net;
using System.Text.Json;
using SmartHealthcare.Core.DTOs;

namespace SmartHealthcare.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = Guid.NewGuid().ToString();
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred. Please try again later.";
            var errors = new List<string>();

            switch (exception)
            {
                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = argEx.Message;
                    errors.Add(argEx.Message);
                    _logger.LogWarning(argEx, "Bad request: {Message}", argEx.Message);
                    break;

                case KeyNotFoundException keyEx:
                    statusCode = HttpStatusCode.NotFound;
                    message = keyEx.Message;
                    errors.Add(keyEx.Message);
                    _logger.LogWarning(keyEx, "Resource not found: {Message}", keyEx.Message);
                    break;

                case InvalidOperationException invEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = invEx.Message;
                    errors.Add(invEx.Message);
                    _logger.LogWarning(invEx, "Invalid operation: {Message}", invEx.Message);
                    break;

                case UnauthorizedAccessException authEx:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Unauthorized access";
                    errors.Add(authEx.Message);
                    _logger.LogWarning(authEx, "Unauthorized access attempt");
                    break;

                case NullReferenceException nullEx:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "A required resource was not found";
                    _logger.LogError(nullEx, "Null reference exception at {TraceId}", traceId);
                    break;

                case Microsoft.EntityFrameworkCore.DbUpdateException dbEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Database operation failed. Please check your input data.";
                    errors.Add("Database error occurred");
                    _logger.LogError(dbEx, "Database error at {TraceId}", traceId);
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception at {TraceId}: {Message}", traceId, exception.Message);
                    break;
            }

            // In development, include more details
            if (_environment.IsDevelopment())
            {
                errors.Add($"Debug: {exception.Message}");
                if (exception.InnerException != null)
                {
                    errors.Add($"Inner: {exception.InnerException.Message}");
                }
            }

            var errorResponse = new ErrorResponseDto
            {
                Message = message,
                StatusCode = (int)statusCode,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow,
                Errors = errors.Any() ? errors : null
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
        }
    }

    // Extension method for easy registration
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
