import mssql from 'mssql';
import { DatabaseException } from '../errors/index.js';

// Pool de conexión a SQL Server con selección de driver por entorno:
//  - LOCAL (Windows):  DB_DRIVER=msnodesqlv8 -> ODBC, conecta como sqlcmd sin tocar
//                      la config del servidor (no requiere TCP/IP ni SQL Browser).
//  - AWS (Lambda/Linux): DB_DRIVER=tedious    -> TCP puro.
//
// Lección aprendida: msnodesqlv8 es nativo de Windows y NO corre en Lambda.
// Nunca dejar el driver nativo como única opción si hay deploy a la nube.

let poolPromise: Promise<mssql.ConnectionPool> | null = null;

export const getPool = async (): Promise<mssql.ConnectionPool> => {
  if (poolPromise) return poolPromise;
  try {
    const driver = process.env.DB_DRIVER ?? 'msnodesqlv8';
    const server = process.env.DB_SERVER!;
    const database = process.env.DB_NAME!;
    const user = process.env.DB_USER!;
    const password = process.env.DB_PASSWORD!;

    if (driver === 'msnodesqlv8') {
      // Import dinámico del wrapper ODBC solo cuando se usa en local.
      const odbc = await import('mssql/msnodesqlv8.js');
      const sqlOdbc = odbc.default;

      // IMPORTANTE: el wrapper msnodesqlv8 consume un objeto config con
      // `connectionString`. Pasar un string pelado NO funciona (da el error
      // "no se encuentra el origen de datos y no se especificó ningún
      // controlador predeterminado"). Lección aprendida en la HU1.
      const connectionString =
        `Driver={ODBC Driver 17 for SQL Server};` +
        `Server=${server};Database=${database};` +
        `Uid=${user};Pwd=${password};` +
        `Encrypt=no;TrustServerCertificate=yes;`;

      poolPromise = new sqlOdbc.ConnectionPool({
        connectionString,
      } as unknown as mssql.config).connect();
    } else {
      // tedious (TCP) para la nube.
      poolPromise = new mssql.ConnectionPool({
        server,
        database,
        user,
        password,
        options: { encrypt: true, trustServerCertificate: false },
      }).connect();
    }
    return await poolPromise;
  } catch (error) {
    poolPromise = null;
    throw new DatabaseException('No se pudo conectar a la base de datos.', error);
  }
};
