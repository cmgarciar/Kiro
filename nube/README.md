# HU1 - Módulo Clientes (AWS + SQL Server on-premise)

CRUD de clientes con **arquitectura híbrida**: el cómputo corre en AWS (Lambda + API Gateway) y la **base de datos es SQL Server on-premise** (base `Kiro`). El frontend es una SPA React servida desde S3 + CloudFront.

- **Infraestructura**: AWS CDK v2 (TypeScript) — Lambda, API Gateway REST, Secrets Manager, S3 + CloudFront.
- **Backend**: Node.js 22 + TypeScript sobre AWS Lambda, acceso a datos con `mssql` vía stored procedures.
- **Base de datos**: SQL Server (tabla `dbo.Clientes` + 5 stored procedures).
- **Frontend**: React 18 + TypeScript + Vite + Zustand.

## Estructura del monorepo

```
nube/
├── database/
│   └── 01_Clientes.sql       # tabla + stored procedures (SQL Server)
├── packages/
│   ├── backend/              # Lambda (handler + service + repository + pool mssql)
│   ├── frontend/             # SPA React (listado, búsqueda, alta/edición/borrado)
│   └── infrastructure/       # AWS CDK (ApiStack, FrontendStack)
├── package.json              # npm workspaces
└── tsconfig.base.json
```

## Modelo de datos (SQL Server — tabla `dbo.Clientes`)

| Columna             | Tipo           | Notas                                             | CA        |
|---------------------|----------------|---------------------------------------------------|-----------|
| `Id`                | INT IDENTITY   | PK, autogenerado, no editable                     | CA1, CA2  |
| `Nombre`            | NVARCHAR(100)  | obligatorio                                       | CA3       |
| `Direccion`         | NVARCHAR(200)  | opcional                                          | CA4       |
| `Telefono`          | NVARCHAR(20)   | opcional, solo dígitos (CHECK constraint)         | CA5       |
| `Activo`            | BIT            | default 1, soft delete lo pone en 0               | CA8       |
| `FechaCreacion`     | DATETIME2      | default GETDATE()                                 | CA10      |
| `FechaModificacion` | DATETIME2      | se actualiza en cada UPDATE/soft delete           | CA10      |

Índice `IX_Clientes_Activo_Nombre` para acelerar listado/búsqueda de activos.

### Stored procedures

| SP                             | Propósito                                             | CA        |
|--------------------------------|-------------------------------------------------------|-----------|
| `usp_Clientes_Crear`           | Inserta y devuelve el cliente con su Id               | CA1, CA10 |
| `usp_Clientes_ObtenerPorId`    | Obtiene un cliente activo                              | -         |
| `usp_Clientes_Listar`          | Lista activos paginado; filtra por Nombre o Teléfono  | CA6, CA9  |
| `usp_Clientes_Actualizar`      | Edita Nombre/Dirección/Teléfono + FechaModificacion   | CA7, CA10 |
| `usp_Clientes_Eliminar`        | Soft delete (Activo=0)                                | CA8, CA10 |

El SP de listado devuelve dos recordsets: `[0]` el total de registros y `[1]` la página, que el backend empaqueta en `{ items, page, pageSize, total, totalPages }`.

## API REST

Base: `https://<api-id>.execute-api.<region>.amazonaws.com/prod`

| Método | Ruta               | Descripción                                      | CA        |
|--------|--------------------|--------------------------------------------------|-----------|
| POST   | `/customers`       | Crear cliente                                    | CA1       |
| GET    | `/customers`       | Listar activos paginado (`?page&pageSize`)       | CA9       |
| GET    | `/customers`       | Buscar (`?search=` por nombre o teléfono)        | CA6       |
| GET    | `/customers/{id}`  | Obtener un cliente activo                         | -         |
| PUT    | `/customers/{id}`  | Actualizar nombre/dirección/teléfono              | CA7, CA10 |
| DELETE | `/customers/{id}`  | Soft delete                                       | CA8, CA10 |

