using abcclientes.Domain.Common;
using abcclientes.Domain.Entities;

namespace abcclientes.Domain.Interfaces;

public interface ICustomerRepository
{
    Task<int> CreateAsync(string nombre, string? direccion, string? telefono);
    Task<PagedResult<Customer>> GetAllAsync(string? search, int page, int pageSize);
    Task<Customer?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, string nombre, string? direccion, string? telefono);
    Task<bool> DeleteAsync(int id);
}
