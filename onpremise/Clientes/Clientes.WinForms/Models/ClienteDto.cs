namespace Clientes.WinForms.Models;

/// <summary>DTO de cliente usado por el cliente WinForms.</summary>
public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>Datos para crear/actualizar un cliente.</summary>
public class ClienteInput
{
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
}

/// <summary>Respuesta paginada de clientes.</summary>
public class ClientePagedDto
{
    public List<ClienteDto> Items { get; set; } = new();
    public int Pagina { get; set; }
    public int TamPagina { get; set; }
    public int Total { get; set; }
    public int TotalPaginas { get; set; }
}
