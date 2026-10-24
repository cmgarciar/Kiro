import sql from 'mssql';
import { SecretsManagerClient, GetSecretValueCommand } from '@aws-sdk/client-secrets-manager';

/**
 * Connection pool a SQL Server on-premise.
 *
 * Arquitectura hibrida: computo en AWS Lambda, base de datos en tierra.
 * Las credenciales se resuelven en runtime desde Secrets Manager
 * (DB_SECRET_ARN). Si no hay secreto configurado (p. ej. ejecucion local),
 * se cae a variables de entorno.
 *
 * El pool se cachea fuera del handler para reutilizarse entre invocaciones.
 */
interface DbSecret {
  server?: string;
  instance?: string;
  database?: string;
  user?: string;
  password?: string;
  port?: number;
}

let cachedSecret: DbSecret | null = null;

const loadSecret = async (): Promise<DbSecret> => {
  if (cachedSecret) return cachedSecret;
  const arn = process.env.DB_SECRET_ARN;
  if (!arn) {
    cachedSecret = {};
    return cachedSecret;
  }
  const client = new SecretsManagerClient({});
  const res = await client.send(new GetSecretValueCommand({ SecretId: arn }));
  cachedSecret = res.SecretString ? (JSON.parse(res.SecretString) as DbSecret) : {};
  return cachedSecret;
};

const buildConfig = (secret: DbSecret): sql.config => {
  const instance = secret.instance ?? process.env.DB_INSTANCE;
  return {
    server: secret.server ?? process.env.DB_SERVER ?? 'localhost',
    database: secret.database ?? process.env.DB_NAME ?? 'Kiro',
    user: secret.user ?? process.env.DB_USER ?? 'kiro',
    password: secret.password ?? process.env.DB_PASSWORD ?? '',
    port: secret.port ?? (process.env.DB_PORT ? Number(process.env.DB_PORT) : 1433),
    options: {
      encrypt: process.env.DB_ENCRYPT === 'true',
      trustServerCertificate: process.env.DB_TRUST_CERT !== 'false',
      enableArithAbort: true,
      // SQLEXPRESS es una instancia con nombre.
      ...(instance ? { instanceName: instance } : {}),
    },
    pool: { max: 5, min: 0, idleTimeoutMillis: 30000 },
  };
};

let poolPromise: Promise<sql.ConnectionPool> | null = null;

export const getPool = (): Promise<sql.ConnectionPool> => {
  if (!poolPromise) {
    poolPromise = loadSecret()
      .then((secret) => new sql.ConnectionPool(buildConfig(secret)).connect())
      .catch((err) => {
        poolPromise = null;
        throw err;
      });
  }
  return poolPromise;
};

export { sql };
