using Clientes.Application.DTOs;
using Clientes.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clientes.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service)
    {
        _service = service;
    }

    /// <summary>Lista/busca clientes activos de forma paginada por Nombre o Teléfono.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ClientePagedResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClientePagedResponse>> Listar(
        [FromQuery] string? filtro,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamPagina = 10)
    {
        var resultado = await _service.ListarAsync(filtro, pagina, tamPagina);
        return Ok(resultado);
    }

    /// <summary>Obtiene un cliente activo por su Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> ObtenerPorId(int id)
    {
        var cliente = await _service.ObtenerPorIdAsync(id);
        return Ok(cliente);
    }

    /// <summary>Crea un nuevo cliente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteResponse>> Crear([FromBody] CrearClienteRequest request)
    {
        var creado = await _service.CrearAsync(request);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    /// <summary>Actualiza Nombre, Dirección y Teléfono de un cliente existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> Actualizar(int id, [FromBody] ActualizarClienteRequest request)
    {
        var actualizado = await _service.ActualizarAsync(id, request);
        return Ok(actualizado);
    }

    /// <summary>Elimina (soft delete) un cliente.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
