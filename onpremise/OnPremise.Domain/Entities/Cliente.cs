namespace OnPremise.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un cliente (POCO, sin dependencias externas).
/// </summary>
public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
