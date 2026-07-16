using ASP_MessageBoard.Services.DTOs;

namespace ASP_MessageBoard.Services.Interfaces
{
    public interface ICommentService
    {
        Task<IReadOnlyList<CommentDetails>> GetByPostIdAsync(
            int postId,
            CancellationToken cancellationToken = default
        );

        Task<CommentDetails> CreateAsync(
            CreateCommentRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
