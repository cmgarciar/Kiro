using abcclientes.Application.DTOs;
using abcclientes.Application.Interfaces;
using abcclientes.Domain.Entities;
using abcclientes.Domain.Exceptions;
using abcclientes.Domain.Interfaces;

namespace abcclientes.Application.Services;

/// <summary>
/// Application service that orchestrates customer use cases and enforces business validations.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
    {
        ValidateNombre(request.Nombre);
        ValidateDireccion(request.Direccion);
        ValidateTelefono(request.Telefono);

        var id = await _repository.CreateAsync(request.Nombre.Trim(), request.Direccion?.Trim(), request.Telefono?.Trim());

        var customer = await _repository.GetByIdAsync(id)
            ?? throw new DatabaseException("Failed to retrieve the newly created customer.");

        return MapToResponse(customer);
    }

    public async Task<CustomerPagedResponse> GetAllAsync(string? search, int page, int pageSize)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        var result = await _repository.GetAllAsync(search?.Trim(), page, pageSize);

        return new CustomerPagedResponse
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }

    public async Task<CustomerResponse> GetByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Customer with Id {id} was not found.");

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        ValidateNombre(request.Nombre);
        ValidateDireccion(request.Direccion);
        ValidateTelefono(request.Telefono);

        var updated = await _repository.UpdateAsync(id, request.Nombre.Trim(), request.Direccion?.Trim(), request.Telefono?.Trim());

        if (!updated)
            throw new NotFoundException($"Customer with Id {id} was not found.");

        var customer = await _repository.GetByIdAsync(id)
            ?? throw new DatabaseException("Failed to retrieve the updated customer.");

        return MapToResponse(customer);
    }

    public async Task DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
            throw new NotFoundException($"Customer with Id {id} was not found.");
    }

    private static void ValidateNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ValidationException("El campo Nombre es obligatorio.");

        if (nombre.Length > 100)
            throw new ValidationException("El campo Nombre no puede superar 100 caracteres.");
    }

    private static void ValidateDireccion(string? direccion)
    {
        if (direccion is not null && direccion.Length > 200)
            throw new ValidationException("El campo Direccion no puede superar 200 caracteres.");
    }

    private static void ValidateTelefono(string? telefono)
    {
        if (string.IsNullOrEmpty(telefono))
            return;

        if (telefono.Length > 20)
            throw new ValidationException("El campo Telefono no puede superar 20 caracteres.");

        if (!telefono.All(char.IsDigit))
            throw new ValidationException("El campo Telefono solo debe contener digitos.");
    }

    private static CustomerResponse MapToResponse(Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            Nombre = customer.Nombre,
            Direccion = customer.Direccion,
            Telefono = customer.Telefono,
            Activo = customer.Activo,
            FechaCreacion = customer.FechaCreacion,
            FechaModificacion = customer.FechaModificacion
        };
    }
}
