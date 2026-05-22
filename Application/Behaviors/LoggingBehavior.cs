using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NexCommerce.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs the start, completion, and elapsed time
/// of every command. Helps surface slow commands during development.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int SlowCommandThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("[MediatR] Handling {RequestName}", requestName);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            sw.Stop();

            if (sw.ElapsedMilliseconds > SlowCommandThresholdMs)
                logger.LogWarning("[MediatR] Slow command detected: {RequestName} took {Elapsed}ms",
                    requestName, sw.ElapsedMilliseconds);
            else
                logger.LogInformation("[MediatR] Handled {RequestName} in {Elapsed}ms",
                    requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "[MediatR] Error handling {RequestName} after {Elapsed}ms",
                requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
