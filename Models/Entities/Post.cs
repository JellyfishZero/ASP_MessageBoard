namespace ASP_MessageBoard.Models.Entities
{
    public class Post
    {
        public int PostId { get; set; }

        public int UserId { get; set; }

        public string Content { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
