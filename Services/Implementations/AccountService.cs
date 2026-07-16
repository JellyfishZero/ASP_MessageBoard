using ASP_MessageBoard.Common.Exceptions;
using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories.Interfaces;
using ASP_MessageBoard.Services.DTOs;
using ASP_MessageBoard.Services.Interfaces;
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
            RegisterRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var phoneNumber = request.PhoneNumber.Trim();
            var email = request.Email.Trim();
            var userName = request.UserName.Trim();
            var biography = string.IsNullOrWhiteSpace(request.Biography)
                ? null
                : request.Biography.Trim();

            var existingUser = await _userRepository.GetByPhoneNumberAsync(
                phoneNumber,
                cancellationToken
            );

            if (existingUser is not null)
            {
                throw new DuplicatePhoneNumberException();
            }

            var user = new User
            {
                UserName = userName,
                PhoneNumber = phoneNumber,
                Email = email,
                Biography = biography,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            try
            {
                return await _userRepository.CreateAsync(user, cancellationToken);
            }
            catch (SqlException exception) when (exception.Number == 50001)
            {
                // 防止兩個請求同時通過前面的重複檢查。
                throw new DuplicatePhoneNumberException(exception);
            }
        }

        public async Task<User?> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var phoneNumber = request.PhoneNumber.Trim();

            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);

            if (user is null)
            {
                return null;
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

            return verificationResult switch
            {
                PasswordVerificationResult.Failed => null,
                PasswordVerificationResult.Success => user,
                PasswordVerificationResult.SuccessRehashNeeded => user,
                _ => null,
            };
        }
    }
}
