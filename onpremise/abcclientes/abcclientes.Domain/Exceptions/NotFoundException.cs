namespace abcclientes.Domain.Exceptions;

/// <summary>
/// Thrown when an entity is not found in the database.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
