using OnPremise.Application.DTOs;
using OnPremise.Application.Interfaces;
using OnPremise.Domain.Entities;
using OnPremise.Domain.Exceptions;
using OnPremise.Domain.Interfaces;

namespace OnPremise.Application.Services;

/// <summary>
/// Caso de uso del módulo Clientes. Orquesta validaciones de negocio y el repositorio.
/// </summary>
public class ClienteService : IClienteService
{
    private const int MAX_NOMBRE = 100;
    private const int MAX_DIRECCION = 200;
    private const int MAX_TELEFONO = 20;

    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<ClienteResponse> CreateAsync(CreateClienteRequest request)
    {
        var nombre = (request.Nombre ?? string.Empty).Trim();
        var direccion = NormalizeOptional(request.Direccion);
        var telefono = NormalizeOptional(request.Telefono);

        ValidateNombre(nombre);
        ValidateDireccion(direccion);
        ValidateTelefono(telefono);

        var entity = new Cliente
        {
            Nombre = nombre,
            Direccion = direccion,
            Telefono = telefono
        };

        var newId = await _repository.CreateAsync(entity);
        var created = await _repository.GetByIdAsync(newId)
            ?? throw new NotFoundException($"No se encontró el cliente recién creado con Id {newId}.");

        return MapToResponse(created);
    }

    public async Task<PagedResult<ClienteResponse>> GetAllAsync(int pageNumber, int pageSize, string? search)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var (items, totalCount) = await _repository.GetAllAsync(pageNumber, pageSize, NormalizeOptional(search));

        return new PagedResult<ClienteResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ClienteResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró el cliente con Id {id}.");
        return MapToResponse(entity);
    }

    public async Task<ClienteResponse> UpdateAsync(int id, UpdateClienteRequest request)
    {
        var nombre = (request.Nombre ?? string.Empty).Trim();
        var direccion = NormalizeOptional(request.Direccion);
        var telefono = NormalizeOptional(request.Telefono);

        ValidateNombre(nombre);
        ValidateDireccion(direccion);
        ValidateTelefono(telefono);

        var entity = new Cliente
        {
            Id = id,
            Nombre = nombre,
            Direccion = direccion,
            Telefono = telefono
        };

        var updated = await _repository.UpdateAsync(entity);
        if (!updated)
            throw new NotFoundException($"No se encontró el cliente con Id {id}.");

        var result = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró el cliente con Id {id}.");
        return MapToResponse(result);
    }

    public async Task DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new NotFoundException($"No se encontró el cliente con Id {id}.");
    }

    // ---- Validaciones de negocio (CA3, CA4, CA5) ----

    private static void ValidateNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ValidationException("El nombre es obligatorio.");
        if (nombre.Length > MAX_NOMBRE)
            throw new ValidationException($"El nombre no puede superar {MAX_NOMBRE} caracteres.");
    }

    private static void ValidateDireccion(string? direccion)
    {
        if (direccion is not null && direccion.Length > MAX_DIRECCION)
            throw new ValidationException($"La dirección no puede superar {MAX_DIRECCION} caracteres.");
    }

    private static void ValidateTelefono(string? telefono)
    {
        if (telefono is null) return;
        if (telefono.Length > MAX_TELEFONO)
            throw new ValidationException($"El teléfono no puede superar {MAX_TELEFONO} caracteres.");
        if (!telefono.All(char.IsDigit))
            throw new ValidationException("El teléfono solo puede contener dígitos.");
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim();
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
