using System.Net;
using System.Text.Json;
using CRM.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IWebHostEnvironment environment)
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
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case NotFoundException notFoundException:
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                errorResponse.Title = "Resource Not Found";
                errorResponse.Status = (int)HttpStatusCode.NotFound;
                errorResponse.Detail = notFoundException.Message;
                response.StatusCode = (int)HttpStatusCode.NotFound;
                _logger.LogWarning(exception, "Resource not found: {Message}", notFoundException.Message);
                break;

            case ValidationException validationException:
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                errorResponse.Title = "Validation Error";
                errorResponse.Status = (int)HttpStatusCode.BadRequest;
                errorResponse.Detail = validationException.Message;
                errorResponse.Errors = validationException.Errors;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogWarning(exception, "Validation error: {Message}", validationException.Message);
                break;

            case BadRequestException badRequestException:
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                errorResponse.Title = "Bad Request";
                errorResponse.Status = (int)HttpStatusCode.BadRequest;
                errorResponse.Detail = badRequestException.Message;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogWarning(exception, "Bad request: {Message}", badRequestException.Message);
                break;

            case DbUpdateConcurrencyException concurrencyException:
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
                errorResponse.Title = "Concurrency Conflict";
                errorResponse.Status = (int)HttpStatusCode.Conflict;
                errorResponse.Detail = "The record you are trying to update has been modified by another user. Please refresh and try again.";
                response.StatusCode = (int)HttpStatusCode.Conflict;
                _logger.LogWarning(exception, "Concurrency conflict occurred");
                break;

            case UnauthorizedAccessException unauthorizedException:
                errorResponse.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                errorResponse.Title = "Unauthorized";
                errorResponse.Status = (int)HttpStatusCode.Unauthorized;
                errorResponse.Detail = "You are not authorized to perform this action.";
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                _logger.LogWarning(exception, "Unauthorized access attempt");
                break;

            case ForbiddenException forbiddenException:
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3";
                errorResponse.Title = "Forbidden";
                errorResponse.Status = (int)HttpStatusCode.Forbidden;
                errorResponse.Detail = forbiddenException.Message;
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                _logger.LogWarning(exception, "Forbidden access: {Message}", forbiddenException.Message);
                break;

            case ConflictException conflictException:
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
                errorResponse.Title = "Conflict";
                errorResponse.Status = (int)HttpStatusCode.Conflict;
                errorResponse.Detail = conflictException.Message;
                response.StatusCode = (int)HttpStatusCode.Conflict;
                _logger.LogWarning(exception, "Conflict occurred: {Message}", conflictException.Message);
                break;

            case UnprocessableEntityException unprocessableException:
                errorResponse.Type = "https://tools.ietf.org/html/rfc4918#section-11.2";
                errorResponse.Title = "Unprocessable Entity";
                errorResponse.Status = 422; // Unprocessable Entity
                errorResponse.Detail = unprocessableException.Message;
                errorResponse.Errors = unprocessableException.Errors;
                response.StatusCode = 422;
                _logger.LogWarning(exception, "Unprocessable entity: {Message}", unprocessableException.Message);
                break;

            case DbUpdateException dbUpdateException:
                // Handle database update exceptions (foreign key, unique constraint, etc.)
                var dbErrorDetail = ExtractDatabaseError(dbUpdateException);
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                errorResponse.Title = "Database Error";
                errorResponse.Status = (int)HttpStatusCode.BadRequest;
                errorResponse.Detail = dbErrorDetail;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogWarning(exception, "Database update error: {Message}", dbUpdateException.Message);
                break;

            case InvalidOperationException invalidOperationException:
                // Handle invalid operation exceptions (business logic errors)
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                errorResponse.Title = "Invalid Operation";
                errorResponse.Status = (int)HttpStatusCode.BadRequest;
                errorResponse.Detail = invalidOperationException.Message;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogWarning(exception, "Invalid operation: {Message}", invalidOperationException.Message);
                break;

            case TimeoutException timeoutException:
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5";
                errorResponse.Title = "Request Timeout";
                errorResponse.Status = (int)HttpStatusCode.RequestTimeout;
                errorResponse.Detail = "The request took too long to process. Please try again.";
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                _logger.LogWarning(exception, "Request timeout occurred");
                break;

            case TaskCanceledException taskCanceledException:
                // Handle cancellation token exceptions
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5";
                errorResponse.Title = "Request Cancelled";
                errorResponse.Status = 499; // Client Closed Request
                errorResponse.Detail = "The request was cancelled.";
                response.StatusCode = 499;
                _logger.LogInformation("Request was cancelled");
                break;

            default:
                errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                errorResponse.Title = "An error occurred while processing your request";
                errorResponse.Status = (int)HttpStatusCode.InternalServerError;
                errorResponse.Detail = _environment.IsDevelopment()
                    ? exception.ToString()
                    : "An error occurred while processing your request. Please try again later.";
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await response.WriteAsync(json);
    }

    /// <summary>
    /// Extracts user-friendly error messages from database exceptions.
    /// </summary>
    private static string ExtractDatabaseError(DbUpdateException exception)
    {
        // Check for foreign key constraint violations
        if (exception.InnerException?.Message.Contains("FOREIGN KEY") == true ||
            exception.InnerException?.Message.Contains("REFERENCE constraint") == true)
        {
            return "Bu kayıt başka kayıtlarla ilişkili olduğu için silinemez veya güncellenemez.";
        }

        // Check for unique constraint violations
        if (exception.InnerException?.Message.Contains("UNIQUE KEY") == true ||
            exception.InnerException?.Message.Contains("UNIQUE constraint") == true ||
            exception.InnerException?.Message.Contains("Cannot insert duplicate key") == true)
        {
            return "Bu değer zaten kullanılıyor. Lütfen farklı bir değer giriniz.";
        }

        // Check for NOT NULL constraint violations
        if (exception.InnerException?.Message.Contains("NOT NULL") == true ||
            exception.InnerException?.Message.Contains("cannot be null") == true)
        {
            return "Zorunlu alanlar eksik. Lütfen tüm zorunlu alanları doldurunuz.";
        }

        // Check for check constraint violations
        if (exception.InnerException?.Message.Contains("CHECK constraint") == true)
        {
            return "Girilen değer geçersiz. Lütfen geçerli bir değer giriniz.";
        }

        // Generic database error
        return "Veritabanı işlemi sırasında bir hata oluştu. Lütfen tekrar deneyiniz.";
    }
}

