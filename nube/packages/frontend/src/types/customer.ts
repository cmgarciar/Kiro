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
