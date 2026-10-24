using Clientes.Domain.Common;
using Clientes.Domain.Entities;

namespace Clientes.Domain.Interfaces;

/// <summary>
/// Contrato de acceso a datos para la entidad Cliente.
/// </summary>
public interface IClienteRepository
{
    Task<Cliente> CrearAsync(Cliente cliente);
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<PagedResult<Cliente>> ListarAsync(string? filtro, int pagina, int tamPagina);
    Task<Cliente?> ActualizarAsync(Cliente cliente);
    Task<bool> EliminarAsync(int id);
}
