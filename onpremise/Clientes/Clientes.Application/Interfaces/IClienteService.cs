using Clientes.Application.DTOs;

namespace Clientes.Application.Interfaces;

/// <summary>
/// Casos de uso de la gestión de clientes.
/// </summary>
public interface IClienteService
{
    Task<ClienteResponse> CrearAsync(CrearClienteRequest request);
    Task<ClienteResponse> ObtenerPorIdAsync(int id);
    Task<ClientePagedResponse> ListarAsync(string? filtro, int pagina, int tamPagina);
    Task<ClienteResponse> ActualizarAsync(int id, ActualizarClienteRequest request);
    Task EliminarAsync(int id);
}
