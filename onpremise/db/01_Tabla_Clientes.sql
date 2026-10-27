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
        Activo            BIT           NOT NULL DEFAULT 1,
        FechaCreacion     DATETIME2     NOT NULL DEFAULT GETDATE(),
        FechaModificacion DATETIME2     NULL
    );
END
GO
