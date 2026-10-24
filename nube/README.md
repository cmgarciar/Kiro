# HU1 - Módulo Clientes (AWS Full-Stack)

CRUD de clientes desplegado en AWS con el ecosistema JavaScript/TypeScript:

- **Infraestructura**: AWS CDK v2 (TypeScript) — DynamoDB, Lambda, API Gateway REST, S3 + CloudFront.
- **Backend**: Node.js 22 + TypeScript sobre AWS Lambda.
- **Frontend**: React 18 + TypeScript + Vite + Zustand.

## Estructura del monorepo

```
nube/
├── packages/
│   ├── backend/          # Lambda (handler + service + repository + validaciones)
│   ├── frontend/         # SPA React (listado, búsqueda, alta/edición/borrado)
│   └── infrastructure/   # AWS CDK (StorageStack, ApiStack, FrontendStack)
├── package.json          # npm workspaces
└── tsconfig.base.json
```

## Modelo de datos (DynamoDB single-table design)

Tabla `ClientesTable`:

| Atributo    | Uso                                                     |
|-------------|---------------------------------------------------------|
| `PK`        | `CUSTOMER#<id>`                                          |
| `SK`        | `METADATA`                                               |
| `GSI1PK`    | `CUSTOMER` (activos) / `CUSTOMER#INACTIVE` (soft delete) |
| `GSI1SK`    | `<createdAt>#<id>` (orden por fecha de creación)         |
| `id`        | UUID autogenerado, no editable (CA1, CA2)                |
| `name`      | string ≤ 100, obligatorio (CA3)                          |
| `address`   | string ≤ 200, opcional (CA4)                             |
| `phone`     | string ≤ 20, solo dígitos, opcional (CA5)                |
| `nameLower` | copia en minúsculas para búsqueda case-insensitive (CA6) |
| `isActive`  | `true` por defecto; soft delete lo pone en `false` (CA8) |
| `createdAt` | ISO 8601, se asigna al crear (CA10)                      |
| `updatedAt` | ISO 8601, se actualiza en cada update/delete (CA10)      |

El índice `GSI1` solo contiene clientes activos en la partición `CUSTOMER`, por lo que listar (CA9) y buscar (CA6) operan únicamente sobre activos. La paginación usa `nextToken` (LastEvaluatedKey codificado en base64).

## API REST

Base: `https://<api-id>.execute-api.<region>.amazonaws.com/prod`

| Método | Ruta               | Descripción                                   | CA        |
|--------|--------------------|-----------------------------------------------|-----------|
| POST   | `/customers`       | Crear cliente                                 | CA1       |
| GET    | `/customers`       | Listar activos paginado (`?limit&nextToken`)  | CA9       |
| GET    | `/customers`       | Buscar (`?search=` por nombre o teléfono)     | CA6       |
| GET    | `/customers/{id}`  | Obtener un cliente activo                      | -         |
| PUT    | `/customers/{id}`  | Actualizar nombre/dirección/teléfono           | CA7, CA10 |
| DELETE | `/customers/{id}`  | Soft delete                                    | CA8, CA10 |

Respuesta de listado/búsqueda:

```json
{ "items": [ { "id": "...", "name": "...", "address": null, "phone": null, "isActive": true, "createdAt": "...", "updatedAt": null } ], "nextToken": null }
```

## Mapa de criterios de aceptación

- **CA1** `POST /customers` crea con `id`, `name`, `address`, `phone`.
- **CA2** `id` = UUID generado en el service; nunca se acepta ni modifica desde el cliente.
- **CA3** `name` obligatorio, máx. 100 (validación backend y frontend).
- **CA4** `address` opcional, máx. 200.
- **CA5** `phone` opcional, solo dígitos (`/^[0-9]+$/`), máx. 20.
- **CA6** `GET /customers?search=` filtra por nombre (case-insensitive) o teléfono.
- **CA7** `PUT /customers/{id}` edita solo nombre/dirección/teléfono.
- **CA8** `DELETE` marca `isActive=false` y lo saca del GSI de activos (no se borra físicamente).
- **CA9** `GET /customers` pagina clientes activos (`limit`, `nextToken`); UI con Anterior/Siguiente.
- **CA10** `createdAt` al crear; `updatedAt` en cada update y en el soft delete.

## Requisitos previos

- Node.js 20+
- AWS CLI configurado (`aws configure`) con credenciales válidas
- Docker **no** es necesario: el bundling de la Lambda usa `esbuild` local vía `aws-lambda-nodejs`

## Instalación

```bash
cd nube
npm install
```

## Validación (sin desplegar)

```bash
# TypeScript backend + frontend
npm run typecheck

# Build del frontend (genera packages/frontend/dist)
npm run build:frontend

# Sintetizar la infraestructura (valida que el stack compila)
npm run synth
```

## Despliegue

1. **Bootstrap** (solo la primera vez por cuenta/región):

   ```bash
   cd packages/infrastructure
   npx cdk bootstrap
   ```

2. **Desplegar backend + datos + API**:

   ```bash
   npm run deploy
   ```

   Anota el output `ClientesApiStack.ApiUrl`.

3. **Construir el frontend apuntando a la API**:

   ```bash
   cd ../frontend
   # Crea .env con la URL real de la API
   echo "VITE_API_URL=https://<api-id>.execute-api.<region>.amazonaws.com/prod" > .env
   npm run build
   ```

4. **Desplegar el frontend** (CloudFront + S3). Vuelve a sintetizar/desplegar: el `FrontendStack` sube `packages/frontend/dist` automáticamente si existe.

   ```bash
   cd ../infrastructure
   npx cdk deploy ClientesFrontendStack
   ```

   El output `ClientesFrontendStack.DistributionDomain` es la URL pública del frontend.

## Desarrollo local del frontend

```bash
cd packages/frontend
cp .env.example .env   # ajusta VITE_API_URL a tu API desplegada
npm run dev            # http://localhost:5173
```

## Limpieza

```bash
cd packages/infrastructure
npm run destroy
```

## Notas de arquitectura (Well-Architected)

- **Seguridad**: IAM de mínimo privilegio (`grantReadWriteData` solo sobre la tabla); S3 privado con Origin Access Control; CORS acotado.
- **Observabilidad**: X-Ray activo en Lambda y API Gateway; logs en CloudWatch con retención de 7 días.
- **Fiabilidad**: DynamoDB con Point-in-Time Recovery.
- En desarrollo se usa `RemovalPolicy.DESTROY`; para producción cambiar a `RETAIN`.
