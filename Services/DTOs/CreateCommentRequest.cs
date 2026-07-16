namespace ASP_MessageBoard.Services.DTOs
{
    public class CreateCommentRequest
    {
        public int UserId { get; init; }

        public int PostId { get; init; }

        public string Content { get; init; } = string.Empty;
    }
}
