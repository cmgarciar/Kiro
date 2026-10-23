namespace abcclientes.Application.DTOs;

/// <summary>
/// Response DTO representing a customer in API responses and WinForms display.
/// </summary>
public class CustomerResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }
}
