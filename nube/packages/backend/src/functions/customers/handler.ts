import type { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { CustomerService } from './service.js';
import { successResponse, errorResponse } from '../../shared/utils/response.js';
import {
  NotFoundException,
  ValidationException,
  DatabaseException,
} from '../../shared/errors/index.js';

const service = new CustomerService();

const DEFAULT_LIMIT = 10;
const MAX_LIMIT = 100;

const parseLimit = (raw: string | undefined): number => {
  const parsed = Number(raw);
  if (!Number.isFinite(parsed) || parsed <= 0) return DEFAULT_LIMIT;
  return Math.min(Math.floor(parsed), MAX_LIMIT);
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
    const id = event.pathParameters?.id;
    const qs = event.queryStringParameters ?? {};

    // POST /customers -> create (CA1)
    if (method === 'POST' && !id) {
      const result = await service.create(parseBody(event.body));
      return successResponse(201, result);
    }

    // GET /customers -> list active paginated (CA9) or search (CA6)
    if (method === 'GET' && !id) {
      const limit = parseLimit(qs.limit ?? undefined);
      const token = qs.nextToken ?? undefined;
      const query = qs.search ?? qs.q;
      const result = query
        ? await service.search(query, limit, token)
        : await service.list(limit, token);
      return successResponse(200, result);
    }

    // GET /customers/{id} -> get one
    if (method === 'GET' && id) {
      const result = await service.getById(id);
      return successResponse(200, result);
    }

    // PUT /customers/{id} -> update (CA7)
    if (method === 'PUT' && id) {
      const result = await service.update(id, parseBody(event.body));
      return successResponse(200, result);
    }

    // DELETE /customers/{id} -> soft delete (CA8)
    if (method === 'DELETE' && id) {
      await service.softDelete(id);
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
