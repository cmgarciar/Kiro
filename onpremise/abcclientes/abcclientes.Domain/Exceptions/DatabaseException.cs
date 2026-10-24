namespace abcclientes.Domain.Exceptions;

public class DatabaseException : Exception
{
    public DatabaseException(string message, Exception? inner = null) : base(message, inner) { }
}
