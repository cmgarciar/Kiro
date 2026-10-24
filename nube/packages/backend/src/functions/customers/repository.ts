import { getPool, sql } from '../../shared/db/pool.js';
import { DatabaseException } from '../../shared/errors/index.js';
import type { Customer, CustomerRow, PaginatedResult } from '../../shared/types/customer.js';

/** Normaliza una fila de SQL Server al modelo de dominio. */
const toCustomer = (row: CustomerRow): Customer => ({
  id: row.Id,
  name: row.Nombre,
  address: row.Direccion ?? null,
  phone: row.Telefono ?? null,
  isActive: Boolean(row.Activo),
  createdAt: new Date(row.FechaCreacion).toISOString(),
  updatedAt: row.FechaModificacion ? new Date(row.FechaModificacion).toISOString() : null,
});

export interface CreateInput {
  name: string;
  address: string | null;
  phone: string | null;
}

export interface UpdateInput {
  id: number;
  name: string;
  address: string | null;
  phone: string | null;
}

export class CustomerRepository {
  /** CA1, CA2, CA10: inserta y devuelve el cliente con su Id autogenerado. */
  async create(input: CreateInput): Promise<Customer> {
    try {
      const pool = await getPool();
      const result = await pool
        .request()
        .input('Nombre', sql.NVarChar(100), input.name)
        .input('Direccion', sql.NVarChar(200), input.address)
        .input('Telefono', sql.NVarChar(20), input.phone)
        .execute<CustomerRow>('dbo.usp_Clientes_Crear');
      return toCustomer(result.recordset[0]);
    } catch (error) {
      throw new DatabaseException('Error al crear el cliente.', error);
    }
  }

  async getById(id: number): Promise<Customer | null> {
    try {
      const pool = await getPool();
      const result = await pool
        .request()
        .input('Id', sql.Int, id)
        .execute<CustomerRow>('dbo.usp_Clientes_ObtenerPorId');
      const row = result.recordset[0];
      return row ? toCustomer(row) : null;
    } catch (error) {
      throw new DatabaseException('Error al obtener el cliente.', error);
    }
  }

  /**
   * CA6 + CA9: lista clientes activos paginados; si se pasa `search`
   * filtra por Nombre o Telefono. El SP devuelve dos recordsets:
   * [0] = total, [1] = pagina de resultados.
   */
  async list(
    search: string | null,
    page: number,
    pageSize: number,
  ): Promise<PaginatedResult<Customer>> {
    try {
      const pool = await getPool();
      const result = await pool
        .request()
        .input('Busqueda', sql.NVarChar(100), search)
        .input('Pagina', sql.Int, page)
        .input('Tamano', sql.Int, pageSize)
        .execute<CustomerRow>('dbo.usp_Clientes_Listar');

      const totalSet = result.recordsets[0] as unknown as Array<{ Total: number }>;
      const total = totalSet?.[0]?.Total ?? 0;
      const rows = (result.recordsets[1] as unknown as CustomerRow[]) ?? [];

      return {
        items: rows.map(toCustomer),
        page,
        pageSize,
        total,
        totalPages: Math.max(1, Math.ceil(total / pageSize)),
      };
    } catch (error) {
      throw new DatabaseException('Error al listar clientes.', error);
    }
  }

  /** CA7, CA10: actualiza campos editables y devuelve el registro. */
  async update(input: UpdateInput): Promise<Customer | null> {
    try {
      const pool = await getPool();
      const result = await pool
        .request()
        .input('Id', sql.Int, input.id)
        .input('Nombre', sql.NVarChar(100), input.name)
        .input('Direccion', sql.NVarChar(200), input.address)
        .input('Telefono', sql.NVarChar(20), input.phone)
        .execute<CustomerRow>('dbo.usp_Clientes_Actualizar');
      const row = result.recordset[0];
      return row ? toCustomer(row) : null;
    } catch (error) {
      throw new DatabaseException('Error al actualizar el cliente.', error);
    }
  }

  /** CA8, CA10: soft delete. Devuelve true si se marco algun registro. */
  async softDelete(id: number): Promise<boolean> {
    try {
      const pool = await getPool();
      const result = await pool
        .request()
        .input('Id', sql.Int, id)
        .execute<{ FilasAfectadas: number }>('dbo.usp_Clientes_Eliminar');
      return (result.recordset[0]?.FilasAfectadas ?? 0) > 0;
    } catch (error) {
      throw new DatabaseException('Error al eliminar el cliente.', error);
    }
  }
}
