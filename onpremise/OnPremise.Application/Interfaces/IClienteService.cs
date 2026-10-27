using OnPremise.Application.DTOs;

namespace OnPremise.Application.Interfaces;

public interface IClienteService
{
    Task<ClienteResponse> CreateAsync(CreateClienteRequest request);
    Task<PagedResult<ClienteResponse>> GetAllAsync(int pageNumber, int pageSize, string? search);
    Task<ClienteResponse> GetByIdAsync(int id);
    Task<ClienteResponse> UpdateAsync(int id, UpdateClienteRequest request);
    Task DeleteAsync(int id);
}
