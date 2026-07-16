SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetByPhoneNumber
    @PhoneNumber VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        UserName,
        PhoneNumber,
        Email,
        PasswordHash,
        CoverImagePath,
        Biography,
        CreatedAt,
        UpdatedAt
    FROM dbo.Users
    WHERE PhoneNumber = @PhoneNumber;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserId,
        UserName,
        PhoneNumber,
        Email,
        PasswordHash,
        CoverImagePath,
        Biography,
        CreatedAt,
        UpdatedAt
    FROM dbo.Users
    WHERE UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_Create
    @UserName NVARCHAR(50),
    @PhoneNumber VARCHAR(20),
    @PasswordHash NVARCHAR(512),
    @Email NVARCHAR(254) = NULL,
    @CoverImagePath NVARCHAR(500) = NULL,
    @Biography NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Users
        WHERE PhoneNumber = @PhoneNumber
    )
    BEGIN
        THROW 50001, N'此手機號碼已經註冊。', 1;
    END;

    BEGIN TRY
        INSERT INTO dbo.Users
        (
            UserName,
            PhoneNumber,
            Email,
            PasswordHash,
            CoverImagePath,
            Biography
        )
        VALUES
        (
            @UserName,
            @PhoneNumber,
            @Email,
            @PasswordHash,
            @CoverImagePath,
            @Biography
        );
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
            THROW 50001, N'此手機號碼已經註冊。', 1;

        THROW;
    END CATCH;

    DECLARE @UserId INT = CONVERT(INT, SCOPE_IDENTITY());

    SELECT
        UserId,
        UserName,
        PhoneNumber,
        Email,
        PasswordHash,
        CoverImagePath,
        Biography,
        CreatedAt,
        UpdatedAt
    FROM dbo.Users
    WHERE UserId = @UserId;
END;
GO
