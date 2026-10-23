namespace abcclientes.Domain.Entities;

/// <summary>
/// Represents a customer entity in the system.
/// </summary>
public class Customer
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }
}
