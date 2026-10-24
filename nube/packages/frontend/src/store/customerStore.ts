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
  // Stack of tokens to support back/forward pagination.
  tokenStack: Array<string | null>;
  nextToken: string | null;
  pageIndex: number;

  setSearch: (value: string) => void;
  fetchFirstPage: () => Promise<void>;
  nextPage: () => Promise<void>;
  prevPage: () => Promise<void>;
  createCustomer: (data: CreateCustomerRequest) => Promise<void>;
  updateCustomer: (id: string, data: UpdateCustomerRequest) => Promise<void>;
  deleteCustomer: (id: string) => Promise<void>;
}

export const useCustomerStore = create<CustomerStore>((set, get) => {
  const load = async (token: string | null): Promise<void> => {
    set({ isLoading: true, error: null });
    try {
      const result = await customerService.list({
        limit: PAGE_SIZE,
        nextToken: token,
        search: get().search,
      });
      set({ customers: result.items, nextToken: result.nextToken, isLoading: false });
    } catch (err) {
      set({ isLoading: false, error: err instanceof Error ? err.message : 'Error desconocido' });
    }
  };

  return {
    customers: [],
    search: '',
    isLoading: false,
    error: null,
    tokenStack: [null],
    nextToken: null,
    pageIndex: 0,

    setSearch: (value) => set({ search: value }),

    fetchFirstPage: async () => {
      set({ tokenStack: [null], pageIndex: 0 });
      await load(null);
    },

    nextPage: async () => {
      const { nextToken, tokenStack, pageIndex } = get();
      if (!nextToken) return;
      set({ tokenStack: [...tokenStack, nextToken], pageIndex: pageIndex + 1 });
      await load(nextToken);
    },

    prevPage: async () => {
      const { tokenStack, pageIndex } = get();
      if (pageIndex === 0) return;
      const newStack = tokenStack.slice(0, -1);
      const token = newStack[newStack.length - 1];
      set({ tokenStack: newStack, pageIndex: pageIndex - 1 });
      await load(token);
    },

    createCustomer: async (data) => {
      await customerService.create(data);
      await get().fetchFirstPage();
    },

    updateCustomer: async (id, data) => {
      await customerService.update(id, data);
      await load(get().tokenStack[get().pageIndex]);
    },

    deleteCustomer: async (id) => {
      await customerService.remove(id);
      await get().fetchFirstPage();
    },
  };
});
