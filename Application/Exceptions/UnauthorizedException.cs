namespace NexCommerce.Application.Exceptions;

/// <summary>Thrown when a user is not authenticated. Mapped to HTTP 401.</summary>
public class UnauthorizedException(string message) : Exception(message);
