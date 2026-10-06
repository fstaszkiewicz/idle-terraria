using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdleTerraria.Api.Infrastructure.Errors;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException &&
            httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug(
                "Żądanie zostało anulowane przez klienta. TraceId: {TraceId}",
                httpContext.TraceIdentifier);

            return true;
        }

        var (statusCode, title, type, detail, logLevel) =
            MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.Log(
                logLevel,
                exception,
                "Nieobsłużony wyjątek. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.Log(
                logLevel,
                "Obsłużony wyjątek domenowy. Status: {StatusCode}, " +
                "TraceId: {TraceId}, ExceptionType: {ExceptionType}",
                statusCode,
                httpContext.TraceIdentifier,
                exception.GetType().Name);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        await _problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });

        return true;
    }

    private static ExceptionMapping MapException(
        Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException =>
                new ExceptionMapping(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "https://httpstatuses.com/401",
                    "Żądanie wymaga poprawnego uwierzytelnienia.",
                    LogLevel.Information),

            KeyNotFoundException =>
                new ExceptionMapping(
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    "https://httpstatuses.com/404",
                    "Żądany zasób nie został znaleziony.",
                    LogLevel.Information),

            InvalidOperationException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    "Invalid operation",
                    "https://httpstatuses.com/400",
                    "Operacja nie może zostać wykonana dla aktualnego stanu zasobu.",
                    LogLevel.Information),

            ArgumentException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    "Invalid argument",
                    "https://httpstatuses.com/400",
                    "Żądanie zawiera nieprawidłowe dane.",
                    LogLevel.Information),

            DbUpdateConcurrencyException =>
                new ExceptionMapping(
                    StatusCodes.Status409Conflict,
                    "Concurrency conflict",
                    "https://httpstatuses.com/409",
                    "Dane zostały zmienione przez inne żądanie. Spróbuj ponownie.",
                    LogLevel.Warning),

            DbUpdateException =>
                new ExceptionMapping(
                    StatusCodes.Status409Conflict,
                    "Database update failed",
                    "https://httpstatuses.com/409",
                    "Nie udało się zapisać zmian z powodu konfliktu danych.",
                    LogLevel.Error),

            _ =>
                new ExceptionMapping(
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "https://httpstatuses.com/500",
                    "Wystąpił nieoczekiwany błąd serwera.",
                    LogLevel.Error)
        };
    }

    private sealed record ExceptionMapping(
        int StatusCode,
        string Title,
        string Type,
        string Detail,
        LogLevel LogLevel);
}