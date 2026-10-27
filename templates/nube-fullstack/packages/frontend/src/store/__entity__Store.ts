import { create } from 'zustand';
import { __entity__Service } from '../services/__entity__Service.js';
import type { __Entity__, Create__Entity__Request } from '../types/__entity__.js';

interface __Entity__Store {
  items: __Entity__[];
  total: number;
  pagina: number;
  tamPagina: number;
  busqueda: string;
  isLoading: boolean;
  error: string | null;
  fetch: () => Promise<void>;
  setBusqueda: (value: string) => void;
  setPagina: (value: number) => void;
  create: (data: Create__Entity__Request) => Promise<void>;
  update: (id: number, data: Create__Entity__Request) => Promise<void>;
  remove: (id: number) => Promise<void>;
}

export const use__Entity__Store = create<__Entity__Store>((set, get) => ({
  items: [],
  total: 0,
  pagina: 1,
  tamPagina: 10,
  busqueda: '',
  isLoading: false,
  error: null,

  fetch: async () => {
    set({ isLoading: true, error: null });
    try {
      const { pagina, tamPagina, busqueda } = get();
      const result = await __entity__Service.list(pagina, tamPagina, busqueda);
      set({ items: result.items, total: result.total, isLoading: false });
    } catch (e) {
      set({ error: (e as Error).message, isLoading: false });
    }
  },

  setBusqueda: (busqueda) => set({ busqueda, pagina: 1 }),
  setPagina: (pagina) => set({ pagina }),

  create: async (data) => {
    await __entity__Service.create(data);
    await get().fetch();
  },
  update: async (id, data) => {
    await __entity__Service.update(id, data);
    await get().fetch();
  },
  remove: async (id) => {
    await __entity__Service.remove(id);
    await get().fetch();
  },
}));
