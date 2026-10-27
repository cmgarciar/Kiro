import { __Entity__Repository } from './repository.js';
import { NotFoundException, ValidationException } from '../../shared/errors/index.js';
import type {
  __Entity__,
  Create__Entity__Request,
  Update__Entity__Request,
  PagedResult,
} from './types.js';

export class __Entity__Service {
  private readonly repository = new __Entity__Repository();

  async list(pagina: number, tamPagina: number, busqueda?: string): Promise<PagedResult<__Entity__>> {
    const safePagina = pagina > 0 ? pagina : 1;
    const safeTam = tamPagina > 0 && tamPagina <= 100 ? tamPagina : 10;
    return this.repository.list(safePagina, safeTam, busqueda);
  }

  async getById(id: number): Promise<__Entity__> {
    const found = await this.repository.getById(id);
    if (!found) throw new NotFoundException('__Entity__', id);
    return found;
  }

  async create(input: Create__Entity__Request): Promise<__Entity__> {
    if (!input.nombre?.trim()) throw new ValidationException('El nombre es obligatorio.');
    return this.repository.create(input);
  }

  async update(id: number, input: Update__Entity__Request): Promise<__Entity__> {
    if (!input.nombre?.trim()) throw new ValidationException('El nombre es obligatorio.');
    const updated = await this.repository.update(id, input);
    if (!updated) throw new NotFoundException('__Entity__', id);
    return updated;
  }

  async remove(id: number): Promise<void> {
    await this.getById(id); // 404 si no existe
    await this.repository.remove(id);
  }
}
