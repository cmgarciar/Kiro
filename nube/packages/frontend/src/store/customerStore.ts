import { create } from 'zustand';
import { customerService } from '../services/customerService';
import type {
  CustomerResponse,
  CreateCustomerRequest,
  UpdateCustomerRequest,
} from '../types/customer';

const PAGE_SIZE = 10;

interface CustomerStore {
  customers: CustomerResponse[];
  search: string;
  isLoading: boolean;
  error: string | null;
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;

  setSearch: (value: string) => void;
  fetchFirstPage: () => Promise<void>;
  goToPage: (page: number) => Promise<void>;
  nextPage: () => Promise<void>;
  prevPage: () => Promise<void>;
  createCustomer: (data: CreateCustomerRequest) => Promise<void>;
  updateCustomer: (id: number, data: UpdateCustomerRequest) => Promise<void>;
  deleteCustomer: (id: number) => Promise<void>;
}

export const useCustomerStore = create<CustomerStore>((set, get) => {
  const load = async (page: number): Promise<void> => {
    set({ isLoading: true, error: null });
    try {
      const result = await customerService.list({
        page,
        pageSize: PAGE_SIZE,
        search: get().search,
      });
      set({
        customers: result.items,
        page: result.page,
        pageSize: result.pageSize,
        total: result.total,
        totalPages: result.totalPages,
        isLoading: false,
      });
    } catch (err) {
      set({ isLoading: false, error: err instanceof Error ? err.message : 'Error desconocido' });
    }
  };

  return {
    customers: [],
    search: '',
    isLoading: false,
    error: null,
    page: 1,
    pageSize: PAGE_SIZE,
    total: 0,
    totalPages: 1,

    setSearch: (value) => set({ search: value }),

    fetchFirstPage: () => load(1),

    goToPage: (page) => load(page),

    nextPage: async () => {
      const { page, totalPages } = get();
      if (page >= totalPages) return;
      await load(page + 1);
    },

    prevPage: async () => {
      const { page } = get();
      if (page <= 1) return;
      await load(page - 1);
    },

    createCustomer: async (data) => {
      await customerService.create(data);
      await load(1);
    },

    updateCustomer: async (id, data) => {
      await customerService.update(id, data);
      await load(get().page);
    },

    deleteCustomer: async (id) => {
      await customerService.remove(id);
      await load(get().page);
    },
  };
});
