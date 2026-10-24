import { CustomerRepository } from './repository.js';
import { NotFoundException } from '../../shared/errors/index.js';
import { validateCreate, validateUpdate } from '../../shared/utils/validation.js';
import type {
  Customer,
  CustomerResponse,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  PaginatedResult,
} from '../../shared/types/customer.js';

const toResponse = (c: Customer): CustomerResponse => ({
  id: c.id,
  name: c.name,
  address: c.address,
  phone: c.phone,
  isActive: c.isActive,
  createdAt: c.createdAt,
  updatedAt: c.updatedAt,
});

const mapPage = (result: PaginatedResult<Customer>): PaginatedResult<CustomerResponse> => ({
  items: result.items.map(toResponse),
  page: result.page,
  pageSize: result.pageSize,
  total: result.total,
  totalPages: result.totalPages,
});

export class CustomerService {
  private readonly repo: CustomerRepository;

  constructor(repo: CustomerRepository = new CustomerRepository()) {
    this.repo = repo;
  }

  /** CA1, CA2, CA10: create con Id autogenerado en SQL Server. */
  async create(body: CreateCustomerRequest): Promise<CustomerResponse> {
    const validated = validateCreate(body);
    const created = await this.repo.create({
      name: validated.name,
      address: validated.address,
      phone: validated.phone,
    });
    return toResponse(created);
  }

  /** CA6, CA9: listar activos paginados; con search filtra por nombre o telefono. */
  async list(
    search: string | null,
    page: number,
    pageSize: number,
  ): Promise<PaginatedResult<CustomerResponse>> {
    const trimmed = search && search.trim().length > 0 ? search.trim() : null;
    const result = await this.repo.list(trimmed, page, pageSize);
    return mapPage(result);
  }

  async getById(id: number): Promise<CustomerResponse> {
    const customer = await this.repo.getById(id);
    if (!customer) {
      throw new NotFoundException('Cliente', id);
    }
    return toResponse(customer);
  }

  /** CA7, CA10: actualiza nombre/direccion/telefono. */
  async update(id: number, body: UpdateCustomerRequest): Promise<CustomerResponse> {
    const existing = await this.repo.getById(id);
    if (!existing) {
      throw new NotFoundException('Cliente', id);
    }
    const validated = validateUpdate(body);
    const updated = await this.repo.update({
      id,
      name: validated.name ?? existing.name,
      address: validated.address !== undefined ? validated.address : existing.address,
      phone: validated.phone !== undefined ? validated.phone : existing.phone,
    });
    if (!updated) {
      throw new NotFoundException('Cliente', id);
    }
    return toResponse(updated);
  }

  /** CA8, CA10: soft delete. */
  async softDelete(id: number): Promise<void> {
    const deleted = await this.repo.softDelete(id);
    if (!deleted) {
      throw new NotFoundException('Cliente', id);
    }
  }
}
