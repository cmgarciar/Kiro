namespace Clientes.Domain.Exceptions;

/// <summary>
/// Se lanza cuando un recurso solicitado no existe.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
