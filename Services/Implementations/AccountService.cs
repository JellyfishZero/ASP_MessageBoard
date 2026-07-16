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
        private readonly IImageStorageService _imageStorageService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            IImageStorageService imageStorageService,
            ILogger<AccountService> logger
        )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _imageStorageService = imageStorageService;
            _logger = logger;
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

            string? coverImagePath = null;

            if (request.CoverImage is not null)
            {
                coverImagePath = await _imageStorageService.SaveCoverImageAsync(
                    request.CoverImage,
                    cancellationToken
                );
            }

            var user = new User
            {
                UserName = userName,
                PhoneNumber = phoneNumber,
                Email = email,
                Biography = biography,
                CoverImagePath = coverImagePath,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            try
            {
                return await _userRepository.CreateAsync(user, cancellationToken);
            }
            catch (SqlException exception) when (exception.Number == 50001)
            {
                if (coverImagePath is not null)
                {
                    await TryDeleteCoverImageAsync(coverImagePath);
                }

                throw new DuplicatePhoneNumberException(exception);
            }
            catch
            {
                if (coverImagePath is not null)
                {
                    await TryDeleteCoverImageAsync(coverImagePath);
                }

                throw;
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

        private async Task TryDeleteCoverImageAsync(string coverImagePath)
        {
            try
            {
                await _imageStorageService.DeleteAsync(coverImagePath);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "無法刪除註冊失敗後留下的 Cover Image：{CoverImagePath}",
                    coverImagePath
                );
            }
        }
    }
}
