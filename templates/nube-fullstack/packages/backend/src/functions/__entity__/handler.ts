import type { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { __Entity__Service } from './service.js';
import { successResponse, errorResponse } from '../../shared/utils/response.js';
import { NotFoundException, ValidationException } from '../../shared/errors/index.js';

// Handler Lambda para API Gateway. En local se usa el mismo service desde
// local-server.ts; este handler queda listo para el día que se despliegue a AWS.
const service = new __Entity__Service();

export const handler = async (
  event: APIGatewayProxyEvent
): Promise<APIGatewayProxyResult> => {
  try {
    const id = event.pathParameters?.id ? Number(event.pathParameters.id) : undefined;

    switch (event.httpMethod) {
      case 'GET': {
        if (id !== undefined) return successResponse(200, await service.getById(id));
        const pagina = Number(event.queryStringParameters?.pagina ?? 1);
        const tamPagina = Number(event.queryStringParameters?.tamPagina ?? 10);
        const busqueda = event.queryStringParameters?.busqueda ?? undefined;
        return successResponse(200, await service.list(pagina, tamPagina, busqueda));
      }
      case 'POST':
        return successResponse(201, await service.create(JSON.parse(event.body ?? '{}')));
      case 'PUT':
        if (id === undefined) return errorResponse(400, 'Falta el id.');
        return successResponse(200, await service.update(id, JSON.parse(event.body ?? '{}')));
      case 'DELETE':
        if (id === undefined) return errorResponse(400, 'Falta el id.');
        await service.remove(id);
        return successResponse(204, {});
      default:
        return errorResponse(405, 'Método no permitido.');
    }
  } catch (error) {
    if (error instanceof NotFoundException) return errorResponse(404, error.message);
    if (error instanceof ValidationException) return errorResponse(400, error.message);
    console.error('Error inesperado:', error);
    return errorResponse(500, 'Error interno del servidor.');
  }
};
