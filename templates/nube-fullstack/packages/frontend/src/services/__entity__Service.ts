import type {
  __Entity__,
  Create__Entity__Request,
  Update__Entity__Request,
  PagedResult,
} from '../types/__entity__.js';

const API_BASE_URL = import.meta.env.VITE_API_URL;
// Ruta en plural de la entidad. Reemplazar por el recurso real (ej: /clientes).
const RESOURCE = '/__entity__s';

const handle = async <T>(response: Response): Promise<T> => {
  if (!response.ok) {
    const body = await response.json().catch(() => ({ error: response.statusText }));
    throw new Error(body.error ?? 'Error en la solicitud.');
  }
  return response.json() as Promise<T>;
};

export const __entity__Service = {
  list: (pagina = 1, tamPagina = 10, busqueda = ''): Promise<PagedResult<__Entity__>> => {
    const params = new URLSearchParams({ pagina: String(pagina), tamPagina: String(tamPagina) });
    if (busqueda) params.set('busqueda', busqueda);
    return fetch(`${API_BASE_URL}${RESOURCE}?${params}`).then((r) => handle<PagedResult<__Entity__>>(r));
  },

  create: (data: Create__Entity__Request): Promise<__Entity__> =>
    fetch(`${API_BASE_URL}${RESOURCE}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    }).then((r) => handle<__Entity__>(r)),

  update: (id: number, data: Update__Entity__Request): Promise<__Entity__> =>
    fetch(`${API_BASE_URL}${RESOURCE}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    }).then((r) => handle<__Entity__>(r)),

  remove: (id: number): Promise<void> =>
    fetch(`${API_BASE_URL}${RESOURCE}/${id}`, { method: 'DELETE' }).then((r) => {
      if (!r.ok) throw new Error('No se pudo eliminar.');
    }),
};
