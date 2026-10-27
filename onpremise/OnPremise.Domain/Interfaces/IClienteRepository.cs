using OnPremise.Domain.Entities;

namespace OnPremise.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para la entidad Cliente.
/// </summary>
public interface IClienteRepository
{
    Task<int> CreateAsync(Cliente cliente);
    Task<(IReadOnlyList<Cliente> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? search);
    Task<Cliente?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(Cliente cliente);
    Task<bool> DeleteAsync(int id);
}
