using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories.Models;

namespace ASP_MessageBoard.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<IReadOnlyList<CommentRecord>> GetByPostIdAsync(
            int postId,
            CancellationToken cancellationToken = default
        );

        Task<CommentRecord> CreateAsync(
            Comment comment,
            CancellationToken cancellationToken = default
        );
    }
}
