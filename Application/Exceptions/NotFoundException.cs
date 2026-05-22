namespace NexCommerce.Application.Exceptions;

/// <summary>Thrown when a requested resource does not exist. Mapped to HTTP 404.</summary>
public class NotFoundException(string message) : Exception(message);
