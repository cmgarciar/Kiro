using abcclientes.Application.DTOs;
using System.Net.Http.Json;

namespace abcclientes.WinForms.Services;

public class CustomerApiService
{
    private readonly HttpClient _http;
    private const string Base = "http://localhost:5163/api/v1/customers";

    public CustomerApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<CustomerPagedResponse> GetAllAsync(string? search, int page, int pageSize)
    {
        var query = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        var response = await _http.GetAsync(Base + query);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerPagedResponse>() ?? new();
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
    {
        var response = await _http.PostAsJsonAsync(Base, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerResponse>() ?? new();
    }

    public async Task<CustomerResponse> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var response = await _http.PutAsJsonAsync($"{Base}/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerResponse>() ?? new();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{Base}/{id}");
        response.EnsureSuccessStatusCode();
    }
}
