namespace ASP_MessageBoard.Repositories.Models
{
    /// <summary>
    /// 特別用來處理因為Stored Procedure預先join User資料表而產生的Post資料表的資料模型
    /// </summary>
    public class PostRecord
    {
        public int PostId { get; init; }

        public int UserId { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Content { get; init; } = string.Empty;

        public string? ImagePath { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? UpdatedAt { get; init; }

        public string? UserCoverImagePath { get; init; }

        public string? UserBiography { get; init; }
    }
}
