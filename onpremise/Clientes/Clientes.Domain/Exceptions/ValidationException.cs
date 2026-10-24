namespace Clientes.Domain.Exceptions;

/// <summary>
/// Se lanza cuando los datos de entrada no cumplen las reglas de negocio.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
