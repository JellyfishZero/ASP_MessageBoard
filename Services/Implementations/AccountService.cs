using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories.Interfaces;
using ASP_MessageBoard.Services.Interfaces;
using ASP_MessageBoard.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace ASP_MessageBoard.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> RegisterAsync(
            RegisterViewModel model,
            CancellationToken cancellationToken = default
        )
        {
            var phoneNumber = model.PhoneNumber.Trim();
            var email = model.Email.Trim();
            var userName = model.UserName.Trim();

            var existingUser = await _userRepository.GetByPhoneNumberAsync(
                phoneNumber,
                cancellationToken
            );

            if (existingUser is not null)
            {
                throw new InvalidOperationException("此手機號碼已經註冊。");
            }

            var user = new User
            {
                UserName = userName,
                PhoneNumber = phoneNumber,
                Email = email,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            try
            {
                return await _userRepository.CreateAsync(user, cancellationToken);
            }
            catch (SqlException exception) when (exception.Number == 50001)
            {
                // 防止兩個請求同時通過前面的重複檢查。
                throw new InvalidOperationException("此手機號碼已經註冊。", exception);
            }
        }
    }
}
