namespace OnPremise.Domain.Exceptions;

/// <summary>
/// Se lanza cuando un recurso solicitado no existe o no está activo.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
