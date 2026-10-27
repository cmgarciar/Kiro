// Tipos del dominio __Entity__ en el frontend. Reemplazar __Entity__ / __entity__.

export interface __Entity__ {
  id: number;
  nombre: string;
  descripcion: string | null;
  activo: boolean;
  fechaCreacion: string;
  fechaModificacion: string | null;
}

export interface Create__Entity__Request {
  nombre: string;
  descripcion?: string | null;
}

export type Update__Entity__Request = Create__Entity__Request;

export interface PagedResult<T> {
  items: T[];
  total: number;
  pagina: number;
  tamPagina: number;
}
