-- =============================================================
-- Modulo: Clientes
-- Base de datos: Kiro
-- Descripcion: Tabla Clientes + Stored Procedures CRUD y busqueda paginada
-- =============================================================

IF OBJECT_ID('dbo.Clientes','U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
        Nombre            NVARCHAR(100)     NOT NULL,
        Direccion         NVARCHAR(200)     NULL,
        Telefono          NVARCHAR(20)      NULL,
        Activo            BIT               NOT NULL CONSTRAINT DF_Clientes_Activo DEFAULT(1),
        FechaCreacion     DATETIME2         NOT NULL CONSTRAINT DF_Clientes_FechaCreacion DEFAULT(GETDATE()),
        FechaModificacion DATETIME2         NULL
    );
END
GO

-- =============================================================
-- sp_ClienteCrear : inserta un nuevo cliente y devuelve el registro creado
-- =============================================================
CREATE OR ALTER PROCEDURE dbo.sp_ClienteCrear
    @Nombre    NVARCHAR(100),
    @Direccion NVARCHAR(200) = NULL,
    @Telefono  NVARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Clientes (Nombre, Direccion, Telefono, Activo, FechaCreacion)
    VALUES (@Nombre, @Direccion, @Telefono, 1, GETDATE());

    DECLARE @NewId INT = SCOPE_IDENTITY();

    SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
    FROM dbo.Clientes
    WHERE Id = @NewId;
END
GO

-- =============================================================
-- sp_ClienteObtenerPorId : devuelve un cliente activo por su Id
-- =============================================================
CREATE OR ALTER PROCEDURE dbo.sp_ClienteObtenerPorId
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
-- sp_ClienteListar : lista/busca clientes activos paginados por Nombre o Telefono
-- @Filtro : texto a buscar en Nombre o Telefono (NULL = todos)
-- Devuelve dos result sets: (1) pagina de datos, (2) total de registros
-- =============================================================
CREATE OR ALTER PROCEDURE dbo.sp_ClienteListar
    @Filtro   NVARCHAR(100) = NULL,
    @Pagina   INT = 1,
    @TamPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina < 1 SET @Pagina = 1;
    IF @TamPagina < 1 SET @TamPagina = 10;

    DECLARE @Offset INT = (@Pagina - 1) * @TamPagina;

    ;WITH Filtrados AS
    (
        SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
        FROM dbo.Clientes
        WHERE Activo = 1
          AND ( @Filtro IS NULL
                OR Nombre   LIKE '%' + @Filtro + '%'
                OR Telefono LIKE '%' + @Filtro + '%' )
    )
    SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
    FROM Filtrados
    ORDER BY Id DESC
    OFFSET @Offset ROWS FETCH NEXT @TamPagina ROWS ONLY;

    SELECT COUNT(*) AS Total
    FROM dbo.Clientes
    WHERE Activo = 1
      AND ( @Filtro IS NULL
            OR Nombre   LIKE '%' + @Filtro + '%'
            OR Telefono LIKE '%' + @Filtro + '%' );
END
GO

-- =============================================================
-- sp_ClienteActualizar : actualiza Nombre, Direccion y Telefono de un cliente activo
-- =============================================================
CREATE OR ALTER PROCEDURE dbo.sp_ClienteActualizar
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
-- sp_ClienteEliminar : soft delete (Activo = 0)
-- =============================================================
CREATE OR ALTER PROCEDURE dbo.sp_ClienteEliminar
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
