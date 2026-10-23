using abcclientes.Application.DTOs;
using System.Net.Http.Json;

namespace abcclientes.WinForms.Services;

/// <summary>
/// HTTP service that communicates with the Customers API endpoints.
/// </summary>
public class CustomerApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5163/api/v1/customers";

    public CustomerApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CustomerPagedResponse> GetAllAsync(string? search, int page, int pageSize)
    {
        var query = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        var response = await _httpClient.GetAsync(BaseUrl + query);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CustomerPagedResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize customer list.");
    }

    public async Task<CustomerResponse> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CustomerResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize customer.");
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CustomerResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize created customer.");
    }

    public async Task<CustomerResponse> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CustomerResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize updated customer.");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
        response.EnsureSuccessStatusCode();
    }
}
