-- =============================================================
-- HU1 - Modulo Clientes
-- Base de datos: Kiro (SQL Server on-premise)
-- Tabla Clientes + Stored Procedures CRUD con soft delete,
-- paginacion y busqueda por Nombre o Telefono.
-- =============================================================

IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,  -- CA1, CA2
        Nombre            NVARCHAR(100)     NOT NULL,                                      -- CA3
        Direccion         NVARCHAR(200)     NULL,                                          -- CA4
        Telefono          NVARCHAR(20)      NULL,                                          -- CA5
        Activo            BIT               NOT NULL CONSTRAINT DF_Clientes_Activo DEFAULT (1),            -- CA8
        FechaCreacion     DATETIME2         NOT NULL CONSTRAINT DF_Clientes_FechaCreacion DEFAULT (GETDATE()), -- CA10
        FechaModificacion DATETIME2         NULL                                           -- CA10
    );
END
GO

-- CA5: el telefono solo debe aceptar digitos (defensa en BD ademas del backend)
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Clientes_Telefono_Digitos')
BEGIN
    ALTER TABLE dbo.Clientes WITH CHECK
        ADD CONSTRAINT CK_Clientes_Telefono_Digitos
        CHECK (Telefono IS NULL OR Telefono NOT LIKE '%[^0-9]%');
END
GO

-- Indice para acelerar listado y busqueda de clientes activos
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Clientes_Activo_Nombre')
BEGIN
    CREATE INDEX IX_Clientes_Activo_Nombre ON dbo.Clientes (Activo, Nombre);
END
GO

-- =============================================================
-- SP: Crear cliente (CA1, CA2, CA10)
-- =============================================================
CREATE OR ALTER PROCEDURE dbo.usp_Clientes_Crear
    @Nombre    NVARCHAR(100),
    @Direccion NVARCHAR(200) = NULL,
    @Telefono  NVARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Clientes (Nombre, Direccion, Telefono)
    VALUES (@Nombre, @Direccion, @Telefono);

    DECLARE @NuevoId INT = SCOPE_IDENTITY();

    SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
    FROM dbo.Clientes
    WHERE Id = @NuevoId;
END
GO

-- =============================================================
-- SP: Obtener cliente por Id (solo activos)
-- =============================================================
CREATE OR ALTER PROCEDURE dbo.usp_Clientes_ObtenerPorId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
    FROM dbo.Clientes
    WHERE Id = @Id AND Activo = 1;
END
GO

-- =============================================================
-- SP: Listar / Buscar clientes activos, paginado (CA6, CA9)
--   @Busqueda NULL -> lista todos los activos
--   @Busqueda con texto -> filtra por Nombre o Telefono
-- =============================================================
CREATE OR ALTER PROCEDURE dbo.usp_Clientes_Listar
    @Busqueda NVARCHAR(100) = NULL,
    @Pagina   INT = 1,
    @Tamano   INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina < 1 SET @Pagina = 1;
    IF @Tamano < 1 SET @Tamano = 10;
    IF @Tamano > 100 SET @Tamano = 100;

    DECLARE @Offset INT = (@Pagina - 1) * @Tamano;
    DECLARE @Filtro NVARCHAR(102) = CASE WHEN @Busqueda IS NULL THEN NULL ELSE '%' + @Busqueda + '%' END;

    -- Total de registros que cumplen el filtro (para la paginacion del cliente)
    SELECT COUNT(*) AS Total
    FROM dbo.Clientes
    WHERE Activo = 1
      AND (@Filtro IS NULL OR Nombre LIKE @Filtro OR Telefono LIKE @Filtro);

    -- Pagina de resultados
    SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
    FROM dbo.Clientes
    WHERE Activo = 1
      AND (@Filtro IS NULL OR Nombre LIKE @Filtro OR Telefono LIKE @Filtro)
    ORDER BY FechaCreacion DESC, Id DESC
    OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
END
GO

-- =============================================================
-- SP: Actualizar cliente (CA7, CA10)
-- =============================================================
CREATE OR ALTER PROCEDURE dbo.usp_Clientes_Actualizar
    @Id        INT,
    @Nombre    NVARCHAR(100),
    @Direccion NVARCHAR(200) = NULL,
    @Telefono  NVARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Clientes
    SET Nombre            = @Nombre,
        Direccion         = @Direccion,
        Telefono          = @Telefono,
        FechaModificacion = GETDATE()
    WHERE Id = @Id AND Activo = 1;

    SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
    FROM dbo.Clientes
    WHERE Id = @Id;
END
GO

-- =============================================================
-- SP: Eliminar cliente - SOFT DELETE (CA8, CA10)
-- =============================================================
CREATE OR ALTER PROCEDURE dbo.usp_Clientes_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Clientes
    SET Activo            = 0,
        FechaModificacion = GETDATE()
    WHERE Id = @Id AND Activo = 1;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO
