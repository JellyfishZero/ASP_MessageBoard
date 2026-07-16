using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.ViewModels;

namespace ASP_MessageBoard.Services.Interfaces
{
    public interface IAccountService
    {
        Task<User> RegisterAsync(
            RegisterViewModel model,
            CancellationToken cancellationToken = default
        );
    }
}
