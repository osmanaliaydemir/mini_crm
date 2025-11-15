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

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
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
}

