import mssql from 'mssql';
import { getPool } from '../../shared/db/pool.js';
import { DatabaseException } from '../../shared/errors/index.js';
import type {
  __Entity__,
  Create__Entity__Request,
  Update__Entity__Request,
  PagedResult,
} from './types.js';

// El repositorio SIEMPRE llama a stored procedures (nunca SQL inline) y envuelve
// los errores de SQL en DatabaseException para que el service/handler los traduzcan
// a códigos HTTP correctos.
//
// Nota de tipos: el 2º argumento de request.input() es el tipo SQL de la columna
// (mssql.NVarChar, mssql.Int, ...), NO mssql.TYPES.

const mapRow = (row: Record<string, unknown>): __Entity__ => ({
  id: row.Id as number,
  nombre: row.Nombre as string,
  descripcion: (row.Descripcion as string | null) ?? null,
  activo: row.Activo as boolean,
  fechaCreacion: (row.FechaCreacion as Date).toISOString(),
  fechaModificacion: row.FechaModificacion
    ? (row.FechaModificacion as Date).toISOString()
    : null,
});

export class __Entity__Repository {
  async list(
    pagina: number,
    tamPagina: number,
    busqueda?: string
  ): Promise<PagedResult<__Entity__>> {
    try {
      const pool = await getPool();
      const result = await pool
        .request()
        .input('Pagina', mssql.Int, pagina)
        .input('TamPagina', mssql.Int, tamPagina)
        .input('Busqueda', mssql.NVarChar(100), busqueda ?? null)
        .execute<Record<string, unknown>>('usp___Entity__s_Listar');

      const recordsets = result.recordsets as mssql.IRecordSet<Record<string, unknown>>[];
      const items = recordsets[0].map(mapRow);
      const total = (recordsets[1]?.[0] as { Total: number } | undefined)?.Total ?? 0;
      return { items, total, pagina, tamPagina };
    } catch (error) {
      throw new DatabaseException('Error al listar los registros.', error);
    }
  }

  async getById(id: number): Promise<__Entity__ | null> {
    try {
      const pool = await getPool();
      const result = await pool
        .request()
        .input('Id', mssql.Int, id)
        .execute('usp___Entity__s_ObtenerPorId');
      const row = result.recordset[0];
      return row ? mapRow(row) : null;
    } catch (error) {
      throw new DatabaseException('Error al obtener el registro.', error);
    }
  }

  async create(input: Create__Entity__Request): Promise<__Entity__> {
    try {
      const pool = await getPool();
      const result = await pool
        .request()
        .input('Nombre', mssql.NVarChar(100), input.nombre)
        .input('Descripcion', mssql.NVarChar(200), input.descripcion ?? null)
        .execute('usp___Entity__s_Crear');
      return mapRow(result.recordset[0]);
    } catch (error) {
      throw new DatabaseException('Error al crear el registro.', error);
    }
  }

  async update(id: number, input: Update__Entity__Request): Promise<__Entity__ | null> {
    try {
      const pool = await getPool();
      const result = await pool
        .request()
        .input('Id', mssql.Int, id)
        .input('Nombre', mssql.NVarChar(100), input.nombre)
        .input('Descripcion', mssql.NVarChar(200), input.descripcion ?? null)
        .execute('usp___Entity__s_Actualizar');
      const row = result.recordset[0];
      return row ? mapRow(row) : null;
    } catch (error) {
      throw new DatabaseException('Error al actualizar el registro.', error);
    }
  }

  async remove(id: number): Promise<void> {
    try {
      const pool = await getPool();
      await pool.request().input('Id', mssql.Int, id).execute('usp___Entity__s_Eliminar');
    } catch (error) {
      throw new DatabaseException('Error al eliminar el registro.', error);
    }
  }
}
