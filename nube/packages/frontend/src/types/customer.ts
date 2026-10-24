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

/** CA9: paginacion por pagina respaldada por SQL Server OFFSET/FETCH. */
export interface PaginatedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}