Respuesta de listado/búsqueda:

```json
{ "items": [ { "id": 1, "name": "...", "address": null, "phone": null, "isActive": true, "createdAt": "...", "updatedAt": null } ], "page": 1, "pageSize": 10, "total": 2, "totalPages": 1 }
```

## Conexión a SQL Server

La Lambda resuelve las credenciales desde **Secrets Manager** (secreto `clientes/sqlserver`), con caída a variables de entorno para ejecución local:

| Variable        | Ejemplo            | Uso                          |
|-----------------|--------------------|------------------------------|
| `DB_SECRET_ARN` | arn del secreto    | fuente principal de la config |
| `DB_SERVER`     | `DESKTOP-0T7DSHL`  | host de SQL Server            |
| `DB_INSTANCE`   | `SQLEXPRESS`       | instancia con nombre          |
| `DB_NAME`       | `Kiro`             | base de datos                 |
| `DB_USER`       | `kiro`             | usuario                       |
| `DB_ENCRYPT`    | `false`            | sin TLS (SQL Express local)   |
| `DB_TRUST_CERT` | `true`             | confiar en el certificado     |

> **Red (importante):** para que una Lambda en AWS alcance un SQL Server que está físicamente on-premise, la Lambda debe desplegarse dentro de una VPC con conectividad hacia la red corporativa (VPN Site-to-Site o Direct Connect). El `ApiStack` deja lista la config y el secreto; el túnel VPC/VPN se ajusta según el entorno de despliegue real.

## Requisitos previos

- Node.js 20+
- SQL Server accesible (en desarrollo: `DESKTOP-0T7DSHL\SQLEXPRESS`, base `Kiro`)
- AWS CLI configurado (`aws configure`) para desplegar la parte de nube

## Base de datos

```bash
sqlcmd -S "DESKTOP-0T7DSHL\SQLEXPRESS" -U kiro -P <password> -d Kiro -C -i database/01_Clientes.sql
```

## Validación (sin desplegar)

```bash
cd nube
npm install
npm run typecheck        # backend + frontend
npm run build:frontend   # genera packages/frontend/dist
npm run synth            # sintetiza la infraestructura (valida que compila)
```

## Despliegue de la nube

1. **Bootstrap** (solo la primera vez por cuenta/región):
   ```bash
   cd packages/infrastructure && npx cdk bootstrap
   ```
2. **Desplegar API** y actualizar el secreto con la contraseña real:
   ```bash
   npm run deploy
   aws secretsmanager put-secret-value --secret-id clientes/sqlserver \
     --secret-string '{"server":"...","instance":"SQLEXPRESS","database":"Kiro","user":"kiro","password":"<real>"}'
   ```
3. **Frontend** apuntando a la API:
   ```bash
   cd ../frontend
   echo "VITE_API_URL=https://<api-id>.execute-api.<region>.amazonaws.com/prod" > .env
   npm run build
   cd ../infrastructure && npx cdk deploy ClientesFrontendStack
   ```

## Mapa de criterios de aceptación

- **CA1** `POST /customers` crea con `name`, `address`, `phone`; `usp_Clientes_Crear`.
- **CA2** `Id` = INT IDENTITY generado por SQL Server; nunca editable.
- **CA3** `name` obligatorio, máx. 100 (validación backend + frontend).
- **CA4** `address` opcional, máx. 200.
- **CA5** `phone` opcional, solo dígitos (regex en backend/frontend + CHECK constraint en BD), máx. 20.
- **CA6** `GET /customers?search=` filtra por nombre o teléfono (`LIKE`).
- **CA7** `PUT /customers/{id}` edita solo nombre/dirección/teléfono.
- **CA8** `DELETE` marca `Activo=0` (soft delete, no borra físicamente).
- **CA9** `GET /customers` pagina activos (`page`, `pageSize`); UI con Anterior/Siguiente y total.
- **CA10** `FechaCreacion` al crear; `FechaModificacion` en cada update y soft delete.
```
