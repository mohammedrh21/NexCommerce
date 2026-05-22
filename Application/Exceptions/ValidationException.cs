namespace NexCommerce.Application.Exceptions;

/// <summary>Thrown when a business rule or input validation fails. Mapped to HTTP 400.</summary>
public class ValidationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = [message];
    }

    public ValidationException(IEnumerable<string> errors) : base(string.Join("; ", errors))
    {
        Errors = errors;
    }
}
