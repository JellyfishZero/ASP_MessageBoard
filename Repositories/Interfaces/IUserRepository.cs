using ASP_MessageBoard.Models.Entities;

namespace ASP_MessageBoard.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByPhoneNumberAsync(
            string phoneNumber,
            CancellationToken cancellationToken = default
        );

        Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

        Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    }
}
