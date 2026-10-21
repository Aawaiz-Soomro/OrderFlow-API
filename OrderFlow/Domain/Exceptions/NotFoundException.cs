namespace OrderFlow.Domain.Exceptions;

// Thrown when a requested resource does not exist. Returned as HTTP 404.
public class NotFoundException(string message) : Exception(message);
