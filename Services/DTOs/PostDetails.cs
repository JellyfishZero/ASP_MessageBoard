namespace ASP_MessageBoard.Services.DTOs
{
    public class PostDetails
    {
        public int PostId { get; init; }

        public int UserId { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Content { get; init; } = string.Empty;

        public string? ImagePath { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? UpdatedAt { get; init; }
    }
}
