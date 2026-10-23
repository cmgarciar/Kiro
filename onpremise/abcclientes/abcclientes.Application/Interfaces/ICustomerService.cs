using abcclientes.Application.DTOs;

namespace abcclientes.Application.Interfaces;

/// <summary>
/// Application service interface for customer use cases.
/// </summary>
public interface ICustomerService
{
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request);

    Task<CustomerPagedResponse> GetAllAsync(string? search, int page, int pageSize);

    Task<CustomerResponse> GetByIdAsync(int id);

    Task<CustomerResponse> UpdateAsync(int id, UpdateCustomerRequest request);

    Task DeleteAsync(int id);
}
