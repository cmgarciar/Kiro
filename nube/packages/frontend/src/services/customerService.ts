import type {
  CustomerResponse,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  PaginatedResult,
} from '../types/customer';

const API_BASE_URL = import.meta.env.VITE_API_URL?.replace(/\/$/, '') ?? '';

const parseError = async (response: Response): Promise<string> => {
  try {
    const data = await response.json();
    return (data?.error as string) ?? `Error ${response.status}`;
  } catch {
    return `Error ${response.status}`;
  }
};

const request = async <T>(url: string, init?: RequestInit): Promise<T> => {
  const response = await fetch(url, init);
  if (!response.ok) {
    throw new Error(await parseError(response));
  }
  if (response.status === 204) return {} as T;
  return response.json() as Promise<T>;
};

export interface ListParams {
  page?: number;
  pageSize?: number;
  search?: string;
}

export const customerService = {
  list: async (params: ListParams = {}): Promise<PaginatedResult<CustomerResponse>> => {
    const qs = new URLSearchParams();
    if (params.page) qs.set('page', String(params.page));
    if (params.pageSize) qs.set('pageSize', String(params.pageSize));
    if (params.search && params.search.trim().length > 0) qs.set('search', params.search.trim());
    const query = qs.toString();
    return request<PaginatedResult<CustomerResponse>>(
      `${API_BASE_URL}/customers${query ? `?${query}` : ''}`,
    );
  },

  create: async (data: CreateCustomerRequest): Promise<CustomerResponse> =>
    request<CustomerResponse>(`${API_BASE_URL}/customers`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    }),

  update: async (id: number, data: UpdateCustomerRequest): Promise<CustomerResponse> =>
    request<CustomerResponse>(`${API_BASE_URL}/customers/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    }),

  remove: async (id: number): Promise<void> =>
    request<void>(`${API_BASE_URL}/customers/${id}`, { method: 'DELETE' }),
};
