using System.Globalization;
using System.Security.Claims;
using ASP_MessageBoard.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ASP_MessageBoard.Authentication
{
    public sealed class ApplicationCookieEvents : CookieAuthenticationEvents
    {
        private readonly IUserRepository _userRepository;

        public ApplicationCookieEvents(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override async Task ValidatePrincipal(
            CookieValidatePrincipalContext context
        )
        {
            var userIdValue = context.Principal?
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            var phoneNumber = context.Principal?
                .FindFirst(ClaimTypes.MobilePhone)
                ?.Value;

            if (
                !int.TryParse(
                    userIdValue,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var userId
                )
                || userId <= 0
                || string.IsNullOrWhiteSpace(phoneNumber)
            )
            {
                await RejectPrincipalAsync(context);
                return;
            }

            var user = await _userRepository.GetByIdAsync(
                userId,
                context.HttpContext.RequestAborted
            );

            if (
                user is null
                || !string.Equals(
                    user.PhoneNumber,
                    phoneNumber,
                    StringComparison.Ordinal
                )
            )
            {
                await RejectPrincipalAsync(context);
            }
        }

        private static async Task RejectPrincipalAsync(
            CookieValidatePrincipalContext context
        )
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
        }
    }
}

