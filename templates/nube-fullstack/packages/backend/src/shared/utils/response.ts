import type { APIGatewayProxyResult } from 'aws-lambda';

const baseHeaders = {
  'Content-Type': 'application/json',
  'Access-Control-Allow-Origin': '*',
};

export const successResponse = (statusCode: number, body: unknown): APIGatewayProxyResult => ({
  statusCode,
  headers: baseHeaders,
  body: JSON.stringify(body),
});

export const errorResponse = (statusCode: number, message: string): APIGatewayProxyResult => ({
  statusCode,
  headers: baseHeaders,
  body: JSON.stringify({ error: message }),
});
