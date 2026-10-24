using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Clientes.WinForms.Models;

namespace Clientes.WinForms.Services;

/// <summary>
/// Servicio HTTP que consume la API REST de clientes.
/// </summary>
public class ClienteApiService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ClienteApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ClientePagedDto> ListarAsync(string? filtro, int pagina, int tamPagina)
    {
        var url = $"api/v1/clientes?pagina={pagina}&tamPagina={tamPagina}";
        if (!string.IsNullOrWhiteSpace(filtro))
            url += $"&filtro={Uri.EscapeDataString(filtro)}";

        var response = await _http.GetAsync(url);
        await AsegurarExito(response);
        return (await response.Content.ReadFromJsonAsync<ClientePagedDto>(JsonOptions))!;
    }

    public async Task<ClienteDto> CrearAsync(ClienteInput input)
    {
        var response = await _http.PostAsJsonAsync("api/v1/clientes", input);
        await AsegurarExito(response);
        return (await response.Content.ReadFromJsonAsync<ClienteDto>(JsonOptions))!;
    }

    public async Task<ClienteDto> ActualizarAsync(int id, ClienteInput input)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/clientes/{id}", input);
        await AsegurarExito(response);
        return (await response.Content.ReadFromJsonAsync<ClienteDto>(JsonOptions))!;
    }

    public async Task EliminarAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/clientes/{id}");
        await AsegurarExito(response);
    }

    /// <summary>
    /// Convierte respuestas de error en una excepción con el mensaje amigable devuelto por la API.
    /// </summary>
    private static async Task AsegurarExito(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        string mensaje = $"Error {(int)response.StatusCode}.";
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(body))
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    mensaje = m.GetString() ?? mensaje;
            }
        }
        catch
        {
            // Si el cuerpo no es JSON, se conserva el mensaje por código de estado.
        }

        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            mensaje = "No se pudo conectar con el servidor de la API.";

        throw new ApplicationException(mensaje);
    }
}
