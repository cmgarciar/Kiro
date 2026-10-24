namespace Clientes.Domain.Exceptions;

/// <summary>
/// Envuelve errores de acceso a datos (SQL).
/// </summary>
public class DatabaseException : Exception
{
    public DatabaseException(string message, Exception innerException)
        : base(message, innerException) { }
}
