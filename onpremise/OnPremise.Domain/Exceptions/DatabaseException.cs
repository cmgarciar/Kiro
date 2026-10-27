namespace OnPremise.Domain.Exceptions;

/// <summary>
/// Envuelve errores de acceso a datos (SqlException) para aislar la capa de infraestructura.
/// </summary>
public class DatabaseException : Exception
{
    public DatabaseException(string message, Exception inner) : base(message, inner) { }
}
