import { v4 as uuidv4 } from 'uuid';
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
  address: c.address ?? null,
  phone: c.phone ?? null,
  isActive: c.isActive,
  createdAt: c.createdAt,
  updatedAt: c.updatedAt ?? null,
});

export class CustomerService {
  private readonly repo: CustomerRepository;

  constructor(repo: CustomerRepository = new CustomerRepository()) {
    this.repo = repo;
  }

  /** CA1, CA2, CA10: create with auto-generated id and creation timestamp. */
  async create(body: CreateCustomerRequest): Promise<CustomerResponse> {
    const validated = validateCreate(body);
    const now = new Date().toISOString();
    const customer: Customer = {
      id: uuidv4(),
      name: validated.name,
      address: validated.address ?? undefined,
      phone: validated.phone ?? undefined,
      isActive: true,
      createdAt: now,
      updatedAt: undefined,
    };
    const created = await this.repo.create(customer);
    return toResponse(created);
  }

  /** CA9: list active customers, paginated. */
  async list(limit: number, token?: string): Promise<PaginatedResult<CustomerResponse>> {
    const result = await this.repo.listActive(limit, token);
    return { items: result.items.map(toResponse), nextToken: result.nextToken };
  }

  /** CA6: search active customers by name or phone. */
  async search(
    query: string,
    limit: number,
    token?: string,
  ): Promise<PaginatedResult<CustomerResponse>> {
    const trimmed = query.trim();
    if (trimmed.length === 0) {
      return this.list(limit, token);
    }
    const result = await this.repo.search(trimmed, limit, token);
    return { items: result.items.map(toResponse), nextToken: result.nextToken };
  }

  async getById(id: string): Promise<CustomerResponse> {
    const customer = await this.repo.getById(id);
    if (!customer || !customer.isActive) {
      throw new NotFoundException('Customer', id);
    }
    return toResponse(customer);
  }

  /** CA7, CA10: update editable fields and set modification timestamp. */
  async update(id: string, body: UpdateCustomerRequest): Promise<CustomerResponse> {
    const existing = await this.repo.getById(id);
    if (!existing || !existing.isActive) {
      throw new NotFoundException('Customer', id);
    }
    const validated = validateUpdate(body);
    const updated: Customer = {
      ...existing,
      name: validated.name ?? existing.name,
      address: validated.address !== undefined ? (validated.address ?? undefined) : existing.address,
      phone: validated.phone !== undefined ? (validated.phone ?? undefined) : existing.phone,
      updatedAt: new Date().toISOString(),
    };
    const result = await this.repo.update(updated);
    return toResponse(result);
  }

  /** CA8, CA10: soft delete. */
  async softDelete(id: string): Promise<void> {
    const existing = await this.repo.getById(id);
    if (!existing || !existing.isActive) {
      throw new NotFoundException('Customer', id);
    }
    await this.repo.softDelete(id, new Date().toISOString());
  }
}
