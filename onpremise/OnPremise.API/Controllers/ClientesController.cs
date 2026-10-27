using Microsoft.AspNetCore.Mvc;
using OnPremise.Application.DTOs;
using OnPremise.Application.Interfaces;

namespace OnPremise.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service)
    {
        _service = service;
    }

    /// <summary>Lista clientes activos paginados. Permite filtrar por nombre o teléfono (CA6, CA9).</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ClienteResponse>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, search);
        return Ok(result);
    }

    /// <summary>Obtiene un cliente activo por su Id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Crea un nuevo cliente (CA1, CA2, CA10).</summary>
    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Create([FromBody] CreateClienteRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Actualiza los datos de un cliente existente (CA7, CA10).</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> Update(int id, [FromBody] UpdateClienteRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        return Ok(updated);
    }

    /// <summary>Elimina (soft delete) un cliente (CA8).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
