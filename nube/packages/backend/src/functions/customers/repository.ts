import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import {
  DynamoDBDocumentClient,
  PutCommand,
  GetCommand,
  QueryCommand,
  UpdateCommand,
} from '@aws-sdk/lib-dynamodb';
import { DatabaseException } from '../../shared/errors/index.js';
import type { Customer, CustomerItem, PaginatedResult } from '../../shared/types/customer.js';

const TABLE_NAME = process.env.TABLE_NAME ?? '';
const GSI1_NAME = 'GSI1';
const ENTITY = 'CUSTOMER';

const client = new DynamoDBClient({});
const docClient = DynamoDBDocumentClient.from(client, {
  marshallOptions: { removeUndefinedValues: true },
});

const pk = (id: string): string => `${ENTITY}#${id}`;
const gsi1sk = (createdAt: string, id: string): string => `${createdAt}#${id}`;

const toItem = (customer: Customer): CustomerItem => ({
  ...customer,
  PK: pk(customer.id),
  SK: 'METADATA',
  // Only active customers get indexed in GSI1 so list/search scan active records only.
  GSI1PK: customer.isActive ? ENTITY : `${ENTITY}#INACTIVE`,
  GSI1SK: gsi1sk(customer.createdAt, customer.id),
  // Lowercased copy of name to support case-insensitive contains search (CA6).
  nameLower: customer.name.toLowerCase(),
});

const stripKeys = (item: CustomerItem): Customer => {
  const { PK, SK, GSI1PK, GSI1SK, nameLower, ...customer } = item;
  return customer;
};

const encodeToken = (key: Record<string, unknown> | undefined): string | null =>
  key ? Buffer.from(JSON.stringify(key)).toString('base64') : null;

const decodeToken = (token: string | undefined): Record<string, unknown> | undefined => {
  if (!token) return undefined;
  try {
    return JSON.parse(Buffer.from(token, 'base64').toString('utf-8'));
  } catch {
    return undefined;
  }
};

export class CustomerRepository {
  async create(customer: Customer): Promise<Customer> {
    try {
      await docClient.send(
        new PutCommand({
          TableName: TABLE_NAME,
          Item: toItem(customer),
          ConditionExpression: 'attribute_not_exists(PK)',
        }),
      );
      return customer;
    } catch (error) {
      throw new DatabaseException('Error al crear el cliente.', error);
    }
  }

  async getById(id: string): Promise<Customer | null> {
    try {
      const result = await docClient.send(
        new GetCommand({
          TableName: TABLE_NAME,
          Key: { PK: pk(id), SK: 'METADATA' },
        }),
      );
      if (!result.Item) return null;
      return stripKeys(result.Item as CustomerItem);
    } catch (error) {
      throw new DatabaseException('Error al obtener el cliente.', error);
    }
  }

  /** CA9: list active customers, paginated and ordered by createdAt. */
  async listActive(limit: number, token?: string): Promise<PaginatedResult<Customer>> {
    try {
      const result = await docClient.send(
        new QueryCommand({
          TableName: TABLE_NAME,
          IndexName: GSI1_NAME,
          KeyConditionExpression: 'GSI1PK = :pk',
          ExpressionAttributeValues: { ':pk': ENTITY },
          Limit: limit,
          ExclusiveStartKey: decodeToken(token),
          ScanIndexForward: false,
        }),
      );
      return {
        items: (result.Items ?? []).map((i) => stripKeys(i as CustomerItem)),
        nextToken: encodeToken(result.LastEvaluatedKey),
      };
    } catch (error) {
      throw new DatabaseException('Error al listar clientes.', error);
    }
  }

  /**
   * CA6: search active customers by name or phone (case-insensitive contains).
   * Filtering is applied server-side over the active-customers GSI partition.
   */
  async search(query: string, limit: number, token?: string): Promise<PaginatedResult<Customer>> {
    try {
      const lower = query.toLowerCase();
      const items: Customer[] = [];
      let lastKey = decodeToken(token);

      // Accumulate matches until we reach the page size or run out of data.
      do {
        const result = await docClient.send(
          new QueryCommand({
            TableName: TABLE_NAME,
            IndexName: GSI1_NAME,
            KeyConditionExpression: 'GSI1PK = :pk',
            FilterExpression: 'contains(#nameLower, :q) OR contains(#phone, :q)',
            ExpressionAttributeNames: { '#nameLower': 'nameLower', '#phone': 'phone' },
            ExpressionAttributeValues: { ':pk': ENTITY, ':q': lower },
            ExclusiveStartKey: lastKey,
            ScanIndexForward: false,
          }),
        );
        for (const i of result.Items ?? []) {
          items.push(stripKeys(i as CustomerItem));
          if (items.length >= limit) break;
        }
        lastKey = result.LastEvaluatedKey;
      } while (lastKey && items.length < limit);

      return { items, nextToken: encodeToken(lastKey) };
    } catch (error) {
      throw new DatabaseException('Error al buscar clientes.', error);
    }
  }

  async update(customer: Customer): Promise<Customer> {
    try {
      await docClient.send(
        new PutCommand({
          TableName: TABLE_NAME,
          Item: toItem(customer),
          ConditionExpression: 'attribute_exists(PK)',
        }),
      );
      return customer;
    } catch (error) {
      throw new DatabaseException('Error al actualizar el cliente.', error);
    }
  }

  /** CA8: soft delete - flip isActive and remove from active GSI partition. */
  async softDelete(id: string, updatedAt: string): Promise<void> {
    try {
      await docClient.send(
        new UpdateCommand({
          TableName: TABLE_NAME,
          Key: { PK: pk(id), SK: 'METADATA' },
          ConditionExpression: 'attribute_exists(PK)',
          UpdateExpression:
            'SET isActive = :false, updatedAt = :now, GSI1PK = :inactivePk',
          ExpressionAttributeValues: {
            ':false': false,
            ':now': updatedAt,
            ':inactivePk': `${ENTITY}#INACTIVE`,
          },
        }),
      );
    } catch (error) {
      throw new DatabaseException('Error al eliminar el cliente.', error);
    }
  }
}
