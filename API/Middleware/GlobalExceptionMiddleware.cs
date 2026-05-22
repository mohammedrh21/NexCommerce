using System.Net;
using System.Text.Json;
using NexCommerce.Application.Common;
using NexCommerce.Application.Exceptions;

namespace NexCommerce.API.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException          => (HttpStatusCode.NotFound,            exception.Message),
            ValidationException        => (HttpStatusCode.BadRequest,     exception.Message),
            UnauthorizedException      => (HttpStatusCode.Unauthorized,       exception.Message),
            ForbiddenException         => (HttpStatusCode.Forbidden,          exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,      exception.Message),
            InvalidOperationException   => (HttpStatusCode.BadRequest,        exception.Message),
            KeyNotFoundException        => (HttpStatusCode.NotFound,          exception.Message),
            _ => (HttpStatusCode.InternalServerError,
                  env.IsDevelopment() ? exception.ToString() : "An unexpected error occurred.")
        };

        var response = ApiResponse<object>.Fail(message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
