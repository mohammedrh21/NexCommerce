using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Exceptions;
using ValidationException = NexCommerce.Application.Exceptions.ValidationException;

namespace NexCommerce.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs all registered FluentValidation validators
/// before the handler executes. Throws <see cref="ValidationException"/> on failure.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures.Select(f => f.ErrorMessage).ToList();
            logger.LogWarning("Validation failed for {RequestType}: {Errors}",
                typeof(TRequest).Name, string.Join("; ", errors));
            throw new ValidationException(errors);
        }

        return await next(cancellationToken);
    }
}
