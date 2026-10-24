/**
 * Logical model for a Customer (HU1 - Modulo Clientes).
 * Field names in English per backend conventions; map to Spanish in the UI.
 */
export interface Customer {
  /** CA1/CA2: unique, auto-generated, non-editable identifier (PK). */
  id: string;
  /** CA3: required, max 100 chars. */
  name: string;
  /** CA4: optional, max 200 chars. */
  address?: string;
  /** CA5: optional, digits only, max 20 chars. */
  phone?: string;
  /** CA8: soft delete flag. Default true. */
  isActive: boolean;
  /** CA10: creation timestamp (ISO 8601). */
  createdAt: string;
  /** CA10: last modification timestamp (ISO 8601). */
  updatedAt?: string;
}

/** Shape stored in DynamoDB (single-table design). */
export interface CustomerItem extends Customer {
  PK: string;
  SK: string;
  GSI1PK: string;
  GSI1SK: string;
  /** Lowercased name for case-insensitive search. */
  nameLower: string;
}

/** Public representation returned by the API. */
export interface CustomerResponse {
  id: string;
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

export interface PaginatedResult<T> {
  items: T[];
  nextToken: string | null;
}
