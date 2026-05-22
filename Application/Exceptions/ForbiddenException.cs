namespace NexCommerce.Application.Exceptions;

/// <summary>Thrown when an authenticated user lacks permission. Mapped to HTTP 403.</summary>
public class ForbiddenException(string message) : Exception(message);
