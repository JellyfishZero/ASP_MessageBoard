using ASP_MessageBoard.Services.DTOs;
using ASP_MessageBoard.ViewModels;

namespace ASP_MessageBoard.Services.Interfaces
{
    public interface IPostService
    {
        Task<IReadOnlyList<PostDetails>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<PostDetails?> GetByIdAsync(int postId, CancellationToken cancellationToken = default);

        Task<PostDetails> CreateAsync(
            CreatePostRequest request,
            CancellationToken cancellationToken = default
        );

        Task<PostDetails> UpdateAsync(
            UpdatePostRequest request,
            CancellationToken cancellationToken = default
        );

        Task DeleteAsync(int postId, int userId, CancellationToken cancellationToken = default);
    }
}
