using System.Data;
using Microsoft.Data.SqlClient;
using OnPremise.Domain.Entities;
using OnPremise.Domain.Exceptions;
using OnPremise.Domain.Interfaces;

namespace OnPremise.Infrastructure.Persistence;

/// <summary>
/// Implementación de IClienteRepository con ADO.NET puro sobre stored procedures.
/// Todas las consultas son parametrizadas.
/// </summary>
public class ClienteRepository : IClienteRepository
{
    private readonly string _connectionString;

    public ClienteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> CreateAsync(Cliente cliente)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClienteCreate", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 100) { Value = cliente.Nombre });
            command.Parameters.Add(new SqlParameter("@Direccion", SqlDbType.NVarChar, 200) { Value = (object?)cliente.Direccion ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.NVarChar, 20) { Value = (object?)cliente.Telefono ?? DBNull.Value });
            var outId = new SqlParameter("@NewId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(outId);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)outId.Value;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(ClienteRepository)}.{nameof(CreateAsync)}: {ex.Message}", ex);
        }
    }

    public async Task<(IReadOnlyList<Cliente> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? search)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClienteGetAll", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
            command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
            command.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar, 100) { Value = (object?)search ?? DBNull.Value });

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            // Primer result set: total count.
            int totalCount = 0;
            if (await reader.ReadAsync())
                totalCount = reader.GetInt32(0);

            // Segundo result set: página de registros.
            var items = new List<Cliente>();
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                    items.Add(Map(reader));
            }

            return (items, totalCount);
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(ClienteRepository)}.{nameof(GetAllAsync)}: {ex.Message}", ex);
        }
    }

    public async Task<Cliente?> GetByIdAsync(int id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClienteGetById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return Map(reader);

            return null;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(ClienteRepository)}.{nameof(GetByIdAsync)}: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateAsync(Cliente cliente)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClienteUpdate", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = cliente.Id });
            command.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 100) { Value = cliente.Nombre });
            command.Parameters.Add(new SqlParameter("@Direccion", SqlDbType.NVarChar, 200) { Value = (object?)cliente.Direccion ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.NVarChar, 20) { Value = (object?)cliente.Telefono ?? DBNull.Value });
            var outRows = new SqlParameter("@Rows", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(outRows);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)outRows.Value > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(ClienteRepository)}.{nameof(UpdateAsync)}: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ClienteDelete", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            var outRows = new SqlParameter("@Rows", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(outRows);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)outRows.Value > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(ClienteRepository)}.{nameof(DeleteAsync)}: {ex.Message}", ex);
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
