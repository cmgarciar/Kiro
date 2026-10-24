namespace Clientes.Application.DTOs;

/// <summary>
/// Datos de entrada para crear un cliente.
/// </summary>
public class CrearClienteRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
}
