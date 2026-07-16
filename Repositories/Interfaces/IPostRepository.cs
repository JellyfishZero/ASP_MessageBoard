using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories.Models;

namespace ASP_MessageBoard.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<IReadOnlyList<PostRecord>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<PostRecord?> GetByIdAsync(int postId, CancellationToken cancellationToken = default);

        Task<PostRecord> CreateAsync(Post post, CancellationToken cancellationToken = default);

        Task<PostRecord> UpdateAsync(Post post, CancellationToken cancellationToken = default);

        Task DeleteAsync(int postId, int userId, CancellationToken cancellationToken = default);
    }
}
