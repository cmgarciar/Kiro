namespace Clientes.Domain.Common;

/// <summary>
/// Resultado paginado genérico para listados.
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int Pagina { get; set; }
    public int TamPagina { get; set; }
    public int Total { get; set; }
    public int TotalPaginas => TamPagina > 0 ? (int)Math.Ceiling((double)Total / TamPagina) : 0;
}
