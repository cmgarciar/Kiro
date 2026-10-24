namespace Clientes.Application.DTOs;

/// <summary>
/// Datos de entrada para actualizar un cliente existente.
/// </summary>
public class ActualizarClienteRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
}
