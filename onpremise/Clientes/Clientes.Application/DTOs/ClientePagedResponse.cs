namespace Clientes.Application.DTOs;

/// <summary>
/// Respuesta paginada de clientes.
/// </summary>
public class ClientePagedResponse
{
    public IReadOnlyList<ClienteResponse> Items { get; set; } = new List<ClienteResponse>();
    public int Pagina { get; set; }
    public int TamPagina { get; set; }
    public int Total { get; set; }
    public int TotalPaginas { get; set; }
}
