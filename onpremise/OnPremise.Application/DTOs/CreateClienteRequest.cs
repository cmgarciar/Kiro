namespace OnPremise.Application.DTOs;

public class CreateClienteRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
}
