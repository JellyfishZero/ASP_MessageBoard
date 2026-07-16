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
            Email NVARCHAR(254) NOT NULL,
            PasswordHash NVARCHAR(512) NOT NULL,
            CoverImagePath NVARCHAR(500) NULL,
            Biography NVARCHAR(500) NULL,
            CreatedAt DATETIME2(0) NOT NULL
                CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),

            CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
            CONSTRAINT CK_Users_UserName_NotBlank
                CHECK (LEN(LTRIM(RTRIM(UserName))) > 0),
            CONSTRAINT CK_Users_PhoneNumber_NotBlank
                CHECK (LEN(LTRIM(RTRIM(PhoneNumber))) > 0),
            CONSTRAINT CK_Users_Email_NotBlank
                CHECK (LEN(LTRIM(RTRIM(Email))) > 0),
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

    IF OBJECT_ID(N'dbo.Posts', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Posts
        (
            PostId INT IDENTITY(1, 1) NOT NULL,
            UserId INT NOT NULL,
            Content NVARCHAR(2000) NOT NULL,
            ImagePath NVARCHAR(500) NULL,
            CreatedAt DATETIME2(0) NOT NULL
                CONSTRAINT DF_Posts_CreatedAt DEFAULT SYSUTCDATETIME(),
            UpdatedAt DATETIME2(0) NULL,

            CONSTRAINT PK_Posts PRIMARY KEY CLUSTERED (PostId),
            CONSTRAINT FK_Posts_Users
                FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
            CONSTRAINT CK_Posts_Content_NotBlank
                CHECK (LEN(LTRIM(RTRIM(Content))) > 0)
        );
    END;

    IF OBJECT_ID(N'dbo.Posts', N'U') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.indexes
           WHERE object_id = OBJECT_ID(N'dbo.Posts')
             AND name = N'IX_Posts_UserId'
       )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Posts_UserId
            ON dbo.Posts (UserId);
    END;

    IF OBJECT_ID(N'dbo.Posts', N'U') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.indexes
           WHERE object_id = OBJECT_ID(N'dbo.Posts')
             AND name = N'IX_Posts_CreatedAt'
       )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Posts_CreatedAt
            ON dbo.Posts (CreatedAt DESC);
    END;

    IF OBJECT_ID(N'dbo.Comments', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Comments
        (
            CommentId INT IDENTITY(1, 1) NOT NULL,
            UserId INT NOT NULL,
            PostId INT NOT NULL,
            Content NVARCHAR(1000) NOT NULL,
            CreatedAt DATETIME2(0) NOT NULL
                CONSTRAINT DF_Comments_CreatedAt DEFAULT SYSUTCDATETIME(),

            CONSTRAINT PK_Comments PRIMARY KEY CLUSTERED (CommentId),
            CONSTRAINT FK_Comments_Users
                FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
            CONSTRAINT FK_Comments_Posts
                FOREIGN KEY (PostId) REFERENCES dbo.Posts (PostId),
            CONSTRAINT CK_Comments_Content_NotBlank
                CHECK (LEN(LTRIM(RTRIM(Content))) > 0)
        );
    END;

    IF OBJECT_ID(N'dbo.Comments', N'U') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.indexes
           WHERE object_id = OBJECT_ID(N'dbo.Comments')
             AND name = N'IX_Comments_PostId'
       )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Comments_PostId
            ON dbo.Comments (PostId);
    END;

    IF OBJECT_ID(N'dbo.Comments', N'U') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.indexes
           WHERE object_id = OBJECT_ID(N'dbo.Comments')
             AND name = N'IX_Comments_UserId'
       )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Comments_UserId
            ON dbo.Comments (UserId);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
