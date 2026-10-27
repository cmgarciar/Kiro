-- =============================================================================
-- Plantilla de tabla. Reemplazar __Entity__ por el nombre de la entidad
-- (ej: Cliente -> tabla Clientes). Columnas en español con PK IDENTITY,
-- auditoría y soft delete, según el estándar del workspace.
-- Ejecutar con:
--   sqlcmd -S "DESKTOP-0T7DSHL\SQLEXPRESS" -U kiro -P Kiro123 -d Kiro -i 01_Tabla___Entity__.sql -C
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = '__Entity__s')
BEGIN
    CREATE TABLE dbo.__Entity__s
    (
        Id                INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Nombre            NVARCHAR(100)  NOT NULL,
        Descripcion       NVARCHAR(200)  NULL,
        Activo            BIT            NOT NULL DEFAULT 1,
        FechaCreacion     DATETIME2      NOT NULL DEFAULT GETDATE(),
        FechaModificacion DATETIME2      NULL
    );

    -- Índice pensado para listados/búsquedas de activos
    CREATE INDEX IX___Entity__s_Activo_Nombre ON dbo.__Entity__s (Activo, Nombre);
END
GO

-- Verificación de estructura
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = '__Entity__s'
ORDER BY ORDINAL_POSITION;
GO
