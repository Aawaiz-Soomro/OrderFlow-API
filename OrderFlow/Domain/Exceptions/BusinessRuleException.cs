namespace OrderFlow.Domain.Exceptions;

// Thrown when a request breaks a business rule (e.g. insufficient stock). Returned as HTTP 400.
public class BusinessRuleException(string message) : Exception(message);
