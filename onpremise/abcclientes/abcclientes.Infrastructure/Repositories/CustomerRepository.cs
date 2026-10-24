using System.Data;
using abcclientes.Domain.Common;
using abcclientes.Domain.Entities;
using abcclientes.Domain.Exceptions;
using abcclientes.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace abcclientes.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not configured.");
    }

    public async Task<int> CreateAsync(string nombre, string? direccion, string? telefono)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_CustomerCreate", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
            cmd.Parameters.Add("@Direccion", SqlDbType.NVarChar, 200).Value = (object?)direccion ?? DBNull.Value;
            cmd.Parameters.Add("@Telefono", SqlDbType.NVarChar, 20).Value = (object?)telefono ?? DBNull.Value;
            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(CustomerRepository)}.CreateAsync: {ex.Message}", ex);
        }
    }

    public async Task<PagedResult<Customer>> GetAllAsync(string? search, int page, int pageSize)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_CustomerGetAll", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = (object?)search ?? DBNull.Value;
            cmd.Parameters.Add("@Page", SqlDbType.Int).Value = page;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
            await conn.OpenAsync();

            var customers = new List<Customer>();
            int totalCount = 0;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                customers.Add(Map(reader));

            if (await reader.NextResultAsync() && await reader.ReadAsync())
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));

            return new PagedResult<Customer>
            {
                Items = customers,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(CustomerRepository)}.GetAllAsync: {ex.Message}", ex);
        }
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_CustomerGetById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(CustomerRepository)}.GetByIdAsync: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateAsync(int id, string nombre, string? direccion, string? telefono)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_CustomerUpdate", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
            cmd.Parameters.Add("@Direccion", SqlDbType.NVarChar, 200).Value = (object?)direccion ?? DBNull.Value;
            cmd.Parameters.Add("@Telefono", SqlDbType.NVarChar, 20).Value = (object?)telefono ?? DBNull.Value;
            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(CustomerRepository)}.UpdateAsync: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_CustomerDelete", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(CustomerRepository)}.DeleteAsync: {ex.Message}", ex);
        }
    }

    private static Customer Map(SqlDataReader r) => new()
    {
        Id = r.GetInt32(r.GetOrdinal("Id")),
        Nombre = r.GetString(r.GetOrdinal("Nombre")),
        Direccion = r.IsDBNull(r.GetOrdinal("Direccion")) ? null : r.GetString(r.GetOrdinal("Direccion")),
        Telefono = r.IsDBNull(r.GetOrdinal("Telefono")) ? null : r.GetString(r.GetOrdinal("Telefono")),
        Activo = r.GetBoolean(r.GetOrdinal("Activo")),
        FechaCreacion = r.GetDateTime(r.GetOrdinal("FechaCreacion")),
        FechaModificacion = r.IsDBNull(r.GetOrdinal("FechaModificacion")) ? null : r.GetDateTime(r.GetOrdinal("FechaModificacion"))
    };
}
