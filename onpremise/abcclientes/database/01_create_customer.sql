/* ============================================================
   PASO 1 — Database objects for Customer module
   ============================================================ */

-- 1a. Create table
IF OBJECT_ID('dbo.Customer','U') IS NOT NULL DROP TABLE dbo.Customer;
GO
CREATE TABLE dbo.Customer (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Direccion NVARCHAR(200) NULL,
    Telefono NVARCHAR(20) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL
);
GO

-- 1b. sp_CustomerCreate
CREATE OR ALTER PROCEDURE dbo.sp_CustomerCreate
    @Nombre NVARCHAR(100),
    @Direccion NVARCHAR(200) = NULL,
    @Telefono NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Customer (Nombre, Direccion, Telefono)
    VALUES (@Nombre, @Direccion, @Telefono);
    SELECT SCOPE_IDENTITY() AS Id;
END
GO

-- 1c. sp_CustomerGetAll (with pagination and search)
CREATE OR ALTER PROCEDURE dbo.sp_CustomerGetAll
    @Search NVARCHAR(100) = NULL,
    @Page INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
    FROM dbo.Customer
    WHERE Activo = 1
      AND (@Search IS NULL OR Nombre LIKE '%' + @Search + '%' OR Telefono LIKE '%' + @Search + '%')
    ORDER BY Id
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) AS TotalCount
    FROM dbo.Customer
    WHERE Activo = 1
      AND (@Search IS NULL OR Nombre LIKE '%' + @Search + '%' OR Telefono LIKE '%' + @Search + '%');
END
GO

-- 1d. sp_CustomerGetById
CREATE OR ALTER PROCEDURE dbo.sp_CustomerGetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
    FROM dbo.Customer
    WHERE Id = @Id AND Activo = 1;
END
GO

-- 1e. sp_CustomerUpdate
CREATE OR ALTER PROCEDURE dbo.sp_CustomerUpdate
    @Id INT,
    @Nombre NVARCHAR(100),
    @Direccion NVARCHAR(200) = NULL,
    @Telefono NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Customer
    SET Nombre = @Nombre,
        Direccion = @Direccion,
        Telefono = @Telefono,
        FechaModificacion = GETDATE()
    WHERE Id = @Id AND Activo = 1;
    SELECT @@ROWCOUNT AS Updated;
END
GO

-- 1f. sp_CustomerDelete (soft delete)
CREATE OR ALTER PROCEDURE dbo.sp_CustomerDelete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Customer
    SET Activo = 0,
        FechaModificacion = GETDATE()
    WHERE Id = @Id AND Activo = 1;
    SELECT @@ROWCOUNT AS Deleted;
END
GO
