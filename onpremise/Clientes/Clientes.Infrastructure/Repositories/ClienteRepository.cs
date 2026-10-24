using System.Data;
using Clientes.Domain.Common;
using Clientes.Domain.Entities;
using Clientes.Domain.Exceptions;
using Clientes.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Clientes.Infrastructure.Repositories;

/// <summary>
/// Implementación de IClienteRepository con ADO.NET + Stored Procedures.
/// </summary>
public class ClienteRepository : IClienteRepository
{
    private readonly string _connectionString;

    public ClienteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Cliente> CrearAsync(Cliente cliente)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClienteCrear", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
            command.Parameters.AddWithValue("@Direccion", (object?)cliente.Direccion ?? DBNull.Value);
            command.Parameters.AddWithValue("@Telefono", (object?)cliente.Telefono ?? DBNull.Value);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return Map(reader);

            throw new DatabaseException("No se pudo crear el cliente.", new InvalidOperationException("El procedimiento no devolvió el registro creado."));
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(ClienteRepository)}.{nameof(CrearAsync)}: {ex.Message}", ex);
        }
    }

    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClienteObtenerPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return Map(reader);

            return null;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(ClienteRepository)}.{nameof(ObtenerPorIdAsync)}: {ex.Message}", ex);
        }
    }

    public async Task<PagedResult<Cliente>> ListarAsync(string? filtro, int pagina, int tamPagina)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClienteListar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Filtro", (object?)filtro ?? DBNull.Value);
            command.Parameters.AddWithValue("@Pagina", pagina);
            command.Parameters.AddWithValue("@TamPagina", tamPagina);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            var items = new List<Cliente>();
            while (await reader.ReadAsync())
                items.Add(Map(reader));

            var total = 0;
            if (await reader.NextResultAsync() && await reader.ReadAsync())
                total = reader.GetInt32(reader.GetOrdinal("Total"));

            return new PagedResult<Cliente>
            {
                Items = items,
                Pagina = pagina,
                TamPagina = tamPagina,
                Total = total
            };
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(ClienteRepository)}.{nameof(ListarAsync)}: {ex.Message}", ex);
        }
    }

    public async Task<Cliente?> ActualizarAsync(Cliente cliente)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClienteActualizar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Id", cliente.Id);
            command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
            command.Parameters.AddWithValue("@Direccion", (object?)cliente.Direccion ?? DBNull.Value);
            command.Parameters.AddWithValue("@Telefono", (object?)cliente.Telefono ?? DBNull.Value);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var actualizado = Map(reader);
                // Si quedó inactivo o no existe, se considera no encontrado.
                return actualizado.Activo ? actualizado : null;
            }

            return null;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(ClienteRepository)}.{nameof(ActualizarAsync)}: {ex.Message}", ex);
        }
    }

    public async Task<bool> EliminarAsync(int id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClienteEliminar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            var filas = (int)(await command.ExecuteScalarAsync() ?? 0);
            return filas > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(ClienteRepository)}.{nameof(EliminarAsync)}: {ex.Message}", ex);
        }
    }

    private static Cliente Map(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString(reader.GetOrdinal("Direccion")),
        Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString(reader.GetOrdinal("Telefono")),
        Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
        FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
        FechaModificacion = reader.IsDBNull(reader.GetOrdinal("FechaModificacion")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaModificacion"))
    };
}
