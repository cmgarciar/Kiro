import type { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { CustomerService } from './service.js';
import { successResponse, errorResponse } from '../../shared/utils/response.js';
import {
  NotFoundException,
  ValidationException,
  DatabaseException,
} from '../../shared/errors/index.js';

const service = new CustomerService();

const DEFAULT_PAGE_SIZE = 10;
const MAX_PAGE_SIZE = 100;

const parsePage = (raw: string | undefined): number => {
  const parsed = Number(raw);
  if (!Number.isFinite(parsed) || parsed < 1) return 1;
  return Math.floor(parsed);
};

const parsePageSize = (raw: string | undefined): number => {
  const parsed = Number(raw);
  if (!Number.isFinite(parsed) || parsed <= 0) return DEFAULT_PAGE_SIZE;
  return Math.min(Math.floor(parsed), MAX_PAGE_SIZE);
};

const parseId = (raw: string | undefined): number => {
  const parsed = Number(raw);
  if (!Number.isInteger(parsed) || parsed < 1) {
    throw new ValidationException('El Id del cliente no es valido.');
  }
  return parsed;
};

const parseBody = <T>(body: string | null): T => {
  if (!body) throw new ValidationException('El cuerpo de la peticion es requerido.');
  try {
    return JSON.parse(body) as T;
  } catch {
    throw new ValidationException('El cuerpo de la peticion no es un JSON valido.');
  }
};

export const handler = async (
  event: APIGatewayProxyEvent,
): Promise<APIGatewayProxyResult> => {
  try {
    const method = event.httpMethod;
    const rawId = event.pathParameters?.id;
    const qs = event.queryStringParameters ?? {};

    // POST /customers -> create (CA1)
    if (method === 'POST' && !rawId) {
      const result = await service.create(parseBody(event.body));
      return successResponse(201, result);
    }

    // GET /customers -> list active paginated (CA9) / search (CA6)
    if (method === 'GET' && !rawId) {
      const page = parsePage(qs.page ?? undefined);
      const pageSize = parsePageSize(qs.pageSize ?? qs.limit ?? undefined);
      const search = qs.search ?? qs.q ?? null;
      const result = await service.list(search, page, pageSize);
      return successResponse(200, result);
    }

    // GET /customers/{id} -> get one
    if (method === 'GET' && rawId) {
      const result = await service.getById(parseId(rawId));
      return successResponse(200, result);
    }

    // PUT /customers/{id} -> update (CA7)
    if (method === 'PUT' && rawId) {
      const result = await service.update(parseId(rawId), parseBody(event.body));
      return successResponse(200, result);
    }

    // DELETE /customers/{id} -> soft delete (CA8)
    if (method === 'DELETE' && rawId) {
      await service.softDelete(parseId(rawId));
      return successResponse(204, {});
    }

    return errorResponse(405, `Metodo o ruta no soportada: ${method}`);
  } catch (error) {
    if (error instanceof NotFoundException) return errorResponse(404, error.message);
    if (error instanceof ValidationException) return errorResponse(400, error.message);
    if (error instanceof DatabaseException) {
      console.error('Database error:', error, error.cause);
      return errorResponse(500, error.message);
    }
    console.error('Unexpected error:', error);
    return errorResponse(500, 'Error interno del servidor.');
  }
};
