-- =============================================
-- Módulo: Clientes (HU1)
-- Stored Procedures CRUD + búsqueda + listado paginado (soft delete)
-- =============================================

CREATE OR ALTER PROCEDURE sp_ClienteCreate
    @Nombre    NVARCHAR(100),
    @Direccion NVARCHAR(200) = NULL,
    @Telefono  NVARCHAR(20)  = NULL,
    @NewId     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Clientes (Nombre, Direccion, Telefono, Activo, FechaCreacion)
        VALUES (@Nombre, @Direccion, @Telefono, 1, GETDATE());

        SET @NewId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrSev INT = ERROR_SEVERITY();
        DECLARE @ErrSta INT = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrSta);
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ClienteGetAll
    @PageNumber INT = 1,
    @PageSize   INT = 10,
    @Search     NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

        SELECT COUNT(1) AS TotalCount
        FROM dbo.Clientes
        WHERE Activo = 1
          AND (@Search IS NULL
               OR Nombre LIKE '%' + @Search + '%'
               OR Telefono LIKE '%' + @Search + '%');

        SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
        FROM dbo.Clientes
        WHERE Activo = 1
          AND (@Search IS NULL
               OR Nombre LIKE '%' + @Search + '%'
               OR Telefono LIKE '%' + @Search + '%')
        ORDER BY Id DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrSev INT = ERROR_SEVERITY();
        DECLARE @ErrSta INT = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrSta);
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ClienteGetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT Id, Nombre, Direccion, Telefono, Activo, FechaCreacion, FechaModificacion
        FROM dbo.Clientes
        WHERE Id = @Id AND Activo = 1;
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrSev INT = ERROR_SEVERITY();
        DECLARE @ErrSta INT = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrSta);
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ClienteUpdate
    @Id        INT,
    @Nombre    NVARCHAR(100),
    @Direccion NVARCHAR(200) = NULL,
    @Telefono  NVARCHAR(20)  = NULL,
    @Rows      INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Clientes
        SET Nombre = @Nombre,
            Direccion = @Direccion,
            Telefono = @Telefono,
            FechaModificacion = GETDATE()
        WHERE Id = @Id AND Activo = 1;

        SET @Rows = @@ROWCOUNT;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrSev INT = ERROR_SEVERITY();
        DECLARE @ErrSta INT = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrSta);
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ClienteDelete
    @Id   INT,
    @Rows INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Clientes
        SET Activo = 0,
            FechaModificacion = GETDATE()
        WHERE Id = @Id AND Activo = 1;

        SET @Rows = @@ROWCOUNT;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrSev INT = ERROR_SEVERITY();
        DECLARE @ErrSta INT = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSev, @ErrSta);
    END CATCH
END
GO
