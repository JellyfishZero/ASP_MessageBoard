namespace ASP_MessageBoard.Repositories.Models
{
    public class CommentRecord
    {
        public int CommentId { get; init; }

        public int UserId { get; init; }

        public string UserName { get; init; } = string.Empty;

        public int PostId { get; init; }

        public string Content { get; init; } = string.Empty;

        public DateTime CreatedAt { get; init; }
    }
}
