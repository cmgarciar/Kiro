using System.Data;
using abcclientes.Domain.Common;
using abcclientes.Domain.Entities;
using abcclientes.Domain.Exceptions;
using abcclientes.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace abcclientes.Infrastructure.Repositories;

/// <summary>
/// ADO.NET implementation of the Customer repository using stored procedures.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public async Task<int> CreateAsync(string nombre, string? direccion, string? telefono)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CustomerCreate", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
            command.Parameters.Add("@Direccion", SqlDbType.NVarChar, 200).Value = (object?)direccion ?? DBNull.Value;
            command.Parameters.Add("@Telefono", SqlDbType.NVarChar, 20).Value = (object?)telefono ?? DBNull.Value;

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(CustomerRepository)}: {ex.Message}", ex);
        }
    }

    public async Task<PagedResult<Customer>> GetAllAsync(string? search, int page, int pageSize)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CustomerGetAll", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = (object?)search ?? DBNull.Value;
            command.Parameters.Add("@Page", SqlDbType.Int).Value = page;
            command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            await connection.OpenAsync();

            var customers = new List<Customer>();
            int totalCount = 0;

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    customers.Add(MapReaderToCustomer(reader));
                }

                // The SP returns two result sets: the paged rows and the total count.
                if (await reader.NextResultAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                    }
                }
            }

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
            throw new DatabaseException($"Database error in {nameof(CustomerRepository)}: {ex.Message}", ex);
        }
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CustomerGetById", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToCustomer(reader);
            }

            return null;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(CustomerRepository)}: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateAsync(int id, string nombre, string? direccion, string? telefono)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CustomerUpdate", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            command.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
            command.Parameters.Add("@Direccion", SqlDbType.NVarChar, 200).Value = (object?)direccion ?? DBNull.Value;
            command.Parameters.Add("@Telefono", SqlDbType.NVarChar, 20).Value = (object?)telefono ?? DBNull.Value;

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(CustomerRepository)}: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CustomerDelete", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Database error in {nameof(CustomerRepository)}: {ex.Message}", ex);
        }
    }

    private static Customer MapReaderToCustomer(SqlDataReader reader)
    {
        return new Customer
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
}
