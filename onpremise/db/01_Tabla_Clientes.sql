-- =============================================
-- Módulo: Clientes (HU1)
-- Tabla dbo.Clientes con soft delete y auditoría.
-- =============================================
IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        Id                INT IDENTITY(1,1) PRIMARY KEY,
        Nombre            NVARCHAR(100) NOT NULL,
        Direccion         NVARCHAR(200) NULL,
        Telefono          NVARCHAR(20)  NULL,
        NIT               NVARCHAR(20)  NULL,
        Activo            BIT           NOT NULL DEFAULT 1,
        FechaCreacion     DATETIME2     NOT NULL DEFAULT GETDATE(),
        FechaModificacion DATETIME2     NULL
    );
END
GO

-- Migración: agregar columna NIT si la tabla ya existía sin ella.
IF COL_LENGTH('dbo.Clientes', 'NIT') IS NULL
BEGIN
    ALTER TABLE dbo.Clientes ADD NIT NVARCHAR(20) NULL;
END
GO
