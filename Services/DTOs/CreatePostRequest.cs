namespace ASP_MessageBoard.Services.DTOs
{
    public class CreatePostRequest
    {
        public int UserId { get; init; }

        public string Content { get; init; } = string.Empty;

        public IFormFile? Image { get; init; }
    }
}
