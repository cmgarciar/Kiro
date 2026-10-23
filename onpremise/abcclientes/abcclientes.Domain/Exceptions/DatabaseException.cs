namespace abcclientes.Domain.Exceptions;

/// <summary>
/// Thrown when a database operation fails.
/// Wraps the original SqlException to avoid leaking infrastructure details.
/// </summary>
public class DatabaseException : Exception
{
    public DatabaseException(string message) : base(message)
    {
    }

    public DatabaseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
