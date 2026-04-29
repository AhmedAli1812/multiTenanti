using HMS.SharedKernel.Primitives;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace HMS.API.Middleware;

/// <summary>
/// Global exception handler — maps domain exceptions to HTTP problem details.
/// Replaces try/catch in every controller.
///
/// Mapping:
///   NotFoundException       → 404
///   ValidationException     → 422
///   ConflictException       → 409
///   UnauthorizedException   → 401
///   DomainException         → 400
///   DbUpdateConcurrencyException → 409 (room race condition retry signal)
///   Exception               → 500
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            await HandleAsync(ctx, ex);
        }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var (status, title, code) = ex switch
        {
            NotFoundException      nfe => (HttpStatusCode.NotFound,
                                          nfe.Message, nfe.Code),

            ValidationException    vex => (HttpStatusCode.UnprocessableEntity,
                                          vex.Message, vex.Code),

            ConflictException      cex => (HttpStatusCode.Conflict,
                                          cex.Message, cex.Code),

            UnauthorizedException  uex => (HttpStatusCode.Unauthorized,
                                          uex.Message, uex.Code),

            DomainException        dex => (HttpStatusCode.BadRequest,
                                          dex.Message, dex.Code),

            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException
                                       => (HttpStatusCode.Conflict,
                                          "A concurrent modification conflict occurred. Please retry.",
                                          "CONCURRENCY_CONFLICT"),

            UnauthorizedAccessException uae
                                       => (HttpStatusCode.Unauthorized,
                                          "Unauthorized.", "UNAUTHORIZED"),

            _                          => (HttpStatusCode.InternalServerError,
                                          "An unexpected error occurred.", "INTERNAL_ERROR"),
        };

        // Log 5xx errors with full stack trace; 4xx as warnings
        if ((int)status >= 500)
            logger.LogError(ex, "[HMS] Unhandled exception: {Message}", ex.Message);
        else
            logger.LogWarning("[HMS] Domain exception [{Code}]: {Message}", code, ex.Message);

        var problem = new ProblemDetails
        {
            Status   = (int)status,
            Title    = title,
            Type     = $"https://hms.api/errors/{code?.ToLowerInvariant() ?? "error"}",
            Instance = ctx.Request.Path,
        };

        // Attach validation errors if present
        if (ex is ValidationException vexInner)
            problem.Extensions["errors"] = vexInner.Errors;

        if (code is not null)
            problem.Extensions["code"] = code;

        ctx.Response.StatusCode  = (int)status;
        ctx.Response.ContentType = "application/problem+json";

        await ctx.Response.WriteAsync(
            JsonSerializer.Serialize(problem, _jsonOptions));
    }
}
