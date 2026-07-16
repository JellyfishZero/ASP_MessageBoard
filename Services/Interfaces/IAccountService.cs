using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Services.Dtos;

namespace ASP_MessageBoard.Services.Interfaces
{
    public interface IAccountService
    {
        Task<User> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
