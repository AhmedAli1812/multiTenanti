using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HMS.SharedKernel.Application.Behaviors;

/// <summary>
/// Logs every MediatR request with execution time.
/// Warns when a handler exceeds the performance threshold (500 ms by default).
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowHandlerThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("[HMS] Handling {RequestName}", requestName);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();

            if (sw.ElapsedMilliseconds > SlowHandlerThresholdMs)
                logger.LogWarning(
                    "[HMS] SLOW handler {RequestName} completed in {ElapsedMs} ms",
                    requestName, sw.ElapsedMilliseconds);
            else
                logger.LogInformation(
                    "[HMS] Handled {RequestName} in {ElapsedMs} ms",
                    requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex,
                "[HMS] Error handling {RequestName} after {ElapsedMs} ms",
                requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
