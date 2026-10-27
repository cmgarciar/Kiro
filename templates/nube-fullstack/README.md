# Plantilla full-stack nube (local)

Esqueleto reutilizable para historias de usuario con el stack React + Node + SQL Server,
corriendo **en local** (SQL Express + API Node + Vite). El código CDK queda escrito como
referencia pero **no se sintetiza ni despliega** (no hay cuenta AWS).

## Qué trae

```
nube-fullstack/
├── db/
│   ├── 01_Tabla___Entity__.sql            # tabla PK IDENTITY + auditoría + soft delete
│   └── 02_StoredProcedures___Entity__.sql # 5 SPs con TRY/CATCH y transacción
├── packages/
│   ├── backend/        # Node + TS: handler Lambda + runner local Express
│   │   └── src/
│   │       ├── functions/__entity__/{types,repository,service,handler}.ts
│   │       ├── shared/{errors,utils,db}/
│   │       └── local-server.ts            # API local que reutiliza el service
│   ├── frontend/       # React + Vite + TS, tema navy
│   │   └── src/
│   │       ├── components/icons.tsx
│   │       ├── pages/__Entity__Page.tsx   # CRUD con el estándar de diseño
│   │       ├── services/__entity__Service.ts
│   │       ├── store/__entity__Store.ts
│   │       └── styles.css                 # tokens + clases navy
│   └── infrastructure/ # CDK de referencia (NO se sintetiza en local)
└── tsconfig.base.json
```

## Marcadores a reemplazar

Al crear un módulo nuevo (ej. `Producto`):

| Marcador        | Reemplazo (ej.) | Dónde                                   |
|-----------------|-----------------|-----------------------------------------|
| `__Entity__`    | `Producto`      | tipos, clases, nombres de archivo       |
| `__entity__`    | `producto`      | carpetas, stores, servicios, rutas      |
| `__entity__s` / `__Entity__s` | `productos` / `Productos` | ruta API, tabla SQL, títulos |

Las columnas de ejemplo son `nombre` y `descripcion`. Ajustarlas a la entidad real
(tabla, SPs, tipos, repository, página).

## Cómo usarla

1. Copiar `templates/nube-fullstack` a la carpeta del proyecto (ej. `nubeaws/`).
2. Renombrar marcadores `__Entity__` / `__entity__` por la entidad real.
3. Ejecutar los `.sql` contra SQL Express con `sqlcmd` y verificar estructura/SPs.
4. `cp .env.example .env` en backend y frontend; ajustar valores.
5. `npm install` en la raíz (instala los workspaces).
6. Backend: `npm run dev:backend`. Frontend: `npm run dev:frontend`.
7. Probar el CRUD end-to-end y confirmar los datos en la BD.

## Verificación

- `npm run typecheck` en la raíz (backend + frontend, cero errores).
- `npm run build --workspace packages/frontend` (build exitoso).
- CRUD probado contra SQL Express (un exit 0 NO basta).
- NUNCA `cdk synth`/`deploy` en local: no hay credenciales AWS.
