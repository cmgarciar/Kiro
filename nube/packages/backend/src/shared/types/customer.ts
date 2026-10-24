/**
 * Logical model for a Customer (HU1 - Modulo Clientes).
 * Persistence: SQL Server on-premise (tabla dbo.Clientes).
 * Field names in English in code; DB columns and UI labels are in Spanish.
 */
export interface Customer {
  /** CA1/CA2: unique, auto-generated (INT IDENTITY), non-editable PK. */
  id: number;
  /** CA3: required, max 100 chars. */
  name: string;
  /** CA4: optional, max 200 chars. */
  address: string | null;
  /** CA5: optional, digits only, max 20 chars. */
  phone: string | null;
  /** CA8: soft delete flag. Default true. */
  isActive: boolean;
  /** CA10: creation timestamp (ISO 8601). */
  createdAt: string;
  /** CA10: last modification timestamp (ISO 8601). */
  updatedAt: string | null;
}

/** Row shape returned by SQL Server stored procedures. */
export interface CustomerRow {
  Id: number;
  Nombre: string;
  Direccion: string | null;
  Telefono: string | null;
  Activo: boolean;
  FechaCreacion: string | Date;
  FechaModificacion: string | Date | null;
}

/** Public representation returned by the API. */
export interface CustomerResponse {
  id: number;
  name: string;
  address: string | null;
  phone: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCustomerRequest {
  name: string;
  address?: string | null;
  phone?: string | null;
}

export interface UpdateCustomerRequest {
  name?: string;
  address?: string | null;
  phone?: string | null;
}

/** CA9: page-based pagination backed by SQL Server OFFSET/FETCH. */
export interface PaginatedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}
