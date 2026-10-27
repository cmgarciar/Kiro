using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OnPremise.WinForms.Models;

namespace OnPremise.WinForms.Services;

/// <summary>
/// Cliente HTTP del módulo Clientes. Encapsula todas las llamadas REST a la API.
/// </summary>
public class ClienteApiService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ClienteApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PagedResult<ClienteDto>> GetAllAsync(int pageNumber, int pageSize, string? search)
    {
        var url = $"api/v1/clientes?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var response = await _http.GetAsync(url);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<PagedResult<ClienteDto>>(JsonOpts)
               ?? new PagedResult<ClienteDto>();
    }

    public async Task<ClienteDto?> CreateAsync(CreateClienteRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/v1/clientes", request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<ClienteDto>(JsonOpts);
    }

    public async Task<ClienteDto?> UpdateAsync(int id, UpdateClienteRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/clientes/{id}", request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<ClienteDto>(JsonOpts);
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/clientes/{id}");
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        string message = $"Error {(int)response.StatusCode}";
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(JsonOpts);
            if (!string.IsNullOrWhiteSpace(error?.Message))
                message = error!.Message!;
        }
        catch
        {
            // Respuesta sin cuerpo JSON; se conserva el mensaje por código.
        }

        throw new ApiException(message, response.StatusCode);
    }
}

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public ApiException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
