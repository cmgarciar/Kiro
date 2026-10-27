-- =============================================================================
-- Plantilla de stored procedures para __Entity__. Reemplazar __Entity__.
-- Todos con SET NOCOUNT ON y TRY/CATCH. Los de escritura, con transacción.
-- Soft delete (nunca DELETE físico). Listados filtran Activo = 1.
-- Ejecutar con:
--   sqlcmd -S "DESKTOP-0T7DSHL\SQLEXPRESS" -U kiro -P Kiro123 -d Kiro -i 02_StoredProcedures___Entity__.sql -C
-- =============================================================================
GO
CREATE OR ALTER PROCEDURE dbo.usp___Entity__s_Listar
    @Pagina   INT = 1,
    @TamPagina INT = 10,
    @Busqueda NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @Offset INT = (@Pagina - 1) * @TamPagina;

        SELECT Id, Nombre, Descripcion, Activo, FechaCreacion, FechaModificacion
        FROM dbo.__Entity__s
        WHERE Activo = 1
          AND (@Busqueda IS NULL OR Nombre LIKE '%' + @Busqueda + '%')
        ORDER BY Nombre
        OFFSET @Offset ROWS FETCH NEXT @TamPagina ROWS ONLY;

        SELECT COUNT(*) AS Total
        FROM dbo.__Entity__s
        WHERE Activo = 1
          AND (@Busqueda IS NULL OR Nombre LIKE '%' + @Busqueda + '%');
    END TRY
    BEGIN CATCH
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @Sev INT = ERROR_SEVERITY();
        DECLARE @Sta INT = ERROR_STATE();
        RAISERROR(@Msg, @Sev, @Sta);
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp___Entity__s_ObtenerPorId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT Id, Nombre, Descripcion, Activo, FechaCreacion, FechaModificacion
        FROM dbo.__Entity__s
        WHERE Id = @Id AND Activo = 1;
    END TRY
    BEGIN CATCH
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @Sev INT = ERROR_SEVERITY();
        DECLARE @Sta INT = ERROR_STATE();
        RAISERROR(@Msg, @Sev, @Sta);
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp___Entity__s_Crear
    @Nombre      NVARCHAR(100),
    @Descripcion NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.__Entity__s (Nombre, Descripcion)
        VALUES (@Nombre, @Descripcion);

        DECLARE @NuevoId INT = SCOPE_IDENTITY();

        SELECT Id, Nombre, Descripcion, Activo, FechaCreacion, FechaModificacion
        FROM dbo.__Entity__s WHERE Id = @NuevoId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @Sev INT = ERROR_SEVERITY();
        DECLARE @Sta INT = ERROR_STATE();
        RAISERROR(@Msg, @Sev, @Sta);
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp___Entity__s_Actualizar
    @Id          INT,
    @Nombre      NVARCHAR(100),
    @Descripcion NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.__Entity__s
        SET Nombre = @Nombre,
            Descripcion = @Descripcion,
            FechaModificacion = GETDATE()
        WHERE Id = @Id AND Activo = 1;

        SELECT Id, Nombre, Descripcion, Activo, FechaCreacion, FechaModificacion
        FROM dbo.__Entity__s WHERE Id = @Id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @Sev INT = ERROR_SEVERITY();
        DECLARE @Sta INT = ERROR_STATE();
        RAISERROR(@Msg, @Sev, @Sta);
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp___Entity__s_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Soft delete: nunca DELETE físico
        UPDATE dbo.__Entity__s
        SET Activo = 0,
            FechaModificacion = GETDATE()
        WHERE Id = @Id AND Activo = 1;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @Sev INT = ERROR_SEVERITY();
        DECLARE @Sta INT = ERROR_STATE();
        RAISERROR(@Msg, @Sev, @Sta);
    END CATCH
END
GO

-- Verificación: los SP creados
SELECT name, type_desc FROM sys.objects
WHERE type = 'P' AND name LIKE 'usp___Entity__s_%'
ORDER BY name;
GO
