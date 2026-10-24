using System.Text.RegularExpressions;
using Clientes.Application.DTOs;
using Clientes.Application.Interfaces;
using Clientes.Domain.Entities;
using Clientes.Domain.Exceptions;
using Clientes.Domain.Interfaces;

namespace Clientes.Application.Services;

/// <summary>
/// Implementación de los casos de uso de clientes. Contiene las validaciones de negocio.
/// </summary>
public class ClienteService : IClienteService
{
    private const int NOMBRE_MAX_LENGTH = 100;
    private const int DIRECCION_MAX_LENGTH = 200;
    private const int TELEFONO_MAX_LENGTH = 20;

    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<ClienteResponse> CrearAsync(CrearClienteRequest request)
    {
        var nombre = (request.Nombre ?? string.Empty).Trim();
        var direccion = NormalizarOpcional(request.Direccion);
        var telefono = NormalizarOpcional(request.Telefono);

        Validar(nombre, direccion, telefono);

        var creado = await _repository.CrearAsync(new Cliente
        {
            Nombre = nombre,
            Direccion = direccion,
            Telefono = telefono
        });

        return MapToResponse(creado);
    }

    public async Task<ClienteResponse> ObtenerPorIdAsync(int id)
    {
        var cliente = await _repository.ObtenerPorIdAsync(id);
        if (cliente is null)
            throw new NotFoundException($"No se encontró el cliente con Id {id}.");

        return MapToResponse(cliente);
    }

    public async Task<ClientePagedResponse> ListarAsync(string? filtro, int pagina, int tamPagina)
    {
        if (pagina < 1) pagina = 1;
        if (tamPagina < 1) tamPagina = 10;

        var filtroNormalizado = NormalizarOpcional(filtro);
        var resultado = await _repository.ListarAsync(filtroNormalizado, pagina, tamPagina);

        return new ClientePagedResponse
        {
            Items = resultado.Items.Select(MapToResponse).ToList(),
            Pagina = resultado.Pagina,
            TamPagina = resultado.TamPagina,
            Total = resultado.Total,
            TotalPaginas = resultado.TotalPaginas
        };
    }

    public async Task<ClienteResponse> ActualizarAsync(int id, ActualizarClienteRequest request)
    {
        var existente = await _repository.ObtenerPorIdAsync(id);
        if (existente is null)
            throw new NotFoundException($"No se encontró el cliente con Id {id}.");

        var nombre = (request.Nombre ?? string.Empty).Trim();
        var direccion = NormalizarOpcional(request.Direccion);
        var telefono = NormalizarOpcional(request.Telefono);

        Validar(nombre, direccion, telefono);

        var actualizado = await _repository.ActualizarAsync(new Cliente
        {
            Id = id,
            Nombre = nombre,
            Direccion = direccion,
            Telefono = telefono
        });

        if (actualizado is null)
            throw new NotFoundException($"No se encontró el cliente con Id {id}.");

        return MapToResponse(actualizado);
    }

    public async Task EliminarAsync(int id)
    {
        var eliminado = await _repository.EliminarAsync(id);
        if (!eliminado)
            throw new NotFoundException($"No se encontró el cliente con Id {id}.");
    }

    // ---------------------------------------------------------
    // Validaciones de negocio (CA3, CA4, CA5)
    // ---------------------------------------------------------
    private static void Validar(string nombre, string? direccion, string? telefono)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ValidationException("El nombre es obligatorio.");

        if (nombre.Length > NOMBRE_MAX_LENGTH)
            throw new ValidationException($"El nombre no puede superar los {NOMBRE_MAX_LENGTH} caracteres.");

        if (direccion is not null && direccion.Length > DIRECCION_MAX_LENGTH)
            throw new ValidationException($"La dirección no puede superar los {DIRECCION_MAX_LENGTH} caracteres.");

        if (telefono is not null)
        {
            if (telefono.Length > TELEFONO_MAX_LENGTH)
                throw new ValidationException($"El teléfono no puede superar los {TELEFONO_MAX_LENGTH} caracteres.");

            if (!Regex.IsMatch(telefono, @"^\d+$"))
                throw new ValidationException("El teléfono solo puede contener dígitos.");
        }
    }

    private static string? NormalizarOpcional(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        return valor.Trim();
    }

    private static ClienteResponse MapToResponse(Cliente c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Direccion = c.Direccion,
        Telefono = c.Telefono,
        Activo = c.Activo,
        FechaCreacion = c.FechaCreacion,
        FechaModificacion = c.FechaModificacion
    };
}
