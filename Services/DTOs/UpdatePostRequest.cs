namespace ASP_MessageBoard.Services.DTOs
{
    public class UpdatePostRequest
    {
        public int PostId { get; init; }

        public int UserId { get; init; }

        public string Content { get; init; } = string.Empty;

        public IFormFile? NewImage { get; init; }

        public bool RemoveImage { get; init; }
    }
}
