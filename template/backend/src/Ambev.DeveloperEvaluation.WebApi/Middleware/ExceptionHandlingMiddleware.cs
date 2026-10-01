using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Runtime.ExceptionServices;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    /// <summary>
    /// Translates exceptions into the API error format (<c>{ "type", "error", "detail" }</c>)
    /// with the conventional HTTP status codes.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, error) = exception switch
            {
                ValidationException validation => (StatusCodes.Status400BadRequest, new ErrorResponse(
                    "ValidationError",
                    "Invalid input data",
                    string.Join(" ", validation.Errors.Select(e => e.ErrorMessage).Distinct()))),

                ResourceNotFoundException notFound => (StatusCodes.Status404NotFound,
                    new ErrorResponse("ResourceNotFound", notFound.Error, notFound.Detail)),

                ConflictException conflict => (StatusCodes.Status409Conflict,
                    new ErrorResponse("ResourceConflict", conflict.Error, conflict.Detail)),

                InvalidCredentialsException credentials => (StatusCodes.Status401Unauthorized,
                    new ErrorResponse("AuthenticationError", "Invalid credentials", credentials.Message)),

                DomainException domain => (StatusCodes.Status400BadRequest,
                    new ErrorResponse("BusinessRuleViolation", "Business rule violation", domain.Message)),

                _ => (StatusCodes.Status500InternalServerError,
                    new ErrorResponse("InternalServerError", "An unexpected error occurred", "The server encountered an unexpected condition. Please try again later."))
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            else
                _logger.LogInformation("Request {Method} {Path} failed with {StatusCode}: {Detail}", context.Request.Method, context.Request.Path, statusCode, error.Detail);

            if (context.Response.HasStarted)
                ExceptionDispatchInfo.Capture(exception).Throw();

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(error, JsonOptions));
        }
    }
}
