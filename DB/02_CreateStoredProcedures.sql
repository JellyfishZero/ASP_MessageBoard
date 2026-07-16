SET NOCOUNT ON;
GO

-- User

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
    @Email NVARCHAR(254),
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

-- Post

CREATE OR ALTER PROCEDURE dbo.usp_Post_Create
    @UserId INT,
    @Content NVARCHAR(2000),
    @ImagePath NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    INSERT INTO dbo.Posts (UserId, Content, ImagePath)
    VALUES (@UserId, @Content, @ImagePath);

    DECLARE @PostId INT = CONVERT(INT, SCOPE_IDENTITY());

    SELECT
        p.PostId,
        p.UserId,
        u.UserName,
        p.Content,
        p.ImagePath,
        p.CreatedAt,
        p.UpdatedAt
    FROM dbo.Posts AS p
    INNER JOIN dbo.Users AS u ON u.UserId = p.UserId
    WHERE p.PostId = @PostId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Post_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PostId,
        p.UserId,
        u.UserName,
        p.Content,
        p.ImagePath,
        p.CreatedAt,
        p.UpdatedAt
    FROM dbo.Posts AS p
    INNER JOIN dbo.Users AS u ON u.UserId = p.UserId
    ORDER BY p.CreatedAt DESC, p.PostId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Post_GetById
    @PostId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PostId,
        p.UserId,
        u.UserName,
        p.Content,
        p.ImagePath,
        p.CreatedAt,
        p.UpdatedAt
    FROM dbo.Posts AS p
    INNER JOIN dbo.Users AS u ON u.UserId = p.UserId
    WHERE p.PostId = @PostId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Post_Update
    @PostId INT,
    @UserId INT,
    @Content NVARCHAR(2000),
    @ImagePath NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.Posts
    SET
        Content = @Content,
        ImagePath = @ImagePath,
        UpdatedAt = SYSUTCDATETIME()
    WHERE PostId = @PostId
      AND UserId = @UserId;

    IF @@ROWCOUNT = 0
        THROW 50010, N'文章不存在或您沒有修改權限。', 1;

    SELECT
        p.PostId,
        p.UserId,
        u.UserName,
        p.Content,
        p.ImagePath,
        p.CreatedAt,
        p.UpdatedAt
    FROM dbo.Posts AS p
    INNER JOIN dbo.Users AS u ON u.UserId = p.UserId
    WHERE p.PostId = @PostId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Post_Delete
    @PostId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Posts WITH (UPDLOCK, HOLDLOCK)
            WHERE PostId = @PostId
              AND UserId = @UserId
        )
            THROW 50011, N'文章不存在或您沒有刪除權限。', 1;

        DELETE FROM dbo.Comments
        WHERE PostId = @PostId;

        DELETE FROM dbo.Posts
        WHERE PostId = @PostId
          AND UserId = @UserId;

        COMMIT TRANSACTION;

        SELECT CAST(1 AS BIT) AS IsDeleted;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO

-- Comment

CREATE OR ALTER PROCEDURE dbo.usp_Comment_Create
    @UserId INT,
    @PostId INT,
    @Content NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Posts
        WHERE PostId = @PostId
    )
        THROW 50020, N'找不到指定的文章。', 1;

    INSERT INTO dbo.Comments
    (
        UserId,
        PostId,
        Content
    )
    VALUES
    (
        @UserId,
        @PostId,
        @Content
    );

    DECLARE @CommentId INT = CONVERT(INT, SCOPE_IDENTITY());

    SELECT
        c.CommentId,
        c.UserId,
        u.UserName,
        c.PostId,
        c.Content,
        c.CreatedAt
    FROM dbo.Comments AS c
    INNER JOIN dbo.Users AS u
        ON u.UserId = c.UserId
    WHERE c.CommentId = @CommentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Comment_GetByPostId
    @PostId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.CommentId,
        c.UserId,
        u.UserName,
        c.PostId,
        c.Content,
        c.CreatedAt
    FROM dbo.Comments AS c
    INNER JOIN dbo.Users AS u
        ON u.UserId = c.UserId
    WHERE c.PostId = @PostId
    ORDER BY c.CreatedAt ASC, c.CommentId ASC;
END;
GO
