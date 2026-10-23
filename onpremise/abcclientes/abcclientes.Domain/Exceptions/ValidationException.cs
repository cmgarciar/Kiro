namespace abcclientes.Domain.Exceptions;

/// <summary>
/// Thrown when a business validation rule is violated.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
