SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Users
        (
            UserId INT IDENTITY(1, 1) NOT NULL,
            UserName NVARCHAR(50) NOT NULL,
            PhoneNumber VARCHAR(20) NOT NULL,
            Email NVARCHAR(254) NULL,
            PasswordHash NVARCHAR(512) NOT NULL,
            CoverImagePath NVARCHAR(500) NULL,
            Biography NVARCHAR(500) NULL,
            CreatedAt DATETIME2(0) NOT NULL
                CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
            UpdatedAt DATETIME2(0) NULL,

            CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
            CONSTRAINT CK_Users_UserName_NotBlank
                CHECK (LEN(LTRIM(RTRIM(UserName))) > 0),
            CONSTRAINT CK_Users_PhoneNumber_NotBlank
                CHECK (LEN(LTRIM(RTRIM(PhoneNumber))) > 0),
            CONSTRAINT CK_Users_PasswordHash_NotBlank
                CHECK (LEN(LTRIM(RTRIM(PasswordHash))) > 0)
        );
    END;

    IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.indexes
           WHERE object_id = OBJECT_ID(N'dbo.Users')
             AND name = N'UX_Users_PhoneNumber'
       )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UX_Users_PhoneNumber
            ON dbo.Users (PhoneNumber);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
