using ASP_MessageBoard.Services.DTOs;

namespace ASP_MessageBoard.ViewModels
{
    public class PostDetailsViewModel
    {
        public PostDetails Post { get; init; } = null!;

        public IReadOnlyList<CommentDetails> Comments { get; init; } = [];

        public CreateCommentViewModel NewComment { get; init; } = new();
    }
}
